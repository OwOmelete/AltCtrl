using System;
using UnityEngine;

public class DeactivateZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        Invoke("Destroy(obj)", 1);
        other.enabled = false;
    }
}