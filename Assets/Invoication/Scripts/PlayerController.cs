using UnityEngine;
using PurrNet;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    
    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float upDownRange = 60f;
    public float mouseSmoothTime = 0.03f;

    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;
    
    [Header("Jump Settings")]
    public float jumpBufferTime = 0.2f;
    public float coyoteTime = 0.15f;
    
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
        
        // ONLY lock cursor for local player
        if (isOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void Update()
    {
        if (!isOwner) return;
        if (PauseMenu.IsPaused) return;

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
        
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
        
        if (characterController.isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            verticalVelocity = -2f;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            verticalVelocity = jumpForce;
            jumpBufferCounter = 0;
        }
        
        if (!characterController.isGrounded)
            verticalVelocity -= gravity * Time.deltaTime;
        
        moveDirection.y = verticalVelocity;
        characterController.Move(moveDirection * Time.deltaTime);
    }
    
    void HandleMouseLook()
    {
        Vector2 targetMouseDelta = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );
        
        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            targetMouseDelta,
            ref currentMouseDeltaVelocity,
            mouseSmoothTime
        );
        
        float mouseX = currentMouseDelta.x * mouseSensitivity;
        float mouseY = currentMouseDelta.y * mouseSensitivity;
        
        transform.Rotate(0, mouseX, 0);
        
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}