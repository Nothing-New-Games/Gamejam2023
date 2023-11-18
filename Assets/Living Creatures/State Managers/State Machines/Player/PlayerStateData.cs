using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateData : StateData
{
    public PlayerStateData(Transform actorTransform) : base(actorTransform)
    {
        ActorTransform = actorTransform;
        Engine = new(ActorTransform);
    }

    public PlayerEngine Engine;
}
