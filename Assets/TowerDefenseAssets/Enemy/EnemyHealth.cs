using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    private float currentHealth;

    [SerializeField] private WaveSpawner waveSpawner;  // pode preencher no Inspector

    void Awake()
    {
        currentHealth = maxHealth;

        if (waveSpawner == null)
        {
#if UNITY_2023_1_OR_NEWER
            waveSpawner = FindFirstObjectByType<WaveSpawner>();
#else
            waveSpawner = FindObjectOfType<WaveSpawner>();
#endif
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"[EnemyHealth] {name} tomou {amount} de dano. Vida atual = {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"[EnemyHealth] {name} morreu por DANO.");

        if (waveSpawner != null)
        {
            waveSpawner.NotifyEnemyDied();
        }

        Destroy(gameObject);
    }
}
