using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;

[ExecuteAlways]
public class TMP_BTextPreprocessor : MonoBehaviour, ITextPreprocessor
{
    static Dictionary<TMP_Text, List<TMP_BTextPreprocessor>> processors;
    TMP_Text text;

    protected void Init()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();
        if (text != null)
        {
            text.textPreprocessor = this;
            processors = processors.CreateAdd(text, this);
            text.ForceMeshUpdate(true, true);
        }
    }

    protected void Disable()
    {
        if (text != null)
        {
            processors.SmartRemove(text, this);
            if (!processors[text].IsNullOrEmpty())
                text.textPreprocessor = processors[text][0];
            else text.textPreprocessor = null;
            text.ForceMeshUpdate(true, true);
        }
    }

    protected virtual void OnEnable()
    {
        Init();
    }

    protected virtual void OnDisable()
    {
        Disable();
    }

    public string PreprocessText(string text)
    {
        if ((this.text != null) && processors.NotNullContainsKey(this.text))
            foreach (TMP_BTextPreprocessor processor in processors[this.text])
                text = processor.ProcessText(text);
        return text;
    }

    protected virtual string ProcessText(string text)
    {
        return text;
    }

    protected int Count(Type type = null)
    {
        if (type == null)
            type = GetType();
        if (text == null)
            text = GetComponent<TMP_Text>();
        if (processors.NotNullContainsKey(text))
            return processors[text].Where(p => p.GetType() == type).Count();
        return 0;
    }

    public void UpdateText()
    {
        if (text != null)
            text.ForceMeshUpdate(true, true);
    }
}
