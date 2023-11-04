using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using WatchDog;

public class SizeableContainer : Container
{
    #region Container Size
    [SerializeField, MinValue(1), Range(1, 24), TabGroup("Customization")]
    protected int _ContainerSize = 1;
    public int GetContainerSize { get { return _ContainerSize; } }
    #endregion


    public void IncreaseSize(int additionalSlots)
    {
        if (_Slots.Count + additionalSlots < MaxSlots)
            _ContainerSize += additionalSlots;

        else 
            Watchdog.CriticalErrorCallback.Invoke(new(new EventMessage($"Unable to add {additionalSlots} slots to {gameObject.name}!")));
    }
    public void DecreaseSize(int slotsToRemove)
    {
        if (0 < _Slots.Count - slotsToRemove)
            _ContainerSize -= slotsToRemove;
        else 
            Watchdog.CriticalErrorCallback.Invoke(new(new EventMessage($"Unable to remove {slotsToRemove} slots from {gameObject.name}!")));
    }


    private void Update()
    {
        if (_ContainerSize > _Slots.Count)
        {
            while (_Slots.Count != GetContainerSize)
            {
                _Slots.Add(GameObject.Instantiate(SlotPrefab, this.transform));
            }
        }
        else if (_ContainerSize < _Slots.Count)
        {
            SlotHandler currentSlot;

            while (_Slots.Count != GetContainerSize)
            {
                currentSlot = _Slots.Last();
                if (currentSlot.GetLoot != null)
                    WorldItem
                        .SpawnItemEvent
                            .Invoke(new(ContainerOwner, currentSlot.GetLoot, currentSlot.GetQuantity));
                Destroy(currentSlot.gameObject);
                _Slots.Remove(currentSlot);
            }
        }
    }
}
