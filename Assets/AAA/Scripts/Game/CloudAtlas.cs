using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudAtlas : MonoBehaviour
{
    public GameObject CloudPrefab;

    public List<GameObject> Clouds;

    public int CloudCount;


    // Start is called before the first frame update
    void Start()
    {
        GenerateInitialClouds();
    }

    private void GenerateInitialClouds()
    {
        for (int i = 0; i < (CloudCount / 3); i++)
        {
            GameObject go = GameObject.Instantiate(CloudPrefab, transform.position + new Vector3((i * 52f), 0f + UnityEngine.Random.Range(-5f, 5f), 0f), Quaternion.identity);
            go.transform.SetParent(this.transform);
            GameObject go2 = GameObject.Instantiate(CloudPrefab, transform.position + new Vector3((i * 52f) + UnityEngine.Random.Range(-5f, 5f), 17f + UnityEngine.Random.Range(-5f, 5f), 0f), Quaternion.identity);
            go2.transform.SetParent(this.transform);
            GameObject go3 = GameObject.Instantiate(CloudPrefab, transform.position + new Vector3((i * 52f) + UnityEngine.Random.Range(-5f, 5f), -17f + UnityEngine.Random.Range(-5f, 5f), 0f), Quaternion.identity);
            go3.transform.SetParent(this.transform);
            Clouds.Add(go);
            Clouds.Add(go2);
            Clouds.Add(go3);

        }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(Camera.main.transform);
    }
}
