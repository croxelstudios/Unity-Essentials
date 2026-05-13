using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ParticlesRateByScale : MonoBehaviour
{
    [OnValueChanged("SetScale")]
    [SerializeField]
    float timeRatePerUnit = 1f;
    [OnValueChanged("SetScale")]
    [SerializeField]
    float distanceRatePerUnit = 0f;
    [OnValueChanged("SetScale")]
    [SerializeField]
    AxisBooleans includeAxis = new AxisBooleans(true, true, true);

    ParticleSystem p;
    void Awake()
    {
        p = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        SetScale();
        OnEditorChange.PropertyModification_In(PropertyModification);
    }

    void OnDisable()
    {
        OnEditorChange.PropertyModification_Out(PropertyModification);
    }

    void SetScale()
    {
        Vector3 scale = RelevantScale();

        ParticleSystem.ShapeModule shape = p.shape;

        float volume = 1f;
        if (includeAxis.x) volume *= shape.scale.x * scale.x;
        if (includeAxis.y) volume *= shape.scale.y * scale.y;
        if (includeAxis.z) volume *= shape.scale.z * scale.z;
        ParticleSystem.EmissionModule emission = p.emission;
        emission.rateOverTime = timeRatePerUnit * volume;
        emission.rateOverDistance = distanceRatePerUnit * volume;
    }

    Vector3 RelevantScale()
    {
        if (p.main.scalingMode == ParticleSystemScalingMode.Shape)
            return transform.lossyScale;
        else return Vector3.one;
    }

#if UNITY_EDITOR
    void PropertyModification(PropertyModification pm)
    {
        if ((p.main.scalingMode == ParticleSystemScalingMode.Shape) && (pm.target is Transform tr))
        {
            if (transform.IsChildOf(tr) && pm.propertyPath.Contains("m_LocalScale"))
                SetScale();
        }
        else if ((pm.target is ParticleSystem ps) && (ps == p))
        {
            if ((pm.propertyPath == "ShapeModule.scale") || (pm.propertyPath == "MainModule.scalingMode"))
                SetScale();
        }
    }
#endif
}
