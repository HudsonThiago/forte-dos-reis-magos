using UnityEngine;
using UnityEngine.Events;

public class InteractiveSpot : MonoBehaviour
{
    public UnityEvent action;

    public void onInteract()
    {
        action.Invoke();
    }
}
