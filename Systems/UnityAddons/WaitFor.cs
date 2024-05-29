using System.Collections;
using UnityEngine;

/// <summary>
/// USAGE: Inside a coroutine: WaitFor.Frames(count)
/// </summary>
public static class WaitFor
{
    public static IEnumerator Frames(int frameCount)
    {
        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
    }

    public static IEnumerator FixedFrames(int frameCount)
    {
        while (frameCount > 0)
        {
            frameCount--;
            yield return new WaitForFixedUpdate();
        }
    }
}
