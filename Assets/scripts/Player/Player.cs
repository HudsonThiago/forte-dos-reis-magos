using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerMovement playerMovement;

    private void Start()
    {
        if (TryGetComponent(out PlayerMovement playerMovement))
        {
            this.playerMovement = playerMovement;
        }
    }

    private void FixedUpdate()
    {
        playerMovement.walkAction();
    }

}
