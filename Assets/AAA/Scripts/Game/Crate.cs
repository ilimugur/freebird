using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour
{
    public Vector3 StartPosition;

    // Start is called before the first frame update
    void Start()
    {
        StartPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < (StartPosition.y - 10f))
        {
            Destroy(this.gameObject);
        }
    }
}
