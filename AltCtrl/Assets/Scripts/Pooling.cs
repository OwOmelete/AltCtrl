using System;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] Transform spawnOrigin;
    public List<GameObject> pool = new();
    [SerializeField] private float spawnInterval = 1;
    private float lastSpawn;

    private void Update()
    {
        if (Time.time - lastSpawn > spawnInterval)
        {
            SpawnObject();
            lastSpawn = Time.time;
        }
    }

    private void SpawnObject()
    {
        if (pool.Count <= 0)
        {
            Instantiate(prefab, spawnOrigin);
        }
        else
        {
            GameObject obj = pool[0];
            pool.RemoveAt(0);
            obj.SetActive(true);
            obj.transform.position = spawnOrigin.position;
        }
        
    }
}
