using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    //declare reference variables
    PlayerInput _playerInput;
    CharacterController _characterController;
    Animator _playerAnimator;

    //variables to store optimized setter/getter parameter IDs
    int _isRunningHash;

    //Variables to store player input values
    Vector2 _currentMovementInput;
    Vector3 _currentMovement;
    Vector3 _appliedMovement;
    bool _isMovementPressed;

    //constants
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationFactorPerFrame = 5f;

    //gravity variables
    float _gravity = -9.8f;
    float _groundedGravity = -0.5f;

    //jumping variables
    bool _isJumpPressed = false;
    float _initialJumpVelocity;
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] private float maxJumpTime = 1f;
    bool _isJumping = false;
    int _isJumpingHash;
    bool _isJumpAnimating = false;

    // state variables

    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    // getters and setters
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public bool IsJumpPressed { get { return _isJumpPressed; } }
    public Animator PlayerAnimator { get { return _playerAnimator; } }
    public CharacterController CharacterController { get { return _characterController; } }
    public int IsJumpingHash { get { return _isJumpingHash; } }
    public bool IsJumpAnimating { set { _isJumpAnimating = value; } }
    public bool IsJumping { set { _isJumping = value; } }
    public float CurrentMovementY { get { return _currentMovement.y; } set { _currentMovement.y = value; } }
    public float AppliedMovementY { get { return _appliedMovement.y; } set { _appliedMovement.y = value; } }
    public float InitialJumpVelocity { get { return _initialJumpVelocity; } }
    public float GroundedGravity { get { return _groundedGravity; } }
    public float Gravity { get { return _gravity; } }


    private void Awake()
    {
        //initialize set reference variables
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _playerAnimator = GetComponent<Animator>();

        // setup state
        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        //set the parameter hash references
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");

        //set the player input callbacks
        _playerInput.PlayerControls.Movement.started += OnMovementInput;
        _playerInput.PlayerControls.Movement.canceled += OnMovementInput;
        _playerInput.PlayerControls.Movement.performed += OnMovementInput;
        _playerInput.PlayerControls.Jump.started += OnJump;
        _playerInput.PlayerControls.Jump.canceled += OnJump;

        SetupJumpVariables();
    }

    // set the initial velocity and gravity using jump heights and duration
    private void SetupJumpVariables()
    {
        float _timeToApex = maxJumpTime / 2;
        _gravity = (-2 * maxJumpHeight) / Mathf.Pow(_timeToApex, 2);
        _initialJumpVelocity = (2 * maxJumpHeight) / _timeToApex;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        _currentState.UpdateState();
        _characterController.Move(_appliedMovement * speed * Time.deltaTime);
    }

    // callback handler function to set the player input values
    private void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x;
        _currentMovement.z = _currentMovementInput.y;
        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    }

    //callback handler function for jump button
    private void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
    }

    private void HandleRotation()
    {
        Vector3 positionToLookAt;
        //the change in position our character should point to
        positionToLookAt.x = _currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = _currentMovement.z;

        //the current rotation of our character
        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }

    private void OnEnable()
    {
        // Enable the player control action map
        _playerInput.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        // disable the player control action map
        _playerInput.PlayerControls.Disable();
    }
}
