using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiator : MonoBehaviour
{
	

	[Tooltip("These objects will be instantiated with DontDestroyOnLoad Flag in the order they appear in this list. Resilient Objects are instantiated prior to regular objects.")]
	public List<Transform> ResilientObjects;

	[Tooltip("These objects will be instantiated in the order they appear in this list. Resilient Objects are instantiated prior to regular objects.")]
	public List<Transform> Objects;

	private bool _didInstantiateObjects = false;

	private static Instantiator _instance;

	public Instantiator Instance
	{
		get { return _instance; }
	}


	//TODO: make this class a singleton
	void OnGUI()
	{

	}


	void OnEnable()
	{
		InstantiateObjects();
	}

	void Awake()
	{
		InstantiateObjects();
	}

    void InstantiateObjects()
    {
	    if (_didInstantiateObjects) return;

	    for (int i = 0; i < ResilientObjects.Count; i++)
	    {
		    if (ResilientObjects[i] != null)
		    {
			    Instantiate(ResilientObjects[i]);
		    }
	    }

	    for (int i = 0; i < Objects.Count; i++)
	    {
		    if (Objects[i] != null)
		    {
			    Instantiate(Objects[i]);
		    }
	    }

		_didInstantiateObjects = true;

		Destroy(this.gameObject);
    }

    void OnDestroy()
    {
	    _instance = null;
	    Objects = null;
	    ResilientObjects = null;
    }

}
