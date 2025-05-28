# Tutorial 1.0 - Add PlayerController to your Game World

After installing the addon from the AssetLib, a PlayerController node can be added into your scene using the Scene pane. PlayerController is implemented as a scene hierarchy, which means it must be instantiated in your game world as a child scene. Click the chain link icon at the top of the Scene pane in Godot.

![](images/getting-started_instantiate-child-scene.png)

This will bring up a window that may be blank or may contain some of your already saved scenes. In the bottom right of the window, make sure to toggle on the Addons switch. You can now find PlayerController.tscn in the list or search for it.

![](images/getting-started_select-scene-addons.png)

Select **PlayerController.tscn**.

![](images/getting-started_player-controller.png)

The default capsule mesh of the PlayerController scene will have been added to the scene. It's name in the node hierarchy is simply "Player". It may not be placed in the scene in an ideal location, so be sure to adjust it's transform until the capsule shape is not obscured by any other collision geometry. 

![](images/getting-started_player-controller-placement-1.png)

![](images/getting-started_player-controller-placement-2.png)

For PlayerController to work properly, make sure that some collision geometry is present in your world and is on Collision Layer 1 (in the next section you will see how this parameter can be adjusted). 

You can now try out PlayerController in your game world.

![](images/getting-started_play-scene.png)
