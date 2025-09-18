using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager SINGLETON { get; private set; }
    public List<GameObject> Panels;
    private void Start()
    {
        if (MenuManager.SINGLETON != null)
        {
            Destroy(MenuManager.SINGLETON);
        }
        MenuManager.SINGLETON = this;
    }

}
