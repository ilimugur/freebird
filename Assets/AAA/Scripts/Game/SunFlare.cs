using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunFlare : MonoBehaviour
{
    public Vector3 LastPosition;
    public float FactorX;
    public float FactorY;
    // Start is called before the first frame update
    void Start()
    {
        LastPosition = Camera.main.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        float distX = (Camera.main.transform.position.x - LastPosition.x) / FactorX;
        float distY = (Camera.main.transform.position.y - LastPosition.y) / FactorY;
        LastPosition = Camera.main.transform.position;
        transform.position = Camera.main.transform.position + new Vector3(4f, 7f, 60f);
        for(int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.position -= new Vector3(i * distX, i * distY, 0f);
        }
        
    }
}
