using UnityEngine;

[DefaultExecutionOrder(-10)]
public class ScaleModifier : DXMonoBehaviour
{
    [SerializeField]
    float smoothTime = 0f;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;

    Vector3 scaleTarget;
    Vector3 currentVirtualScale;
    Vector3 tmpSpd;
    bool isInit;

    void OnDisable()
    {
        RestoreScaleInstant();
    }

    void Update()
    {
        if (timeMode.IsSmooth()) DoUpdate(timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) DoUpdate(timeMode.DeltaTime());
    }

    void DoUpdate(float deltaTime)
    {
        TryInit();
        Vector3 newScale = Vector3.SmoothDamp(
            currentVirtualScale, scaleTarget, ref tmpSpd, smoothTime, Mathf.Infinity, deltaTime);
        transform.localScale = transform.localScale.InverseScale(currentVirtualScale);
        transform.localScale = Vector3.Scale(transform.localScale, newScale);
        currentVirtualScale = newScale;
    }

    void TryInit()
    {
        if (!isInit)
        {
            scaleTarget = currentVirtualScale = Vector3.one;
            isInit = true;
        }
    }

    void Scale(Vector3 newScl)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            transform.localScale = newScl;
        else
#endif
            scaleTarget = newScl;
    }

    Vector3 Current()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return transform.localScale;
        else
#endif
        {
            TryInit();
            return scaleTarget;
        }
    }

    void ScaleInstant(Vector3 newScl)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            scaleTarget = currentVirtualScale = newScl;
        transform.localScale = newScl;
    }

    public void Scale(float scale)
    {
        Scale(Current() * scale);
    }

    public void ScaleInstant(float scale)
    {
        ScaleInstant(Current() * scale);
    }

    public void ScaleX(float scale)
    {
        Vector3 current = Current();
        Scale(new Vector3(current.x * scale, current.y, current.z));
    }

    public void ScaleY(float scale)
    {
        Vector3 current = Current();
        Scale(new Vector3(current.x, current.y * scale, current.z));
    }

    public void ScaleZ(float scale)
    {
        Vector3 current = Current();
        Scale(new Vector3(current.x, current.y, current.z * scale));
    }

    public void ScaleAdd(float scale)
    {
        Scale(Current() + (Vector3.one * scale));
    }

    public void ScaleXAdd(float scale)
    {
        Vector3 current = Current();
        Scale(new Vector3(current.x + scale, current.y, current.z));
    }

    public void ScaleYAdd(float scale)
    {
        Vector3 current = Current();
        Scale(new Vector3(current.x, current.y + scale, current.z));
    }

    public void ScaleZAdd(float scale)
    {
        Vector3 current = Current();
        Scale(new Vector3(current.x, current.y, current.z + scale));
    }

    public void ScaleSet(float scale)
    {
        Scale(Vector3.one * scale);
    }

    public void ScaleSetInstant(float scale)
    {
        ScaleInstant(Vector3.one * scale);
    }

    public void ScaleXSet(float scale)
    {
        Vector3 current = Current();
        Scale(new Vector3(scale, current.y, current.z));
    }

    public void ScaleYSet(float scale)
    {
        Vector3 current = Current();
        Scale(new Vector3(current.x, scale, current.z));
    }

    public void ScaleZSet(float scale)
    {
        Vector3 current = Current();
        Scale(new Vector3(current.x, current.y, scale));
    }

    public void RestoreScale()
    {
        Scale(Vector3.one);
    }

    public void RestoreScaleInstant()
    {
        TryInit();
        transform.localScale = transform.localScale.InverseScale(currentVirtualScale);
        scaleTarget = currentVirtualScale = Vector3.one;
    }
}
