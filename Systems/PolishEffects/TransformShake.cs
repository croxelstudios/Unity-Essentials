using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TransformShake : BOffsetBasedTransformer<Vector3>
{
    [SerializeField]
    Transform transformOverride = null;
    [SerializeField]
    [Min(0.001f)]
    float amount = 1f;
    [SerializeField]
    float smooth = 0.1f;
    [SerializeField]
    [Min(0.02f)]
    float _targetChangeFrecuency = 0.02f;
    public float targetChangeFrecuency
    {
        get { return _targetChangeFrecuency; }
        set { _targetChangeFrecuency = value; }
    }
    [SerializeField]
    [Range(0f, 1f)]
    float _intensity = 1f;
    public float intensity
    {
        get { return _intensity; }
        set { _intensity = value; }
    }
    [SerializeField]
    AxisBooleans axes = new AxisBooleans(true, true, true);
    [SerializeField]
    Space space = Space.World;
    [SerializeField]
    bool shakeWhileEnabled = false;
    [SerializeField]
    bool hasCinemachineBrain = false;
    [SerializeField]
    bool applyToParent = false;

    Vector3 currentSpd;
    Coroutine shakeCo;

    protected override void Awake()
    {
        if (transformOverride == null) transformOverride = transform;
        if (applyToParent) transformOverride = transformOverride.parent;
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (hasCinemachineBrain)
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        if (shakeWhileEnabled)
            Shake(Mathf.Infinity);
    }

    protected override void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        if (shakeCo != null) StopCoroutine(shakeCo);
        base.OnDisable();
    }

    public void Shake(float time)
    {
        ShakeRelative(time);
    }

    public void ShakeRelative(float time)
    //TO DO: Should calculate smoothRelative?
    {
        if (this.IsActiveAndEnabled())
        {
            if (shakeCo != null) StopCoroutine(shakeCo);
            shakeCo = StartCoroutine(ShakeTime(time));
        }
    }

    public void Shake(float time, float targetChangeFrecuency, float amount, float smooth)
    //TO DO: Should calculate smoothRelative?
    {
        if (this.IsActiveAndEnabled())
        {
            if (shakeCo != null) StopCoroutine(shakeCo);
            shakeCo = StartCoroutine(ShakeTime(time, targetChangeFrecuency, amount, smooth));
        }
    }

    IEnumerator ShakeTime(float time)
    {
        float t = time;
        float tc = targetChangeFrecuency;
        Vector3 target = SetNewRandomTarget(amount);
        while (t > 0f)
        {
            yield return timeMode.WaitFor();
            ShakeOp(ref target, ref t, ref tc, targetChangeFrecuency, amount, smooth);
        }
        GoBackToDefault();
    }

    IEnumerator ShakeTime(float time, float targetChangeTime, float amount, float smooth)
    {
        float t = time;
        float tc = targetChangeTime;
        Vector3 target = SetNewRandomTarget(amount);
        while (t > 0f)
        {
            yield return timeMode.WaitFor();
            ShakeOp(ref target, ref t, ref tc, targetChangeTime, amount, smooth);
        }
        GoBackToDefault();
    }

    void ShakeOp(ref Vector3 target, ref float t, ref float tc, float targetChangeTime,
        float amount, float smooth)
    {
        float delta = timeMode.DeltaTime();
        t -= delta;
        tc -= delta * intensity;

        if (tc < 0f)
        {
            target = SetNewRandomTarget(amount);
            tc = targetChangeTime;
        }

        Vector3 newCurrent = Vector3.SmoothDamp(Current(), target, ref currentSpd,
            GetSmoothness(smooth, amount), Mathf.Infinity, timeMode.DeltaTime());
        newCurrent = Vector3.Lerp(Vector3.zero, newCurrent, intensity);
        ApplyTransform(newCurrent - Current());
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPaused)
#endif
            if (camera.transform.IsChildOf(transformOverride))
                Transformation(Current());
    }

    Vector3 SetNewRandomTarget(float amount)
    {
        float doubleAmount = amount * 2f;
        return new Vector3(axes.x ? (Random.value * doubleAmount) - amount : 0f, axes.y ? (Random.value * doubleAmount) - amount : 0f,
            axes.z ? (Random.value * doubleAmount) - amount : 0f);
    }

    float GetSmoothness(float smooth, float amount)
    {
        //return smooth / amount; //Relative
        return smooth; //Absolute
    }

    protected override void Transformation(Vector3 value)
    {
        transformOverride.DXTranslate(value, space);
    }
}
