using Sirenix.OdinInspector;
using System;
using UnityEngine;

[ExecuteAlways]
public class RenderersSetColor_Shift : RenderersSetColor
{
    [Header("Shift Behaviour")]
#if UNITY_EDITOR
    [SerializeField]
    bool executeInEditMode = false;
#endif

    [SerializeField]
    bool useGlobalTime = false;
    [SerializeField]
    ShiftMode shiftMode;
    [ShowIf("shiftMode", ShiftMode.Gradient)]
    [SerializeField] //TO DO: Prevent color and update properties from showing
    public Gradient gradient = new Gradient()
    {
        colorKeys = new GradientColorKey[4] {
            new GradientColorKey(Color.red, 0),
            new GradientColorKey(Color.green, 0.333f),
            new GradientColorKey(Color.blue, 0.666f),
            new GradientColorKey(Color.red, 1f)
        },
        alphaKeys = new GradientAlphaKey[2] {
            new GradientAlphaKey(1, 0),
            new GradientAlphaKey(1, 1)
        }
    };
    [ShowIf("shiftMode", ShiftMode.MultipleGradient)]
    [SerializeField]
    GradientArrayWrapper gradientsWrapper =
        new GradientArrayWrapper(new GradientArray[] { new GradientArray(new Gradient(), 1f) });
    [ShowIf("shiftMode", ShiftMode.ColorBand)]
    [SerializeField]
    ColorBand colorBand;
    [SerializeField]
    float duration = 0.143f;
    [SerializeField]
    float _speed = 1f;
    public float speed { get { return _speed; } set { _speed = value; } }
    [SerializeField]
    RenderingTimeMode timeMode = RenderingTimeMode.Update;

    float currentTime = 0;

    void LateUpdate()
    {
#if UNITY_EDITOR
        if (executeInEditMode || Application.isPlaying)
#endif
        {
            if (useGlobalTime) currentTime = (timeMode.Time() * speed / duration) % 1f;
            else currentTime = (currentTime + (timeMode.DeltaTime() * speed / duration)) % 1f;
            switch (shiftMode)
            {
                case ShiftMode.Gradient:
                    color = gradient.Evaluate(currentTime);
                    break;
                case ShiftMode.MultipleGradient:
                    int g = -1;
                    float count = 0f;
                    float div = 0;
                    do
                    {
                        g++;
                        div = gradientsWrapper.gradients[g].durationWeight / gradientsWrapper.fullDuration;
                        count += div;
                    }
                    while (count < currentTime);
                    count -= div;
                    float t = (currentTime - count) * gradientsWrapper.fullDuration;
                    color = gradientsWrapper.gradients[g].gradient.Evaluate(t);
                    break;
                case ShiftMode.ColorBand:
                    color = colorBand.Evaluate(currentTime);
                    break;
            }
            TryUpdate();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ResetTime();
    }

    public void ResetTime()
    {
        currentTime = 0f;
    }

    enum ShiftMode
    {
        Gradient,
        MultipleGradient,
        ColorBand
    }

    [Serializable]
    struct GradientArray
    {
        public Gradient gradient;
        public float durationWeight;

        public GradientArray(Gradient gradient, float durationWeight)
        {
            this.gradient = gradient;
            this.durationWeight = durationWeight;
        }
    }

    [Serializable]
    struct GradientArrayWrapper
    {
        public GradientArray[] gradients;
        float _fullDuration;
        public float fullDuration
        {
            get
            {
                if (_fullDuration <= 0f) _fullDuration = GetFullDuration();
                return _fullDuration;
            }
            private set { _fullDuration = value; }
        }

        float GetFullDuration()
        {
            float r = 0f;
            for (int i = 0; i < gradients.Length; i++)
                r += gradients[i].durationWeight;
            return r;
        }

        public GradientArrayWrapper(GradientArray[] gradients)
        {
            this.gradients = gradients;
            _fullDuration = -1;
        }
    }
}
