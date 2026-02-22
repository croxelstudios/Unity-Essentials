using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RenderersSetTexture_FromTMP : RenderersSetTexture
{
    [SerializeField]
    string[] otherProperties = null;
    [SerializeField]
    RenderingTimeModeOrOnEnable timeMode = RenderingTimeModeOrOnEnable.Update;

    Dictionary<Renderer, TMP_Text> tmps;

#if UNITY_EDITOR
    void OnValidate()
    {
        propertyIsReadOnly = true;
    }
#endif

    void LateUpdate()
    {
        if (timeMode.IsSmooth())
            UpdateBehaviour();
    }

    Texture[] oldTexs;
    protected override void Init()
    {
        tmps = new Dictionary<Renderer, TMP_Text>();

        UpdateRenderersInternal();

        for (int i = 0; i < rend.Length; i++)
        {
            TMP_Text tmp = rend[i].gameObject.GetComponent<TMP_Text>();
            tmps.Add(rend[i], tmp);
        }

        base.Init();

        List<Texture> list = new List<Texture>();
        foreach (KeyValuePair<Renderer, TMP_Text> kv in tmps)
        {
            TMP_FontAsset font = kv.Value?.font;
            if (font != null)
                list.Add(font.atlasTexture);
        }
        oldTexs = list.ToArray();
    }

    protected override void UpdateBehaviour()
    {
        int j = 0;
        for (int i = 0; i < rend.Length; i++)
        {
            TMP_FontAsset font = tmps[rend[i]]?.font;
            if (font != null)
            {
                Texture tex = font.atlasTexture;
                if (tex != oldTexs[j])
                {
                    oldTexs[j] = tex;
                    _texture = tex;
                    break;
                }
                j++;
            }
        }

        base.UpdateBehaviour();
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        TMP_FontAsset font = tmps[rendMat.rend]?.font;
        if (font != null)
            _texture = font.atlasTexture;

        base.BlSetProperty(block, rendMat);

        foreach (string prop in otherProperties)
            block.SetTexture(prop, texture);
    }

    protected override void VSetProperty(RendMatProp rendMat)
    {
        TMP_FontAsset font = tmps[rendMat.rend]?.font;
        if (font != null)
            _texture = font.atlasTexture;

        base.VSetProperty(rendMat);

        foreach (string prop in otherProperties)
            rendMat.material.SetTexture(prop, texture);
    }
}
