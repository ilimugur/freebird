﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDirector : MonoBehaviour
{

    private PlaneController _Target;
    public PlaneController Target
    {
        get
        {
            if (!_Target)
            {
                _Target = FindObjectOfType<PlaneController>();
            }
            return _Target;
        }
    }
    public float FollowSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*
        Vector3 newPos = transform.position;
        newPos.x = Target.CameraTarget.x;
        transform.position = newPos;
        */

        var targetPosition = Target.Transform.position;
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, FollowSpeed);
        newPos = new Vector3(targetPosition.x, newPos.y, -10f);
        transform.position = newPos;
    }
}