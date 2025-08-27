using System.Collections;
using UnityEngine;

public class BounceAnimation : MonoBehaviour
{
    [SerializeField]
    float bounceHeight = 0.1f;
    [SerializeField]
    float baseDuration = 0.3f;
    [SerializeField]
    DXEvent touchedFloor = null;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;
    public float _speed = 0f;
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    float currentTime;
    float currentOffset;

    void Awake()
    {
        currentTime = 0f;
        currentOffset = 0f;
    }

    void Update()
    {
        if (timeMode.IsSmooth())
            DoBounce(speed, ref currentOffset, ref currentTime, timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed())
            DoBounce(speed, ref currentOffset, ref currentTime, Time.fixedDeltaTime);
    }

    void DoBounce(float speed, ref float currentOffset, ref float currentTime, float deltaTime)
    {
        if (speed <= 0f) { if (currentTime != 0f) currentTime = 1f; }
        else currentTime += speed * deltaTime / baseDuration;
        if (currentTime >= 1f)
        {
            touchedFloor?.Invoke();
            currentTime -= 1f;
        }
        float newOffset = (1 - Mathf.Pow((2f * currentTime) - 1f, 2f)) * bounceHeight * speed;
        transform.Translate(Vector3.up * (newOffset - currentOffset));
        currentOffset = newOffset;
    }

    public void DoOneBounce()
    {
        StartCoroutine(OneBounceCo());
    }

    IEnumerator OneBounceCo()
    {
        float t = baseDuration;
        float currentOffset = 0f;
        float currentTime = 0f;
        while (t > 0)
        {
            yield return timeMode.WaitFor();
            float deltaTime = timeMode.DeltaTime();
            t -= deltaTime;

            DoBounce(1f, ref currentOffset, ref currentTime, deltaTime);
        }
        transform.Translate(Vector3.up * (0f - currentOffset));
    }
}
