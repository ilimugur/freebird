using UnityEngine;

public class OilClusterElement : MonoBehaviour
{
    public OilGenerator OilGeneratorScript;

    void Awake()
    {
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        OilGeneratorScript.HideEnteredClusterElement(gameObject);
    }
}