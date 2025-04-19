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
				player.Gravity.SetAdditionalGravityPower(player.Gravity.GetAdditionalGravityPower() * GravityReduction);
				GD.Print("player.Gravity.AdditionalGravityPower = ", player.Gravity.GetAdditionalGravityPower());
			}
		};
		BodyExited += (Node3D body) =>
		{
			if (body is PlayerController player) {
				player.Gravity.SetAdditionalGravityPower(player.Gravity.GetAdditionalGravityPower() / GravityReduction);
				GD.Print("player.Gravity.AdditionalGravityPower = ", player.Gravity.GetAdditionalGravityPower());
			}
		};
	}
}

