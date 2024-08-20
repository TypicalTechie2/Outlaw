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
    Vector3 appliedMovement;
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

    // callback handler function to set the player input values
    private void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    //callback handler function for jump button
    private void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    private void HandleAnimation()
    {
        // get parameter values from  animator
        bool isRunning = playerAnimator.GetBool(isRunningHash);

        //start running if movement pressed is true and not already running
        if (isMovementPressed && !isRunning)
        {
            playerAnimator.SetBool(isRunningHash, true);
        }

        //stop running if is movementPressed is false and not already running
        else if (!isMovementPressed && isRunning)
        {
            playerAnimator.SetBool(isRunningHash, false);
        }
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

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleAnimation();

        appliedMovement.x = currentMovement.x;
        appliedMovement.z = currentMovement.z;
        characterController.Move(appliedMovement * speed * Time.deltaTime);

        HandleGravity();
        handleJump();
    }

    // set the initial velocity and gravity using jump heights and duration
    private void SetupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    // launch character into the air with initial vertical velocity if conditions met
    private void handleJump()
    {
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {
            playerAnimator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping = true;
            currentMovement.y = initialJumpVelocity;
            appliedMovement.y = initialJumpVelocity;
        }
        else if (!isJumpPressed && isJumping && characterController.isGrounded)
        {
            isJumping = false;
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
                playerAnimator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;
            }

            currentMovement.y = groundedGravity;
            appliedMovement.y = groundedGravity;

        }
        else if (isFalling)
        {
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (gravity * fallMultiplier * Time.deltaTime);
            appliedMovement.y = Mathf.Max((previousYVelocity + currentMovement.y) * 0.5f, -20.0f);
        }
        else
        {
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (gravity * Time.deltaTime);
            appliedMovement.y = (previousYVelocity + currentMovement.y) * 0.5f;
        }

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
