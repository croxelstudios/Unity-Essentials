using System;
using UnityEngine;
using UnityEngine.Events;

public class FloatModifyModes : MonoBehaviour
{
    [SerializeField]
    int mode = 0;
    [SerializeField]
    ScaleMode[] scaleModes = new ScaleMode[] { new ScaleMode(1f, 0f, 0f, false, false) };
    [SerializeField]
    DXFloatEvent modifiedValue = null;

    float value;

    public void ModifyValue(int value)
    {
        ModifyValue((float)value);
    }

    public void ModifyValue(float value)
    {
        this.value = value;
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
        float newValue = (value + scaleModes[mode].preAddition) * scaleModes[mode].scale;
        if (scaleModes[mode].oneMinus) newValue = 1f - newValue;
        newValue += scaleModes[mode].postAddition;
        modifiedValue?.Invoke(newValue);
    }

    public void SetMode(int mode)
    {
        this.mode = (int)Mathf.Clamp(mode, 0f, scaleModes.Length);
    }

    [Serializable]
    public struct ScaleMode
    {
        public float scale;
        public float preAddition;
        public float postAddition;
        public bool abs;
        public bool oneMinus;

        public ScaleMode(float scale, float preAddition, float postAddition, bool abs, bool oneMinus)
        {
            this.scale = scale;
            this.preAddition = preAddition;
            this.postAddition = postAddition;
            this.abs = abs;
            this.oneMinus = oneMinus;
        }
    }
}
