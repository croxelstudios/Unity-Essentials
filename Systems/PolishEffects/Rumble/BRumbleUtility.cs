using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class BRumbleUtility : MonoBehaviour
{
    //TO DO: Maybe add curve to the SpeedBehaviour class and use that here and in TransformShake too.
    static SortedDictionary<int, List<BRumbleUtility>> rumbles;
    static Dictionary<int, (float, float)> rumbleValues;
    static float globalIntensity = 1f;

    [SerializeField]
    protected GamepadSelection gamepad = GamepadSelection.Current;
    [Indent]
    [ShowIf("gamepad", GamepadSelection.Specific)]
    [SerializeField]
    protected int gamepadIndex = -1;
    int gamepadId => gamepad == GamepadSelection.Specific ? gamepadIndex :
        gamepad == GamepadSelection.Current ? -1 : -2;
    [SerializeField]
    int priorityGroup = 0;
    [SerializeField]
    Motor motor = Motor.Both;
    [MinValue(0f)]
    [SerializeField]
    float smooth = 0f;
    [SerializeField]
    [Range(0f, 1f)]
    float _intensity = 1f;
    public float intensity { get { return _intensity; } set { _intensity = value; } }
    [SerializeField]
    float _rumbleTime = 0.2f;
    public float rumbleTime { get { return _rumbleTime; } set { _rumbleTime = value; } }
    [SerializeField]
    bool blinkCurve = false;
    [ShowIf("blinkCurve")]
    [SerializeField]
    protected AnimationCurve rumbleCurve =
        new AnimationCurve(new Keyframe[] {
            new(0f, 0f, 1f, 1f),
            new(1f, 1f, 0f, 0f),
            new(0f, 0f, 1f, 1f) });
    [SerializeField]
    bool rumbleWhileEnabled = false;
    [SerializeField]
    bool skipFirstFrameBlinks = true;

    public float currentRumble { get; private set; }
    bool didFirstFrame;
    bool canBlink { get { return (!skipFirstFrameBlinks) || didFirstFrame; } }

    enum Motor { Both, Left, Right }
    protected enum GamepadSelection { Current, All, Specific }

    protected virtual void Awake()
    {
        rumbles = rumbles.CreateAdd(priorityGroup, this);
    }

    protected virtual void OnEnable()
    {
        if (rumbleWhileEnabled) SetRumble(intensity);
        if (skipFirstFrameBlinks)
            StartCoroutine(FrameDelay());
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        //if (gameObject.activeInHierarchy)
        //    SetRumble(0f);
        //else
            SetRumble_Internal(0f);
        if (skipFirstFrameBlinks)
            didFirstFrame = false;
    }

    IEnumerator FrameDelay()
    {
        yield return WaitFor.Frames(1);
        didFirstFrame = true;
    }

    public void SetGamepad(int gamepadIndex)
    {
        if (gamepadIndex < 0)
            gamepad = GamepadSelection.Current;
        else if (gamepadIndex >= GamepadCount())
            gamepad = GamepadSelection.All;
        else
        {
            gamepad = GamepadSelection.Specific;
            this.gamepadIndex = gamepadIndex;
        }
    }

    public void SetRumbleInstant()
    {
        SetRumbleInstant(intensity);
    }

    public void SetGlobalIntensity(float intensity)
    {
        globalIntensity = intensity;
    }

    public void SetRumbleInstant(float intensity)
    {
        if (this.IsActiveAndEnabled())
            SetRumble_Internal(intensity);
    }

    public void SetRumble()
    {
        SetRumble(intensity);
    }

    public void SetRumble(float intensity)
    {
        if (this.IsActiveAndEnabled())
        {
            if (smooth <= 0f) SetRumble_Internal(intensity);
            else StartCoroutine(SetRumbleCoroutine(intensity));
        }
    }

    [ContextMenu("Blink Rumble")]
    public void BlinkRumble()
    {
        BlinkRumble(intensity);
    }

    public void BlinkRumble(float intensity)
    {
        if (this.IsActiveAndEnabled() && canBlink)
        {
            StopAllCoroutines();
            if (blinkCurve) StartCoroutine(RumbleByCurveCoroutine(intensity));
            else StartCoroutine(BlinkRumbleCoroutine(intensity));
        }
    }

    IEnumerator BlinkRumbleCoroutine(float intensity)
    {
        SetRumble(intensity);
        yield return new WaitForSecondsRealtime(rumbleTime);
        SetRumble(0f);
    }

    IEnumerator SetRumbleCoroutine(float intensity)
    {
        float current = currentRumble;
        float spd = 0f;

        while (!Mathf.Approximately(current, intensity))
        {
            current = Mathf.SmoothDamp(current, intensity, ref spd,
                smooth, Mathf.Infinity, Time.unscaledDeltaTime);
            SetRumble_Internal(current);
            yield return null;
        }

        SetRumble_Internal(intensity);
    }

    IEnumerator RumbleByCurveCoroutine(float intensity)
    {
        float startTime = Time.unscaledTime;
        float duration = rumbleCurve.keys.Max(x => x.time) * rumbleTime;
        float endTime = startTime + duration;

        while (Time.unscaledTime < endTime)
        {
            float t = (Time.unscaledTime - startTime) / rumbleTime;
            float current = rumbleCurve.Evaluate(t) * intensity;

            SetRumble_Internal(current);
            yield return null;
        }

        SetRumble_Internal(0f);
    }

    void SetRumble_Internal(float amount)
    {
        currentRumble = amount;
        UpdateGlobalRumble();
    }

    void UpdateGlobalRumble()
    {
        float lRumble = 0f;
        float rRumble = 0f;
        bool checkLeft = motor != Motor.Right;
        bool checkRight = motor != Motor.Left;

        if (rumbleValues.SmartGetValue(gamepadId, out (float, float) values))
        {
            if (!checkLeft) lRumble = values.Item1;
            if (!checkRight) rRumble = values.Item2;
        }
        else rumbleValues = rumbleValues.CreateAdd(gamepadId, (lRumble, rRumble));

        for (int i = rumbles.Last().Key; i >= 0; i--)
        {
            float lSum = 0f;
            float rSum = 0f;

            for (int j = 0; j < rumbles[i].Count; j++)
            {
                BRumbleUtility rumble = rumbles[i][j];
                if (rumble.gamepadId == gamepadId)
                    switch (motor)
                    {
                        case Motor.Both:
                            if (checkLeft) lSum += rumble.currentRumble;
                            if (checkRight) rSum += rumble.currentRumble;
                            break;
                        case Motor.Left:
                            if (checkLeft) lSum += rumble.currentRumble;
                            break;
                        case Motor.Right:
                            if (checkRight) rSum += rumble.currentRumble;
                            break;
                    }
            }

            if (lSum > 0f)
            {
                lRumble = Mathf.Clamp01(lSum);
                checkLeft = false;
            }

            if (rSum > 0f)
            {
                rRumble = Mathf.Clamp01(rSum);
                checkRight = false;
            }

            if (!(checkLeft || checkRight))
                break;
        }

        DoSetRumble(lRumble, rRumble);
    }

    void DoSetRumble(float left, float right)
    {
        rumbleValues[gamepadId] = (left, right);
        SetRumble(left * globalIntensity, right * globalIntensity);
    }

    protected virtual int GamepadCount()
    {
        return 0;
    }

    protected virtual void SetRumble(float left, float right)
    {

    }
}
