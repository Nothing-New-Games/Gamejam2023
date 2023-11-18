using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIsMoving : Transition
{
    public PlayerIsMoving(State nextState) : base(nextState)
    {
    }

    public override bool CheckCondition(ref StateData passedData)
    {
        bool PlayerIsNull = Player.GetPlayerInstance == null;
        if (!PlayerIsNull)
            return (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                && Player.GetPlayerInstance.IsGrounded;


        return false;
    }
}
