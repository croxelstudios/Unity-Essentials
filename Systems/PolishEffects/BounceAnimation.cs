using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class BounceAnimation : BOffsetBasedTransformer<float>
{
    [SerializeField]
    float bounceHeight = 1f;
    [Min(0.001f)]
    [SerializeField]
    float baseDuration = 0.5f;
    [SerializeField]
    bool local = false;
    [SerializeField]
    DXEvent touchedFloor = null;
    public float _speed = 1f;
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    float currentTime;

    protected override void Awake()
    {
        currentTime = 0f;
        base.Awake();
    }

    void Update()
    {
        if (timeMode.IsSmooth())
            DoBounce(speed, ref currentTime, timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed())
            DoBounce(speed, ref currentTime, timeMode.DeltaTime());
    }

    void DoBounce(float speed, ref float currentTime, float deltaTime)
    {
        if (speed <= 0f) { if (currentTime != 0f) currentTime = 1f; }
        else currentTime += speed * deltaTime / baseDuration;
        if (currentTime >= 1f)
        {
            touchedFloor?.Invoke();
            currentTime -= 1f;
        }
        float newOffset = (1 - Mathf.Pow((2f * currentTime) - 1f, 2f)) * bounceHeight * speed;
        SetTransformation(newOffset);
    }

    protected override void Transformation(float value)
    {
        transform.DXTranslate(Vector3.up * value, local ? Space.Self : Space.World);
    }

    protected override void ResetValues()
    {
        currentTime = 0f;
        base.ResetValues();
    }

    public void DoOneBounce()
    {
        StartCoroutine(OneBounceCo());
    }

    IEnumerator OneBounceCo()
    {
        float t = baseDuration;
        float currentTime = 0f;
        while (t > 0)
        {
            yield return timeMode.WaitFor();
            float deltaTime = timeMode.DeltaTime();
            t -= deltaTime;

            DoBounce(1f, ref currentTime, deltaTime);
        }
        ResetTransform();
    }
}
