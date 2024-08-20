using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {

    }
    public override void EnterState()
    {
        handleJump();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        HandleGravity();
    }

    public override void ExitState()
    {
        _contexts.PlayerAnimator.SetBool(_contexts.IsJumpingHash, false);
        _contexts.IsJumpAnimating = false;
    }

    public override void InitializeSubState()
    {

    }

    public override void CheckSwitchStates()
    {
        if (_contexts.CharacterController.isGrounded)
        {
            SwitchState(_factory.Grounded());
        }
    }

    void handleJump()
    {
        _contexts.PlayerAnimator.SetBool(_contexts.IsJumpingHash, true);
        _contexts.IsJumpAnimating = true;
        _contexts.IsJumping = true;
        _contexts.CurrentMovementY = _contexts.InitialJumpVelocity;
        _contexts.AppliedMovementY = _contexts.InitialJumpVelocity;
    }

    void HandleGravity()
    {
        bool isFalling = _contexts.CurrentMovementY <= 0.0f || !_contexts.IsJumpPressed;
        float fallMultiplier = 2.0f;


        if (isFalling)
        {
            float previousYVelocity = _contexts.CurrentMovementY;
            _contexts.CurrentMovementY = _contexts.CurrentMovementY + (_contexts.Gravity * fallMultiplier * Time.deltaTime);
            _contexts.AppliedMovementY = Mathf.Max((previousYVelocity + _contexts.CurrentMovementY) * 0.5f, -20.0f);
        }
        else
        {
            float previousYVelocity = _contexts.CurrentMovementY;
            _contexts.CurrentMovementY = _contexts.CurrentMovementY + (_contexts.Gravity * Time.deltaTime);
            _contexts.CurrentMovementY = (previousYVelocity + _contexts.CurrentMovementY) * 0.5f;
        }
    }
}
