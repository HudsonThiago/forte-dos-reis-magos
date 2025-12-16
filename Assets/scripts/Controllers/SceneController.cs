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
        screenList.ForEach(screen =>
        {
            if (screen.activeSelf == true)
            {
                currentScreen = screen;
            }
        });

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

    private void StopMenuMusic()
    {
        MenuMusic menuMusic = FindObjectOfType<MenuMusic>();
        if (menuMusic != null)
        {
            Destroy(menuMusic.gameObject);
        }
    }

    public void goToScreen(int targetScreen)
    {
        currentScreen.SetActive(false);
        screenList[targetScreen].SetActive(true);
        currentScreen = screenList[targetScreen];
    }

    public void loadScene(string scene)
    {
        StopMenuMusic();
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    public void loadScene(int scene)
    {
        StopMenuMusic();
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
