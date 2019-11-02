using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDirector : MonoBehaviour
{

    public PlaneController Target;
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

        Vector3 newPos = Vector3.MoveTowards(transform.position, Target.CameraTarget, FollowSpeed);
        newPos = new Vector3(Target.CameraTarget.x, newPos.y, -10f);
        transform.position = newPos;
    }
}
