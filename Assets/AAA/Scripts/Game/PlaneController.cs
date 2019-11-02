﻿using System;
using UnityEngine;

public enum PlanePhysicsMode
{
	Drop,
	Carry,
}

[Serializable]
public class DropModeConfiguration
{
	public float TargetHorizontalSpeed = 6f;
	public float VerticalSpeedToLeanConversionFactor = 5f;
	public float LeaningSpeedSmoothingFactor = 0.15f;
	public float PushImpulse = 150f;
}

[Serializable]
public class CarryModeConfiguration
{
	public float TargetHorizontalSpeed = 6f;
	public float VerticalSpeedToLeanConversionFactor = 5f;
	public float LeaningSpeedSmoothingFactor = 0.15f;
	public float PushForce = 30f;
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

	#endregion

	#region Links

	[Header("Links")]
	public Transform Transform;
	public Rigidbody2D Rigidbody;

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
	}

	private void OnEnableControls()
	{
		EnableControls();
		CurrentHorizontalSpeed = TargetHorizontalSpeed;
	}

	#endregion

	#region Physics

	[Header("Configuration")]
	public PlanePhysicsMode Mode = PlanePhysicsMode.Drop;
	public DropModeConfiguration DropConfiguration;
	public CarryModeConfiguration CarryConfiguration;

	public float TargetHorizontalSpeed
	{
		get
		{
			switch (Mode)
			{
				case PlanePhysicsMode.Drop: return DropConfiguration.TargetHorizontalSpeed;
				case PlanePhysicsMode.Carry: return CarryConfiguration.TargetHorizontalSpeed;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	internal float CurrentVerticalSpeed;
	internal float CurrentHorizontalSpeed;

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

			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void CalculatePhysics_Drop()
	{
		var config = DropConfiguration;

		// Adjust horizontal speed.
		OverrideHorizontalSpeedToMatchTargetHorizontalSpeed();

		if (IsPushingDown)
		{
			Rigidbody.AddForce(new Vector2(0f, config.PushImpulse), ForceMode2D.Impulse);
			InstantiateCrate();
		}

		// Leaning with vertical speed. Note that this forcefully overrides angular velocity.
		OverrideAngularSpeedToLeanTheNose(config.VerticalSpeedToLeanConversionFactor, config.LeaningSpeedSmoothingFactor);
	}

	private void CalculatePhysics_Carry()
	{
		var config = CarryConfiguration;

		// Adjust horizontal speed. Note that this forcefully overrides horizontal speed.
		OverrideHorizontalSpeedToMatchTargetHorizontalSpeed();

		if (IsPushing)
		{
			Rigidbody.AddForce(new Vector2(0f, config.PushForce), ForceMode2D.Force);
		}

		// Leaning with vertical speed. Note that this forcefully overrides angular velocity.
		OverrideAngularSpeedToLeanTheNose(config.VerticalSpeedToLeanConversionFactor, config.LeaningSpeedSmoothingFactor);
	}

	private void OverrideHorizontalSpeedToMatchTargetHorizontalSpeed()
	{
		Rigidbody.velocity = new Vector2(CurrentHorizontalSpeed, Rigidbody.velocity.y);
	}

	private void OverrideAngularSpeedToLeanTheNose(float verticalSpeedToLeanConversionFactor, float leaningSpeedSmoothingFactor)
	{
		var verticalSpeed = Rigidbody.velocity.y;
		var targetLeanAngle = verticalSpeed * verticalSpeedToLeanConversionFactor;
		var currentLeanAngle = Rigidbody.rotation;

		// Find the exact required angular speed that turns the nose to Target Lean Angle.
		var targetAngularSpeed = (targetLeanAngle - currentLeanAngle) / Time.deltaTime;

		// Then smooth out that speed.
		var smoothAngularSpeed = targetAngularSpeed * leaningSpeedSmoothingFactor;

		Rigidbody.angularVelocity = smoothAngularSpeed;
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

	private void CalculateInput()
	{
		var currentlyDown = IsControlsEnabled && Input.GetMouseButton(0);

		if (IsPushing != currentlyDown)
		{
			PushingStartFrame = currentlyDown
				? Time.frameCount
				: -1;
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

	#region Crate

	[Header("Crate")]
	public GameObject CratePrefab;
	public Transform CrateInstantiationLocation;
	public Vector2 CrateInitialVelocity = new Vector3(0f, 1f);
	public float CrateInitialAngularSpeed = 3f;

	public void InstantiateCrate()
	{
		var currentVelocity = Rigidbody.velocity;
		var go = Instantiate(CratePrefab, CrateInstantiationLocation.position, Quaternion.identity);
		var body = go.GetComponent<Rigidbody2D>();
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
