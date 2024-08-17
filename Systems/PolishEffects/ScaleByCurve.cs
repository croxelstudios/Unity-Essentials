using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ScaleByCurve : MonoBehaviour
{
    [SerializeField]
    float time = 1f;
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

    float lastScaleModifier;
    bool initialized = false;
    Coroutine co;

    void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        if (scaleOnEnable) DoScaleAnimation();
    }

    void OnDisable()
    {
        if (co != null) StopCoroutine(co);
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
        Init();
        if (this.IsActiveAndEnabled())
        {
            if (co != null) StopCoroutine(co);
            co = StartCoroutine(ScaleAnimation(time));
        }
    }

    IEnumerator ScaleAnimation(float time)
    {
        float t = time;

        if (normalizeScaleAtStart)
        {
            if (transform.localScale.sqrMagnitude <= 0f) transform.localScale = Vector3.one;
            else transform.localScale = transform.localScale.normalized;
        }

        while (t > 0f)
        {
            float current = curve.Evaluate(1 - (t / time)) * scale;

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
            yield return timeMode.WaitFor();
            t -= timeMode.DeltaTime();
        }

        finishedScale?.Invoke();
        co = null;
    }
}
