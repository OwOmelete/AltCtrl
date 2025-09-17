using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Movement : MonoBehaviour
{
    private Vector3 startPos, endPos, departPos;
    public bool isMoving = false;
    public float MoveTime;
    private int level;
    public List<GameObject> levelparent;

    private void Start()
    {
        departPos = transform.localPosition;
        level = 0;
        levelparent[level].SetActive(true);
    }

    void Update()
    {
        float x = 0;
        float y = 0;

        if (Input.GetKey(KeyCode.J)) x = -1;
        if (Input.GetKey(KeyCode.L) && x == 0) x = 1;
        if (Input.GetKey(KeyCode.I) && x == 0 && y == 0) y = 1; 
        if (Input.GetKey(KeyCode.K) && x == 0 && y == 0) y = -1;

    
        if (!isMoving && (x != 0 || y != 0)) StartCoroutine(MovePlayer(new Vector3(x, y, 0f)));
    }

    IEnumerator MovePlayer(Vector3 dir)
    {
        isMoving = true;
        float nextMove = 0f;
        startPos = transform.localPosition;
        endPos = startPos + dir;
        Vector3 endPosWorld = transform.parent.TransformPoint(endPos);
        Vector3 startPosWorld = transform.parent.TransformPoint(startPos);
        Vector3 cameraPos = Camera.main.WorldToScreenPoint(endPosWorld);
        Ray ray = Camera.main.ScreenPointToRay(cameraPos);
        Debug.DrawRay(Camera.main.transform.position, endPosWorld,Color.red,0.5f);
        RaycastHit[] reyonx = Physics.RaycastAll(ray,Mathf.Infinity);
        bool gg = false;
       
        foreach(RaycastHit R in reyonx)
        {
            if (R.collider.tag == "obstacle")
            {
                isMoving = false;
                yield break;
            }
            else if (R.collider.tag == "arrivée")
            {
                Debug.Log("vous avez atteint la fin du niveau");
                gg = true;
                if (level <= 1)
                {
                    levelparent[level].SetActive(false);
                    level += 1;
                    levelparent[level].SetActive(true);
                }
                else if (level <= 2)
                {
                    Debug.Log("vous avez gagné");
                }
            }
            
        }
    
        while(nextMove < MoveTime)
        {
            transform.localPosition = Vector3.Lerp(startPos, endPos, nextMove / MoveTime);
            nextMove += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = endPos;

        isMoving = false;

        if(gg)
        {
            transform.localPosition = departPos;
        }
    }


}

