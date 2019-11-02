﻿using UnityEngine;

public class PlaneController : MonoBehaviour
{
	#region Initialization

	protected void Awake()
	{
		if (DisableControlsAtStart)
		{
			DisableControls();
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
	}

	#endregion

	#region Physics

	[Header("Configuration")]
	public float VerticalSpeed;
	public float MoveSpeed;

	private void CalculatePhysics()
	{
		Rigidbody.AddForce(Vector2.right * (MoveSpeed - Transform.rotation.z), ForceMode2D.Force);

		if (IsPushingDown)
		{
			Rigidbody.AddForce(Vector2.up * VerticalSpeed, ForceMode2D.Impulse);

			InstantiateCrate();
		}

		if (Rigidbody.velocity.y > 10f)
		{
			Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, 10f);
		}
		else if (Rigidbody.velocity.y < -10f)
		{
			Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, -10f);
		}

		if (Rigidbody.velocity.y > 0)
		{
			Transform.Rotate(Vector3.forward, 10f);
			if (Transform.rotation.z > 0.3f)
				Transform.rotation = new Quaternion(0, 0, 0.3f, 1f);
		}
		else if (Rigidbody.velocity.y < 0)
		{
			Transform.Rotate(Vector3.forward, -1f);
			if (Transform.rotation.z < -0.3f)
				Transform.rotation = new Quaternion(0, 0, -0.3f, 1f);
		}
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
	public Vector3 CrateInstantiationOffset = new Vector3(0, -0.7f, 0);

	public void InstantiateCrate()
	{
		Instantiate(CratePrefab, Transform.position + CrateInstantiationOffset, Quaternion.identity);
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