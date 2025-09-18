using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Pooling : MonoBehaviour
{
    [SerializeField] GameObject Montagne1;
    [SerializeField] GameObject Montagne2;
    [SerializeField] Transform spawnOriginLeft;
    [SerializeField] Transform spawnOriginRight;
    public List<GameObject> pool = new();
    [SerializeField] private float spawnInterval = 1;
    private float lastSpawn;
    private bool canSpawn = true;

    private void Update()
    {
        if (Time.time - GameManager.INSTANCE.startTime > GameManager.INSTANCE.timeBeforeLanding)
        {
            canSpawn = false;
        }
        if (canSpawn)
        {
            if (Time.time - lastSpawn > spawnInterval)
            {
                SpawnObject();
                lastSpawn = Time.time;
            }
        }
    }

    private void SpawnObject()
    {
        if (pool.Count <= 0)
        {
            int r = Random.Range(0, 2);
            GameObject obstacle = null;
            if (r == 0)
            {
                obstacle = Montagne1;
            }

            if (r == 1)
            {
                obstacle = Montagne2;
            }

            r = Random.Range(0, 2);
            Transform t = null;
            
            if (r == 0)
            {
                t = spawnOriginLeft;
            }
            if (r == 1)
            {
                t = spawnOriginRight;
            }

            float v = Random.Range(0, 360);
            Instantiate(obstacle, t.position, Quaternion.Euler(0, v, 0));
        }
        /*else
        {
            GameObject obj = pool[0];
            pool.RemoveAt(0);
            obj.SetActive(true);
            obj.transform.position = spawnOrigin.position;
        }*/
        
    }
}
