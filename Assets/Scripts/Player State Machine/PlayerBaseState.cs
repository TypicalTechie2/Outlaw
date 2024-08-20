public abstract class PlayerBaseState
{
    protected PlayerStateMachine _contexts;
    protected PlayerStateFactory _factory;
    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        _contexts = currentContext;
        _factory = playerStateFactory;
    }
    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public abstract void CheckSwitchStates();

    public abstract void InitializeSubState();

    void UpdateStates()
    {

    }

    protected void SwitchState(PlayerBaseState newState)
    {
        // current state exits state
        ExitState();

        // new state enters new state
        newState.EnterState();

        // switch current state of context
        _contexts.CurrentState = newState;
    }

    protected void SetSuperState()
    {

    }

    protected void SetSubState()
    {

    }
}
