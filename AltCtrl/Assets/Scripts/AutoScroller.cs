using System;
using UnityEngine;

public class AutoScroller : MonoBehaviour
{
    [SerializeField] private float speed;

    private void FixedUpdate()
    {
        transform.position += Vector3.back * (0.1f * speed);
    }
}
