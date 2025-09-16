using UnityEngine;
using System.Collections;

[AddComponentMenu("FX/Thunder Zone Spawner")]
[RequireComponent(typeof(MeshFilter))]
public class ThunderZoneSpawner : MonoBehaviour
{
    [Header("Prefab à instancier (SpriteRenderer + Animator)")]
    [Tooltip("Le préfab à faire apparaître aléatoirement dans la zone")]
    public GameObject thunderPrefab;

    [Header("Timing")]
    [Tooltip("Temps de base (secondes) entre deux instanciations")]
    public float baseInterval = 1.5f;
    [Tooltip("Borne A de l'aléa ajouté au temps de base (peut être négatif)")]
    public float randomAddMin = -0.5f;
    [Tooltip("Borne B de l'aléa ajouté au temps de base")]
    public float randomAddMax = 1.0f;
    [Tooltip("Utiliser le temps non-scalé (Ignore Time.timeScale)")]
    public bool useUnscaledTime = false;

    [Header("Durée de vie & hiérarchie")]
    [Tooltip("Chaque instance sera détruite après ce délai (secondes)")]
    public float prefabLifetime = 5f;
    [Tooltip("Parent optionnel pour ranger proprement les instances")]
    public Transform instancesParent;
    [Tooltip("Décalage local en Z pour éviter le z-fighting avec le quad")]
    public float localZOffset = 0.01f;

    [Header("Démarrage")]
    [Tooltip("Lancer automatiquement le spawn au OnEnable()")]
    public bool spawnOnEnable = true;

    private MeshFilter _meshFilter;
    private Coroutine _loop;

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    void OnEnable()
    {
        if (spawnOnEnable && thunderPrefab != null)
            _loop = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnOne();

            float jitter = Random.Range(randomAddMin, randomAddMax);
            float nextDelay = Mathf.Max(0.01f, baseInterval + jitter);

            if (useUnscaledTime)
                yield return WaitForSecondsUnscaled(nextDelay);
            else
                yield return new WaitForSeconds(nextDelay);
        }
    }


    public void SpawnOne()
    {
        if (thunderPrefab == null || _meshFilter == null || _meshFilter.sharedMesh == null)
            return;

    
        var meshBounds = _meshFilter.sharedMesh.bounds; 
        float lx = Random.Range(meshBounds.min.x, meshBounds.max.x);
        float ly = Random.Range(meshBounds.min.y, meshBounds.max.y);
        float lz = meshBounds.center.z + localZOffset;

        Vector3 localPoint = new Vector3(lx, ly, lz);
        Vector3 worldPoint = transform.TransformPoint(localPoint);

        Quaternion rot = thunderPrefab.transform.rotation;
        Transform parent = instancesParent != null ? instancesParent : null;

        GameObject inst = Instantiate(thunderPrefab, worldPoint, rot, parent);
        if (prefabLifetime > 0f)
            Destroy(inst, prefabLifetime);
    }

  
    IEnumerator WaitForSecondsUnscaled(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_meshFilter == null || _meshFilter.sharedMesh == null) return;

        var b = _meshFilter.sharedMesh.bounds;
        Vector3[] corners =
        {
            new Vector3(b.min.x, b.min.y, b.center.z),
            new Vector3(b.max.x, b.min.y, b.center.z),
            new Vector3(b.max.x, b.max.y, b.center.z),
            new Vector3(b.min.x, b.max.y, b.center.z)
        };

        for (int i = 0; i < corners.Length; i++)
            corners[i] = transform.TransformPoint(corners[i]);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < 4; i++)
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
    }
#endif
}
