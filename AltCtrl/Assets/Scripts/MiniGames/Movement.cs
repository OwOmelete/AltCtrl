using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Movement : AbstractMiniGame
{
    private Vector3 startPos, endPos, departPos;
    public bool isMoving = false;
    public float MoveTime;
    private int level;
    public List<GameObject> levelparent;
    [SerializeField] private GameObject miniGameObject;
    [SerializeField] private GameObject player;

    protected override void MiniGameStart()
    {
        miniGameObject.SetActive(true);
        departPos = player.transform.localPosition;
        level = 0;
        levelparent[level].SetActive(true);
    }

    protected override void MiniGameUpdate()
    {
        float x = 0;
        float y = 0;

        if (Input.GetKey(KeyCode.J)) x = -1;
        if (Input.GetKey(KeyCode.L) && x == 0) x = 1;
        if (Input.GetKey(KeyCode.I) && x == 0 && y == 0) y = 1; 
        if (Input.GetKey(KeyCode.K) && x == 0 && y == 0) y = -1;

    
        if (!isMoving && (x != 0 || y != 0)) StartCoroutine(MovePlayer(new Vector3(x, y, 0f)));
    }

    public override void Win()
    {
        miniGameObject.SetActive(false);
        enabled = false;
    }

    IEnumerator MovePlayer(Vector3 dir)
    {
        isMoving = true;
        float nextMove = 0f;
        startPos = player.transform.localPosition;
        endPos = startPos + dir;
        Vector3 endPosWorld = player.transform.parent.TransformPoint(endPos);
        Vector3 startPosWorld = player.transform.parent.TransformPoint(startPos);
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
            else if (R.collider.tag == "arrivee")
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
                    Win();
                }
            }
            
        }
    
        while(nextMove < MoveTime)
        {
            player.transform.localPosition = Vector3.Lerp(startPos, endPos, nextMove / MoveTime);
            nextMove += Time.deltaTime;
            yield return null;
        }

        player.transform.localPosition = endPos;

        isMoving = false;

        if(gg)
        {
            player.transform.localPosition = departPos;
        }
    }


}

