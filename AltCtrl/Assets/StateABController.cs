using System;
using UnityEngine;

[DisallowMultipleComponent]
public class StateABController : MonoBehaviour
{
    [Header("Cibles (auto-assign si laissées vides)")]
    [Tooltip("MeshRenderer portant le Material à faire varier (Base Map Color).")]
    public MeshRenderer targetRenderer;

    [Tooltip("Script Waves à piloter (interpolation des Octaves & uvScale).")]
    public Waves targetWaves;

    [Tooltip("Script ScrollTexture à piloter (scrollSpeedX/Y).")]
    public ScrollTexture targetScroll;
    
    public enum ColorPropertyMode { Auto, BaseColor, Color, Custom }

    [Header("Material Color (Base Map)")]
    [Tooltip("Auto : tente _BaseColor puis _Color.\nBaseColor : force _BaseColor.\nColor : force _Color.\nCustom : utilise la chaîne ci-dessous.")]
    public ColorPropertyMode colorPropertyMode = ColorPropertyMode.Auto;

    [Tooltip("Nom de propriété couleur custom (si mode = Custom).")]
    public string customColorProperty = "_BaseColor";

    [Tooltip("Index du material à piloter sur le MeshRenderer.")]
    public int materialIndex = 0;
    
    [Header("Durée par défaut et options")]
    [Min(0f)] public float defaultDuration = 1f;
    [Tooltip("Capture automatiquement l'état A au Start().")]
    public bool captureAOnStart = true;
    [Tooltip("Initialise B comme copie de A au Start() (modifiable ensuite).")]
    public bool initBFromAOnStart = true;

    [Serializable]
    public class MaterialState
    {
        public Color baseColor = Color.white;
    }

    [Serializable]
    public class WavesState
    {
        [Tooltip("Note : modifié seulement en fin de transition (voir Remarque dans le code).")]
        public int Dimension = 10;
        public float uvScale = 1f;

        [Tooltip("Copie éditable de Waves.Octave[]")]
        public Waves.Octave[] Octaves = Array.Empty<Waves.Octave>();
    }

    [Serializable]
    public class ScrollTextureState
    {
        public float scrollSpeedX = 0f;
        public float scrollSpeedY = 0f;
    }

    [Serializable]
    public class Snapshot
    {
        public MaterialState material = new MaterialState();
        public WavesState waves = new WavesState();
        public ScrollTextureState scroll = new ScrollTextureState();
    }

    [Header("Etat A (capturé depuis la scène)")]
    public Snapshot stateA = new Snapshot();

    [Header("Etat B (cible, éditable)")]
    public Snapshot stateB = new Snapshot();

  
    private Coroutine _transitionCo;
    private bool _isAToB = true; 

   
    #region Unity lifecycle
    private void Reset()
    {
        AutoAssignTargets();
    }

    private void Awake()
    {
        AutoAssignTargets();
    }

    private void Start()
    {
        if (captureAOnStart)
            CaptureAFromScene();

        if (initBFromAOnStart)
            CopyAtoB();
    }

    private void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.G))
            GoAToB(defaultDuration);

        if (Input.GetKeyDown(KeyCode.H))
            GoBToA(defaultDuration);
    }
    #endregion
   

    #region Public API
   
    public void GoAToB(float duration)
    {
        StartTransition(true, Mathf.Max(0f, duration));
    }

   
    public void GoBToA(float duration)
    {
        StartTransition(false, Mathf.Max(0f, duration));
    }
    #endregion

    #region Capture/Init helpers (ContextMenu pour confort)
    [ContextMenu("StateAB/Capture A From Scene")]
    public void CaptureAFromScene()
    {
   
        if (TryGetMaterial(out var mat, out var colorProp))
        {
            stateA.material.baseColor = GetMaterialColor(mat, colorProp);
        }

    
        if (targetWaves != null)
        {
            stateA.waves.Dimension = targetWaves.Dimension;
            stateA.waves.uvScale = targetWaves.uvScale;
            stateA.waves.Octaves = DeepCopy(targetWaves.Octaves);
        }
        
        if (targetScroll != null)
        {
            stateA.scroll.scrollSpeedX = targetScroll.scrollSpeedX;
            stateA.scroll.scrollSpeedY = targetScroll.scrollSpeedY;
        }
    }

    [ContextMenu("StateAB/Copy A -> B (overwrite)")]
    public void CopyAtoB()
    {
        stateB.material.baseColor = stateA.material.baseColor;

        stateB.waves.Dimension = stateA.waves.Dimension;
        stateB.waves.uvScale = stateA.waves.uvScale;
        stateB.waves.Octaves = DeepCopy(stateA.waves.Octaves);

        stateB.scroll.scrollSpeedX = stateA.scroll.scrollSpeedX;
        stateB.scroll.scrollSpeedY = stateA.scroll.scrollSpeedY;
    }

    [ContextMenu("StateAB/Apply A instantly")]
    public void ApplyAInstant()
    {
        ApplySnapshot(stateA);
    }

    [ContextMenu("StateAB/Apply B instantly")]
    public void ApplyBInstant()
    {
        ApplySnapshot(stateB);
    }
    #endregion


    #region Transition core
    private void StartTransition(bool aToB, float duration)
    {
        if (_transitionCo != null)
        {
            StopCoroutine(_transitionCo);
            _transitionCo = null;
        }

        _isAToB = aToB;
        _transitionCo = StartCoroutine(Co_Transition(aToB, duration));
    }

    private System.Collections.IEnumerator Co_Transition(bool aToB, float duration)
    {
        if (duration <= 0f)
        {

            ApplySnapshot(aToB ? stateB : stateA);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            float u = t / duration; 
            ApplyLerp(u, aToB);
            t += Time.deltaTime;
            yield return null;
        }

     
        ApplyLerp(1f, aToB);


        _transitionCo = null;
    }

    private void ApplyLerp(float u, bool aToB)
    {
        var from = aToB ? stateA : stateB;
        var to   = aToB ? stateB : stateA;


        if (TryGetMaterial(out var mat, out var colorProp))
        {
            var c = Color.Lerp(from.material.baseColor, to.material.baseColor, u);
            SetMaterialColor(mat, colorProp, c);
        }


        if (targetScroll != null)
        {
            targetScroll.scrollSpeedX = Mathf.Lerp(from.scroll.scrollSpeedX, to.scroll.scrollSpeedX, u);
            targetScroll.scrollSpeedY = Mathf.Lerp(from.scroll.scrollSpeedY, to.scroll.scrollSpeedY, u);
        }


        if (targetWaves != null)
        {
 
            targetWaves.uvScale = Mathf.Lerp(from.waves.uvScale, to.waves.uvScale, u);


            LerpOctavesIntoTarget(targetWaves, from.waves.Octaves, to.waves.Octaves, u);


            if (u >= 1f)
                targetWaves.Dimension = to.waves.Dimension;
        }
    }

    private void ApplySnapshot(Snapshot snap)
    {
 
        if (TryGetMaterial(out var mat, out var colorProp))
        {
            SetMaterialColor(mat, colorProp, snap.material.baseColor);
        }


        if (targetWaves != null)
        {
            targetWaves.uvScale = snap.waves.uvScale;
            targetWaves.Dimension = snap.waves.Dimension;
            
            if (snap.waves.Octaves != null)
                targetWaves.Octaves = DeepCopy(snap.waves.Octaves);
        }


        if (targetScroll != null)
        {
            targetScroll.scrollSpeedX = snap.scroll.scrollSpeedX;
            targetScroll.scrollSpeedY = snap.scroll.scrollSpeedY;
        }
    }
    #endregion


    #region Helpers - Materials
    private void AutoAssignTargets()
    {
        if (!targetRenderer) targetRenderer = GetComponent<MeshRenderer>();
        if (!targetWaves)    targetWaves    = GetComponent<Waves>();
        if (!targetScroll)   targetScroll   = GetComponent<ScrollTexture>();
    }

    private bool TryGetMaterial(out Material mat, out string colorProp)
    {
        mat = null;
        colorProp = null;

        if (!targetRenderer) return false;

        var mats = targetRenderer.materials;
        if (mats == null || mats.Length == 0) return false;

        int idx = Mathf.Clamp(materialIndex, 0, mats.Length - 1);
        mat = mats[idx];

        colorProp = ResolveColorProperty(mat);
        return !string.IsNullOrEmpty(colorProp) && mat.HasProperty(colorProp);
    }

    private string ResolveColorProperty(Material mat)
    {
        switch (colorPropertyMode)
        {
            case ColorPropertyMode.BaseColor:
                return "_BaseColor";
            case ColorPropertyMode.Color:
                return "_Color";
            case ColorPropertyMode.Custom:
                return string.IsNullOrEmpty(customColorProperty) ? "_BaseColor" : customColorProperty;
            case ColorPropertyMode.Auto:
            default:
                if (mat != null)
                {
                    if (mat.HasProperty("_BaseColor")) return "_BaseColor";
                    if (mat.HasProperty("_Color"))     return "_Color";
                }
                return "_BaseColor";
        }
    }

    private static Color GetMaterialColor(Material mat, string prop)
    {
        return mat.GetColor(prop);
    }

    private static void SetMaterialColor(Material mat, string prop, Color c)
    {
        mat.SetColor(prop, c);
        if (prop == "_Color") mat.color = c;
        if (prop == "_BaseColor") mat.SetColor("_BaseColor", c);
    }
    #endregion

    #region Helpers - Waves Octaves
    private static Waves.Octave[] DeepCopy(Waves.Octave[] src)
    {
        if (src == null) return null;
        var dst = new Waves.Octave[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            dst[i].speed     = src[i].speed;
            dst[i].scale     = src[i].scale;
            dst[i].height    = src[i].height;
            dst[i].alternate = src[i].alternate;
        }
        return dst;
    }

    private static void LerpOctavesIntoTarget(Waves target, Waves.Octave[] from, Waves.Octave[] to, float u)
    {
        if (target == null) return;

        int nFrom = from?.Length ?? 0;
        int nTo   = to?.Length   ?? 0;
        int n     = Mathf.Min(nFrom, nTo);
        
        if (target.Octaves == null || target.Octaves.Length != n)
            target.Octaves = new Waves.Octave[n];

        for (int i = 0; i < n; i++)
        {
            var f = from[i];
            var t = to[i];

            Waves.Octave o;
            o.speed     = Vector2.Lerp(f.speed, t.speed, u);
            o.scale     = Vector2.Lerp(f.scale, t.scale, u);
            o.height    = Mathf.Lerp(f.height, t.height, u);
            o.alternate = (u < 0.5f) ? f.alternate : t.alternate;

            target.Octaves[i] = o;
        }

     
    }
    #endregion
}
