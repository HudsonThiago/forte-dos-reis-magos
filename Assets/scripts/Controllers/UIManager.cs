using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ScreenName
{
    MAIN,
    DIALOG
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<MainScreen> screenList;
    public MainScreen currentScreen;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        screenList = FindObjectsByType<MainScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList<MainScreen>();
        currentScreen = screenList.FirstOrDefault(s=>s.screenName == ScreenName.MAIN);
    }

    public void toPrevScreen()
    {
        if (currentScreen != null && currentScreen.prevScreen != null)
        {
            toScreen(currentScreen.prevScreen.screenName);
        }
    }

    public void toScreen(ScreenName screenName)
    {
        MainScreen screen = screenList.FirstOrDefault(s => s.screenName.Equals(screenName));

        if (screen != null)
        {
            screen.prevScreen = currentScreen;
            currentScreen = screen;
            currentScreen.prevScreen.gameObject.SetActive(false);
            currentScreen.gameObject.SetActive(true);
        }
    }

    public bool isMainScreen()
    {
        return isScreen(ScreenName.MAIN);
    }

    public bool isScreen(ScreenName screen)
    {
        return currentScreen.screenName == screen;
    }


}


