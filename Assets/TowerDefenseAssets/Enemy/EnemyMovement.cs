using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movimentação")]
    public Transform[] waypoints;
    public float speed = 3f;

    [Header("Referências")]
    public WaveSpawner spawner;
    public PlayerHealth playerHealth;

    [Header("Dano ao Player ao chegar no fim")]
    public int damageToPlayer = 10;

    private int currentWaypointIndex = 0;

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = targetWaypoint.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            ReachWaypoint();
        }
        else
        {
            transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        }
    }

    private void ReachWaypoint()
    {
        currentWaypointIndex++;
        if (currentWaypointIndex >= waypoints.Length)
        {
            OnPathEnd();
        }
    }

    private void OnPathEnd()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageToPlayer);
        }

        if (spawner != null)
        {
            Debug.Log($"[EnemyMovement] {name} chegou ao fim do caminho. NotifyEnemyDied().");
            spawner.NotifyEnemyDied();
        }

        Destroy(gameObject);
    }
}
