using Godot;

namespace Exodus.Scripts.Player.PlayerController;

public partial class Gravity: Node3D
{
    [Export(PropertyHint.Range, "50,110,")]
    private int _weight = 70;
    
    [Export] private float StartVelocity = 3f;
    [Export] private float AdditionalGravityPower = 2f;
    
    private float _gravity;

    public void Init(float gravitySetting)
    {
        _gravity = gravitySetting;
    }

    public float CalculateJumpForce() => _weight * (_gravity * (StartVelocity / AdditionalGravityPower));
    public float CalculateGravityForce() => _gravity * _weight / 30;
}