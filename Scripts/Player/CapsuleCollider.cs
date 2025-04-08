using ExodusGlobal;
using Godot;

namespace Exodus.Scripts.Player.PlayerController;

public partial class CapsuleCollider: Node3D
{
    private CapsuleShape3D _playerCapsuleShape;
    
    private const float CapsuleDefaultHeight = 2.0f;
    private const float CapsuleCrouchHeight = 1.0f;

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

    public bool BetweenCrouchingAndNormalHeight()
    {
        return _playerCapsuleShape.Height > CapsuleCrouchHeight && _playerCapsuleShape.Height < CapsuleDefaultHeight;
    }

    public bool IsDefaultHeight()
    {
        return CustomMath.AreAlmostEqual(_playerCapsuleShape.Height,  CapsuleDefaultHeight);
    }

    public bool IsCrouchingHeight()
    {
        return CustomMath.AreAlmostEqual(_playerCapsuleShape.Height, CapsuleCrouchHeight);
    }

    public void PerformCrouching(float delta, float crouchTransitionSpeed)
    {
        _playerCapsuleShape.Height -= delta * crouchTransitionSpeed;
        
        _playerCapsuleShape.Height = Mathf.Clamp(
            _playerCapsuleShape.Height, CapsuleCrouchHeight, CapsuleDefaultHeight);
    }

    public void UndoCrouching(float delta, float crouchTransitionSpeed)
    {
        _playerCapsuleShape.Height += delta * crouchTransitionSpeed;
        
        _playerCapsuleShape.Height = Mathf.Clamp(
            _playerCapsuleShape.Height, CapsuleCrouchHeight, CapsuleDefaultHeight);
    }
}