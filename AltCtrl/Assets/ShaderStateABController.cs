using System;
using UnityEngine;
using DG.Tweening; 

[DisallowMultipleComponent]
public class ShaderStateABController : MonoBehaviour
{
    [Header("Cible")]
    public MeshRenderer targetRenderer;
    [Tooltip("Index du material à piloter sur le MeshRenderer.")]
    public int materialIndex = 0;

    [Header("Timing")]
    [Min(0f)] public float defaultDuration = 1f;
    [Tooltip("Courbe d'assouplissement (1D) appliquée au t [0..1]. Laissez vide pour linéaire.")]
    public AnimationCurve ease = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Options d'init")]
    public bool captureAOnStart = true;
    public bool initBFromAOnStart = true;

    [Header("Texture switching")]
    [Tooltip("Si true, la texture _MainTex est remplacée quand u>=1 (fin transition). Sinon, switch immédiat au début.")]
    public bool switchTextureAtEnd = true;

    [Header("Debug R")]
    [Tooltip("Cible utilisée quand on appuie la touche R (peut rester vide si vous appelez la méthode ResetBlurAndTilt(go) vous-même).")]
    public GameObject defaultTweenTargetForR;
    [Tooltip("Remet le Z local à 0 avant la rotation R.")]
    public bool resetZtoZeroBeforeTween = true;

  
    private static readonly int ID_MainTex    = Shader.PropertyToID("_MainTex");
    private static readonly int ID_BaseColor  = Shader.PropertyToID("_BaseColor");
    private static readonly int ID_Size       = Shader.PropertyToID("_Size");
    private static readonly int ID_T          = Shader.PropertyToID("_T");
    private static readonly int ID_Distortion = Shader.PropertyToID("_Distortion");
    private static readonly int ID_Blur       = Shader.PropertyToID("_Blur");

    [Serializable]
    public class Snapshot
    {
        [Header("Texture (_MainTex)")]
        public Texture mainTex;
        public Vector2 mainTexTiling = Vector2.one;
        public Vector2 mainTexOffset = Vector2.zero;

        [Header("Couleur (_BaseColor) - A = Opacité")]
        public Color baseColor = Color.white;

        [Header("Floats")]
        public float size = 1f;       
        public float t = 1f;          
        public float distortion = 0f; 
        public float blur = 1f;       
    }

    [Header("Etat A (capturé)")]
    public Snapshot stateA = new Snapshot();

    [Header("Etat B (cible)")]
    public Snapshot stateB = new Snapshot();

    private Coroutine _transitionCo;
    private Coroutine _blurCo;
    private Coroutine _distortionCo;


    #region Unity
    private void Reset()
    {
        if (!targetRenderer) targetRenderer = GetComponent<MeshRenderer>();
    }

    private void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        if (captureAOnStart) CaptureAFromMaterial();
        if (initBFromAOnStart) CopyAtoB();
    }

    public void wiper()
    {
        if (defaultTweenTargetForR != null)
            ResetBlurAndTilt(defaultTweenTargetForR);
        else
            ResetBlurAndTilt(null);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.T)) GoAToB(defaultDuration);
        if (Input.GetKeyDown(KeyCode.Y)) GoBToA(defaultDuration);

        
        if (Input.GetKeyDown(KeyCode.R))
        {
            wiper();
        }
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


    // --- remplace entièrement la méthode ---
    public void ResetBlurAndTilt(GameObject tweenTarget)
    {
        // Stoppe la transition A<->B
        if (_transitionCo != null)
        {
            StopCoroutine(_transitionCo);
            _transitionCo = null;
        }

        // Stoppe les coroutines en cours
        if (_blurCo != null)       { StopCoroutine(_blurCo);       _blurCo = null; }
        if (_distortionCo != null) { StopCoroutine(_distortionCo); _distortionCo = null; }

        if (TryGetTargetMaterial(out var mat))
        {
            // Lancer Blur -> 0 en 1s
            if (mat.HasProperty(ID_Blur))
            {
                float startBlur = mat.GetFloat(ID_Blur);
                _blurCo = StartCoroutine(Co_FloatToZero(mat, ID_Blur, startBlur, 1f));
            }

            // Lancer Distortion -> 0 en 1s  (NOUVEAU)
            if (mat.HasProperty(ID_Distortion))
            {
                float startDist = mat.GetFloat(ID_Distortion);
                _distortionCo = StartCoroutine(Co_FloatToZero(mat, ID_Distortion, startDist, 1f));
            }
        }

        // DOTween sur la cible (0 -> -85 -> 0 sur 2s)
        if (tweenTarget != null)
        {
            var tr = tweenTarget.transform;
            tr.DOKill(true);

            if (resetZtoZeroBeforeTween)
            {
                var e = tr.localEulerAngles; e.z = 0f; tr.localEulerAngles = e;
            }

            var startEuler = tr.localEulerAngles;
            var downEuler  = new Vector3(startEuler.x, startEuler.y, 85f);

            DG.Tweening.Sequence seq = DOTween.Sequence();
            seq.Append(tr.DOLocalRotate(downEuler, 1f, RotateMode.Fast).SetEase(Ease.OutQuad));
            seq.Append(tr.DOLocalRotate(startEuler, 1f, RotateMode.Fast).SetEase(Ease.InQuad));
        }
    }

    #endregion

// --- nouveau helper générique : float -> 0 en 'duration' ---
    private System.Collections.IEnumerator Co_FloatToZero(Material mat, int propId, float start, float duration)
    {
        if (!mat || !mat.HasProperty(propId)) yield break;

        float t = 0f;
        while (t < duration)
        {
            float u = t / duration;
            mat.SetFloat(propId, Mathf.Lerp(start, 0f, u));
            t += Time.deltaTime;
            yield return null;
        }
        mat.SetFloat(propId, 0f);
    }

    
    #region Capture / Apply / Copy (ContextMenu)
    [ContextMenu("ShaderAB/Capture A From Material")]
    public void CaptureAFromMaterial()
    {
        if (!TryGetTargetMaterial(out var mat)) return;

        if (mat.HasProperty(ID_MainTex))
        {
            stateA.mainTex       = mat.GetTexture(ID_MainTex);
            stateA.mainTexTiling = mat.GetTextureScale(ID_MainTex);
            stateA.mainTexOffset = mat.GetTextureOffset(ID_MainTex);
        }

        if (mat.HasProperty(ID_BaseColor))
            stateA.baseColor = mat.GetColor(ID_BaseColor);

        if (mat.HasProperty(ID_Size))       stateA.size       = mat.GetFloat(ID_Size);
        if (mat.HasProperty(ID_T))          stateA.t          = mat.GetFloat(ID_T);
        if (mat.HasProperty(ID_Distortion)) stateA.distortion = mat.GetFloat(ID_Distortion);
        if (mat.HasProperty(ID_Blur))       stateA.blur       = mat.GetFloat(ID_Blur);
    }

    [ContextMenu("ShaderAB/Copy A -> B (overwrite)")]
    public void CopyAtoB()
    {
        stateB.mainTex       = stateA.mainTex;
        stateB.mainTexTiling = stateA.mainTexTiling;
        stateB.mainTexOffset = stateA.mainTexOffset;

        stateB.baseColor = stateA.baseColor;

        stateB.size       = stateA.size;
        stateB.t          = stateA.t;
        stateB.distortion = stateA.distortion;
        stateB.blur       = stateA.blur;
    }

    [ContextMenu("ShaderAB/Apply A instantly")]
    public void ApplyAInstant()
    {
        ApplySnapshot(stateA);
    }

    [ContextMenu("ShaderAB/Apply B instantly")]
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
        _transitionCo = StartCoroutine(Co_Transition(aToB, duration));
    }

    private System.Collections.IEnumerator Co_Transition(bool aToB, float duration)
    {
        var from = aToB ? stateA : stateB;
        var to   = aToB ? stateB : stateA;

        if (!TryGetTargetMaterial(out var mat)) yield break;

        bool texturesDiffer = from.mainTex != to.mainTex;
        if (texturesDiffer && !switchTextureAtEnd)
        {
            if (mat.HasProperty(ID_MainTex)) mat.SetTexture(ID_MainTex, to.mainTex);
        }

        if (duration <= 0f)
        {
            ApplySnapshot(to);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            float u = t / duration;
            float eased = ease != null ? Mathf.Clamp01(ease.Evaluate(u)) : u;
            ApplyLerp(mat, from, to, eased, texturesDiffer);
            t += Time.deltaTime;
            yield return null;
        }

        ApplyLerp(mat, from, to, 1f, texturesDiffer);

        if (texturesDiffer && switchTextureAtEnd && mat.HasProperty(ID_MainTex))
            mat.SetTexture(ID_MainTex, to.mainTex);

        _transitionCo = null;
    }

    private void ApplyLerp(Material mat, Snapshot from, Snapshot to, float u, bool texturesDiffer)
    {
        if (!mat) return;

        if (mat.HasProperty(ID_BaseColor))
        {
            var c = Color.Lerp(from.baseColor, to.baseColor, u);
            mat.SetColor(ID_BaseColor, c);
        }

        if (mat.HasProperty(ID_Size))       mat.SetFloat(ID_Size,       Mathf.Lerp(from.size,       to.size,       u));
        if (mat.HasProperty(ID_T))          mat.SetFloat(ID_T,          Mathf.Lerp(from.t,          to.t,          u));
        if (mat.HasProperty(ID_Distortion)) mat.SetFloat(ID_Distortion, Mathf.Lerp(from.distortion, to.distortion, u));
        if (mat.HasProperty(ID_Blur))       mat.SetFloat(ID_Blur,       Mathf.Lerp(from.blur,       to.blur,       u));

        if (mat.HasProperty(ID_MainTex))
        {
            Vector2 til = Vector2.Lerp(from.mainTexTiling, to.mainTexTiling, u);
            Vector2 ofs = Vector2.Lerp(from.mainTexOffset, to.mainTexOffset, u);
            mat.SetTextureScale(ID_MainTex, til);
            mat.SetTextureOffset(ID_MainTex, ofs);
        }
    }

    private void ApplySnapshot(Snapshot s)
    {
        if (!TryGetTargetMaterial(out var mat)) return;

        if (mat.HasProperty(ID_MainTex))
        {
            mat.SetTexture(ID_MainTex, s.mainTex);
            mat.SetTextureScale(ID_MainTex, s.mainTexTiling);
            mat.SetTextureOffset(ID_MainTex, s.mainTexOffset);
        }

        if (mat.HasProperty(ID_BaseColor))  mat.SetColor(ID_BaseColor,  s.baseColor);
        if (mat.HasProperty(ID_Size))       mat.SetFloat(ID_Size,       s.size);
        if (mat.HasProperty(ID_T))          mat.SetFloat(ID_T,          s.t);
        if (mat.HasProperty(ID_Distortion)) mat.SetFloat(ID_Distortion, s.distortion);
        if (mat.HasProperty(ID_Blur))       mat.SetFloat(ID_Blur,       s.blur);
    }
    #endregion

    #region Helpers
    private System.Collections.IEnumerator Co_BlurToZero(Material mat, float start, float duration)
    {
        if (!mat.HasProperty(ID_Blur)) yield break;

        float t = 0f;
        while (t < duration)
        {
            float u = t / duration;
            mat.SetFloat(ID_Blur, Mathf.Lerp(start, 0f, u));
            t += Time.deltaTime;
            yield return null;
        }
        mat.SetFloat(ID_Blur, 0f);
        _blurCo = null;
    }

    private bool TryGetTargetMaterial(out Material mat)
    {
        mat = null;
        if (!targetRenderer)
        {
            Debug.LogWarning($"[{nameof(ShaderStateABController)}] Pas de MeshRenderer assigné.");
            return false;
        }

        var mats = targetRenderer.materials; 
        if (mats == null || mats.Length == 0)
        {
            Debug.LogWarning($"[{nameof(ShaderStateABController)}] Aucuns materials sur le renderer.");
            return false;
        }

        int idx = Mathf.Clamp(materialIndex, 0, mats.Length - 1);
        mat = mats[idx];

        if (targetRenderer.materials[idx] != mat)
        {
            mats[idx] = mat;
            targetRenderer.materials = mats;
        }

        return true;
    }
    #endregion
}
