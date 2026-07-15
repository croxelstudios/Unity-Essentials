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
        modes[mode].SetScale(value);
        Apply();
    }

    void Apply()
    {
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
        this.mode = (int)Mathf.Clamp(mode, 0f, modes.Length);
    }

    [Serializable]
    public struct Modification
    {
        [SerializeField]
        [PropertyOrder(1)]
        [ListDrawerSettings(ShowFoldout = false, HideAddButton = true)]
        Operation[] operations;

        [Button("+ Scale")]
        [HorizontalGroup]
        public void AddScale()
        {
            operations = operations.Resize(operations.Length + 1);
            operations[operations.Length - 1] = new Operation(Operation.Type.Scale);
        }

        [Button("+ Add")]
        [HorizontalGroup]
        public void AddAdd()
        {
            operations = operations.Resize(operations.Length + 1);
            operations[operations.Length - 1] = new Operation(Operation.Type.Add);
        }

        [Button("+ Abs")]
        [HorizontalGroup]
        public void AddAbs()
        {
            operations = operations.Resize(operations.Length + 1);
            operations[operations.Length - 1] = new Operation(Operation.Type.Abs);
        }

        [Button("+ OneMinus")]
        [HorizontalGroup]
        public void AddOneMinus()
        {
            operations = operations.Resize(operations.Length + 1);
            operations[operations.Length - 1] = new Operation(Operation.Type.OneMinus);
        }

        [Button("+ Clamp")]
        [HorizontalGroup]
        public void AddClamp()
        {
            operations = operations.Resize(operations.Length + 1);
            operations[operations.Length - 1] = new Operation(Operation.Type.Clamp);
        }

        [Button("+ Round")]
        [HorizontalGroup]
        public void AddRound()
        {
            operations = operations.Resize(operations.Length + 1);
            operations[operations.Length - 1] = new Operation(Operation.Type.Round);
        }

        [Button("+ Loop")]
        [HorizontalGroup]
        public void AddLoop()
        {
            operations = operations.Resize(operations.Length + 1);
            operations[operations.Length - 1] = new Operation(Operation.Type.Loop);
        }

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
        public enum Type { Scale, Add, Abs, OneMinus, Clamp, Round, Loop }
        public enum RoundMode { Nearest, Floor, Ceil }

        [HideLabel]
        [HorizontalGroup]
        public Type type;
        [HideLabel]
        [HorizontalGroup]
        [ShowIf("@(type == Type.Scale) || (type == Type.Add)")]
        public float amount;
        [ShowIf("@(type == Type.Clamp) || (type == Type.Loop)")]
        public Vector2 between;
        [HideLabel]
        [HorizontalGroup]
        [ShowIf("@type == Type.Round")]
        public RoundMode roundMode;

        public Operation(Type type)
        {
            this.type = type;
            amount = (type == Type.Scale) ? 1f : 0f;
            between = (type == Type.Loop) ? new Vector2(-180f, 180f) : new Vector2(0f, 1f);
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
            between = new Vector2(0f, ((type == Type.Clamp) || (type == Type.Loop)) ? amount : 1f);
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
                case Type.Loop:
                    return value.Loop(between.x, between.y);
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
