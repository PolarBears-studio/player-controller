@tool
extends Path3D

const RISER_SURFACE = 0
const THREAD_SURFACE = 1
const STRINGER_SURFACE = 2

const EPSILON = 0.001

@export_group("Stairs")
## Controls whether the stair generates a base to the path or not
@export
var generate_bottom := true:
	set(v):
		if generate_bottom == v:
			return
		generate_bottom = v
		queue_update()
@export_range(EPSILON, 100.0, EPSILON, "or_greater", "suffix:m")
var height := 1.0:
	set(v):
		if height == v:
			return
		height = maxf(EPSILON, v)
		queue_update()
@export_range(EPSILON, 100.0, EPSILON, "or_greater", "suffix:m")
var width := 1.0:
	set(v):
		if width == v:
			return
		width = maxf(EPSILON, v)
		queue_update()
@export_range(EPSILON, 100.0, EPSILON, "or_greater", "suffix:m")
var step_height := 0.25:
	set(v):
		if step_height == v:
			return
		step_height = maxf(EPSILON, v)
		queue_update()

@export_group("Materials")
## Sides and bottom of a stair
@export var stringer_material: Material:
	set(v):
		if stringer_material == v:
			return
		stringer_material = v
		if _mesh_instance and _mesh_instance.get_surface_override_material_count() >= STRINGER_SURFACE:
			_mesh_instance.set_surface_override_material(STRINGER_SURFACE, stringer_material)
		# Update other mats if only stringer is provided
		self.riser_material = riser_material
		self.thread_material = thread_material
## Vertical sections of the steps
@export var riser_material: Material:
	set(v):
		if v and riser_material == v:
			return
		riser_material = v
		if _mesh_instance and _mesh_instance.get_surface_override_material_count() >= RISER_SURFACE:
			_mesh_instance.set_surface_override_material(
				RISER_SURFACE,
				stringer_material if not riser_material else riser_material
			)
## Top of the steps
@export var thread_material: Material:
	set(v):
		if v and thread_material == v:
			return
		thread_material = v
		if _mesh_instance and _mesh_instance.get_surface_override_material_count() >= THREAD_SURFACE:
			_mesh_instance.set_surface_override_material(
				THREAD_SURFACE,
				stringer_material if not thread_material else thread_material
			)

@export_group("Collisions")
@export
var make_simple_collision := false:
	set(v):
		if make_simple_collision == v:
			return
		make_simple_collision = v
		queue_update()
@export_flags_3d_physics
var collision_layer := 1:
	set(v):
		if collision_layer == v:
			return
		collision_layer = v
		if _static_body:
			_static_body.collision_layer = collision_layer
@export_flags_3d_physics
var collision_mask := 1:
	set(v):
		if collision_mask == v:
			return
		collision_mask = v
		if _static_body:
			_static_body.collision_mask = collision_mask

var _path_follow: PathFollow3D
var _mesh_instance: MeshInstance3D
var _static_body: StaticBody3D
var _concave_shape: ConcavePolygonShape3D

var _pending_update := false

func _init() -> void:
	curve_changed.connect(queue_update)
	_init_children()

func _ready() -> void:
	if curve.point_count < 2:
		curve.clear_points()
		curve.add_point(Vector3.ZERO)
		curve.add_point(Vector3.FORWARD)
	queue_update()

func _init_children() -> void:
	if not _path_follow:
		_path_follow = PathFollow3D.new()
		_path_follow.transform = Transform3D.IDENTITY
		_path_follow.loop = false
		_path_follow.progress_ratio = 0.0
		_path_follow.rotation_mode = PathFollow3D.ROTATION_ORIENTED
		_path_follow.use_model_front = true
		add_child(_path_follow, false, Node.INTERNAL_MODE_BACK)

	if not _mesh_instance:
		_mesh_instance = MeshInstance3D.new()
		_mesh_instance.owner = owner
		add_child(_mesh_instance)
		#add_child(_mesh_instance, false, Node.INTERNAL_MODE_BACK)

	if not _static_body:
		_static_body = StaticBody3D.new()
		_static_body.collision_layer = collision_layer
		_static_body.collision_mask = collision_mask
		add_child(_static_body, false, Node.INTERNAL_MODE_BACK)

	if not _concave_shape:
		_concave_shape = ConcavePolygonShape3D.new()
		var collision_shape := CollisionShape3D.new()
		collision_shape.shape = _concave_shape
		_static_body.add_child(collision_shape)

func _get_vertex_position(progress_ratio: float, h_offset: float, v_offset: float) -> Vector3:
	if not _path_follow:
		return Vector3.ZERO
	_path_follow.progress_ratio = progress_ratio
	_path_follow.h_offset = h_offset
	_path_follow.v_offset = v_offset
	return _path_follow.transform.origin

func _update() -> void:
	var vertices := _create_stair_vertices(false)
	if _mesh_instance:
		_mesh_instance.mesh = _create_stair_mesh(vertices)
		_mesh_instance.set_surface_override_material(STRINGER_SURFACE, stringer_material)
		_mesh_instance.set_surface_override_material(
				RISER_SURFACE,
				stringer_material if not riser_material else riser_material
			)
		_mesh_instance.set_surface_override_material(
				THREAD_SURFACE,
				stringer_material if not thread_material else thread_material
			)
	if _concave_shape:
		if make_simple_collision:
			_concave_shape.set_faces(_create_stair_vertices(true))
		else:
			_concave_shape.set_faces(vertices)
	_pending_update = false

## Expects the non-simple output of _create_stair_vertices. Returns an ArrayMesh with surfaces for risers, threads, and stringers.
func _create_stair_mesh(vertices: PackedVector3Array) -> ArrayMesh:
	var num_steps := floori(height / step_height)
	if vertices.is_empty() or num_steps <= 0:
		return ArrayMesh.new()

	var riser_vertices := PackedVector3Array()
	var thread_vertices := PackedVector3Array()
	var stringer_vertices := PackedVector3Array()

	const VERTS_PER_TRI := 3
	# First step
	riser_vertices.append_array(vertices.slice(0, 2 * VERTS_PER_TRI))
	thread_vertices.append_array(vertices.slice(2 * VERTS_PER_TRI, 4 * VERTS_PER_TRI))
	stringer_vertices.append_array(vertices.slice(4 * VERTS_PER_TRI, 10 * VERTS_PER_TRI))

	const TRIS_PER_STEP := 12
	const FIRST_STEP_OFFSET := 10 * VERTS_PER_TRI
	for i in range(1, num_steps):
		var offset := FIRST_STEP_OFFSET + (i-1) * TRIS_PER_STEP * VERTS_PER_TRI
		riser_vertices.append_array(vertices.slice(offset, offset + 2 * VERTS_PER_TRI))
		thread_vertices.append_array(vertices.slice(offset + 2 * VERTS_PER_TRI, offset + 4 * VERTS_PER_TRI))
		stringer_vertices.append_array(vertices.slice(offset + 4 * VERTS_PER_TRI, offset + 12 * VERTS_PER_TRI))

	# End of stairs
	stringer_vertices.append_array(vertices.slice(-2 * VERTS_PER_TRI))

	var mesh := ArrayMesh.new()
	var arr := []
	arr.resize(Mesh.ARRAY_MAX)
	arr[Mesh.ARRAY_VERTEX] = riser_vertices
	arr[Mesh.ARRAY_NORMAL] = _create_normal_from_triangles(riser_vertices)
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arr)
	arr[Mesh.ARRAY_VERTEX] = thread_vertices
	arr[Mesh.ARRAY_NORMAL] = _create_normal_from_triangles(thread_vertices)
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arr)
	arr[Mesh.ARRAY_VERTEX] = stringer_vertices
	arr[Mesh.ARRAY_NORMAL] = _create_normal_from_triangles(stringer_vertices)
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arr)
	return mesh

## Returns all the triangles in the stair. (A triangle is three Vector3s in clockwise order looking towards the face).
## [codeblock]
## [
##    # Numbers represent the amount of triangles.
##    First Step (Thread[2], Stringer[4]) if simple else (Riser[2], Thread[2], Stringer[6]),
##    Inbetween Steps (Thread[2], Stringer[6]) if simple else (Riser[2], Thread[2], Stringer[8]),
##    Back face ([2])
## ]
## [/codeblock]
func _create_stair_vertices(simple := false) -> PackedVector3Array:
	var arr := PackedVector3Array()

	var num_steps := floori(height / step_height)
	var progress_per_step := 1.0 / float(num_steps)
	var half_width := width / 2.0

	if num_steps > 0:
		# Generate first step
		var curr_progress := 0.0
		var curr_height := 0.0
		var step_vertices := PackedVector3Array()
		step_vertices.push_back(_get_vertex_position(curr_progress, half_width, curr_height))
		step_vertices.push_back(_get_vertex_position(curr_progress, half_width, step_height))
		step_vertices.push_back(_get_vertex_position(curr_progress,-half_width, step_height))
		step_vertices.push_back(_get_vertex_position(curr_progress,-half_width, curr_height))

		step_vertices.push_back(_get_vertex_position(progress_per_step, half_width, curr_height))
		step_vertices.push_back(_get_vertex_position(progress_per_step, half_width, step_height))
		step_vertices.push_back(_get_vertex_position(progress_per_step, half_width, step_height + step_height))
		step_vertices.push_back(_get_vertex_position(progress_per_step,-half_width, step_height + step_height))
		step_vertices.push_back(_get_vertex_position(progress_per_step,-half_width, step_height))
		step_vertices.push_back(_get_vertex_position(progress_per_step,-half_width, curr_height))

		const STEP_INDICES: PackedInt32Array = [
			0, 1, 3, # Riser
			1, 2, 3,
			1, 5, 2, # Thread
			2, 5, 8,
			0, 4, 5, # Stringer
			0, 5, 1,
			2, 8, 3,
			3, 8, 9,
			3, 9, 0,
			0, 9, 4,
		]
		const SIMPLIFIED_STEP_INDICES: PackedInt32Array = [
			0, 5, 3, # Thread
			3, 5, 8,
			0, 4, 5, # Stringer
			3, 8, 9,
			3, 9, 0,
			0, 9, 4,
		]
		for j in (SIMPLIFIED_STEP_INDICES if simple else STEP_INDICES):
			arr.push_back(step_vertices[j])

	# Generate steps until end of last step
	for i in range(1, num_steps):
		var curr_progress := i * progress_per_step
		var curr_height := i * step_height

		var bottom_height := 0.0 if generate_bottom else curr_height - step_height
		var step_vertices := PackedVector3Array()
		step_vertices.push_back(_get_vertex_position(curr_progress, half_width, bottom_height))
		step_vertices.push_back(_get_vertex_position(curr_progress, half_width, curr_height))
		step_vertices.push_back(_get_vertex_position(curr_progress, half_width, curr_height + step_height))
		step_vertices.push_back(_get_vertex_position(curr_progress,-half_width, curr_height + step_height))
		step_vertices.push_back(_get_vertex_position(curr_progress,-half_width, curr_height))
		step_vertices.push_back(_get_vertex_position(curr_progress,-half_width, bottom_height))

		var next_progress := curr_progress + progress_per_step
		var next_height := curr_height + step_height
		bottom_height = 0.0 if generate_bottom else curr_height
		step_vertices.push_back(_get_vertex_position(next_progress, half_width, bottom_height))
		step_vertices.push_back(_get_vertex_position(next_progress, half_width, next_height))
		step_vertices.push_back(_get_vertex_position(next_progress, half_width, next_height + step_height))
		step_vertices.push_back(_get_vertex_position(next_progress,-half_width, next_height + step_height))
		step_vertices.push_back(_get_vertex_position(next_progress,-half_width, next_height))
		step_vertices.push_back(_get_vertex_position(next_progress,-half_width, bottom_height))

		const STEP_INDICES: PackedInt32Array = [
			1, 2, 4, # Riser
			2, 3, 4,
			2, 7, 3, # Thread
			3, 7,10,
			0, 6, 1, # Stringer
			1, 6, 7,
			1, 7, 2,
			3,10, 4,
			4,10,11,
			4,11, 5,
			5,11, 0,
			0,11, 6,
		]
		const SIMPLIFIED_STEP_INDICES: PackedInt32Array = [
			1, 7, 4, # Thread
			4, 7,10,
			0, 6, 1, # Stringer
			1, 6, 7,
			4,10,11,
			4,11, 5,
			5,11, 0,
			0,11, 6,
		]
		for j in (SIMPLIFIED_STEP_INDICES if simple else STEP_INDICES):
			arr.push_back(step_vertices[j])

	if num_steps > 0:
		var end_step := num_steps
		var end_progress := end_step * progress_per_step
		var end_height := end_step * step_height
		var end_vertices := PackedVector3Array()
		var bottom_height := 0.0 if generate_bottom else end_height - step_height
		end_vertices.push_back(_get_vertex_position(end_progress, half_width, bottom_height))
		end_vertices.push_back(_get_vertex_position(end_progress, half_width, end_height))
		end_vertices.push_back(_get_vertex_position(end_progress,-half_width, end_height))
		end_vertices.push_back(_get_vertex_position(end_progress,-half_width, bottom_height))
		arr.push_back(end_vertices[0])
		arr.push_back(end_vertices[2])
		arr.push_back(end_vertices[1])
		arr.push_back(end_vertices[0])
		arr.push_back(end_vertices[3])
		arr.push_back(end_vertices[2])
	return arr

# Function only needed because Godot's MeshInstance has terrible shadows with generated normals.
func _create_normal_from_triangles(vertices: PackedVector3Array) -> PackedVector3Array:
	if vertices.size() < 3:
		return []
	var normals := PackedVector3Array()
	for i in range(0, vertices.size(), 3):
		var plane := Plane(vertices[i], vertices[i+1], vertices[i+2])
		normals.push_back(plane.normal)
		normals.push_back(plane.normal)
		normals.push_back(plane.normal)
	return normals

func queue_update() -> void:
	if _pending_update:
		return
	_update.call_deferred()
	_pending_update = true
