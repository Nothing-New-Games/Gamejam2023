using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "SO's/New Pickup Item", fileName = "Item")]
public class PickupItem : ScriptableObject
{
    #region Main
    [PreviewField(300), HorizontalGroup("Center", MarginRight = 30), /*HideLabel*/]
    [Required, PropertyTooltip("Prefab for the item when it is spawned in the world.")]
    public GameObject Prefab;

    [PreviewField(50), TabGroup("Main")]
    [Required, PropertyTooltip("Used when assigning a pickup item to any container.")]
    public Sprite ContainerSprite;

    [TabGroup("Main")]
    public string Description = "Doesn't look particularly useful, but who knows?";

    [TabGroup("Main")]
    [PropertyTooltip("Integer identifier for the item.")]
    public int ItemID = -1;

    [TabGroup("Main")]
    [ValueDropdown("valueDropdownItems"), PropertyTooltip("Determines what can be done with an item when in a player inventory.")]
    public ItemType ItemType = ItemType.Misc;

    [TabGroup("Main")]
    [Range(0, 1000), PropertyTooltip("How much an item weighs when carried in an inventory.")]
    public float Weight = 0f;

    [TabGroup("Main")]
    [MinValue(0f), PropertyTooltip("This will be used when calculating the purchase value of an item.")]
    public float Desirability = 0f;

    [TabGroup("Main")]
    [ValueDropdown("valueDropdownQuality"), PropertyTooltip("Effects the potential of an item when used."), ShowIf("@IsQuestItem == false"), GUIColor("GetQualityColor")]
    public ItemQuality ItemQuality = ItemQuality.Standard;
    #endregion

    #region Restrictions
    public bool IsEquipable => AvailableRightClickOptions.Contains(ItemMenuOptions.Equip);

    [TabGroup("Main")]
    [PropertyTooltip("Prevents the player from doing an action that would remove the item from the world rendering quests incompletable.")]
    public bool IsQuestItem = false;

    [TabGroup("Unique Options"), PropertyTooltip("Determines what the right click options are for an item in the inventory.")]
    public List<ItemMenuOptions> AvailableRightClickOptions = new List<ItemMenuOptions>();

    [TabGroup("Unique Options")]
    [ShowIf("IsEquipable"), PropertyTooltip("Restricts the player from wielding an offhand when this item is equipped in the mainhand.")]
    public bool IsTwoHanded = false;

    #region Damage
    [TabGroup("Unique Options")]
    [Range(0, 9999), OnValueChanged("ValidateMinMax"), ShowIf("IsEquipable"), PropertyTooltip("The lowest number of damage the item can deal when used as a weapon.")]
    public int MinDamge = 0;
    [TabGroup("Unique Options")]
    [Range(0, 9999), OnValueChanged("ValidateMinMax"), ShowIf("IsEquipable"), PropertyTooltip("The highest number of damage the item can deal when used as a weapon.")]
    public int MaxDamge = 1;

    public int GetPreModifiedDamage()
    {
        return Random.Range(MinDamge, MaxDamge + 1);
    }
    #endregion
    #endregion



    private ValueDropdownList<ItemType> valueDropdownItems = new()
    {
        { "Misc",   ItemType.Misc },
        { "Head",	ItemType.Head },
        { "Chest",	ItemType.Chest },
        { "Glove",	ItemType.Glove },
        { "Boot",	ItemType.Boot },
        { "Neck",	ItemType.Neck },
        { "Weapon",	ItemType.Weapon },
        { "Offhand",	ItemType.Offhand },
        { "Consumable",	ItemType.Consumable },
        { "Material",	ItemType.Material },
	};
    private ValueDropdownList<ItemQuality> valueDropdownQuality = new()
    {
        { "Poor", ItemQuality.Poor },
        { "Amateur", ItemQuality.Amateur },
        { "Standard", ItemQuality.Standard },
        { "Good", ItemQuality.Good },
        { "Exceptional", ItemQuality.Exceptional },
        { "Master",	ItemQuality.Master },
	};

    private int previousMin = 0;
    private int previousMax = 1;
    private void ValidateMinMax()
    {
        if (MinDamge > MaxDamge)
        {
            if (previousMax != MaxDamge)
                MinDamge = MaxDamge -1;
            else if (previousMin != MinDamge)
                MaxDamge = MinDamge +1;
        }

        previousMin = MinDamge;
        previousMax = MaxDamge;
    }
    private Color GetQualityColor()
    {
        return ItemQuality switch
        {
            ItemQuality.Poor => Color.red,
            ItemQuality.Amateur => Color.yellow,
            ItemQuality.Standard => Color.white,
            ItemQuality.Good => Color.green,
            ItemQuality.Exceptional => Color.cyan,
            ItemQuality.Master => Color.magenta,
            _ => Color.white,
        };
    }
}



public enum ItemType
{
    Head, Chest, Glove, Boot, Container, Neck, Weapon, Offhand, Consumable, Material, Misc
}
public enum ItemQuality
{
    Poor, Amateur, Standard, Good, Exceptional, Master
}

public enum ItemMenuOptions
{
    Equip, Consume, CobineWith, Drop, Examine, Destroy
}