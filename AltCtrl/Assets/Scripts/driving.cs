using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class driving : MonoBehaviour
{
    public float displacement;
    
    private bool grounded = false;
    [SerializeField] private int pv = 3;
    private Collider lastObstacle;
    private bool isLanding = false;
    [SerializeField] private int landingSpeed;
    [SerializeField] private int landingDisplacement;
    [SerializeField] private int landingDuration;
    [SerializeField] private Transform centerPosition;
    [SerializeField] private Image[] lifeDisplay;


    private void Start()
    {
        foreach (var img in lifeDisplay)
        {
            img.enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !grounded)
        {
            grounded = true;
            isLanding = true;
            centerPosition.DOMoveY(centerPosition.position.y - landingDisplacement, landingDuration)
                .SetEase(Ease.InOutQuad).OnComplete(landed);
            //StartCoroutine(landing());
        }
        if(isLanding)
        {
            transform.position = centerPosition.position;
        }
    }

    private void landed()
    {
        isLanding = false;
    }
    private void FixedUpdate()
    {
        Vector3 pos = new Vector3();
        if (!isLanding)
        {
            if (grounded)
            {
                pos += centerPosition.position + new Vector3(Input.GetAxisRaw("Horizontal") * displacement,0, 0);
            }
            else
            {
                pos += centerPosition.position + new Vector3(Input.GetAxisRaw("Horizontal") * displacement, Input.GetAxisRaw("Vertical") * displacement, 0);
            }
            transform.position = pos;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
        if (lastObstacle != other)
        {
            pv -= 1;
            Debug.Log("pv : " + pv);
            lastObstacle = other;
            lifeDisplay[2 - pv].enabled = true;
        }
    }

    IEnumerator landing()
    {
        isLanding = true;
        yield return new WaitForSeconds(3);
        isLanding = false;
    }
}
