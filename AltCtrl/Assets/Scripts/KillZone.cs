using System;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private Pooling pooling;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Background"))
        {
            pooling.pool.Add(other.gameObject);
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(other.gameObject);
        }
    }
}
