using System;
using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
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
    protected DXEvent beforeCall = null;
    [FoldoutGroup("Standard Calls")]
    protected DXEvent called = null;

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

        activeSignals = activeSignals.CreateAdd(type, this);
    }

    protected virtual void OnUnload()
    {
        if (type == null) type = GetType();
        activeSignals.SmartRemove(type, this);
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
        listeners = listeners.CreateAdd(new Action(listener, index));
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

    public void AddListener(UnityAction action, bool beforeCall = false)
    {
        if (beforeCall) this.beforeCall = this.beforeCall.CreateAddListener(action);
        else called = called.CreateAddListener(action);
    }

    public void RemoveListener(UnityAction action)
    {
        beforeCall.SmartRemoveListener(action);
        called.SmartRemoveListener(action);
    }
}

public interface IValueSignal
{
    //Base
    public string name { get; }
    public string popupName { get; }
    public bool dynamicSearch { get; }
    public void OnEnable();
    public void OnDisable();
    public void ActivateDynamicSearch();
    public void DeactivateDynamicSearch();
    public void AddAction(BBaseSignalListener listener, int index);
    public void RemoveAction(BBaseSignalListener receiver, int index);
    public void Reset();
    public void AddListener(UnityAction action, bool beforeCall = false);
    //

    public void SetValueParse(string value);

    public string GetStringValue();
}

public class ValueSignal<T> : BaseSignal, IValueSignal
{
    [SerializeField]
    bool resetValueOnStart = true;
    [SerializeField]
    [ShowIf("MustShowStartValue")]
    protected T startValue = default;
    [HideIf("MustShowStartValue")]
    [OnValueChanged("CallSignalOnCurrentTagAndValues")]
    public T currentValue = default;

    protected virtual void SetValue(T value)
    {
#if UNITY_EDITOR
        if (MustShowStartValue())
            startValue = value;
#endif
        currentValue = value;
    }

    public void SetValueParse(string value)
    {
#if UNITY_EDITOR
        if (MustShowStartValue())
            startValue = value.Replace("|;&", ",").Parse<T>();
#endif
        CallSignal(value.Replace("|;&", ",").Parse<T>());
    }

    public virtual string GetStringValue()
    {
#if UNITY_EDITOR
        if (MustShowStartValue())
            return startValue.ToString().Replace(",", "|;&");
#endif
        return currentValue.ToString().Replace(",", "|;&");
    }

    [TagSelector]
    public virtual void CallSignal(T value, string tag = "")
    {
        OnEnable();
        if (IsDifferentFromCurrent(value))
        {
            SetValue(value);
            beforeCall?.Invoke();
            if (dynamicSearch) DynamicSearch<BBaseSignalListener<T>>();
            if (listeners != null)
            {
                T finalValue = Calculate();
                for (int i = (listeners.Count - 1); i >= 0; i--)
                {
                    if ((tag == "") || (tag == listeners[i].receiver.tag))
                        LaunchActions(((BBaseSignalListener<T>)listeners[i].receiver), listeners[i].index, finalValue);
                }
            }
            if (dynamicSearch) listeners = null;
            called?.Invoke();
        }
    }

    public void CallSignal(T value, IEnumerable<GameObject> objects)
    {
        CallSignal<GameObject>(value, objects);
    }

    public void CallSignal(T value, IEnumerable<Transform> transforms)
    {
        CallSignal<Transform>(value, transforms);
    }

    protected virtual void CallSignal<O>(T value, IEnumerable<O> objects) where O : Object
    {
        OnEnable();
        if (IsDifferentFromCurrent(value))
        {
            SetValue(value);
            beforeCall?.Invoke();
            if (dynamicSearch) DynamicSearch<BBaseSignalListener<T>>();
            if (listeners != null)
            {
                T finalValue = Calculate();
                for (int i = (listeners.Count - 1); i >= 0; i--)
                {
                    bool isChild = false;
                    foreach (O obj in objects)
                    {
                        Transform tr = obj.GetTransform();
                        if (listeners[i].receiver.transform.IsChildOf(tr))
                        {
                            isChild = true;
                            break;
                        }
                    }
                    if (isChild)
                        LaunchActions(((BBaseSignalListener<T>)listeners[i].receiver), listeners[i].index, finalValue);
                }
            }
            if (dynamicSearch) listeners = null;
            called?.Invoke();
        }
    }

    void LaunchActions(BBaseSignalListener<T> listener, int index, T value)
    {
        listener.LaunchActions(index, value);
    }

    public void CallSignal(T value)
    {
        CallSignal(value, "");
    }

    public static void Set(ValueSignal<T> signal, T value)
    {
#if UNITY_EDITOR
        if (signal.MustShowStartValue())
            signal.startValue = value;
#endif
        signal.CallSignal(value);
    }

    protected virtual T Calculate()
    {
        return currentValue;
    }

    protected virtual bool IsDifferentFromCurrent(T value)
    {
        return !value.Equals(currentValue);
    }

#if UNITY_EDITOR
    public bool MustShowStartValue()
    {
        return resetValueOnStart && !Application.isPlaying;
    }

    public void CallSignalOnCurrentTagAndValues()
    {
        if (Application.isPlaying)
            CallSignal(currentValue, currentTag);
    }
#endif

    protected override void OnLoad()
    {
        if (resetValueOnStart)
        {
            SetValue(startValue);
            AfterReset();
        }
        base.OnLoad();
    }

    public override void Reset()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            SetValue(startValue);
        else
#endif
            CallSignal(startValue);

        AfterReset();
    }

    protected virtual void AfterReset()
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
    }

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
        t = t.Replace("PPtr<$", "").Replace(">", "");
        if (typeof(BaseSignal).Name == t)
            EditorGUI.PropertyField(position, property);
        else
            property.objectReferenceValue = BaseSignalManager.DrawSignalNames(Type.GetType(t),
                property.objectReferenceValue, position, label, false);
        EditorGUI.EndProperty();
    }
}
#endif
