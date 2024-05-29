using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraExtension_GetFrustrumSizeAtDistance
{
    public static Vector2 GetFrustrumSizeAtDistance(this Camera cam, float distance)
    {
        return GetHalfFrustrumSizeAtDistance(cam, distance) * 2f;
    }

    public static Vector2 GetHalfFrustrumSizeAtDistance(this Camera cam, float distance)
    {
        Vector2 size = Vector2.zero;
        size.y = distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        size.x = size.y * cam.aspect;
        return size;
    }

    public static float GetDistanceFromFrustrumHeight(this Camera cam, float frustrumHeight)
    {
        return GetDistanceFromHalfFrustrumHeight(cam, frustrumHeight) * 0.5f;
    }

    public static float GetDistanceFromHalfFrustrumHeight(this Camera cam, float halfFrustrumHeight)
    {
        return halfFrustrumHeight / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
    }
}
