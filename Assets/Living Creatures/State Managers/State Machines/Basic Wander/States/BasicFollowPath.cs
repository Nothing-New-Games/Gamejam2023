using PigeonCarrier;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WatchDog;

public class BasicFollowPath : State
{
    public override State OnUpdateState(ref StateData passedData)
    {
        BasicMovementData data = (BasicMovementData)passedData;

        if (data.CurrentPathPoint == null) data.CurrentPathPoint = data.Actor.PathPoints.First();

        //Use the rigid body component to move to the desired destination.
        if (!Player.IsGamePaused && Vector3.Distance(data.ActorTransform.position, data.CurrentPathPoint.Point) > data.Actor.MinDistToDest)
        {
            //Rotate
            Quaternion targetRotation = Quaternion.LookRotation(data.CurrentPathPoint.Point - data.ActorTransform.position);
            data.ActorTransform.rotation = Quaternion.Slerp(data.ActorTransform.rotation, targetRotation, data.Actor.RotationSpeed * Time.deltaTime);

            //Capture the rigidbody
            Rigidbody rb = data.ActorTransform.GetComponent<Rigidbody>();

            //Move Towards
            rb.velocity =
                (data.CurrentPathPoint.Point - data.ActorTransform.position + data.ActorTransform.forward * Time.fixedDeltaTime).normalized * data.Actor.MovementForceMultiplier;
            //.AddForce(data.CurrentDestination - data.ActorTransform.position * data.MovementSpeed * Time.fixedDeltaTime);
        }
        else if (!Player.IsGamePaused) //Look around
        {
            data.CurrentIdleTime += Time.deltaTime;

            if (data.CurrentIdleTime >= data.CurrentPathPoint.DelayBeforeMovingToNextPoint)
            {
                if (data.Actor.PathPoints.IndexOf(data.CurrentPathPoint) +1 >= data.Actor.PathPoints.Count)
                    data.CurrentPathPoint = data.Actor.PathPoints[0];
                else
                    data.CurrentPathPoint = data.Actor.PathPoints[data.Actor.PathPoints.IndexOf(data.CurrentPathPoint) +1];
                Watchdog.LogMessageCallback.Invoke(new(new EventMessage($"{data.ActorTransform.name} has waited long enough. Moving on to next pos.", LogTypes.Debug)));
                data.CurrentIdleTime = 0f;
            }
            Debug.Log($"{data.ActorTransform.name} is supposed to be looking around, but is not coded yet!");
        }

        //Convert the data back so that we may pass it to the returned state.
        passedData = data;

        return OnLateUpdateState(ref passedData);
    }
}
