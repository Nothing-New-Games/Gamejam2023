using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWandering : State
{
    public BasicWandering(Dictionary<State, Transition> possibleTransitions, bool MustWanderFromSpawn, float maxWanderDist) : base(possibleTransitions)
    {
        _PossibleTransitions = possibleTransitions;
        _CanOnlyWanderFromSpawnPoint = MustWanderFromSpawn;
        _MaxWanderDistance = maxWanderDist;
        _Rigidbody = _Parent.GetComponent<Rigidbody>();
    }

    GameObject _Parent;
    Vector3 _SpawnPosition;
    float _MaxWanderDistance;
    /// <summary>
    /// If false, the current position of the creature will be used instead.
    /// </summary>
    bool _CanOnlyWanderFromSpawnPoint;
    Rigidbody _Rigidbody;

    Vector3 _CurrentDestination = new();

    public override void OnUpdateState()
    {
        //Check if we have a destination
        if (_CurrentDestination == new Vector3())
        {
            //If not, check if we can wander from the only from the spawn point,
            if (!_CanOnlyWanderFromSpawnPoint)
            {
                //Then choose a destination within the wander distance of there.
                _CurrentDestination =
                    new Vector3(Random.Range(-_MaxWanderDistance, _MaxWanderDistance), 0, Random.Range(-_MaxWanderDistance, _MaxWanderDistance))
                    +
                    _SpawnPosition;
            }
            else
            {
                //Choose a destination from our current position.
                _CurrentDestination =
                    new Vector3(Random.Range(-_MaxWanderDistance, _MaxWanderDistance), 0, Random.Range(-_MaxWanderDistance, _MaxWanderDistance))
                    +
                    _Parent.transform.position;
            }
        }

        //Use the rigid body component to move to the desired destination.
        _Rigidbody.MovePosition(_CurrentDestination);

        //Loop through all transitions
        foreach (var transition in _PossibleTransitions)
        {
            //Check if any conditions are met.
            if (transition.Value.CheckCondition())
            {
                //Set the new condition to the key
            }
        }
    }

    public override void Initialize(GameObject parent)
    {
        _Parent = parent;
        _SpawnPosition = parent.transform.position;
    }

    
    //Needs to know how far they can wander.
    //Needs to have a distance it can be from the destination.
    //Chooses a random destination from within a range of itself or the spawning position.
    //Boolean to determine if they choose a position from their spawn or current position.
    //Needs to know the movement speed that it can move at. (Reference to the defaults?)
}
