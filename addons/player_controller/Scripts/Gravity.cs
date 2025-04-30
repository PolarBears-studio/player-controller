using Godot;

namespace Exodus.Scripts.Player.PlayerController;

public partial class Gravity: Node3D
{
	[Export(PropertyHint.Range, "0,100,0.1,or_greater")]
	public float Weight { get; set; } = 70.0f;
	[Export(PropertyHint.Range, "0,20,0.1,or_greater")]
	public float StartVelocity { get; set; } = 3.0f;
	[Export(PropertyHint.Range, "0.1,10,0.1,or_greater")]
	public float AdditionalGravityPower { get; set; } = 2f;

	private float _gravity;

	public void Init(float gravitySetting)
	{
		_gravity = gravitySetting;
	}

	public float CalculateJumpForce() => Weight * (_gravity * (StartVelocity / AdditionalGravityPower));
	public float CalculateGravityForce() => _gravity * Weight / 30.0f;
}
