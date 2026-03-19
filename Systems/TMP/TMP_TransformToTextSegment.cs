using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteAlways]
public class TMP_TransformToTextSegment : DXMonoBehaviour
{
    [SerializeField]
    string textSegment = "·";
    [SerializeField]
    Transform transformToMove = null;
    [SerializeField]
    bool turnSegmentTransparent = true;
    [SerializeField]
    bool handleObjectActivation = true;
    [SerializeField]
    bool inheritColor = false;

    TMP_Text text;
    Graphic[] targetGraphics;
    Vector3 scale = Vector3.one;

    const int verticesPerChar = 4;
    const float minScale = 0.0001f;

    void OnEnable()
    {
        text = GetComponent<TMP_Text>();
        targetGraphics = transformToMove.GetComponentsInChildren<Graphic>();
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

    void UpdatePosition(ScriptableRenderContext con, List<Camera> cams)
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
            Vector3 oMin = min;
            Vector3 oMax = max;
            text.ForceMeshUpdate();
            Color color = Color.clear;
            Color minCol = color;
            Color maxCol = color;
            float colorDiv = textSegment.Length * verticesPerChar;
            for (int j = 0; j < textSegment.Length; j++)
            {
                TMP_CharacterInfo ch = textInfo.characterInfo[i + j];
                Mesh mesh = textInfo.meshInfo[ch.materialReferenceIndex].mesh;
                Color32[] colors = mesh.colors32;
                Vector3[] vertices = mesh.vertices;

                //Real min max
                Vector3 bottomLeft = vertices[ch.vertexIndex];
                Vector3 topRight = vertices[ch.vertexIndex + 2];
                min = min.Min(bottomLeft);
                max = max.Max(topRight);

                //Check scale 0
                if (((max - min).sqrMagnitude > Mathf.Epsilon) && ch.isVisible)
                    characterVisible = true;

                //Original min max, to do correct scaling
                bottomLeft = ch.bottomLeft;
                topRight = ch.topRight;
                oMin = oMin.Min(bottomLeft);
                oMax = oMax.Max(topRight);

                //Manage color and alpha
                if (!colors.IsNullOrEmpty())
                {
                    for (byte k = 0; k < verticesPerChar; k++)
                        color += ((Color)colors[ch.vertexIndex + k]) / colorDiv;
                    if (j == 0)
                        minCol = colors[ch.vertexIndex];
                    if (j == (textSegment.Length - 1))
                        maxCol = colors[ch.vertexIndex + 2];

                    if (turnSegmentTransparent)
                    {
                        for (byte k = 0; k < verticesPerChar; k++)
                            colors[ch.vertexIndex + k].a = 0;
                        textInfo.meshInfo[ch.materialReferenceIndex].colors32 = colors;
                        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                    }
                }
            }

            //Position
            Vector3 center = Vector3.Lerp(min, max, 0.5f);
            Vector3 pos = transform.TransformPoint(center);
            transformToMove.position = pos;

            //Scaling
            Vector3 newScale = (max - min).InverseScale(oMax - oMin).Max(minScale);
            Vector3 dif = newScale.InverseScale(scale);
            transformToMove.localScale =
                Vector3.Scale(transformToMove.localScale, dif);
            scale = newScale;

            //Color
            if (inheritColor && (!targetGraphics.IsNullOrEmpty()))
            {
                //center = pos;
                //min = transform.TransformPoint(min);
                //max = transform.TransformPoint(max);
                maxCol.a = minCol.a = color.a = text.color.a;
                foreach (Graphic graphic in targetGraphics)
                {
                    //CanvasRenderer rend = graphic.canvasRenderer;
                    //Mesh mesh = rend.GetMesh();
                    //mesh.MarkDynamic();
                    //Color32[] colors = mesh.colors32;
                    //Vector3[] positions = mesh.vertices;
                    //for (int j = 0; j < colors.Length; j++)
                    //{
                    //    bool isMax = false;
                    //    Vector3 p = graphic.transform.TransformPoint(positions[j]);
                    //    Vector3 toExt = min - center;
                    //    Vector3 toTarget = p - center;
                    //    Vector3 project = Vector3.Project(toTarget, toExt);
                    //    float lerp;
                    //    if (Vector3.Dot(project, toExt) < 0f)
                    //    {
                    //        toExt = max - center;
                    //        project = Vector3.Project(toTarget, toExt);
                    //        isMax = true;
                    //    }
                    //    lerp = project.magnitude / toExt.magnitude;
                    //    colors[j] = (Color32)Color.LerpUnclamped(
                    //        color, isMax ? maxCol : minCol, lerp);
                    //}
                    //mesh.colors32 = colors;
                    //rend.SetMesh(mesh);
                    //^ CRASHES. It would be much better than currrent method if it worked.
                    graphic.color = color;
                }
            }
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
