using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

using UnityEngine;

namespace CleverCrow.Fluid.UniqueIds
{
#if UNITY_EDITOR
    [CustomEditor(typeof(UniqueId), true)]
    public class UniqueIdEditor : Editor
    {
        bool IsPrefabInstance =>
            PrefabUtility.GetPrefabInstanceStatus(target) != PrefabInstanceStatus.NotAPrefab;
        private bool IsPrefabStage =>
            UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null;
        bool IsPrefab =>
            PrefabUtility.GetPrefabAssetType(target) !=
            PrefabAssetType.NotAPrefab && !IsPrefabInstance || IsPrefabStage;

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI();

            var idProp = serializedObject.FindProperty("_id");
            var id = idProp.stringValue;

            GUI.enabled = false;
            EditorGUILayout.TextField("id", id);
            GUI.enabled = true;

            if (!IsPrefab)
            {
                EditForm(idProp);
            }

            if (IsPrefab)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    idProp.stringValue = null;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void EditForm (SerializedProperty idProp)
        {
            if (string.IsNullOrEmpty(idProp.stringValue) ||
                UniqueIdTracker.ids.Exists(x => (x.Id == idProp.stringValue) &&
                (x != (serializedObject.targetObject as UniqueId))))
            {
                idProp.stringValue = GetGUID();
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Copy ID"))
            {
                var textEditor = new TextEditor {text = idProp.stringValue};
                textEditor.SelectAll();
                textEditor.Copy();
            }

            if (GUILayout.Button("Randomize ID")
                && EditorUtility.DisplayDialog("Randomize ID", "Are you sure, this can break existing save data", "Randomize",
                    "Cancel"))
            {
                idProp.stringValue = GetGUID();
                serializedObject.ApplyModifiedProperties();
            }
        }

        private static string GetGUID ()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public static class UniqueIdTracker
    {
        public static List<UniqueId> ids;

        static UniqueIdTracker()
        {
            RefreshCurrentIds();
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        static void RefreshCurrentIds()
        {
            if (ids == null)
                ids = new List<UniqueId>();
            else ids.Clear();
            ids.AddRange(GameObject.FindObjectsByType<UniqueId>(FindObjectsSortMode.None));
        }

        static void OnHierarchyChanged()
        {
            RefreshCurrentIds();
        }
    }
#endif
}
