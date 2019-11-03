using UnityEngine;

public class Pilot : MonoBehaviour
{
	public Vector2 LinearDrag;
	public Rigidbody2D[] Rigidbodies;

	protected void FixedUpdate()
	{
		foreach (var body in Rigidbodies)
		{
			body.velocity = Vector2.Scale(body.velocity, LinearDrag);
		}
	}
}
