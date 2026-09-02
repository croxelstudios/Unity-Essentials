using UnityEngine;
using System.Collections.Generic;

public static class ComputableMeshExtension_CutMesh
{
    static ComputeShader cuttingCompute;
    const string cutMeshComputeShaderName = "CutMeshCompute";
    const string getIntersectionsKernel = "GetIntersections";
    const string getTriangleCutDataKernel = "GetTriangleCutData";
    const string getTriangleCutDataSquareKernel = "GetTriangleCutData_SquareCut";
    const string cleanNullAreaTrianglesInIntersectionKernel = "CleanNullAreaTrianglesInIntersection";

    struct TriangleCutProperties
    {
        public int triCount;
        public int cutPoint1;
        public int cutPoint2;
        public Vector3Int newTri1;
        public Vector3Int newTri2;
        public Vector3Int newTri3;
        public Vector3Int newTri4;

        public static int Size()
        {
            return
                sizeof(int) +
                sizeof(int) + // cutPoint1;
                sizeof(int) + // cutPoint2;
                sizeof(int) * 3 + // Triangle1;
                sizeof(int) * 3 + // Triangle2;
                sizeof(int) * 3 + // Triangle3;
                sizeof(int) * 3; // Triangle4;
        }

        public Vector3Int[] InterpretIndexes(Intersection[] intersections)
        {
            Vector3Int[] result;
            switch (triCount)
            {
                case 3:
                    result = new Vector3Int[3];
                    result[0].x = InterpretTriValue(newTri1.x, intersections);
                    result[0].y = InterpretTriValue(newTri1.y, intersections);
                    result[0].z = InterpretTriValue(newTri1.z, intersections);

                    result[1].x = InterpretTriValue(newTri2.x, intersections);
                    result[1].y = InterpretTriValue(newTri2.y, intersections);
                    result[1].z = InterpretTriValue(newTri2.z, intersections);

                    result[2].x = InterpretTriValue(newTri3.x, intersections);
                    result[2].y = InterpretTriValue(newTri3.y, intersections);
                    result[2].z = InterpretTriValue(newTri3.z, intersections);
                    break;
                case 4:
                    result = new Vector3Int[4];
                    result[0].x = InterpretTriValue(newTri1.x, intersections);
                    result[0].y = InterpretTriValue(newTri1.y, intersections);
                    result[0].z = InterpretTriValue(newTri1.z, intersections);

                    result[1].x = InterpretTriValue(newTri2.x, intersections);
                    result[1].y = InterpretTriValue(newTri2.y, intersections);
                    result[1].z = InterpretTriValue(newTri2.z, intersections);

                    result[2].x = InterpretTriValue(newTri3.x, intersections);
                    result[2].y = InterpretTriValue(newTri3.y, intersections);
                    result[2].z = InterpretTriValue(newTri3.z, intersections);

                    result[3].x = InterpretTriValue(newTri4.x, intersections);
                    result[3].y = InterpretTriValue(newTri4.y, intersections);
                    result[3].z = InterpretTriValue(newTri4.z, intersections);
                    break;
                default:
                    result = new Vector3Int[4];
                    result[0] = newTri1;
                    result[1] = newTri2;
                    result[2] = newTri3;
                    result[3] = newTri4;
                    break;
            }
            return result;
        }

        int InterpretTriValue(int value, Intersection[] intersections)
        {
            switch (value)
            {
                case -1:
                    return intersections[cutPoint1].info;
                case -2:
                    return intersections[cutPoint1].info + 1;
                case -3:
                    return intersections[cutPoint2].info;
                case -4:
                    return intersections[cutPoint2].info + 1;
                default:
                    return value;
            }
        }
    }

    struct Intersection
    {
        public int info;
        public int leftIndex;
        public ComputableMesh.VertexData point;
        public ComputableMesh.VertexData extraPoint;

        public static int Size()
        {
            return
                sizeof(int) +
                sizeof(uint) +
                ComputableMesh.VertexData.Size() +
                ComputableMesh.VertexData.Size();
        }
    };

    public static void CutMesh(this ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint, int submesh = -1)
    {
        CutMesh(mesh, planeNormal, planePoint, out int[] side1, out int[] side2, out int[] extremes,
            -1, submesh);
    }

    public static void CutMesh(this ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint, float minArea, int submesh = -1)
    {
        CutMesh(mesh, planeNormal, planePoint, out int[] side1, out int[] side2, out int[] extremes,
            minArea, submesh);
    }

    public static void CutMesh(this ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint,
        out int[] side1, out int[] side2, out int[] extremes, int submesh = -1)
    {
        CutMesh(mesh, planeNormal, planePoint, out side1, out side2, out extremes,
            -1, submesh);
    }

    public static void CutMesh(this ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint,
        out int[] side1, out int[] side2, out int[] extremes, float minArea, int submesh = -1)
    {
        if (cuttingCompute == null)
            cuttingCompute = (ComputeShader)Resources.Load(cutMeshComputeShaderName);

        if (submesh < 0)
        {
            List<int> side1l = new List<int>();
            List<int> side2l = new List<int>();
            List<int> extremesl = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                CutMesh_Internal(mesh, planeNormal, planePoint,
                    out side1, out side2, out extremes, i, minArea);
                side1l.AddRange(side1);
                side2l.AddRange(side2);
                extremesl.AddRange(extremes);
            }
            side1 = side1l.ToArray();
            side2 = side2l.ToArray();
            extremes = extremesl.ToArray();
        }
        else CutMesh_Internal(mesh, planeNormal, planePoint,
            out side1, out side2, out extremes, submesh, minArea);
    }

    static void CutMesh_Internal(ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint,
        out int[] side1, out int[] side2, out int[] extremes, int submesh, float minArea = -1f)
    {
        GetPlaneCutData(mesh, planeNormal, planePoint, submesh,
            out ComputeBuffer intersectionsBuff, out ComputeBuffer cutsDataBuff);

        //
        //

        if (minArea > 0f)
            Compute_CleanNullAreaTriangles(mesh, intersectionsBuff, cutsDataBuff,
                minArea, (int)mesh.GetIndexCount(submesh));

        RebuildMeshFromCutData(mesh, intersectionsBuff, cutsDataBuff, submesh,
            out side1, out side2, out extremes);
    }

    public static void CutMesh_Square(this ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint, Vector3 upDirection,
        float squareSize, int submesh = -1)
    {
        CutMesh_Square(mesh, planeNormal, planePoint, upDirection, squareSize,
            out int[] side1, out int[] side2, out int[] extremes, -1, submesh);
    }

    public static void CutMesh_Square(this ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint, Vector3 upDirection,
        float squareSize, float minArea, int submesh = -1)
    {
        CutMesh_Square(mesh, planeNormal, planePoint, upDirection, squareSize,
            out int[] side1, out int[] side2, out int[] extremes, minArea, submesh);
    }

    public static void CutMesh_Square(this ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint,
        Vector3 upDirection, float squareSize, out int[] side1, out int[] side2,
        out int[] extremes, int submesh = -1)
    {
        CutMesh_Square(mesh, planeNormal, planePoint, upDirection, squareSize,
            out side1, out side2, out extremes, -1, submesh);
    }

    public static void CutMesh_Square(this ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint,
        Vector3 upDirection, float squareSize, out int[] side1, out int[] side2,
        out int[] extremes, float minArea, int submesh = -1)
    {
        if (cuttingCompute == null)
            cuttingCompute = (ComputeShader)Resources.Load(cutMeshComputeShaderName);

        if (submesh < 0)
        {
            List<int> side1l = new List<int>();
            List<int> side2l = new List<int>();
            List<int> extremesl = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                CutMesh_Square_Internal(mesh, planeNormal, planePoint, upDirection, squareSize,
                    out side1, out side2, out extremes, i, minArea);
                side1l.AddRange(side1);
                side2l.AddRange(side2);
                extremesl.AddRange(extremes);
            }
            side1 = side1l.ToArray();
            side2 = side2l.ToArray();
            extremes = extremesl.ToArray();
        }
        else CutMesh_Square_Internal(mesh, planeNormal, planePoint, upDirection, squareSize,
            out side1, out side2, out extremes, submesh, minArea);
    }

    public static void CutMesh_Square_Internal(this ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint,
        Vector3 upDirection, float squareSize, out int[] side1, out int[] side2,
        out int[] extremes, int submesh, float minArea = -1f)
    {
        GetPlaneCutData(mesh, planeNormal, planePoint, submesh,
            out ComputeBuffer intersectionsBuff, out ComputeBuffer cutsDataBuff);

        //
        if (squareSize > 0f)
            Compute_GetTriangleCutDatas_SquareCut(mesh, intersectionsBuff, cutsDataBuff,
                upDirection, squareSize, submesh);
        //

        if (minArea > 0f)
            Compute_CleanNullAreaTriangles(mesh, intersectionsBuff, cutsDataBuff,
                minArea, (int)mesh.GetIndexCount(submesh));

        RebuildMeshFromCutData(mesh, intersectionsBuff, cutsDataBuff, submesh,
            out side1, out side2, out extremes);
    }

    static void GetPlaneCutData(ComputableMesh mesh, Vector3 planeNormal, Vector3 planePoint, int submesh,
        out ComputeBuffer intersectionsBuff, out ComputeBuffer triangleCutsDataBuff)
    {
        int indexCount = (int)mesh.GetIndexCount(submesh);

        //Get edges
        ComputeBuffer edgesBuff = mesh.GetEdges(submesh);

        //Get intersections
        intersectionsBuff = new ComputeBuffer(indexCount, Intersection.Size());
        Compute_GetIntersections(mesh, edgesBuff, intersectionsBuff,
            planeNormal, planePoint, indexCount);

        triangleCutsDataBuff = new ComputeBuffer(
            indexCount / 3, TriangleCutProperties.Size());
        Compute_GetTriangleCutDatas(mesh, intersectionsBuff, triangleCutsDataBuff, submesh);
    }

    static void RebuildMeshFromCutData(ComputableMesh mesh, ComputeBuffer intersectionsBuff, ComputeBuffer triangleCutsDataBuff,
        int submesh, out int[] side1, out int[] side2, out int[] extremes)
    {
        Intersection[] interArr = new Intersection[intersectionsBuff.count];
        intersectionsBuff.GetData(interArr);
        TriangleCutProperties[] cutsArr = new TriangleCutProperties[triangleCutsDataBuff.count];
        triangleCutsDataBuff.GetData(cutsArr);

        //Structure vertices to add
        List<int> side1l = new List<int>();
        List<int> side2l = new List<int>();
        List<int> extremesl = new List<int>();
        List<ComputableMesh.VertexData> verticesToAdd = new List<ComputableMesh.VertexData>();
        for (int i = 0; i < interArr.Length; i++)
            if (interArr[i].info == -1)
            {
                int vertexID = mesh.vertexCount + verticesToAdd.Count;

                interArr[i].info = vertexID;
                verticesToAdd.Add(interArr[i].point);
                verticesToAdd.Add(interArr[i].point);

                side1l.Add(vertexID);
                side2l.Add(vertexID + 1);
            }
            else if (interArr[i].info == -2)
            {
                int vertexID = mesh.vertexCount + verticesToAdd.Count;

                interArr[i].info = vertexID;
                verticesToAdd.Add(interArr[i].extraPoint);

                extremesl.Add(vertexID);
            }
        side1 = side1l.ToArray();
        side2 = side2l.ToArray();
        extremes = extremesl.ToArray();

        //Structure indices to add
        List<uint> indicesToAdd = new List<uint>();
        List<uint> trianglesToRemove = new List<uint>();
        for (int i = 0; i < cutsArr.Length; i++)
        {
            Vector3Int[] tris;
            if (cutsArr[i].triCount > 1)
            {
                trianglesToRemove.Add((uint)i * 3);
                tris = cutsArr[i].InterpretIndexes(interArr);
                for (int j = 0; j < tris.Length; j++)
                    if (tris[j].x >= 0)
                    {
                        indicesToAdd.Add((uint)tris[j].x);
                        indicesToAdd.Add((uint)tris[j].y);
                        indicesToAdd.Add((uint)tris[j].z);
                    }
            }
        }

        //Add vertices
        mesh.AddVertices(verticesToAdd);

        //Replace triangles
        for (int i = 0; i < indicesToAdd.Count; i += 3)
        {
            if (trianglesToRemove.Count > 0)
            {
                int ind = (int)trianglesToRemove[0];
                trianglesToRemove.RemoveAt(0);
                mesh.ReplaceIndex(ind, indicesToAdd[i], submesh);
                indicesToAdd.RemoveAt(i);
                mesh.ReplaceIndex(ind + 1, indicesToAdd[i], submesh);
                indicesToAdd.RemoveAt(i);
                mesh.ReplaceIndex(ind + 2, indicesToAdd[i], submesh);
                indicesToAdd.RemoveAt(i);
                i -= 3;
            }
            else break;
        }

        //Add triangles
        mesh.AddIndices(indicesToAdd, submesh);
    }

    static void Compute_GetIntersections(ComputableMesh mesh,
        ComputeBuffer edgesDataBuff, ComputeBuffer intersectionsBuff,
        Vector3 planeNormal, Vector3 planePoint, int indexCount)
    {
        cuttingCompute.SetVector("planeNormal", planeNormal);
        cuttingCompute.SetVector("planePoint", planePoint);

        int ki = cuttingCompute.FindKernel(getIntersectionsKernel);
        cuttingCompute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "vertices", mesh.vertexBuffer);
        cuttingCompute.SetBuffer(ki, "edges", edgesDataBuff);
        cuttingCompute.SetBuffer(ki, "intersections", intersectionsBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            indexCount / Computables.Numthreads_Large), 1, 1);
    }

    static void Compute_GetTriangleCutDatas(ComputableMesh mesh,
        ComputeBuffer intersectionsBuff, ComputeBuffer cutsDataBuff, int submesh)
    {
        int indexStart = (int)mesh.GetIndexStart(submesh);
        int indexCount = (int)mesh.GetIndexCount(submesh);

        int ki = cuttingCompute.FindKernel(getTriangleCutDataKernel);
        cuttingCompute.SetInt("indexStart", indexStart);
        cuttingCompute.SetInt("indexCount", indexCount);
        cuttingCompute.SetInt("indexStride", mesh.indexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "indices", mesh.indexBuffer);
        cuttingCompute.SetBuffer(ki, "intersections", intersectionsBuff);
        cuttingCompute.SetBuffer(ki, "cutsData", cutsDataBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Computables.Numthreads_Small), 1, 1);
    }

    static void Compute_GetTriangleCutDatas_SquareCut(ComputableMesh mesh,
        ComputeBuffer intersectionsBuff, ComputeBuffer cutsDataBuff,
        Vector3 upDirection, float size, int submesh)
    {
        int indexStart = (int)mesh.GetIndexStart(submesh);
        int indexCount = (int)mesh.GetIndexCount(submesh);

        cuttingCompute.SetVector("upDirection", upDirection);
        cuttingCompute.SetFloat("size", size);

        int ki = cuttingCompute.FindKernel(getTriangleCutDataSquareKernel);
        cuttingCompute.SetInt("indexStart", indexStart);
        cuttingCompute.SetInt("indexCount", indexCount);
        cuttingCompute.SetInt("indexStride", mesh.indexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "indices", mesh.indexBuffer);
        cuttingCompute.SetBuffer(ki, "intersections", intersectionsBuff);
        cuttingCompute.SetBuffer(ki, "cutsData", cutsDataBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Computables.Numthreads_Small), 1, 1);
    }

    static void Compute_CleanNullAreaTriangles(ComputableMesh mesh, ComputeBuffer intersectionsBuff,
        ComputeBuffer cutsDataBuff, float minArea, int indexCount)
    {
        cuttingCompute.SetFloat("minArea", minArea);

        int ki = cuttingCompute.FindKernel(cleanNullAreaTrianglesInIntersectionKernel);
        cuttingCompute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "vertices", mesh.vertexBuffer);
        cuttingCompute.SetBuffer(ki, "intersections", intersectionsBuff);
        cuttingCompute.SetBuffer(ki, "cutsData", cutsDataBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Computables.Numthreads_Small), 1, 1);
    }
}
