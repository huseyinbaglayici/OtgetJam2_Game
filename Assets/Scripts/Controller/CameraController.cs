using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Player Transform")] public Transform rotatePlayerTransform;

    [Header("Camera Settings")] public float mouseSensitivity = 2f; // Daha düşük başlangıç değeri

    [Header("Look Limits")] public float minX = -90f;
    public float maxX = 90f;

    [Header("Smoothing (Optional)")] public bool useSmoothing = false;
    public float rotationSpeed = 10f; // Eğer smoothing kullanılacaksa

    private float rotateX;
    private float rotateY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        if (!InputManager.Instance.bInputEnabled) return;

        Vector2 mouseInput = InputManager.Instance.GetMousePosition();

        rotateX -= mouseInput.y * mouseSensitivity;
        rotateY += mouseInput.x * mouseSensitivity;
        rotateX = Mathf.Clamp(rotateX, minX, maxX);
        
        Quaternion targetRotation = Quaternion.Euler(rotateX, rotateY, 0);
        
        if (useSmoothing)
        {
            rotatePlayerTransform.rotation =
                Quaternion.Slerp(rotatePlayerTransform.rotation, targetRotation,
                    Time.deltaTime * rotationSpeed);
        }
        else
        {
            rotatePlayerTransform.rotation = targetRotation;
        }
    }
}