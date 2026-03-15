using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class RaycastProcess : MonoBehaviour

{

    private Camera mainCamera;

    private Ray ray;

    private RaycastHit hit;

    public float distance = 3f;

    private ITargetable currentTargetable;

    private ICollectable currentCollectable;

    Vector3 originalPos;

    public Transform playerSocket;

    public Transform largeObjectSocket;

    private Transform currentSocket;

    GameObject inspected;



    public bool onInspect = false;

    bool isDropping = false;

    bool isPickingUp = false;

    bool isCursorVisible = false;

    public MovementController playerScript;

    public MouseLook mouseLook;

    [SerializeField] private float rotationSpeed = 200f;

    [SerializeField] private float positionSmoothness = 0.2f;

    [SerializeField] private float lerpDuration = 0.5f;

    [SerializeField] private Transform playerHand;

    private ICollectable heldItem;

    private SwayController swayController;



    private Coroutine inspectPickupCoroutine;

    private Coroutine inspectDropCoroutine;



    void Start()

    {

        mainCamera = Camera.main;

        if (playerHand == null)

        {

            playerHand = GameObject.FindWithTag("Player").transform.Find("Hand");

            if (playerHand == null)

            {

                Debug.LogError("PlayerHand bulunamadý.");

            }

        }

        swayController = playerHand.GetComponent<SwayController>();

        if (swayController == null)

        {

            Debug.LogWarning("SwayController bulunamadý, playerHand üzerinde olduðundan emin olun.");

        }

        currentSocket = playerSocket;



        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;

        isCursorVisible = false;

    }



    void Update()

    {

        ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out hit, 100))

        {

            if (hit.collider.TryGetComponent(out ITargetable targetable))

            {

                if (currentTargetable != targetable)

                {

                    if (currentTargetable != null)

                    {

                        currentTargetable.ToggleHighlight(false);

                    }

                    currentTargetable = targetable;

                    currentTargetable.ToggleHighlight(true);

                    if (targetable is ICollectable collectable)

                    {

                        currentCollectable = collectable;

                    }

                    else

                    {

                        currentCollectable = null;

                    }

                }

            }

            else

            {

                if (currentTargetable != null)

                {

                    currentTargetable.ToggleHighlight(false);

                    currentTargetable = null;

                    currentCollectable = null;

                }

            }

        }

        else

        {

            if (currentTargetable != null)

            {

                currentTargetable.ToggleHighlight(false);

                currentTargetable = null;

                currentCollectable = null;

            }

        }



        if (Input.GetMouseButtonDown(0) && currentCollectable != null)

        {

            if (currentCollectable is FlashlightCollectable)

            {

                heldItem = currentCollectable;

                currentCollectable.Collect(playerHand);

                currentCollectable = null;

            }

        }

        else if (Input.GetKeyDown(KeyCode.E) && currentCollectable != null)

        {

            if (currentCollectable is FlashlightCollectable flashlight)

            {

                flashlight.AddToInventory();

                TaskManager.Instance.CompleteTask("task_find_flashlight");

                currentCollectable = null;

            }

        }



        if (Input.GetMouseButtonDown(1) && heldItem != null)

        {

            Vector3 dropPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;

            Quaternion dropRotation = (heldItem as MonoBehaviour).transform.rotation;

            heldItem.Drop(dropPosition, dropRotation);

            heldItem = null;

            Debug.Log("Flashlight Dropped with Rotation: " + dropRotation.eulerAngles);

        }



        inspectedRayCast();

    }



    void inspectedRayCast()

    {

        if (Physics.Raycast(ray, out hit, distance) && !onInspect && !isDropping && !isPickingUp && heldItem == null)

        {

            if (hit.transform.CompareTag("Object") && hit.transform != playerHand && !hit.transform.CompareTag("Flashlight"))

            {

                Debug.Log("Raycast Hit: " + hit.transform.name);

                if (Input.GetKeyDown(KeyCode.Mouse0))

                {

                    inspected = hit.transform.gameObject;

                    Debug.Log("Inspected Object: " + inspected.name);

                    originalPos = hit.transform.position;

                    onInspect = true;

                    if (inspected.transform.localScale.x > 0.9f)

                    {

                        currentSocket = largeObjectSocket;

                    }

                    else

                    {

                        currentSocket = playerSocket;

                    }

                    inspectPickupCoroutine = StartCoroutine(inspectPickupItem());

                }

            }

        }



        if (onInspect)

        {

            Vector3 socketPosition = mainCamera.transform.position + mainCamera.transform.forward * 1f;

            inspected.transform.position = Vector3.Lerp(inspected.transform.position, socketPosition, positionSmoothness);



            if (Input.GetMouseButton(0))

            {

                Debug.Log("Rotating Object: " + inspected.name);

                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

                inspected.transform.Rotate(Vector3.right, -mouseY, Space.World);

                inspected.transform.Rotate(Vector3.up, mouseX, Space.World);

            }

        }



        if (Input.GetKeyDown(KeyCode.Mouse1) && onInspect && !isPickingUp)

        {

            inspectDropCoroutine = StartCoroutine(inspectDropItem());

            onInspect = false;

        }

    }



    IEnumerator inspectPickupItem()

    {

        isPickingUp = true;

        playerScript.enabled = false;

        mouseLook.enabled = false;

        if (swayController != null) swayController.enabled = false;

        Cursor.lockState = CursorLockMode.None;

        Cursor.visible = true;

        isCursorVisible = true;

        Debug.Log("Cursor Visibility: " + isCursorVisible);



        if (inspected != null)

        {

            foreach (Transform child in inspected.transform)

            {

                if (child.CompareTag("HiddenCode"))

                {

                    HiddenCode hiddenCode = child.GetComponent<HiddenCode>();

                    if (hiddenCode != null && !hiddenCode.IsFound())

                    {

                        child.gameObject.SetActive(true);

                        Debug.Log("Hidden Code Activated: " + child.name);

                    }

                }

            }

        }



        yield return new WaitForSeconds(0.2f);



        if (inspected != null)

        {

            float lerpTime = 0f;

            Vector3 startPos = inspected.transform.position;

            Vector3 targetPos = mainCamera.transform.position + mainCamera.transform.forward * 1f;

            while (lerpTime < lerpDuration)

            {

                if (inspected == null)

                {

                    Debug.LogWarning("Inspected object became null during coroutine.");

                    isPickingUp = false;

                    yield break;

                }



                lerpTime += Time.deltaTime;

                float t = lerpTime / lerpDuration;

                inspected.transform.position = Vector3.Lerp(startPos, targetPos, t);

                yield return null;

            }

            inspected.transform.position = targetPos;

            inspected.transform.rotation = Quaternion.identity;

            var rb = inspected.GetComponent<Rigidbody>();

            if (rb != null) rb.isKinematic = true;

            Debug.Log("Picked Up Object: " + inspected.name);

        }

        isPickingUp = false;

    }



    IEnumerator inspectDropItem()

    {

        isDropping = true;

        if (inspected != null)

        {

            foreach (Transform child in inspected.transform)

            {

                if (child.CompareTag("HiddenCode"))

                {

                    child.gameObject.SetActive(false);

                    Debug.Log("Hidden Code Deactivated: " + child.name);

                }

            }

            inspected.transform.rotation = Quaternion.identity;

            float lerpTime = 0f;

            Vector3 startPos = inspected.transform.position;

            while (lerpTime < lerpDuration)

            {

                if (inspected == null)

                {

                    Debug.LogWarning("Inspected object became null during coroutine.");

                    isDropping = false;

                    yield break;

                }



                lerpTime += Time.deltaTime;

                float t = lerpTime / lerpDuration;

                inspected.transform.position = Vector3.Lerp(startPos, originalPos, t);

                yield return null;

            }

            inspected.transform.position = originalPos;

            var rb = inspected.GetComponent<Rigidbody>();

            if (rb != null) rb.isKinematic = false;

            Debug.Log("Dropped Object: " + inspected.name);

        }

        yield return new WaitForSeconds(0.2f);

        playerScript.enabled = true;

        mouseLook.enabled = true;

        if (swayController != null) swayController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;

        isCursorVisible = false;

        inspected = null;

        isDropping = false;

    }



    public void ExitInspectionMode()

    {

        if (onInspect)

        {

            if (inspectPickupCoroutine != null)

            {

                StopCoroutine(inspectPickupCoroutine);

            }

            if (inspectDropCoroutine != null)

            {

                StopCoroutine(inspectDropCoroutine);

            }



            if (inspected != null)

            {

                inspected.transform.position = originalPos;

                inspected.transform.rotation = Quaternion.identity;

                var rb = inspected.GetComponent<Rigidbody>();

                if (rb != null) rb.isKinematic = false;



                foreach (Transform child in inspected.transform)

                {

                    if (child.CompareTag("HiddenCode"))

                    {

                        child.gameObject.SetActive(false);

                    }

                }

            }

            onInspect = false;

            isPickingUp = false;

            isDropping = false;

            inspected = null;

            Debug.Log("Ýnceleme modu menü açýldýðý için kapatýldý.");

        }

    }



    public void EquipItemFromInventory(GameObject itemPrefab)

    {

        if (heldItem != null)

        {

            Vector3 dropPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;

            Quaternion dropRotation = (heldItem as MonoBehaviour).transform.rotation;

            heldItem.Drop(dropPosition, dropRotation);

            heldItem = null;

        }



        if (itemPrefab != null)

        {

            GameObject instantiatedItem = Instantiate(itemPrefab, playerHand.position, playerHand.rotation);

            instantiatedItem.transform.SetParent(playerHand);

            instantiatedItem.transform.localPosition = Vector3.zero;

            instantiatedItem.transform.localRotation = Quaternion.identity;



            Rigidbody rb = instantiatedItem.GetComponent<Rigidbody>();

            if (rb != null) rb.isKinematic = true;



            heldItem = instantiatedItem.GetComponent<ICollectable>();

            if (heldItem != null)

            {

                heldItem.Collect(playerHand);

            }



            Debug.Log("Envanterden eþya eline alýndý: " + itemPrefab.name);

        }

        else

        {

            Debug.Log("Eli boþalttý.");

        }

    }

}