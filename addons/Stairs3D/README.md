# Stairs3D
Addon for creating procedural stairs in Godot Engine

I made this to test my stair climbing mechanic for my character controller. Not intended for use in actual games.

![image](https://user-images.githubusercontent.com/47208466/172578880-e70a08d8-ce26-44e5-ab01-83f61b877181.png)

## Usage
Add the Stairs3D node from the node menu.

Add points by using Ctrl + left click, delete with right click.

Shift + left click and drag on points to control the curvature.

	generate_bottom

Controls whether the stair generates a base to the path or not.

	height

The maximum distance the stairs will generate away from the path.

	width, step_height

¯\\\_(ツ)\_/¯

	stringer_material, riser_material, thread_material

Materials for the side/base, front and top of steps. Using a triplanar material is recommended (especially for the stringer). Stringer material will be used if the other materials are not assigned.

	make_simple_collision, collision_layer, collision_mask

Collision settings.
