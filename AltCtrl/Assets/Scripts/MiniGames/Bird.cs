using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MiniGames
{
    public class Bird : AbstractMiniGame
    {
        [Header("Parents / Positionnement")]
        public Transform defaultSpawnRoot;
        public List<Transform> spawnRoots = new();

        [Header("Prefabs (Etape 1, 2, et 3)")]

        public GameObject firstStagePrefab;
        public GameObject birdPrefab;
        public GameObject thirdPrefabOnReplace;
        public bool thirdAsChildOfBird = false;

        [Header("Etape 1 - Apparition progressive")]
        public float firstTargetScale = 1f;
        public float firstScaleDuration = 0.75f;
        public Ease firstScaleEase = Ease.OutBack;

        [Header("Etape 2 - Squishy du pigeon")]
        public float squishDuration = 1f;
        [Range(0f, 0.6f)] public float squishAmount = 0.2f;

        [Header("Spawns pendant le squish")]
        public List<GameObject> spawnCandidates = new();
        public int spawnCount = 6;
        public Vector2 randomX = new(-2f, 2f);
        public Vector2 randomY = new(1f, 3f);
        public float spawnTravelTime = 0.8f;
        public Ease spawnTravelEase = Ease.OutQuad;
        public float spawnTotalZDegrees = 720f;
        public Vector2 spawnZDegreesRandomAdd = new(-180f, 180f);

        [Header("Balayage / Remplacement")]
        public float sweepSpeed = 6f;
        public Vector2 sweepExtraSpeedRange = new(0f, 3f);
        public float sweepScaleXGainPerSecond = 0.1f;
        public GameObject sweepReplacementPrefab;
        public Transform sweepReplacementSpawnPoint;
        public Transform sweepReplacementParent;

        private bool birdOnScreen = false;
        private Transform firstInst;
        private Transform birdInst;
        private Sequence running;
        private Transform chosenRoot;

        private readonly List<Transform> spawnedInstances = new();
        [SerializeField] private GameObject picto;

        protected override void MiniGameStart()
        {
            picto.SetActive(true);
            chosenRoot = ChooseRoot();
            birdOnScreen = true;
            StartCoroutine(RunFlow());
        }

        private Transform ChooseRoot()
        {
            var valids = new List<Transform>();
            if (spawnRoots != null && spawnRoots.Count > 0)
            {
                foreach (var t in spawnRoots) if (t) valids.Add(t);
            }
            if (valids.Count > 0) return valids[Random.Range(0, valids.Count)];
            if (defaultSpawnRoot) return defaultSpawnRoot;
            return transform;
        }

        private IEnumerator RunFlow()
        {
            KillTweensAndCleanup();

            var parent = chosenRoot ? chosenRoot : transform;

            if (!firstStagePrefab || !birdPrefab)
            {
                yield break;
            }

         
            var go1 = Instantiate(firstStagePrefab, parent.position, parent.rotation, parent);
            firstInst = go1.transform;
            firstInst.localScale = Vector3.zero;
            yield return firstInst.DOScale(firstTargetScale, Mathf.Max(0.01f, firstScaleDuration))
                                  .SetEase(firstScaleEase)
                                  .WaitForCompletion();

            Vector3 pos = firstInst.position;
            Quaternion rot = firstInst.rotation;
            Transform par = firstInst.parent;
            Destroy(go1);
            firstInst = null;

            var go2 = Instantiate(birdPrefab, pos, rot, par);
            birdInst = go2.transform;


            if (thirdPrefabOnReplace)
            {
                Transform thirdParent = thirdAsChildOfBird ? birdInst : par;
                var go3 = Instantiate(thirdPrefabOnReplace, pos, rot, thirdParent);

                go3.transform.localScale = birdInst.localScale;
            }

          
            running = DOTween.Sequence();
            Vector3 baseScale = birdInst.localScale;
            float a = Mathf.Clamp01(squishAmount);

            running.Append(birdInst.DOScale(new Vector3(baseScale.x * (1f + a), baseScale.y * (1f - a), baseScale.z),
                                            squishDuration * 0.5f).SetEase(Ease.OutQuad));
            running.Append(birdInst.DOScale(baseScale, squishDuration * 0.5f).SetEase(Ease.OutBack));

            int n = Mathf.Max(0, spawnCount);
            for (int i = 0; i < n; i++)
            {
                float when = (squishDuration / Mathf.Max(n, 1)) * i + 0.01f;
                running.InsertCallback(when, () => SpawnOneFrom(birdInst ? birdInst.position : parent.position, parent));
            }

            yield return running.WaitForCompletion();
        }

        private void SpawnOneFrom(Vector3 start, Transform parent)
        {
            if (spawnCandidates == null || spawnCandidates.Count == 0) return;

            int idx = Random.Range(0, spawnCandidates.Count);
            var prefab = spawnCandidates[idx];
            if (!prefab) return;

            var go = Instantiate(prefab, start, Quaternion.identity, parent);


            Vector2 xr = randomX; if (xr.x > xr.y) (xr.x, xr.y) = (xr.y, xr.x);
            Vector2 yr = randomY; if (yr.x > yr.y) (yr.x, yr.y) = (yr.y, yr.x);
            Vector3 target = start + new Vector3(Random.Range(xr.x, xr.y), Random.Range(yr.x, yr.y), 0f);


            float zMin = Mathf.Min(spawnZDegreesRandomAdd.x, spawnZDegreesRandomAdd.y);
            float zMax = Mathf.Max(spawnZDegreesRandomAdd.x, spawnZDegreesRandomAdd.y);
            float extraZ = Random.Range(zMin, zMax);

            var mover = go.GetComponent<MiniSpawnMover>();
            if (!mover) mover = go.AddComponent<MiniSpawnMover>();

            mover.Launch(
                startPos: start,
                targetPos: target,
                travelTime: Mathf.Max(0.05f, spawnTravelTime),
                moveEase: spawnTravelEase,
                totalZDegrees: spawnTotalZDegrees + extraZ
            );
            
            spawnedInstances.Add(go.transform);
        }

        private void KillTweensAndCleanup()
        {
            running?.Kill();
            running = null;

            if (firstInst)
            {
                firstInst.DOKill(); Destroy(firstInst.gameObject); firstInst = null;
            }

            if (birdInst)
            {
                birdInst.DOKill();  Destroy(birdInst.gameObject);  birdInst  = null;
            }
        }


        private void StartSweepBirdAndSpawnReplacement()
        {
            SpawnSweepReplacement();

            if (birdInst)
            {
                birdInst.DOKill();
                var t = birdInst;
                birdInst = null;
                StartCoroutine(SweepToRightAndDestroy(t, sweepSpeed));
            }
        }

        private void StartSweepSpawnedAndSpawnReplacement()
        {
            SpawnSweepReplacement();

            if (spawnedInstances.Count > 0)
            {
                var copy = new List<Transform>(spawnedInstances);
                foreach (var tr in copy)
                {
                    if (!tr) { spawnedInstances.Remove(tr); continue; }
                    tr.DOKill();

                    float min = Mathf.Min(sweepExtraSpeedRange.x, sweepExtraSpeedRange.y);
                    float max = Mathf.Max(sweepExtraSpeedRange.x, sweepExtraSpeedRange.y);
                    float extra = Random.Range(min, max);
                    float finalSpeed = Mathf.Max(0.1f, sweepSpeed + extra);

                    StartCoroutine(SweepToRightAndDestroy(tr, finalSpeed, () =>
                    {
                        spawnedInstances.Remove(tr);
                    }));
                }
            }
        }

        private void SpawnSweepReplacement()
        {
            if (!sweepReplacementPrefab) return;

            Transform parent = sweepReplacementParent ? sweepReplacementParent :
                               (chosenRoot ? chosenRoot : transform);

            Vector3 pos = sweepReplacementSpawnPoint ? sweepReplacementSpawnPoint.position : parent.position;
            Instantiate(sweepReplacementPrefab, pos, Quaternion.identity, parent);
        }

        private IEnumerator SweepToRightAndDestroy(Transform target, float speed, System.Action onDone = null)
        {
            if (!target) yield break;

            bool HasAnyRendererVisible()
            {
                var rs = target.GetComponentsInChildren<Renderer>();
                if (rs != null && rs.Length > 0)
                {
                    foreach (var r in rs) if (r && r.isVisible) return true;
                    return false;
                }
                var cam = Camera.main;
                if (!cam) return false;
                var v = cam.WorldToViewportPoint(target.position);
                return v.z > 0f && v.x > 0f && v.x < 1f && v.y > 0f && v.y < 1f;
            }

            while (target && HasAnyRendererVisible())
            {
                target.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

                // Gain de scale X par seconde pendant le balayage
                Vector3 ls = target.localScale;
                ls.x += sweepScaleXGainPerSecond * Time.deltaTime;
                target.localScale = ls;

                yield return null;
            }

            if (target) Destroy(target.gameObject);
            onDone?.Invoke();
        }

        protected override void MiniGameUpdate()
        {

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (birdOnScreen)
                {
                    StartSweepBirdAndSpawnReplacement();
                    Debug.Log("l'oiseau est parti mais il reste les plumes ^^");
                    birdOnScreen = false;
                }
                else
                {
                    StartSweepSpawnedAndSpawnReplacement();
                    Debug.Log("les plumes sont parties aussi :D");
                    Win();
                }
            }
        }

        public override void Win()
        {
            picto.SetActive(false);
            enabled = false;
        }
    }
    
    public class MiniSpawnMover : MonoBehaviour
    {
        private Tween moveT, rotT;

        public void Launch(Vector3 startPos, Vector3 targetPos, float travelTime, Ease moveEase, float totalZDegrees)
        {
            transform.position = startPos;
            moveT = transform.DOMove(targetPos, travelTime).SetEase(moveEase);
            rotT  = transform.DORotate(new Vector3(0f, 0f, totalZDegrees), travelTime, RotateMode.FastBeyond360)
                            .SetRelative(true)
                            .SetEase(Ease.Linear);
        }

        private void OnDisable()
        {
            moveT?.Kill();
            rotT?.Kill();
        }
    }
}
