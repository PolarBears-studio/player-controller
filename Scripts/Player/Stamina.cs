using System;
using Godot;

namespace Exodus.Scripts;

public partial class Stamina : Node
{
	[Export] private float _maxRunTime = 10.0f;
	// Regenerate run time multiplier (when run 10s and _regRunTimeMultiplier = 2.0f to full regenerate you need 5s)
	[Export] private float _regRunTimeMultiplier = 2.0f;

	private float _currentRunTime;

	private float _walkSpeed;
	private float _sprintSpeed;

	public void SetSpeeds(float walkSpeed, float sprintSpeed)
	{
		this._walkSpeed = walkSpeed;
		this._sprintSpeed = sprintSpeed;
	}

	public float AccountStamina(double delta, float wantedSpeed)
	{
		if (Math.Abs(wantedSpeed - _sprintSpeed) > 0.1f)
		{
			float runtimeLeft = _currentRunTime - (_regRunTimeMultiplier * (float)delta);
			
			if (_currentRunTime != 0.0f)
				_currentRunTime = Math.Clamp(runtimeLeft, 0, _maxRunTime);
			
			return wantedSpeed;
		}

		_currentRunTime = Math.Clamp(_currentRunTime + (float) delta, 0, _maxRunTime);
		
		return _currentRunTime >= _maxRunTime ? _walkSpeed : wantedSpeed;
	}
}
