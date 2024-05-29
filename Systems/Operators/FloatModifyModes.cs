using System;
using UnityEngine;
using UnityEngine.Events;

public class FloatModifyModes : MonoBehaviour
{
    [SerializeField]
    int mode = 0;
    [SerializeField]
    ScaleMode[] scaleModes = null;
    [SerializeField]
    DXFloatEvent modifiedValue = null;

    public void ModifyValue(int value)
    {
        ModifyValue((float)value);
    }

    public void ModifyValue(float value)
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
