using UnityEngine;
using Sirenix.OdinInspector;
using static SpeedBehaviour;

public class BToTarget<T, P> : MonoBehaviour where P : ITransformationSequence, new()
{
    [SerializeField]
    [Tooltip("Wether this code should apply transformations to the 'origin' or it should just send the events elsewhere")]
    protected bool applyInTransform = false;

    [Header("Target")]
    #region Target
    [SerializeField]
    [HideLabel]
    [InlineProperty]
    protected OriginTarget originTarget = new OriginTarget("Player");
    public Transform target { get { return originTarget.target; } set { originTarget.SetTarget(value); } }
    public Transform origin { get { return originTarget.origin; } set { originTarget.SetOrigin(value); } }
    [Space]
    [SerializeField]
    [Tooltip("Wether or not the resulting action should be projected onto a 2D plane")]
    protected bool projectOnPlane = false;
    [SerializeField]
    [ShowIf("projectOnPlane")]
    [Tooltip("Wether or not the projection plane should be calculated in origin's local space")]
    protected bool projectionLocal = false;
    [SerializeField]
    [ShowIf("projectOnPlane")]
    [Tooltip("Projection plane normal")]
    protected Vector3 planeNormal = Vector3.back;
    #endregion

    [Header("Speed behaviour")]
    [SerializeField]
    [InlineProperty]
    [HideLabel]
    protected SpeedBehaviour speedBehaviour = new SpeedBehaviour(SpeedMode.Linear);
    public bool doAccelerate
    {
        get { return speedBehaviour.doAccelerate; }
        set { speedBehaviour.doAccelerate = value; }
    }

    [SerializeField]
    protected bool local = false;
    [SerializeField]
    protected bool sendWhenZeroToo = false;
    [SerializeField]
    protected TargetMode targetMode = TargetMode.ToExactPoint;
    [SerializeField]
    [ShowIf("@((int)targetMode) == (int)TargetMode.StopAtMargin")]
    [Tooltip("Minimum distance to the target before the result becomes zero.")]
    [Min(0.0000001f)]
    protected float margin = 0.01f;
    [SerializeField]
    [Tooltip("When is this code executed")]
    protected TimeModeOrOnEnable timeMode = TimeModeOrOnEnable.Update;
    [SerializeField]
    protected bool sendFrameMovement = false;

    public enum TargetMode { ToExactPoint, NeverStop, StopAtMargin }

    DynamicInfo dynamicInfo;

    void Reset()
    {
        originTarget.SetOrigin(transform);
    }

    void Update()
    {
        if (timeMode.IsSmooth()) OnUpdate();
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) OnUpdate();
    }

    /// <summary>
    /// Calculates values and checks if the events should be sent.
    /// </summary>
    /// <param name="deltaTime">Last frame deltaTime</param>
    public void CheckEvents(float deltaTime)
    {
        if (originTarget.IsNotNull() && (deltaTime > 0f))
            Execute(deltaTime);
    }

    public void SetMaxSpeed(float speed)
    {
        speedBehaviour.SetMaxSpeed(speed);
    }

    void Execute(float deltaTime)
    {
        float inverseDeltaTime = deltaTime.Reciprocal();

        P path = GetPath();
        //path.Draw();

        T spd = default;
        if (ShouldIExecute(path))
            switch (speedBehaviour.speedMode)
            {
                case SpeedMode.Accelerated:
                    spd = Accelerated(path, ref dynamicInfo, deltaTime);
                    break;
                case SpeedMode.SmoothDamp:
                    spd = SmoothDamp(path, ref dynamicInfo, deltaTime);
                    break;
                case SpeedMode.LerpSmooth:
                    spd = LerpSmooth(path, deltaTime);
                    break;
                case SpeedMode.Teleport:
                    spd = Teleport(path, deltaTime);
                    break;
                default: //Linear
                    spd = Linear(path, deltaTime);
                    break;
            }

        Execute(spd, deltaTime);
    }

    void UpdatePrev(ref T prev)
    {
        prev = Current();
    }

    protected void ResetSpeed()
    {
        dynamicInfo.speed = default;
    }

    protected virtual void Awake()
    {
        UpdatePrev(ref dynamicInfo.prev);
    }

    protected virtual void OnEnable()
    {
        ResetSpeed();
        if (timeMode.OnEnable()) CheckEvents(timeMode.DeltaTime());
    }

    /// <summary>
    /// Calculates vectors and checks if the events should be sent.
    /// Then, if "accountForCurrentSpeed" is true,
    /// it calculates current speed based on previous transform position to account for external forces.
    /// </summary>
    protected virtual void OnUpdate()
    {
        float deltaTime = timeMode.DeltaTime();

        CheckEvents(deltaTime);

        if (speedBehaviour.AffectedByCurrentSpeed() && (deltaTime != 0f))
        {
            UpdateSpeed(ref dynamicInfo.speed, dynamicInfo.prev, dynamicInfo.accelHalf, deltaTime);
        }
        dynamicInfo.accelHalf = default;

        UpdatePrev(ref dynamicInfo.prev);
    }

    protected virtual P GetPath()
    {
        return new P();
    }

    protected virtual bool ShouldIExecute(P path)
    {
        float distanceToTarget = path.GetMagnitude();
        if ((distanceToTarget < margin) && (targetMode == TargetMode.StopAtMargin))
            distanceToTarget = 0f;
        return distanceToTarget > 0f;
    }

    protected virtual T Accelerated(P path, ref DynamicInfo dynamicInfo, float deltaTime)
    {
        return default;
    }

    protected virtual T SmoothDamp(P path, ref DynamicInfo dynamicInfo, float deltaTime)
    {
        return default;
    }

    protected virtual T LerpSmooth(P path, float deltaTime)
    {
        return default;
    }

    protected virtual T Teleport(P path, float deltaTime)
    {
        return default;
    }

    protected virtual T Linear(P path, float deltaTime)
    {
        return default;
    }

    protected virtual void Execute(T spd, float deltaTime)
    {
    }

    /// <summary>
    /// Instantly moves the origin transform to the target position
    /// </summary>
    public virtual void Teleport()
    {
    }

    public virtual T Current()
    {
        return default;
    }

    public virtual void UpdateSpeed(ref T speed, T prev, T accelHalf, float deltaTime)
    {
    }

    protected struct DynamicInfo
    {
        public T speed;
        public T prev;
        public T accelHalf;

        public DynamicInfo(T speed, T prev, T accelHalf)
        {
            this.speed = speed;
            this.prev = prev;
            this.accelHalf = accelHalf;
        }
    }
}
