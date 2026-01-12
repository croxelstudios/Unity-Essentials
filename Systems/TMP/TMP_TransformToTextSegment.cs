using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class TMP_TransformToTextSegment : MonoBehaviour
{
    [SerializeField]
    string textSegment = "·";
    [SerializeField]
    Transform transformToMove = null;
    [SerializeField]
    bool turnSegmentTransparent = true;
    [SerializeField]
    bool handleObjectActivation = true;

    TMP_Text text;

    const int verticesPerChar = 4;

    void OnEnable()
    {
        text = GetComponent<TMP_Text>();
        Canvas.willRenderCanvases += UpdatePosition;
    }

    void OnDisable()
    {
        Canvas.willRenderCanvases -= UpdatePosition;
        StopAllCoroutines();
    }

    void UpdatePosition()
    {
        UpdatePosition(new ScriptableRenderContext(), null);
    }

    public void UpdatePosition(ScriptableRenderContext con, List<Camera> cams)
    {
        string t = text.text;
        t = Regex.Replace(t, "<.*?>", string.Empty, RegexOptions.Singleline | RegexOptions.Compiled);
        t = t.Replace("'", " ");
        TMP_TextInfo textInfo = text.textInfo;
        bool characterVisible = false;
        if (t.Contains(textSegment))
        {
            int i = t.IndexOf(textSegment);

            Vector3 min = Vector3.one * Mathf.Infinity;
            Vector3 max = -min;
            text.ForceMeshUpdate();
            for (int j = 0; j < textSegment.Length; j++)
            {
                TMP_CharacterInfo ch = textInfo.characterInfo[i + j];
                Color32[] colors = textInfo.meshInfo[ch.materialReferenceIndex].colors32;

                Vector3 bottomLeft = ch.bottomLeft;
                Vector3 topRight = ch.topRight;
                min = new Vector3(Mathf.Min(min.x, bottomLeft.x),
                        Mathf.Min(min.y, bottomLeft.y), Mathf.Min(min.z, bottomLeft.z));
                max = new Vector3(Mathf.Max(max.x, topRight.x),
                    Mathf.Max(max.y, topRight.y), Mathf.Max(max.z, topRight.z));

                for (byte k = 0; k < verticesPerChar; k++)
                    colors[ch.vertexIndex + k].a = 0;

                if (((max - min).sqrMagnitude > Mathf.Epsilon) && ch.isVisible)
                    characterVisible = true;

                if (turnSegmentTransparent)
                {
                    textInfo.meshInfo[ch.materialReferenceIndex].colors32 = colors;
                    text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                }
            }
            transform.TransformPoint(min).DebugDraw();
            transform.TransformPoint(max).DebugDraw();

            Vector3 pos = transform.TransformPoint(Vector3.Lerp(min, max, 0.5f));
            transformToMove.position = pos;
        }

        if (handleObjectActivation)
        {
            if (characterVisible)
                transformToMove.gameObject.SetActive(true);
            else
                transformToMove.gameObject.SetActive(false);
        }
    }
}
