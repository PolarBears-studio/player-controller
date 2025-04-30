using Godot;

namespace PolarBears.PlayerControllerAddon;

public partial class Stamina : Node
{
	[Export(PropertyHint.Range, "0,60,0.1,suffix:s,or_greater")]
	public float MaxRunTime { get; set; } = 10.0f;
	// Regenerate run time multiplier (when run 10s and RunTimeMultiplier = 2.0f to full regenerate you need 5s)
	[Export(PropertyHint.Range, "0,10,0.01,or_greater")]
	public float RunTimeMultiplier { get; set; } = 2.0f;

	private float _currentRunTime;

	private float _walkSpeed;
	private float _sprintSpeed;

	public void SetSpeeds(float walkSpeed, float sprintSpeed)
	{
		_walkSpeed = walkSpeed;
		_sprintSpeed = sprintSpeed;
	}

	public float AccountStamina(double delta, float wantedSpeed)
	{
		if (Mathf.Abs(wantedSpeed - _sprintSpeed) > 0.1f)
		{
			float runtimeLeft = _currentRunTime - (RunTimeMultiplier * (float)delta);
			
			if (_currentRunTime != 0.0f)
				_currentRunTime = Mathf.Clamp(runtimeLeft, 0, MaxRunTime);
			
			return wantedSpeed;
		}

		_currentRunTime = Mathf.Clamp(_currentRunTime + (float) delta, 0, MaxRunTime);
		
		return _currentRunTime >= MaxRunTime ? _walkSpeed : wantedSpeed;
	}
}
