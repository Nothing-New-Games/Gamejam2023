using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OnEnterCurrentState, OnLeaveCurrentState, Update, OnStateAwake
/// </summary>
public class StateManager
{
    public StateData Data;
    public StateManager(string startingTransition, StateData data)
    {
        Data = data;
        _CurrentState = Data.GetTransition(startingTransition).RetrieveNextState();
    }

    public string CurrentStateName => _CurrentState.ToString();

    private State _CurrentState { get; set; }
    public bool IsStateEmpty => _CurrentState == null;


    public void  OnStartCurrentState() => _CurrentState = _CurrentState?.OnEnterState(ref Data);
    public void OnLeaveCurrentState() => _CurrentState = _CurrentState?.OnLeaveState(ref Data);
    public void OnUpdate() => _CurrentState = _CurrentState?.OnUpdateState(ref Data);
    public void OnLateUpdate() => _CurrentState = _CurrentState?.OnLateUpdateState(ref Data);
    public void OnStateAwake() => _CurrentState = _CurrentState?.OnStateAwake(ref Data);
}

/// <summary>
/// OnEnter, OnLeave, OnUpdate, OnLateUpdateState, OnStateAwake, CheckCondition
/// </summary>
public abstract class State
{
    public virtual State OnEnterState(ref StateData data) { return this; }
    public virtual State OnLeaveState(ref StateData data) { return this; }
    public virtual State OnLateUpdateState(ref StateData data)
    {
        foreach (var collection in data.GetTransitions)
        {
            if (collection.Value.CheckCondition(ref data))
            {
                return collection.Value.RetrieveNextState();
            }
        }

        return this;
    }
    public virtual State OnUpdateState(ref StateData data) { return this; }
    public virtual State OnStateAwake(ref StateData data) { return this; }
}

public abstract class Transition
{
    public Transition(State nextState) => _NextState = nextState;

    protected State _NextState;

    public abstract bool CheckCondition(ref StateData passedData);

    public State RetrieveNextState() => _NextState;
}

public class StateData
{
    public Transform ActorTransform;
    public StateData(Transform actorTransform) => ActorTransform = actorTransform;

    
    private Dictionary<string, Transition> _AllTransitions;
    public Dictionary<string, Transition> GetTransitions => _AllTransitions;

    public Dictionary<string, Transition> SetTransitions { set { if (_AllTransitions == null) _AllTransitions = value; } }
    public Transition GetTransition(string transitionName) => _AllTransitions[transitionName];
}