using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class driving : MonoBehaviour
{
    public float displacement;
    
    private bool grounded = false;

    private void Update()
    {
        transform.position = new Vector3(Input.GetAxisRaw("Horizontal") * displacement, Input.GetAxisRaw("Vertical") * displacement, 0);
        
    }
}
