using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct CustomRangeFloat
{
    [BoxGroup("0", ShowLabel = false)]
    [HorizontalGroup("0/0")]
    public float min;
    [BoxGroup("0", ShowLabel = false)]
    [HorizontalGroup("0/0")]
    public float max;
    [BoxGroup("0", ShowLabel = false)]
    [LabelText("")]
    [Range(0f, 1f)]
    public float rangedValue;
    public float value { get { return Mathf.Lerp(min, max, rangedValue); } }

    public CustomRangeFloat(float min, float max, float rangedValue)
    {
        this.min = min;
        this.max = max;
        this.rangedValue = rangedValue;
    }

    public void Randomize()
    {
        rangedValue = Random.value;
    }
}
