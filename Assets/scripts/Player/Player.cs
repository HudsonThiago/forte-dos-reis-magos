using Assets.scripts;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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
