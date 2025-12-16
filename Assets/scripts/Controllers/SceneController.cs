using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;
    [SerializeField] private List<GameObject> screenList;
    [SerializeField] private GameObject currentScreen;

    private void Awake()
    {
        if(screenList.Count > 0)
        {
            screenList.ForEach(screen =>
            {
                if (screen.activeSelf == true)
                {
                    currentScreen = screen;
                }
            });
        }

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameObject getPanel(int index)
    {
        return screenList[index];
    }


    public void goToScreen(int targetScreen)
    {
        currentScreen.SetActive(false);
        screenList[targetScreen].SetActive(true);
        currentScreen = screenList[targetScreen];
    }

    public void loadScene(string scene)
    {

        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    public void loadScene(int scene)
    {

        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
