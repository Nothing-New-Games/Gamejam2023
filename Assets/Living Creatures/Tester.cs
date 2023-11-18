using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Tester : Creature, IPickupAble
{
    public bool IsTalkable => false;

    public bool IsAttackable => false;

    [ReadOnly, LabelText("Current Destination:"), DisplayAsString, ShowInInspector]
    [TabGroup("Main", "Debug")]
    public string CurerntDestinationAsString => GetDestination();
    private string GetDestination()
    {
        if (_StateManager == null)
            return "No destionation.";

        return ((BasicMovementData)(_StateManager.Data)).GetDestAsString;
    }

    public override async Task OnAwake()
    {
        ActorTransform = transform;

        _StateManager = new("Idle", new BasicMovementData(transform)
        {
            SetTransitions = new()
            {
                { "Idle", new BasicNearDestination(new BasicIdle()) },
                { "Wandering", new BasicIdledForDuration(new BasicWandering()) },
                { "FollowingPath", new BasicIdledForDuration(new BasicFollowPath()) }
            },
            MinDistToDest = MinDistToDest,
            IsFreeRoaming = IsFreeRoaming,
            IsFollowingPath = FollowsRoute,
            SpawnPoint = transform.position,
            MaxWanderDist = MaxWanderDist,
            CurrentDestination = transform.position,
            MinIdleDuration = MinIdleDuration,
            MaxIdleDuration = MaxIdleDuration,
            MovementSpeed = MovementForceMultiplier,
            RotationSpeed = RotationSpeed,
            PathPoints = PathPoints,
        });

        await Task.Yield();
    }





    [FoldoutGroup("Main/Debug/Toggles"), LabelWidth(300)]
    [Tooltip("Draws a sphere where the destination is, with a width of the min distance to the destination.")]
    public bool DrawDestination = false;

    [FoldoutGroup("Main/Debug/Toggles"), LabelWidth(300)]
    [Tooltip("Draws a sphere where the spawn point is located.")]
    public bool DrawSpawnPoint = false;

    [FoldoutGroup("Main/Debug/Toggles"), LabelWidth(300)]
    [Tooltip("Draws a sphere to show where the creature can wander within..")]
    public bool DrawWanderZone = false;

    protected override void OnDrawGizmos()
    {
        if (_StateManager != null)
        {
            if (DrawGizmos)
            {

                var basicData = ((BasicMovementData)(_StateManager.Data));
                if (DrawDestination && _StateManager.CurrentStateName != "BasicIdle")
                {
                    Gizmos.color = Color.cyan;
                        /*ColorByDistance(basicData.MinDistToDest / Vector3.Distance(transform.position, basicData.CurrentDestination));*/
                    Gizmos.DrawWireSphere(basicData.CurrentDestination, basicData.MinDistToDest * 3);
                }
                if (DrawSpawnPoint)
                {
                    Gizmos.color = Color.green;
                        /*ColorByDistance(basicData.MinDistToDest / Vector3.Distance(transform.position, basicData.CurrentDestination));*/
                    Gizmos.DrawWireSphere(basicData.SpawnPoint, 1);
                }
                if (DrawWanderZone)
                {
                    Gizmos.color = Color.red;
                    /*ColorByDistance(basicData.MinDistToDest / Vector3.Distance(transform.position, basicData.CurrentDestination));*/
                    if (!basicData.IsFreeRoaming)
                        Gizmos.DrawWireSphere(basicData.SpawnPoint, basicData.MaxWanderDist);
                    else if (basicData.CurrentDestination != new Vector3())
                        Gizmos.DrawWireSphere(basicData.CurrentDestination, basicData.MaxWanderDist);
                    else
                        Gizmos.DrawWireSphere(basicData.ActorTransform.position, basicData.MaxWanderDist);
                }
            }
        }

        base.OnDrawGizmos();
    }
}
