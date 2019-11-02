using UnityEngine;

public class PlaneController : MonoBehaviour
{
	#region Initialization

	// protected void Awake()
	// {
	// }

	#endregion

	#region Update

	protected void FixedUpdate()
	{
		UpdateMethodSecond();
	}

	#endregion

	#region Links

	[Header("Links")]
	public Transform Transform;
	public Rigidbody2D Rigidbody;

	#endregion

	#region Physics

	[Header("Configuration")]
	public float VerticalSpeed;
	public float MoveSpeed;

	void UpdateMethodSecond()
	{
		Rigidbody.AddForce(Vector2.right * (MoveSpeed - Transform.rotation.z), ForceMode2D.Force);

		if (Input.GetMouseButtonDown(0))
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

	#region Crate

	[Header("Crate")]
	public GameObject CratePrefab;
	public Vector3 CrateInstantiationOffset = new Vector3(0, -0.7f, 0);

	public void InstantiateCrate()
	{
		Instantiate(CratePrefab, Transform.position + CrateInstantiationOffset, Quaternion.identity);
	}

	#endregion
}
