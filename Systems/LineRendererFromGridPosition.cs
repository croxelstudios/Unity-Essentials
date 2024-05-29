using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererFromGridPosition : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    string targetTag = "Player";
    [SerializeField]
    float gridSize = 1f;
    //[SerializeField]
    //Vector3 gridOffset = Vector3.zero;
    [SerializeField]
    int nodeLimit = 100;

    LineRenderer line;
    Transform target;
    Vector3 lastPos;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        target = GameObject.FindGameObjectWithTag(targetTag)?.transform;
        UpdateLineRenderer(transform.InverseTransformPoint(target.position));
    }

    void Update()
    {
        UpdateLineRenderer(transform.InverseTransformPoint(target.position));
        line.SetPosition(line.positionCount - 1, transform.InverseTransformPoint(target.position));
    }

    Vector3Int PosToGridPos(Vector3 pos)
    {
        return new Vector3Int(Mathf.FloorToInt(pos.x / gridSize),
            Mathf.FloorToInt(pos.y / gridSize), Mathf.FloorToInt(pos.z / gridSize));
    }

    Vector3 GridPosToPos(Vector3Int gridPos)
    {
        float halfSize = gridSize * 0.5f;
        return new Vector3((gridPos.x * gridSize) + halfSize,
            (gridPos.y * gridSize) + halfSize, (gridPos.z * gridSize) + halfSize);
    }

    void UpdateLineRenderer(Vector3 newPos)
    {
        Vector3Int newGridPos = PosToGridPos(newPos);
        Vector3Int lastGridPos = PosToGridPos(lastPos);
        if ((line.positionCount <= 0) || (newGridPos != lastGridPos))
        {
            if (line.positionCount > 0)
            {
                Vector3Int dif = newGridPos - lastGridPos;
                int squares = Mathf.Abs(dif.x) + Mathf.Abs(dif.y) + Mathf.Abs(dif.z);
                Vector3 lineStart = lastPos;
                Vector3Int squareStart = lastGridPos;
                for (int i = 0; i < squares; i++)
                {
                    Vector3 center = GridPosToPos(squareStart);
                    Vector3 localLineStart = lineStart - center;
                    Vector3 localLineEnd = newPos - center;
                    Vector4 next = CubeSidesCheck(localLineStart, localLineEnd, gridSize);
                    Vector3Int newSquare = squareStart + new Vector3Int((int)next.x, (int)next.y, (int)next.z);

                    if (PosToGridPos(line.GetPosition(line.positionCount - 2)) == newSquare)
                        RemoveLastLinePos();
                    else if (PosToGridPos(line.GetPosition(line.positionCount - 2)) != squareStart)
                        AddLinePos(GridPosToPos(squareStart));

                    lineStart += ((newPos - lineStart).normalized * next.w);
                    squareStart = newSquare;
                }
            }
            else
            {
                line.positionCount += 1;
                AddLinePos(GridPosToPos(newGridPos));
            }

            lastPos = newPos;
        }
    }

    void RemoveLastLinePos()
    {
        line.positionCount -= 1;
    }

    void AddLinePos(Vector3 pos)
    {
        if (line.positionCount < nodeLimit)
        {
            line.positionCount += 1;
            line.SetPosition(line.positionCount - 2, pos);
        }
    }

    Vector4 CubeSidesCheck(Vector3 lineStart, Vector3 lineEnd, float cubeSize)
    {
        cubeSize *= 0.5f;
        Vector3 v = lineEnd - lineStart;
        float t;
        Vector3 inter;
        //X
        if (v.x >= 0f)
        {
            //(1, 0, 0)
            t = TFunction(V.x, cubeSize);
            inter = new Vector3(cubeSize, RTFunction(V.y, t), RTFunction(V.z, t));
            if ((Mathf.Abs(inter.y) <= Mathf.Abs(cubeSize)) && (Mathf.Abs(inter.z) <= Mathf.Abs(cubeSize)))
                return new Vector4(1, 0, 0, inter.magnitude);
        }
        else
        {
            //(-1, 0, 0)
            t = TFunction(V.x, -cubeSize);
            inter = new Vector3(cubeSize, RTFunction(V.y, t), RTFunction(V.z, t));
            if ((Mathf.Abs(inter.y) <= Mathf.Abs(cubeSize)) && (Mathf.Abs(inter.z) <= Mathf.Abs(cubeSize)))
                return new Vector4(-1, 0, 0, inter.magnitude);
        }
        //Y
        if (v.y >= 0f)
        {
            //(0, 1, 0)
            t = TFunction(V.y, cubeSize);
            inter = new Vector3(RTFunction(V.x, t), cubeSize, RTFunction(V.z, t));
            if ((Mathf.Abs(inter.x) <= Mathf.Abs(cubeSize)) && (Mathf.Abs(inter.z) <= Mathf.Abs(cubeSize)))
                return new Vector4(0, 1, 0, inter.magnitude);
        }
        else
        {
            //(0, -1, 0)
            t = TFunction(V.y, -cubeSize);
            inter = new Vector3(RTFunction(V.x, t), cubeSize, RTFunction(V.z, t));
            if ((Mathf.Abs(inter.x) <= Mathf.Abs(cubeSize)) && (Mathf.Abs(inter.z) <= Mathf.Abs(cubeSize)))
                return new Vector4(0, -1, 0, inter.magnitude);
        }
        //Z
        if (v.z >= 0f)
        {
            //(0, 0, 1)
            t = TFunction(V.z, cubeSize);
            inter = new Vector3(RTFunction(V.x, t), RTFunction(V.y, t), cubeSize);
            if ((Mathf.Abs(inter.x) <= Mathf.Abs(cubeSize)) && (Mathf.Abs(inter.y) <= Mathf.Abs(cubeSize)))
                return new Vector4(0, 0, 1, inter.magnitude);
        }
        else
        {
            //(0, 0, -1)
            t = TFunction(V.z, -cubeSize);
            inter = new Vector3(RTFunction(V.x, t), RTFunction(V.y, t), cubeSize);
            if ((Mathf.Abs(inter.x) <= Mathf.Abs(cubeSize)) && (Mathf.Abs(inter.y) <= Mathf.Abs(cubeSize)))
                return new Vector4(0, 0, -1, inter.magnitude);
        }
        //Fail
        return Vector4.zero;

        float TFunction(V n, float p)
        {
            switch (n)
            {
                case V.x: return (p - lineStart.x) / v.x;
                case V.y: return (p - lineStart.y) / v.y;
                case V.z: return (p - lineStart.z) / v.z;
                default: return 0;
            }
        }

        float RTFunction(V n, float t)
        {
            switch (n)
            {
                case V.x: return lineStart.x + (t * v.x);
                case V.y: return lineStart.y + (t * v.y);
                case V.z: return lineStart.z + (t * v.z);
                default: return 0;
            }
        }
    }

    enum V { x, y, z }
}
