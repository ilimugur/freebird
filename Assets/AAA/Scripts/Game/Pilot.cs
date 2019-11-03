using UnityEngine;

public class Pilot : MonoBehaviour
{
	public Vector2 LinearDrag;
	public Rigidbody2D[] Rigidbodies;
	public Rigidbody2D HeadRigidbody;
	public Rigidbody2D BodyRigidbody;

	public float FlutterDuration = 2f;
	public float FlutterWaveFrequency = 2f;
	public float FlutterWaveAmplitude = 2f;

	public float DiveSpeed = 10f;

	private float FlyStartTime;
	private PlaneController Plane;

	public void StartFlying(PlaneController plane, Vector2 velocity, float angularVelocity)
	{
		Plane = plane;
		FlyStartTime = Time.time;

		foreach (var body in Rigidbodies)
		{
			body.velocity = velocity;
			body.angularVelocity = angularVelocity;
		}
	}

	protected void FixedUpdate()
	{
		foreach (var body in Rigidbodies)
		{
			body.velocity = Vector2.Scale(body.velocity, LinearDrag);
		}

		var now = Time.time;

		if (now < FlyStartTime + FlutterDuration)
		{
			var torque = Mathf.Sin(now * FlutterWaveFrequency) * FlutterWaveAmplitude;
			BodyRigidbody.AddTorque(torque);
			foreach (var body in Rigidbodies)
			{
				body.AddTorque(-torque);
			}
		}
		else
		{
			var diff = Plane.Rigidbody.position - HeadRigidbody.position;
			HeadRigidbody.velocity = diff.normalized * DiveSpeed;
		}
	}
}
