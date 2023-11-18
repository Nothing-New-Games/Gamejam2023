using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BasicIdledForDuration : Transition
{
    public BasicIdledForDuration(State nextState) : base(nextState)
    {

    }

    public override bool CheckCondition(ref StateData passedData)
    {
        BasicMovementData data = (BasicMovementData)passedData;

        if (!Player.IsGamePaused)
            data.CurrentIdleTime += Time.deltaTime;

        if (data.CurrentIdleTime >= data.ChosenIdleTime)
        {
            data.ChosenIdleTime = Random.Range(data.MinIdleDuration, data.MaxIdleDuration);
            data.CurrentIdleTime = 0f;

            if (data.IsFollowingPath && _NextState.ToString() == "BasicFollowPath")
                return true;
            else if (!data.IsFollowingPath && _NextState.ToString() == "BasicWandering")
                return true;
        }

        return false;
    }
}
