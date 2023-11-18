using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicIdle : State
{   
    public override State OnUpdateState(ref StateData passedData) => OnLateUpdateState(ref passedData);
}
