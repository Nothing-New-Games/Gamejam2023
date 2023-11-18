using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using WatchDog;
using Random = UnityEngine.Random;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;

[RequireComponent(typeof(Rigidbody))]
public class Creature : SerializedMonoBehaviour, IAttackable
{
    public Transform ActorTransform { get; set; }

    #region Protected Variables
    [TabGroup("Main",  "Protected"), ShowInInspector]
    [Tooltip("Enum representing the living state of the creature.")]
    protected LivingState _livingState { get; private set; }
    [TabGroup("Main",  "Protected"), ShowInInspector]
    [Tooltip("List of all status effects the creature is currently under.")]
    protected List<StatusEffects> _currentEffects { get; private set; } = new List<StatusEffects>();
    [ShowInInspector, TabGroup("Main",  "Protected")]
    [Tooltip("The current health the creature has, can be changed during playtime in the editor. Varible is protected, so changes will not stick.")]
    public int CurrentHealth {  get; protected set; }
    /// <summary>
    /// Max health for the creature. Is set by defaults.
    /// </summary>
    protected int _maxHealth;

    [ShowInInspector, TabGroup("Main",  "Debug"), DisplayAsString]
    [Tooltip("The target the creature currently has. Can be changed during playtime in the editor. Variable is protected, so changes will not stick.")]
    protected ITargetable _currentTarget { get; set; }
    [ShowInInspector, TabGroup("Main",  "Protected"), Sirenix.OdinInspector.ReadOnly]
    [Tooltip("A list of all living creatures to this one. Auto populates and is determined by MaxTargetingDistance.")]
    protected List<Creature> NearbyLiving { get; private set; } = new();


    protected StateManager _StateManager;
    public StateManager GetStateManager => _StateManager;
    #endregion

    #region Private Variables
    private static List<Creature> AllCreatures = new();
    #endregion

    #region Debug
    [Title("Read Only")]
    [DisplayAsString, ReadOnly, ShowInInspector]
    [TabGroup("Main", "Debug"), LabelText("Current State: ")]
    private string CurrentStateAsString = "No state manager set.";
    #endregion

    #region Customization

    #region Targetting
    [ShowInInspector, TabGroup("Main", "Customization"), ListDrawerSettings(AddCopiesLastElement = true)]
    public List<ITargetable> TargetableCreatures = new();
    [TabGroup("Main", "Customization")]
    public bool IsPlayer = false;
    #endregion
    #region Sight
    [TabGroup("Main/Customization/SubTabs", "Sight"), Range(1, 1000), MinValue(1)]
    [Tooltip("The max distance a creature has to be in order to detect something.")]
    public float MaxTargetingDistance = 10f;
    [TabGroup("Main/Customization/SubTabs", "Sight"), Range(0, 180), MinValue(0), MaxValue(180)]
    [Tooltip("The Line of Sight angle representing how wide the living creature can see infront of them.")]
    public int LOSDegree = 40;
    [TabGroup("Main/Customization/SubTabs", "Sight"), Range(1, 99), MinValue(1), MaxValue(99), OnValueChanged("OnDetectionWeightChange")]
    [Tooltip("Percentage for how likely a living creature can detect things when looking towards them.\nThis + Distance will always = 100")]
    public int AngleDetectionWeight = 75;
    [TabGroup("Main/Customization/SubTabs", "Sight"), Range(1, 99), MinValue(1), MaxValue(99), OnValueChanged("OnDetectionWeightChange")]
    [Tooltip("Percentage for how likely a living creature can detect things when closer to them.\nThis + Angle will always = 100")]
    public int DistanceDetectionWeight = 25;
    [TabGroup("Main/Customization/SubTabs", "Sight"), Range(0, 100), MinValue(0), MaxValue(100)]
    [Tooltip("This gives makes it harder for the creature to find a target, when normally they would have succeeded.")]
    public int DetectionFailureChance = 0;
    #endregion
    #region Movement
    [TabGroup("Main/Customization/SubTabs", "Movement")]
    [Tooltip("The transform that will be used for movement. If left null, will use the transform for the object the script is attached to.")]
    public Transform CreatureTransform;
    [ShowInInspector, TabGroup("Main/Customization/SubTabs", "Movement"), MinValue(0)]
    [Tooltip("The speed at which the creature can move through the world.")]
    public float MovementForceMultiplier = 10f;
    [ShowInInspector, TabGroup("Main/Customization/SubTabs", "Movement"), MinValue(0)]
    [Tooltip("The speed at which the creature can turn its body.")]
    public float RotationSpeed = 10f;

    [TabGroup("Main/Customization/SubTabs", "Movement"), MinValue(0), ShowIf("@FollowsRoute == false && IsPlayer == false")]
    [Tooltip("Sets the max distance the creature can wander from its wander point. Only works in edit mode.")]
    public float MaxWanderDist = 10f;
    [TabGroup("Main/Customization/SubTabs", "Movement"), MinValue(0), ShowIf("@IsPlayer == false")]
    [Tooltip("Sets the min distance the creature will be before considerd \"at\" the destination. Only works in edit mode.")]
    public float MinDistToDest = 0.1f;
    [TabGroup("Main/Customization/SubTabs", "Movement"), MinValue(0), ShowIf("@FollowsRoute == false && IsPlayer == false")]
    [Tooltip("Sets the min duration the creature can idle for. Only works in edit mode.")]
    public float MinIdleDuration = 1f;
    [TabGroup("Main/Customization/SubTabs", "Movement"), MinValue(0), ShowIf("@FollowsRoute == false && IsPlayer == false")]
    [Tooltip("Sets the max duration the creature can idle for. Only works in edit mode.")]
    public float MaxIdleDuration = 10f;


    [TabGroup("Main/Customization/SubTabs", "Movement"), ShowIf("@FollowsRoute && IsPlayer == false"), ShowInInspector, OdinSerialize]
    [Tooltip("A list of points the creature will move to."), ListDrawerSettings(AddCopiesLastElement = true)]
    public List<CreaturePathPoint> PathPoints = new();

    [Title("Toggles")]
    [TabGroup("Main/Customization/SubTabs", "Movement"), ShowIf("@FollowsRoute == false && IsPlayer == false")]
    [Tooltip("If true, the creature will choose a destination from its current position. If false, the creature will wander from its spawn point.")]
    public bool IsFreeRoaming = false;

    [TabGroup("Main/Customization/SubTabs", "Movement"), ShowIf("@IsPlayer == false")]
    [Tooltip("If true, the creature will follow a specific route instead of random points.")]
    public bool FollowsRoute = false;
    #endregion
    #region Health
    [TabGroup("Main/Customization/SubTabs", "Health")]
    public int MaxHealth = 10;

    [TabGroup("Main/Customization/SubTabs", "Health")]
    [Tooltip("Value is in percentage"), OnStateUpdate("CalculateNearDeathValue"), Range(0, 99)]
    public int NearDeathPercentage = 10;

    [TabGroup("Main/Customization/SubTabs", "Health"), DisplayAsString]
    public string NearDeathValue;

    [Title("Toggles")]
    [TabGroup("Main/Customization/SubTabs", "Health")]
    [HideIf("@NearDeathPercentage == 0 || IsPlayer == true")]
    public bool FleesOnNearDeath = false;

    [TabGroup("Main/Customization/SubTabs", "Health")]
    [HideIf("@NearDeathPercentage == 0")]
    public bool KillableWhenNearDeath = true;
    #endregion
    #region Stats
    [MinValue(1), TabGroup("Main/Customization/SubTabs", "Stats"), OdinSerialize, ShowInInspector]
    protected int Level = 1;
    [MinValue(0), TabGroup("Main/Customization/SubTabs", "Stats"), OdinSerialize, ShowInInspector]
    protected int ExperienceReward = 50;
    [MinValue(0), TabGroup("Main/Customization/SubTabs", "Stats"), OdinSerialize, ShowInInspector]
    protected float MaxMovementSpeed = 10;
    [MinValue(0), TabGroup("Main/Customization/SubTabs", "Stats"), OdinSerialize, ShowInInspector]
    protected float MaxRotationSpeed = 10;
    [OnStateUpdate("CalculateAlignment"), Range(-100, 100), TabGroup("Main/Customization/SubTabs", "Stats"), OdinSerialize, ShowInInspector]
    protected int AlignmentLevel = 0;
    [ReadOnly, GUIColor("GetAlignmentColor"), TabGroup("Main/Customization/SubTabs", "Stats"), DisplayAsString, LabelText("Alignment: ")]
    public Alignment AssignedAlignment;

    //[Title("Damage Types")]
    [TabGroup("Main/Customization/SubTabs", "Stats"), OdinSerialize/*, ShowInInspector*/]
    [ListDrawerSettings(AddCopiesLastElement = false, AlwaysAddDefaultValue = false, CustomAddFunction = "AddNewElementToResistences")]
    protected List<DamageTypes> Resistences = new();

    [TabGroup("Main/Customization/SubTabs", "Stats"), OdinSerialize/*, ShowInInspector*/]
    [ListDrawerSettings(AddCopiesLastElement = false, AlwaysAddDefaultValue = false, CustomAddFunction = "AddNewElementToWeaknesses")]
    protected List<DamageTypes> Weaknesses = new();

    [TabGroup("Main/Customization/SubTabs", "Stats"), OdinSerialize/*, ShowInInspector*/]
    [ListDrawerSettings(AddCopiesLastElement = false, AlwaysAddDefaultValue = false, CustomAddFunction = "AddNewElementToImmunities")]
    protected List<DamageTypes> Immunities = new();
    #endregion
    #region Events
    //[ShowInInspector, TabGroup("Main/Customization/SubTabs", "Events"), DictionaryDrawerSettings(IsReadOnly = true)]
    //public Dictionary<string, WatchdogEvent> Events = new()
    //{
    //    { DefaultEvents.Damaged.ToString(), new() },
    //    { DefaultEvents.Healed.ToString(), new() },
    //    { DefaultEvents.BehaviorStateChange.ToString(), new() },
    //    { DefaultEvents.Killed.ToString(), new() },
    //};

    //[TabGroup("Main/Customization/SubTabs", "Events")]
    //public string NewEventCallbackName;
    //[TabGroup("Main/Customization/SubTabs", "Events"), Button("AddNewCallback", ButtonSizes.Medium)]
    //private void AddNewCallback()
    //{
    //    if (Events.ContainsKey(NewEventCallbackName))
    //    {
    //        Debug.LogWarning("Name already exists for callback " + NewEventCallbackName);
    //        return;
    //    }
    //    else if (NewEventCallbackName == "")
    //    {
    //        Debug.LogError("You must fill in the text field before submitting!");
    //        return;
    //    }
        
    //    Events.Add(NewEventCallbackName, new());
    //    NewEventCallbackName = string.Empty;
    //}

    //public WatchdogEvent GetEvent(DefaultEvents eventName) =>
    //    Events[eventName.ToString()];
    //public WatchdogEvent GetEvent(string eventName)
    //{
    //    if (Events.ContainsKey(eventName))
    //        return Events[eventName];

    //    else
    //    {
    //        Debug.LogWarning($"Event {eventName} not found on {name}!");
    //        return null;
    //    }
    //}
    #endregion
    #endregion

    #region Enum States
    /// <summary>
    /// Determined by the movement input recieved by the controller.
    /// </summary>
    public enum MovementState
    {
        Idle, Walking, Running,
    }
    /// <summary>
    /// The concept is we will continue to capture the movement state, but the effect of said state will be restricted by 
    /// YAxisState. <para>Default is BootsOnTheGround</para>
    /// </summary>
    public enum YAxisState
    {
        BootsOnTheGround,
        Jumping_Forward, Jumping_Backward, Jumping_Left, Jumping_Right, Jumping_Forward_Left, Jumping_Forward_Right, Jumping_Backward_Left, Jumping_Backward_Right,
        Falling_Forward, Falling_Backward, Falling_Left, Falling_Right, Falling_Forward_Left, Falling_Forward_Right, Falling_Backward_Left, Falling_Backward_Right
    }
    /// <summary>
    /// This should be considered when determining what actions are possible.
    /// <param>When the ActionState is being calculated, the current state should be taken into account.</param>
    /// </summary>
    public enum ActionState
    {
        Interacting, Talking, PerformingHeavyAttack, PerformingLightAttack, PerformingSpecialAttack, 
    }
    /// <summary>
    /// Determines what kind of living creature this is.
    /// </summary>
    public enum LivingState
    {
        Alive, Dead, Ephemeral, Undead, Contrivance, Magical,
    }
    /// <summary>
    /// All status effects that can be applied to a creature.
    /// </summary>
    public enum StatusEffects
    {
        Blinded, Charmed, Deafened, Frightened, Grappled, Incapacitated, Invisible, Paralyzed, Petrified, Poisoned, Prone,
        Restrained, Stunned, Exhaustion, Unconcious, Numb, Enraged, Dazed, Burning, Charged, Wet, Sleeping, Bleeding, Blind,
        Blight, Cursed, Confused, Frozen, Rot, Stasis, Toxic, 
    }
    #endregion

    #region Unity Methods
    private async void Start()
    {
        await OnStart();

        _StateManager.OnStartCurrentState();
    }
    private async void Update()
    {
        await OnUpdate();

        _StateManager.OnUpdate();

        CurrentStateAsString = GetCurrentStateAsString();
    }
    private async void LateUpdate()
    {
        foreach (Creature target in AllCreatures)
        {
            if (NearbyLiving.Contains(target))
            {
                if (Vector3.Distance(target.transform.position, transform.position) > MaxTargetingDistance)
                {
                    NearbyLiving.Remove(target);
                }
            }
            else if (Vector3.Distance(target.transform.position, transform.position) <= MaxTargetingDistance)
            {
                NearbyLiving.Add(target);
            }
        }

        await OnLateUpdate();

        _StateManager.OnLateUpdate();
    }
    private async void Awake()
    {
        AllCreatures.Add(this);
        ActorTransform = transform;

        #region Stat Assignment
        CurrentHealth = _maxHealth;
        #endregion

        if (_Gradient == null && !DistanceGradient.colorKeys.Any(color => color.color == Color.white))
        {
            _Gradient = DistanceGradient;
        }
        else if (_Gradient != null)
            DistanceGradient = _Gradient;

        await OnAwake();

        _StateManager.OnStateAwake();

        if (_StateManager.IsStateEmpty == true)
        {
            //Watchdog.LogWarningCallback.Invoke(new(new EventMessage($"Creature {name} is without a default state!")));
            Debug.LogWarning($"Creature {name} is without a default state!");
            gameObject.SetActive(false);
            return;
        }
    }
    private async void OnApplicationQuit()
    {
        NearbyLiving = null;
        AllCreatures = null;

        await OnApplicationExit();
    }
    #endregion

    #region Virtual Methods
    public virtual async Task OnStart() => await Task.Yield();
    public virtual async Task OnUpdate() => await Task.Yield();
    public virtual async Task OnAwake() => await Task.Yield();
    public virtual async Task OnLateUpdate() => await Task.Yield();
    public virtual async Task OnApplicationExit() => await Task.Yield();
    #endregion

    #region Gizmos
    #region Toggles
    [FoldoutGroup("Main/Debug/Toggles", expanded: true), LabelWidth(300)]
    [Tooltip("If true, draws all checked gizmo debug tools for the creature.")]
    public bool DrawGizmos = false;
    [FoldoutGroup("Main/Debug/Toggles"), LabelWidth(300)]
    [Tooltip("Draws all cardinal directions, including up and down.")]
    public bool DrawCardinals = false;
    [FoldoutGroup("Main/Debug/Toggles"), LabelWidth(300)]
    [Tooltip("Draws gizmos representing custom directions based on provided values.")]
    public bool DrawCustomDirections = false;
    [FoldoutGroup("Main/Debug/Toggles"), LabelWidth(300), OnValueChanged("ToggleNearbyGizmo"), HideIf("DrawToAllTargets")]
    [Tooltip("Draws a gizmo line to all targets within the MaxTargetingDistance.")]
    public bool DrawToNearbyTargets = false;
    [FoldoutGroup("Main/Debug/Toggles"), LabelWidth(300), OnValueChanged("ToggleAllGizmo"), HideIf("DrawToNearbyTargets")]
    [Tooltip("Draws a gizmo line to all possible targets in the scene.")]
    public bool DrawToAllTargets = false;
    [FoldoutGroup("Main/Debug/Toggles"), LabelWidth(300)]
    [Tooltip("Draws gizmo lines representing what the creature counts as immediately infront of it, based on LOSDegree.")]
    public bool DrawLineOfSight = false;
    [FoldoutGroup("Main/Debug/Toggles"), LabelWidth(300)]
    [Tooltip("Draws a circle where each point of a set path is."), ShowIf("@IsPlayer == false")]
    public bool DrawPathPoints = false;


    private void ToggleNearbyGizmo()
    {
        DrawToAllTargets = !DrawToNearbyTargets;
    }
    private void ToggleAllGizmo()
    {
        DrawToNearbyTargets = !DrawToAllTargets;
    }
    #endregion

    [ShowInInspector, FoldoutGroup("Main/Debug/Gizmo Settings"), Range(1, 20), MinValue(1)]
    [Tooltip("Determines how far a gizmo can be drawn that doesn't represent a distance already.")]
    protected float DrawDistance = 3f;

    #region Directions
    [FoldoutGroup("Main/Debug/Gizmo Settings")]
    [ShowInInspector, OdinSerialize, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
    [HideIf("@DrawCardinals == false")]
    protected Direction[] CardinalDirections =
    {
        new("Forward", Vector3.forward, Color.blue), //Forward
        new("Right", Vector3.right, Color.green), //Right
        new("Backward", -Vector3.forward, Color.red), //backward
        new("Left", -Vector3.right, Color.yellow), //Left
        new("Forward-Right", Vector3.forward + Vector3.right, Color.black), //forward-right
        new("Back-Right", -Vector3.forward + Vector3.right, Color.cyan), //back-right
        new("Back-Left", -Vector3.forward - Vector3.right, Color.white), //back-left
        new("Forward-Left", Vector3.forward - Vector3.right, Color.magenta), //forward-left
        new("Up", Vector3.up, Color.gray), //Up
        new("Down", -Vector3.up, Color.grey) //Down
    };

    [FoldoutGroup("Main/Debug/Gizmo Settings")]
    [ShowInInspector, OdinSerialize]
    protected List<CustomDirection> CustomDirections = new();


    #region Direction Classes
    [HideReferenceObjectPicker]
    protected class Direction
    {
        [HideIf("@true")]
        public string DirectionName;
        [FoldoutGroup("$DirectionName")]
        public Vector3 DirectionAsVector;
        [FoldoutGroup("$DirectionName")]
        public Color ColorIdentifier;

        public Direction(string name, Vector3 directionAsVector, Color colorIdentifier)
        {
            DirectionName = name;
            DirectionAsVector = directionAsVector;
            ColorIdentifier = colorIdentifier;
        }
    }

    [HideReferenceObjectPicker]
    protected class CustomDirection
    {
        [HideLabel, FoldoutGroup("$DirectionName")]
        public string DirectionName = "Custom Direction";
        [FoldoutGroup("$DirectionName")]
        public Vector3 DirectionAsVector = new();
        [FoldoutGroup("$DirectionName")]
        public Color ColorIdentifier = new(0, 0, 0, 1);
    }
    #endregion
    #endregion

    protected virtual void OnDrawGizmos()
    {
        if (DrawGizmos)
        {
            #region Directions
            if (DrawCardinals)
                foreach (var direction in CardinalDirections)
                {
                    Gizmos.color = direction.ColorIdentifier;
                    Gizmos.DrawLine(transform.position, direction.DirectionAsVector * DrawDistance + transform.position);
                }

            if (DrawCustomDirections)
                foreach (var direction in CustomDirections)
                {
                    Gizmos.color = direction.ColorIdentifier;
                    Gizmos.DrawLine(transform.position, direction.DirectionAsVector * DrawDistance + transform.position);
                }
            #endregion

            #region Visual Representation
            var LOSRight = Quaternion.Euler(0, LOSDegree, 0) * transform.forward * (float)MaxTargetingDistance;
            var LOSLeft = Quaternion.Euler(0, -LOSDegree, 0) * transform.forward * (float)MaxTargetingDistance;
            if (DrawLineOfSight)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, LOSRight + transform.position);
                Gizmos.DrawLine(transform.position, LOSLeft + transform.position);
            }
            #endregion

            #region Targets
            if (DrawToAllTargets && AllCreatures != null && AllCreatures.Count > 0)
                foreach (var living in AllCreatures)
                {
                    float drawDist = Mathf.Clamp(Vector3.Distance(transform.position, living.transform.position), 0, MaxTargetingDistance);
                    Vector3 direction = (living.transform.position - transform.position).normalized;
                    float angleToTarget = Vector3.Angle(transform.forward, direction);

                    float distToTarget = Vector3.Distance(transform.position, living.transform.position);

                    Gizmos.color = ColorByDistance(CalculateDetectionProbability(living as ITargetable, true));
                    Gizmos.DrawLine(transform.position + transform.forward, direction * drawDist + transform.position);
                }
            if (DrawToNearbyTargets && NearbyLiving != null && NearbyLiving.Count > 0)
                foreach (var living in NearbyLiving)
                {
                    float drawDist = Mathf.Clamp(Vector3.Distance(transform.position, living.transform.position), 0, MaxTargetingDistance);
                    Vector3 direction = (living.transform.position - transform.position).normalized;
                    float angleToTarget = Vector3.Angle(transform.forward, direction);

                    float distToTarget = Vector3.Distance(transform.position, living.transform.position);

                    Gizmos.color = ColorByDistance(CalculateDetectionProbability(living as ITargetable, true));
                    Gizmos.DrawLine(transform.position + transform.forward, direction * drawDist + transform.position);
                }
                
            #endregion

            #region Paths
            if (DrawPathPoints && PathPoints != null)
            {
                Gizmos.color = Color.magenta;
                foreach (var point in PathPoints)
                {
                    Gizmos.DrawWireSphere(point.Point, 0.5f);
                }
            }
            #endregion
        }
    }

    
    public static Gradient _Gradient;
    [OnStateUpdate("GradientChanged"), TabGroup("Main",  "Global")]
    [Tooltip("Only one is needed to set the gradient. Player has it currently")]
    public Gradient DistanceGradient = new();
    private void GradientChanged() => _Gradient = DistanceGradient;


    protected Color ColorByDistance(float probability)
    {
        return _Gradient.Evaluate(probability);
    }
    #endregion

    #region Methods
    public float CalculateDetectionProbability(ITargetable targetable, bool calculatingForGizmo = false)
    {
        if (targetable == null) return 0;

        // Calculate distance between observer and target
        float distance = Vector3.Distance(targetable.ActorTransform.position, transform.position);

        // If the target is beyond the maximum detection distance, return 0% chance
        if (distance > MaxTargetingDistance)
        {
            return 0;
        }

        // Calculate angle between forward direction of observer and target
        float angle = Vector3.Angle(transform.forward, targetable.ActorTransform.position - transform.position);

        // Normalize the distance and angle to be within the range [0, 1]
        float normalizedDistance = Mathf.Clamp01(distance / MaxTargetingDistance);
        float normalizedAngle = Mathf.Clamp01(angle / 180f); // Assuming 180 degrees as maximum angle

        // Calculate the base detection probability based on distance alone
        float baseDetectionProbability = Mathf.Clamp01(1 - normalizedDistance);

        // Convert DetectionFailureChance from percentage to a value between 0 and 1
        float failureChance = DetectionFailureChance / 100;

        if (!calculatingForGizmo)
        {
            // Generate a random value between 0 and 1
            float randomValue = Random.Range(0.0f, 1.0f);

            // Check if the random value is less than the failure chance
            if (randomValue <= failureChance)
            {
                // Failure occurred, return 0% chance of detection
                return 0;
            }
        }
        else if (failureChance == 1)
            return 0;

        // Calculate the final detection probability considering angle and random failure chance
        float detectionProbability = baseDetectionProbability + (1 - baseDetectionProbability) * (1 - normalizedAngle);

        // Ensure the detection probability is within the range [0, 1]
        detectionProbability = Mathf.Clamp01(detectionProbability);

        return detectionProbability;
    }

    protected bool IsTargetableInFront(ITargetable targetable)
    {
        if (targetable == null) return false;

        float angle = Vector3.Angle(transform.forward, targetable.ActorTransform.position - transform.position);
        return angle <= LOSDegree || angle <= -LOSDegree;
    }
    protected bool IsTargetableClose(ITargetable targetable)
    {
        if (targetable == null) return false;

        float distance = Vector3.Distance(transform.position, targetable.ActorTransform.position);
        return distance <= MaxTargetingDistance;
    }

    public ITargetable ClosestTargetable()
    {
        ITargetable closestTarget = null;
        float lastClosestDistanceMeasured = 0f;

        ITargetable nextTargetable = null;
        foreach (var targetable in NearbyLiving)
        {
            if (targetable == this) continue;

            if (targetable as ITargetable != null)
            {
                nextTargetable = targetable as ITargetable;
                if (closestTarget == null && IsTargetableInFront(nextTargetable))
                {
                    closestTarget = targetable as ITargetable;
                    lastClosestDistanceMeasured = Vector3.Distance(closestTarget.ActorTransform.position, transform.position);
                }
                else if (Vector3.Distance(targetable.transform.position, transform.position) < lastClosestDistanceMeasured)
                {
                    closestTarget = targetable as ITargetable;
                    lastClosestDistanceMeasured = Vector3.Distance(targetable.transform.position, transform.position);
                }
            }
            else continue;
        }

        return closestTarget;
    }
    
    public ITargetable ClosestAndInFrontTargetable()
    {
        var closestTarget = ClosestTargetable();

        if (IsTargetableInFront(closestTarget))
            return closestTarget;
        else return null;
    }


    public List<ITargetable> DetectMultipleTargets()
    {
        List<ITargetable> allDetected = new();

        ITargetable potentialTarget = null;
        foreach (var targetable in NearbyLiving)
        {
            if (CalculateDetectionProbability(potentialTarget) > Random.Range(0f, 1f))
            {
                potentialTarget = targetable as ITargetable;
                if (potentialTarget != null) 
                    allDetected.Add(potentialTarget);
            }
        }
        

        return allDetected;
    }

    public virtual void DealDamage(int damage, DamageTypes damageType = DamageTypes.TrueDamage)
    {
        foreach (var immunity in Immunities)
        {
            if (immunity == damageType)
            {
                damage = 0;
                break;
            }
        }
        if (damage != 0)
        {
            foreach (var resistence in Resistences)
            {
                if (resistence == damageType)
                {
                    damage = Mathf.CeilToInt(damage / 2);
                    break;
                }
            }

            CurrentHealth -= damage;
        }
    }
    #endregion

    #region OdinMethods
    private float lastAngleWeight;
    private float lastDistanceWeight;
    private void OnDetectionWeightChange()
    {
        if (DistanceDetectionWeight != 100 - AngleDetectionWeight)
        {
            if (lastAngleWeight != AngleDetectionWeight)
                DistanceDetectionWeight = 100 - AngleDetectionWeight;
            else if (lastDistanceWeight != DistanceDetectionWeight)
                AngleDetectionWeight = 100 - DistanceDetectionWeight;
        }

        lastDistanceWeight = DistanceDetectionWeight;
        lastAngleWeight = AngleDetectionWeight;
    }

    private string GetCurrentStateAsString()
    {
        if (_StateManager == null) return "No state manager set.";

        return _StateManager.CurrentStateName;
    }


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

        NearDeathValue = ((float)MaxHealth * ((float)NearDeathPercentage / 100)).ToString();
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

public interface ITargetable
{
    public Transform ActorTransform { get; set; }
}

public interface IAttackable : ITargetable
{
    public abstract void DealDamage(int damage, DamageTypes tdamageType = DamageTypes.TrueDamage);

    /// <summary>
    /// Determines what happens when something is attacked.
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="args"></param>
    public abstract void Attacked(object attacker, EventArgObjects args);
}

public interface IChatty : ITargetable
{
    public abstract void TalkToNPC(IChatty initiator);
}

public interface IPickupAble : ITargetable
{

}
public interface ILootable : IPickupAble
{
    public abstract PickupItem Take();
    public abstract PickupItem itemData { get; }
}

[HideReferenceObjectPicker]
public class CreaturePathPoint
{
    public Vector3 Point;
    [LabelText("Movement Delay"), MinValue(0)]
    public float DelayBeforeMovingToNextPoint;
}