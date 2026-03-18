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
    bool inheritColor = true;

    TMP_Text text;
    Graphic[] targetGraphics;

    const int verticesPerChar = 4;

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

                Vector3 bottomLeft = vertices[ch.vertexIndex];
                Vector3 topRight = vertices[ch.vertexIndex + 2];
                min = new Vector3(Mathf.Min(min.x, bottomLeft.x),
                        Mathf.Min(min.y, bottomLeft.y), Mathf.Min(min.z, bottomLeft.z));
                max = new Vector3(Mathf.Max(max.x, topRight.x),
                    Mathf.Max(max.y, topRight.y), Mathf.Max(max.z, topRight.z));

                if (((max - min).sqrMagnitude > Mathf.Epsilon) && ch.isVisible)
                    characterVisible = true;

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

                //TO DO: Scale the image as the character scale.
                //Maybe comparing bottomLeft and topRight in the mesh
                //with the data recorded by TMP in characterInfo
            }

            Vector3 center = Vector3.Lerp(min, max, 0.5f);
            Vector3 pos = transform.TransformPoint(center);
            transformToMove.position = pos;

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
