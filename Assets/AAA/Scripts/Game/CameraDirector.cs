﻿using UnityEngine;

public class CameraDirector : MonoBehaviour
{
	private PlaneController _Target;
	public PlaneController Target
	{
		get
		{
			if (!_Target)
			{
				_Target = FindObjectOfType<PlaneController>();
			}
			return _Target;
		}
	}
	public float SmoothingFactor = 0.03f;
	public float PositionX = -1f;

	void FixedUpdate()
	{
		var currentPosition = transform.position;
		var targetPosition = Target.Transform.position;
		targetPosition.z = -30f;
		var newPosition = currentPosition + (targetPosition - currentPosition) * SmoothingFactor;
		newPosition.x = targetPosition.x + PositionX;
		transform.position = newPosition;
	}
}
