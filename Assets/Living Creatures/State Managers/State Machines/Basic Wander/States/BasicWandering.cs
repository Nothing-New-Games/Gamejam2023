using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BasicWandering : State
{
    public override State OnUpdateState(ref StateData passedData)
    {
        BasicMovementData data = passedData as BasicMovementData;

        //Check if we have a destination
        if (data.CurrentDestination == new Vector3())
        {
            //If not, check if we can wander from the only from the spawn point,
            if (data.Actor.IsFreeRoaming)
            {
                //Choose a destination from our current position.
                data.CurrentDestination = new Vector3(UnityEngine.Random.Range(-data.Actor.MaxWanderDist, data.Actor.MaxWanderDist), 0, UnityEngine.Random.Range(-data.Actor.MaxWanderDist, data.Actor.MaxWanderDist))
                    +
                    data.ActorTransform.position;
            }
            else
            {
                //Then choose a destination within the wander distance of there.
                data.CurrentDestination =
                    new Vector3(UnityEngine.Random.Range(-data.Actor.MaxWanderDist, data.Actor.MaxWanderDist), 0, UnityEngine.Random.Range(-data.Actor.MaxWanderDist, data.Actor.MaxWanderDist))
                    +
                    data.SpawnPoint;
            }

            //Raycast to determine if there are any obstacles in the way.
            RaycastHit hit;
            //Adjust the current position to the nearest point before the obstacle.
            Physics.Raycast(new Ray(data.ActorTransform.position, data.CurrentDestination - data.ActorTransform.position), out hit, Vector3.Distance(data.ActorTransform.position, data.CurrentDestination));
            if (hit.transform != null)
                data.CurrentDestination = hit.transform.position - (data.Actor.MinDistToDest * data.ActorTransform.position);
        }

        //Use the rigid body component to move to the desired destination.
        if (!Player.IsGamePaused)
        {
            //Rotate
            Quaternion targetRotation = Quaternion.LookRotation(data.CurrentDestination - data.ActorTransform.position);
            data.ActorTransform.rotation = Quaternion.Slerp(data.ActorTransform.rotation, targetRotation, data.Actor.RotationSpeed * Time.deltaTime);

            //Capture the rigidbody
            Rigidbody rb = data.ActorTransform.GetComponent<Rigidbody>();

            //Move Towards
            rb.velocity =
                (data.CurrentDestination - data.ActorTransform.position + data.ActorTransform.forward * Time.fixedDeltaTime).normalized * data.Actor.MovementForceMultiplier;
                //.AddForce(data.CurrentDestination - data.ActorTransform.position * data.MovementSpeed * Time.fixedDeltaTime);
        }

        //Convert the data back so that we may pass it to the returned state.
        passedData = data;

        //Get the next state and pass the data along.
        return OnLateUpdateState(ref passedData);
    }
    
    //Needs to know how far they can wander.
    //Needs to have a distance it can be from the destination.
    //Chooses a random destination from within a range of itself or the spawning position.
    //Boolean to determine if they choose a position from their spawn or current position.
    //Needs to know the movement speed that it can move at. (Reference to the defaults?)
}
