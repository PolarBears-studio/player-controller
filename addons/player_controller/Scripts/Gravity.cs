using Godot;

namespace Exodus.Scripts.Player.PlayerController;

public partial class Gravity: Node3D
{
	[Export(PropertyHint.Range, "50,110,")]
	public float _weight = 70;

	[Export] private float StartVelocity = 3f;
	public float GetStartVelocity() {
		return StartVelocity;
	}
	public void SetStartVelocity(float value) {
		StartVelocity = value;
	}

	[Export] private float AdditionalGravityPower = 2f;
	public float GetAdditionalGravityPower() {
		return AdditionalGravityPower;
	}
	public void SetAdditionalGravityPower(float value) {
		AdditionalGravityPower = value;
	}

	private float _gravity;

	public void Init(float gravitySetting)
	{
		_gravity = gravitySetting;
	}

	public float CalculateJumpForce() => _weight * (_gravity * (StartVelocity / AdditionalGravityPower));
	public float CalculateGravityForce() => _gravity * _weight / 30;
}
