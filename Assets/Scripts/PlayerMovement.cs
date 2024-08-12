using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //declare reference variables
    PlayerInput playerInput;
    CharacterController characterController;
    Animator playerAnimator;

    //variables to store optimized setter/getter parameter IDs
    int isRunningHash;

    //Variables to store player input values
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    bool isMovementPressed;

    //constants
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationFactorPerFrame = 5f;

    //gravity variables
    private float gravity = -9.8f;
    private float groundedGravity = -0.5f;

    //jumping variables
    bool isJumpPressed = false;
    private float initialJumpVelocity;
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] private float maxJumpTime = 1f;
    bool isJumping = false;
    int isJumpingHash;
    bool isJumpAnimating = false;

    private void Awake()
    {
        //initialize set reference variables
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        playerAnimator = GetComponent<Animator>();

        //set the parameter hash references
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        //set the player input callbacks
        playerInput.PlayerControls.Movement.started += OnMovementInput;
        playerInput.PlayerControls.Movement.canceled += OnMovementInput;
        playerInput.PlayerControls.Movement.performed += OnMovementInput;
        playerInput.PlayerControls.Jump.started += OnJump;
        playerInput.PlayerControls.Jump.canceled += OnJump;

        SetupJumpVariables();
    }

    private void SetupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    private void handleJump()
    {
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {
            playerAnimator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping = true;
            currentMovement.y = initialJumpVelocity * 0.5f;
        }
        else if (!isJumpPressed && isJumping && characterController.isGrounded)
        {
            isJumping = false;
        }
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    private void HandleRotation()
    {
        Vector3 positionToLookAt;
        //the change in position our character should point to
        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        //the current rotation of our character
        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    private void HandleAnimation()
    {
        bool isRunning = playerAnimator.GetBool(isRunningHash);

        if (isMovementPressed && !isRunning)
        {
            playerAnimator.SetBool(isRunningHash, true);
        }

        else if (!isMovementPressed && isRunning)
        {
            playerAnimator.SetBool(isRunningHash, false);
        }
    }

    private void HandleGravity()
    {
        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplier = 2.0f;

        //apply gravity depending on if the character is on ground or not
        if (characterController.isGrounded)
        {
            if (isJumpAnimating)
            {
                playerAnimator.SetBool(isJumpingHash, true);
                isJumpAnimating = false;
            }
            playerAnimator.SetBool(isJumpingHash, false);

            currentMovement.y = groundedGravity * Time.deltaTime;
        }
        else if (isFalling)
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (gravity * fallMultiplier * Time.deltaTime);
            float nextYVelocity = Mathf.Max((previousYVelocity + newYVelocity) * 0.5f, -20.0f);
            currentMovement.y = nextYVelocity;
        }
        else
        {
            float previousYVelocity = currentMovement.y;
            float newVelocity = currentMovement.y + (gravity * Time.deltaTime);
            float nextVelocity = (previousYVelocity + newVelocity) * 0.5f;
            currentMovement.y = nextVelocity;
        }

    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleAnimation();
        characterController.Move(currentMovement * speed * Time.deltaTime);
        HandleGravity();
        handleJump();
    }

    private void OnEnable()
    {
        // Enable the player controls action map
        playerInput.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.PlayerControls.Disable();
    }
}
