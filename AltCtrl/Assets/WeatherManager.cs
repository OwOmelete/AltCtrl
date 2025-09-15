using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    [Header("Références")]
    [Tooltip("ParticleSystem à contrôler (sera auto-renseigné avec un enfant si laissé vide).")]
    [SerializeField] private ParticleSystem childParticleSystem;

    private Coroutine rateTweenRoutine;

    private void Awake()
    {
     
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (childParticleSystem == null)
        {
            var all = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in all)
            {
                if (ps.transform != transform)
                {
                    childParticleSystem = ps;
                    break;
                }
            }
        }

        if (childParticleSystem == null)
        {
            Debug.LogError("ajoutez un enfant avec un ParticleSystem.");
        }
    }

    public void RateOverTime(float Time, float newRateOverTime)
    {
        if (childParticleSystem == null)
        {
            Debug.LogError("ajoutez un enfant avec un ParticleSystem.");
            return;
        }

        if (rateTweenRoutine != null)
            StopCoroutine(rateTweenRoutine);

        rateTweenRoutine = StartCoroutine(RateOverTimeCoroutine(Time, newRateOverTime));
    }

    private IEnumerator RateOverTimeCoroutine(float duration, float targetRate)
    {
        var emission = childParticleSystem.emission;
        
        float startRate = GetCurrentRateOverTime(emission);

        duration = Mathf.Max(0f, duration);
        if (duration == 0f)
        {
            emission.rateOverTime = targetRate;
            rateTweenRoutine = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += UnityEngine.Time.deltaTime;
            float alpha = Mathf.Clamp01(t / duration);
            float current = Mathf.Lerp(startRate, targetRate, alpha);
            emission.rateOverTime = current;
            yield return null;
        }

        emission.rateOverTime = targetRate;
        rateTweenRoutine = null;
    }


    private float GetCurrentRateOverTime(ParticleSystem.EmissionModule emission)
    {
        var curve = emission.rateOverTime;
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return curve.constant;
            case ParticleSystemCurveMode.TwoConstants:
                return curve.constantMax;
            case ParticleSystemCurveMode.Curve:
            case ParticleSystemCurveMode.TwoCurves:
                return curve.Evaluate(childParticleSystem.time);
            default:
                return curve.constant;
        }
    }
    
    public float CurrentRateOverTime
    {
        get
        {
            if (childParticleSystem == null) return 0f;
            return GetCurrentRateOverTime(childParticleSystem.emission);
        }
    }
}
