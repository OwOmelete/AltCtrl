using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GalleryModeManager : MonoBehaviour
{
    public static GalleryModeManager SINGLETON { get; private set; }
    public float TimeToAFK;
    public Coroutine CoroutineAfk;
    public bool Gaming, Starting, Ending, Win;
    public bool CD;   
    private void Awake()
    {
        if (SINGLETON != null && SINGLETON!= this)
        {
            Destroy(gameObject);
            return;
        }
        SINGLETON = this;
        DontDestroyOnLoad(this);
        Starting = true;
    }
    public void Update()
    {
        if (Starting&&Input.anyKeyDown)
        {
            if (!CD)
            {
                StartGaming();
            }
        }
        else if (Ending && Input.anyKeyDown)
        {
            if (!CD)
            {
                AfkTimer(0);
            }
        }
    }

    public void AfkTimer(float time)
    {
        if (CoroutineAfk != null) StopCoroutine(CoroutineAfk);
        CoroutineAfk = StartCoroutine(TimerToMenu(time));
    }
    IEnumerator CoolDown(float x)
    {
        CD = true;
        yield return new WaitForSeconds(x);
        CD = false;
    }
    IEnumerator TimerToMenu(float x)
    {
        yield return new WaitForSeconds(x);
        SceneManager.LoadScene("Menu");
        StartCoroutine(ShowMenu(0));
        Starting = true;
        Ending = false;
    }
    public void StartGaming()
    {
        Starting = false;
        Gaming = true;
        SceneManager.LoadScene("MainScene");
        AfkTimer(TimeToAFK);
    }

    public void EndingScreen()
    {
        SceneManager.LoadScene("Menu");

        Gaming = false;
        Ending = true;
        if (Win)
        {
            StartCoroutine(CoolDown(3f));
            StartCoroutine(ShowMenu(1));

        }
        else
        {
            StartCoroutine(CoolDown(3f));
            StartCoroutine(ShowMenu(2));

        }
        AfkTimer(12f);
    }
    IEnumerator ShowMenu(int index)
    {
        while (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Menu"))
        {
            yield return null;
        }
        foreach (GameObject obj in MenuManager.SINGLETON.Panels)
        {
            obj.SetActive(false);
        }
        MenuManager.SINGLETON.Panels[index].SetActive(true);
        Debug.Log(index);
    }

}
