using UnityEngine;

public class PlayerControllerHandler : MonoBehaviour
{
    [SerializeField] private MovementController playerMovement;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private SwayController swayController;

    void Start()
    {
        
        if (playerMovement == null) playerMovement = GetComponent<MovementController>();
        if (mouseLook == null) mouseLook = GetComponent<MouseLook>();
        if (swayController == null) swayController = GetComponent<SwayController>();

        EnablePlayerControls();
    }

    public void EnablePlayerControls()
    {
        if (playerMovement != null) playerMovement.enabled = true;
        if (mouseLook != null) mouseLook.enabled = true;
        if (swayController != null) swayController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void DisablePlayerControls()
    {
        if (playerMovement != null) playerMovement.enabled = false;
        if (mouseLook != null) mouseLook.enabled = false;
        if (swayController != null) swayController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}