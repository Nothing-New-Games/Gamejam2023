using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAirborn : State
{
    public PlayerAirborn()
    {
        
    }

    public override State OnUpdateState(ref StateData passedData)
    {
        PlayerStateData data = passedData as PlayerStateData;


        data.Engine.HandleMovement(Player.GetPlayerInstance.MovementForceMultiplier /2);


        passedData = data;
        return OnLateUpdateState(ref passedData);
    }
}
