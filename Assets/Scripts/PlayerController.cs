using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    
    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float upDownRange = 60f;
    
    [Header("Jump Settings")]
    public float jumpBufferTime = 0.2f; // Time window to buffer jump input
    public float coyoteTime = 0.15f; // Extra time to jump after leaving ground
    
    private Camera playerCamera;
    private CharacterController characterController;
    
    private float verticalRotation = 0;
    private float verticalVelocity = 0;
    
    private float jumpBufferCounter = 0;
    private float coyoteTimeCounter = 0;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        if (characterController == null)
            Debug.LogError("CharacterController not found!");
        if (playerCamera == null)
            Debug.LogError("Camera not found!");
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }
    
    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        moveDirection *= speed;
        
        // Jump input buffering
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        
        // Coyote time (grace period after leaving ground)
        if (characterController.isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            verticalVelocity = -2f; // Small downward force to keep grounded
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        
        // Check for jump (with buffer and coyote time)
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            verticalVelocity = jumpForce;
            jumpBufferCounter = 0; // Consume the jump
            Debug.Log("Jump executed!");
        }
        
        // Apply gravity
        if (!characterController.isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        
        moveDirection.y = verticalVelocity;
        characterController.Move(moveDirection * Time.deltaTime);
    }
    
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        transform.Rotate(0, mouseX, 0);
        
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}