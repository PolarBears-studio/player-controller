using Godot;
using System;

namespace PolarBears.PlayerControllerAddon;

public partial class LowGravityDome : Area3D
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

