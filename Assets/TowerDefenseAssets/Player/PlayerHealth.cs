using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuração de vida")]
    public int maxHealth = 10;
    [HideInInspector] public int currentHealth;

    [Header("UI")]
    public TMP_Text healthText;
    public GameObject gameOverScreen;

    private bool isGameOver = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isGameOver) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        Debug.Log("GAME OVER!");

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}
