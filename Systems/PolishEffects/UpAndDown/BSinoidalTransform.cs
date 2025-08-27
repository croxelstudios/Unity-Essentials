using UnityEngine;
using Sirenix.OdinInspector;

public class BSinoidalTransform : BOffsetBasedTransform
{
    [SerializeField]
    [PropertyOrder(2)]
    [Tooltip("Cycles per second")]
    protected float _speed = 1f;
    //public float speed { get { return _speed; } set { _speed = value; } }
    [SerializeField]
    [PropertyOrder(2)]
    [Range(0f, 1f)]
    float startTime = 0f;

    float currentAngle;

    void Update()
    {
        if (timeMode.IsSmooth()) UpdatePosition(timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) UpdatePosition(Time.fixedDeltaTime);
    }

    void UpdatePosition(float deltaTime)
    {
        float dif = SineWave(ref currentAngle, deltaTime * _speed) - Current();
        ApplyTransform(dif);
    }

    float SineWave(ref float currentAngle, float deltaTime)
    {
        currentAngle = Mathf.Repeat(currentAngle + (360f * deltaTime), 360f);
        return SineWave(currentAngle);
    }

    float SineWave(float currentAngle)
    {
        return Mathf.Sin(currentAngle * Mathf.Deg2Rad);
    }

    protected override void ResetTransform()
    {
        currentAngle = startTime * 360f;
        base.ResetTransform();
    }
}
