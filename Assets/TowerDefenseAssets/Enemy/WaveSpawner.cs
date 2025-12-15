using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{
    [Header("Referências")]
    public GameObject enemyPrefab;
    public Transform[] waypoints;
    public Button nextWaveButton; // botão Next Wave

    public PlayerHealth playerHealth;

    [Header("Configuração por horda (índice 0 = Wave 1)")]
    public int[] enemiesPerWave = new int[3];
    public float[] speedPerWave = new float[3];
    public float[] spawnIntervalPerWave = new float[3];

    private int currentWaveIndex = -1;
    private bool isSpawning = false;
    private int enemiesAlive = 0;

    // ============ UPGRADES POR WAVE ============
    [Header("Upgrades por Wave")]
    public SimpleCannon[] cannons;
    public GameObject upgradePanel;

    public Button damageUpgradeButton;
    public Button rangeUpgradeButton;
    public Button attackSpeedUpgradeButton;

    public float damageUpgradeAmount = 2f;
    public float rangeUpgradeAmount = 1.5f;
    public float attackSpeedUpgradeAmount = 0.2f;

    private bool awaitingUpgradeChoice = false; 

    // ============ WIN / GAME END ============  
    [Header("Win UI")]
    public GameObject winPanel;    

    private void Awake()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (damageUpgradeButton != null)
            damageUpgradeButton.onClick.AddListener(ApplyDamageUpgrade);

        if (rangeUpgradeButton != null)
            rangeUpgradeButton.onClick.AddListener(ApplyRangeUpgrade);

        if (attackSpeedUpgradeButton != null)
            attackSpeedUpgradeButton.onClick.AddListener(ApplyAttackSpeedUpgrade);
    }

    public void StartNextWave()
    {
        if (awaitingUpgradeChoice)
        {
            Debug.Log("Escolha um upgrade antes de iniciar a próxima wave.");
            return;
        }

        if (winPanel != null && winPanel.activeSelf)
        {
            Debug.Log("Jogo já finalizado. Não há mais waves.");
            return;
        }

        if (isSpawning || enemiesAlive > 0)
        {
            Debug.Log("Não pode iniciar nova wave ainda. Wave atual em andamento ou inimigos vivos.");
            return;
        }

        currentWaveIndex++;

        int waveCount = Mathf.Min(
            enemiesPerWave.Length,
            Mathf.Min(speedPerWave.Length, spawnIntervalPerWave.Length)
        );

        if (currentWaveIndex >= waveCount)
        {
            Debug.Log("Não há mais waves para iniciar.");
            return;
        }

        if (nextWaveButton != null)
        {
            nextWaveButton.interactable = false;
        }

        StartCoroutine(SpawnWave(currentWaveIndex));
    }

    private IEnumerator SpawnWave(int waveIndex)
    {
        isSpawning = true;

        Debug.Log($"Iniciando Wave {waveIndex + 1}");

        int enemiesToSpawn = enemiesPerWave[waveIndex];
        float enemySpeed = speedPerWave[waveIndex];
        float spawnInterval = spawnIntervalPerWave[waveIndex];

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy(enemySpeed);
            yield return new WaitForSeconds(spawnInterval);
            enemiesAlive++;
        }

        Debug.Log($"Wave {waveIndex + 1} terminou de spawnar.");

        isSpawning = false;

        TryEnableButtonOrUpgradeOrWin(); 
    }

    private void SpawnEnemy(float enemySpeed)
    {
        if (enemyPrefab == null || waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("EnemyPrefab ou Waypoints não definidos no WaveSpawner!");
            return;
        }

        GameObject enemy = Instantiate(
            enemyPrefab,
            waypoints[0].position,
            Quaternion.identity
        );

        // Conta mais um inimigo vivo
        enemiesAlive++;

        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.waypoints = waypoints;
            movement.speed = enemySpeed;
            movement.spawner = this;
            movement.playerHealth = playerHealth;
        }
        else
        {
            Debug.LogError("EnemyPrefab não possui EnemyMovement!");
        }
    }

    public void NotifyEnemyDied()
    {
            enemiesAlive--;

            if (enemiesAlive < 0) enemiesAlive = 0;

            TryEnableButtonOrUpgradeOrWin();
    }


    private void TryEnableButtonOrUpgradeOrWin()
    {

        if (isSpawning || enemiesAlive > 0)
            return;

        int waveCount = Mathf.Min(
            enemiesPerWave.Length,
            Mathf.Min(speedPerWave.Length, spawnIntervalPerWave.Length)
        );

        bool lastWaveFinished = currentWaveIndex >= waveCount - 1;

        if (lastWaveFinished)
        {
            ShowWinPanel();
        }
        else
        {
            ShowUpgradePanel();
        }
    }

    // ============ LÓGICA DE UPGRADE ============

    private void ShowUpgradePanel()
    {
        if (upgradePanel == null)
        {
            if (nextWaveButton != null)
                nextWaveButton.interactable = true;

            Debug.Log("Wave finalizada totalmente. Botão Next liberado (sem sistema de upgrade configurado).");
            return;
        }

        awaitingUpgradeChoice = true;

        upgradePanel.SetActive(true);

        if (nextWaveButton != null)
            nextWaveButton.interactable = false;

        Debug.Log("Wave finalizada. Escolha um upgrade (Dano / Range / Atk Speed).");
    }

    private void ApplyDamageUpgrade()
    {
        foreach (var cannon in cannons)
        {
            if (cannon != null)
                cannon.UpgradeDamage(damageUpgradeAmount);
        }

        Debug.Log($"+{damageUpgradeAmount} de Dano aplicado em todos os canhões.");
        FinishUpgradeChoice();
    }

    private void ApplyRangeUpgrade()
    {
        foreach (var cannon in cannons)
        {
            if (cannon != null)
                cannon.UpgradeRange(rangeUpgradeAmount);
        }

        Debug.Log($"+{rangeUpgradeAmount} de Range aplicado em todos os canhões.");
        FinishUpgradeChoice();
    }

    private void ApplyAttackSpeedUpgrade()
    {
        foreach (var cannon in cannons)
        {
            if (cannon != null)
                cannon.UpgradeAttackSpeed(attackSpeedUpgradeAmount);
        }

        Debug.Log($"+{attackSpeedUpgradeAmount} de Attack Speed aplicado em todos os canhões.");
        FinishUpgradeChoice();
    }

    private void FinishUpgradeChoice()
    {
        awaitingUpgradeChoice = false;

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        if (nextWaveButton != null)
            nextWaveButton.interactable = true;

        Debug.Log("Upgrade escolhido. Você pode clicar em Next Wave para iniciar a próxima.");
    }

    // ============ WIN PANEL ============

    private void ShowWinPanel()
    {
        Debug.Log("Todas as waves concluídas. VITÓRIA!");

        Time.timeScale = 0f;

        awaitingUpgradeChoice = false;

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        // Desabilita o botão Next Wave
        if (nextWaveButton != null)
            nextWaveButton.interactable = false;

        // Mostra tela de vitória
        if (winPanel != null)
            winPanel.SetActive(true);
    }
}
