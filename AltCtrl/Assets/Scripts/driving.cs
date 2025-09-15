using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class driving : MonoBehaviour
{
    public float displacement;
    
    private bool grounded = false;
    [SerializeField] private int pv = 3;

    private void Update()
    {
        transform.position = new Vector3(Input.GetAxisRaw("Horizontal") * displacement, Input.GetAxisRaw("Vertical") * displacement, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
        pv -= 1;
        Debug.Log("pv : " + pv);
    }
}
