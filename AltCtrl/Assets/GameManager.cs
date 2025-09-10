using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE;
    public bool savon = false;
    [SerializeField] private List<AbstractMiniGame> miniGamesList = new();

    private void Awake()
    {
        INSTANCE = this;
    }

    private void Start()
    {
        LaunchMiniGame();
    }

    private void LaunchMiniGame()
    {
        int r = Random.Range(0, miniGamesList.Count-1);
        miniGamesList[r].enabled = true;
    }
}
