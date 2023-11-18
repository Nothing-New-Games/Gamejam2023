using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdle : State
{
    public PlayerIdle()
    {
        
    }

    public override State OnUpdateState(ref StateData passedData)
    {
        PlayerStateData data = passedData as PlayerStateData;

        if (Input.GetKeyDown(KeyCode.Space))
            data.Engine.HandleJump(Player.GetPlayerInstance.JumpForceMultiplier);

        passedData = data;
        return OnLateUpdateState(ref passedData);
    }
}
