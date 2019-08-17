
public class StateMachine<TInput>
{
    public State<TInput> currentState;

    public StateMachine(State<TInput> initialState)
    {
        this.currentState = initialState;
        currentState.OnEnter();
    }

    public void Update()
    {
        currentState.OnUpdate();
    }

    public void Feed(TInput input)
    {
        var nextState = currentState.GetTransition(input);
        if (nextState != null)
        {
            currentState.OnExit();
            currentState = nextState;
            currentState.OnEnter();
        }
    }
}
