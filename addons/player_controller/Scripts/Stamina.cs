using System;
using Godot;

namespace PolarBears.PlayerControllerAddon;

public partial class Stamina : Node
{
	[Export(PropertyHint.Range, "0,60,,suffix:s,or_greater")]
	public float _maxRunTime { get; set; } = 10.0f;
	// Regenerate run time multiplier (when run 10s and _regRunTimeMultiplier = 2.0f to full regenerate you need 5s)
	[Export(PropertyHint.Range, "0,10,,or_greater")]
	public float _regRunTimeMultiplier { get; set; } = 2.0f;

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
			float runtimeLeft = _currentRunTime - (_regRunTimeMultiplier * (float)delta);
			
			if (_currentRunTime != 0.0f)
				_currentRunTime = Mathf.Clamp(runtimeLeft, 0, _maxRunTime);
			
			return wantedSpeed;
		}

		_currentRunTime = Mathf.Clamp(_currentRunTime + (float) delta, 0, _maxRunTime);
		
		return _currentRunTime >= _maxRunTime ? _walkSpeed : wantedSpeed;
	}
}
