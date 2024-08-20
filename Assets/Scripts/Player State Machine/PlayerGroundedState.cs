using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{

    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {

    }
    public override void EnterState()
    {
        _contexts.AppliedMovementY = _contexts.GroundedGravity;
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {

    }

    public override void CheckSwitchStates()
    {
        if (_contexts.IsJumpPressed)
        {
            SwitchState(_factory.Jump());
        }
    }
}
