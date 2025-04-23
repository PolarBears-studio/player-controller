using Godot;

namespace Exodus.Scripts.Player.PlayerController;

public partial class Gravity: Node3D
{
	// NOTE: I don't find this API very intuitive. Weight is actually being used
	// as mass, I do not know why it is being divided by 30, StartVelocity is not
	// being used as a velocity at all, and it's impossible for me to know the
	// units or scale of AdditionalGravityPower.
	// I am viewing this from a physics perspective. Maybe this is a perfectly
	// normal formula game dev's use I just never encountered.

	[Export(PropertyHint.Range, "0,100,,or_greater")]
	public float Weight { get; set; } = 70.0f;
	[Export(PropertyHint.Range, "0,20,,or_greater")]
	public float StartVelocity { get; set; } = 3.0f;
	[Export(PropertyHint.Range, "0.1,10,,or_greater")]
	public float AdditionalGravityPower { get; set; } = 2f;

	private float _gravity;

	public void Init(float gravitySetting)
	{
		_gravity = gravitySetting;
	}

	public float CalculateJumpForce() => Weight * (_gravity * (StartVelocity / AdditionalGravityPower));
	public float CalculateGravityForce() => _gravity * Weight / 30;
}
