using Godot;

namespace Exodus.Scripts.Player.PlayerController;

public partial class FieldOfView: Node3D
{
    [Export] private float BaseFov = 75.0f;
    [Export] private float FovChangeFactor = 1.2f;
    [Export] private float FovChangeSpeed = 6.25f;
    
    private Camera3D _camera;

    public void Init(Camera3D cam)
    {
        _camera = cam;
    }

    public struct FovParameters
    {
        public bool IsCrouchingHeight;
        public float Delta;
        public float SprintSpeed;
        public Vector3 Velocity;
    }
    
    public void PerformFovAdjustment(FovParameters parameters)
    {
        float velocityClamped = Mathf.Clamp(
            parameters.Velocity.Length(), 0.5f, parameters.SprintSpeed * 2);

        float targetFov = BaseFov + FovChangeFactor * velocityClamped;
			
        if (parameters.IsCrouchingHeight){
            targetFov = BaseFov - FovChangeFactor  * velocityClamped;
        }

        _camera.Fov = Mathf.Lerp(_camera.Fov, targetFov, parameters.Delta * FovChangeSpeed);
    }
}