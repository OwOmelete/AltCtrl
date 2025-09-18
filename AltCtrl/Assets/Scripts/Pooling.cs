using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Pooling : MonoBehaviour
{
    [SerializeField] GameObject Montagne1;
    [SerializeField] GameObject Montagne2;
    [SerializeField] GameObject vach;
    [SerializeField] GameObject fouin;
    [SerializeField] Transform spawnOriginLeft;
    [SerializeField] Transform spawnOriginRight;
    [SerializeField] Transform spawnOriginGround;
    public List<GameObject> pool = new();
    [SerializeField] private float spawnInterval = 1;
    private float lastSpawn;
    private bool canSpawn = true;

    private void Update()
    {
        if (GameManager.INSTANCE.canSpawnObstacle)
        {
            if (Time.time - lastSpawn > GameManager.INSTANCE.spawnInterval)
            {
                if (GameManager.INSTANCE.landed)
                {
                    SpawnGround();
                }
                else
                {
                    SpawnObject();
                }
                lastSpawn = Time.time;
            }
        }
    }
    
    private void SpawnGround()
    {
            int r = Random.Range(0, 2);
            GameObject obstacle = null;
            if (r == 0)
            {
                obstacle = fouin;
            }

            if (r == 1)
            {
                obstacle = vach;
            }

            r = Random.Range(0, 3);
            Vector3 t = spawnOriginGround.position;

            if (r == 0)
            {
            }

            if (r == 1)
            {
                t += Vector3.left*3;
            }

            if (r == 2)
            {
                t =Vector3.right*3;
            }

            float v = Random.Range(0, 360);
            Instantiate(obstacle, t, Quaternion.Euler(0, v, 0));
    }

    private void SpawnObject()
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
            
        /*else
        {
            GameObject obj = pool[0];
            pool.RemoveAt(0);
            obj.SetActive(true);
            obj.transform.position = spawnOrigin.position;
        }*/
    }
}
