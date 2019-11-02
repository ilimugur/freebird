using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ControlEnabler : MonoBehaviour
{
	private Collider2D _collider;
    // Start is called before the first frame update
    void Awake()
    {
	    _collider = GetComponent<Collider2D>();
    }

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.layer == Constants.PlaneLayer)
		{
			GameManager.Instance.StartRound();
		}
	}
}
