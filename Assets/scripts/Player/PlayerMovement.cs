using Assets.scripts;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerMovement : MonoBehaviour, Movement
{
    [Header("Movement")]
    public Rigidbody rb;
    public Vector2 direction { get; set; }
    public Vector3 moveDirection { get; set; }
    public bool isRunning { get; set; }
    public bool isMoving;
    public float moveSpeed;

    private void Awake()
    {
        isMoving = true;
        if (gameObject.TryGetComponent(out Rigidbody rb))
        {
            this.rb = rb;
        }
        rb.freezeRotation = true;
    }

    public void setDirection(Vector2 direction)
    {
        this.direction = direction;
    }

    public void walkAction()
    {
        if (isRunning && isMoving)
        {
            moveDirection = transform.forward * direction.y + transform.right * direction.x;
            rb.linearVelocity = new Vector3(
                moveDirection.normalized.x * moveSpeed,
                rb.linearVelocity.y,
                moveDirection.normalized.z * moveSpeed
            );
        }
        else
        {
            // Se não estiver movendo, desacelera rapidamente
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    public void speedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
}
