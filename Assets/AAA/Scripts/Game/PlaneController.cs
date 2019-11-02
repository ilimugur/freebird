using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public Rigidbody2D RB;

    public float VerticalSpeed;
    public float MoveSpeed;

    public GameObject CratePrefab;

    public Vector3 CameraTarget;

    void FixedUpdate()
    {
        UpdateMethodSecond();
    }

    void UpdateMethodSecond()
    {
        RB.AddForce(Vector2.right * (MoveSpeed - transform.rotation.z), ForceMode2D.Force);

        if (Input.GetMouseButtonDown(0))
        {
            RB.AddForce(Vector2.up * VerticalSpeed, ForceMode2D.Impulse);

            GameObject go = GameObject.Instantiate(CratePrefab, this.transform.position + new Vector3(0, -0.7f, 0), Quaternion.identity);
        }

        if (RB.velocity.y > 10f)
        {
            RB.velocity = new Vector2(RB.velocity.x, 10f);
        }
        else if (RB.velocity.y < -10f)
        {
            RB.velocity = new Vector2(RB.velocity.x, -10f);
        }

        if (RB.velocity.y > 0)
        {
            CameraTarget = transform.position + new Vector3(0f, 1 + transform.rotation.z);
            transform.Rotate(Vector3.forward, 10f);
            if (transform.rotation.z > 0.3f)
                transform.rotation = new Quaternion(0, 0, 0.3f, 1f);
        }
        else if (RB.velocity.y < 0)
        {
            CameraTarget = transform.position + new Vector3(0f, -1 - transform.rotation.z);
            transform.Rotate(Vector3.forward, -1f);
            if (transform.rotation.z < -0.3f)
                transform.rotation = new Quaternion(0, 0, -0.3f, 1f);
        }
    }

    void UpdateMethodFirst()
    {
        if (Input.GetMouseButton(0))
        {
            RB.AddForce(Vector2.up * VerticalSpeed, ForceMode2D.Force);
        }

        if (RB.velocity.y > 0)
        {
            Quaternion rot = transform.rotation;
            transform.Rotate(Vector3.forward, 1f);
            if (transform.rotation.z > 0.3f)
                transform.rotation = rot;
        }
        else if (RB.velocity.y < 0)
        {
            Quaternion rot = transform.rotation;
            transform.Rotate(Vector3.forward, -1f);

            if (transform.rotation.z < -0.3f)
                transform.rotation = rot;
        }
    }
}
