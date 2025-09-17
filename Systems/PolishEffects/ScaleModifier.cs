using UnityEngine;

[DefaultExecutionOrder(-10)]
public class ScaleModifier : MonoBehaviour
{
    [SerializeField]
    float smoothTime = 0f;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;

    Vector3 scaleTarget;
    Vector3 currentVirtualScale;
    Vector3 tmpSpd;

    void OnEnable()
    {
        currentVirtualScale = Vector3.one;
        scaleTarget = Vector3.one;
    }

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
        Vector3 newScale = Vector3.SmoothDamp(
            currentVirtualScale, scaleTarget, ref tmpSpd, smoothTime, Mathf.Infinity, deltaTime);
        transform.localScale = transform.localScale.InverseScale(currentVirtualScale);
        transform.localScale = Vector3.Scale(transform.localScale, newScale);
        currentVirtualScale = newScale;
    }

    public void Scale(float scale)
    {
        scaleTarget *= scale;
    }

    public void ScaleX(float scale)
    {
        scaleTarget = new Vector3(scaleTarget.x * scale, scaleTarget.y, scaleTarget.z);
    }

    public void ScaleY(float scale)
    {
        scaleTarget = new Vector3(scaleTarget.x, scaleTarget.y * scale, scaleTarget.z);
    }

    public void ScaleZ(float scale)
    {
        scaleTarget = new Vector3(scaleTarget.x, scaleTarget.y, scaleTarget.z * scale);
    }

    public void ScaleAdd(float scale)
    {
        scaleTarget += new Vector3(scale, scale, scale);
    }

    public void ScaleXAdd(float scale)
    {
        scaleTarget = new Vector3(scaleTarget.x + scale, scaleTarget.y, scaleTarget.z);
    }

    public void ScaleYAdd(float scale)
    {
        scaleTarget = new Vector3(scaleTarget.x, scaleTarget.y + scale, scaleTarget.z);
    }

    public void ScaleZAdd(float scale)
    {
        scaleTarget = new Vector3(scaleTarget.x, scaleTarget.y, scaleTarget.z + scale);
    }

    public void ScaleSet(float scale)
    {
        scaleTarget = new Vector3(scale, scale, scale);
    }

    public void ScaleXSet(float scale)
    {
        scaleTarget = new Vector3(scale, scaleTarget.y, scaleTarget.z);
    }

    public void ScaleYSet(float scale)
    {
        scaleTarget = new Vector3(scaleTarget.x, scale, scaleTarget.z);
    }

    public void ScaleZSet(float scale)
    {
        scaleTarget = new Vector3(scaleTarget.x, scaleTarget.y, scale);
    }

    public void RestoreScale()
    {
        scaleTarget = Vector3.one;
    }

    public void RestoreScaleInstant()
    {
        transform.localScale = transform.localScale.InverseScale(currentVirtualScale);
        currentVirtualScale = Vector3.one;
        scaleTarget = Vector3.one;
    }
}
