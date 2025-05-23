using Godot;

namespace PolarBears.PlayerControllerAddon;

public partial class StairsSystem: Node3D
{
	[Export(PropertyHint.Range, "0,10,0.01,suffix:m,or_greater")]
	public float MaxStepHeight = 0.5f;

	private RayCast3D _stairsBelowRayCast3D;
	private RayCast3D _stairsAheadRayCast3D;

	private Node3D _cameraSmooth;

	private bool _snappedToStairsLastFrame;

	private Vector3? _savedCameraGlobalPos;

	public void Init(RayCast3D stairsBelowRayCast3D, RayCast3D stairsAheadRayCast3D, Node3D cameraSmooth)
	{
		_stairsBelowRayCast3D = stairsBelowRayCast3D;
		_stairsAheadRayCast3D = stairsAheadRayCast3D;
		_cameraSmooth = cameraSmooth;

	}

	public bool WasSnappedToStairsLastFrame() { return _snappedToStairsLastFrame; }

	private bool RunBodyTestMotion(
		Transform3D from, Vector3 motion, Rid rid, out PhysicsTestMotionResult3D resultOut)
	{
		PhysicsTestMotionResult3D result = new PhysicsTestMotionResult3D();

		resultOut = result;

		PhysicsTestMotionParameters3D parameters = new PhysicsTestMotionParameters3D
		{
			From = from,
			Motion = motion,
			Margin = 0.00001f
		};

		return PhysicsServer3D.BodyTestMotion(rid, parameters, result);
	}


	public struct UpStairsCheckParams
	{
		public bool IsOnFloorCustom;
		public bool IsCapsuleHeightLessThanNormal;
		public bool CurrentSpeedGreaterThanWalkSpeed;
		public bool IsCrouchingHeight;
		public float Delta;
		public float FloorMaxAngle;
		public Vector3 GlobalPositionFromDriver;
		public Vector3 Velocity;
		public Transform3D GlobalTransformFromDriver;
		public Rid Rid;
	}

	public delegate void UpdateAfterUpStairsCheck(CharacterBody3D cb3D);
	public struct UpStairsCheckResult
	{
		public bool UpdateRequired;
		public UpdateAfterUpStairsCheck Update;
	}

	public UpStairsCheckResult SnapUpStairsCheck(UpStairsCheckParams parameters)
	{
		UpStairsCheckResult updateIsNotRequired = new UpStairsCheckResult
		{
			UpdateRequired = false,
			Update = (CharacterBody3D cb3D) => { }
		};

		if (!parameters.IsOnFloorCustom) { return updateIsNotRequired; }

		// Different velocity multipliers are set for different situations because we alter player's speed
		// depending on those situations. For example, while crouching, player is moving slower, so we should account
		// for this while running body test motion, as the velocity of the player becomes lower. When sprinting,
		// player will have higher velocity and we can compensate it by setting lower velocity multiplier.

		float motionVelocityMultiplier;

		if (parameters.IsCapsuleHeightLessThanNormal)
		{
			motionVelocityMultiplier = 1.55f;  // Going to crouch mode
		}else if (parameters.CurrentSpeedGreaterThanWalkSpeed)
		{
			motionVelocityMultiplier = 1.1f;  // Sprinting
		}
		else
		{
			motionVelocityMultiplier = 1.4f;  // Walking
		}

		Vector3 expectedMoveMotion = parameters.Velocity *
									 new Vector3(motionVelocityMultiplier, 0.0f, motionVelocityMultiplier) *
									 parameters.Delta;

		Vector3 offset = expectedMoveMotion + new Vector3(0, MaxStepHeight * 2.0f, 0);

		Transform3D stepPosWithClearance = parameters.GlobalTransformFromDriver.Translated(offset);

		Vector3 motion = new Vector3(0, -MaxStepHeight * 2.0f, 0);
		bool doesProjectionCollide = RunBodyTestMotion(
			stepPosWithClearance, motion, parameters.Rid, out var downCheckResult);

		if (doesProjectionCollide)
		{
			GodotObject collider = downCheckResult.GetCollider();

			if (!collider.IsClass("StaticBody3D") && !collider.IsClass("CSGShape3D"))
			{
				return updateIsNotRequired;
			}

			// We add 0.5 because when player is crouching, his height is less than normal by the factor of 2:
			// so, 2 meters / 2 = 1 meter (Crouching height). In Godot, this is achieved by subtracting 0.5 from the
			// top and the bottom of capsule collider shape. As a result, GlobalPosition goes under the ground by 0.5
			// because capsule shape collider was cut off by 0.5 from the bottom, and GlobalPosition does not give a
			// a damn about physics (it's not the GlobalPosition of capsule shape, it's just the global coordinate of a
			// point in space). So, MaxHeight is 0.5 - 0.5(GlobalPosition that went under the ground) = 0. It means that
			// MaxStepHeight for the player while he is crouching is 0, so he can't overcome obstacles while crouching.
			// In order to address this problem we should add some offset to 0. Basically, the average height of
			// stairs is 0.23. But, also, we want to climb automatically to obstacles that are 50cm height. So, we add
			// 0.5 offset. And it means, that we balanced the max step height while crouching: when capsule height is
			// normal then max step height is 0.5, when player is crouching, then max step height is also 0.5.

			float maxStepHeightAdjusted = parameters.IsCrouchingHeight ? MaxStepHeight + 0.5f : MaxStepHeight;

			Vector3 stepHeight = stepPosWithClearance.Origin + downCheckResult.GetTravel() - parameters.GlobalPositionFromDriver;
			float stepHeightYToTravelEnd = stepHeight.Y;

			float realStepHeightY = (downCheckResult.GetCollisionPoint() - parameters.GlobalPositionFromDriver).Y;

			if (stepHeightYToTravelEnd <= 0.01 || realStepHeightY > maxStepHeightAdjusted)
			{
				return updateIsNotRequired;
			}

			_stairsAheadRayCast3D.GlobalPosition = downCheckResult.GetCollisionPoint() + new Vector3(
				0, MaxStepHeight, 0) + expectedMoveMotion.Normalized() * 0.1f;

			_stairsAheadRayCast3D.ForceRaycastUpdate();

			// It's needed in order to deny too steep angles of climbing. For hills. And hills-like bumps
			// For casual stairs it's, of course, will pass
			if (!IsSurfaceTooSteep(
					_stairsAheadRayCast3D.GetCollisionNormal(), parameters.FloorMaxAngle))
			{

				return new UpStairsCheckResult
				{
					UpdateRequired = true,
					Update = (CharacterBody3D cb3D) =>
					{
						SaveCameraGlobalPosForSmoothing();

						cb3D.GlobalPosition = stepPosWithClearance.Origin + downCheckResult.GetTravel();
						cb3D.ApplyFloorSnap();

						_snappedToStairsLastFrame = true;
					}
				};
			}
		}

		return updateIsNotRequired;
	}

	public struct DownStairsCheckParams
	{
		public bool IsOnFloor;
		public bool IsCrouchingHeight;
		public float LastFrameWasOnFloor;
		public float CapsuleDefaultHeight;
		public float CurrentCapsuleHeight;
		public float FloorMaxAngle;
		public float VelocityY;
		public Transform3D GlobalTransformFromDriver;
		public Rid Rid;
	}

	public delegate void UpdateAfterDownStairsCheck(CharacterBody3D cb3D);

	public struct DownStairsCheckResult
	{
		public bool UpdateIsRequired;
		public UpdateAfterDownStairsCheck Update;

	}

	public DownStairsCheckResult SnapDownStairsCheck(DownStairsCheckParams parameters)
	{
		bool didSnap = false;

		if (parameters.IsCrouchingHeight)
		{
			float yCoordAdjustment = (parameters.CapsuleDefaultHeight - parameters.CurrentCapsuleHeight) / 2.0f;
			_stairsBelowRayCast3D.Position = new Vector3(0f, yCoordAdjustment, 0.0f);
		}
		else
		{
			_stairsBelowRayCast3D.Position = new Vector3(0f, 0.0f, 0.0f);
		}

		_stairsBelowRayCast3D.ForceRaycastUpdate();

		bool floorBelow = _stairsBelowRayCast3D.IsColliding() && !IsSurfaceTooSteep(
			_stairsBelowRayCast3D.GetCollisionNormal(), parameters.FloorMaxAngle);

		float differenceInPhysicalFrames = Engine.GetPhysicsFrames() - parameters.LastFrameWasOnFloor;

		bool wasOnFloorLastFrame = Mathf.IsEqualApprox(differenceInPhysicalFrames, 1.0f);

		PhysicsTestMotionResult3D bodyTestResult = new PhysicsTestMotionResult3D();

		if (!parameters.IsOnFloor && parameters.VelocityY <= 0 &&
			(wasOnFloorLastFrame || _snappedToStairsLastFrame) && floorBelow)
		{
			Vector3 motion = new Vector3(0, -MaxStepHeight, 0);

			bool doesProjectionCollide = RunBodyTestMotion(
				parameters.GlobalTransformFromDriver, motion, parameters.Rid, out bodyTestResult);

			if (doesProjectionCollide)
			{
				didSnap = true;
			}
		}

		_snappedToStairsLastFrame = didSnap;

		if (_snappedToStairsLastFrame)
		{
			return new DownStairsCheckResult
			{
				UpdateIsRequired = true,
				Update = (CharacterBody3D cb3D) =>
				{
					SaveCameraGlobalPosForSmoothing();

					float yDelta = bodyTestResult.GetTravel().Y;

					Vector3 positionForModification = cb3D.Position;

					positionForModification.Y += yDelta;

					cb3D.Position = positionForModification;

					cb3D.ApplyFloorSnap();
				}
			};
		}

		return new DownStairsCheckResult
		{
			UpdateIsRequired = false,
			Update = (CharacterBody3D cb3D) => { }
		};
	}

	private void SaveCameraGlobalPosForSmoothing()
	{
		if (_savedCameraGlobalPos == null)
		{
			_savedCameraGlobalPos = _cameraSmooth.GlobalPosition;
		}

	}

	public struct SlideCameraParams
	{
		public bool IsCapsuleHeightLessThanNormal;
		public bool CurrentSpeedGreaterThanWalkSpeed;
		public bool BetweenCrouchingAndNormalHeight;
		public float Delta;
	}

	private const float CrouchingLerpingWeight = 15;
	private const float WalkingLerpingWeight = 30;
	private const float SprintingLerpingWeight = 75;

	private const float DefaultLerpingWeight = 100;

	private float _lerpingWeight = DefaultLerpingWeight;

	private const float MaxCameraDelayDistance = 0.25f;

	public void SlideCameraSmoothBackToOrigin(SlideCameraParams parameters)
	{
		if (_savedCameraGlobalPos == null)
		{
			return;
		}

		Vector3 savedCameraGlobalPosConverted = (Vector3)_savedCameraGlobalPos;

		Vector3 globalPositionForModification = _cameraSmooth.GlobalPosition;
		globalPositionForModification.Y = savedCameraGlobalPosConverted.Y;
		_cameraSmooth.GlobalPosition = globalPositionForModification;

		Vector3 positionForModification = _cameraSmooth.Position;
		positionForModification.Y = Mathf.Clamp(
			_cameraSmooth.Position.Y, -MaxCameraDelayDistance, MaxCameraDelayDistance);
		_cameraSmooth.Position = positionForModification;



		if (parameters.IsCapsuleHeightLessThanNormal)
		{
			_lerpingWeight = CrouchingLerpingWeight;
		}else
		{
			if (parameters.CurrentSpeedGreaterThanWalkSpeed)
			{
				_lerpingWeight = SprintingLerpingWeight;
			}
			else
			{
				_lerpingWeight = WalkingLerpingWeight;
			}
		}

		// Smooth control, to smoothly go to crouching mode on stairs (if capsule height has default height initially)
		if (parameters.BetweenCrouchingAndNormalHeight)
		{
			_lerpingWeight = 150;
			positionForModification.Y = 0.05f;
		}

		positionForModification.Y = Mathf.Lerp(
			_cameraSmooth.Position.Y, 0.0f,  _lerpingWeight  * parameters.Delta);

		_cameraSmooth.Position = positionForModification;

		_savedCameraGlobalPos = _cameraSmooth.GlobalPosition;

		if (_cameraSmooth.Position.Y == 0)
		{
			_savedCameraGlobalPos = null;
		}
	}
	
	private bool IsSurfaceTooSteep(Vector3 normal, float floorMaxAngle)
	{
		return normal.AngleTo(Vector3.Up) > floorMaxAngle;
	}
}
