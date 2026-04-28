using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class FloatModifyModes : MonoBehaviour
{
    [SerializeField]
    [OnValueChanged("Apply")]
    float currentValue = 0f;
    [SerializeField]
    int mode = 0;
    [SerializeField]
    ScaleMode[] scaleModes = new ScaleMode[] { new ScaleMode(1f, 0f, 0f, false, false) };
    [SerializeField]
    DXFloatEvent modifiedValue = null;
    [SerializeField]
    RoundMode roundMode = RoundMode.Floor;
    [SerializeField]
    DXIntEvent intValue = null;

    enum RoundMode { Floor, Ceil, Round }

    public void ModifyValue(int value)
    {
        ModifyValue((float)value);
    }

    public void ModifyValue(float value)
    {
        currentValue = value;
        Apply();
    }

    public void ModifyScale(int value)
    {
        ModifyScale((float)value);
    }

    public void ModifyScale(float value)
    {
        ScaleMode sclMode = scaleModes[mode];
        sclMode.scale = value;
        scaleModes[mode] = sclMode;
        Apply();
    }

    void Apply()
    {
        float newValue = scaleModes[mode].Apply(currentValue);
        modifiedValue?.Invoke(newValue);
        int iValue;
        switch (roundMode)
        {
            case RoundMode.Ceil:
                iValue = Mathf.CeilToInt(newValue);
                break;
            case RoundMode.Round:
                iValue = Mathf.RoundToInt(newValue);
                break;
            default:
                iValue = Mathf.FloorToInt(newValue);
                break;
        }
        intValue?.Invoke(iValue);
    }

    public void SetMode(int mode)
    {
        this.mode = (int)Mathf.Clamp(mode, 0f, scaleModes.Length);
    }

    [Serializable]
    public struct ScaleMode
    {
        public float scale;
        [SerializeField]
        float preAddition;
        [SerializeField]
        float postAddition;
        [SerializeField]
        bool abs;
        [SerializeField]
        bool oneMinus;
        [SerializeField]
        bool clamp;
        [Indent]
        [ShowIf("clamp")]
        [SerializeField]
        Vector2 between;

        public ScaleMode(float scale, float preAddition, float postAddition,
            bool abs, bool oneMinus)
        {
            this.scale = scale;
            this.preAddition = preAddition;
            this.postAddition = postAddition;
            this.abs = abs;
            this.oneMinus = oneMinus;
            clamp = false;
            between = new Vector2(0f, 1f);
        }

        public ScaleMode(float scale, float preAddition, float postAddition,
            bool abs, bool oneMinus, Vector2 between)
        {
            this.scale = scale;
            this.preAddition = preAddition;
            this.postAddition = postAddition;
            this.abs = abs;
            this.oneMinus = oneMinus;
            clamp = true;
            this.between = between;
        }

        public float Apply(float value)
        {
            if (abs) value = Mathf.Abs(value);
            value += preAddition;
            value *= scale;
            if (oneMinus) value = 1f - value;
            value += postAddition;
            if (clamp) value = Mathf.Clamp(value, between.x, between.y);
            return value;
        }
    }
}
