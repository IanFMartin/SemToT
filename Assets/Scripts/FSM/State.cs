using System;
using System.Collections.Generic;

public class State<TInput>
{
    public string name;

    public Action OnEnter = delegate { };
    public Action OnUpdate = delegate { };
    public Action OnExit = delegate { };

    private Dictionary<TInput, State<TInput>> transitions = new Dictionary<TInput, State<TInput>>();

    public State(string name)
    {
        this.name = name;
    }

    public void AddTransition(TInput input, State<TInput> nextState)
    {
        transitions[input] = nextState;
    }

    public State<TInput> GetTransition(TInput input)
    {
        if (transitions.ContainsKey(input))
            return transitions[input];
        return null;
    }
}