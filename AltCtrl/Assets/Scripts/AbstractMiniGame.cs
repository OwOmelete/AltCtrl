using System;
using UnityEngine;

public abstract class AbstractMiniGame : MonoBehaviour
{
    protected abstract void MiniGameStart();

    protected abstract void MiniGameUpdate();

    public void Update()
    {
        MiniGameUpdate();
    }

    public void OnEnable()
    {
        MiniGameStart();
    }

    public abstract void Win();
}
