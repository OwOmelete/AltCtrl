using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Tooltip("Temps avant destruction (secondes)")]
    [Min(0f)] public float delay = 3f;

    void Start()
    {
        if (delay <= 0f) Destroy(gameObject);
        else Destroy(gameObject, delay);
    }
}