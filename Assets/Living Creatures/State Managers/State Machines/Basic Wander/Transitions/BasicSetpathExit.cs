using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSetpathExit : Transition
{
    public BasicSetpathExit(State nextState) : base(nextState)
    {
    }

    public override bool CheckCondition(ref StateData data)
    {
        return true;
    }
}
