using System;
using Exodus.Scripts.Player.PlayerController;
using ExodusGlobal;
using Godot;

namespace Exodus.Scripts.Player;


public partial class HealthSystem : Node3D
{
	new enum Rotation
	{
		NoRotation = 0,
		CameraRotationTriggered = 1,
		RotatingOnZAxis = 2,
		ReturningBack = 3,
	}
	
	[ExportGroup("General")]
	
	[Export(PropertyHint.Range, "0,100,")]
	private float _maxHealth = 100;
	
	[Export(PropertyHint.Range, "0,100,")]
	private float _currentHealth = 100;
	
	// Required to hide Vignette effect
	private float _currentHealthInPrevFrame;
	
	private float _thresholdVelYForDamage = -15.0f;
	
	private float _currentVelocityYInAir;
	private Gravity _gravity;
    
	private CharacterBody3D _characterBody3D;

	private float _minimalDamageUnit = 25f;
	
	[ExportGroup("Camera rotation when taking damage")]
	[Export] private float _rotationSpeed = 9.0f;
	[Export] private float _rotationDegree = 14f;

	private Camera3D _camera;
	private float _cameraInitialRotationZ;
	private float _targetRotationZAxis;
	private Rotation _cameraRotation = Rotation.NoRotation;
	private float _progressOnCamRotation;

	[ExportGroup("Distortion effect (When alive)")] 
	
	// Screen darkness controls how dark the screen will be, where 0.0 - natural color of the screen(unaltered)
	// and 1.0 - black screen
	
	[Export(PropertyHint.Range, "0.0,1.0,")]
	private float _screenDarknessMin;
	
	[Export(PropertyHint.Range, "0.0,1.0,")]
	private float _screenDarknessMax = 0.3f;
	
	[Export(PropertyHint.Range, "0.0,1.0,")]
	private float _distortionSpeedMin;
	
	[Export(PropertyHint.Range, "0.0,1.0,")]
	private float _distortionSpeedMax = 0.6f;

	[Export(PropertyHint.Range, "0.0,1.0,")]
	private float _distortionSizeMin;
	
	[Export(PropertyHint.Range, "0.0,1.0,")]
	private float _distortionSizeMax = 0.008f;
	
	private Vector2 _uvOffset = Vector2.Zero;
	private float _offsetResetThreshold = 5.0f;
	
	private ShaderMaterial _distortionMaterial;
	
	// TODO: add description to these fields
	[ExportGroup("Vignette effect (When alive)")] 
	[Export(PropertyHint.Range, "0.0,0.5,")]
	private float _activeZoneMultiplierMin;
	
	[Export(PropertyHint.Range, "0.0,0.6,")]
	private float _activeZoneMultiplierMax;
	
	[Export(PropertyHint.Range, "0.0,1.0,")]
	private float _multiplierDeltaForAnimation = 0.1f;
	
	[Export(PropertyHint.Range, "0.0,1.0,")]
	private float _softness = 1.0f;
	
	[Export] private float _speedMin = 1.6f;
	[Export] private float _speedMax = 4f;
	
	private float _timeAccumulator;
	
	private float _currentSpeed;

	private const float InitialMultiplierMidVal = 0.6f;
	private const float MultiplierMidValToHideVignette = 0.8f; 
	private float _currentMultiplierMidValue;
	
	private ShaderMaterial _vignetteMaterial;

	[ExportGroup("Regeneration")] 
	[Export(PropertyHint.Range, "1.0,10.0,")] private float _timeInSecondsBeforeRegeneration = 10f;
	[Export(PropertyHint.Range, "1.0,10.0,")] private float _regenerationSpeed = 10f;
	
	// Death / GameOver
	[ExportGroup("Death")] 
	[ExportSubgroup("Before Fade Out")]
	[Export(PropertyHint.Range, "0.2,0.5")] private float _blurLimitValueToStartFadeOut = 0.3f;
	[Export(PropertyHint.Range, "0.0,4.0")] private float _blurValueToStartFadeOut = 3.376f;
	
	[ExportSubgroup("Speeds")]
	[Export(PropertyHint.Range, "1.0,20.0")] private float _cameraDropSpeedOnDeath = 18f;
	[Export(PropertyHint.Range, "0.05,5.0")]private float _fadeOutSpeed = 0.11f;

	[Export(PropertyHint.Range, "0.0,10.0")] private float _blurLimitSpeedOnDeath = 0.9f;
	[Export(PropertyHint.Range, "0.0,10.0")] private float _blurSpeedOnDeath = 1.5f;
		
	[ExportSubgroup("Target values")]
	[Export(PropertyHint.Range, "1.0,0.5")] private float _cameraHeightOnDeath = 0.68f;
	[Export(PropertyHint.Range, "1.0,5.0")] private float _fadeOutTargetValue = 4.0f;
	
	// TODO: add setter: _blurLimitValueToStartFadeOut should always be less than _blurLimitTargetValue
	// (control it in editor)
	[Export(PropertyHint.Range, "0.0,0.5")] private float _blurLimitTargetValue = 0.5f;
	
	// TODO: add setter: _blurValueToStartFadeOut should always be less than _blurTargetValue
	// (control it in editor)
	[Export(PropertyHint.Range, "0.0,10.0")] private float _blurTargetValue = 7.0f;
	
	[ExportSubgroup("Other")]
	[Export(PropertyHint.Range, "0.0,4.0")] private float _screenDarknessToReloadScene = 1.74f;

	private bool _deathAnimationPlayed;
	private float _screenDarknessOnDeath;
	private float _currentBlurLimit;
	private float _currentBlur;
	private float _currentScreenDarkness;
	
	private bool _dead;
	private Node3D _head;
	private AnimationPlayer _animationPlayer;
	private ShaderMaterial _blurMaterial;

	public struct HealthSystemInitParams
	{
		public Gravity Gravity;
		public CharacterBody3D Parent;
		public Camera3D Camera;
		public AnimationPlayer AnimationPlayer;
		public Node3D Head;
		public ColorRect VignetteRect;
		public ColorRect DistortionRect;
		public ColorRect BlurRect;
	}
	
	public void Init(HealthSystemInitParams initParams)
	{
		_currentHealth = _maxHealth;
		_currentHealthInPrevFrame = _currentHealth;
		_currentMultiplierMidValue = InitialMultiplierMidVal;
		
		_currentSpeed = _speedMin;

		_gravity = initParams.Gravity;
		_characterBody3D = initParams.Parent;
		_camera = initParams.Camera;
		
		_head = initParams.Head;

		_vignetteMaterial = initParams.VignetteRect.Material as ShaderMaterial;
		_distortionMaterial = initParams.DistortionRect.Material as ShaderMaterial;
		_blurMaterial = initParams.BlurRect.Material as ShaderMaterial;
		
		// Resetting shaders' parameters
		
		_vignetteMaterial.SetShaderParameter(Constants.VIGNETTE_SHADER_MULTIPLIER, 1.0f);	
		_vignetteMaterial.SetShaderParameter(Constants.VIGNETTE_SHADER_SOFTNESS, 1.0f);	
		
		_distortionMaterial.SetShaderParameter(Constants.DISTORTION_SHADER_SCREEN_DARKNESS, 0.0f);
		_distortionMaterial.SetShaderParameter(Constants.DISTORTION_SHADER_DARKNESS_PROGRESSION, 0.0f);
		
		_distortionMaterial.SetShaderParameter(
			Constants.DISTORTION_SHADER_UV_OFFSET, new Vector2(0.0f, 0.0f));

		_distortionMaterial.SetShaderParameter(Constants.DISTORTION_SHADER_SIZE, 0.0);
		
		_blurMaterial.SetShaderParameter(Constants.BLUR_SHADER_LIMIT, 0.0f);
		_blurMaterial.SetShaderParameter(Constants.BLUR_SHADER_BLUR, 0.0f);
		
		_animationPlayer = initParams.AnimationPlayer;
	}
	
	public override void _Process(double delta)
	{
		float deltaConverted = (float)delta;

		HandleDeath(deltaConverted);
		
		HandleVignetteShader(deltaConverted);
		HandleDistortionShader(deltaConverted);
		
		HandleCameraRotationOnHit(deltaConverted);
		HandleDamageOnFall();

		HandleHealthRegeneration(deltaConverted);
	}
	
	public void TakeDamage(float amount)
	{
		if (_dead)
		{
			return;
		}
		
		if (_cameraRotation == Rotation.NoRotation)
		{
			_cameraRotation = Rotation.CameraRotationTriggered;
		}

		_currentHealth -= amount;
		_currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
		
		if (_currentHealth == 0)
		{
			_dead = true;
			return;
		}

		_lastHitTime = DateTime.UtcNow;
	}

	public float GetCurrentHealth() { return _currentHealth; }
	
#if DEBUG
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
			
			if (eventKey.Pressed && eventKey.Keycode == Key.H)
				TakeDamage(_minimalDamageUnit);
	}
#endif

	public bool IsDead() { return _dead; }

	private void HandleDeath(float delta)
	{
		if (!_dead) { return; }

		if (!_deathAnimationPlayed)
		{
			_animationPlayer.PlayCameraRotationOnDeath();
			_deathAnimationPlayed = true;
		}
		
		Vector3 newPosition = _head.Position;
		newPosition.Y = Mathf.Lerp(newPosition.Y, _cameraHeightOnDeath, _cameraDropSpeedOnDeath * delta);
		
		if (newPosition.Y < _cameraHeightOnDeath) { newPosition.Y = _cameraHeightOnDeath; }
		
		_head.Position = newPosition;
		
		_currentBlurLimit = CustomMath.Lerp(
			_currentBlurLimit, _blurLimitTargetValue, _blurLimitSpeedOnDeath * delta);
		
		_blurMaterial.SetShaderParameter(Constants.BLUR_SHADER_LIMIT, _currentBlurLimit);
		
		_currentBlur = CustomMath.Lerp(_currentBlur, _blurTargetValue, _blurSpeedOnDeath * delta);
		_blurMaterial.SetShaderParameter(Constants.BLUR_SHADER_BLUR, _currentBlur);	
		
		if (_currentBlurLimit >= _blurLimitValueToStartFadeOut && _currentBlur >= _blurValueToStartFadeOut)
		{
			float currentScreenDarknessVariant = (float)_distortionMaterial.GetShaderParameter(
				Constants.DISTORTION_SHADER_SCREEN_DARKNESS);
		
			_screenDarknessOnDeath = Mathf.Lerp(
				currentScreenDarknessVariant, _fadeOutTargetValue, _fadeOutSpeed * delta);
			
			_distortionMaterial.SetShaderParameter(
				Constants.DISTORTION_SHADER_SCREEN_DARKNESS, _screenDarknessOnDeath);
			
			if (_screenDarknessOnDeath >=_screenDarknessToReloadScene)
			{
				GD.Print("reload");
				// Reload the current scene
				GetTree().ReloadCurrentScene();
			}
		}
	}

	private void HandleVignetteShader(float delta)
	{
		if (CustomMath.AreAlmostEqual(_currentHealth, _maxHealth))
		{
			_currentHealthInPrevFrame = _currentHealth;
			_currentMultiplierMidValue = InitialMultiplierMidVal;
			_timeAccumulator = 0;
			return;
		}
		
		float healthNormalized = _currentHealth / _maxHealth;
		float healthReverted = 1 - healthNormalized;
		
		float newAnimationSpeed = CustomMath.Lerp(_speedMin, _speedMax, healthReverted);
		_currentSpeed = CustomMath.Lerp(_currentSpeed, newAnimationSpeed, delta);
		
		float completeSinCycle = Mathf.Tau / _currentSpeed;

		_timeAccumulator += delta;
		
		if (CustomMath.AreAlmostEqual(completeSinCycle, _timeAccumulator))
		{
			_timeAccumulator = 0;
		}

		float rawAnimationWeight = Mathf.Sin(_timeAccumulator * _currentSpeed);

		float animationWeight = Mathf.Abs(rawAnimationWeight);

		float difference = _currentHealthInPrevFrame - _currentHealth;

		float newMultiplierMidValue;	
		
		if (difference < 0)
		{
			newMultiplierMidValue = CustomMath.Lerp(
				MultiplierMidValToHideVignette, _activeZoneMultiplierMin, healthReverted);	
		} else
		{
			newMultiplierMidValue = CustomMath.Lerp(
				_activeZoneMultiplierMax, _activeZoneMultiplierMin, healthReverted);
		}
		
		_currentMultiplierMidValue = CustomMath.Lerp(
			_currentMultiplierMidValue, newMultiplierMidValue,  delta);
			
		float multiplier = CustomMath.Lerp(_currentMultiplierMidValue + _multiplierDeltaForAnimation,
			_currentMultiplierMidValue - _multiplierDeltaForAnimation,  
			animationWeight * animationWeight);

		
		_vignetteMaterial.SetShaderParameter(Constants.VIGNETTE_SHADER_MULTIPLIER, multiplier);
		_vignetteMaterial.SetShaderParameter(Constants.VIGNETTE_SHADER_SOFTNESS, _softness);

		_currentHealthInPrevFrame = _currentHealth;
	}

	private void HandleDistortionShader(float delta)
	{
		if (CustomMath.AreAlmostEqual(_currentHealth, _maxHealth))
		{
			return;
		}

		float healthNormalized = _currentHealth / _maxHealth;
		float healthReverted = 1 - healthNormalized;
		
		_distortionMaterial.SetShaderParameter(
			Constants.DISTORTION_SHADER_DARKNESS_PROGRESSION, healthReverted);

		if (!_dead)
		{
			float screenDarkness = Mathf.Remap(
				healthReverted, 0, 1, _screenDarknessMin, _screenDarknessMax);
		
			_distortionMaterial.SetShaderParameter(
				Constants.DISTORTION_SHADER_SCREEN_DARKNESS, screenDarkness);
		}
		
		float distortionSpeed =  Mathf.Remap(
			healthReverted, 0.0f, 1.0f, _distortionSpeedMin, _distortionSpeedMax);

		float offsetVal = delta * distortionSpeed;
		
		_uvOffset += new Vector2(offsetVal, offsetVal);
		
		_distortionMaterial.SetShaderParameter(
			Constants.DISTORTION_SHADER_UV_OFFSET, _uvOffset);
		
		if (_uvOffset.X > _offsetResetThreshold) { _uvOffset.X = 0.0f; _uvOffset.Y = 0.0f; }
		
		float distortionSize = Mathf.Remap(
			healthReverted, 0.0f, 1.0f, _distortionSizeMin, _distortionSizeMax);
		
		_distortionMaterial.SetShaderParameter(
			Constants.DISTORTION_SHADER_SIZE, distortionSize);
	}

	private void RotateCameraOnZAxis(float delta, float targetAngleInRadians, Rotation rotationStateToSetOnFinish)
	{
		_progressOnCamRotation += delta * _rotationSpeed;
		_progressOnCamRotation = Mathf.Clamp(_progressOnCamRotation, 0f, 1f);
			
		float lerpedAngleZ = Mathf.LerpAngle(
			_camera.Rotation.Z,targetAngleInRadians, _progressOnCamRotation);
			
		_camera.Rotation = new Vector3(_camera.Rotation.X, _camera.Rotation.Y, lerpedAngleZ);
		
		float difference = Mathf.Abs(targetAngleInRadians - _camera.Rotation.Z);
		
		if (difference < Constants.ACCEPTABLE_TOLERANCE)
		{
			_cameraRotation = rotationStateToSetOnFinish;
			_progressOnCamRotation = 0;
		} 
	}
	
	private void HandleCameraRotationOnHit(float delta)
	{
		if (_cameraRotation == Rotation.NoRotation || _dead) return;

		if (_cameraRotation == Rotation.CameraRotationTriggered)
		{
			float randomVal = GameManager.Instance.GetRandomFloatBetween0And1();
			
			if (randomVal < 0.5f)
			{
				_targetRotationZAxis = Mathf.DegToRad(_rotationDegree * -1);
			}
			else
			{
				_targetRotationZAxis = Mathf.DegToRad(_rotationDegree);
			}

			_cameraRotation = Rotation.RotatingOnZAxis;
		}

		if (_cameraRotation == Rotation.RotatingOnZAxis)
		{
			RotateCameraOnZAxis(delta, _targetRotationZAxis, Rotation.ReturningBack);
		}

		if (_cameraRotation == Rotation.ReturningBack)
		{
			RotateCameraOnZAxis(delta, 0, Rotation.NoRotation);
		}
	}

	private void HandleDamageOnFall()
	{
		if (_dead) { return;}
		
		if (!_characterBody3D.IsOnFloor())
		{
			_currentVelocityYInAir = _characterBody3D.Velocity.Y;
		}
		else
		{
			if (_currentVelocityYInAir < _thresholdVelYForDamage)
			{
				float hit = Mathf.Remap(_currentVelocityYInAir,
					_thresholdVelYForDamage, _thresholdVelYForDamage - 9.0f, 
					_minimalDamageUnit, _maxHealth);
				
				GD.Print("Hit damage: ", hit);
				
				TakeDamage(hit);
			}

			_currentVelocityYInAir = 0.0f;
		}
	}

	private DateTime? _lastHitTime;

	private void HandleHealthRegeneration(float delta)
	{
		if (_lastHitTime == null || _dead) return;
		
		DateTime lastHitTimeConverted = (DateTime)_lastHitTime;
		
		double differenceInSeconds = (DateTime.UtcNow - lastHitTimeConverted).TotalSeconds;
		float differenceInSecondsConverted = (float)differenceInSeconds;
		
		if (differenceInSecondsConverted < _timeInSecondsBeforeRegeneration)
		{
			return;
		}
		
		if (CustomMath.AreAlmostEqual(_currentHealth, _maxHealth))
		{
			_currentHealth = _maxHealth;
			_lastHitTime = null;
			return;
		}

		_currentHealth += delta * _regenerationSpeed;
		_currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
	}
}
