using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Creature : MonoBehaviour
{
    #region Required
    [TabGroup("Required"), Required]
    [Tooltip("This determines all the creature stats and loadout.")]
    public Living_Creature _defaults;
    #endregion

    #region Public Variables
    #endregion

    #region Protected Variables
    [TabGroup("Protected"), ShowInInspector]
    [Tooltip("Enum representing the living state of the creature.")]
    protected LivingState _livingState { get; private set; }
    [TabGroup("Protected"), ShowInInspector]
    [Tooltip("List of all status effects the creature is currently under.")]
    protected List<StatusEffects> _currentEffects { get; private set; } = new List<StatusEffects>();
    protected int _level { get { return _defaults.Level; } }
    [ShowInInspector, TabGroup("Protected")]
    [Tooltip("The current health the creature has, can be changed during playtime in the editor. Varible is protected, so changes will not stick.")]
    protected int _currentHealth {  get; private set; }
    /// <summary>
    /// Max health for the creature. Is set by defaults.
    /// </summary>
    protected int _maxHealth { get { return _defaults.MaxHealth; } }
    [ShowInInspector, TabGroup("Protected")]
    [Tooltip("The speed at which the creature can move through the world.")]
    protected float _maxMovementSpeed { get; private set; }
    [ShowInInspector, TabGroup("Protected")]
    [Tooltip("The speed at which the creature can turn its body.")]
    protected float _maxRotationSpeed { get; private set; }
    [ShowInInspector, TabGroup("Protected")]
    [Tooltip("The target the creature currently has. Can be changed during playtime in the editor. Variable is protected, so changes will not stick.")]
    protected Creature _currentTarget { get; private set; }
    [ShowInInspector, TabGroup("Protected"), Sirenix.OdinInspector.ReadOnly]
    [Tooltip("A list of all living creatures to this one. Auto populates and is determined by MaxTargetingDistance.")]
    protected List<Creature> NearbyLiving { get; private set; } = new();
    #endregion

    #region Private Variables
    private static List<Creature> AllLiving = new();
    private StateManager StateManager;
    #endregion

    #region Customization
    [TabGroup("Customization"), Range(1, 1000), MinValue(1)]
    [Tooltip("The max distance a creature has to be in order to detect something.")]
    public float MaxTargetingDistance = 10f;
    [TabGroup("Customization"), Range(0, 180), MinValue(0), MaxValue(180)]
    [Tooltip("The Line of Sight angle representing how wide the living creature can see infront of them.")]
    public int LOSDegree = 40;
    [TabGroup("Customization"), Range(1, 99), MinValue(1), MaxValue(99), OnValueChanged("OnDetectionWeightChange")]
    [Tooltip("Percentage for how likely a living creature can detect things when looking towards them.\nThis + Distance will always = 100")]
    public int AngleDetectionWeight = 75;
    [TabGroup("Customization"), Range(1, 99), MinValue(1), MaxValue(99), OnValueChanged("OnDetectionWeightChange")]
    [Tooltip("Percentage for how likely a living creature can detect things when closer to them.\nThis + Angle will always = 100")]
    public int DistanceDetectionWeight = 25;
    [TabGroup("Customization"), Range(0, 100), MinValue(0), MaxValue(100)]
    [Tooltip("This gives makes it harder for the creature to find a target, when normally they would have succeeded.")]
    public int DetectionFailureChance = 0;
    #endregion

    #region Getters
    [ShowInInspector, LabelText("Level"), TabGroup("Debug")]
    [Tooltip("Shows the current level of the creature.")]
    public int GetLevel { get { return _level; } }
    [ShowInInspector, LabelText("Current Health"), TabGroup("Debug")]
    [Tooltip("Shows the current health of the creature.")]
    public int GetCurrentHealth { get { return _currentHealth; } }
    [Tooltip("Shows the max health the creature can have.")]
    public int GetMaxHealth { get { return _maxHealth; } }
    [ShowInInspector, LabelText("Current Target"), TabGroup("Debug")]
    [Tooltip("Shows the current target of this creature.")]
    public Creature GetCurrentTarget { get { return _currentTarget; } }


    public Living_Creature GetDefaults { get { return _defaults; } }
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

        StateManager.OnStartCurrentState();
    }
    private async void Update()
    {
        await OnUpdate();

        StateManager.Update();
    }
    private async void LateUpdate()
    {
        foreach (Creature target in AllLiving)
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

        StateManager.LateUpdate();
    }
    private async void Awake()
    {
        AllLiving.Add(this);

        #region Stat Assignment
        _currentHealth = _maxHealth;
        #endregion

        if (_Gradient == null && !DistanceGradient.colorKeys.Any(color => color.color == Color.white))
        {
            _Gradient = DistanceGradient;
        }
        else if (_Gradient != null)
            DistanceGradient = _Gradient;

        await OnAwake();

        StateManager.OnStateAwake();
    }
    private async void OnApplicationQuit()
    {
        NearbyLiving = null;
        AllLiving = null;

        await OnApplicationExit();
    }
    #endregion

    #region Virtual Methods
    public virtual async Task OnStart() => await Task.Yield();
    public virtual async Task OnUpdate() => await Task.Yield();
    public virtual async Task OnAwake() => await Task.Yield();
    public virtual async Task OnLateUpdate() => await Task.Yield();
    public virtual async Task OnApplicationExit() => await Task.Yield();
    protected virtual void SetInitialState(State initial) => StateManager = new(initial, gameObject);
    #endregion

    #region Gizmos
    [ShowInInspector, TabGroup("Debug"), Range(1, 20), MinValue(1)]
    [Tooltip("Determines how far a gizmo can be drawn that doesn't represent a distance already.")]
    protected float DrawDistance = 3f;

    #region Directions
    [ShowInInspector, TabGroup("Debug"), OdinSerialize, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
    [HideIf("@bools.DrawCardinals == false")]
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

    [ShowInInspector, TabGroup("Debug"), OdinSerialize]
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

    #region Booleans
    [TabGroup("Debug"), ShowInInspector, HideLabel]
    private MyBooleans bools = new MyBooleans();

    [HideReferenceObjectPicker]
    private class MyBooleans
    {
        [FoldoutGroup("Booleans"), LabelWidth(300)]
        [Tooltip("If true, draws all checked gizmo debug tools for the creature.")]
        public bool DrawGizmos = false;
        [FoldoutGroup("Booleans"), LabelWidth(300)]
        [Tooltip("Draws all cardinal directions, including up and down.")]
        public bool DrawCardinals = false;
        [FoldoutGroup("Booleans"), LabelWidth(300)]
        [Tooltip("Draws gizmos representing custom directions based on provided values.")]
        public bool DrawCustomDirections = false;
        [FoldoutGroup("Booleans"), LabelWidth(300), OnValueChanged("ToggleNearbyGizmo"), HideIf("DrawToAllTargets")]
        [Tooltip("Draws a gizmo line to all targets within the MaxTargetingDistance.")]
        public bool DrawToNearbyTargets = false;
        [FoldoutGroup("Booleans"), LabelWidth(300), OnValueChanged("ToggleAllGizmo"), HideIf("DrawToNearbyTargets")]
        [Tooltip("Draws a gizmo line to all possible targets in the scene.")]
        public bool DrawToAllTargets = false;
        [FoldoutGroup("Booleans"), LabelWidth(300)]
        [Tooltip("Draws gizmo lines representing what the creature counts as immediately infront of it, based on LOSDegree.")]
        public bool DrawLineOfSight = false;


        private void ToggleNearbyGizmo()
        {
            DrawToAllTargets = !DrawToNearbyTargets;
        }
        private void ToggleAllGizmo()
        {
            DrawToNearbyTargets = !DrawToAllTargets;
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (bools.DrawGizmos)
        {
            #region Directions
            if (bools.DrawCardinals)
                foreach (var direction in CardinalDirections)
                {
                    Gizmos.color = direction.ColorIdentifier;
                    Gizmos.DrawLine(transform.position, direction.DirectionAsVector * DrawDistance + transform.position);
                }

            if (bools.DrawCustomDirections)
                foreach (var direction in CustomDirections)
                {
                    Gizmos.color = direction.ColorIdentifier;
                    Gizmos.DrawLine(transform.position, direction.DirectionAsVector * DrawDistance + transform.position);
                }
            #endregion

            #region Visual Representation
            var LOSRight = Quaternion.Euler(0, LOSDegree, 0) * transform.forward * (float)DrawDistance;
            var LOSLeft = Quaternion.Euler(0, -LOSDegree, 0) * transform.forward * (float)DrawDistance;
            if (bools.DrawLineOfSight)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, LOSRight + transform.position);
                Gizmos.DrawLine(transform.position, LOSLeft + transform.position);
            }
            #endregion

            #region Targets
            if (bools.DrawToAllTargets && AllLiving != null && AllLiving.Count > 0)
                foreach (var living in AllLiving)
                {
                    float drawDist = Mathf.Clamp(Vector3.Distance(transform.position, living.transform.position), 0, MaxTargetingDistance);
                    Vector3 direction = (living.transform.position - transform.position).normalized;
                    float angleToTarget = Vector3.Angle(transform.forward, direction);

                    float distToTarget = Vector3.Distance(transform.position, living.transform.position);

                    Gizmos.color = ColorByDistance(CalculateDetectionProbability(living, true));
                    Gizmos.DrawLine(transform.position + transform.forward, direction * drawDist + transform.position);
                }
            if (bools.DrawToNearbyTargets && NearbyLiving != null && NearbyLiving.Count > 0)
                foreach (var living in NearbyLiving)
                {
                    float drawDist = Mathf.Clamp(Vector3.Distance(transform.position, living.transform.position), 0, MaxTargetingDistance);
                    Vector3 direction = (living.transform.position - transform.position).normalized;
                    float angleToTarget = Vector3.Angle(transform.forward, direction);

                    float distToTarget = Vector3.Distance(transform.position, living.transform.position);

                    Gizmos.color = ColorByDistance(CalculateDetectionProbability(living, true));
                    Gizmos.DrawLine(transform.position + transform.forward, direction * drawDist + transform.position);
                }
            #endregion
        }
    }

    
    public static Gradient _Gradient;
    [OnStateUpdate("GradientChanged"), TabGroup("Global")]
    [Tooltip("Only one is needed to set the gradient. Player has it currently")]
    public Gradient DistanceGradient = new();
    private void GradientChanged() => _Gradient = DistanceGradient;


    private Color ColorByDistance(float probability)
    {
        return _Gradient.Evaluate(probability);
    }
    #endregion

    #region Methods
    public float CalculateDetectionProbability(Creature living, bool gizmoDraw = false)
    {
        // Calculate distance between observer and target
        float distance = Vector3.Distance(living.transform.position, transform.position);

        // If the target is beyond the maximum detection distance, return 0% chance
        if (distance > MaxTargetingDistance)
        {
            return 0;
        }

        // Calculate angle between forward direction of observer and target
        float angle = Vector3.Angle(transform.forward, living.transform.position - transform.position);

        // Normalize the distance and angle to be within the range [0, 1]
        float normalizedDistance = Mathf.Clamp01(distance / MaxTargetingDistance);
        float normalizedAngle = Mathf.Clamp01(angle / 180f); // Assuming 180 degrees as maximum angle

        // Calculate the base detection probability based on distance alone
        float baseDetectionProbability = Mathf.Clamp01(1 - normalizedDistance);

        // Convert DetectionFailureChance from percentage to a value between 0 and 1
        float failureChance = DetectionFailureChance / 100;

        if (!gizmoDraw)
        {
            // Generate a random value between 0 and 1
            float randomValue = UnityEngine.Random.Range(0.0f, 1.0f);

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
    #endregion
}