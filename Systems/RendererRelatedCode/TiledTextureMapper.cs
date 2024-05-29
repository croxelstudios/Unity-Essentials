using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class TiledTextureMapper : MonoBehaviour
{
    MeshFilter filter;

    [SerializeField]
    bool applyWorldPosition = false;
    [SerializeField]
    bool applyRotation = false;
    [SerializeField]
    bool applyScale = true;
    [SerializeField]
    bool update = false;

    Mesh originalMesh;
    Mesh modifiedMesh;

    void OnEnable()
    {
        filter = GetComponent<MeshFilter>();
        UpdateMeshes();
    }

    void Update()
    {
        if (update) UpdateMeshes();
        else if (filter.sharedMesh != originalMesh) UpdateMeshes();
    }

    void OnWillRenderObject()
    {
        filter.sharedMesh = modifiedMesh;
    }

    void OnRenderObject()
    {
        filter.sharedMesh = originalMesh;
    }

    void UpdateMeshes()
    {
        DestroyImmediate(modifiedMesh);
        if (filter.sharedMesh != null) originalMesh = filter.sharedMesh;
        modifiedMesh = CopyMesh(originalMesh);

        Vector2[] uv = modifiedMesh.uv;
        for (int i = 0; i < uv.Length; i++)
        {
            Quaternion quat = Quaternion.FromToRotation(modifiedMesh.normals[i], Vector3.forward);
            Vector3 vertexPosition = modifiedMesh.vertices[i];
            if (applyScale) vertexPosition = Vector3.Scale(vertexPosition, transform.lossyScale);
            if (applyRotation) vertexPosition = transform.rotation * vertexPosition;
            if (applyWorldPosition) vertexPosition += transform.position;
            uv[i] = (quat * vertexPosition);
            uv[i] = new Vector2(uv[i].x, -uv[i].y);
            //Y is reversed due to unity's weird reference system
        }
        modifiedMesh.uv = uv;
    }

    Mesh CopyMesh(Mesh mesh)
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.uv = mesh.uv;
        newMesh.normals = mesh.normals;
        newMesh.colors = mesh.colors;
        newMesh.tangents = mesh.tangents;
        return newMesh;
    }
}
