using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMP_WordArt : MonoBehaviour
{
    [SerializeField]
    WaveOptions positionWave = new WaveOptions();
    [SerializeField]
    ParabolaOptions positionParabola = new ParabolaOptions();
    [SerializeField]
    WaveOptions scaleWave = new WaveOptions();
    [SerializeField]
    ParabolaOptions scaleParabola = new ParabolaOptions();

    TMP_Text TMP;

    void OnEnable()
    {
        TMP = GetComponent<TMP_Text>();
    }

    void Update()
    {
        TMP.ForceMeshUpdate();
        for (int i = 0; i < TMP.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = TMP.textInfo.characterInfo[i];

            if (charInfo.character == ' ') continue;

            Vector3[] vertices = TMP.textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            int vertexIndex = charInfo.vertexIndex;
            Vector3 bottomLeft = vertices[vertexIndex];
            Vector3 topLeft = vertices[vertexIndex + 1];
            Vector3 topRight = vertices[vertexIndex + 2];
            Vector3 bottomRight = vertices[vertexIndex + 3];

            Vector3 center = Vector3.Lerp(bottomLeft, topRight, 0.5f);
            Vector3 corner = topRight - center;

            float yOffset = positionWave.Apply(center.x) + positionParabola.Apply(center.x);
            float scale = 1 + scaleWave.Apply(center.x) + scaleParabola.Apply(center.x);
            bottomLeft = Apply(center, -corner, scale, yOffset);
            topLeft = Apply(center, new Vector3(-corner.x, corner.y, corner.z), scale, yOffset);
            topRight = Apply(center, corner, scale, yOffset);
            bottomRight = Apply(center, new Vector3(corner.x, -corner.y, corner.z), scale, yOffset);

            vertices[vertexIndex] = bottomLeft;
            vertices[vertexIndex + 1] = topLeft;
            vertices[vertexIndex + 2] = topRight;
            vertices[vertexIndex + 3] = bottomRight;
            TMP.textInfo.meshInfo[charInfo.materialReferenceIndex].vertices = vertices;
        }
        TMP.UpdateVertexData();
        //TMP.ForceMeshUpdate();
    }

    Vector3 Apply(Vector3 center, Vector3 corner, float scale, float yOffset)
    {
        return (center + (corner * scale)) + (Vector3.up * yOffset);
    }

    [Serializable]
    struct WaveOptions
    {
        public float amount;
        public float offset;
        public float size;

        public WaveOptions(float amount, float offset, float size)
        {
            this.amount = amount;
            this.offset = offset;
            this.size = size;
        }

        public float Apply(float xPosition)
        {
            return Mathf.Sin((xPosition * size * 0.01f) - offset) * amount;
        }
    }

    [Serializable]
    struct ParabolaOptions
    {
        public float amount;
        public float offset;

        public ParabolaOptions(float amount, float offset)
        {
            this.amount = amount;
            this.offset = offset;
        }

        public float Apply(float xPosition)
        {
            float p = xPosition + offset;
            return (p * p) * amount * 0.01f;
        }
    }
}
