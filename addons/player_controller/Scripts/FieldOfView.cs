using Godot;

namespace PolarBears.PlayerControllerAddon;

public partial class FieldOfView: Node3D
{
	[Export(PropertyHint.Range, "0,180,0.1,degrees")]
	public float BaseFov         { get; set; } = 75.0f;
	[Export(PropertyHint.Range, "0,10,0.01,or_greater")]
	public float FovChangeFactor { get; set; } = 1.2f;
	[Export(PropertyHint.Range, "0,10,0.01,or_greater")]
	public float FovChangeSpeed  { get; set; } = 6.25f;

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
			parameters.Velocity.Length(), 0.5f, parameters.SprintSpeed * 2.0f);

		float targetFov = BaseFov + FovChangeFactor * velocityClamped;

		if (parameters.IsCrouchingHeight){
			targetFov = BaseFov - FovChangeFactor  * velocityClamped;
		}

		_camera.Fov = Mathf.Lerp(_camera.Fov, targetFov, parameters.Delta * FovChangeSpeed);
	}
}
