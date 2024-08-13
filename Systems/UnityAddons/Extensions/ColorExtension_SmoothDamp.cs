using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtension_SmoothDamp
{
    public static Color SmoothDamp(this Color c, Color target, ref Color speed, float smoothTime)
    {
        Color newColor = c;
        newColor.r = Mathf.SmoothDamp(c.r, target.r, ref speed.r, smoothTime);
        newColor.g = Mathf.SmoothDamp(c.g, target.g, ref speed.g, smoothTime);
        newColor.b = Mathf.SmoothDamp(c.b, target.b, ref speed.b, smoothTime);
        newColor.a = Mathf.SmoothDamp(c.a, target.a, ref speed.a, smoothTime);
        return newColor;
    }
}
