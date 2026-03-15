using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    private CharacterController characterController;
    
    public bool IsSneaking { get; private set; } 
    public bool IsHidden { get; private set; } = false;

    [Header("Movement Settings")]
    [SerializeField] private float normalSpeed = 3f;
    [SerializeField] private float sneakSpeed = 1.5f;

    [Header("Gravity & Ground")]
    [SerializeField] private Transform groundPosition;
    [SerializeField] private LayerMask groundMask;
    private float groundDistance = 0.4f;
    private float gravity = -9.8f;
    
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (IsHidden) return;

        // Yerçekimi ve Hareketi tek fonksiyonda topladık
        HandleMovementAndGravity();
    }

    public void SetHidingState(bool state)
    {
        IsHidden = state;
        velocity = Vector3.zero;
        characterController.enabled = !state;
    }

    private void HandleMovementAndGravity()
    {
        isGrounded = Physics.CheckSphere(groundPosition.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        IsSneaking = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = IsSneaking ? sneakSpeed : normalSpeed;

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        
        // Yerçekimini hıza ekle
        velocity.y += gravity * Time.deltaTime;

        // Hareketi ve yerçekimini TEK BİR Move çağrısı ile uygula (Performans için kritik)
        Vector3 finalMovement = (moveDirection * currentSpeed) + velocity;
        characterController.Move(finalMovement * Time.deltaTime);

        bool isMoving = moveDirection.magnitude > 0.1f;

        if (VoiceManager.Instance != null)
        {
            VoiceManager.Instance.HandleFootsteps(isMoving, isGrounded, IsSneaking);
        }
    }
}
