using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Tester : Creature, IPickupAble
{
    public bool isBeingHeld { get; set; }

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
        //Player.HoldItemButtonClicked += OnPickup;

        _StateManager = new("Idle", new BasicMovementData(transform)
        {
            SetTransitions = new()
            {
                { "Idle", new BasicNearDestination(new BasicIdle()) },
                { "Wandering", new BasicIdledForDuration(new BasicWandering()) },
                { "FollowingPath", new BasicIdledForDuration(new BasicFollowPath()) }
            },
            SpawnPoint = transform.position,
            CurrentDestination = transform.position,
            Actor = this,
        });

        await Task.Yield();
    }

    public override async Task OnUpdate()
    {
        TemporaryCeaseStateFire = isBeingHeld;
        if (TemporaryCeaseStateFire)
        {
            _RB.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            _RB.constraints = RigidbodyConstraints.FreezeRotationY;
        }

        await Task.Yield();
    }



    [FoldoutGroup("Main/Debug/Gizmo Toggles"), LabelWidth(300)]
    [Tooltip("Draws a sphere where the destination is, with a width of the min distance to the destination.")]
    public bool DrawDestination = false;

    [FoldoutGroup("Main/Debug/Gizmo Toggles"), LabelWidth(300)]
    [Tooltip("Draws a sphere where the spawn point is located.")]
    public bool DrawSpawnPoint = false;

    [FoldoutGroup("Main/Debug/Gizmo Toggles"), LabelWidth(300)]
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
                    Gizmos.DrawWireSphere(basicData.CurrentDestination, MinDistToDest * 3);
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
                    if (!IsFreeRoaming)
                        Gizmos.DrawWireSphere(basicData.SpawnPoint, MaxWanderDist);
                    else if (basicData.CurrentDestination != new Vector3())
                        Gizmos.DrawWireSphere(basicData.CurrentDestination, MaxWanderDist);
                    else
                        Gizmos.DrawWireSphere(transform.position, MaxWanderDist);
                }
            }
        }

        base.OnDrawGizmos();
    }
}
