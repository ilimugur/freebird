using UnityEngine;

public class PilotBody : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("ONTRIG " + other.gameObject.name);
	}
}
