using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[CustomEditor(typeof(MeshFilter)), CanEditMultipleObjects]
public class MeshFilterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty mesh = serializedObject.FindProperty("m_Mesh");

        if (mesh != null)
            EditorGUILayout.PropertyField(mesh);

        Mesh m = (Mesh)mesh.objectReferenceValue;

        if (m != null)
        {
            string dontcare = null;
            z_ModelSource source = GetMeshGUID(m, ref dontcare);

            if (source == z_ModelSource.Scene &&
                !(ReflectionUtil_Pb.IsProBuilderObject(((MeshFilter)serializedObject.targetObject).gameObject)))
            {
                if (GUILayout.Button(new GUIContent("Save to Asset", "Save this instance mesh to an Asset so that you can use it as a prefab.")))
                {
                    SaveMeshAsset(m);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    /**
     *	Describes the origin of a mesh.
     */
    public enum z_ModelSource
    {
        Imported = 0x0,
        Asset = 0x1,
        Scene = 0x2,
        AdditionalVertexStreams = 0x3
    }

    /**
     *	Return the mesh source, and the guid if applicable (scene instances don't get GUIDs).
     */
    public static z_ModelSource GetMeshGUID(Mesh mesh, ref string guid)
    {
        string path = AssetDatabase.GetAssetPath(mesh);

        if (path != "")
        {
            AssetImporter assetImporter = AssetImporter.GetAtPath(path);

            if (assetImporter != null)
            {
                // Only imported model (e.g. FBX) assets use the ModelImporter,
                // where a saved asset will have an AssetImporter but *not* ModelImporter.
                // A procedural mesh (one only existing in a scene) will not have any.
                if (assetImporter is ModelImporter)
                {
                    guid = AssetDatabase.AssetPathToGUID(path);
                    return z_ModelSource.Imported;
                }
                else
                {
                    guid = AssetDatabase.AssetPathToGUID(path);
                    return z_ModelSource.Asset;
                }
            }
            else
            {
                return z_ModelSource.Scene;
            }
        }

        return z_ModelSource.Scene;
    }

    const int DIALOG_OK = 0;
    const int DIALOG_ALT = 2;
    const string DO_NOT_SAVE = "DO_NOT_SAVE";

    /**
     *	Save any modifications to the z_EditableObject.  If the mesh is a scene mesh or imported mesh, it
     *	will be saved to a new asset.  If the mesh was originally an asset mesh, the asset is overwritten.
     * 	\return true if save was successful, false if user-canceled or otherwise failed.
     */
    public static bool SaveMeshAsset(Mesh mesh, MeshFilter meshFilter = null, SkinnedMeshRenderer skinnedMeshRenderer = null)
    {
        string save_path = DO_NOT_SAVE;

        string guid = null;
        z_ModelSource source = GetMeshGUID(mesh, ref guid);

        switch (source)
        {
            case z_ModelSource.Asset:

                int saveChanges = EditorUtility.DisplayDialogComplex(
                    "Save Changes",
                    "Save changes to edited mesh?",
                    "Save",             // DIALOG_OK
                    "Cancel",           // DIALOG_CANCEL
                    "Save As");         // DIALOG_ALT

                if (saveChanges == DIALOG_OK)
                    save_path = AssetDatabase.GetAssetPath(mesh);
                else if (saveChanges == DIALOG_ALT)
                    save_path = EditorUtility.SaveFilePanelInProject("Save Mesh As", mesh.name + ".asset", "asset", "Save edited mesh to");
                else
                    return false;

                break;

            case z_ModelSource.Imported:
            case z_ModelSource.Scene:
            default:
                // @todo make sure path is in Assets/
                save_path = EditorUtility.SaveFilePanelInProject("Save Mesh As", mesh.name + ".asset", "asset", "Save edited mesh to");
                break;
        }

        if (!save_path.Equals(DO_NOT_SAVE) && !string.IsNullOrEmpty(save_path))
        {
            Object existing = AssetDatabase.LoadMainAssetAtPath(save_path);

            if (existing != null && existing is Mesh)
            {
                // save over an existing mesh asset
                Copy((Mesh)existing, mesh);
                DestroyImmediate(mesh);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, save_path);
            }

            AssetDatabase.Refresh();

            if (meshFilter != null)
                meshFilter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(save_path, typeof(Mesh));
            else if (skinnedMeshRenderer != null)
                skinnedMeshRenderer.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(save_path, typeof(Mesh));

            return true;
        }

        // Save was canceled
        return false;
    }

    /**
     *	Copy @src mesh values to @dest
     */
    public static void Copy(Mesh dest, Mesh src)
    {
        dest.Clear();
        dest.vertices = src.vertices;

        List<Vector4> uvs = new List<Vector4>();

        src.GetUVs(0, uvs); dest.SetUVs(0, uvs);
        src.GetUVs(1, uvs); dest.SetUVs(1, uvs);
        src.GetUVs(2, uvs); dest.SetUVs(2, uvs);
        src.GetUVs(3, uvs); dest.SetUVs(3, uvs);

        dest.normals = src.normals;
        dest.tangents = src.tangents;
        dest.boneWeights = src.boneWeights;
        dest.colors = src.colors;
        dest.colors32 = src.colors32;
        dest.bindposes = src.bindposes;

        dest.subMeshCount = src.subMeshCount;

        for (int i = 0; i < src.subMeshCount; i++)
            dest.SetIndices(src.GetIndices(i), src.GetTopology(i), i);

        dest.name = IncrementPrefix("z", src.name);
    }

    /**
     *	Returns a new name with incremented prefix.
     */
    internal static string IncrementPrefix(string prefix, string name)
    {
        string str = name;

        Regex regex = new Regex("^(" + prefix + "[0-9]*_)");
        Match match = regex.Match(name);

        if (match.Success)
        {
            string iteration = match.Value.Replace(prefix, "").Replace("_", "");
            int val = 0;

            if (int.TryParse(iteration, out val))
            {
                str = name.Replace(match.Value, prefix + (val + 1) + "_");
            }
            else
                str = prefix + "0_" + name;
        }
        else
        {
            str = prefix + "0_" + name;
        }

        return str;
    }
}
