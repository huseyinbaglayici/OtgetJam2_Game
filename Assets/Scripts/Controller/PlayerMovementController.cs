using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoSingleton<PlayerMovementController>
{
    [Header("Movement Settings")]
    public float movementSpeed = 7f;
    
    [Header("Physics Settings")]
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float groundedGravity = -0.5f; // Yerde sabit tutmak için
    
    [Header("Camera")]
    [SerializeField, ChildCameraProperty] private CinemachineCamera virtualCamera;
    
    private CharacterController _controller;
    private Vector3 moveDirection;
    private float verticalVelocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        MovePlayer();
        ApplyGravity();
        
        // Hareket ve yerçekimini birleştir
        Vector3 finalMovement = moveDirection + Vector3.up * verticalVelocity;
        _controller.Move(finalMovement * Time.deltaTime);
    }

    private void MovePlayer()
    {
        if (!InputManager.Instance.bInputEnabled) 
        {
            moveDirection = Vector3.zero;
            return;
        }

        float2 movementInput = InputManager.Instance.GetMovementInput();
        
        // Kamera yönüne göre hareket
        Vector3 forward = virtualCamera.transform.forward;
        Vector3 right = virtualCamera.transform.right;
        
        // Y bileşenini sıfırla (sadece yatay hareket)
        forward.y = 0;
        right.y = 0;
        
        forward.Normalize();
        right.Normalize();

        // Hareket yönünü hesapla
        moveDirection = (forward * movementInput.y + right * movementInput.x) * movementSpeed;
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded)
        {
            // Yerdeyken hafif aşağı basınç uygula
            verticalVelocity = groundedGravity;
        }
        else
        {
            // Havadayken yerçekimi uygula
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }
}