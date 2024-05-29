using TMPro;
using UnityEngine;
using System.Collections;

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

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        StartCoroutine(WaitTillEndFrame());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void UpdatePosition()
    {
        TMP_TextInfo textInfo = text.textInfo;
        string t = textInfo.textComponent.text;
        if (t.Contains(textSegment))
        {
            int i = t.IndexOf(textSegment);

            if (turnSegmentTransparent)
            {
                if (t.Contains(alphaIntro))
                {
                    t = t.Remove(t.IndexOf(alphaIntro), alphaIntro.Length);
                    i = t.IndexOf(textSegment);
                    t = t.Remove(i + textSegment.Length, alphaOutro.Length);
                    textInfo.textComponent.text = t;
                }
            }

            TMP_CharacterInfo c = textInfo.characterInfo[i];
            float l = 0f;
            for (int j = 0; j < textSegment.Length; j++)
            {
                TMP_CharacterInfo ch = textInfo.characterInfo[i+j];
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

    IEnumerator WaitTillEndFrame()
    {
        yield return new WaitForEndOfFrame();
        UpdatePosition();
    }
}
