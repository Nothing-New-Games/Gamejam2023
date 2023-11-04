using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootContainer : SizeableContainer
{
    [ShowInInspector]
    public Dictionary<string, Loot> LootTable = new();
    [OnStateUpdate("ValidateMinMax"), TabGroup("Customization"), MinValue(0)]
    public int MinItemsSpawned = 0;
    [OnStateUpdate("ValidateMinMax"), TabGroup("Customization"), MinValue(0)]
    public int MaxItemsSpawned = 0;

    private int previousMin = 0;
    private int previousMax = 1;

    //Scripts can inherit from this to create say... Crate, Goblin, ect to have unique loot tables
    //Things can then inherit from that to make superior varients and such. It'll be awesome!


    private void ValidateMinMax()
    {
        if (MinItemsSpawned > MaxItemsSpawned)
        {
            if (previousMax != MaxItemsSpawned)
                MinItemsSpawned = MaxItemsSpawned - 1;
            else if (previousMin != MinItemsSpawned)
                MaxItemsSpawned = MinItemsSpawned + 1;
        }

        previousMin = MinItemsSpawned;
        previousMax = MaxItemsSpawned;
    }

    [Button, TabGroup("Methods")]
    public void GenerateLoot()
    {

    }
}