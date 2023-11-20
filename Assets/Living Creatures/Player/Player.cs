using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using WatchDog;

public class Player : Creature, IAttackable
{
    private static Player PlayerInstance;
    private static PlayerCamera _Camera;
    public static Player? GetPlayerInstance
    {
        get
        {
            if (PlayerInstance == null)
            {
                Watchdog.CriticalErrorCallback.Invoke(new(new EventMessage("No player found! Please create a player instance and restart!")));
                return null;
            }

            return PlayerInstance;
        }
    }

    public static bool IsGamePaused { get; internal set; }

    private bool HoldItemInfront = false;
    public static WatchdogEvent HoldItemButtonClicked = new();
    public static WatchdogEvent HoldItemButtonReleased = new();


    [ShowInInspector, TabGroup("Main/Customization/SubTabs", "Movement"), MinValue(0)]
    [Tooltip("The max velocity the player can move at when on the ground.")]
    public float MaxGroundVelocity = 5f;
    [ShowInInspector, TabGroup("Main/Customization/SubTabs", "Movement"), MinValue(0)]
    [Tooltip("The max velocity the player can move at when in the air.")]
    public float MaxAirbornVelocity = 0.7f;
    [ShowInInspector, TabGroup("Main/Customization/SubTabs", "Movement"), MinValue(0)]
    [Tooltip("The force multiplier used to apply jumping to the player.")]
    public float JumpForceMultiplier = 25;

    [ShowInInspector, TabGroup("Main/Customization/SubTabs", "Pickup"), MinValue(0)]
    [Tooltip("The amount of force that will be applied to the player when they jump.")]
    public float PickupOffset = 0f;

    [TabGroup("Main", "Debug"), ReadOnly, DisplayAsString]
    public bool IsGrounded { get; private set; } = false;

    #region Idk if I need these right now
    public void Damage(int damageTaken, DamageTypes[] damageTypes = null)
    {
        //Compare damage type to the resistences and weaknesses
        throw new System.NotImplementedException();
    }

    public void Heal(int healthReplenished, HealingTypes healingType)
    {
        throw new System.NotImplementedException();
    }

    public void Killed()
    {
        throw new System.NotImplementedException();
    }
    #endregion

    public override async Task OnAwake()
    {
        if (PlayerInstance == null)
        {
            IsPlayer = true;
            PlayerInstance = this;
        }
        else
        {
            Watchdog.CriticalErrorCallback.Invoke(new(new EventMessage("Multiple instances of the player detected!")));
            Destroy(this);
            return;
        }

        ActorTransform = transform;
        _Camera = GetComponentInChildren<PlayerCamera>();
        
        _StateManager = new("Idle", new PlayerStateData(transform)
        {
            SetTransitions = new()
            {
                { "Idle", new PlayerStoppedMoving(new PlayerIdle()) },
                { "Moving", new PlayerIsMoving(new PlayerMoving()) },
                { "Airborn", new PlayerIsFalling(new PlayerAirborn()) }
            }
        });

        HoldItemButtonClicked += OnPickupButtonClicked;
        HoldItemButtonReleased += OnPickupButtonReleased;

        HoldItemButtonClicked += IPickupAble.OnInteractionChange;
        HoldItemButtonReleased += IPickupAble.OnInteractionChange;

        await Task.Yield();
    }

    public override async Task OnUpdate()
    {
        float playerHeight = GetComponent<Collider>().bounds.size.y;
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.1f);

        RaycastHit hit;

        _currentTarget = ClosestAndInFrontTargetable();

        try
        {
            if (_currentTarget != null)
            {
                Physics.Raycast(transform.position, _currentTarget.ActorTransform.position - transform.position, out hit);

                if (hit.transform.gameObject != _currentTarget.ActorTransform.gameObject)
                {
                    Debug.Log($"{hit.transform.name} is in the way of {_currentTarget.ActorTransform.name}!");
                    _currentTarget = null;
                }
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning($"Unknown exception when handling pickup movement!\n{ex.Message}");
        }

        //Debug.Log(_currentTarget);

        if (_currentTarget != null )
        {
            if (Input.GetKey(KeyCode.Mouse0) && _currentTarget as IPickupAble != null)
                HoldItemButtonClicked.Invoke(new(_currentTarget as IPickupAble, _Camera.transform.forward * PickupOffset + transform.position));
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
            HoldItemButtonReleased.Invoke(new(_currentTarget as IPickupAble, new Vector3()));

        await Task.Yield();
    }

    private void OnPickupButtonClicked(object caller, EventArgObjects args) => HoldItemInfront = true;
    private void OnPickupButtonReleased(object caller, EventArgObjects args) => HoldItemInfront = false;
}