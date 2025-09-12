using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE;
    private List<AbstractMiniGame> miniGamesList = new();
    private float miniGameInterval;
    [SerializeField] private GameObject obstacle;
    [SerializeField] private Transform spawnOrigin;
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

    private void Awake()
    {
        INSTANCE = this;
    }

    private void Start()
    {
        miniGamesList = MiniGamesPhase1.ToList();
        miniGameInterval = MiniGameIntervalPhase1;
        LaunchMiniGame();
    }

    private void Update()
    {
        if (Time.time - lastSpawn > spawnInterval)
        {
            SpawnObstacle();
            lastSpawn = Time.time;
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
        Instantiate(obstacle, getPosition(r), Quaternion.identity);
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
            int r = Random.Range(0, miniGamesList.Count);
            miniGamesList[r].enabled = true;
        }
    }
}
