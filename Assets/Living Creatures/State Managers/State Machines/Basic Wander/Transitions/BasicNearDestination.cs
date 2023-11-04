using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNearDestination : Transition
{
    public BasicNearDestination(State nextState) : base(nextState)
    {
    }

    protected override bool CheckCondition()
    {
        throw new NotImplementedException();
    }

    protected override void SetConditionMetResponse(Action onConditionMet)
    {
        throw new NotImplementedException();
    }
}
