using Game.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ScreenName
{
    MAIN,
    DIALOG,
    DIG
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<MainScreen> screenList;
    public MainScreen currentScreen;
    public GameObject transitionScreen;

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
            if (isMainScreen())
            {
                CursorManager.Instance.gameCursor();
            }
        }
    }

    public void toScreen(string screenName)
    {
        if (Enum.TryParse<ScreenName>(screenName, out var screen))
        {
            toScreen(screen);
        }
    }

    public MainScreen getScreen(ScreenName screenName)
    {
        return screenList.FirstOrDefault(s => s.screenName == ScreenName.MAIN);
    }

    public bool isMainScreen()
    {
        return isScreen(ScreenName.MAIN);
    }

    public bool isScreen(ScreenName screen)
    {
        return currentScreen.screenName == screen;
    }

    public void toExcavationScreen()
    {
        StartCoroutine(toExcavationCoroutine());
    }

    IEnumerator toExcavationCoroutine()
    {
        if(transitionScreen.TryGetComponent(out AnimationSystem animationSystem))
        {
            yield return new WaitForSeconds(animationSystem.changeAnimation("transitionStart"));
            toScreen(ScreenName.DIG);
            currentScreen.prevScreen = getScreen(ScreenName.MAIN);
            yield return new WaitForSeconds(2);
            yield return new WaitForSeconds(animationSystem.changeAnimation("transitionEnd"));
            CursorManager.Instance.mouseCursor();

        }
    }

    public void finishExcavation()
    {
        StartCoroutine(finishExcavationCoroutine());
    }

    IEnumerator finishExcavationCoroutine()
    {
        if (transitionScreen.TryGetComponent(out AnimationSystem animationSystem))
        {
            yield return new WaitForSeconds(animationSystem.changeAnimation("transitionStart"));
            toScreen(ScreenName.MAIN);
            currentScreen.prevScreen = getScreen(ScreenName.DIG);
            yield return new WaitForSeconds(2);
            yield return new WaitForSeconds(animationSystem.changeAnimation("transitionEnd"));
            CursorManager.Instance.gameCursor();

        }
    }

}


