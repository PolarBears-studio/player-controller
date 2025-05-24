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

Refer to the [Tutorial 3.1](tutorial-3.md#tutorial-31-utilizing-playercontroller-signal-api) for instructions on utilizing Player signals.

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

This node also supports signals a developer can trigger their own code off of.

| Signal Name     | Description |
| :--             | :--         |
| Damaged(float)  | Emitted whenever the player is damaged, and passes the amount the player was damaged by in the signal. |
| Died            | Emitted whenever the player is killed. |
| FullyRecovered  | Emitted whenever the player has fully regenerated their health. |

Refer to the [Tutorial 3.1](tutorial-3.md#tutorial-31-utilizing-playercontroller-signal-api) for instructions on utilizing Player signals.

### Health Metrics

This inspector section contains all properties relating to health values and their regeneration.

#### Amounts

| Export Name | Description |
| :--                      | :--         |
| Max Health               | Maximum health of the player. Current Health will never go above this value. |
| Current Health           | The health of the player at the start of a scene. |
| Minimal Damage Unit      | The minimal amount the player may be damaged by when falling. Also used for the damage debug tool that is activated by pressing "H" |

#### Regeneration

These properties control how the player regenerates health over time. If no regeneration is desired for your project, **Regeneration Speed** can be set to zero to disable it.

| Export Name    | Description |
| :--                         | :--         |
| Seconds Before Regeneration | The delay before health regeneration kicks in. |
| Regeneration Speed          | The speed at which health regenerates in units of hit points per second. |

### Damage Camera Effects

The health system features a robust system of visual effects from a distortion shader to head shake upon damage taken, all of which can be configured with a large suite of properties.

!!! info "Damaged Debug Tool"

    TODO fill out this section

#### Camera Shake

When the player takes damage the first-person camera will be quickly rotated off center-view as if recoiling from an attack.

| Export Name     | Description |
| :--             | :--         |
| Rotation Speed  | The speed at which the camera spins along its z-axis when the Player takes damage. |
| Rotation Degree | The maximum angle, in degrees, by which the camera spins along its z-axis when the Player takes damage. To disable the camera shake effect, set this value to zero. |

#### Visual Distortion

The visual distortion effect applies a de-saturation, Perlin noise distortion effect, and vignetting to the first-person camera's view that all scale relative to the player's damage level.

| Export Name         | Description |
| :--                 | :--         |
| Screen Darkness Max/Min  |  |
| Distortion Speed Max/Min |  |
| Distortion Size Max/Min  |  |

#### Vignetting

When the player is damage, a vignette will appear that oscillates. The amplitude of its oscillations are relative to the player's health level.

| Export Name     | Description |
| :--             | :-- |
| Active Zone Multiplier Max/Min | |
| Multiplier Delta for Animation | |
| Softness        |  |
| Speed Max/Min |  |

### Death

When the player meets their demise, the camera will be procedurally animated to drop to the floor and the screen will full de-saturate and fade out.

#### Before Fade Out

#### Speeds

#### Target Values

#### Other

## Stamina

TODO verify statements and test limitless sprint

This node controls the player's ability to maintain a sprint. The ability to Sprint can be disabled by setting **Max Run Time** to zero. To allow the player to sprint indefinitely, toggle **Limitless Sprint** to true; when true the other 2 parameters in this node will have no effect.

| Export Name         | Description |
| :--                 | :--         |
| Limitless Sprint    | Set this to true to allow the player to sprint indefinitely. |
| Max Run Time        | The maximum time the player is allowed to sprint for before becoming exhausted. |
| Sprint Time Regeneration Multiplier | A multiplier that determines the regeneration speed of the player. Full stamina regeneration will always be equal to the Max Run Time divided by this value. For example, if this value equals 2 and the **Max Sprint Time** equals 10, then the player will regenerate stamina in 10 / 2 = 5 seconds. |

## StairsSystem

Any stair shaped collision object or curb can be procedurally handled by this node, allowing players to ascend and descend whatever whatever heights you deem them capable of stepping over.

| Export Name      | Description |
| :--              | :--         |
| Max Step Height  | The maximum height of a curb or stair step that the play can stride up onto. Anything higher than this value will be treated as a wall, but could still be jumped over if the jump is set high enough. |

## Gravity

Control's how the player falls back to the ground and how high they can jump.

| Export Name      | Description |
| :--              | :--         |
| Weight           | Not weight in the physics sense. Behaves more like a multiplier on both jumps and the force of gravity. |
| Start Velocity   | Not velocity in the physics sense. Behaves like a multiplier on the player's jump. Higher values results to higher jump heights. |
| Additional Gravity Power | Acts as a multiplier to makes jumps higher. The lower this value, the higher the player can jump. |

## Bobbing

As the player moves they bob their head to the rhythm of their movement. To disable head bob entirely, simply set **Bobbing Amplitude** to zero.

| Export Name      | Description |
| :--              | :--         |
| Bobbing Frequency | The rate at which bobbing occurs. |
| Bobbing Amplitude | The maximum deviation from center of a head bob. |

## Mouse

This node is responsible for rotating the player controller and it's camera so the player can look around the world.

| Export Name     | Description |
| :--             | :--         |
| Sensitivity     | The speed of movement relative to mouse/controller motions. High values results in faster movement. |
| Use Controller  | If true, right analog controller look will be used instead of mouse. The speed of looking will still be controlled by **Sensitivity**. |

## FieldOfView

Controls field of view (FOV) slide effects, like when the player sprints. If no FOV sliding is desired, this can be disabled by setting **FOV Change Factor** to 0.0.

| Export Name    | Description |
| :--            | :--         |
| Base FOV       | The nominal FOV value of the first-person camera. |
| FOV Change Factor | A multiplier to velocity that determines by how much the FOV increased above **Base FOV**. The higher the value, the greater the change in FOV from sprinting will be. |
| FOV Change Speed | The speed at which FOV is transition between Base FOV and the modified FOV, and vice versa. |
