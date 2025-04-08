using Godot;

namespace Exodus.Scripts.Player.PlayerController;

public partial class Driver : CharacterBody3D
{
	[Export] private float WalkSpeed = 5.0f;
	[Export] private float SprintSpeed = 7.2f;

	[Export] private float CrouchSpeed = 2.5f;
	[Export] private float CrouchTransitionSpeed = 20.0f;

	private float _currentSpeed;

	private const float DecelerationSpeedFactorFloor = 15.0f;
	private const float DecelerationSpeedFactorAir = 7.0f;

	private float _lastFrameWasOnFloor = -Mathf.Inf;

	private Node3D _head;
	private const int NumOfHeadCollisionDetectors = 4;
	private RayCast3D[] _headCollisionDetectors;

	// Other Components
	private Bobbing _bobbing;
	private FieldOfView _fieldOfView;
	private Stamina _stamina;
	private StairsSystem _stairsSystem;
	private CapsuleCollider _capsuleCollider;
	private Gravity _gravity;
	private HealthSystem _healthSystem;
	private Mouse _mouse;

	public override void _Ready()
	{
		_currentSpeed = WalkSpeed;
		
		_head = GetNode<Node3D>("Head");
		
		_headCollisionDetectors = new RayCast3D[NumOfHeadCollisionDetectors];

		for (int i = 0; i < NumOfHeadCollisionDetectors; i++)
		{
			_headCollisionDetectors[i] = GetNode<RayCast3D>(
				"HeadCollisionDetectors/HeadCollisionDetector" + i);
		}

		// Getting dependencies of the components(In godot we manage this from upwards to downwards not vice versa)
		Camera3D camera = GetNode<Camera3D>("Head/CameraSmooth/Camera3D");

		RayCast3D stairsBelowRayCast3D = GetNode<RayCast3D>("StairsBelowRayCast3D");
		RayCast3D stairsAheadRayCast3D = GetNode<RayCast3D>("StairsAheadRayCast3D");

		Node3D cameraSmooth = GetNode<Node3D>("Head/CameraSmooth");

		CollisionShape3D collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		CapsuleShape3D playerCapsuleShape = collisionShape.Shape as CapsuleShape3D;

		AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		
		// Getting universal setting from GODOT editor to be in sync
		float gravitySetting = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

		ColorRect vignetteRect = GetNode<ColorRect>(
			"Head/CameraSmooth/Camera3D/CLVignette(Layer_1)/HealthVignetteRect");
		
		ColorRect distortionRect = GetNode<ColorRect>(
			"Head/CameraSmooth/Camera3D/CLDistortion(Layer_2)/HealthDistortionRect");

		ColorRect blurRect = GetNode<ColorRect>("Head/CameraSmooth/Camera3D/CLBlur(Layer_2)/BlurRect");
		
		Node3D mapNode = GetTree().Root.FindChild("Map", true, false) as Node3D;
		
		// Getting components

		_bobbing = GetNode<Bobbing>("Bobbing");
		_bobbing.Init(camera);

		_fieldOfView = GetNode<FieldOfView>("FieldOfView");
		_fieldOfView.Init(camera);

		_stamina = GetNode<Stamina>("Stamina");
		_stamina.SetSpeeds(WalkSpeed, SprintSpeed);

		_stairsSystem = GetNode<StairsSystem>("StairsSystem");
		_stairsSystem.Init(stairsBelowRayCast3D, stairsAheadRayCast3D, cameraSmooth);

		_capsuleCollider = GetNode<CapsuleCollider>("CapsuleCollider");
		_capsuleCollider.Init(playerCapsuleShape);

		_gravity = GetNode<Gravity>("Gravity");
		_gravity.Init(gravitySetting);

		_healthSystem = GetNode<HealthSystem>("HealthSystem");
		
		HealthSystem.HealthSystemInitParams healthSystemParams = new HealthSystem.HealthSystemInitParams()
		{
			Gravity = _gravity,
			Parent = this,
			Camera = camera,
			AnimationPlayer = animationPlayer,
			Head =  _head,
			VignetteRect = vignetteRect,
			DistortionRect = distortionRect,
			BlurRect = blurRect,
		};
		
		_healthSystem.Init(healthSystemParams);
		
		_mouse = GetNode<Mouse>("Mouse");
		_mouse.Init(_head, camera, _healthSystem.IsDead);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isOnFloorCustom())
		{
			_lastFrameWasOnFloor = Engine.GetPhysicsFrames();
		}

		// Adding the gravity
		if (!isOnFloorCustom())
		{
			Velocity = new Vector3(
				x: Velocity.X,
				y: Velocity.Y - (_gravity.CalculateGravityForce() * (float)delta),
				z: Velocity.Z);
		}

		bool doesCapsuleHaveCrouchingHeight = _capsuleCollider.IsCrouchingHeight();

		bool isPlayerDead = _healthSystem.IsDead();

		// Handle Jumping
		if (Input.IsActionJustPressed("jump") && isOnFloorCustom() 
				&& !doesCapsuleHaveCrouchingHeight && !isPlayerDead)
		{
			Velocity = new Vector3(
				x: Velocity.X,
				y: _gravity.CalculateJumpForce() * (float)delta,
				z: Velocity.Z);
		}
		
		bool isHeadTouchingCeiling = IsHeadTouchingCeiling();
		bool doesCapsuleHaveDefaultHeight = _capsuleCollider.IsDefaultHeight();

		// The code below is required to quickly adjust player's position on Y-axis when there's a ceiling on the
		// trajectory of player's jump and player is standing
		if (isHeadTouchingCeiling && doesCapsuleHaveDefaultHeight)
		{
			Velocity = new Vector3(
				x: Velocity.X,
				y: Velocity.Y - 2.0f,
				z: Velocity.Z);
		}

		if (!isPlayerDead)
		{
			
			// Used both for detecting the moment when we enter into crouching mode and the moment when we're already
			// in the crouching mode
			if (Input.IsActionPressed("crouch") ||
			    (doesCapsuleHaveCrouchingHeight && isHeadTouchingCeiling))
			{
				_capsuleCollider.PerformCrouching((float)delta, CrouchTransitionSpeed);
				_currentSpeed = CrouchSpeed;
			}
			// Used both for the moment when we exit the crouching mode and for the moment when we just walk
			else
			{
				_capsuleCollider.UndoCrouching((float)delta, CrouchTransitionSpeed);
				_currentSpeed = WalkSpeed;
			}
		}

		// Each component of the boolean statement for sprinting is required
		if (Input.IsActionPressed("sprint") && !isHeadTouchingCeiling && 
		    !doesCapsuleHaveCrouchingHeight && !isPlayerDead)
		{
			_currentSpeed = SprintSpeed;
		}

		// Get the input direction
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");

		// Basis is a 3x4 matrix. It contains information about scaling and rotation of head.
		// By multiplying our Vector3 by this matrix we're doing multiple things:
		// a) We start to operate in global space;
		// b) We're applying to Vector3 the current rotation of "head" object;
		// c) We're applying to Vector3 the current scaling of "head" object;
		Vector3 direction = (_head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (isPlayerDead)
		{
			direction = Vector3.Zero;
		}

		if (isOnFloorCustom())
		{
			// Set velocity based on input direction when on the floor
			if (direction.Length() > 0)
			{
				float availableSpeed = _stamina.AccountStamina(delta, _currentSpeed);

				float newX = direction.X * availableSpeed;
				float newZ = direction.Z * availableSpeed;

				Velocity = new Vector3(newX, Velocity.Y, newZ);
			}
			// If there is no input, smoothly decelerate the character on the floor
			else
			{
				float xDeceleration = Mathf.Lerp(Velocity.X, direction.X * _currentSpeed,
					(float)delta * DecelerationSpeedFactorFloor);
				float zDeceleration = Mathf.Lerp(Velocity.Z, direction.Z * _currentSpeed,
					(float)delta * DecelerationSpeedFactorFloor);

				Velocity = new Vector3(xDeceleration, Velocity.Y, zDeceleration);
			}
		}
		else
		{
			float xDeceleration = Mathf.Lerp(Velocity.X, direction.X * _currentSpeed,
				(float)delta * DecelerationSpeedFactorAir);
			float zDeceleration = Mathf.Lerp(Velocity.Z, direction.Z * _currentSpeed,
				(float)delta * DecelerationSpeedFactorAir);

			Velocity = new Vector3(xDeceleration, Velocity.Y, zDeceleration);
		}
		
		if (isPlayerDead)
		{
			MoveAndSlide();
			return;
		}
		
		Bobbing.CameraBobbingParams cameraBobbingParams = new Bobbing.CameraBobbingParams
		{
			Delta = (float)delta,
			IsOnFloorCustom = isOnFloorCustom(),
			Velocity = Velocity
		};
		
		_bobbing.PerformCameraBobbing(cameraBobbingParams);

		FieldOfView.FovParameters fovParams = new FieldOfView.FovParameters
		{
			IsCrouchingHeight = _capsuleCollider.IsCrouchingHeight(),
			Delta = (float)delta,
			SprintSpeed = SprintSpeed,
			Velocity = Velocity
		};
		
		_fieldOfView.PerformFovAdjustment(fovParams);

        StairsSystem.UpStairsCheckParams upStairsCheckParams = new StairsSystem.UpStairsCheckParams
        {
            IsOnFloorCustom = isOnFloorCustom(),
            IsCapsuleHeightLessThanNormal = _capsuleCollider.IsCapsuleHeightLessThanNormal(),
            CurrentSpeedGreaterThanWalkSpeed = _currentSpeed > WalkSpeed,
            IsCrouchingHeight = _capsuleCollider.IsCrouchingHeight(),
            Delta = (float)delta,
            FloorMaxAngle = FloorMaxAngle,
            GlobalPositionFromDriver = GlobalPosition,
            Velocity = Velocity,
            GlobalTransformFromDriver = GlobalTransform,
            Rid = GetRid()
        };
        
        // TODO: SnapUpStairsCheck influences the ability of player to crouch because of `stepHeightY <= 0.01` part
        // Ideally, it should not. SnapUpStairsCheck and SnapDownStairsCheck should be called, when player is actually
        // on the stairs

        StairsSystem.UpStairsCheckResult upStairsCheckResult = _stairsSystem.SnapUpStairsCheck(upStairsCheckParams);

        if (upStairsCheckResult.UpdateRequired)
        {
	        upStairsCheckResult.Update(this);
        }
        else
        {
            MoveAndSlide();
        
            StairsSystem.DownStairsCheckParams downStairsCheckParams = new StairsSystem.DownStairsCheckParams
            {
                IsOnFloor = IsOnFloor(),  // TODO: replace on IsOnFloor Custom
                IsCrouchingHeight = _capsuleCollider.IsCrouchingHeight(),
                LastFrameWasOnFloor = _lastFrameWasOnFloor,
                CapsuleDefaultHeight = _capsuleCollider.GetDefaultHeight(),
                CurrentCapsuleHeight = _capsuleCollider.GetCurrentHeight(),
                FloorMaxAngle = FloorMaxAngle,
                VelocityY = Velocity.Y,
                GlobalTransformFromDriver = GlobalTransform,
                Rid = GetRid()
            };
        
            StairsSystem.DownStairsCheckResult downStairsCheckResult = _stairsSystem.SnapDownStairsCheck(
	            downStairsCheckParams);

            if (downStairsCheckResult.UpdateIsRequired)
            {
	            downStairsCheckResult.Update(this);
            }
        }
        
        StairsSystem.SlideCameraParams slideCameraParams = new StairsSystem.SlideCameraParams
        {
            IsCapsuleHeightLessThanNormal = _capsuleCollider.IsCapsuleHeightLessThanNormal(),
            CurrentSpeedGreaterThanWalkSpeed = _currentSpeed > WalkSpeed, 
            BetweenCrouchingAndNormalHeight  = _capsuleCollider.BetweenCrouchingAndNormalHeight(),
            Delta = (float)delta
        };
        
        _stairsSystem.SlideCameraSmoothBackToOrigin(slideCameraParams);
    }

	private bool IsHeadTouchingCeiling()
	{
		for (int i = 0; i < NumOfHeadCollisionDetectors; i++)
		{
			if (_headCollisionDetectors[i].IsColliding())
			{
				return true;
			}
		}

		return false;
	}
     
    private bool isOnFloorCustom()
    {
        return IsOnFloor() || _stairsSystem.WasSnappedToStairsLastFrame();
    }
}