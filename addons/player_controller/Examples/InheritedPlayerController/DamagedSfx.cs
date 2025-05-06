using Godot;
using System;

public partial class DamagedSfx : AudioStreamPlayer3D
{
	public void OnPlayerDamaged(float damage_amount)
	{
		PitchScale = Mathf.Remap(damage_amount, 0f, 100f, 0.8f, 1.2f);
		Play();
	}
}
