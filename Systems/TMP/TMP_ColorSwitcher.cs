using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMP_ColorSwitcher : MonoBehaviour
{
    [SerializeField]
    Color[] colors = null;

    Color oldColor;
    TMP_Text t;

    void Initialize()
    {
        t = GetComponent<TMP_Text>();
    }

    public void SwitchColor(int colorId)
    {
        SwitchColor(colors[colorId]);
    }

    public void SwitchColor(Color color)
    {
        if (!t) Initialize();
        if (t.color != color)
        {
            oldColor = t.color;
            t.color = color;
        }
    }

    public void RevertColor()
    {
        if (!t) Initialize();
        if (t.color != oldColor)
        {
            Color oc = t.color;
            t.color = oldColor;
            oldColor = oc;
        }
    }
}
