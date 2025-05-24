using Godot;

namespace PolarBears.PlayerControllerAddon;

public partial class Bobbing: Node3D
{
	[Export(PropertyHint.Range, "0,10,0.01,or_greater")]
	public float BobbingFrequency { set; get; } = 2.4f;
	[Export(PropertyHint.Range, "0,0.4,0.01,or_greater")]
	public float BobbingAmplitude { set; get; } = 0.08f;

	private Camera3D _camera;

	public void Init(Camera3D cam)
	{
		_camera = cam;
	}

	public struct CameraBobbingParams
	{
		public float Delta;
		public bool IsOnFloorCustom;
		public Vector3 Velocity;
	}

	private float _bobbingAccumulator;  // Constantly increases when player moves in X or/and Z axis

	public void PerformCameraBobbing(CameraBobbingParams parameters)
	{
		if (parameters.IsOnFloorCustom)
		{
			// Head bob
			_bobbingAccumulator += parameters.Delta * parameters.Velocity.Length();

			Vector3 newPositionForCamera = Vector3.Zero;

			// As the _bobbingAccumulator increases we're changing values for sin and cos functions.
			// Because both of them are just waves, we will be slide up with y and then slide down with y
			// creating bobbing effect. The same works for cos. As the _bobbingAccumulator increases the cos decreases and then increases

			newPositionForCamera.Y = Mathf.Sin(_bobbingAccumulator * BobbingFrequency) * BobbingAmplitude;
			newPositionForCamera.X = Mathf.Cos(_bobbingAccumulator * BobbingFrequency / 2.0f) * BobbingAmplitude;

			_camera.Position = newPositionForCamera;
		}
	}
}
