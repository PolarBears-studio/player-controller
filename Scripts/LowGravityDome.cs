using Godot;
using System;

namespace Exodus.Scripts.Player.PlayerController;

public partial class LowGravityDome : Area3D
{
	[Export] public float GravityReduction = 0.4f;

	public override void _Ready()
	{
		BodyEntered += (Node3D body) =>
		{
			if (body is PlayerController player) {
				player.Gravity.AdditionalGravityPower *= GravityReduction;
				GD.Print("player.Gravity.AdditionalGravityPower = ", player.Gravity.AdditionalGravityPower);
			}
		};
		BodyExited += (Node3D body) =>
		{
			if (body is PlayerController player) {
				player.Gravity.AdditionalGravityPower /= GravityReduction;
				GD.Print("player.Gravity.AdditionalGravityPower = ", player.Gravity.AdditionalGravityPower);
			}
		};
	}
}

