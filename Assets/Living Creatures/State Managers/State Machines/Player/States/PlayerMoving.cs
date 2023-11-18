using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoving : State
{
    public PlayerMoving()
    {
        
    }

    public override State OnUpdateState(ref StateData passedData)
    {
        PlayerStateData data = passedData as PlayerStateData;


        data.Engine.HandleMovement(Player.GetPlayerInstance.MovementForceMultiplier);

        if (Input.GetKeyDown(KeyCode.Space))
            data.Engine.HandleJump(Player.GetPlayerInstance.JumpForceMultiplier);


        passedData = data;

        return OnLateUpdateState(ref passedData);
    }
}
