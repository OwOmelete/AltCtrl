using UnityEngine;

public class FullScreen : MonoBehaviour
{
    public Vector2Int borderSize;
    public Vector2Int defaultWindowSize;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!BorderlessWindow.framed)
            return;

        BorderlessWindow.SetFramelessWindow();
        BorderlessWindow.MoveWindowPos(Vector2Int.zero, Screen.width - borderSize.x, Screen.height - borderSize.y);
        
        BorderlessWindow.MoveWindowPos(Vector2Int.zero, defaultWindowSize.x, defaultWindowSize.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
