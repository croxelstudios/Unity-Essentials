using UnityEngine;
using UnityEngine.Events;
using System;
using Sirenix.OdinInspector;

//TO DO: Micro-optimize by saving the button values in a static dictionary so that
//other launchers with the same input are not checking it twice (or a similar solution)
public class BInputEventLauncherr : MonoBehaviour
{
    [SerializeField]
    ScaledTimeMode checkTime = ScaledTimeMode.Update;
    [SerializeField]
    ButtonInput[] buttons = null;
    [SerializeField]
    AxisInput[] axes = null;
    [SerializeField]
    VectorInput[] joysticks = null;
    [SerializeField]
    ButtonsState[] buttonStates = null;

    void OnEnable()
    {
        //Buttons
        for (int n = 0; n < buttons.Length; n++)
        {
            if (buttons[n].checkOnEnable && GetButton(buttons[n].button))
                PressButton(ref buttons[n]);
        }

        //Button states
        for (int n = 0; n < buttonStates.Length; n++)
        {
            if (buttonStates[n].checkOnEnable)
            {
                bool state = true;
                foreach (ButtonState button in buttonStates[n].buttonsState)
                    if (GetButton(button.button) != button.state)
                    {
                        state = false;
                        break;
                    }

                if (state)
                {
                    if (!buttonStates[n].prevState)
                    {
                        if (buttonStates[n].isToggle)
                        {
                            if (buttonStates[n].isPressed)
                            {
                                buttonStates[n].SetState(false);
                                buttonStates[n].events.Released?.Invoke();
                            }
                            else
                            {
                                buttonStates[n].SetState(true);
                                buttonStates[n].events.Pressed?.Invoke();
                            }
                        }
                        else
                        {
                            buttonStates[n].SetState(true);
                            buttonStates[n].events.Pressed?.Invoke();
                        }
                    }
                }
                else if (buttonStates[n].prevState)
                {
                    if (!buttonStates[n].isToggle)
                    {
                        buttonStates[n].SetState(false);
                        buttonStates[n].events.Released?.Invoke();
                    }
                }
                buttonStates[n].SetPrevState(state);

            }
        }
        //TO DO: Setter as a sepparate function
    }

    void Update()
    {
        if (!checkTime.IsFixed()) CheckInput();
    }

    void FixedUpdate()
    {
        if (checkTime.IsFixed()) CheckInput();
    }

    void OnDisable()
    {
        UnpressAll(false);
    }

    void CheckInput()
    {
        //Buttons
        for (int n = 0; n < buttons.Length; n++)
        {
            if (GetButtonDown(buttons[n].button))
                PressButton(ref buttons[n]);

            if (GetButtonUp(buttons[n].button))
                ReleaseButton(ref buttons[n]);
        }

        //Axes
        for (int n = 0; n < axes.Length; n++)
        {
            // Calculate value
            float value = GetAxis(axes[n].axis);
            if (axes[n].clamp01) value = Mathf.Clamp01(value);
            value = axes[n].useCurve ? EvaluateAbsoluteCurve(axes[n].curve, value) : value;

            // Calculate pressing state
            bool state = Mathf.Abs(value) > 0f;

            if (axes[n].sendWhenZeroToo || state)
            {
                axes[n].AxisValue?.Invoke(value);
                axes[n].AbsAxisValue?.Invoke(Mathf.Abs(value));
            }

            bool prevState = axes[n].isPressed;
            axes[n].SetState(state);

            if (state) { if (!prevState) axes[n].events.Pressed?.Invoke(); }
            else if (prevState) axes[n].events.Released?.Invoke();
        }
        //TO DO: Setter as a sepparate function

        //Joysticks
        for (int n = 0; n < joysticks.Length; n++)
        {
            // Calculate value
            Vector2 value = GetVector(joysticks[n].joystick);
            value = new Vector2(
                (Mathf.Abs(value.x) < joysticks[n].independantAxesDeadzone) ? 0f : value.x,
                (Mathf.Abs(value.y) < joysticks[n].independantAxesDeadzone) ? 0f : value.y);
            switch (joysticks[n].valueLimit)
            {
                case ValueLimit.Clamp01:
                    value = Mathf.Clamp01(value.magnitude) * value.normalized;
                    break;
                case ValueLimit.Normalized:
                    value = value.normalized;
                    break;
                default:
                    break;
            }
            if (joysticks[n].valueLimit != ValueLimit.Normalized)
                value = ((joysticks[n].valueLimit != ValueLimit.Normalized) && joysticks[n].useCurve) ?
                    EvaluateAbsoluteCurve(joysticks[n].curve, value.magnitude) * value.normalized : value;

            // Calculate pressing state
            bool state = Mathf.Abs(value.sqrMagnitude) > 0f;

            if (state && joysticks[n].squareShape)
            {
                float mag = value.magnitude;
                float magRatio = (mag < 1f) ? (mag / 1f) : 1f;
                //WARNING: This will cause faster accelerations (not speed) when using keyboard,
                //but it's the best solution since I can not detect which device
                //the player is using here and this is normally going to be used while normalized anyway
                if (Mathf.Abs(value.x) > Mathf.Abs(value.y))
                    value *= Mathf.Sign(value.x) / value.x;
                else value *= Mathf.Sign(value.y) / value.y;
                value *= magRatio;
            }

            if (joysticks[n].sendWhenZeroToo || state)
            {
                joysticks[n].JoystickValue?.Invoke(value);
                joysticks[n].JoystickMagnitude?.Invoke(value.magnitude);
            }

            bool prevState = joysticks[n].isPressed;
            joysticks[n].SetState(state);

            if (state)
            {
                if (!prevState)
                    joysticks[n].events.Pressed?.Invoke();
            }
            else if (prevState)
                joysticks[n].events.Released?.Invoke();
        }
        //TO DO: Setter as a sepparate function

        //Button states
        for (int n = 0; n < buttonStates.Length; n++)
        {
            bool state = true;
            foreach (ButtonState button in buttonStates[n].buttonsState)
                if (GetButton(button.button) != button.state)
                {
                    state = false;
                    break;
                }

            if (state)
            {
                if (!buttonStates[n].prevState)
                {
                    if (buttonStates[n].isToggle)
                    {
                        if (buttonStates[n].isPressed)
                        {
                            buttonStates[n].SetState(false);
                            buttonStates[n].events.Released?.Invoke();
                        }
                        else
                        {
                            buttonStates[n].SetState(true);
                            buttonStates[n].events.Pressed?.Invoke();
                        }
                    }
                    else
                    {
                        buttonStates[n].SetState(true);
                        buttonStates[n].events.Pressed?.Invoke();
                    }
                }
            }
            else if (buttonStates[n].prevState)
            {
                if (!buttonStates[n].isToggle)
                {
                    buttonStates[n].SetState(false);
                    buttonStates[n].events.Released?.Invoke();
                }
            }
            buttonStates[n].SetPrevState(state);
        }
        //TO DO: Setter as a sepparate function
    }

    public void UnpressAll(bool resetToggles)
    {
        for (int n = 0; n < buttons.Length; n++) if (buttons[n].isPressed && (resetToggles || !buttons[n].isToggle))
            {
                buttons[n].SetState(false);
                buttons[n].events.Released?.Invoke();
            }

        for (int n = 0; n < axes.Length; n++)
        {
            if (axes[n].sendWhenZeroToo) axes[n].AxisValue?.Invoke(0f);
            if (axes[n].isPressed)
            {
                axes[n].SetState(false);
                axes[n].events.Released?.Invoke();
            }
        }

        for (int n = 0; n < joysticks.Length; n++)
        {
            if (joysticks[n].sendWhenZeroToo)
            {
                joysticks[n].JoystickValue?.Invoke(Vector2.zero);
                joysticks[n].JoystickMagnitude?.Invoke(0f);
            }
            if (joysticks[n].isPressed)
            {
                joysticks[n].SetState(false);
                joysticks[n].events.Released?.Invoke();
            }
        }

        for (int n = 0; n < buttonStates.Length; n++)
        {
            if (buttonStates[n].isPressed && (resetToggles || !buttonStates[n].isToggle))
            {
                buttonStates[n].SetState(false);
                buttonStates[n].events.Released?.Invoke();
            }
        }
    }

    public virtual void ResetInput()
    {
    }

    public void PressButton(string buttonName)
    {
        for (int n = 0; n < buttons.Length; n++)
        {
            if (buttons[n].button.name == buttonName)
                PressButton(ref buttons[n]);
        }
    }

    public void ReleaseButton(string buttonName)
    {
        for (int n = 0; n < buttons.Length; n++)
        {
            if (buttons[n].button.name == buttonName)
                ReleaseButton(ref buttons[n]);
        }
    }

    void PressButton(ref ButtonInput button)
    {
        if (button.isToggle)
        {
            if (button.isPressed)
            {
                button.SetState(false);
                button.events.Released?.Invoke();
            }
            else
            {
                button.SetState(true);
                button.events.Pressed?.Invoke();
            }
        }
        else
        {
            button.SetState(true);
            button.events.Pressed?.Invoke();
        }
    }

    void ReleaseButton(ref ButtonInput button)
    {
        if (!button.isToggle)
        {
            button.SetState(false);
            button.events.Released?.Invoke();
        }
    }

    float EvaluateAbsoluteCurve(AnimationCurve curve, float value)
    {
        return curve.Evaluate(Mathf.Abs(value)) * Mathf.Sign(value);
    }

    [Serializable]
    struct ButtonInput
    {
        [HideLabel]
        [InlineProperty]
        [FoldoutGroup("@GetName()")]
        public Button button;
        [FoldoutGroup("@GetName()")]
        public bool isToggle;
        [FoldoutGroup("@GetName()")]
        public bool checkOnEnable;
        [FoldoutGroup("@GetName()")]
        public ButtonEvents events;
        [FoldoutGroup("@GetName()")]
        public bool isPressed { get; private set; }

        public ButtonInput(Button button, bool isToggle, bool checkOnEnable)
        {
            this.button = button;
            this.isToggle = isToggle;
            this.checkOnEnable = checkOnEnable;
            events = new ButtonEvents();
            isPressed = false;
        }

        public void SetState(bool set)
        {
            isPressed = set;
        }

        string GetName()
        {
            if (string.IsNullOrEmpty(button.name))
                return "Button";
            else return button.name;
        }
    }

    [Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [Serializable]
    struct AxisInput
    {
        [HideLabel]
        [InlineProperty]
        [FoldoutGroup("@GetName()")]
        public Axis axis;
        [FoldoutGroup("@GetName()")]
        public ButtonEvents events;
        [FoldoutGroup("@GetName()")]
        public bool clamp01;
        [FoldoutGroup("@GetName()")]
        public bool sendWhenZeroToo;
        [FoldoutGroup("@GetName()")]
        public bool useCurve;
        [FoldoutGroup("@GetName()")]
        [ShowIf("@useCurve")]
        public AnimationCurve curve;
        [FoldoutGroup("@GetName()")]
        public FloatEvent AxisValue;
        [FoldoutGroup("@GetName()")]
        public FloatEvent AbsAxisValue;
        [FoldoutGroup("@GetName()")]
        public bool isPressed { get; private set; }

        public AxisInput(Axis axis, bool clamp01, bool sendWhenZeroToo, AnimationCurve curve)
        {
            this.axis = axis;
            this.clamp01 = clamp01;
            this.sendWhenZeroToo = sendWhenZeroToo;
            events = new ButtonEvents();
            AxisValue = null;
            AbsAxisValue = null;
            isPressed = false;
            useCurve = (curve != null);
            this.curve = curve;
        }

        public void SetState(bool set)
        {
            isPressed = set;
        }

        string GetName()
        {
            if ((axis.names.Length <= 0) || string.IsNullOrEmpty(axis.names[0]))
                return "Axis";
            else return axis.names[0];
        }
    }

    [Serializable] //TO DO: Implement DXVectorEvent
    class VectorEvent : UnityEvent<Vector2> { }

    [Serializable]
    struct VectorInput
    {
        [HideLabel]
        [InlineProperty]
        [FoldoutGroup("@GetName()")]
        public Joystick joystick;
        [FoldoutGroup("@GetName()")]
        public ButtonEvents events;
        [FoldoutGroup("@GetName()")]
        public ValueLimit valueLimit;
        [FoldoutGroup("@GetName()")]
        public bool squareShape;
        [FoldoutGroup("@GetName()")]
        public bool sendWhenZeroToo;
        [FoldoutGroup("@GetName()")]
        public float independantAxesDeadzone;
        [FoldoutGroup("@GetName()")]
        [ShowIf("@((int)valueLimit) != 2")]
        public bool useCurve;
        [FoldoutGroup("@GetName()")]
        [ShowIf("@useCurve && (((int)valueLimit) != 2)")]
        public AnimationCurve curve;
        [FoldoutGroup("@GetName()")]
        public VectorEvent JoystickValue;
        [FoldoutGroup("@GetName()")]
        public FloatEvent JoystickMagnitude;
        [FoldoutGroup("@GetName()")]
        public bool isPressed { get; private set; }

        public VectorInput(Joystick joystick, float independantAxesDeadzone, ValueLimit valueLimit, bool squareShape,
            bool sendWhenZeroToo, AnimationCurve curve)
        {
            this.joystick = joystick;
            this.independantAxesDeadzone = independantAxesDeadzone;
            this.valueLimit = valueLimit;
            this.squareShape = squareShape;
            this.sendWhenZeroToo = sendWhenZeroToo;
            events = new ButtonEvents();
            JoystickValue = null;
            JoystickMagnitude = null;
            isPressed = false;
            useCurve = (curve != null);
            this.curve = curve;
        }

        public void SetState(bool set)
        {
            isPressed = set;
        }

        string GetName()
        {
            if ((joystick.names.Length <= 0) ||
                (string.IsNullOrEmpty(joystick.names[0].x) && string.IsNullOrEmpty(joystick.names[0].y)))
                return "Joystick";
            else return joystick.names[0].x + "&" + joystick.names[0].y;
        }
    }

    enum ValueLimit { Clamp01, Normalized, None };

    [Serializable]
    struct ButtonsState
    {
        [FoldoutGroup("@GetName()")]
        public bool isToggle;
        [FoldoutGroup("@GetName()")]
        public bool checkOnEnable;
        [FoldoutGroup("@GetName()")]
        public ButtonState[] buttonsState;
        [FoldoutGroup("@GetName()")]
        public ButtonEvents events;
        public bool isPressed { get; private set; }
        public bool prevState { get; private set; }

        public ButtonsState(ButtonState[] buttonsState, bool isToggle, bool checkOnEnable)
        {
            this.isToggle = isToggle;
            this.checkOnEnable = checkOnEnable;
            this.buttonsState = buttonsState;
            events = new ButtonEvents();
            isPressed = false;
            prevState = false;
        }

        public void SetState(bool set)
        {
            isPressed = set;
        }

        public void SetPrevState(bool set)
        {
            prevState = set;
        }

        string GetName()
        {
            string total = buttonsState[0].GetName();
            for (int i = 1; i < buttonsState.Length; i++)
                total += "&" + buttonsState[i].GetName();
            if (string.IsNullOrEmpty(total))
                return "ButtonsState";
            else return total;
        }
    }

    [Serializable]
    struct ButtonState
    {
        [HideLabel]
        [InlineProperty]
        [FoldoutGroup("@GetName()")]
        public Button button;
        [FoldoutGroup("@GetName()")]
        public bool state;

        public ButtonState(Button button, bool state)
        {
            this.button = button;
            this.state = state;
        }

        public string GetName()
        {
            if (string.IsNullOrEmpty(button.name))
                return (state ? "" : "!") + "Button";
            else return (state ? "" : "!") + button.name;
        }
    }

    [Serializable]
    struct ButtonEvents
    {
        public DXEvent Pressed;
        public DXEvent Released;

        public ButtonEvents(DXEvent pressed, DXEvent released)
        {
            Pressed = pressed;
            Released = released;
        }
    }

    [Serializable]
    protected struct JoystickAxes
    {
        public string x;
        public string y;

        public JoystickAxes(string x, string y)
        {
            this.x = x;
            this.y = y;
        }
    }

    //Overridable input functions
    [Serializable]
    protected struct Button
    {
        public string name;
    }

    [Serializable]
    protected struct Axis
    {
        public string[] names;
    }

    [Serializable]
    protected struct Joystick
    {
        public JoystickAxes[] names;
    }

    protected virtual bool GetButton(Button button)
    {
        return false;
    }

    protected virtual bool GetButtonDown(Button button)
    {
        return false;
    }

    protected virtual bool GetButtonUp(Button button)
    {
        return false;
    }

    protected virtual float GetAxis(Axis axis)
    {
        return 0f;
    }

    protected virtual Vector2 GetVector(Joystick joystick)
    {
        return Vector2.zero;
    }
}
