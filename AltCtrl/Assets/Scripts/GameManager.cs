using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE;
    [HideInInspector] public bool savon = false;
    [SerializeField] private List<AbstractMiniGame> miniGamesList = new();
    [SerializeField] private GameObject obstacle;
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private float spawnInterval;
    [SerializeField] private driving driving;
    private float lastSpawn;

    private void Awake()
    {
        INSTANCE = this;
    }

    private void Start()
    {
        LaunchMiniGame();
    }

    private void Update()
    {
        if (Time.time - lastSpawn > spawnInterval)
        {
            SpawnObstacle();
            lastSpawn = Time.time;
        }
    }
    
    private Vector3 getPosition(int r)
    {
        Vector3 direction = Vector3.zero;
        if (r == 0)
        {
        }
        if (r == 1)
        {
            direction = Vector3.right;
        }
        if (r == 2)
        {
            direction = Vector3.left;
        }
        if (r == 3)
        {
            direction = Vector3.down;
        }
        if (r == 4)
        {
            direction = Vector3.up;
        }
        return spawnOrigin.position + direction * driving.displacement;
    }

    private void SpawnObstacle()
    {
        int r = Random.Range(0, 5);
        Instantiate(obstacle, getPosition(r), Quaternion.identity);
    }

    IEnumerator timer()
    {
        yield return new WaitForSeconds(120);
        spawnInterval = 6;
        yield return new WaitForSeconds(120);
        spawnInterval = 4;
    }

    private void LaunchMiniGame()
    {
        int r = Random.Range(0, miniGamesList.Count);
        miniGamesList[r].enabled = true;
    }
}
