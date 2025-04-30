using System;
using Godot;

namespace PolarBears.PlayerControllerAddon;

public partial class HealthSystem : Node3D
{
	new enum Rotation
	{
		NoRotation = 0,
		CameraRotationTriggered = 1,
		RotatingOnZAxis = 2,
		ReturningBack = 3,
	}
	
	[ExportGroup("Health Metrics")]
	[ExportSubgroup("Amounts")]
	[Export(PropertyHint.Range, "0,100,,or_greater")]
	public float MaxHealth { get; set; } = 100.0f;
	[Export(PropertyHint.Range, "0,100,,or_greater")]
	public float CurrentHealth { get; set; } = 100.0f;
	[Export(PropertyHint.Range, "0,100,,or_greater")]
	public float MinimalDamageUnit { get; set; } = 25.0f;
	[ExportSubgroup("Regeneration")]
	[Export(PropertyHint.Range, "0,10,,suffix:s,or_greater")]
	public float SecondsBeforeRegeneration { get; set; } = 5.5f;
	[Export(PropertyHint.Range, "0,10,,or_greater")]
	public float RegenerationSpeed { get; set; } = 10.0f;

	[ExportGroup("Damage Camera Effects")]
	[ExportSubgroup("Camera Shake")]
	[Export(PropertyHint.Range, "0,1,")]
	public float RotationSpeed  { get; set; } = 9.0f;
	[Export(PropertyHint.Range, "0,180,,degrees")]
	public float RotationDegree { get; set; } = 14.0f;
	[ExportSubgroup("Visual Distortion")]
	// Screen darkness controls how dark the screen will be, where 0.0 - natural
	// color of the screen(unaltered) and 1.0 - black screen
	[Export(PropertyHint.Range, "0.0,1.0,")]
	public float ScreenDarknessMin { get; set; } = 0.0f;
	[Export(PropertyHint.Range, "0.0,1.0,")]
	public float ScreenDarknessMax { get; set; } = 0.3f;
	[Export(PropertyHint.Range, "0.0,1.0,")]
	public float DistortionSpeedMin { get; set; } = 0.0f;
	[Export(PropertyHint.Range, "0.0,1.0,")]
	public float DistortionSpeedMax { get; set; } = 0.6f;
	[Export(PropertyHint.Range, "0.0,1.0,")]
	public float DistortionSizeMin { get; set; } = 0.0f;
	[Export(PropertyHint.Range, "0.0,1.0,")]
	public float DistortionSizeMax { get; set; } = 1.0f;
	[ExportSubgroup("Vignetting")]
	// NOTE: These limits seem arbitrary, perhaps [0, 1] would be better?
	[Export(PropertyHint.Range, "0.0,0.5,")]
	public float ActiveZoneMultiplierMin { get; set; } = 0.45f;
	[Export(PropertyHint.Range, "0.0,0.6,")]
	// NOTE: what value should this default to? It is never set.
	public float ActiveZoneMultiplierMax { get; set; } = 0.475f;
	[Export(PropertyHint.Range, "0.0,1.0,,or_greater")]
	public float MultiplierDeltaForAnimation { get; set; } = 0.066f;
	[Export(PropertyHint.Range, "0.0,1.0,")]
	public float Softness { get; set; } = 1.0f;
	// NOTE: is this referring to animation speed??
	[Export] public float SpeedMin { get; set; } = 2.95f;
	[Export] public float SpeedMax { get; set; } = 4.0f;

	// Death / GameOver
	[ExportGroup("Death")]
	[ExportSubgroup("Before Fade Out")]
	[Export(PropertyHint.Range, "0.2,0.5,,or_less,or_greater")]
	public float BlurLimitValueToStartFadeOut { get; set; } = 0.3f;
	[Export(PropertyHint.Range, "0.0,4.0,,or_greater")]
	public float BlurValueToStartFadeOut { get; set; } = 3.376f;
	[ExportSubgroup("Speeds")]
	[Export(PropertyHint.Range, "0.1,20.0,,or_greater")]
	public float CameraDropSpeedOnDeath { get; set; } = 18.0f;
	[Export(PropertyHint.Range, "0.05,5.0,,or_greater")]
	public float FadeOutSpeed { get; set; } = 0.11f;

	[Export(PropertyHint.Range, "0.1,10,,or_greater")]
	public float BlurLimitSpeedOnDeath { get; set; } = 0.9f;
	[Export(PropertyHint.Range, "0.1,10,,or_greater")]
	public float BlurSpeedOnDeath { get; set; } = 1.5f;

	[ExportSubgroup("Target values")]
	[Export(PropertyHint.Range, "0,5.0,,suffix:m,or_greater")]
	public float CameraHeightOnDeath { get; set; } = 0.68f;
	[Export(PropertyHint.Range, "0,5.0,,suffix:m,or_greater")]
	public float FadeOutTargetValue { get; set; } = 4.0f;

	// TODO: add setter: BlurLimitValueToStartFadeOut should always be less than BlurLimitTargetValue
	// (control it in editor)
	[Export(PropertyHint.Range, "0,10,,or_greater")]
	public float BlurLimitTargetValue { get; set; } = 0.5f;

	// TODO: add setter: BlurValueToStartFadeOut should always be less than BlurTargetValue
	// (control it in editor)
	[Export(PropertyHint.Range, "0,10,,or_greater")]
	public float BlurTargetValue { get; set; } = 7.0f;

	[ExportSubgroup("Other")]
	[Export(PropertyHint.Range, "0.0,4.0,,suffix:s,or_greater")]
	public float ScreenDarknessToReloadScene { get; set; } = 1.74f;


	// Required to hide Vignette effect
	private float _currentHealthInPrevFrame;

	private float _thresholdVelYForDamage = -15.0f;

	private float _currentVelocityYInAir;
	private Gravity _gravity;

	private CharacterBody3D _characterBody3D;

	private Camera3D _camera;
	private float _cameraInitialRotationZ;
	private float _targetRotationZAxis;
	private Rotation _cameraRotation = Rotation.NoRotation;
	private float _progressOnCamRotation;

	private Vector2 _uvOffset = Vector2.Zero;
	private float _offsetResetThreshold = 5.0f;

	private ShaderMaterial _distortionMaterial;

	private float _timeAccumulator;

	private float _currentSpeed;

	private const float InitialMultiplierMidVal = 0.6f;
	private const float MultiplierMidValToHideVignette = 0.8f;
	private float _currentMultiplierMidValue;

	private ShaderMaterial _vignetteMaterial;

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
		_currentHealthInPrevFrame = CurrentHealth;
		_currentMultiplierMidValue = InitialMultiplierMidVal;
		
		_currentSpeed = SpeedMin;

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

		CurrentHealth -= amount;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
		
		if (CurrentHealth == 0)
		{
			_dead = true;
			return;
		}

		_lastHitTime = DateTime.UtcNow;
	}

	public float GetCurrentHealth() { return CurrentHealth; }
	
#if DEBUG
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
			
			if (eventKey.Pressed && eventKey.Keycode == Key.H)
				TakeDamage(MinimalDamageUnit);
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
		newPosition.Y = Mathf.Lerp(newPosition.Y, CameraHeightOnDeath, CameraDropSpeedOnDeath * delta);
		
		if (newPosition.Y < CameraHeightOnDeath) { newPosition.Y = CameraHeightOnDeath; }
		
		_head.Position = newPosition;
		
		_currentBlurLimit = Mathf.Lerp(
			_currentBlurLimit, BlurLimitTargetValue, BlurLimitSpeedOnDeath * delta);
		
		_blurMaterial.SetShaderParameter(Constants.BLUR_SHADER_LIMIT, _currentBlurLimit);
		
		_currentBlur = Mathf.Lerp(_currentBlur, BlurTargetValue, BlurSpeedOnDeath * delta);
		_blurMaterial.SetShaderParameter(Constants.BLUR_SHADER_BLUR, _currentBlur);	
		
		if (_currentBlurLimit >= BlurLimitValueToStartFadeOut && _currentBlur >= BlurValueToStartFadeOut)
		{
			float currentScreenDarknessVariant = (float)_distortionMaterial.GetShaderParameter(
				Constants.DISTORTION_SHADER_SCREEN_DARKNESS);
		
			_screenDarknessOnDeath = Mathf.Lerp(
				currentScreenDarknessVariant, FadeOutTargetValue, FadeOutSpeed * delta);
			
			_distortionMaterial.SetShaderParameter(
				Constants.DISTORTION_SHADER_SCREEN_DARKNESS, _screenDarknessOnDeath);
			
			if (_screenDarknessOnDeath >= ScreenDarknessToReloadScene)
			{
				GD.Print("reload");
				// Reload the current scene
				GetTree().ReloadCurrentScene();
			}
		}
	}

	private void HandleVignetteShader(float delta)
	{
		if (Mathf.IsEqualApprox(CurrentHealth, MaxHealth))
		{
			_currentHealthInPrevFrame = CurrentHealth;
			_currentMultiplierMidValue = InitialMultiplierMidVal;
			_timeAccumulator = 0;
			return;
		}
		
		float healthNormalized = CurrentHealth / MaxHealth;
		float healthReverted = 1 - healthNormalized;
		
		float newAnimationSpeed = Mathf.Lerp(SpeedMin, SpeedMax, healthReverted);
		_currentSpeed = Mathf.Lerp(_currentSpeed, newAnimationSpeed, delta);
		
		float completeSinCycle = Mathf.Tau / _currentSpeed;

		_timeAccumulator = Mathf.Wrap(_timeAccumulator + delta, 0.0f, Mathf.Tau);

		float rawAnimationWeight = Mathf.Sin(_timeAccumulator * _currentSpeed);

		float animationWeight = Mathf.Abs(rawAnimationWeight);

		float difference = _currentHealthInPrevFrame - CurrentHealth;

		float newMultiplierMidValue;	
		
		if (difference < 0)
		{
			newMultiplierMidValue = Mathf.Lerp(
				MultiplierMidValToHideVignette, ActiveZoneMultiplierMin, healthReverted);
		} else
		{
			newMultiplierMidValue = Mathf.Lerp(
				ActiveZoneMultiplierMax, ActiveZoneMultiplierMin, healthReverted);
		}
		
		_currentMultiplierMidValue = Mathf.Lerp(
			_currentMultiplierMidValue, newMultiplierMidValue,  delta);
			
		float multiplier = Mathf.Lerp(_currentMultiplierMidValue + MultiplierDeltaForAnimation,
			_currentMultiplierMidValue - MultiplierDeltaForAnimation,
			animationWeight * animationWeight);

		
		_vignetteMaterial.SetShaderParameter(Constants.VIGNETTE_SHADER_MULTIPLIER, multiplier);
		_vignetteMaterial.SetShaderParameter(Constants.VIGNETTE_SHADER_SOFTNESS, Softness);

		_currentHealthInPrevFrame = CurrentHealth;
	}

	private void HandleDistortionShader(float delta)
	{
		if (Mathf.IsEqualApprox(CurrentHealth, MaxHealth))
		{
			return;
		}

		float healthNormalized = CurrentHealth / MaxHealth;
		float healthReverted = 1 - healthNormalized;
		
		_distortionMaterial.SetShaderParameter(
			Constants.DISTORTION_SHADER_DARKNESS_PROGRESSION, healthReverted);

		if (!_dead)
		{
			float screenDarkness = Mathf.Remap(
				healthReverted, 0, 1, ScreenDarknessMin, ScreenDarknessMax);
		
			_distortionMaterial.SetShaderParameter(
				Constants.DISTORTION_SHADER_SCREEN_DARKNESS, screenDarkness);
		}
		
		float distortionSpeed =  Mathf.Remap(
			healthReverted, 0.0f, 1.0f, DistortionSpeedMin, DistortionSpeedMax);

		float offsetVal = delta * distortionSpeed;
		
		_uvOffset += new Vector2(offsetVal, offsetVal);
		
		_distortionMaterial.SetShaderParameter(
			Constants.DISTORTION_SHADER_UV_OFFSET, _uvOffset);
		
		if (_uvOffset.X > _offsetResetThreshold) { _uvOffset.X = 0.0f; _uvOffset.Y = 0.0f; }
		
		float distortionSize = Mathf.Remap(
			healthReverted, 0.0f, 1.0f, DistortionSizeMin, DistortionSizeMax);
		
		_distortionMaterial.SetShaderParameter(
			Constants.DISTORTION_SHADER_SIZE, distortionSize);
	}

	private void RotateCameraOnZAxis(float delta, float targetAngleInRadians, Rotation rotationStateToSetOnFinish)
	{
		_progressOnCamRotation += delta * RotationSpeed;
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
			if (GD.Randi() % 2 == 0)
			{
				_targetRotationZAxis = Mathf.DegToRad(RotationDegree * -1);
			}
			else
			{
				_targetRotationZAxis = Mathf.DegToRad(RotationDegree);
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
					MinimalDamageUnit, MaxHealth);
				
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
		
		if (differenceInSecondsConverted < SecondsBeforeRegeneration)
		{
			return;
		}
		
		if (Mathf.IsEqualApprox(CurrentHealth, MaxHealth))
		{
			CurrentHealth = MaxHealth;
			_lastHitTime = null;
			return;
		}

		CurrentHealth += delta * RegenerationSpeed;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
	}
}
