using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using WatchDog;

public class Container : MonoBehaviour
{
    

    [ShowInInspector, TabGroup("Customization"), Required]
    public List<Loot> ContainedItems = new(24);

    [Required, TabGroup("Required")]
    public GameObject ContainerOwner;


    protected List<SlotHandler> _Slots = new List<SlotHandler>();
    [Required, TabGroup("Required")]
    public SlotHandler SlotPrefab;

    [TabGroup("Customization")]
    public bool IsDestroyable = false;


    protected readonly int MaxSlots = 24;

    private void Awake()
    {
        _Slots = GetComponentsInChildren<SlotHandler>().ToList();
        if (_Slots.Count < ContainedItems.Count)
        {
            Watchdog.LogWarningCallback.Invoke(new(new EventMessage($"Unable to add all the loot to {ContainerOwner.name}'s {name}, as it is missing the required number of slots!")));
            gameObject.SetActive(false);
            return;
        }
        else
        {
            foreach (var loot in  ContainedItems)
            {
                AddToContainer(loot.Item, loot.Quantity);
            }
        }
    }

    public bool AddToContainer(PickupItem itemData, int quantityToAdd)
    {
        //Needs to be rewritten to allow for item stacks... Why don't we remove caps?
        bool successful = false;
        foreach (SlotHandler slot in _Slots)
            successful = slot.AddToSlot(new() { Item = itemData, Quantity = quantityToAdd });

        return successful;
    }
    public bool RemoveFromContainer(PickupItem itemData, int quantityToRemove)
    {
        List<SlotHandler> containingSlots = new();
        int numberFound = 0;

        foreach (SlotHandler slot in _Slots)
        {
            if (slot.GetQuantity <= quantityToRemove && slot.GetLoot.Item == itemData)
            {
                containingSlots.Add(slot);
                numberFound += slot.GetQuantity;
            }
            if (numberFound >= quantityToRemove) break;
        }

        int remainderFromSlot;
        foreach (SlotHandler slot in containingSlots)
        {
            if (quantityToRemove > slot.GetQuantity)
            {
                quantityToRemove -= slot.GetQuantity;
                remainderFromSlot = slot.RemoveFromSlot(slot.GetQuantity);
            }
            else remainderFromSlot = slot.RemoveFromSlot(quantityToRemove);

            if (remainderFromSlot == 0)
                return true;
        }

        return false;
    }

    [Button, TabGroup("Methods"), ShowIf("@IsDestroyable == true"), HideInEditorMode]
    public void DestroyContainer()
    {
        if (ContainerOwner != Player.GetPlayerInstance)
        {
            Watchdog.LogWarningCallback.Invoke(new(new EventMessage("Unable to destroy player!")));
            return;
        }



        foreach (SlotHandler slot in _Slots)
        {
            if (slot.GetLoot != null)
                WorldItem.SpawnItemEvent.Invoke(new(ContainerOwner, slot.GetLoot, slot.GetQuantity));
        }

        Destroy(ContainerOwner);
    }
}
