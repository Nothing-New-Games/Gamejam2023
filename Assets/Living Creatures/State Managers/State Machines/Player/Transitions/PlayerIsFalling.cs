using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIsFalling : Transition
{
    public PlayerIsFalling(State nextState) : base(nextState)
    {
    }

    public override bool CheckCondition(ref StateData passedData)
    {
        bool PlayerIsNull = Player.GetPlayerInstance == null;
        if (!PlayerIsNull)
            return !Player.GetPlayerInstance.IsGrounded;

        return false;
    }
}
