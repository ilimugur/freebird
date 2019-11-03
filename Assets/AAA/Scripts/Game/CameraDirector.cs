using UnityEngine;

public class CameraDirector : MonoBehaviour
{
	public static CameraDirector Instance;

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

	private void Awake()
	{
		Instance = this;
	}

	void FixedUpdate()
	{
		var currentPosition = transform.position;
		var targetPosition = Target.Transform.position;
		targetPosition.z = -30f;
		targetPosition.x += 3f;
		var newPosition = currentPosition + (targetPosition - currentPosition) * SmoothingFactor;
		// newPosition.x = targetPosition.x + PositionX;
		transform.position = newPosition;
	}
}
