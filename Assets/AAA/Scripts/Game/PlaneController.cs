using System;
using DG.Tweening;
using UnityEngine;

public enum PlanePhysicsMode
{
	Drop,
	Carry,
	Circus,
	Cartoon,
}

[Serializable]
public class DropModeConfiguration
{
	public float TargetHorizontalSpeed = 6f;
	public float VerticalSpeedToLeanConversionFactor = 5f;
	public float LeaningSpeedSmoothingFactor = 0.15f;
	public AnimationCurve PassedTimeAfterLastPush;

	public float PushImpulse = 50f;
	public AnimationCurve TargetHeightOverTime;

	public int InitialCrateCount = 3;
}

[Serializable]
public class CarryModeConfiguration
{
	public float TargetHorizontalSpeed = 6f;
	public float VerticalSpeedToLeanConversionFactor = 5f;
	public float LeaningSpeedSmoothingFactor = 0.15f;
	public AnimationCurve PassedTimeAfterLastPush;
	public float PushForce = 30f;
}

[Serializable]
public class CircusModeConfiguration
{
	public AnimationCurve ForwardForceDegradationBySpeed;
	public float FullThrottleForwardForce = 1000f;
	public float FullThrottlePower = 1f;
	public float HalfThrottlePower = 0.5f;

	public float FullThrottleRotationTorque = 70f;
	public float HalfThrottleRotationTorque = -30f;

	public AnimationCurve LongitudinalSpeedToWingForceFactor = AnimationCurve.Linear(0f, 1f, 1f, 1f);
	public float WingForce;
}

[Serializable]
public class CartoonModeConfiguration
{
	public AnimationCurve ForwardForceDegradationBySpeed;
	public float FullThrottleForwardForce = 1000f;
	public float FullThrottlePower = 1f;
	public float HalfThrottlePower = 0.5f;

	public float FullThrottleRotationTorque = 70f;
	public float HalfThrottleRotationTorque = -30f;

	public AnimationCurve LongitudinalSpeedToWingForceFactor = AnimationCurve.Linear(0f, 1f, 1f, 1f);
	public float WingForce;
}

public class PlaneController : MonoBehaviour
{
	private float _fuel;

	#region Initialization

	protected void Awake()
	{
		InitializeEvents();
		InitializeAcrobacyParameters();
		InitializeExhaust();
		InitializePhysics();
	}

	#endregion

	#region Update

	protected void FixedUpdate()
	{
		CalculateInput();
		CalculatePhysics();
		CalculateAcrobacyEvents();
		_previousLookAngle = Rigidbody.rotation;
		_previousVelocity = Rigidbody.velocity;
		FixedUpdateExhaust();
		ExpendFuel();
	}

	protected void LateUpdate()
	{
		LateUpdatePropeller();
	}

	#endregion

	#region Links

	[Header("Links")]
	public Transform Transform;
	public Rigidbody2D Rigidbody;
	public Animator PropellerAnimator;
    public GameObject CrashBlast;

	#endregion

	#region Events

	private void InitializeEvents()
	{
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_LOAD, OnLevelLoad);
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_START, OnLevelStart);
		EventManager.Instance.StartListening(Constants.EVENT_ENABLE_CONTROLS, OnEnableControls);
		EventManager.Instance.StartListening(Constants.EVENT_GAIN_FUEL, (float value) => OnGainFuel(value));
		EventManager.Instance.StartListening(Constants.EVENT_SET_FUEL, (float value) => OnSetFuel(value));
	}

	private void InitializeAcrobacyParameters()
	{
		_previousLookAngle = Rigidbody.rotation;
		_previousVelocity = Rigidbody.velocity;
		_startingAngle = Rigidbody.rotation;
	}

	private void OnLevelLoad()
	{
		Debug.Log("Resetting plane");
		enabled = false;
		PlaceToSpawnLocation();
		ResetPhysics();
		ResetCrates();
		ResetCrash();
		ResetPilot();
		ResetExhaust();
		ResetEngine();
		DisableControls();
	}

	private void OnLevelStart()
	{
		enabled = true;
	}

	private void OnEnableControls()
	{
		EnableControls();
	}

	private void CalculateAcrobacyEvents()
	{
		
		var verticalStanceMinimumAngle = 65;
		var verticalStanceMaximumAngle = 100;

		var levelFlightMinimumAngle = -10;
		var levelFlightMaximumAngle = 10;

		var velocity = Rigidbody.velocity;
		var angle = Rigidbody.rotation;
		var angleClipped = angle % 360f;
		while (angleClipped > 180f)
		{
			angleClipped -= 360f;
		}
		var angularVelocity = Rigidbody.angularVelocity;
		var localVelocity = Rigidbody.GetRelativeVector(velocity);
		var speed = velocity.magnitude;

		_totalAngleCovered += angle - _previousLookAngle;
		var previousLookAngleClipped = _previousLookAngle % 360f;
		while (previousLookAngleClipped > 180f)
		{
			previousLookAngleClipped -= 360f;
		}

		//StartVerticalStance, //angle>=80 && angle <= 100
		if ((_previousLookAngle < verticalStanceMinimumAngle && angleClipped >= verticalStanceMinimumAngle) ||
		    _previousLookAngle > verticalStanceMaximumAngle && angleClipped <= verticalStanceMaximumAngle)
		{
			var ev = new AcrobacyEvent()
			{
				Position = Rigidbody.position,
				Time = Time.time,
				Type = AcrobacyEvent.AcrobacyEventType.StartVerticalStance
			};
			EventManager.Instance.TriggerEvent(Constants.EVENT_ACROBACY_START_VERTICAL_STANCE,ev);
		}

		//EndVerticalStance,//angle < 80 || angle <100
		if ((_previousLookAngle >= verticalStanceMinimumAngle && angleClipped < verticalStanceMinimumAngle) ||
		    _previousLookAngle <= verticalStanceMaximumAngle && angleClipped > verticalStanceMaximumAngle)
		{
			var ev = new AcrobacyEvent()
			{
				Position = Rigidbody.position,
				Time = Time.time,
				Type = AcrobacyEvent.AcrobacyEventType.EndVerticalStance
			};
			EventManager.Instance.TriggerEvent(Constants.EVENT_ACROBACY_END_VERTICAL_STANCE,ev);
		}

		//StartLevelFlight,//angle<10 && angle>=-10
		if ((previousLookAngleClipped < levelFlightMinimumAngle && angleClipped >= levelFlightMinimumAngle) ||
		    previousLookAngleClipped > levelFlightMaximumAngle && angleClipped <= levelFlightMaximumAngle)
		{
			var ev = new AcrobacyEvent()
			{
				Position = Rigidbody.position,
				Time = Time.time,
				Type = AcrobacyEvent.AcrobacyEventType.StartLevelFlight
			};
			EventManager.Instance.TriggerEvent(Constants.EVENT_ACROBACY_START_LEVEL_FLIGHT,ev);
		}

		//EndLevelFlight,//angle>10 || angle <-10
		if ((previousLookAngleClipped >= levelFlightMinimumAngle && angleClipped < levelFlightMinimumAngle) ||
		    previousLookAngleClipped <= levelFlightMaximumAngle && angleClipped > levelFlightMaximumAngle)
		{
			var ev = new AcrobacyEvent()
			{
				Position = Rigidbody.position,
				Time = Time.time,
				Type = AcrobacyEvent.AcrobacyEventType.EndLevelFlight
			};
			EventManager.Instance.TriggerEvent(Constants.EVENT_ACROBACY_END_LEVEL_FLIGHT,ev);
		}

		//Loop,
		if (angle > _startingAngle + (_loopCount+1) * 360f)
		{
			_loopCount++;
			var ev = new AcrobacyEvent()
			{
				Position = Rigidbody.position,
				Time = Time.time,
				Type = AcrobacyEvent.AcrobacyEventType.Loop
			};
			EventManager.Instance.TriggerEvent(Constants.EVENT_ACROBACY_COMPLETE_LOOP,ev);
			//EditorApplication.isPaused = true;
		}
		
		//StartFreeDescent,//that 30° downward free flight
		
		//HeadsDown,
		//TailContactsGround,
		//TailLeavesGround,
		//WheelTouchesGround,
		//WheelLeavesGround,
		//ReachedSpace,
	}

	#endregion

	#region Physics

	[Header("Configuration")]
	public PlanePhysicsMode Mode = PlanePhysicsMode.Drop;
	public float PlaneMass = 50f;
	public DropModeConfiguration DropConfiguration;
	public CarryModeConfiguration CarryConfiguration;
	public CircusModeConfiguration CircusConfiguration;
	public CartoonModeConfiguration CartoonConfiguration;

	// internal float CurrentVerticalSpeed;
	internal float CurrentHorizontalSpeed;
	internal float InitialHeight;

	private Vector2 _previousVelocity;
	private float _previousLookAngle;
	private float _totalAngleCovered;
	private float _startingAngle;
	private int _loopCount;

	private void InitializePhysics()
	{
		InitialHeight = Transform.position.y;
	}

	private void ResetPhysics()
	{
		switch (Mode)
		{
			case PlanePhysicsMode.Drop:
			{
				Rigidbody.mass = PlaneMass + DropConfiguration.InitialCrateCount * CrateMass;
				DOTween.To(() => CurrentHorizontalSpeed, x => CurrentHorizontalSpeed = x, DropConfiguration.TargetHorizontalSpeed, 2f);
				break;
			}

			case PlanePhysicsMode.Carry:
			{
				Rigidbody.mass = PlaneMass;
				DOTween.To(() => CurrentHorizontalSpeed, x => CurrentHorizontalSpeed = x, CarryConfiguration.TargetHorizontalSpeed, 2f);
				break;
			}

			case PlanePhysicsMode.Circus:
			{
				Rigidbody.mass = PlaneMass;
				break;
			}

			case PlanePhysicsMode.Cartoon:
			{
				Rigidbody.mass = PlaneMass;
				break;
			}

			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void CalculatePhysics()
	{
		switch (Mode)
		{
			case PlanePhysicsMode.Drop:
				CalculatePhysics_Drop();
				break;

			case PlanePhysicsMode.Carry:
				CalculatePhysics_Carry();
				break;

			case PlanePhysicsMode.Circus:
				CalculatePhysics_Circus();
				break;

			case PlanePhysicsMode.Cartoon:
				CalculatePhysics_Cartoon();
				break;

			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void CalculatePhysics_Drop()
	{
		var config = DropConfiguration;
		var velocity = Rigidbody.velocity;
		var angularVelocity = Rigidbody.angularVelocity;

		// Adjust horizontal speed.
		OverrideHorizontalSpeedToMatchTargetHorizontalSpeed(ref velocity);

		if (IsPushingDown)
		{
			if (CurrentCrateCount > 0)
			{
				CurrentCrateCount--;
				Rigidbody.mass -= CrateMass;
				Rigidbody.AddForce(new Vector2(0f, config.PushImpulse), ForceMode2D.Impulse);
				InstantiateCrate(velocity);
			}
		}

		// Apply wing force.
		ApplyWingForce(config.TargetHeightOverTime, ref velocity);

		// Leaning with vertical speed. Note that this forcefully overrides angular velocity.
		OverrideAngularSpeedToLeanTheNose(config.VerticalSpeedToLeanConversionFactor,
		                                  config.LeaningSpeedSmoothingFactor,
		                                  config.PassedTimeAfterLastPush,
		                                  velocity, ref angularVelocity);

		Rigidbody.velocity = velocity;
		Rigidbody.angularVelocity = angularVelocity;
	}

	private void CalculatePhysics_Carry()
	{
		var config = CarryConfiguration;
		var velocity = Rigidbody.velocity;
		var angularVelocity = Rigidbody.angularVelocity;

		// Adjust horizontal speed. Note that this forcefully overrides horizontal speed.
		OverrideHorizontalSpeedToMatchTargetHorizontalSpeed(ref velocity);

		if (IsPushing)
		{
			Rigidbody.AddForce(new Vector2(0f, config.PushForce), ForceMode2D.Force);
		}

		// Leaning with vertical speed. Note that this forcefully overrides angular velocity.
		OverrideAngularSpeedToLeanTheNose(config.VerticalSpeedToLeanConversionFactor,
		                                  config.LeaningSpeedSmoothingFactor,
		                                  config.PassedTimeAfterLastPush,
		                                  velocity, ref angularVelocity);

		Rigidbody.velocity = velocity;
		Rigidbody.angularVelocity = angularVelocity;
	}

	private void CalculatePhysics_Circus()
	{
		var config = CircusConfiguration;
		var velocity = Rigidbody.velocity;
		var angle = Rigidbody.rotation;
		var angleClipped = angle % 360f;
		if (angleClipped > 180f)
		{
			angleClipped -= 360f;
		}
		var angularVelocity = Rigidbody.angularVelocity;
		var localVelocity = Rigidbody.GetRelativeVector(velocity);
		var speed = velocity.magnitude;

		if (IsPushing)
		{
			// Rigidbody.AddForce(new Vector2(0f, config.PushForce), ForceMode2D.Force);
		}

		// Propeller force
		if (!IsTurnedOff)
		{
			var currentPower = GameManager.Instance.IsGameStarted
				? IsPushing ? config.FullThrottlePower : config.HalfThrottlePower
				: 0f;
			var currentPowerForce = currentPower * config.FullThrottleForwardForce;
			var degrade = config.ForwardForceDegradationBySpeed.Evaluate(Mathf.Max(localVelocity.x, 0f));

			if (angleClipped < -120f || angleClipped > 120f)
			{
				degrade = 0f;
			}

			Rigidbody.AddRelativeForce(new Vector2(currentPowerForce * degrade, 0f), ForceMode2D.Force);
		}

		// Wing force
		{
			var longitudinalSpeed = localVelocity.x;

			var forceFactor = config.LongitudinalSpeedToWingForceFactor.Evaluate(longitudinalSpeed);
			var force = config.WingForce * forceFactor;

			Rigidbody.AddRelativeForce(new Vector2(0f, force), ForceMode2D.Force);
		}

		// Nose up with touch input.
		{
			if (IsPushing)
			{
				if (angleClipped < -120f || angleClipped > 120f)
				{
					Rigidbody.AddTorque(config.FullThrottleRotationTorque * 2f);
				}
				else
				{
					Rigidbody.AddTorque(config.FullThrottleRotationTorque);
				}
			}
			else
			{
				if (angleClipped > -30f && angleClipped < 90f)
				{
					Rigidbody.AddTorque(config.HalfThrottleRotationTorque);
				}
			}
		}

		// Release the pilot.
		{
			if (angleClipped > 90f)
			{
				ReleasePilot();
			}
		}

		// Debug
		{
			var rotationInput = 0f;
			if (Input.GetKey(KeyCode.W))
			{
				rotationInput += 1f;
			}
			if (Input.GetKey(KeyCode.S))
			{
				rotationInput -= 1f;
			}
			if (Input.GetKey(KeyCode.A))
			{
				Rigidbody.AddForce(new Vector2(-2000f, 0f));
			}
			if (Input.GetKey(KeyCode.D))
			{
				Rigidbody.AddForce(new Vector2(2000f, 0f));
			}

			Rigidbody.AddTorque(rotationInput * config.FullThrottleRotationTorque);
		}
	}

	private void CalculatePhysics_Cartoon()
	{
		var config = CartoonConfiguration;
		var velocity = Rigidbody.velocity;
		var angle = Rigidbody.rotation;

		while (angle < 0) angle += 360;
		while (angle > 360) angle -= 360;

		var angleClipped = angle % 360f;
		if (angleClipped > 180f)
		{
			angleClipped -= 360f;
		}
		var angularVelocity = Rigidbody.angularVelocity;
		var localVelocity = Rigidbody.GetRelativeVector(velocity);
		var speed = velocity.magnitude;

		if (IsPushing)
		{
			// Rigidbody.AddForce(new Vector2(0f, config.PushForce), ForceMode2D.Force);
		}

		// Propeller force
		if (!IsTurnedOff)
		{
			var currentPower = GameManager.Instance.IsGameStarted
				? IsPushing ? config.FullThrottlePower : config.HalfThrottlePower
				: 0f;
			var currentPowerForce = currentPower * config.FullThrottleForwardForce;
			var degrade = config.ForwardForceDegradationBySpeed.Evaluate(Mathf.Max(localVelocity.x, 0f));

			//if (angleClipped < -30f || angleClipped > 120f)
			//{
			//	degrade = 0f;
			//}

			Rigidbody.AddRelativeForce(new Vector2(currentPowerForce * degrade * ((angle > 93 && angle < 270) ? 0.5f : 1f), 0f), ForceMode2D.Force);
		}

		// Wing force
		{
			var longitudinalSpeed = localVelocity.x;

			var forceFactor = config.LongitudinalSpeedToWingForceFactor.Evaluate(longitudinalSpeed);
			var force = config.WingForce * forceFactor;

			Rigidbody.AddRelativeForce(new Vector2(0f, force), ForceMode2D.Force);
		}

		// Nose up with touch input.
		{
			if (IsPushing)
			{
				Rigidbody.AddTorque(config.FullThrottleRotationTorque); // * ((angle > 93 && angle < 270) ? 0.5f : 1f));
			}
			else
			{
				Rigidbody.AddTorque(config.HalfThrottleRotationTorque * ((angle > 90 && angle < 330) ? -5f : 1f));
				//if (angleClipped > -30f || angleClipped > 120f)
				//{
				//	Rigidbody.AddTorque(config.HalfThrottleRotationTorque);
				//}
			}
		}

		// Release the pilot.
		{
			if (angleClipped > 90f)
			{
				ReleasePilot();
			}
		}

		// Debug
		{
			var rotationInput = 0f;
			if (Input.GetKey(KeyCode.W))
			{
				rotationInput += 1f;
			}
			if (Input.GetKey(KeyCode.S))
			{
				rotationInput -= 1f;
			}
			if (Input.GetKey(KeyCode.A))
			{
				Rigidbody.AddForce(new Vector2(-2000f, 0f));
			}
			if (Input.GetKey(KeyCode.D))
			{
				Rigidbody.AddForce(new Vector2(2000f, 0f));
			}

			Rigidbody.AddTorque(rotationInput * config.FullThrottleRotationTorque);
		}
	}

	private void ApplyWingForce(AnimationCurve baseTargetHeightOverTime, ref Vector2 velocity)
	{
		var currentHeight = Rigidbody.position.y;

		var baseTargetHeight = baseTargetHeightOverTime.Evaluate(GameManager.Instance.GamePassedTime);
		var targetHeight = InitialHeight + baseTargetHeight;

		var targetVerticalVelocity = (targetHeight - currentHeight) / Time.deltaTime;

		velocity.y = targetVerticalVelocity;
	}

	// private void ApplyWingForce(float wingForceY, AnimationCurve wingForceDegradeOverTime)
	// {
	// 	var passedTime = GameManager.Instance.RoundPassedTime;
	// 	var degrade = wingForceDegradeOverTime.Evaluate(passedTime);
	// 	var speedFactor = Mathf.Clamp01(CurrentHorizontalSpeed / TargetHorizontalSpeed);
	// 	Rigidbody.AddForce(new Vector2(0f, wingForceY * speedFactor * degrade), ForceMode2D.Force);
	// }

	private void OverrideHorizontalSpeedToMatchTargetHorizontalSpeed(ref Vector2 velocity)
	{
		velocity = new Vector2(CurrentHorizontalSpeed, velocity.y);
	}

	private void OverrideAngularSpeedToLeanTheNose(float verticalSpeedToLeanConversionFactor, float leaningSpeedSmoothingFactor, AnimationCurve curve, Vector2 velocity, ref float angularVelocity)
	{
		var verticalSpeed = velocity.y;
		var targetLeanAngle = verticalSpeed * verticalSpeedToLeanConversionFactor;
		var currentLeanAngle = Rigidbody.rotation;

		// Apply additional nose angle for a little while after starting to push.
		targetLeanAngle += curve.Evaluate(PassedTimeAfterLastPush);

		// Find the exact required angular speed that turns the nose to Target Lean Angle.
		var targetAngularSpeed = (targetLeanAngle - currentLeanAngle) / Time.deltaTime;

		// Then smooth out that speed.
		var smoothAngularSpeed = targetAngularSpeed * leaningSpeedSmoothingFactor;

		angularVelocity = smoothAngularSpeed;
	}

	#endregion

	#region Input

	private bool IsControlsEnabled = true;

	private bool IsPushing;
	private bool IsPushingDown
	{
		get
		{
			return IsPushing && PushingStartFrame == Time.frameCount;
		}
	}
	private int PushingStartFrame;
	private float PushingStartTime;

	private float PassedTimeAfterLastPush
	{
		get
		{
			if (!IsPushing)
				return float.MaxValue;
			return Time.time - PushingStartTime;
		}
	}

	private void CalculateInput()
	{
		var currentlyDown =
			!IsTurnedOff &&
			IsControlsEnabled &&
			Input.GetMouseButton(0) &&
			(Mode != PlanePhysicsMode.Drop || CurrentCrateCount > 0);

		if (IsPushing != currentlyDown)
		{
			if (currentlyDown)
			{
				PushingStartFrame = Time.frameCount;
				PushingStartTime = Time.time;
			}
			else
			{
				PushingStartFrame = -1;
				PushingStartTime = 0f;
			}
		}
		IsPushing = currentlyDown;
	}

	private void DisableControls()
	{
		IsControlsEnabled = false;
	}

	private void EnableControls()
	{
		IsControlsEnabled = true;
	}

	#endregion

	#region Crash

	private bool IsCrashed;
    private GameObject _blastEffect;

    private void ResetCrash()
    {
        IsCrashed = false;
        UnityEngine.Object.Destroy(_blastEffect);
    }

    private void OnCollisionEnter2D(Collision2D other)
	{
		if (!IsCrashed)
		{
			if (other.gameObject.layer == 9) // Ground
			{
				Debug.Log("Crashed");
				IsCrashed = true;
                _blastEffect = Instantiate(CrashBlast, this.transform);
                _blastEffect.transform.localPosition = Vector3.zero;

				EventManager.Instance.TriggerEvent(Constants.EVENT_PLANE_CRASHED);

				TurnOff();
				GameManager.Instance.InformGameEnd();
			}
		}
	}

	#endregion

	#region Engine

	private bool IsTurnedOff;

	private void ResetEngine()
	{
		IsTurnedOff = false;
	}

	private void TurnOff()
	{
		if (!IsTurnedOff)
		{
			IsTurnedOff = true;
		}
	}

	#endregion

	#region Pilot

	[Header("Pilot")]
	public GameObject PilotPrefab;
	public Transform PilotInstantiationLocation;
	public Vector2 PilotAdditionalLaunchVelocity;
	public float PilotLaunchAngularSpeed;
	public float PilotReleaseMinimumDuration = 2f;

	private bool IsPilotReleased => PilotReleaseTime > 0f;
	private float PilotReleaseTime;
	private Pilot ReleasedPilot;

	private void ResetPilot()
	{
		PilotReleaseTime = 0f;

		if (ReleasedPilot)
		{
			Destroy(ReleasedPilot.gameObject);
			ReleasedPilot = null;
		}
	}

	public void ReleasePilot()
	{
		if (!IsPilotReleased)
		{
			PilotReleaseTime = Time.time;

			var go = Instantiate(PilotPrefab, PilotInstantiationLocation.position, Quaternion.identity);
			ReleasedPilot = go.GetComponent<Pilot>();
			ReleasedPilot.StartFlying(this, Rigidbody.velocity + PilotAdditionalLaunchVelocity, PilotLaunchAngularSpeed);
		}
	}

	public void GatherPilot()
	{
		if (IsPilotReleased && Time.time > PilotReleaseTime + PilotReleaseMinimumDuration)
		{
			PilotReleaseTime = 0f;
			
			EventManager.Instance.TriggerEvent(Constants.EVENT_INCREMENT_SCORE, Constants.ScoreBonusPerPilotCatch);

			if (ReleasedPilot)
			{
				Destroy(ReleasedPilot.gameObject);
				ReleasedPilot = null;
			}
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (IsPilotReleased)
		{
			if (other.gameObject.layer == 14) // Crate layer
			{
				GatherPilot();
			}
		}
	}

	#endregion

	#region Propeller

	[Header("Propeller")]
	public float FullThrottlePropellerSpeed = 1f;
	public float NoThrottlePropellerSpeed = 0.1f;
	public float TurnOffStall = 0.02f;

	private void LateUpdatePropeller()
	{
		if (IsTurnedOff)
		{
			PropellerAnimator.speed = Mathf.Max(PropellerAnimator.speed - TurnOffStall, 0f);
		}
		else
		{
			PropellerAnimator.speed = IsPushing
				? FullThrottlePropellerSpeed
				: NoThrottlePropellerSpeed;
		}
	}

	#endregion

	#region Exhaust

	[Header("Exhaust")]
	public ParticleSystem ExhaustParticleSystem;
	public float ExhaustRateFactorAtNoThrottle = 0.3f;
	private ParticleSystem.EmissionModule ExhaustEmission;
	private float InitialEmissionRate;

	private void InitializeExhaust()
	{
		ExhaustEmission = ExhaustParticleSystem.emission;
		InitialEmissionRate = ExhaustEmission.rateOverTimeMultiplier;
	}

	private void ResetExhaust()
	{
		ExhaustEmission.enabled = false;
	}

	private void FixedUpdateExhaust()
	{
		var active = !IsTurnedOff && GameManager.Instance.IsGameStarted;
		ExhaustEmission.enabled = active;
		if (active)
		{
			ExhaustEmission.rateOverTimeMultiplier = (IsPushing ? 1f : ExhaustRateFactorAtNoThrottle) * InitialEmissionRate;
		}
	}

	#endregion

	#region Benzin

	private void ExpendFuel()
	{
		float deltaFuel = Constants.FuelExpenditurePerSecond * Time.fixedDeltaTime;
		OnGainFuel(deltaFuel);
	}
	#endregion

	#region Crate

	[Header("Crate")]
	public GameObject CratePrefab;
	public Transform CrateInstantiationLocation;
	public Vector2 CrateInitialVelocity = new Vector3(0f, 1f);
	public float CrateInitialAngularSpeed = 3f;
	public float CrateMass = 25f;

	internal int CurrentCrateCount;

	private void ResetCrates()
	{
		switch (Mode)
		{
			case PlanePhysicsMode.Drop:
			{
				CurrentCrateCount = DropConfiguration.InitialCrateCount;
				break;
			}

			case PlanePhysicsMode.Carry:
				break;

			case PlanePhysicsMode.Circus:
				break;

			case PlanePhysicsMode.Cartoon:
				break;

			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void InstantiateCrate(Vector2 velocity)
	{
		var currentVelocity = velocity;
		var go = Instantiate(CratePrefab, CrateInstantiationLocation.position, Quaternion.identity);
		var body = go.GetComponent<Rigidbody2D>();
		body.mass = CrateMass;
		body.velocity = currentVelocity + CrateInitialVelocity;
		body.angularVelocity = CrateInitialAngularSpeed;
	}

	#endregion

	#region Spawn Location

	[Header("Spawning")]
	public Vector3 SpawnLocation;

	public void PlaceToSpawnLocation()
	{
		Debug.Log("Placing plane to spawn location");

		Transform.position = SpawnLocation;
		Transform.rotation = Quaternion.identity;
		Rigidbody.velocity = Vector3.zero;
		Rigidbody.angularVelocity = 0f;
	}

	#endregion


	public void OnGainFuel(float value)
	{
		_fuel += value;
		if (_fuel > Constants.FuelCapacity) _fuel = Constants.FuelCapacity;
		if (_fuel < 0)
		{
			_fuel = 0;
			EventManager.Instance.TriggerEvent(Constants.EVENT_OUT_OF_FUEL);
			TurnOff();
		}
		EventManager.Instance.TriggerEvent(Constants.EVENT_SET_PROGRESSBAR, _fuel / Constants.FuelCapacity);
	}

	public void OnSetFuel(float value)
	{
		_fuel = value;
		EventManager.Instance.TriggerEvent(Constants.EVENT_SET_PROGRESSBAR, _fuel / Constants.FuelCapacity);
	}
}
