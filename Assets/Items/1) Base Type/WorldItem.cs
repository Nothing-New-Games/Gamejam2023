using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using WatchDog;

public class WorldItem : SerializedMonoBehaviour
{
    [OdinSerialize]
    public PickupItem ItemData { get; private set; }
    public int Quantity { get; private set; }

    /// <summary>
    /// Spawns an item at the specified location.
    /// <para>ItemType, int Quantity (Optional), Quaternion (Optional). Takes either GameObject for spawner OR vector3</para>
    /// </summary>
    /// 
    public static WatchdogEvent SpawnItemEvent { get; set; } = new();
    
    /// <summary>
    /// Called when the <see cref="SpawnItemEvent"/> is called. Do not call this!
    /// </summary>
    public static void SpawnWorldItem(object sender, EventArgObjects args)
    {
        GameObject spawner = null;
        
        try
        {
            var allArgs = args.GetAllArgs();
            int quantity = 1;
            Vector3 SpawnPosition = new();
            Quaternion Rotation = new();
            PickupItem itemData = null;

            foreach ( var arg in allArgs )
            {
                if (arg.GetType() == typeof(int))
                        quantity = (int) arg;
                if (arg.GetType() == typeof(Vector3))
                    SpawnPosition = (Vector3) arg;
                if (arg.GetType() == typeof(GameObject))
                    spawner = (GameObject) arg;
                if (arg.GetType() == typeof(Quaternion))
                    Rotation = (Quaternion) arg;
                if (arg.GetType() == typeof(PickupItem))
                    itemData = (PickupItem) arg;
            }

            if (SpawnPosition == new Vector3() && spawner == null)
            {
                if (sender.GetType() == typeof(GameObject))
                    spawner = sender as GameObject;
                else
                {
                    Watchdog.LogWarningCallback.Invoke(new(new EventMessage("Unable to find a spawn position or spawner! Rejecting item spawn!")));
                    return;
                }
            }
            if (itemData == null)
            {
                Watchdog.LogWarningCallback.Invoke(new(new EventMessage("Unable to find the item data! Rejecting item spawn!")));
                return;
            }


            WorldItem newWorldItem = itemData.Prefab.GetComponent<WorldItem>().SetQuantity(quantity);
            WorldItem spawned;

            //Can add some randomization to this, so loot is spread out in a cone infront of the player.
            //Maybe add a check to see if the item is within x distance from the spawner on hit, we rotate by x degrees?
            //On full rotation, we.. I'm not sure lmao

            if (spawner != null)
            {
                RaycastHit hit;
                Physics.Raycast(spawner.transform.position, spawner.transform.TransformDirection(Vector3.forward) * 3, out hit);

                if (hit.point != new Vector3())
                    spawned = GameObject.Instantiate(newWorldItem, hit.point, Rotation);
                else spawned = GameObject.Instantiate(newWorldItem, spawner.transform.position + spawner.transform.TransformDirection(Vector3.forward) * 3, Rotation);
            }
            else //This means the item may be spawned at position (0,0,0) with a rotation of (0,0,0,0) if no quaternion or rotation are found.
                spawned = GameObject.Instantiate(newWorldItem, SpawnPosition, Rotation);


            Watchdog.LogMessageCallback.Invoke(new(new EventMessage($"Spawned new {spawned.name} at {spawned.transform.position} with a rotation of {spawned.transform.rotation}", PigeonCarrier.LogTypes.Info)));
        }
        catch (Exception ex)
        {
            Watchdog.LogWarningCallback.Invoke(new(new EventMessage($"Error when spawning item!\n{ex.Message}")));
        }
    }

    public WorldItem SetQuantity(int quantity)
    {
        Quantity = quantity;
        return this;
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        name = ItemData.name;
    }
}
