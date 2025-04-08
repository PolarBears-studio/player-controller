@tool
extends EditorPlugin

const STAIR_3D_SCRIPT := preload("stairs_3d.gd")
const STAIR_3D_ICON := preload("Stairs3D.svg")

func _enter_tree():
	add_custom_type("Stairs3D", "Path3D", STAIR_3D_SCRIPT, STAIR_3D_ICON)


func _exit_tree():
	remove_custom_type("Stairs3D")
