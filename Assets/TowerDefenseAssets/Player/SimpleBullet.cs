using UnityEngine;

public class SimpleBullet : MonoBehaviour
{
    [Header("Atributos da Bala")]
    public float speed = 15f;
    public float damage = 10f;
    public float maxLifeTime = 5f;

    [Header("Configurações de colisão (opcional)")]
    public string enemyTag = "Inimigo";

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Start()
    {
        Destroy(gameObject, maxLifeTime);
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);
    }

    private void HitTarget()
    {
        Debug.Log($"[SimpleBullet] {name} acertou {target.name}");

        EnemyHealth hp = target.GetComponent<EnemyHealth>();
        if (hp == null)
            hp = target.GetComponentInParent<EnemyHealth>();
        if (hp == null)
            hp = target.GetComponentInChildren<EnemyHealth>();

        if (hp != null)
        {
            hp.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning($"[SimpleBullet] {target.name} não tem EnemyHealth em nenhum nível (self/parent/children).");
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(enemyTag) && !other.CompareTag(enemyTag))
            return;

        Debug.Log($"[SimpleBullet] OnTriggerEnter com {other.name}");

        EnemyHealth hp = other.GetComponent<EnemyHealth>();
        if (hp == null)
            hp = other.GetComponentInParent<EnemyHealth>();
        if (hp == null)
            hp = other.GetComponentInChildren<EnemyHealth>();

        if (hp != null)
        {
            hp.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
