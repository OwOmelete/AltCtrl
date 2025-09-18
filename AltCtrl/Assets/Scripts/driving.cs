using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
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

    // Interne
    private Tween _camShakeTween;

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
        if (Input.GetKeyDown(KeyCode.Space) && !grounded)
        {
            grounded = true;
            isLanding = true;
            centerPosition.DOMoveY(centerPosition.position.y - landingDisplacement, landingDuration)
                .SetEase(Ease.InOutQuad).OnComplete(landed);
        }
        if (isLanding)
        {
            transform.position = centerPosition.position;
        }
    }

    private void landed()
    {
        GameManager.INSTANCE.landed = true;
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
        Destroy(other.gameObject);
        if (lastObstacle != other)
        {
            pv -= 1;
            Debug.Log("pv : " + pv);
            lastObstacle = other;
            
            int idx = Mathf.Clamp(2 - pv, 0, lifeDisplay.Length - 1);
            if (lifeDisplay != null && lifeDisplay.Length > 0 && lifeDisplay[idx] != null)
                lifeDisplay[idx].enabled = true;
        }
        
        DoCollisionFeedback(other);
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
                spawnPos = other != null ? other.ClosestPoint(transform.position) : transform.position;
                spawnRot = Quaternion.identity;
            }

            Instantiate(spawnPrefab, spawnPos, spawnRot , spawnPoint);
        }
    }

    IEnumerator landing()
    {
        isLanding = true;
        yield return new WaitForSeconds(3);
        isLanding = false;
    }
}
