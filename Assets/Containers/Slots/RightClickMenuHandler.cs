using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WatchDog;

public class RightClickMenuHandler : MonoBehaviour
{
    private static RightClickMenuHandler Instance;
    public static RightClickMenuHandler GetInstance {  get { return Instance; } }

    private Loot _loot;
    private SlotHandler _slot;

    public static WatchdogEvent RightClickMenuDisappearedCallback = new();


    private bool firstRun = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != null)
            Watchdog.CriticalErrorCallback.Invoke(new(new EventMessage("Cannot create multiple instances of " + name)));
    }

    public void AssignItemToRightClick(SlotHandler slot, Loot loot)
    {
        _loot = loot;
        _slot = slot;
    }

    private bool VerifyItem()
    {
        if (_loot != null)
        {
            return true;
        }

        Watchdog.LogWarningCallback.Invoke(new(new EventMessage("Item was null when interacted with in inventory!")));
        return false;
    }
    public void MenuHasVanished(object sender, EventArgObjects args)
    {
        _loot = null;
        _slot = null;

        if (gameObject.activeInHierarchy)
            gameObject.SetActive(false);
    }


    public void EquipClicked()
    {
        if (VerifyItem())
        {
            //Equip the item to the respective slot.
        }

        RightClickMenuDisappearedCallback.Invoke(new());
    }
    public void ConsumeClicked()
    {
        if (VerifyItem())
        {
            //Consume the item and apply effect to the player
        }

        RightClickMenuDisappearedCallback.Invoke(new());
    }
    public void CombineWithClicked()
    {
        if (VerifyItem())
        {
            //Do we want to code this?
            //If the item is not highlighted, highlight it. If one is highlighted, attempt to combine them. (Essentially the use feature from osrs)
        }

        RightClickMenuDisappearedCallback.Invoke(new());
    }
    public void DropClicked()
    {
        if (VerifyItem())
        {
            WorldItem.SpawnItemEvent.Invoke(new EventArgObjects(_loot.Item, _loot.Quantity, Player.GetPlayerInstance.gameObject));
        }

        RightClickMenuDisappearedCallback.Invoke(new());
    }
    public void ExamineClicked()
    {
        if (VerifyItem())
        {
            //Write the description to the console (If there is one)
        }

        RightClickMenuDisappearedCallback.Invoke(new());
    }
    public void DestroyClicked()
    {
        if (VerifyItem())
        {
            //Remove the item from the inventory
        }

        RightClickMenuDisappearedCallback.Invoke(new());
    }
    public void UnknownButtonClicked()
    {
        if (VerifyItem())
        {
            //Extra button, no idea what to do with it yet :D
        }

        RightClickMenuDisappearedCallback.Invoke(new());
    }
}
