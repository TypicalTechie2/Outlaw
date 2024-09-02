using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //declare reference variables
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    // variables to store optimized setter/getter parameter IDs
    int isRunninHash;
    int isJumpingHash;

    // variables to store plaer input values
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 appliedMovement;
    Vector3 cameraRelativeMovement;

    bool isMovementPressed;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationFactorPerFrame = 1.0f;

    // gravity variables
    float gravity = -9.81f;
    float groundedGravity = -0.05f;

    // jumping variables
    bool isJumpPressed = false;
    float initialJumpVelocity;
    [SerializeField] private float maxJumpHeight = 2f;
    [SerializeField] private float maxJumpTime = 1.5f;
    bool isJumping = false;
    bool isJumpAnimating = false;



    private void Awake()
    {
        // initially set reference variables
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isRunninHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        // set the player input callbacks
        playerInput.PlayerControlsIP.Move.started += OnMovementInput;
        playerInput.PlayerControlsIP.Move.canceled += OnMovementInput;
        playerInput.PlayerControlsIP.Move.performed += OnMovementInput;
        playerInput.PlayerControlsIP.Jump.started += OnJumpInput;
        playerInput.PlayerControlsIP.Jump.canceled += OnJumpInput;

        SetupJumpVariables();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void SetupJumpVariables()
    {
        float _timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(_timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / _timeToApex;
    }

    // Update is called once per frame
    void Update()
    {

        HandleROtation();
        HandleAnimation();

        appliedMovement.x = currentMovement.x;
        appliedMovement.z = currentMovement.z;

        cameraRelativeMovement = ConvertToCameraSpace(appliedMovement);
        characterController.Move(cameraRelativeMovement * moveSpeed * Time.deltaTime);

        HandleGravity();
        HandleJump();
    }



    void HandleJump()
    {
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {
            animator.SetBool(isJumpingHash, true);
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
    // handle function to set the player input values
    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void HandleROtation()
    {
        Vector3 _positionToLookAt;

        _positionToLookAt.x = cameraRelativeMovement.x;
        _positionToLookAt.y = 0.0f;
        _positionToLookAt.z = cameraRelativeMovement.z;

        Quaternion _currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_positionToLookAt);
            transform.rotation = Quaternion.Slerp(_currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }

    void OnJumpInput(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    void HandleAnimation()
    {
        bool isRunning = animator.GetBool(isRunninHash);

        if (isMovementPressed && !isRunning)
        {
            animator.SetBool(isRunninHash, true);
        }

        else if (!isMovementPressed && isRunning)
        {
            animator.SetBool(isRunninHash, false);
        }
    }

    void HandleGravity()
    {
        bool _isFalling = currentMovement.y <= 0.0f;
        float fallMultiplier = 2.0f;

        // apply proper gravity depending on if the player is grounded or not
        if (characterController.isGrounded)
        {
            animator.SetBool(isJumpingHash, false);
            isJumpAnimating = false;
            appliedMovement.y = groundedGravity;
        }
        else if (_isFalling)
        {
            float _previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (gravity * fallMultiplier * Time.deltaTime);
            appliedMovement.y = Mathf.Max(_previousYVelocity + currentMovement.y) * 0.5f - 0.5f;
        }
        else
        {
            float _previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (gravity * Time.deltaTime);
            appliedMovement.y = (_previousYVelocity + currentMovement.y) * 0.5f;
        }
    }

    Vector3 ConvertToCameraSpace(Vector3 vectorToRotate)
    {
        // store Y value of the original vector to rotate
        float _currentYValue = vectorToRotate.y;

        // get the forward and right directional vectors of the camera 
        Vector3 _cameraForward = Camera.main.transform.forward;
        Vector3 _cameraRight = Camera.main.transform.right;

        // remove the y values to ignore upward/downward camera angles
        _cameraForward.y = 0;
        _cameraRight.y = 0;

        // re-normalize both vectors so they each have a magnitude of 1
        _cameraForward = _cameraForward.normalized;
        _cameraRight = _cameraRight.normalized;

        // rotate the X and Z vectorToRotate values to camera space
        Vector3 _cameraForwardZ = vectorToRotate.z * _cameraForward;
        Vector3 _cameraRightX = vectorToRotate.x * _cameraRight;

        // the sum of both products is the Vector3 in camera space
        Vector3 _vectorRotatedToCameraSpace = _cameraForwardZ + _cameraRightX;
        _vectorRotatedToCameraSpace.y = _currentYValue;
        return _vectorRotatedToCameraSpace;
    }

    private void OnEnable()
    {
        playerInput.PlayerControlsIP.Enable();
    }

    private void OnDisable()
    {
        playerInput.PlayerControlsIP.Disable();
    }
}
