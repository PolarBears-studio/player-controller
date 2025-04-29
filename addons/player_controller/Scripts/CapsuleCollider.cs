using PlayerControllerGlobal;
using Godot;

// RECOMMENDATION: Why not attached this script to the CollisionShape3D?

namespace Exodus.Scripts.Player.PlayerController;

public partial class CapsuleCollider: Node3D
{
    // RECOMMENDATION rename to CapsuleStandHeight
    [Export(PropertyHint.Range, "0,5.0,,suffix:m,or_greater")]
    public float CapsuleDefaultHeight { get; set; } = 2.0f;
    [Export(PropertyHint.Range, "0,5.0,,suffix:m,or_greater")]
    public float CapsuleCrouchHeight  { get; set; } = 1.0f;

    private CapsuleShape3D _playerCapsuleShape;

    public void Init(CapsuleShape3D playerCapsuleShape)
    {
        _playerCapsuleShape = playerCapsuleShape;
    }
    
    public float GetCurrentHeight() { return _playerCapsuleShape.Height; }
    public float GetDefaultHeight() { return CapsuleDefaultHeight; }
    
    public bool IsCapsuleHeightLessThanNormal()
    {
        return _playerCapsuleShape.Height < CapsuleDefaultHeight;
    }

    public bool IsBetweenCrouchingAndNormalHeight()
    {
        return _playerCapsuleShape.Height > CapsuleCrouchHeight
            && _playerCapsuleShape.Height < CapsuleDefaultHeight;
    }

    public bool IsDefaultHeight()
    {
        return Mathf.IsEqualApprox(_playerCapsuleShape.Height,  CapsuleDefaultHeight);
    }

    public bool IsCrouchingHeight()
    {
        return Mathf.IsEqualApprox(_playerCapsuleShape.Height, CapsuleCrouchHeight);
    }

    // RECOMMENDATION: name Crouch()
    public void PerformCrouching(float delta, float crouchTransitionSpeed)
    {
        _playerCapsuleShape.Height -= delta * crouchTransitionSpeed;
        
        _playerCapsuleShape.Height = Mathf.Clamp(
            _playerCapsuleShape.Height, CapsuleCrouchHeight, CapsuleDefaultHeight);
    }

    // RECOMMENDATION: name Stand()
    public void UndoCrouching(float delta, float crouchTransitionSpeed)
    {
        _playerCapsuleShape.Height += delta * crouchTransitionSpeed;
        
        _playerCapsuleShape.Height = Mathf.Clamp(
            _playerCapsuleShape.Height, CapsuleCrouchHeight, CapsuleDefaultHeight);
    }
}
