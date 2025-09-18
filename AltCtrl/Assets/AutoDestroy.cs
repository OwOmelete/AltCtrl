using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    
    List<string> clips = new List<string> { "SFX Essuie-glaces" };
    [Tooltip("Temps avant destruction (secondes)")]
    [Min(0f)] public float delay = 3f;

    void Start()
    {
        SoundManager.Instance.PlayRandomSFX(clips, 0.9f, 1.1f);
        if (delay <= 0f) Destroy(gameObject);
        else Destroy(gameObject, delay);
    }
}