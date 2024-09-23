using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class TiledTextureMapper : MonoBehaviour
{
    MeshFilter filter;

#if UNITY_EDITOR
    [SerializeField]
    bool disableInEditor = true;
#endif
    [SerializeField]
    bool applyWorldPosition = false;
    [SerializeField]
    bool applyRotation = false;
    [SerializeField]
    bool applyScale = true;

    Mesh originalMesh;
    [SerializeField]
    [HideInInspector]
    Mesh modifiedMesh;
    TransformData prev;

    void OnEnable()
    {
        filter = GetComponent<MeshFilter>();
        prev = new TransformData(transform);
        UpdateMesh();
    }

    void Update()
    {
        if (CheckChanges())
            UpdateMesh();
    }

    bool CheckChanges()
    {
        if (applyScale && (prev.lossyScale != transform.lossyScale)) return true;
        if (applyRotation && (prev.rotation != transform.rotation)) return true;
        if (applyWorldPosition && (prev.position != transform.position)) return true;
        if ((filter.sharedMesh != originalMesh) && (filter.sharedMesh != modifiedMesh)) return true;
        return false;
    }

    void OnWillRenderObject()
    {
#if UNITY_EDITOR
        if ((!disableInEditor) || Application.isPlaying)
#endif
            filter.sharedMesh = modifiedMesh;
    }

    void OnRenderObject()
    {
#if UNITY_EDITOR
        if ((!disableInEditor) || Application.isPlaying)
#endif
            filter.sharedMesh = originalMesh;
    }

    void UpdateMesh()
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

    [Button("Update Mesh")]
    public void _UpdateMesh()
    {
        UpdateMesh();
        filter.sharedMesh = modifiedMesh;
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
        newMesh.name = "New_" + mesh.name;
        return newMesh;
    }
}
