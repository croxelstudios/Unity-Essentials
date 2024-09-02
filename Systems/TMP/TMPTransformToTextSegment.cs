using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class TMPTransformToTextSegment : MonoBehaviour
{
    [SerializeField]
    string textSegment = "·";
    [SerializeField]
    Transform transformToMove = null;
    [SerializeField]
    bool turnSegmentTransparent = true;

    TMP_Text text;

    const string alphaIntro = "<alpha=#00>";
    const string alphaOutro = "<alpha=#FF>";

    void OnEnable()
    {
        text = GetComponent<TMP_Text>();
        Canvas.willRenderCanvases += UpdatePosition;
        RenderPipelineManager.beginContextRendering += UpdatePosition;
        RenderPipelineManager.endContextRendering += EliminateAlphaWords;
    }

    void OnDisable()
    {
        Canvas.willRenderCanvases -= UpdatePosition;
        RenderPipelineManager.beginContextRendering -= UpdatePosition;
        RenderPipelineManager.endContextRendering -= EliminateAlphaWords;
    }
    void UpdatePosition()
    {
        UpdatePosition(new ScriptableRenderContext(), null);
    }

    public void UpdatePosition(ScriptableRenderContext con, List<Camera> cams)
    {
        TMP_TextInfo textInfo = text.textInfo;
        string t = textInfo.textComponent.text;
        if (t.Contains(textSegment))
        {
            EliminateAlphaWords();
            t = textInfo.textComponent.text;
            int i = t.IndexOf(textSegment);

            TMP_CharacterInfo c = textInfo.characterInfo[i];
            float l = 0f;
            for (int j = 0; j < textSegment.Length; j++)
            {
                TMP_CharacterInfo ch = textInfo.characterInfo[i + j];
                l += Vector3.Distance(ch.topRight, ch.topLeft);
            }
            Vector3 pos = transform.TransformPoint(Vector3.Lerp(c.bottomLeft, c.topLeft, 0.5f) + (Vector3.right * l * 0.5f));
            transformToMove.position = pos;

            if (turnSegmentTransparent)
            {
                t = t.Insert(i, alphaIntro);
                t = t.Insert(i + alphaIntro.Length + textSegment.Length, alphaOutro);
                textInfo.textComponent.text = t;
            }
        }
    }

    void EliminateAlphaWords()
    {
        EliminateAlphaWords(new ScriptableRenderContext(), null);
    }

    void EliminateAlphaWords(ScriptableRenderContext con, List<Camera> cams)
    {
        TMP_TextInfo textInfo = text.textInfo;
        string t = textInfo.textComponent.text;
        if (t.Contains(textSegment))
        {
            if (turnSegmentTransparent)
            {
                if (t.Contains(alphaIntro))
                //TO DO: This is incompatible with more alphas in the text :(
                {
                    t = t.Remove(t.IndexOf(alphaIntro), alphaIntro.Length);
                    int i = t.IndexOf(textSegment);
                    t = t.Remove(i + textSegment.Length, alphaOutro.Length);
                    textInfo.textComponent.text = t;
                }
            }
        }
    }

    IEnumerator WaitTillEndFrame()
    {
        yield return new WaitForEndOfFrame();
        EliminateAlphaWords();
        StartCoroutine(WaitTillEndFrame());
    }
}
