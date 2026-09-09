using UnityEngine;

public static class VectorExtension_RoundToInt
{
    public static Vector3Int RoundToInt(this Vector3 input)
    {
        return new Vector3Int(
            Mathf.RoundToInt(input.x),
            Mathf.RoundToInt(input.y),
            Mathf.RoundToInt(input.z)
            );
    }

    public static Vector3Int FloorToInt(this Vector3 input)
    {
        return new Vector3Int(
            Mathf.FloorToInt(input.x),
            Mathf.FloorToInt(input.y),
            Mathf.FloorToInt(input.z)
            );
    }

    public static Vector3Int CeilToInt(this Vector3 input)
    {
        return new Vector3Int(
            Mathf.CeilToInt(input.x),
            Mathf.CeilToInt(input.y),
            Mathf.CeilToInt(input.z)
            );
    }

    public static Vector2Int RoundToInt(this Vector2 input)
    {
        return new Vector2Int(
            Mathf.RoundToInt(input.x),
            Mathf.RoundToInt(input.y)
            );
    }

    public static Vector2Int FloorToInt(this Vector2 input)
    {
        return new Vector2Int(
            Mathf.FloorToInt(input.x),
            Mathf.FloorToInt(input.y)
            );
    }

    public static Vector2Int CeilToInt(this Vector2 input)
    {
        return new Vector2Int(
            Mathf.FloorToInt(input.x),
            Mathf.FloorToInt(input.y)
            );
    }
}
