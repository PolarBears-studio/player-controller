# API Reference
This page is a comprehensive guide to everything the plugin is capable of. It is organized by each script that makes the **PlayerController.tscn** scene work. We will start with an explanation of the PlayerController.cs script and its exports.

The root PlayerController.cs script provides public access to its child scripts, so that an external script can easily gain access to either PlayerController's members or the members of its children. Any one of the exports mentioned here in this document may be accessed by a script that has a reference to the node by accessing the member of the same name as what shows in the editor inspector, but without spaces. For example, to get or set the Player's walk speed use `Player.WalkSpeed`, or to get or set the Player's current health use `Player.HealthSystem.CurrentHealth`. 

## PlayerController (Player in Scene pane)

This is the root script of the scene and therefore manages all of its childs' operations. It inherits from a [CharacterBody3D](https://docs.godotengine.org/en/latest/classes/class_characterbody3d.html).

| Export Name | Description |
| :--                      | :--         |
| Walk Speed               | The speed in meters per second the player can walk at |
| Sprint Speed             | The speed in meters per second the player can sprint at (hold shift) |
| Crouch Speed             | The speed in meters per second the player can move at while crouched (hold ctrl) |
| Crouch Transition Speed  | The speed in meters per second at which the player can transition from standing to crouching and vice verse |

The Player node also implements signals developers can trigger their own scripted events off of.

| Signal Name | Description |
| :--     | :-- |
| Jumped  | This emits from the Player node at the start of a player's jump |
| HeadHit | This emits whenever the player's head collides with a surface |



### Input

The Player node is meant to be drag-and-drop, so by default typically WASD keyboard and mouse input is hard-coded into the PlayerController script. However, a user can easily override some or all of these inputs by setting the values in the **Input** section of the Player inspector to input action names present in the user's project. Controller buttons can even been supported via this method. For more details on how to implement input actions into your project, refer to the [Godot documention on InputMap](https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html#inputmap). The following is a table of actions supported by Player and their corresponding input action override value.

| Action        | Key | Override Export String |
| :--           | :-- | :-- |
| Move Foward   | W   | Move Forward Input Action |
| Move Backward | S   | Move Backward Input Action |
| Strafe Left   | A   | Strafe Left Input Action |
| Strafe Right  | D   | Strafe Right Input Action |
| Jump          | Space bar | Jump Input Action |
| Crouch        | Ctrl | Crouch Input Action |
| Sprint        | Shift | Sprint Input Action |

Since the look control is handled by a different script, refer to the [Mouse section](api-reference.md#Mouse) later in this document to learn how to switch mouse look to right analog look and adjust the sensitivity for both.

## CapsuleCollider

This script is responsible for managing the height of the collision capsule. It inherits from a [CollisionShape3D](https://docs.godotengine.org/en/latest/classes/class_collisionshape3d.html)

| Export Name | Description |
| :--                      | :--         |
| Capsule Default Height   | The height of the player's capsule collider nominally. |
| Capsule Crouch Height    | The height of the player's capsule collider while crouching. |

## HealthSystem

This script is responsible for managing the player's health and rendering both the damage and death effects. It inherits from [Node3D](https://docs.godotengine.org/en/latest/classes/class_node3d.html).

### Health Metrics

#### Amounts

| Export Name | Description |
| :--                      | :--         |
| Max Health               | Maximum health of the player. CurrentHealth will never go above this value. |
| Current Health           | The health of the player at the start of a scene. |
| Minimal Damage Unit      | The minimal amount the player may be damaged by when falling. Also used for the damage debug tool that is activated by pressing "H" |

#### Regeneration

| Export Name    | Description |
| :--                         | :--         |
| Seconds Before Regeneration | The delay before health regeneration kicks in. |
| Regeneration Speed          | The speed at which health regenerates in units of hit points per second. If no regeneration is desired, setting this to zero will disable health regeneration. |

### Damage Camera Effects

#### Camera Shake

| Export Name     | Description |
| :--             | :--         |
| Rotation Speed  | The speed at which the camera spins along its z-axis when the Player takes damage. |
| Rotation Degree | The maximum angle, in degrees, by which the camera spins along its z-axis when the Player takes damage. To disable the camera shake effect, set this value to zero. |

#### Visual Distortion
| Export Name    | Description |
| :--                         | :--         |
| 


## Stamina

## StairsSystem

## Gravity

## Bobbing

## Mouse

## FieldOfView

## AnimationPlayer
