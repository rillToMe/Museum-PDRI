using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -20f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("References")]
    public Transform cameraTransform;

    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private float rotationX = 0f;
    private bool isGrounded;

    // Input variables
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool sprintPressed;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Auto setup camera
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
        }

        // Auto create ground check
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Reset velocity saat di ground
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Movement
        HandleMovement();

        // Mouse Look
        HandleMouseLook();

        // Jump
        HandleJump();

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Cursor control
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void HandleMovement()
    {
        // Baca input dari keyboard (WASD atau Arrow Keys)
        moveInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Check sprint
        sprintPressed = Keyboard.current.leftShiftKey.isPressed;
        float currentSpeed = sprintPressed ? sprintSpeed : moveSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // Baca input mouse
        lookInput = Mouse.current.delta.ReadValue() * mouseSensitivity * 0.1f;

        // Rotate player (horizontal)
        transform.Rotate(Vector3.up * lookInput.x);

        // Rotate camera (vertical)
        rotationX -= lookInput.y;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        }
    }

    void HandleJump()
    {
        jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;

        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}