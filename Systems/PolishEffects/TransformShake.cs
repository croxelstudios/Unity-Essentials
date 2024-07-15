using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TransformShake : MonoBehaviour
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
    TimeMode timeMode = TimeMode.Update;
    [SerializeField]
    Space space = Space.World;
    [SerializeField]
    bool shakeWhileEnabled = false;
    [SerializeField]
    bool hasCinemachineBrain = false;
    [SerializeField]
    bool applyToParent = false;

    Vector3 current;
    Vector3 currentSpd;
    Coroutine co;

    void Awake()
    {
        if (transformOverride == null) transformOverride = transform;
        if (applyToParent) transformOverride = transformOverride.parent;
    }

    void OnEnable()
    {
        if (hasCinemachineBrain)
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        if (shakeWhileEnabled)
            Shake(Mathf.Infinity);
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        if (co != null) StopCoroutine(co);
        if (gameObject.activeInHierarchy)
            co = StartCoroutine(BackToDefault());
        else
        {
            transformOverride.Translate(-current, space);
            current = Vector3.zero;
        }
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
            if (co != null) StopCoroutine(co);
            co = StartCoroutine(ShakeTime(time));
        }
    }

    public void Shake(float time, float targetChangeFrecuency, float amount, float smooth)
    //TO DO: Should calculate smoothRelative?
    {
        if (this.IsActiveAndEnabled())
        {
            if (co != null) StopCoroutine(co);
            co = StartCoroutine(ShakeTime(time, targetChangeFrecuency, amount, smooth));
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
            float delta = timeMode.DeltaTime();
            t -= delta;
            tc -= delta * intensity;

            if (tc < 0f)
            {
                target = SetNewRandomTarget(amount);
                tc = targetChangeFrecuency;
            }

            Vector3 newCurrent = Vector3.SmoothDamp(current, target, ref currentSpd, GetSmoothness(smooth, amount),
                Mathf.Infinity, timeMode.DeltaTime());
            newCurrent = Vector3.Lerp(Vector3.zero, newCurrent, intensity);
            transformOverride.Translate(newCurrent - current, space);
            current = newCurrent;
        }
        co = StartCoroutine(BackToDefault());
    }

    IEnumerator ShakeTime(float time, float targetChangeTime, float amount, float smooth)
    {
        float t = time;
        float tc = targetChangeTime;
        Vector3 target = SetNewRandomTarget(amount);
        while (t > 0f)
        {
            yield return timeMode.WaitFor();
            float delta = timeMode.DeltaTime();
            t -= delta;
            tc -= delta * intensity;

            if (tc < 0f)
            {
                target = SetNewRandomTarget(amount);
                tc = targetChangeTime;
            }

            Vector3 newCurrent = Vector3.SmoothDamp(current, target, ref currentSpd, GetSmoothness(smooth, amount),
                Mathf.Infinity, timeMode.DeltaTime());
            newCurrent = Vector3.Lerp(Vector3.zero, newCurrent, intensity);
            transformOverride.Translate(newCurrent - current, space);
            current = newCurrent;
        }
        co = StartCoroutine(BackToDefault());
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPaused)
#endif
            if (camera.transform.IsChildOf(transformOverride))
                transformOverride.Translate(current, space);
    }

    Vector3 SetNewRandomTarget(float amount)
    {
        float doubleAmount = amount * 2f;
        return new Vector3(axes.x ? (Random.value * doubleAmount) - amount : 0f, axes.y ? (Random.value * doubleAmount) - amount : 0f,
            axes.z ? (Random.value * doubleAmount) - amount : 0f);
    }

    IEnumerator BackToDefault()
    {
        while (current.sqrMagnitude > 0f)
        {
            yield return timeMode.WaitFor();

            Vector3 newCurrent = Vector3.SmoothDamp(current, Vector3.zero, ref currentSpd, GetSmoothness(smooth, amount));
            transformOverride.Translate(newCurrent - current, space);
            current = newCurrent;
        }
        transformOverride.Translate(-current, space);
        current = Vector3.zero;
    }

    float GetSmoothness(float smooth, float amount)
    {
        //return smooth / amount; //Relative
        return smooth; //Absolute
    }
}
