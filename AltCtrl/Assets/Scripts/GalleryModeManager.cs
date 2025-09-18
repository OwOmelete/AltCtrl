using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GalleryModeManager : MonoBehaviour
{
    public static GalleryModeManager SINGLETON { get; private set; }
    public float TimeToAFK;
    public Coroutine CoroutineAfk;
    public bool Gaming, Starting, Ending, Win;
    public bool ClickedOnce;    
    private void Start()
    {
        if (GalleryModeManager.SINGLETON != null)
        {
            Destroy(GalleryModeManager.SINGLETON);
        }
        GalleryModeManager.SINGLETON = this;
        DontDestroyOnLoad(this);
        Starting = true;
    }
    public void Update()
    {
        if ((Starting||Ending)&&Input.anyKeyDown)
        {
            if (ClickedOnce)
            {
                ClickedOnce = false;
                StartGaming();
            }
            else
            {
                ClickedOnce = true;
                //activer le texte qui dit de Go
            }
        }
    }

    public void AfkTimer(float time)
    {
        if (CoroutineAfk != null) StopCoroutine(CoroutineAfk);
        CoroutineAfk = StartCoroutine(TimerToMenu(time));
    }

    IEnumerator TimerToMenu(float x)
    {
        yield return new WaitForSeconds(x);
        SceneManager.LoadScene("Menu");
        Starting = true;
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
        Gaming = false;
        if (Win)
        {
            SceneManager.LoadScene("Win");
        }
        else
        {
            SceneManager.LoadScene("Loose");
        }
        AfkTimer(10f);
    }

}
