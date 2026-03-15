using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private CharacterController characterController;
    public bool IsSneaking { get; private set; } // Dýþarýdan okunabilir, içeriden deðiþtirilebilir

    [Header("Movement Settings")]
    [SerializeField] private float normalSpeed = 3f; // Normal hýz
    [SerializeField] private float sneakSpeed = 1.5f; // Yavaþ yürüme hýzý

    public bool IsHidden { get; private set; } = false;

    private float currentSpeed; // O anki hýz

    private float gravity = -9.8f;
    private Vector3 velocity;

    [Header("Ground Check")]
    [SerializeField] private Transform groundPosition;
    [SerializeField] private bool isGrounded;
    private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        currentSpeed = normalSpeed;
    }

    void Update()
    {

        if (IsHidden) return;

        PlayerMove();
        PlayerGravity();
    }


    public void SetHidingState(bool state)
    {
        IsHidden = state;

        // Saklanýnca yerçekimi veya kayma yapmamasý için velocity'i sýfýrla
        velocity = Vector3.zero;

        // Karakter kontrolcüsünü de duruma göre kapatabiliriz (Fizik çakýþmasýný önlemek için)
        characterController.enabled = !state;
    }

    private void PlayerMove()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Shift tuþuna basýlý mý kontrolü
        IsSneaking = Input.GetKey(KeyCode.LeftShift);

        // Hýzý ayarla: Shift basýlýysa yavaþ, deðilse normal
        currentSpeed = IsSneaking ? sneakSpeed : normalSpeed;

        Vector3 direction = transform.right * horizontal + transform.forward * vertical;

        // Hareketi uygula
        characterController.Move(direction * currentSpeed * Time.deltaTime);

        bool isMoving = direction.magnitude > 0.1f;

        if (VoiceManager.Instance != null)
        {
            // DÜZELTÝLEN KISIM BURASI:
            // Artýk 3. parametre olarak 'isSneaking' verisini de gönderiyoruz.
            VoiceManager.Instance.HandleFootsteps(isMoving, isGrounded, IsSneaking);
        }
    }

    private void PlayerGravity()
    {
        isGrounded = Physics.CheckSphere(groundPosition.position, groundDistance, groundMask);
        velocity.y += gravity * Time.deltaTime;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        characterController.Move(velocity * Time.deltaTime);
    }
}