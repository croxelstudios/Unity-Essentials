using UnityEngine;

public static class VectorExtension_GridRound
{
    public static Vector3 GridRound(this Vector3 position, Vector3 gridCellSize)
    {
        position.x %= gridCellSize.x;
        position.y %= gridCellSize.y;
        position.z %= gridCellSize.z;
        return position;
    }

    public static Vector3 GridRoundForward(this Vector3 position, Vector3 gridCellSize)
    {
        Vector3 newPosition = position.GridRound(gridCellSize);
        if (Mathf.Abs(position.x) > Mathf.Abs(newPosition.x))
            newPosition.x += Mathf.Sign(position.x) * gridCellSize.x;
        if (Mathf.Abs(position.y) > Mathf.Abs(newPosition.y))
            newPosition.y += Mathf.Sign(position.y) * gridCellSize.y;
        if (Mathf.Abs(position.z) > Mathf.Abs(newPosition.z))
            newPosition.z += Mathf.Sign(position.z) * gridCellSize.z;
        return newPosition;
    }

    public static Vector3 GridRoundBackwards(this Vector3 position, Vector3 gridCellSize)
    {
        Vector3 newPosition = position.GridRound(gridCellSize);
        if (Mathf.Abs(position.x) < Mathf.Abs(newPosition.x))
            newPosition.x -= Mathf.Sign(position.x) * gridCellSize.x;
        if (Mathf.Abs(position.y) < Mathf.Abs(newPosition.y))
            newPosition.y -= Mathf.Sign(position.y) * gridCellSize.y;
        if (Mathf.Abs(position.z) < Mathf.Abs(newPosition.z))
            newPosition.z -= Mathf.Sign(position.z) * gridCellSize.z;
        return newPosition;
    }

    public static Vector3 GridRound(this Vector3 position, float gridCellSize)
    {
        position.x %= gridCellSize;
        position.y %= gridCellSize;
        position.z %= gridCellSize;
        return position;
    }

    public static Vector3 GridRoundForward(this Vector3 position, float gridCellSize)
    {
        Vector3 newPosition = position.GridRound(gridCellSize);
        if (Mathf.Abs(position.x) > Mathf.Abs(newPosition.x))
            newPosition.x += Mathf.Sign(position.x) * gridCellSize;
        if (Mathf.Abs(position.y) > Mathf.Abs(newPosition.y))
            newPosition.y += Mathf.Sign(position.y) * gridCellSize;
        if (Mathf.Abs(position.z) > Mathf.Abs(newPosition.z))
            newPosition.z += Mathf.Sign(position.z) * gridCellSize;
        return newPosition;
    }

    public static Vector3 GridRoundBackwards(this Vector3 position, float gridCellSize)
    {
        Vector3 newPosition = position.GridRound(gridCellSize);
        if (Mathf.Abs(position.x) < Mathf.Abs(newPosition.x))
            newPosition.x -= Mathf.Sign(position.x) * gridCellSize;
        if (Mathf.Abs(position.y) < Mathf.Abs(newPosition.y))
            newPosition.y -= Mathf.Sign(position.y) * gridCellSize;
        if (Mathf.Abs(position.z) < Mathf.Abs(newPosition.z))
            newPosition.z -= Mathf.Sign(position.z) * gridCellSize;
        return newPosition;
    }

    public static Vector2 GridRound(this Vector2 position, Vector2 gridCellSize)
    {
        position.x %= gridCellSize.x;
        position.y %= gridCellSize.y;
        return position;
    }

    public static Vector2 GridRoundForward(this Vector2 position, Vector2 gridCellSize)
    {
        Vector2 newPosition = position.GridRound(gridCellSize);
        if (Mathf.Abs(position.x) > Mathf.Abs(newPosition.x))
            newPosition.x += Mathf.Sign(position.x) * gridCellSize.x;
        if (Mathf.Abs(position.y) > Mathf.Abs(newPosition.y))
            newPosition.y += Mathf.Sign(position.y) * gridCellSize.y;
        return newPosition;
    }

    public static Vector2 GridRoundBackwards(this Vector2 position, Vector2 gridCellSize)
    {
        Vector2 newPosition = position.GridRound(gridCellSize);
        if (Mathf.Abs(position.x) < Mathf.Abs(newPosition.x))
            newPosition.x -= Mathf.Sign(position.x) * gridCellSize.x;
        if (Mathf.Abs(position.y) < Mathf.Abs(newPosition.y))
            newPosition.y -= Mathf.Sign(position.y) * gridCellSize.y;
        return newPosition;
    }

    public static Vector2 GridRound(this Vector2 position, float gridCellSize)
    {
        position.x %= gridCellSize;
        position.y %= gridCellSize;
        return position;
    }

    public static Vector2 GridRoundForward(this Vector2 position, float gridCellSize)
    {
        Vector2 newPosition = position.GridRound(gridCellSize);
        if (Mathf.Abs(position.x) > Mathf.Abs(newPosition.x))
            newPosition.x += Mathf.Sign(position.x) * gridCellSize;
        if (Mathf.Abs(position.y) > Mathf.Abs(newPosition.y))
            newPosition.y += Mathf.Sign(position.y) * gridCellSize;
        return newPosition;
    }

    public static Vector2 GridRoundBackwards(this Vector2 position, float gridCellSize)
    {
        Vector2 newPosition = position.GridRound(gridCellSize);
        if (Mathf.Abs(position.x) < Mathf.Abs(newPosition.x))
            newPosition.x -= Mathf.Sign(position.x) * gridCellSize;
        if (Mathf.Abs(position.y) < Mathf.Abs(newPosition.y))
            newPosition.y -= Mathf.Sign(position.y) * gridCellSize;
        return newPosition;
    }
}
