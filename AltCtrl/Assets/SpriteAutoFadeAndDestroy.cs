using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAutoFadeAndDestroy : MonoBehaviour
{
    [Header("Références")]
    public SpriteRenderer spriteRenderer;

    [Header("Paramètres")]
    [Min(0f)] public float delayBeforeFade = 3f;   // temps d'attente avant de commencer à disparaître
    [Min(0f)] public float fadeDuration   = 3f;    // durée de la disparition
    public bool startAutomatically        = true;  // lance tout seul à l'Enable
    public bool useUnscaledTime           = false; // ignorer Time.timeScale si besoin (UI, pause, etc.)

    private Coroutine _routine;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (startAutomatically)
            Begin();
    }

    /// <summary>Lance la séquence (attente -> fondu -> destruction).</summary>
    public void Begin()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FadeThenDestroy());
    }

    private IEnumerator FadeThenDestroy()
    {
        if (!spriteRenderer) yield break;

        // 1) Attente
        float t = 0f;
        while (t < delayBeforeFade)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        // 2) Fondu (alpha -> 0)
        float elapsed = 0f;
        float startA  = spriteRenderer.color.a;
        const float endA = 0f;

        if (fadeDuration <= 0f)
        {
            SetAlpha(endA);
        }
        else
        {
            while (elapsed < fadeDuration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float k = Mathf.Clamp01(elapsed / fadeDuration);
                SetAlpha(Mathf.Lerp(startA, endA, k));
                yield return null;
            }
            SetAlpha(0f); // sécurité: alpha exactement à 0
        }

        // 3) Destruction
        Destroy(gameObject);
    }

    private void SetAlpha(float a)
    {
        var c = spriteRenderer.color;
        c.a = a;
        spriteRenderer.color = c;
    }
}
