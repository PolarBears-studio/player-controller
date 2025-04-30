using Godot;

// This script is an example of how you can create game systems that
// interact with PlayerController. This script applies a low gravity effect
// to any PlayerController that enters the Area3D. It does this by modifying
// the value of AdditionalGravityPower owned by the Gravity child of
// PlayerController.

namespace PolarBears.PlayerControllerAddon;

public partial class LowGravityArea3D : Area3D
{
	[Export] public float GravityReduction { set; get; } = 0.4f;

	public override void _Ready()
	{
		BodyEntered += (Node3D body) =>
		{
			if (body is PlayerController player) {
				player.Gravity.AdditionalGravityPower *= GravityReduction;
				GD.Print("Low Gravity Zone Entered");
			}
		};
		BodyExited += (Node3D body) =>
		{
			if (body is PlayerController player) {
				player.Gravity.AdditionalGravityPower /= GravityReduction;
				GD.Print("Low Gravity Zone Exited");
			}
		};
	}
}

