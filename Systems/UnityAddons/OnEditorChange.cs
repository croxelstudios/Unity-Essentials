#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

public static class OnEditorChange
{
    public static void PropertyModification_In(OnPropertyModification action)
    {
        Undo.postprocessModifications -= OnPostprocess;
        Undo.postprocessModifications += OnPostprocess;

        onPropertyModification += action;
    }

    public static void PropertyModification_Out(OnPropertyModification action)
    {
        onPropertyModification -= action;
    }

    public delegate void OnPropertyModification(PropertyModification pm);
    static event OnPropertyModification onPropertyModification;

    static UndoPropertyModification[] OnPostprocess(UndoPropertyModification[] modifications)
    {
        foreach (UndoPropertyModification m in modifications)
        {
            PropertyModification pm = m.currentValue;
            if (!actions.IsNullOrEmpty())
                foreach (KeyValuePair<ModificationData, List<Action>> kvp in actions)
                    if (kvp.Key.Matches(pm))
                        foreach (Action a in kvp.Value)
                            a?.Invoke();
            onPropertyModification?.Invoke(pm);
        }

        return modifications;
    }

    static Dictionary<ModificationData, List<Action>> actions;

    public static void OnEditorChange_In(this Object inObj, string targetPath, Action action, string propertyPath)
    {
        Undo.postprocessModifications -= OnPostprocess;
        Undo.postprocessModifications += OnPostprocess;

        actions = actions.CreateAdd(new ModificationData(targetPath, inObj, propertyPath), action);
    }

    public static void OnEditorChange_Out(this Object inObj, string targetPath, Action action, string propertyPath)
    {
        actions.SmartRemove(new ModificationData(targetPath, inObj, propertyPath), action);
    }

    public static void OnEditorChange_In(this Object inObj, Action action)
    {
        inObj.OnEditorChange_In(null, action, null);
    }

    public static void OnEditorChange_Out(this Object inObj, Action action)
    {
        inObj.OnEditorChange_Out(null, action, null);
    }

    public static void OnEditorChange_In(this Object inObj, string targetPath, Action action)
    {
        inObj.OnEditorChange_In(targetPath, action, null);
    }

    public static void OnEditorChange_Out(this Object inObj, string targetPath, Action action)
    {
        inObj.OnEditorChange_Out(targetPath, action, null);
    }

    public static void OnEditorChange_In(this Object inObj, Action action, string propertyPath)
    {
        inObj.OnEditorChange_In(null, action, propertyPath);
    }

    public static void OnEditorChange_Out(this Object inObj, Action action, string propertyPath)
    {
        inObj.OnEditorChange_Out(null, action, propertyPath);
    }

    struct ModificationData : IEquatable<ModificationData>
    {
        public Object target;
        public string targetPath;
        public string propertyPath;

        public ModificationData(Object target, string propertyPath)
        {
            this.target = target;
            targetPath = "";
            this.propertyPath = propertyPath;
        }

        public ModificationData(Object target)
        {
            this.target = target;
            targetPath = "";
            propertyPath = "";
        }

        public ModificationData(string targetPath, Object target, string propertyPath)
        {
            this.target = target;
            this.targetPath = targetPath;
            this.propertyPath = propertyPath;
        }

        public ModificationData(string targetPath, Object target)
        {
            this.target = target;
            this.targetPath = targetPath;
            propertyPath = "";
        }

        public override bool Equals(object other)
        {
            if (!(other is ModificationData)) return false;
            return Equals((ModificationData)other);
        }

        public bool Equals(ModificationData other)
        {
            return (target == other.target)
                && (targetPath == other.targetPath)
                && (propertyPath == other.propertyPath);
        }

        public override int GetHashCode()
        {
            return HashMaker.Elements(target, targetPath, propertyPath);
        }

        public static bool operator ==(ModificationData o1, ModificationData o2)
        {
            return o1.Equals(o2);
        }

        public static bool operator !=(ModificationData o1, ModificationData o2)
        {
            return !o1.Equals(o2);
        }

        public bool Matches(PropertyModification pm)
        {
            Object[] targets = new Object[] { target };
            if (!targetPath.IsNullOrEmpty())
            {
                targets[0] = ReflectionTools.GetValue<Object>(target, targetPath);
                if (targets[0] == null)
                {
                    int length = ReflectionTools.GetValue<int>(target, targetPath + ".Length");
                    targets = new Object[length];
                    for (int i = 0; i < length; i++)
                        targets[i] =
                            ReflectionTools.GetValue<Object>(target, targetPath + ".Array.data[" + i + "]");
                }
            }

            bool matches = false;
            for (int i = 0; i < targets.Length; i++)
                if (targets[i] == pm.target)
                {
                    matches = true;
                    break;
                }

            if (matches && (!propertyPath.IsNullOrEmpty()))
            {
                string path = propertyPath;
                if (propertyPath[0] == '.')
                    path = ReflectionTools.GetValue<string>(target, propertyPath.Remove(0, 1));
                if (!PathContainsProp(pm.propertyPath, path))
                    matches = false;
            }

            return matches;
        }

        bool PathContainsProp(string a, string b)
        {
            a = a.ToLower().Replace("m_", "");
            b = b.ToLower().Replace("m_", "");
            if (a.Contains(b)) return true;
            else return false;
        }
    }
}
#endif
