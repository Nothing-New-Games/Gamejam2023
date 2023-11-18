using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovementData : StateData
{
    //A good amount of these can be removed and instead taken directly from the creature class through the transform.

    public float MinDistToDest;

    public bool IsFreeRoaming;
    public bool IsFollowingPath;

    public Vector3 SpawnPoint { get; set; }

    public float MaxWanderDist;

    public Vector3 CurrentDestination { get; set; }

    public float MovementSpeed;
    public float RotationSpeed;

    public float MinIdleDuration;
    public float MaxIdleDuration;

    public float CurrentIdleTime;
    public float ChosenIdleTime;

    public List<CreaturePathPoint> PathPoints;
    public CreaturePathPoint CurrentPathPoint;

    public BasicMovementData(Transform actorTransform) : base(actorTransform)
    {
    }

    public string GetDestAsString => CurrentDestination.ToString();
}