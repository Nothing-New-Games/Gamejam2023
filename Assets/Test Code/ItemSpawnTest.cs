using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnTest : MonoBehaviour
{
    [Button, HideInEditorMode]
    public void SpawnItem()
    {
        if (DesiredSpawnPos == new Vector3())
            WorldItem.SpawnItemEvent.Invoke(new(gameObject, ItemData, Quantity));
        else
            WorldItem.SpawnItemEvent.Invoke(new(ItemData, Quantity, DesiredSpawnPos, DesiredSpawnRotation));
    }

    public PickupItem ItemData;
    [MinValue(1)]
    public int Quantity;
    public Vector3 DesiredSpawnPos;
    public Quaternion DesiredSpawnRotation;

    public bool ShowSpawnLocation = false;
    [ShowIf("@ShowSpawnLocation")]
    public float SpawnMarkerRadius = 5f;

    private void OnDrawGizmos()
    {
        if (ShowSpawnLocation)
            Gizmos.DrawWireSphere(transform.position + transform.TransformDirection(Vector3.forward) * 3, SpawnMarkerRadius);
    }
}
