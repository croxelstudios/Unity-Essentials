using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GraphicColorShift : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    bool executeInEditMode = false;
#endif
    [SerializeField]
    bool affectChildren = false;
    [SerializeField]
    bool pingPong = false;
    [Header("Shift Behaviour")]
    [SerializeField]
    ShiftMode shiftMode;

    Graphic[] graphics;

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

    [ShowIf("shiftMode", ShiftMode.colorBand)]
    [SerializeField]
    ColorBand colorBand;
    [SerializeField]
    float duration = 0.143f;
    [SerializeField]
    protected RenderingTimeMode timeMode = RenderingTimeMode.Update;
    [SerializeField]
    float _speed = 1f;
    public float speed { get { return _speed; } set { _speed = value; } }

    Color[] originals;

    void UpdateBehaviour()
    {
#if UNITY_EDITOR
        if (executeInEditMode || Application.isPlaying)
#endif
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                Color original = originals[i];
                float time = timeMode.Time() * speed / duration;
                if (pingPong)
                    time = Mathf.PingPong(time, 1f);
                else
                    time = Mathf.Repeat(time, 1f);
                switch (shiftMode)
                {
                    case ShiftMode.gradient:
                        graphic.color = gradient.Evaluate(time) * original;
                        break;
                    case ShiftMode.multipleGradient:
                        graphic.color = gradientsWrapper.Evaluate(time) * original;
                        break;
                    case ShiftMode.colorBand:
                        graphic.color = colorBand.Evaluate(time) * original;
                        break;
                }
            }
        }
    }

    void OnEnable()
    {
        UpdateGraphics();
    }

    void OnDisable()
    {
        for (int i = 0; i < graphics.Length; i++)
            graphics[i].color = originals[i];
    }

    void Update()
    {
        UpdateBehaviour();
    }

    public void UpdateGraphics()
    {
        List<Graphic> list = new List<Graphic>();
        if (affectChildren)
            list.AddRange(GetComponentsInChildren<Graphic>());
        else
            list.Add(GetComponent<Graphic>());

        for (int i = (list.Count - 1); i >= 0; i--)
        {
            GraphicEffect_Exclude exclude = list[i].GetComponent<GraphicEffect_Exclude>();
            if ((exclude != null) && exclude.enabled)
                list.RemoveAt(i);
        }

        graphics = list.ToArray();

        originals = new Color[graphics.Length];
        for (int i = 0; i < graphics.Length; i++)
            originals[i] = graphics[i].color;
    }

    public void ChangeTimeMode(int timeMode)
    {
        this.timeMode = (RenderingTimeMode)timeMode;
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
        public float duration { get { return 1f / individualSpeed; } }

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

        public float totalTime
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < gradients.Length; i++)
                    total += gradients[i].duration;
                return total;
            }
        }

        public Color Evaluate(float time)
        {
            float current = 0f;
            int gradient = 0;
            for (int i = 0; i < gradients.Length; i++)
            {
                current += gradients[i].duration / totalTime;
                if (time < current)
                {
                    gradient = i;
                    current = (1f - (current - time)) *
                        totalTime / gradients[i].duration;
                }
            }
            return gradients[gradient].gradient.Evaluate(current);
        }
    }
}
