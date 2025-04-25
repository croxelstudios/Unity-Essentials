using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System.IO;
using Object = UnityEngine.Object;
#endif

public class BaseSignal : ScriptableObject
{
    public string popupName { get { return name.Replace('_', '/'); } }
    [SerializeField]
    bool _dynamicSearch = false;
    public bool dynamicSearch { get { return _dynamicSearch; } }
    [SerializeField]
    protected string currentTag = "";
    [FoldoutGroup("Standard Calls")]
    public DXEvent beforeCall = null;
    [FoldoutGroup("Standard Calls")]
    public DXEvent called = null;

    bool enabled;
    public static Dictionary<Type, List<BaseSignal>> activeSignals;
    protected List<Action> listeners;
    Type type;

    public static event Action<Type, BaseSignal> OnEnableCallback;

    public void OnEnable()
    {
        if (!enabled)
        {
            OnLoad();
            enabled = true;
        }
    }

    public void OnDisable()
    {
        if (enabled)
        {
            OnUnload();
            enabled = false;
        }
    }

    protected virtual void OnLoad()
    {
        if (type == null) type = GetType();

        OnEnableCallback?.Invoke(type, this);

        if (activeSignals == null) activeSignals = new Dictionary<Type, List<BaseSignal>>();
        if (!activeSignals.ContainsKey(type)) activeSignals.Add(type, new List<BaseSignal>());
        activeSignals[type].Add(this);
    }

    protected virtual void OnUnload()
    {
        if (type == null) type = GetType();
        if ((activeSignals != null) && activeSignals.ContainsKey(type))
            activeSignals[type].Remove(this);
    }

    [ContextMenu("Set as dynamic")]
    public void ActivateDynamicSearch()
    {
        _dynamicSearch = true;
    }

    [ContextMenu("Set as static")]
    public void DeactivateDynamicSearch()
    {
        _dynamicSearch = false;
    }

    /// <summary>
    /// Looks for every listener in the scene and updates the actions list
    /// </summary>
    /// <param name="type">Subtype of BaseSignalListener to account for</param>
    protected void DynamicSearch(Type type)
    {
        listeners = new List<Action>();
        BBaseSignalListener[] allListeners =
            FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.None) as BBaseSignalListener[];
        foreach (BBaseSignalListener listener in allListeners)
        {
            listener.UpdateSignals();
            for (int i = 0; i < listener.signals.Length; i++)
                if (listener.signals[i] == this)
                    listeners.Add(new Action(listener, i));
        }
    }

    protected void DynamicSearch<T>() where T : BBaseSignalListener
    {
        listeners = new List<Action>();
        BBaseSignalListener[] allListeners = FindObjectsByType<T>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (BBaseSignalListener listener in allListeners)
        {
            listener.UpdateSignals();
            for (int i = 0; i < listener.signals.Length; i++)
                if (listener.signals[i] == this)
                    listeners.Add(new Action(listener, i));
        }
    }

    public void AddAction(BBaseSignalListener listener, int index)
    {
        if (listeners == null) listeners = new List<Action>();
        Action newAction = new Action(listener, index);
        listeners.Add(newAction);
    }

    public void RemoveAction(BBaseSignalListener receiver, int index)
    {
        if (listeners != null)
        {
            Action toRemove = new Action(receiver, index);
            listeners.Remove(toRemove);
        }
    }

    public struct Action
    {
        public BBaseSignalListener receiver;
        public int index;

        public Action(BBaseSignalListener receiver, int index)
        {
            this.receiver = receiver;
            this.index = index;
        }
    }

    public static void ResetAllSignals()
    {
        foreach (KeyValuePair<Type, List<BaseSignal>> pair in activeSignals)
        {
            List<BaseSignal> signals = pair.Value;
            for (int i = 0; i < signals.Count; i++)
                signals[i].Reset();
        }
    }

    public static T[] GetFromSubstring<T>(string substring, T[] exceptions = null)
        where T : BaseSignal
    {
        List<T> signals = new List<T>();
        if (typeof(T) == typeof(BaseSignal))
            foreach (KeyValuePair<Type, List<BaseSignal>> kvp in activeSignals)
                for (int i = 0; i < kvp.Value.Count; i++)
                    signals.Add(kvp.Value[i] as T);
        else signals.AddRange(Array.ConvertAll(activeSignals[typeof(T)].ToArray(), item => (T)item));

        for (int i = signals.Count - 1; i >= 0; i--)
            if (((exceptions != null) && exceptions.Contains(signals[i])) ||
                (signals[i] == null) || !signals[i].name.Contains(substring))
                signals.RemoveAt(i);

        return signals.ToArray();
    }

    public virtual void Reset()
    {

    }
}

#if UNITY_EDITOR
class BaseSignalManager : IDisposable
{
    static BaseSignalManager m_Instance;
    readonly Dictionary<Type, List<BaseSignal>> m_assets = new Dictionary<Type, List<BaseSignal>>();

    BaseSignalManager()
    {
        BaseSignal.OnEnableCallback += Register;
    }

    static BaseSignalManager instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new BaseSignalManager();
                m_Instance.Refresh(); //TO DO: Could this system be used to solve the StringPupupData problems?
            }

            return m_Instance;
        }

        set { m_Instance = value; }
    }

    protected static IEnumerable<BaseSignal> GetAssets(Type type)
    {
        if (!instance.m_assets.ContainsKey(type))
            instance.m_assets.Add(type, new List<BaseSignal>());
        foreach (BaseSignal asset in instance.m_assets[type])
        {
            if (asset != null)
                yield return asset;
        }
    } //TO DO: Should be generic static function, outside this

    public static BaseSignal CreateSignalAssetInstance(Type type, string path)
    {
        BaseSignal newSignal = ScriptableObject.CreateInstance(type) as BaseSignal;
        newSignal.name = Path.GetFileNameWithoutExtension(path);

        BaseSignal asset = AssetDatabase.LoadMainAssetAtPath(path) as BaseSignal;
        if (asset != null)
        {
            EditorUtility.CopySerialized(newSignal, asset);
            Object.DestroyImmediate(newSignal);
            return asset;
        }

        AssetDatabase.CreateAsset(newSignal, path);
        return newSignal;
    }

    public void Dispose()
    {
        BaseSignal.OnEnableCallback -= Register;
    }

    void Register(Type type, BaseSignal a)
    {
        if (!m_assets.ContainsKey(type))
            m_assets.Add(type, new List<BaseSignal>());
        m_assets[type].Add(a);
    }

    void Refresh()
    {
        string[] guids = AssetDatabase.FindAssets("t:BaseSignal");
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            BaseSignal asset = AssetDatabase.LoadAssetAtPath<BaseSignal>(path);
            Register(asset.GetType(), asset);
        }
    }

    public static Object DrawSignalNames(Type type, Object currentObjectReferenceValue, Rect position, GUIContent label, bool multipleValues)
    {
        BaseSignal signalAsset = (BaseSignal)currentObjectReferenceValue;
        List<BaseSignal> assets = GetAssets(type).ToList();
        int index = assets.IndexOf(signalAsset);

        List<GUIContent> availableNames = new List<GUIContent>();
        using (new GUIMixedValueScope(multipleValues))
        {
            availableNames.Add(new GUIContent("None"));

            availableNames.AddRange(assets.Select(x => new GUIContent(x.popupName)));
            availableNames.Add(new GUIContent("Create…"));

            int curValue = index + 1;
            int selected = EditorGUI.Popup(position, label, curValue, availableNames.ToArray());

            if (selected != curValue)
            {
                int noneEntryIdx = 0;
                if (selected == noneEntryIdx) // None
                    signalAsset = null;
                else if (selected == availableNames.Count - 1) // "Create New Asset"
                {
                    string path = GetNewSignalPath(type.Name);
                    if (!string.IsNullOrEmpty(path))
                        signalAsset = CreateSignalAssetInstance(type, path); //TO DO: It doesn't auto select it. 
                                                                             //Maybe it doesn't register it properly until next frame?
                    GUIUtility.ExitGUI();
                }
                else
                    signalAsset = assets[selected - 1];
                return signalAsset;
            }
        }
        return currentObjectReferenceValue;
    }

    public static string GetNewSignalPath(string type)
    {
        return EditorUtility.SaveFilePanelInProject(
            "Create " + type,
            "New " + type,
            "asset",
            "Create " + type);
    }

    struct GUIMixedValueScope : IDisposable
    {
        readonly bool m_PrevValue;
        public GUIMixedValueScope(bool newValue)
        {
            m_PrevValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = newValue;
        }

        public void Dispose()
        {
            EditorGUI.showMixedValue = m_PrevValue;
        }
    }
}

[CustomPropertyDrawer(typeof(BaseSignal))]
public class BaseSignalPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        string t = property.type;
        property.objectReferenceValue = BaseSignalManager.DrawSignalNames(Type.GetType(property.type.Replace("PPtr<$", "").Replace(">", "")),
            property.objectReferenceValue, position, label, false);
        EditorGUI.EndProperty();
    }
}
#endif
