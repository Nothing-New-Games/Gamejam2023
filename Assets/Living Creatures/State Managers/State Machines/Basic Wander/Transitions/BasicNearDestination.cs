using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNearDestination : Transition
{
    public BasicNearDestination(State nextState) : base(nextState)
    {
        _NextState = nextState;
    }

    public override bool CheckCondition(ref StateData passedData)
    {
        BasicMovementData data = (BasicMovementData)passedData;
        bool closeEnough = Vector3.Distance(data.ActorTransform.position, data.CurrentDestination) < data.MinDistToDest;

        if (closeEnough)
        {
            data.CurrentDestination = new();
            data.ActorTransform.GetComponent<Rigidbody>().velocity = new();
            passedData = data;
        }

        return closeEnough;
    }
}
