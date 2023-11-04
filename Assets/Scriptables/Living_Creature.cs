using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WatchDog;

[CreateAssetMenu(menuName = "SO's/New Living Creature", fileName = "Living Creature")]
public class Living_Creature : ScriptableObject
{
    #region Health Tab
    [TabGroup("Health")]
    public int MaxHealth = 10;
    [Tooltip("Value is in percentage"), OnStateUpdate("CalculateNearDeathValue"), Range(0, 99), TabGroup("Health")]
    public int NearDeathPercentage = 10;
    [HideIf("@NearDeathPercentage == 0"), TabGroup("Health")]
    public bool FleesOnNearDeath = false;
    [HideIf("@NearDeathPercentage == 0"), TabGroup("Health")]
    public bool KillableWhenNearDeath = true;
    #endregion

    #region Target
    [ShowInInspector, TabGroup("Targeting")]
    public List<Living_Creature> TargetableCreatures = new();
    #endregion

    #region Stats
    [MinValue(1), TabGroup("Stats")]
    public int Level = 1;
    [MinValue(0), TabGroup("Stats")]
    public int ExperienceReward = 50;
    [TabGroup("Stats")]
    public float MaxMovementSpeed = 10;
    [TabGroup("Stats")]
    public float MaxRotationSpeed = 10;
    [OnStateUpdate("CalculateAlignment"), Range(-100, 100), TabGroup("Stats")]
    public int AlignmentLevel = 0;


    [ShowInInspector, TabGroup("Stats")]
    [ListDrawerSettings(AddCopiesLastElement = false, AlwaysAddDefaultValue = false, CustomAddFunction = "AddNewElementToResistences")]
    public List<DamageTypes> Resistences = new();
    [ShowInInspector, TabGroup("Stats")]
    [ListDrawerSettings(AddCopiesLastElement = false, AlwaysAddDefaultValue = false, CustomAddFunction = "AddNewElementToWeaknesses")]
    public List<DamageTypes> Weaknesses = new();
    [ShowInInspector, TabGroup("Stats")]
    [ListDrawerSettings(AddCopiesLastElement = false, AlwaysAddDefaultValue = false, CustomAddFunction = "AddNewElementToImmunities")]
    public List<DamageTypes> Immunities = new();
    #endregion

    #region Events
    [ShowInInspector, TabGroup("Events"), DictionaryDrawerSettings(IsReadOnly = true)]
    public Dictionary<string, Watchdog> Events = new()
    {
        { DefaultEvents.Damaged.ToString(), new() },
        { DefaultEvents.Healed.ToString(), new() },
        { DefaultEvents.BehaviorStateChange.ToString(), new() },
        { DefaultEvents.Killed.ToString(), new() },
    };
    
    [TabGroup("Events")]
    public string NewEventCallbackName;
    [TabGroup("Events"), Button("AddNewCallback", ButtonSizes.Medium)]
    private void AddNewCallback()
    {
        if (Events.ContainsKey(NewEventCallbackName))
        {
            Debug.LogWarning("Name already exists for callback " +  NewEventCallbackName);
            return;
        }

        Events.Add(NewEventCallbackName, new());
        NewEventCallbackName = string.Empty;
    }

    public Watchdog GetEvent(DefaultEvents eventName) =>
        Events[eventName.ToString()];
    public Watchdog GetEvent(string eventName)
    {
        if (Events.ContainsKey(eventName))
            return Events[eventName];

        else
        {
            Debug.LogWarning($"Event {eventName} not found on {name}!");
            return null;
        }
    }
    #endregion

    #region Debug
    [ReadOnly, TabGroup("Debug")]
    public string NearDeathValue;
    [ReadOnly, GUIColor("GetAlignmentColor"), TabGroup("Debug")]
    public Alignment AssignedAlignment;
    #endregion

    #region Inspector Methods
    private void CalculateAlignment()
    {
        if (AlignmentLevel <= -50)
            AssignedAlignment = Alignment.Evil;
        else if (AlignmentLevel <= 0)
            AssignedAlignment = Alignment.Neutral;
        else if (AlignmentLevel <= 50)
            AssignedAlignment = Alignment.Good;
    }
    private Color GetAlignmentColor()
    {
        return AssignedAlignment switch
        {
            Alignment.Evil => Color.red,
            Alignment.Neutral => Color.yellow,
            Alignment.Good => Color.green,
            _ => Color.white,
        };
    }
    private void CalculateNearDeathValue()
    {
        if (NearDeathPercentage == 0)
        {
            NearDeathValue = "I'll sleep when I'm dead!";
            return;
        }

        NearDeathValue = ((float) MaxHealth * ((float) NearDeathPercentage / 100)).ToString();
    }
    private void AddNewElementToResistences()
    {
        if (Resistences.Count == DamageTypeUtility.TotalDamageTypes)
        {
            Debug.LogWarning("Unable to add more Resistences!");
            return;
        }


        if (Resistences.Count > 0)
            Resistences.Add(DamageTypeUtility.GetNextDamageType(Resistences.Last()));
        else Resistences.Add(DamageTypeUtility.GetFirst);
    }
    private void AddNewElementToWeaknesses()
    {
        if (Weaknesses.Count == DamageTypeUtility.TotalDamageTypes)
        {
            Debug.LogWarning("Unable to add more Weaknesses!");
            return;
        }


        if (Weaknesses.Count > 0)
            Weaknesses.Add(DamageTypeUtility.GetNextDamageType(Weaknesses.Last()));
        else Weaknesses.Add(DamageTypeUtility.GetFirst);
    }
    private void AddNewElementToImmunities()
    {
        if (Immunities.Count == DamageTypeUtility.TotalDamageTypes)
        {
            Debug.LogWarning("Unable to add more Immunities!");
            return;
        }


        if (Immunities.Count > 0)
            Immunities.Add(DamageTypeUtility.GetNextDamageType(Immunities.Last()));
        else Immunities.Add(DamageTypeUtility.GetFirst);
    }
    #endregion
}


public enum Alignment
{
    Good, Evil, Neutral
}

public enum DefaultEvents
{
    Damaged,
    Healed,
    BehaviorStateChange,
    Killed,
}