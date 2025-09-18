using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class driving : MonoBehaviour
{
    public float displacement;
    private bool grounded = false;
    [SerializeField] private int pv = 3;
    private Collider lastObstacle;
    private bool isLanding = false;
    [SerializeField] private float landingSpeed;
    [SerializeField] private float landingDisplacement;
    [SerializeField] private float landingDuration;
    [SerializeField] private float MovementSpeed;
    [SerializeField] private Transform centerPosition;
    [SerializeField] private Image[] lifeDisplay;

    [Header("collision")]
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float shakeIntensity = 0.6f;
    [SerializeField] private float shakeDuration = 0.25f;

    List<string> clips = new List<string> { "Sound_damage" };
    List<string> clips2 = new List<string> { "VoiceLine Contact Imminent 3 2 1" };
    
    // Interne
    private Tween _camShakeTween;
    private bool canLand = false;
    private bool wheelsOut = false;
    private bool isBraking = false;
    [SerializeField] private TextureScroller ground;
    [SerializeField] private Transform groundStart;
    [SerializeField] private Transform groundEndLose;
    [SerializeField] private Transform groundEndWin;
    [SerializeField] private GameObject prefabTree;
    private bool hasSpawnTree;
    private bool instantDeath;

    private void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
    }

    private void Start()
    {
        foreach (var img in lifeDisplay)
        {
            if (img) img.enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBraking = true;
        }
        if (Time.time - GameManager.INSTANCE.startTime > GameManager.INSTANCE.timeBeforeLanding)
        {
            canLand = true;
        }
        if (Input.GetKeyDown(KeyCode.Space) && !grounded && canLand)
        {
            grounded = true;
            isLanding = true;
            centerPosition.DOMoveY(centerPosition.position.y - landingDisplacement, landingDuration)
                .SetEase(Ease.InOutQuad).OnComplete(landed);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            wheelsOut = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            wheelsOut = false;
        }
        if (isLanding)
        {
            transform.position = centerPosition.position;
        }

        if (Time.time - GameManager.INSTANCE.startTime > GameManager.INSTANCE.timeBeforeBrake)
        {
            if (isBraking)
            {
                Debug.Log("frein");
                ground.targetSpeed = math.lerp(ground.targetSpeed, 0, Time.deltaTime);
                if (hasSpawnTree == false)
                {
                    GameObject tree = Instantiate(prefabTree, groundStart);
                    hasSpawnTree = true;
                }
                else
                {
                    groundStart.transform.GetChild(0).position =  math.lerp(groundStart.transform.GetChild(0).position, groundEndWin.position, Time.deltaTime);
                }
                
                
                
            }
            else
            {
                
                Debug.Log("pas frein");
                ground.targetSpeed = math.lerp(ground.targetSpeed, 0, Time.deltaTime);
                if (hasSpawnTree == false)
                {
                    GameObject tree = Instantiate(prefabTree, groundStart);
                    hasSpawnTree = true;
                }
                else
                {
                    if (groundStart.transform.GetChild(0).position != null)
                    {
                        groundStart.transform.GetChild(0).position =  math.lerp(groundStart.transform.GetChild(0).position, groundEndLose.position, Time.deltaTime);
                    }
                    
                }
            }
        }
    }

    


    private void landed()
    {
        if (!wheelsOut)
        {
            pv -= 1;
            DoCollisionFeedback(null);
        }
        GameManager.INSTANCE.canSpawnObstacle = true;
        GameManager.INSTANCE.landed = true;
        Debug.Log(GameManager.INSTANCE.canSpawnObstacle);
        GameManager.INSTANCE.spawnInterval = 1;
        isLanding = false;
    }

    private void FixedUpdate()
    {
        Vector3 pos = new Vector3();
        if (!isLanding)
        {
            if (grounded)
            {
                pos += centerPosition.position + new Vector3(Input.GetAxisRaw("Horizontal") * displacement, 0, 0);
            }
            else
            {
                pos += centerPosition.position + new Vector3(Input.GetAxisRaw("Horizontal") * displacement, Input.GetAxisRaw("Vertical") * displacement, 0);
            }
            transform.position = Vector3.Lerp(transform.position, pos, MovementSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "instantDeath" && instantDeath == false)
        {
            pv -= 3;
            Debug.Log("instantDeath");
            // faire explosion et MORT
            instantDeath = true;
        }
        else
        {
            Destroy(other.gameObject);
            if (lastObstacle != other)
            {
                pv -= 1;
                Debug.Log("pv : " + pv);
                lastObstacle = other;
            
                int idx = Mathf.Clamp(2 - pv, 0, lifeDisplay.Length - 1);
                if (lifeDisplay != null && lifeDisplay.Length > 0 && lifeDisplay[idx] != null)
                    lifeDisplay[idx].enabled = true;
                SoundManager.Instance.PlayRandomSFX(clips, 1f, 1f);
            }
        
            DoCollisionFeedback(other);
        }
        

    }

    private void DoCollisionFeedback(Collider other)
    {
        if (targetCamera != null)
        {
            _camShakeTween?.Kill(true);
            
            int vibrato = 18;
            float randomness = 90f;
            
            _camShakeTween = targetCamera.transform.DOShakePosition(
                duration: shakeDuration,
                strength: new Vector3(shakeIntensity, shakeIntensity, 0f),
                vibrato: vibrato,
                randomness: randomness,
                snapping: false,
                fadeOut: true
            );
        }
        
        if (spawnPrefab != null)
        {
            Vector3 spawnPos;
            Quaternion spawnRot;

            if (spawnPoint != null)
            {
                spawnPos = spawnPoint.position;
                spawnRot = spawnPoint.rotation;
            }
            else
            {
                if (other != null)
                {
                    spawnPos = other != null ? other.ClosestPoint(transform.position) : transform.position;
                    spawnRot = Quaternion.identity;
                    Instantiate(spawnPrefab, spawnPos, spawnRot , spawnPoint);
                }
            }
        }
    }

    IEnumerator landing()
    {
        isLanding = true;
        yield return new WaitForSeconds(3);
        isLanding = false;
    }
}
