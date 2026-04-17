using UnityEngine;
using Sirenix.OdinInspector;
using static SpeedBehaviour;

//TO DO: Add intermediate class that removes the Q component.
//TO DO: General Path class that implements ITransformationSequence and generalized common methods
public class BToTarget<T, P, Q> : DXMonoBehaviour where P : ITransformationSequence, new()
{
    [PropertyOrder(-5)]
    [Tooltip("Wether this code should apply transformations to the 'origin' or if " +
        "it should just send the events elsewhere")]
    [SerializeField]
    protected bool applyInTransform = false;

    #region Target
    [Header("Target")]
    [HideLabel]
    [InlineProperty]
    [PropertyOrder(-4)]
    [SerializeField]
    protected OriginTarget originTarget = new OriginTarget("Player");
    public Transform target { get { return originTarget.target; } set { originTarget.SetTarget(value); } }
    public Transform origin { get { return originTarget.origin; } set { originTarget.SetOrigin(value); } }
    #endregion

    #region Transformation
    [Header("Transformation")]
    [PropertyOrder(-3)]
    [Tooltip("Wether or not the resulting action should be projected onto a 2D plane")]
    [SerializeField]
    protected bool projectOnPlane = false;
    [PropertyOrder(-3)]
    [ShowIf("projectOnPlane")]
    [Tooltip("Wether or not the projection plane should be calculated in origin's local space")]
    [SerializeField]
    protected bool projectLocally = false;
    [PropertyOrder(-3)]
    [ShowIf("projectOnPlane")]
    [Tooltip("Projection plane normal")]
    [SerializeField]
    protected Vector3 planeNormal = Vector3.back;
    [PropertyOrder(-3)]
    [SerializeField]
    bool local = false;
    public bool locally { get { return local; } }
    #endregion

    #region Speed
    [Header("Speed behaviour")]
    [HideLabel]
    [InlineProperty]
    [PropertyOrder(-2)]
    [SerializeField]
    protected SpeedBehaviour speedBehaviour = new SpeedBehaviour(SpeedMode.Linear);
    public bool doAccelerate
    {
        get { return speedBehaviour.doAccelerate; }
        set { speedBehaviour.doAccelerate = value; }
    }
    [PropertyOrder(-2)]
    [SerializeField]
    protected TargetMode targetMode = TargetMode.ToExactPoint;
    [PropertyOrder(-2)]
    [ShowIf("@((int)targetMode) == (int)TargetMode.StopAtMargin")]
    [Tooltip("Minimum distance to the target before the result becomes zero.")]
    [SerializeField]
    [Min(0.0000001f)]
    protected float margin = 0.01f;
    [PropertyOrder(-2)]
    [Tooltip("When is this code executed")]
    [SerializeField]
    protected TimeModeOrOnEnable timeMode = TimeModeOrOnEnable.Update;
    [PropertyOrder(-2)]
    [HideIf("@(timeMode == TimeModeOrOnEnable.FixedUpdate) ||" +
        "(timeMode == TimeModeOrOnEnable.OnEnable)")]
    [Indent]
    [SerializeField]
    bool afterAnimations = false;
    #endregion

    #region Event info
    [Space]
    [PropertyOrder(-1)]
    [SerializeField]
    protected bool sendWhenZeroToo = false;
    [LabelText("Per Frame Value")]
    [PropertyOrder(-1)]
    [SerializeField]
    protected bool sendFrameMovement = false;
    #endregion

    #region Events
    [SerializeField]
    [PropertyOrder(10)]
    [FoldoutGroup("$StartStopFoldout")]
    [Tooltip("Resulting transformation was zero and is not zero now")]
    DXEvent started = null;
    [SerializeField]
    [PropertyOrder(10)]
    [FoldoutGroup("$StartStopFoldout")]
    [Tooltip("Resulting transformation was not zero and is zero now")]
    DXEvent stopped = null;

    TransformData recorded;

#if UNITY_EDITOR
    public string StartStopFoldout()
    {
        return "START and STOP" +
            ((started.IsNull() && stopped.IsNull()) ? "" : " ⚠");
    }
#endif

    float prevSpd;
    #endregion

    public enum TargetMode { ToExactPoint, NeverStop, StopAtMargin }

    DynamicInfo dynamicInfo;
    protected T prev;

    void Reset()
    {
        originTarget.SetOrigin(transform);
    }

    void Update()
    {
        if ((!afterAnimations) && timeMode.IsSmooth())
            OnUpdate();
    }

    void FixedUpdate()
    {
        if ((!afterAnimations) && timeMode.IsFixed())
            OnUpdate();
    }

    void LateUpdate()
    {
        if (afterAnimations && timeMode.IsSmooth())
            OnUpdate();
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

        T spd = Default<T>.Value;
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
        dynamicInfo = new DynamicInfo();
        UpdatePrev(ref dynamicInfo.prev);
    }

    protected virtual void Awake()
    {
        recorded = new TransformData(origin);
        UpdatePrev(ref dynamicInfo.prev);
    }

    protected virtual void OnEnable()
    {
        prevSpd = 0f;
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
            UpdateSpeed(ref dynamicInfo.speed, dynamicInfo.accelHalf, dynamicInfo.prev, deltaTime);
        dynamicInfo.accelHalf = Default<Q>.Value;

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
        return Default<T>.Value;
    }

    protected virtual T SmoothDamp(P path, ref DynamicInfo dynamicInfo, float deltaTime)
    {
        return Default<T>.Value;
    }

    protected virtual T LerpSmooth(P path, float deltaTime)
    {
        return Default<T>.Value;
    }

    protected virtual T Teleport(P path, float deltaTime)
    {
        return Default<T>.Value;
    }

    protected virtual T Linear(P path, float deltaTime)
    {
        return Default<T>.Value;
    }

    protected virtual void Execute(T spd, float deltaTime)
    {
    }

    protected void CheckStartStop(float perSecondSpeed)
    {
        if (prevSpd <= Mathf.Epsilon)
        {
            if (perSecondSpeed > Mathf.Epsilon) started?.Invoke();
        }
        else if (perSecondSpeed <= Mathf.Epsilon) stopped?.Invoke();
        prevSpd = perSecondSpeed;
    }

    /// <summary>
    /// Instantly moves the origin transform to the target position
    /// </summary>
    public void Teleport()
    {
        Set(Target(), locally);
    }

    public void Respawn()
    {
        recorded.SetInTransform(origin);
    }

    public virtual void Set(T target, bool isLocal = false)
    {
    }

    public virtual void Apply(T speed, bool isLocal)
    {
    }

    public T Get(Transform tr)
    {
        return locally ? ToLocal(GetGlobal(tr)) : GetGlobal(tr);
    }

    public virtual T GetGlobal(Transform tr)
    {
        return Default<T>.Value;
    }

    public virtual T ToLocal(T value)
    {
        return value;
    }

    public T Target()
    {
        return Get(target);
    }

    public T Current()
    {
        return Get(origin);
    }

    public virtual void UpdateSpeed(ref Q speed, Q accelHalf, T prev, float deltaTime)
    {
    }

    protected struct DynamicInfo
    {
        public Q speed;
        public Q accelHalf;
        public T prev;

        public DynamicInfo(Q speed, Q accelHalf, T prev)
        {
            this.speed = speed;
            this.prev = prev;
            this.accelHalf = accelHalf;
        }
    }
}
