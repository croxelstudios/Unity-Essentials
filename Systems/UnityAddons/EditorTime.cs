using UnityEditor;

[InitializeOnLoad]
public static class EditorTime
{
    public static int frameCount;

    static EditorTime()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        frameCount++;
    }
}
