using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class GraphicColorShift : MonoBehaviour
{
    Graphic graphic;

#if UNITY_EDITOR
    [SerializeField]
    bool executeInEditMode = false;
#endif

    [Header("Shift Behaviour")]
    [SerializeField]
    ShiftMode shiftMode;

    [ShowIf("shiftMode", ShiftMode.gradient)]
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

    [ShowIf("shiftMode", ShiftMode.multipleGradient)]
    [SerializeField]
    GradientArrayWrapper gradientsWrapper;
    int gradientsCurrentIndex = 0;
    float prevCurrentTime = 0;

    [ShowIf("shiftMode", ShiftMode.colorBand)]
    [SerializeField]
    ColorBand colorBand;
    [SerializeField]
    float duration = 0.143f;
    [SerializeField]
    protected TimeModeOrOnEnable timeMode = TimeModeOrOnEnable.Update;
    [SerializeField]
    float _speed = 1f;
    public float speed { get { return _speed; } set { _speed = value; } }

    Color original;

    void UpdateBehaviour()
    {
#if UNITY_EDITOR
        if (executeInEditMode || Application.isPlaying)
#endif
        {
            switch (shiftMode)
            {
                case ShiftMode.gradient:
                    graphic.color = gradient.Evaluate((timeMode.Time() * speed / duration) % 1f) * original;
                    break;
                case ShiftMode.multipleGradient:
                    float currentTime = prevCurrentTime +
                        (timeMode.DeltaTime() * speed / duration *
                        gradientsWrapper.gradients[gradientsCurrentIndex].individualSpeed);
                    if (currentTime >= 1) currentTime = 0;
                    if (currentTime < prevCurrentTime)
                    {
                        gradientsCurrentIndex++;
                        if (gradientsCurrentIndex >= gradientsWrapper.gradients.Length)
                            gradientsCurrentIndex = 0;
                    }
                    prevCurrentTime = currentTime;
                    graphic.color = gradientsWrapper.gradients[gradientsCurrentIndex].gradient.Evaluate(currentTime) * original;
                    break;
                case ShiftMode.colorBand:
                    graphic.color = colorBand.Evaluate((timeMode.Time() * speed / duration) % 1f) * original;
                    break;
            }
        }
    }

    void OnEnable()
    {
        graphic = GetComponent<Graphic>();
        original = graphic.color;
    }

    void OnDisable()
    {
        graphic.color = original;
    }

    void Update()
    {
        if (timeMode != TimeModeOrOnEnable.FixedUpdate)
            UpdateBehaviour();
    }

    void FixedUpdate()
    {
        if (timeMode == TimeModeOrOnEnable.FixedUpdate)
            UpdateBehaviour();
    }

    enum ShiftMode
    {
        gradient,
        multipleGradient,
        colorBand
    }

    [Serializable]
    struct GradientArray
    {
        public Gradient gradient;
        public float individualSpeed;

        public GradientArray(Gradient gradient, float individualSpeed)
        {
            this.gradient = gradient;
            this.individualSpeed = individualSpeed;
        }
    }

    [Serializable]
    struct GradientArrayWrapper
    {
        public GradientArray[] gradients;
    }
}
