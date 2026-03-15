using UnityEngine;
using TMPro;
using UnityEngine.EventSystems; // UI Tıklamalarını algılamak için ŞART!

public class PlayerRaycaster : MonoBehaviour
{
    private Camera mainCamera;
    private Ray ray;
    private RaycastHit hit;
    private ITargetable currentTargetable;
    private ICollectable currentCollectable;

    [SerializeField] private InspectionHandler inspectionHandler;
    [SerializeField] private PlayerControllerHandler playerControllerHandler;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private float raycastDistance = 3f;

    void Start()
    {
        mainCamera = Camera.main;
        if (inspectionHandler == null)
            inspectionHandler = GetComponent<InspectionHandler>();
        if (playerControllerHandler == null)
            playerControllerHandler = GetComponent<PlayerControllerHandler>();
        if (interactionText != null)
            interactionText.text = "";
    }

    void Update()
    {
        // --- 1. UI KONTROLÜ (BU SATIR SORUNU ÇÖZER) ---
        // Eğer mouse bir UI elemanının (Kasa tuşları vb.) üzerindeyse Raycast atma.
        // Böylece arkadaki kasaya tekrar tıklayıp animasyonu bozmazsın.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearCurrentTarget();
            return;
        }

        HandleRaycastAndHighlight();
        HandleInput();
    }

    private void HandleRaycastAndHighlight()
    {
        ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        bool foundTarget = false;

        // Trigger'ları (Enemy alanlarını) yoksaymak için 'QueryTriggerInteraction.Ignore' kullanıyoruz
        if (Physics.Raycast(ray, out hit, raycastDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            ITargetable targetable = null;

            // Önce çarptığımız objede script var mı?
            if (!hit.collider.TryGetComponent(out targetable))
            {
                // Yoksa ebeveyninde (Parent) var mı? (Kapı koluna tıklayınca kapıyı bulsun diye)
                targetable = hit.collider.GetComponentInParent<ITargetable>();
            }

            if (targetable != null)
            {
                if (currentTargetable != targetable)
                {
                    ClearCurrentTarget();
                    currentTargetable = targetable;
                    currentTargetable.ToggleHighlight(true);
                }
                foundTarget = true;
                currentCollectable = targetable as ICollectable;
            }
            else if (hit.transform.CompareTag("ExitDoor"))
            {
                ExitDoor exitDoor = hit.collider.GetComponentInParent<ExitDoor>();
                if (exitDoor != null)
                {
                    if (currentTargetable != exitDoor)
                    {
                        ClearCurrentTarget();
                        currentTargetable = exitDoor;
                        currentTargetable.ToggleHighlight(true);
                    }
                    foundTarget = true;
                }
            }
        }

        // UI Yazısı Güncelleme
        if (interactionText != null)
        {
            if (foundTarget)
            {
                if (currentCollectable != null)
                {
                    if (currentCollectable is Diary) interactionText.text = "E'ye Bas (Günlük Oku)";
                    else if (currentCollectable is Battery) interactionText.text = "E'ye Bas (Pil Topla)";
                    else if (currentCollectable is Key key) interactionText.text = key != null ? key.InteractionText : "E'ye Bas (Topla)";
                    else if (currentCollectable is FlashlightCollectable) interactionText.text = "E'ye Bas (Fener Topla)";
                    else interactionText.text = "E'ye Bas";
                }
                else if (currentTargetable is ExitDoor exitDoor)
                {
                    interactionText.text = exitDoor != null ? exitDoor.InteractionText : "E'ye Bas (Kapıyı Aç)";
                }
            }
            else
            {
                interactionText.text = "";
            }
        }

        if (!foundTarget) ClearCurrentTarget();
    }

    private void HandleInput()
    {
        // Tıklama Algılama (Yine Trigger Ignore ile)
        if (Input.GetMouseButtonDown(0))
        {
            ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out hit, raycastDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                // 🔹 Çekmece (Parent Kontrolü ile)
                DrawerController drawer = hit.collider.GetComponentInParent<DrawerController>();
                if (drawer != null)
                {
                    drawer.ToggleDrawer();
                    return;
                }

                // 🔹 Kapı (Parent Kontrolü ile)
                SimpleDoorOpener door = hit.collider.GetComponentInParent<SimpleDoorOpener>();
                if (door != null)
                {
                    door.ToggleDoor();
                    return;
                }

                // 🔹 Soru Kasası (Parent Kontrolü ile)
                questionSafeController questionSafe = hit.collider.GetComponentInParent<questionSafeController>();
                if (questionSafe != null)
                {
                    if (!questionSafe.isUnlocked)
                    {
                        questionSafe.OpenQuestionCanvas();
                        return;
                    }
                }

                // 🔹 Şifreli Kasa (SafeController)
                SafeController safe = hit.collider.GetComponentInParent<SafeController>();
                if (safe != null)
                {
                    if (!safe.isUnlocked)
                    {
                        safe.OpenSafeUI();
                        return;
                    }
                }

                // 🔹 Obje İnceleme
                if (hit.transform.CompareTag("Object"))
                {
                    if (inspectionHandler != null)
                    {
                        // Fener kontrolünü kaldırdık, el doluyken de inceleyebilirsin.
                        inspectionHandler.StartInspection(hit.transform.gameObject, hit.transform.position, hit.transform.rotation);
                        if (playerControllerHandler != null) playerControllerHandler.DisablePlayerControls();
                    }
                    return;
                }
            }
        }

        // 🔹 E Tuşu ile Toplama
        if (Input.GetKeyDown(KeyCode.E) && currentTargetable != null)
        {
            if (currentCollectable != null)
            {
                if (currentCollectable is Diary diary)
                {
                    currentCollectable.AddToInventory();
                    diary.OpenDiaryCanvas();
                }
                else if (currentCollectable is FlashlightCollectable flashlight)
                {
                    currentCollectable.AddToInventory();
                    // InventoryManager varsa kullan
                    if (flashlight.ItemDataProperty != null && InventoryManager.Instance != null)
                        InventoryManager.Instance.EquipItemFromInventory(flashlight.ItemDataProperty.itemPrefab);
                }
                else if (currentCollectable is Battery battery)
                {
                    battery.OnCollect();
                }
                else
                {
                    currentCollectable.AddToInventory();
                }
            }
            else if (currentTargetable is ExitDoor exitDoor)
            {
                exitDoor.Interact();
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
            currentCollectable = null;
        }

        if (interactionText != null)
            interactionText.text = "";
    }
}