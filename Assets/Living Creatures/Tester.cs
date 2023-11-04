using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : Creature
{
    private void Awake()
    {
        SetInitialState(new BasicIdle());
    }
}
