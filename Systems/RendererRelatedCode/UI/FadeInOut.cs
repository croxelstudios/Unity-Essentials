using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Sirenix.OdinInspector;

public class FadeInOut : MonoBehaviour
{
    AlphaHolder alphaHolder;

    [SerializeField]
    float timeFadeOut = 0.5f;
    [SerializeField]
    float timeFadeIn = 0.5f;
    [SerializeField]
    FadeBehaviour onEnableBehaviour = FadeBehaviour.None;
    [SerializeField]
    bool deactivateObjectWhenInvisible = true;
    [SerializeField]
    bool resetAlphaOnCall = false;
    [SerializeField]
    bool unscaledTime = false;
    [SerializeField]
    bool useBlack = false;
    [Space]
    [SerializeField]
    DXEvent isIn = null;
    [SerializeField]
    DXEvent isOut = null;
    [SerializeField]
    [FoldoutGroup("Begin events")]
    DXEvent beginIn = null;
    [SerializeField]
    [FoldoutGroup("Begin events")]
    DXEvent beginOut = null;

    Coroutine co;
    FadeBehaviour current;
    FadeBehaviour onEnableOverride = FadeBehaviour.None;
    bool init;

    enum FadeBehaviour { FadeIn, FadeOut, None }

    void GetAlphaHolder()
    {
        if (!alphaHolder.isEnabledAndNotNull)
            alphaHolder = AlphaHolder.GetFromObject(gameObject, useBlack);
    }

    void OnEnable()
    {
        if (!init)
        {
            GetAlphaHolder();
            //alphaHolder.alpha = 0f; //Alpha 0 when disabled

            FadeBehaviour check;
            if (onEnableOverride != FadeBehaviour.None)
                check = onEnableOverride;
            else check = onEnableBehaviour;

            switch (check)
            {
                case FadeBehaviour.FadeIn:
                    FadeIn();
                    break;
                case FadeBehaviour.FadeOut:
                    FadeOut();
                    break;
                default:
                    CheckInvisibleBehaviour(alphaHolder.alpha);
                    break;
            }

            if (onEnableOverride != FadeBehaviour.None)
                onEnableOverride = FadeBehaviour.None;

            if (gameObject.activeInHierarchy) init = true;
        }
    }

    void OnDisable()
    {
        switch (current)
        {
            case FadeBehaviour.FadeIn:
                alphaHolder.alpha = 1f;
                break;
            case FadeBehaviour.FadeOut:
                alphaHolder.alpha = 0f;
                break;
        }
        StopEffect();
        init = false;
    }

    public void FadeIn()
    {
        GetAlphaHolder();
        bool objectWasDisabled = !gameObject.activeInHierarchy;
        if (resetAlphaOnCall || objectWasDisabled) alphaHolder.alpha = 0f; //Alpha 0 when disabled
        StopEffect();
        if (objectWasDisabled)
        {
            onEnableOverride = FadeBehaviour.FadeIn;
            gameObject.SetActive(true);
        }
        else if (alphaHolder.alpha < 1f)
            co = StartCoroutine(FadeTo(true));
    }

    public void FadeOut()
    {
        GetAlphaHolder();
        bool objectWasDisabled = !gameObject.activeInHierarchy;
        if (resetAlphaOnCall) alphaHolder.alpha = 1f;
        else if (objectWasDisabled) alphaHolder.alpha = 0f; //Alpha 0 when disabled
        StopEffect();
        if (objectWasDisabled)
        {
            onEnableOverride = FadeBehaviour.FadeOut;
            gameObject.SetActive(true);
        }
        else if (alphaHolder.alpha > 0f)
            co = StartCoroutine(FadeTo(false));
    }

    public void FadeInInstant()
    {
        GetAlphaHolder();
        StopEffect();
        alphaHolder.alpha = 1f;
        init = true;
    }

    public void FadeOutInstant()
    {
        GetAlphaHolder();
        StopEffect();
        alphaHolder.alpha = 0f;
        init = true;
    }

    void CheckInvisibleBehaviour(float alpha)
    {
        if (deactivateObjectWhenInvisible)
        {
            if (alpha <= 0f) gameObject.SetActive(false);
            else if (alpha > 0f) gameObject.SetActive(true);
        }
    }

    void StopEffect()
    {
        if (co != null) StopCoroutine(co);
        current = FadeBehaviour.None;
    }

    IEnumerator FadeTo(bool fadingIn)
    {
        yield return null;
        if (fadingIn)
        {
            current = FadeBehaviour.FadeIn;
            beginIn?.Invoke();
        }
        else
        {
            current = FadeBehaviour.FadeOut;
            beginOut?.Invoke();
        }

        float currentAlpha;
        bool finished = false;
        do
        {
            float fadeTime = fadingIn ? timeFadeIn : timeFadeOut;
            float deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (deltaTime > fadeTime) deltaTime = 0f; //Don't progress if lag spike
            currentAlpha = alphaHolder.alpha;

            if (fadingIn) currentAlpha = Mathf.Min(1f, currentAlpha + (deltaTime / fadeTime));
            else currentAlpha = Mathf.Max(0f, currentAlpha - (deltaTime / fadeTime));

            if ((currentAlpha >= 1f) && (alphaHolder.alpha < 1f))
            {
                isIn?.Invoke();
                finished = true;
            }
            else if ((currentAlpha <= 0f) && (alphaHolder.alpha > 0f))
            {
                isOut?.Invoke();
                finished = true;
            }

            alphaHolder.alpha = currentAlpha;
            yield return null;
        }
        while (!finished);
        CheckInvisibleBehaviour(currentAlpha);
        StopEffect();
    }

    struct AlphaHolder
    {
        Graphic graphic;
        CanvasGroup canvasGroup;
        RenderersSetColor rsc;
        Light light;
        MaterialPropertyTweaker mpt;
        Mode mode;
        public bool isNotNull
        {
            get
            {
                return (graphic != null) ||
                    (canvasGroup != null) ||
                    (rsc != null) ||
                    (light != null) ||
                    (mpt != null);
            }
        }
        public bool isEnabledAndNotNull
        {
            get
            {
                return ((graphic != null) && graphic.enabled) ||
                    ((canvasGroup != null) && canvasGroup.enabled) ||
                    ((rsc != null) && rsc.enabled) ||
                    ((light != null) && light.enabled) ||
                    ((mpt != null) && mpt.enabled);
            }
        }
        bool useBlackValue;
        Vector2 hs;

        enum Mode { Graphic, CanvasGroup, RSC, Light, MPT }

        public float alpha
        {
            get
            {
                switch (mode)
                {
                    case Mode.Graphic:
                        return graphic.color.a;
                    case Mode.CanvasGroup:
                        return canvasGroup.alpha;
                    case Mode.RSC:
                        if (useBlackValue)
                        {
                            Color.RGBToHSV(rsc.color, out float h, out float s, out float v);
                            return v;
                        }
                        else return rsc.color.a;
                    case Mode.Light:
                        Color.RGBToHSV(light.color, out float hl, out float sl, out float vl);
                        return vl;
                    case Mode.MPT:
                        return mpt.GetAlpha(useBlackValue);
                    default:
                        return 0f;
                }
            }
            set
            {
                Color c;
                switch (mode)
                {
                    case Mode.Graphic:
                        c = graphic.color;
                        c.a = value;
                        graphic.color = c;
                        break;
                    case Mode.CanvasGroup:
                        canvasGroup.alpha = value;
                        break;
                    case Mode.RSC:
                        c = rsc.color;
                        if (useBlackValue)
                        {
                            Color.RGBToHSV(c, out float h, out float s, out float v);
                            c = Color.HSVToRGB(h, s, value);
                        }
                        else c.a = value;
                        rsc.SetColor(c);
                        break;
                    case Mode.Light:
                        c = light.color;
                        Color.RGBToHSV(c, out float hl, out float sl, out float vl);
                        c = Color.HSVToRGB(hs.x, hs.y, value);
                        light.color = c;
                        break;
                    case Mode.MPT:
                        mpt.SetAlpha(value, useBlackValue);
                        break;
                }
            }
        }

        public AlphaHolder(Graphic graphic)
        {
            mode = Mode.Graphic;
            this.graphic = graphic;
            canvasGroup = null;
            rsc = null;
            light = null;
            mpt = null;
            useBlackValue = false;
            hs = Vector2.zero;
        }

        public AlphaHolder(CanvasGroup canvasGroup)
        {
            mode = Mode.CanvasGroup;
            this.canvasGroup = canvasGroup;
            graphic = null;
            rsc = null;
            light = null;
            mpt = null;
            useBlackValue = false;
            hs = Vector2.zero;
        }

        public AlphaHolder(RenderersSetColor rsc, bool useBlackValue = false)
        {
            mode = Mode.RSC;
            this.rsc = rsc;
            graphic = null;
            canvasGroup = null;
            light = null;
            mpt = null;
            this.useBlackValue = useBlackValue;
            hs = Vector2.zero;
        }

        public AlphaHolder(Light light)
        {
            mode = Mode.Light;
            this.light = light;
            graphic = null;
            canvasGroup = null;
            rsc = null;
            mpt = null;
            useBlackValue = false;
            Color.RGBToHSV(light.color, out float h, out float s, out float v);
            hs = new Vector2(h, s);
        }

        public AlphaHolder(MaterialPropertyTweaker mpt, bool useBlackValue = false)
        {
            mode = Mode.MPT;
            this.mpt = mpt;
            graphic = null;
            canvasGroup = null;
            rsc = null;
            light = null;
            this.useBlackValue = useBlackValue;
            hs = Vector2.zero;
        }

        public static AlphaHolder GetFromObject(GameObject obj, bool useBlack = false, bool onlyEnabled = true)
        {
            AlphaHolder holder = GetEnabledFromObject(obj, false, useBlack, onlyEnabled);
            if (!holder.isNotNull)
                holder = GetEnabledFromObject(obj, true, useBlack, onlyEnabled);

            return holder;
        }

        static AlphaHolder GetEnabledFromObject(GameObject obj, bool inChildren = false,
            bool useBlack = false, bool onlyEnabled = true)
        {
            CanvasGroup canvasGroup = inChildren ? obj.GetComponentInChildren<CanvasGroup>() :
                obj.GetComponent<CanvasGroup>();
            if ((canvasGroup != null) && (canvasGroup.enabled || !onlyEnabled))
                return new AlphaHolder(canvasGroup);
            else
            {
                Graphic gr = inChildren ? obj.GetComponentInChildren<Graphic>() : obj.GetComponent<Graphic>();
                if ((gr != null) && (gr.enabled || !onlyEnabled))
                    return new AlphaHolder(gr);
                else
                {
                    RenderersSetColor rsc = inChildren ? obj.GetComponentInChildren<RenderersSetColor>() :
                        obj.GetComponent<RenderersSetColor>();
                    if ((rsc != null) && (rsc.enabled || !onlyEnabled))
                        return new AlphaHolder(rsc, useBlack);
                    else
                    {
                        Light light = inChildren ? obj.GetComponentInChildren<Light>() :
                            obj.GetComponent<Light>();
                        if ((light != null) && (light.enabled || !onlyEnabled))
                            return new AlphaHolder(light);
                        else
                        {
                            MaterialPropertyTweaker mpt = inChildren ?
                                obj.GetComponentInChildren<MaterialPropertyTweaker>() :
                                obj.GetComponent<MaterialPropertyTweaker>();
                            if ((mpt != null) && (mpt.enabled || !onlyEnabled))
                                return new AlphaHolder(mpt, useBlack);
                        }
                    }
                }
            }

            return new AlphaHolder();
        }
    }

    public void SetFadeInTime(float nTime)
    {
        timeFadeIn = nTime;
    }

    public void SetFadeOutTime(float nTime)
    {
        timeFadeOut = nTime;
    }

    public void SetFadeTime(float nTime)
    {
        SetFadeInTime(nTime);
        SetFadeOutTime(nTime);
    }
}
