﻿using System;
using DG.Tweening;
using UnityEngine;

public enum PlanePhysicsMode
{
	Drop,
	Carry,
	Circus,
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

public class PlaneController : MonoBehaviour
{
	#region Initialization

	protected void Awake()
	{
		if (DisableControlsAtStart)
		{
			DisableControls();
		}
		else
		{
			EnableControls();
		}
		InitializeEvents();
	}

	#endregion

	#region Update

	protected void FixedUpdate()
	{
		CalculateInput();
		CalculatePhysics();
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

	#endregion

	#region Events

	private void InitializeEvents()
	{
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_START, OnLevelStart);
		EventManager.Instance.StartListening(Constants.EVENT_ENABLE_CONTROLS, OnEnableControls);
	}

	private void OnLevelStart()
	{
		PlaceToSpawnLocation();
		InitializeCrates();
		InitializePhysics();
	}

	private void OnEnableControls()
	{
		EnableControls();
	}

	#endregion

	#region Physics

	[Header("Configuration")]
	public PlanePhysicsMode Mode = PlanePhysicsMode.Drop;
	public float PlaneMass = 50f;
	public DropModeConfiguration DropConfiguration;
	public CarryModeConfiguration CarryConfiguration;
	public CircusModeConfiguration CircusConfiguration;

	// internal float CurrentVerticalSpeed;
	internal float CurrentHorizontalSpeed;
	internal float InitialHeight;

	private void InitializePhysics()
	{
		InitialHeight = Transform.position.y;

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
		{
			var currentPower = GameManager.Instance.IsGameStarted
				? IsPushing ? config.FullThrottlePower : config.HalfThrottlePower
				: 0f;
			var currentPowerForce = currentPower * config.FullThrottleForwardForce;
			var degrade = config.ForwardForceDegradationBySpeed.Evaluate(Mathf.Max(localVelocity.x, 0f));

			if (angleClipped < -30f || angleClipped > 120f)
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
				Rigidbody.AddTorque(config.FullThrottleRotationTorque);
			}
			else
			{
				if (angleClipped > -30f && angleClipped < 90f)
				{
					Rigidbody.AddTorque(config.HalfThrottleRotationTorque);
				}
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

	#region Propeller

	[Header("Propeller")]
	public float FullThrottlePropellerSpeed = 1f;
	public float NoThrottlePropellerSpeed = 0.1f;

	private void LateUpdatePropeller()
	{
		PropellerAnimator.speed = IsPushing
			? FullThrottlePropellerSpeed
			: NoThrottlePropellerSpeed;
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

	private void InitializeCrates()
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
	public bool DisableControlsAtStart = false;

	private void PlaceToSpawnLocation()
	{
		Transform.position = SpawnLocation;
		Transform.rotation = Quaternion.identity;
		Rigidbody.velocity = Vector3.zero;
		Rigidbody.angularVelocity = 0f;
	}

	#endregion
}
