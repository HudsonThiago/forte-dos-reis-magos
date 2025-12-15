using Assets.scripts;
using Game.Entities;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;


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

    public float range;
    public GameObject interactBox;

    private void Start()
    {
        if (TryGetComponent(out Player player))
        {
            this.player = player;
        }
    }

    public void movement(InputAction.CallbackContext input)
    {
        if(UIManager.Instance.isMainScreen())
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
        } else
        {
            if(input.started && UIManager.Instance.isScreen(ScreenName.DIALOG)){
                if(UIManager.Instance.currentScreen.TryGetComponent(out INavigation navigation)){
                    navigation.navigation(input.ReadValue<Vector2>());
                }
            }
        }
    }

    public void look(InputAction.CallbackContext input)
    {
        if (input.performed && UIManager.Instance.isMainScreen())
        {
            Vector2 lookInput = input.ReadValue<Vector2>();

            float mouseX = lookInput.x * cursorSensitivity;
            float mouseY = lookInput.y * cursorSensitivity;
            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
            Vector3 direction = rotation * Vector3.forward;
            Vector3 origin = transform.position + Vector3.up * 0.7f;

            camera.transform.rotation = rotation;
            player.transform.rotation = Quaternion.Euler(0, yRotation, 0);

            if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, range))
            {
                Debug.DrawRay(origin, direction * hitInfo.distance, Color.red, 0.01f);

                if (hitInfo.collider.CompareTag("interactable"))
                {
                    interactBox.SetActive(true);
                }
                else
                {
                    interactBox.SetActive(false);
                }
            }
            else
            {
               interactBox.SetActive(false);
            }
        }
    }

    public void interact(InputAction.CallbackContext input)
    {
        if (UIManager.Instance.isMainScreen())
        {
            if (input.started)
            {
                Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
                Vector3 direction = rotation * Vector3.forward;

                Vector3 origin = transform.position + Vector3.up * 0.7f;

                if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, range))
                {
                    Debug.DrawRay(origin, direction * hitInfo.distance, Color.red, 1f);

                    if (hitInfo.collider.CompareTag("interactable"))
                    {
                        if (hitInfo.collider.TryGetComponent(out InteractiveSpot interactiveSpot))
                        {
                            interactiveSpot.onInteract();
                        }
                    }
                }
            }
        }
        else
        {
            if (input.started && UIManager.Instance.isScreen(ScreenName.DIALOG))
            {
                if (UIManager.Instance.currentScreen.TryGetComponent(out IInteraction interaction))
                {
                    interaction.interact();
                }
            }
        }
    }

    public void cancel(InputAction.CallbackContext input)
    {
        if (input.performed)
        {
            if (!UIManager.Instance.isMainScreen())
            {
                UIManager.Instance.toPrevScreen();
            }
        }
    }
}
