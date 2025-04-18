using System;
using Godot;

namespace Exodus.Scripts.Player.PlayerController;

public partial class Mouse : Node3D
{
    
    private Node3D _head;
    private Camera3D _camera;
    private const float Sensitivity = 0.004f;

    public delegate bool IsDead();

    private IsDead _isPlayerDead;
    
    public void Init(Node3D head, Camera3D cam, IsDead isDeadFunc)
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        
        _head = head;
        _camera = cam;
        _isPlayerDead = isDeadFunc;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_isPlayerDead()) return;
        
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            // Horizontal movement of head
            float angleForHorizontalRotation = -eventMouseMotion.Relative.X * Sensitivity;
            _head.RotateY(angleForHorizontalRotation);

            // Vertical movement of head
            Vector3 currentCameraRotation = _camera.Rotation;
            currentCameraRotation.X += Convert.ToSingle(-eventMouseMotion.Relative.Y * Sensitivity);
            currentCameraRotation.X = Mathf.Clamp (currentCameraRotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90));

            _camera.Rotation = currentCameraRotation;				
        }
    }
}
