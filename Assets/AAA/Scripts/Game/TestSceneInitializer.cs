using System.Collections;
using UnityEngine;

public class TestSceneInitializer : MonoBehaviour
{
	public Transform SpawnLocation;

	IEnumerator Start()
	{
		yield return null;
		var plane = FindObjectOfType<PlaneController>();
		plane.SpawnLocation = SpawnLocation.position;
		EventManager.Instance.TriggerEvent(Constants.EVENT_LEVEL_START);
		EventManager.Instance.TriggerEvent(Constants.EVENT_ENABLE_CONTROLS);
	}
}
