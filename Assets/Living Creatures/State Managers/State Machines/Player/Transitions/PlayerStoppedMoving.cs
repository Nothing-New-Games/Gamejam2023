using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStoppedMoving : Transition
{
    public PlayerStoppedMoving(State nextState) : base(nextState)
    {

    }

    public override bool CheckCondition(ref StateData passedData)
    {
        bool PlayerIsNull = Player.GetPlayerInstance == null;
        if (!PlayerIsNull) 
            return 
                Input.GetAxis("Horizontal") == 0 
                && Input.GetAxis("Vertical") == 0 
                && Player.GetPlayerInstance.IsGrounded;
        else return false;
    }
}
