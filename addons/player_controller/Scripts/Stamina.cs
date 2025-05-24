using Godot;

namespace PolarBears.PlayerControllerAddon;

public partial class Stamina : Node
{
	[Export]
	public bool LimitlessSprint { get; set; } = false;
	[Export(PropertyHint.Range, "0,60,0.1,suffix:s,or_greater")]
	public float MaxSprintTime { get; set; } = 10.0f;
	// Regenerate run time multiplier (when run 10s and SprintTimeRegenerationMultiplier = 2.0f to full regenerate you need 5s)
	[Export(PropertyHint.Range, "0,10,0.01,or_greater")]
	public float SprintTimeRegenerationMultiplier { get; set; } = 2.0f;

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
		if (LimitlessSprint)
		{
			return wantedSpeed;
		}
		if (Mathf.Abs(wantedSpeed - _sprintSpeed) > 0.1f)
		{
			float runtimeLeft = _currentRunTime - (SprintTimeRegenerationMultiplier * (float)delta);
			
			if (_currentRunTime != 0.0f)
				_currentRunTime = Mathf.Clamp(runtimeLeft, 0, MaxSprintTime);
			
			return wantedSpeed;
		}

		_currentRunTime = Mathf.Clamp(_currentRunTime + (float) delta, 0, MaxSprintTime);
		
		return _currentRunTime >= MaxSprintTime ? _walkSpeed : wantedSpeed;
	}
}
