using UnityEngine;
using System.Collections.Generic;

public static class ColliderExtension_GetMesh //TO DO: Should generate a ComputableMesh.
{
    public static Mesh GetMesh(this Collider collider, bool applyWorldTransform = false)
    {
        if (collider == null)
            return null;

        Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
        Mesh result;
        switch (collider)
        {
            case MeshCollider meshCollider:
                if (applyWorldTransform)
                    result = CloneMesh(meshCollider.sharedMesh);
                else result = meshCollider.sharedMesh;
                break;
            case BoxCollider boxCollider:
                result = BuildBoxMesh(boxCollider);
                break;
            case SphereCollider sphereCollider:
                result = BuildSphereMesh(sphereCollider, 12, 18);
                break;
            case CapsuleCollider capsuleCollider:
                result = BuildCapsuleMesh(capsuleCollider, 12, 18);
                break;
            default:
                // TerrainCollider and custom colliders require bespoke baking.
                result = null;
                break;
        }

        if ((result != null) && applyWorldTransform)
            result.ApplyMatrix(localToWorld);

        return result;
    }

    static Mesh CloneMesh(Mesh source)
    {
        if (source == null)
            return null;

        Mesh mesh = Object.Instantiate(source);
        mesh.name = $"WorldSpace_{source.name}";
        return mesh;
    }

    static Mesh BuildBoxMesh(BoxCollider box)
    {
        Vector3 center = box.center;
        Vector3 size = box.size * 0.5f;

        Vector3[] v =
        {
            center + new Vector3(-size.x, -size.y, -size.z),
            center + new Vector3( size.x, -size.y, -size.z),
            center + new Vector3( size.x, -size.y,  size.z),
            center + new Vector3(-size.x, -size.y,  size.z),
            center + new Vector3(-size.x,  size.y, -size.z),
            center + new Vector3( size.x,  size.y, -size.z),
            center + new Vector3( size.x,  size.y,  size.z),
            center + new Vector3(-size.x,  size.y,  size.z),
        };

        int[] t =
        {
            0,2,1, 0,3,2,
            4,5,6, 4,6,7,
            0,1,5, 0,5,4,
            1,2,6, 1,6,5,
            2,3,7, 2,7,6,
            3,0,4, 3,4,7
        };

        return BuildMesh(v, t);
    }

    static Mesh BuildSphereMesh(SphereCollider sphere, int latitudeSegments, int longitudeSegments)
    {
        int vertCount = (latitudeSegments + 1) * (longitudeSegments + 1);
        var vertices = new Vector3[vertCount];
        var triangles = new List<int>(latitudeSegments * longitudeSegments * 6);
        float radius = sphere.radius;
        Vector3 center = sphere.center;

        int index = 0;
        for (int lat = 0; lat <= latitudeSegments; lat++)
        {
            float a1 = Mathf.PI * lat / latitudeSegments;
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= longitudeSegments; lon++)
            {
                float a2 = 2f * Mathf.PI * lon / longitudeSegments;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                vertices[index++] = center + new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }

        for (int lat = 0; lat < latitudeSegments; lat++)
        {
            for (int lon = 0; lon < longitudeSegments; lon++)
            {
                int current = lat * (longitudeSegments + 1) + lon;
                int next = current + longitudeSegments + 1;

                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + 1);

                triangles.Add(current + 1);
                triangles.Add(next);
                triangles.Add(next + 1);
            }
        }

        return BuildMesh(vertices, triangles.ToArray());
    }

    static Mesh BuildCapsuleMesh(CapsuleCollider capsule, int radialSegments, int heightSegments)
    {
        // Simple lat-long capsule approximation, oriented along the collider direction.
        // This is adequate as a baking primitive; refine as needed for your boolean backend.
        radialSegments = Mathf.Max(6, radialSegments);
        heightSegments = Mathf.Max(1, heightSegments);

        float radius = capsule.radius;
        float height = Mathf.Max(capsule.height, radius * 2f);
        float cylinderHeight = Mathf.Max(0f, height - 2f * radius);
        Vector3 center = capsule.center;

        int rings = (heightSegments * 2) + 2;
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        for (int ring = 0; ring <= rings; ring++)
        {
            float t = ring / (float)rings;
            float y;
            float r;

            if (t < 0.5f)
            {
                float u = t * 2f;
                float theta = Mathf.Lerp(0f, Mathf.PI * 0.5f, u);
                y = -cylinderHeight * 0.5f - Mathf.Cos(theta) * radius;
                r = Mathf.Sin(theta) * radius;
            }
            else
            {
                float u = (t - 0.5f) * 2f;
                float theta = Mathf.Lerp(Mathf.PI * 0.5f, Mathf.PI, u);
                y = cylinderHeight * 0.5f - Mathf.Cos(theta) * radius;
                r = Mathf.Sin(theta) * radius;
            }

            for (int i = 0; i <= radialSegments; i++)
            {
                float a = (2f * Mathf.PI * i) / radialSegments;
                vertices.Add(center + new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r));
            }
        }

        int stride = radialSegments + 1;
        for (int ring = 0; ring < rings; ring++)
        {
            for (int i = 0; i < radialSegments; i++)
            {
                int a = ring * stride + i;
                int b = a + stride;

                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(a + 1);

                triangles.Add(a + 1);
                triangles.Add(b);
                triangles.Add(b + 1);
            }
        }

        Mesh mesh = BuildMesh(vertices.ToArray(), triangles.ToArray());

        // Rotate from Y axis to capsule direction.
        Quaternion rotation = capsule.direction switch
        {
            0 => Quaternion.Euler(0f, 0f, -90f),
            1 => Quaternion.identity,
            2 => Quaternion.Euler(90f, 0f, 0f),
            _ => Quaternion.identity
        };

        mesh.ApplyMatrix(Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one));
        return mesh;
    }

    static Mesh BuildMesh(Vector3[] vertices, int[] triangles)
    {
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
