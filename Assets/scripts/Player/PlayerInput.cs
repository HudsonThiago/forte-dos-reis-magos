using Assets.scripts;
using UnityEngine;
using UnityEngine.InputSystem;


/*
 * Classe para definição das ações do player. Devem ser definidas no arquivo playerActions
 */
public class PlayerInput : MonoBehaviour
{
    public GameObject camera;
    public Player player;
    public float cursorSensitivity = 0.2f;

    public float xRotation;
    public float yRotation;

    private void Start()
    {
        if (TryGetComponent(out Player player))
        {
            this.player = player;
        }
    }

    public void movement(InputAction.CallbackContext input)
    {
        if (gameObject.TryGetComponent(out Movement movement))
        {
            if (input.performed)
            {
                movement.isRunning = true;
                movement.setDirection(input.ReadValue<Vector2>());
            }
            if (input.canceled)
            {
                movement.isRunning = false;
                movement.setDirection(Vector2.zero);
            }
        }
    }

    public void look(InputAction.CallbackContext input)
    {
        if (input.performed)
        {
            Vector2 lookInput = input.ReadValue<Vector2>();

            float mouseX = lookInput.x * cursorSensitivity;
            float mouseY = lookInput.y * cursorSensitivity;
            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            camera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            player.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }
}
