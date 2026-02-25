using Sirenix.OdinInspector;
using UnityEngine;

public class ScaleByCurve : MonoBehaviour
{
    [SerializeField]
    float time = 1f;
    [SerializeField]
    float reverseScaleTimeModifier = 1f;
    [SerializeField]
    float scale = 1f;
    [SerializeField]
    [MinValue(0.0001f)]
    float minScale = 0.01f;
    [SerializeField]
    AnimationCurve curve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 0f, 0f) });
    [SerializeField]
    bool scaleOnEnable = true;
    [SerializeField]
    bool normalizeScaleAtStart = false;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;
    [SerializeField]
    DXEvent finishedScale = null;
    [SerializeField]
    DXEvent finishedReverseScale = null;

    float currentTime;
    bool negative;
    float lastScaleModifier;
    bool initialized;
    Coroutine co;

    //Vector3 currentVirtualScale;
    //TO DO: Make this script relative so it is compatible with other scaling scripts.

    void Awake()
    {
        Init();
        //currentVirtualScale = Vector3.one;
    }

    void OnEnable()
    {
        if (scaleOnEnable) DoScaleAnimation();
    }

    void OnDisable()
    {
        if (co != null) StopCoroutine(co);
    }

    void Update()
    {
        if (timeMode.IsSmooth())
            DoUpdate(timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed())
            DoUpdate(timeMode.DeltaTime());
    }

    void DoUpdate(float deltaTime)
    {
        if (currentTime > 0f)
        {
            ScaleAtTime(currentTime);

            currentTime -= deltaTime;

            if (currentTime <= 0f)
            {
                currentTime = 0f;
                if (negative) finishedReverseScale?.Invoke();
                else finishedScale?.Invoke();
            }
        }
    }

    void ScaleAtTime(float currentTime)
    {
        float factor = currentTime / ((negative ? reverseScaleTimeModifier : 1f) * time);
        float current = curve.Evaluate(negative ? factor : 1 - factor) * scale;

        //Prevent 0 scale locks
        if (Mathf.Abs(current) < minScale) current = minScale * Mathf.Sign(current);
        if (transform.localScale.x < minScale)
            transform.localScale = new Vector3(minScale, transform.localScale.y, transform.localScale.z);
        if (transform.localScale.y < minScale)
            transform.localScale = new Vector3(transform.localScale.x, minScale, transform.localScale.z);
        if (transform.localScale.z < minScale)
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, minScale);

        //Modify scale according to curve without affecting other scaling behaviours
        float currentMult = current / lastScaleModifier;
        transform.localScale = transform.localScale * currentMult;

        lastScaleModifier = current;
    }

    void Init()
    {
        if (!initialized)
        {
            lastScaleModifier = 1f;
            initialized = true;
        }
    }

    public void DoScaleAnimation()
    {
        if (this.IsActiveAndEnabled())
        {
            Init();

            if (normalizeScaleAtStart)
            {
                if (transform.localScale.sqrMagnitude <= 0f) transform.localScale = Vector3.one;
                else transform.localScale = transform.localScale.normalized;
            }
            if (currentTime <= 0)
                currentTime = time;
            else if (negative)
                currentTime = Mathf.Clamp(time - currentTime, 0f, time);
            negative = false;

            ScaleAtTime(currentTime);
        }
    }

    public void ReverseScaleAnimation()
    {
        if (this.IsActiveAndEnabled())
        {
            Init();

            if (normalizeScaleAtStart)
            {
                if (transform.localScale.sqrMagnitude <= 0f) transform.localScale = Vector3.one;
                else transform.localScale = transform.localScale.normalized;
            }
            float t = reverseScaleTimeModifier * time;
            if (currentTime <= 0)
                currentTime = t;
            else if (!negative)
                currentTime = Mathf.Clamp(t - currentTime, 0f, t);
            negative = true;

            ScaleAtTime(currentTime);
        }
    }
}
