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
    Modification[] modes = new Modification[] { new Modification(Operation.Type.Scale) };
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
        //ScaleMode sclMode = scaleModes[mode];
        //sclMode.scale = value;
        //scaleModes[mode] = sclMode;
        modes[mode].SetScale(value);
        Apply();
    }

    void Apply()
    {
        //float newValue = scaleModes[mode].Apply(currentValue);
        float newValue = modes[mode].Apply(currentValue);
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
            if (oneMinus) value = 1f - value;
            value += preAddition;
            value *= scale;
            value += postAddition;
            if (clamp) value = Mathf.Clamp(value, between.x, between.y);
            return value;
        }
    }

    [Serializable]
    public struct Modification
    {
        [SerializeField]
        Operation[] operations;

        public Modification(Operation.Type type)
        {
            operations = new Operation[] { new Operation(type) };
        }

        public Modification(Operation.Type type, float amount)
        {
            operations = new Operation[] { new Operation(type, amount) };
        }

        public Modification(float min, float max)
        {
            operations = new Operation[] { new Operation(min, max) };
        }

        public Modification(Vector2 clampBetween)
        {
            operations = new Operation[] { new Operation(clampBetween) };
        }

        public Modification(Operation[] operations)
        {
            this.operations = operations;
        }

        public float Apply(float value)
        {
            for (int i = 0; i < operations.Length; i++)
                value = operations[i].Apply(value);
            return value;
        }

        public void SetScale(float value)
        {
            for (int i = 0; i < operations.Length; i++)
                if (operations[i].type == Operation.Type.Scale)
                {
                    Operation op = operations[i];
                    op.amount = value;
                    operations[i] = op;
                    return;
                }
            operations = operations.Resize(operations.Length + 1);
            operations[operations.Length - 1] = new Operation(value);
        }
    }

    [Serializable]
    public struct Operation
    {
        public enum Type { Scale, Add, Abs, OneMinus, Clamp, Round }
        public enum RoundMode { Nearest, Floor, Ceil }

        [HideLabel]
        [HorizontalGroup]
        public Type type;
        [HideLabel]
        [HorizontalGroup]
        [ShowIf("@(type == Type.Scale) || (type == Type.Add)")]
        public float amount;
        [ShowIf("@type == Type.Clamp")]
        public Vector2 between;
        [HideLabel]
        [HorizontalGroup]
        [ShowIf("@type == Type.Round")]
        public RoundMode roundMode;

        public Operation(Type type)
        {
            this.type = type;
            amount = (type == Type.Scale) ? 1f : 0f;
            between = new Vector2(0f, 1f);
            roundMode = RoundMode.Nearest;
        }

        public Operation(float scale)
        {
            type = Type.Scale;
            amount = scale;
            between = new Vector2(0f, amount);
            roundMode = RoundMode.Nearest;
        }

        public Operation(Type type, float amount)
        {
            this.type = type;
            this.amount = ((type != Type.Clamp) && (type != Type.Round)) ? amount : 0f;
            between = new Vector2(0f, (type == Type.Clamp) ? amount : 1f);
            roundMode = (type == Type.Round) ?
                (RoundMode)Mathf.Clamp(Mathf.Floor(amount), 0f, 2f) : RoundMode.Nearest;
        }

        public Operation(float min, float max)
        {
            type = Type.Clamp;
            amount = 0f;
            between = new Vector2(min, max);
            roundMode = RoundMode.Nearest;
        }

        public Operation(Vector2 clampBetween)
        {
            type = Type.Clamp;
            amount = 0f;
            between = clampBetween;
            roundMode = RoundMode.Nearest;
        }

        public Operation(RoundMode roundMode)
        {
            type = Type.Round;
            amount = 0f;
            between = new Vector2(0f, 1f);
            this.roundMode = roundMode;
        }

        public float Apply(float value)
        {
            switch (type)
            {
                case Type.Scale:
                    return value * amount;
                case Type.Add:
                    return value + amount;
                case Type.Abs:
                    return Mathf.Abs(value);
                case Type.OneMinus:
                    return 1f - value;
                case Type.Clamp:
                    return Mathf.Clamp(value, between.x, between.y);
                case Type.Round:
                    switch (roundMode)
                    {
                        case RoundMode.Floor:
                            return Mathf.Floor(value);
                        case RoundMode.Ceil:
                            return Mathf.Ceil(value);
                        default:
                            return Mathf.Round(value);
                    }
                default:
                    return value;
            }
        }
    }
}
