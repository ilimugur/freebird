using UnityEngine;

public class Pilot : MonoBehaviour
{
    public Vector2 LinearDrag;
    public Rigidbody2D Rigidbody;

    protected void FixedUpdate()
    {
        Rigidbody.velocity = Vector2.Scale(Rigidbody.velocity, LinearDrag);
    }
}
