using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class BRumbleUtility : MonoBehaviour
{
    [SerializeField]
    int priorityGroup = 0;
    [Space]
    [SerializeField]
    protected AnimationCurve rumbleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 0f, 0f) });

    [Header("Smooth")]
    [SerializeField]
    protected float smoothTime = 0f;
    [SerializeField]
    protected float minDiff = 0.01f;

    public float currentRumble { get; private set; }

    static SortedDictionary<int, List<BRumbleUtility>> rumbles;

    protected virtual void Awake()
    {
        if (rumbles == null) rumbles = new SortedDictionary<int, List<BRumbleUtility>>();
        if (!rumbles.ContainsKey(priorityGroup)) rumbles.Add(priorityGroup, new List<BRumbleUtility>());
        rumbles[priorityGroup].Add(this);
    }

    [ContextMenu("Rumble by curve")]
    public void RumbleByCurve()
    {
        if (this.IsActiveAndEnabled())
            StartCoroutine(StartRumbleByCurveCoroutine());
    }

    IEnumerator StartRumbleByCurveCoroutine()
    {
        float startTime = Time.time;
        float duration = rumbleCurve.keys.Max(x => x.time);
        float endTime = startTime + duration;

        while (Time.time < endTime)
        {
            float t = Time.time - startTime;
            float intensity = rumbleCurve.Evaluate(t);

            SetRumble_Internal(intensity);
            yield return null;
        }

        SetRumble_Internal(0f);
    }

    public void SetRumble(float intensity)
    {
        if (this.IsActiveAndEnabled())
            SetRumble_Internal(intensity);
    }

    [ContextMenu("Rumble smooth")]
    public void RumbleSmooth(float intensity)
    {
        if (this.IsActiveAndEnabled())
            StartCoroutine(StartSmoothedRumbleCoroutine(intensity));
    }

    IEnumerator StartSmoothedRumbleCoroutine(float intensity)
    {
        float current = currentRumble;
        float spd = 0f;

        while (Mathf.Abs(current - intensity) > minDiff)
        {
            current = Mathf.SmoothDamp(current, intensity, ref spd,
                smoothTime, Mathf.Infinity, Time.deltaTime);
            SetRumble_Internal(current);
            yield return null;
        }

        SetRumble_Internal(intensity);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        SetRumble_Internal(0f);
    }

    void SetRumble_Internal(float amount)
    {
        currentRumble = amount;
        UpdateGlobalRumble();
    }

    void UpdateGlobalRumble()
    {
        float globalRumble = 0f;

        for (int i = rumbles.Last().Key; i >= 0; i--)
        {
            float sum = 0f;

            for (int j = 0; j < rumbles[i].Count; j++)
                sum += rumbles[i][j].currentRumble;

            if (sum > 0f)
            {
                globalRumble = Mathf.Clamp01(sum);
                break;
            }
        }

        SetGlobalRumble(globalRumble);
    }

    protected virtual void SetGlobalRumble(float amount)
    {
    }
}
