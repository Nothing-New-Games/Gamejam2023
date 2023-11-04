using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;

#region Chaos
//public abstract class StateMachine : MonoBehaviour
//{
//    /// <summary>
//    /// The current state being used by the state machine.
//    /// </summary>
//    private State _CurrentState;
//    /// <summary>
//    /// Calls the update method of the current state.
//    /// </summary>
//    private void Update() => _CurrentState?.Update();

//    /// <summary>
//    /// Exits from the current state, if there is one, and sets the current state to the new state, then calls the current states enter method.
//    /// </summary>
//    /// <param name="newState"></param>
//    private void SetState(State newState)
//    {
//        _CurrentState?.Exit();
//        _CurrentState = newState;
//        _CurrentState.Enter();
//    }

//    /// <summary>
//    /// Initializes the state machine and sets the current state to the initial state.
//    /// </summary>
//    /// <param name="initialState">The state the machine will start with.</param>
//    /// <param name="states">A dictionary of states and states it can switch to along with the transition to exit that state.</param>
//    protected void Init(State initialState, Dictionary<State, Dictionary<StateTransition, State>> states)
//    {
//        foreach (var state in states)
//        {
//            foreach (var transition in state.Value)
//            {
//                transition.Key.Callback = () => SetState(transition.Value);
//                state.Key.AddTransition(transition.Key);
//            }
//        }

//        SetState(initialState);
//    }
//}
//public abstract class State
//{
//    /// <summary>
//    /// List of all transitions belonging to this state.
//    /// </summary>
//    private List<StateTransition> _Transitions = new();

//    /// <summary>
//    /// Assigsn the transition to the state.
//    /// </summary>
//    public void AddTransition(StateTransition transition) => _Transitions.Add(transition);

//    /// <summary>
//    /// State Enter Method. Calls the enter for all state transitions.
//    /// </summary>
//    public virtual void Enter()
//    {
//        foreach (var transition in _Transitions)
//            transition.Enter();
//    }

//    /// <summary>
//    /// State Exit Method. Calls the exit for all state transitions.
//    /// </summary>
//    public virtual void Exit()
//    {
//        foreach (var transition in _Transitions)
//            transition.Exit();
//    }

//    /// <summary>
//    /// State Update Method.
//    /// <para>Calls the update for all state transitions under it.</para>
//    /// </summary>
//    public virtual void Update()
//    {
//        foreach (var transition in _Transitions)
//            transition.Update();
//    }
//}
//public abstract class StateTransition
//{
//    /// <summary>
//    /// The callback for when the transition can exit.
//    /// </summary>
//    public Action Callback { get; set; }

//    /// <summary>
//    /// Checks the condition to determine if we can exit the state.
//    /// </summary>
//    /// <returns>
//    /// <see langword="true"/> if we can exit.
//    /// <para><see langword="false"/> if we cannot exit.</para>
//    /// </returns>
//    public abstract bool CheckCondition();

//    /// <summary>
//    /// State Transition Enter
//    /// </summary>
//    public virtual void Enter() { }

//    /// <summary>
//    /// State Transition Exit
//    /// </summary>
//    public virtual void Exit() { }

//    /// <summary>
//    /// State Transition Update method
//    /// </summary>
//    public virtual void Update() { }
//}
#endregion

/// <summary>
/// OnEnterCurrentState, OnLeaveCurrentState, Update, OnStateAwake
/// </summary>
public class StateManager
{
    public StateManager(State initialState, GameObject parent)
    {
        foreach (var state in PossibleStates)
        {
            state.Initialize(parent);
        }

        _CurrentState = initialState;
    }

    private State _CurrentState;

    [Searchable, TabGroup("Customization"), ShowInInspector, OdinSerialize]
    public List<State> PossibleStates = new List<State>();

    public virtual void OnStartCurrentState() => _CurrentState?.OnEnterState();
    public virtual void OnLeaveCurrentState() => _CurrentState?.OnLeaveState();
    public void Update() => _CurrentState?.OnUpdateState();
    public void LateUpdate() => _CurrentState?.OnLateUpdateState();
    public virtual void OnStateAwake() => _CurrentState?.OnStateAwake();
}

/// <summary>
/// OnEnter, OnLeave, OnUpdate, OnLateUpdateState, OnStateAwake, CheckCondition
/// </summary>
public abstract class State
{
    public State(Dictionary<State, Transition> possibleTransitions) { }

    protected Dictionary<State, Transition> _PossibleTransitions;
    protected GameObject _Parent;
    public void SetParent(GameObject parent)
    {
        if (_Parent == null)
            _Parent = parent;
    }

    public abstract void Initialize(GameObject parent);
    public virtual void OnEnterState() { }
    public virtual void OnLeaveState() { }
    public virtual void OnUpdateState() { }
    public virtual void OnLateUpdateState() { }
    public virtual void OnStateAwake() { }
}

public abstract class Transition
{
    public Transition(State nextState) => _NextState = nextState;

    private Action OnConditionMet;

    private State _NextState;

    protected abstract void SetConditionMetResponse(Action onConditionMet);

    public abstract bool CheckCondition();
}