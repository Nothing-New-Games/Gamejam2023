using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovementData : StateData
{
    public Vector3 SpawnPoint { get; set; }

    public Vector3 CurrentDestination { get; set; }

    public float CurrentIdleTime;
    public float ChosenIdleTime;

    public CreaturePathPoint CurrentPathPoint;

    public Creature Actor { get; set; }

    public BasicMovementData(Transform actorTransform) : base(actorTransform)
    {
    }

    public string GetDestAsString => CurrentDestination.ToString();
}