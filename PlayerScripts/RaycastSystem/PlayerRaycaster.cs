using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerRaycaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InspectionHandler inspectionHandler;
    [SerializeField] private PlayerControllerHandler playerControllerHandler;
    [SerializeField] private TextMeshProUGUI interactionText;
    
    [Header("Settings")]
    [SerializeField] private float raycastDistance = 3f;

    private ITargetable currentTargetable;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (inspectionHandler == null) inspectionHandler = GetComponent<InspectionHandler>();
        if (playerControllerHandler == null) playerControllerHandler = GetComponent<PlayerControllerHandler>();
        if (interactionText != null) interactionText.text = "";
    }

    void Update()
    {
        // UI'ın üzerine gelindiyse dünyadaki objeleri taramayı bırak
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearCurrentTarget();
            return;
        }

        HandleRaycast();
        HandleInput();
    }

    private void HandleRaycast()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            ITargetable targetable = hit.collider.GetComponentInParent<ITargetable>();

            if (targetable != null)
            {
                if (currentTargetable != targetable)
                {
                    ClearCurrentTarget();
                    currentTargetable = targetable;
                    currentTargetable.ToggleHighlight(true);
                }

                UpdateInteractionText(hit.transform, targetable);
            }
            else
            {
                ClearCurrentTarget();
            }
        }
        else
        {
            ClearCurrentTarget();
        }
    }

    private void UpdateInteractionText(Transform hitTransform, ITargetable targetable)
    {
        if (interactionText == null) return;

        // Obje incelenebilir bir objeyse
        if (hitTransform.CompareTag("Object"))
        {
            interactionText.text = "E'ye Bas (İncele)";
            return;
        }

        // ICollectable ise özel mesajlar
        if (targetable is ICollectable collectable)
        {
            if (collectable is Diary) interactionText.text = "E'ye Bas (Günlük Oku)";
            else if (collectable is Battery) interactionText.text = "E'ye Bas (Pil Topla)";
            else if (collectable is Key key) interactionText.text = key.InteractionText;
            else interactionText.text = "E'ye Bas (Topla)";
        }
        // Geri kalan her şey (Kapı, Kasa, Çekmece) için genel mesaj
        else
        {
            interactionText.text = "E'ye Bas / Tıkla (Etkileşime Gir)";
        }
    }

    private void HandleInput()
    {
        if (currentTargetable == null) return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) 
            return;

        // 1. SOL TIK (Genel Etkileşimler: Kapı, Kasa, Çekmece)
        if (Input.GetMouseButtonDown(0))
        {
            if (hit.transform.CompareTag("Object") && inspectionHandler != null)
            {
                inspectionHandler.StartInspection(hit.transform.gameObject, hit.transform.position, hit.transform.rotation);
                if (playerControllerHandler != null) playerControllerHandler.DisablePlayerControls();
                ClearCurrentTarget();
                return;
            }

            // MUCİZE BURADA: Kapı mı, çekmece mi diye sormuyoruz. Sadece "Çalıştır" diyoruz.
            if (!(currentTargetable is ICollectable))
            {
                currentTargetable.Interact(); 
            }
        }

        // 2. E TUŞU (Toplama İşlemleri)
        if (Input.GetKeyDown(KeyCode.E) && currentTargetable is ICollectable currentCollectable)
        {
            if (currentCollectable is Diary diary)
            {
                currentCollectable.AddToInventory();
                diary.OpenDiaryCanvas();
            }
            else if (currentCollectable is Battery battery)
            {
                battery.OnCollect();
            }
            else if (currentCollectable is FlashlightCollectable flashlight)
            {
                currentCollectable.AddToInventory();
                if (flashlight.ItemDataProperty != null && InventoryManager.Instance != null)
                    InventoryManager.Instance.EquipItemFromInventory(flashlight.ItemDataProperty.itemPrefab);
            }
            else
            {
                currentCollectable.AddToInventory();
            }
            ClearCurrentTarget();
        }
    }

    private void ClearCurrentTarget()
    {
        if (currentTargetable != null)
        {
            currentTargetable.ToggleHighlight(false);
            currentTargetable = null;
        }

        if (interactionText != null) interactionText.text = "";
    }
}
