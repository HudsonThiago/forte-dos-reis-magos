using UnityEngine;

public class SimpleCannon : MonoBehaviour
{
    [Header("Atributos do Canhão")]
    public float damage = 10f;        // Dano por tiro
    public float attackSpeed = 1f;    // Tiros por segundo
    public float range = 12f;         // Alcance para achar inimigos

    [Header("Referências")]
    public Transform firePoint;       // Posição onde a bala nasce
    public GameObject bulletPrefab;   // Prefab da bala visual

    [Header("Configurações")]
    public string enemyTag = "Inimigo";
    public float bulletSpeed = 15f;   // Velocidade da bala

    private Transform target;
    private float cooldown = 0f;

    void Start()
    {
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.2f);
    }

    void Update()
    {
        if (target == null)
            return;

        cooldown -= Time.deltaTime;

        if (cooldown <= 0f)
        {
            Shoot();
            cooldown = 1f / attackSpeed;
        }
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        float closestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && closestDistance <= range)
            target = nearestEnemy.transform;
        else
            target = null;
    }

   void Shoot()
{
    if (bulletPrefab == null || firePoint == null || target == null)
    {
        Debug.LogWarning($"[Cannon {name}] Não conseguiu atirar. bulletPrefab={bulletPrefab != null}, firePoint={firePoint != null}, target={target != null}");
        return;
    }

    Debug.Log($"[Cannon {name}] Atirando em {target.name}");

    GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    bulletGO.transform.localScale = Vector3.one * 0.3f;

    SimpleBullet bullet = bulletGO.GetComponent<SimpleBullet>();
    if (bullet != null)
    {
        bullet.SetTarget(target);
        bullet.damage = damage;
        bullet.speed = bulletSpeed;
    }
}


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    // ========= MÉTODOS DE UPGRADE =========

    public void UpgradeDamage(float amount)
    {
        damage += amount;
        if (damage < 0f) damage = 0f;
    }

    public void UpgradeRange(float amount)
    {
        range += amount;
        if (range < 0f) range = 0f;
    }

    public void UpgradeAttackSpeed(float amount)
    {
        attackSpeed += amount;
        if (attackSpeed < 0.1f) attackSpeed = 0.1f; // evita zero ou negativo
    }
}
