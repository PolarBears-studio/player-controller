# General Information

🚀 Plug & Play First-Person Controller for Godot Mono 4.4+
Just drag, drop, and you're ready to go FPS-style! This first-person character controller is tailor-made for developers who want solid, smooth, and satisfying movement right out of the box.

💡 Core Features That Feel Great

* 🏃‍♂️ Movement – Smooth walking, running, and turning that feels intuitive and responsive
* 🧍‍♂️ Jumping, crouching, and smooth crouch jumps!
* 🔧 Automatic handling of low height bumps and obstacles – Automatically adjusts your character’s movement to smoothly handle low height bumps and small obstacles
* 🪜 Automatic stair climbing – walk up and down step-shaped terrain effortlessly
* 💥 Health, damage, and death – complete with dramatic shader effects
* 🤸 Head-bump protection – jump without getting stuck in ceilings
* 🎢 Bobbing movement – immersive bounce while walking, crouching, and sprinting to bring your character to life
* 🚫 No mesh invasion – tight collision keeps your player out of walls, floors, and ceilings where they don’t belong
* 🔧 Developer API – Fully customizable! Access key functions through an easy-to-use API to tweak or extend the controller to fit your project needs  

👨‍💻 Built by devs, for devs — clean, extendable, and battle-tested. Just drop it into your scene and go! 🛠️
Perfect for FPS games, exploration projects, or rapid prototyping.  

## Requirements
[Godot .NET 4.4](https://godotengine.org/download/archive/)

## Other links

* [Asset libary link for the addon](https://godotengine.org/asset-library/asset/4020)
* Contribute or post issues on our [GitHub Repository](https://github.com/PolarBears-studio/player-controller) 
* Join our [discord server](https://discord.com/channels/1165743149621182605/1368527596907790347) when you have questions about the player controller

## Troubleshooting

### If you see "Cannot instantiate C# script because..."

Godot .NET detects whether you are using C# in your project, and if you are it will reveal the "Build Project" (looks like a hammer) button to the left of "Run Project". However, when you import our addon into a Godot project that previously had no C# scripts, this button may not appear. Should this happen to you, simply navigate to and click on **Project** > **Tools** > **C#** > **Create C# Solution**. The "Build Project" hammer will now appear. Click it to build C# scripts and the message should go away.
