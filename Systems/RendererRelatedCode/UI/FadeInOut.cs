using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class FadeInOut : MonoBehaviour
{
    AlphaHolder alphaHolder;

    [SerializeField]
    float timeFadeOut = 0.5f;
    [SerializeField]
    float timeFadeIn = 0.5f;
    [SerializeField]
    OnEnableBehaviour onEnableBehaviour = OnEnableBehaviour.None;
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

    Coroutine co;

    enum OnEnableBehaviour { FadeIn, FadeOut, None }

    void GetAlphaHolder()
    {
        if (!alphaHolder.isNotNull)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null) alphaHolder = new AlphaHolder(canvasGroup);
            else
            {
                Graphic gr = GetComponentInChildren<Graphic>();
                if (gr != null) alphaHolder = new AlphaHolder(gr);
                else
                {
                    RenderersSetColor rsc = GetComponentInChildren<RenderersSetColor>();
                    if (rsc != null) alphaHolder = new AlphaHolder(rsc, useBlack);
                }
            }
        }
    }

    void OnEnable()
    {
        GetAlphaHolder();
        //alphaHolder.alpha = 0f; //Alpha 0 when disabled
        switch (onEnableBehaviour)
        {
            case OnEnableBehaviour.FadeIn:
                FadeIn();
                break;
            case OnEnableBehaviour.FadeOut:
                FadeOut();
                break;
            default:
                CheckInvisibleBehaviour(alphaHolder.alpha);
                break;
        }
    }

    public void FadeIn()
    {
        bool objectWasDisabled = !gameObject.activeInHierarchy;
        gameObject.SetActive(true);
        if (resetAlphaOnCall || objectWasDisabled) alphaHolder.alpha = 0f; //Alpha 0 when disabled
        if (co != null) StopCoroutine(co);
        if (alphaHolder.alpha < 1f) co = StartCoroutine(FadeTo(true));
    }

    public void FadeOut()
    {
        GetAlphaHolder();
        bool objectWasDisabled = !gameObject.activeInHierarchy;
        if (resetAlphaOnCall) alphaHolder.alpha = 1f;
        else if (objectWasDisabled) alphaHolder.alpha = 0f; //Alpha 0 when disabled
        if (co != null) StopCoroutine(co);
        if (alphaHolder.alpha > 0f)
        {
            gameObject.SetActive(true);
            co = StartCoroutine(FadeTo(false));
        }
    }

    void CheckInvisibleBehaviour(float alpha)
    {
        if (deactivateObjectWhenInvisible)
        {
            if (alpha <= 0f) gameObject.SetActive(false);
            else if (alpha > 0f) gameObject.SetActive(true);
        }
    }

    IEnumerator FadeTo(bool fadingIn)
    {
        float currentAlpha;
        bool finished = false;
        do
        {
            float deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            currentAlpha = alphaHolder.alpha;

            if (fadingIn) currentAlpha = Mathf.Min(1f, currentAlpha + (deltaTime / timeFadeIn));
            else currentAlpha = Mathf.Max(0f, currentAlpha - (deltaTime / timeFadeOut));

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
    }

    struct AlphaHolder
    {
        Graphic graphic;
        CanvasGroup canvasGroup;
        RenderersSetColor rsc;
        Mode mode;
        public bool isNotNull;
        bool useBlackValue;

        enum Mode { Graphic, CanvasGroup, RSC }

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
                }
            }
        }

        public AlphaHolder(Graphic graphic)
        {
            mode = Mode.Graphic;
            this.graphic = graphic;
            canvasGroup = null;
            rsc = null;
            isNotNull = true;
            useBlackValue = false;
        }

        public AlphaHolder(CanvasGroup canvasGroup)
        {
            mode = Mode.CanvasGroup;
            this.canvasGroup = canvasGroup;
            graphic = null;
            rsc = null;
            isNotNull = true;
            useBlackValue = false;
        }

        public AlphaHolder(RenderersSetColor rsc, bool useBlackValue = false)
        {
            mode = Mode.RSC;
            this.rsc = rsc;
            graphic = null;
            canvasGroup = null;
            isNotNull = true;
            this.useBlackValue = useBlackValue;
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
