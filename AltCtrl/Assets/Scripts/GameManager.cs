using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE;
    private List<AbstractMiniGame> miniGamesList = new();
    private float miniGameInterval;
    [SerializeField] private GameObject obstacle;
    [SerializeField] private GameObject MountainLeft;
    [SerializeField] private GameObject MountainRight;
    [SerializeField] private GameObject MountainDown;
    [SerializeField] private GameObject vach;
    [SerializeField] private GameObject foin;
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private Transform spawnOriginLeft;
    [SerializeField] private Transform spawnOriginRight;
    [SerializeField] private Transform spawnOriginDown;
    [SerializeField] private Transform spawnOriginLanding;
    [SerializeField] private float spawnInterval;
    [SerializeField] private driving driving;
    [SerializeField] private int MiniGameIntervalPhase1;
    [SerializeField] private int MiniGameIntervalPhase2;
    [SerializeField] private int MiniGameIntervalPhase3;
    [SerializeField] private int MiniGameIntervalPhase4;
    [SerializeField] private int MiniGameIntervalPhase5;
    [SerializeField] private AbstractMiniGame[] MiniGamesPhase1;
    [SerializeField] private AbstractMiniGame[] MiniGamesPhase2;
    [SerializeField] private AbstractMiniGame[] MiniGamesPhase3;
    [SerializeField] private AbstractMiniGame[] MiniGamesPhase4;
    [SerializeField] private AbstractMiniGame[] MiniGamesPhase5;
    private float lastSpawn;
    private float lastMiniGame;
    [HideInInspector] public float startTime;
    [HideInInspector] public bool landed;
    public float timeBeforeObstacleSpawningStop = 300;
    public float timeBeforeLanding = 310;
    private bool canSpawnObstacle = true;
    
    private void Awake()
    {
        INSTANCE = this;
    }

    private void Start()
    {
        startTime = Time.time;
        spawnInterval = 8;
        miniGamesList = MiniGamesPhase1.ToList();
        miniGameInterval = MiniGameIntervalPhase1;
        LaunchMiniGame();
    }

    private void Update()
    {
        if (Time.time - startTime > timeBeforeObstacleSpawningStop)
        {
            canSpawnObstacle = false;
        }

        if (canSpawnObstacle)
        {
            if (Time.time - lastSpawn > spawnInterval)
            {
                SpawnObstacle();
                lastSpawn = Time.time;
            }
        }
        
        if (Time.time - lastMiniGame > miniGameInterval)
        {
            LaunchMiniGame();
            lastMiniGame = Time.time;
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
        Vector3 spawn = Vector3.zero;
        spawn = getPosition(r);
        GameObject obs = obstacle;
        if (r == 1)
        {
            obs = MountainLeft;
            spawn = spawnOriginLeft.position;
        }
        if (r == 2)
        {
            obs = MountainRight;
            spawn = spawnOriginRight.position;
        }
        Instantiate(obs, spawn, Quaternion.identity);
    }

    IEnumerator timer()
    {
        yield return new WaitForSeconds(60);
        miniGameInterval = MiniGameIntervalPhase2;
        miniGamesList = MiniGamesPhase2.ToList();
        yield return new WaitForSeconds(60);
        spawnInterval = 6;
        miniGameInterval = MiniGameIntervalPhase3;
        miniGamesList = MiniGamesPhase3.ToList();
        yield return new WaitForSeconds(60);
        miniGameInterval = MiniGameIntervalPhase4;
        miniGamesList = MiniGamesPhase4.ToList();
        yield return new WaitForSeconds(60);
        miniGameInterval = MiniGameIntervalPhase5;
        miniGamesList = MiniGamesPhase5.ToList();
        spawnInterval = 4;
    }

    private void LaunchMiniGame()
    {
        if (miniGamesList.Count > 0)
        {
            bool anyInactiveMinigame = false;
            foreach (var i in miniGamesList)
            {
                if (!i.isActiveAndEnabled)
                {
                    anyInactiveMinigame = true;
                }
            }

            if (anyInactiveMinigame)
            {
                int r = Random.Range(0, miniGamesList.Count);
                if (miniGamesList[r].isActiveAndEnabled)
                {
                    LaunchMiniGame();
                }
                else
                {
                    miniGamesList[r].enabled = true;
                }
            }
        }
    }
}
