using System.Collections.Generic;
using UnityEngine;

public class Renderizable
{
    static Dictionary<GameObject, Renderizable> renderizables = null;
    static bool wasCleaned;

    //Types
    public RenType renType;
    public RendType rendType; 
    public MeshFilter filter;
    public Renderer renderer;
    public CustomRenderer customRenderer;
    public object nullableObject
    {
        get
        {
            switch (renType)
            {
                case RenType.Filter:
                    return filter;
                case RenType.Sprite:
                    return renderer;
                case RenType.Custom:
                    return customRenderer;
                default:
                    return renderer;
            }
        }
    }

    //Obj data
    public Transform transform;
    public GameObject gameObject;
    public bool enabled
    {
        get
        {
            switch (rendType)
            {
                case RendType.Custom:
                    return (customRenderer != null) && customRenderer.enabled;
                default:
                    return (renderer != null) && renderer.enabled;
            }
        }
        set
        {
            switch (rendType)
            {
                case RendType.Custom:
                    if (customRenderer != null) customRenderer.enabled = value;
                    break;
                default:
                    if (renderer != null) renderer.enabled = value;
                    break;
            }
        }
    }

    //Nullable
    FrameNullTracker isNullTracker;
    public bool isNull { get { isNullTracker.obj = nullableObject; return isNullTracker.IsNull(); } }

    //Visibility
    public bool internalIsVisible { get { isVisibleTracker.rend = renderer; return isVisibleTracker.Get(); } }
    IsVisibleTracker isVisibleTracker;

    //Material access
    public Material[] sharedMaterials
    {
        get
        {
            switch (rendType)
            {
                case RendType.Custom:
                    return (customRenderer != null) ?
                        customRenderer.sharedMaterials : null;
                default:
                    return (renderer != null) ?
                        renderer.sharedMaterials : null;
            }
        }
        set
        {
            switch (rendType)
            {
                case RendType.Custom:
                    if (customRenderer != null)
                        customRenderer.sharedMaterials = value;
                    break;
                default:
                    if (renderer != null)
                        renderer.sharedMaterials = value;
                    break;
            }
        }
    }
    public Material[] materials
    {
        get
        {
            switch (rendType)
            {
                case RendType.Custom:
                    return (customRenderer != null) ?
                        customRenderer.instMaterials : null;
                default:
                    return (renderer != null) ?
                        renderer.materials : null;
            }
        }
        set
        {
            switch (rendType)
            {
                case RendType.Custom:
                    if (customRenderer != null)
                        customRenderer.instMaterials = value;
                    break;
                default:
                    if (renderer != null)
                        renderer.materials = value;
                    break;
            }
        }
    }

    static List<GameObject> auxGOs;

    public Renderizable(MeshFilter filter)
    {
        this.filter = filter;
        gameObject = filter.gameObject;
        renderer = gameObject.GetComponent<MeshRenderer>();
        customRenderer = null;
        transform = null;
        renType = RenType.Filter;
        rendType = RendType.Renderer;
        isVisibleTracker = new IsVisibleTracker(renderer);
        isNullTracker = new FrameNullTracker();

        if (filter != null)
            transform = filter.transform;

        isNullTracker = new FrameNullTracker(nullableObject);

        TryAddDictionary();
    }

    public Renderizable(Renderer renderer)
    {
        filter = renderer.GetFilter();
        this.renderer = renderer;
        gameObject = renderer.gameObject;
        customRenderer = null;
        transform = null;
        renType = (renderer is SpriteRenderer) ? RenType.Sprite : RenType.Renderer;
        rendType = RendType.Renderer;
        isVisibleTracker = new IsVisibleTracker(renderer);
        isNullTracker = new FrameNullTracker();

        if (renderer != null)
            transform = renderer.transform;

        isNullTracker = new FrameNullTracker(nullableObject);

        TryAddDictionary();
    }

    public Renderizable(CustomRenderer customRenderer)
    {
        filter = null;
        renderer = null;
        this.customRenderer = customRenderer;
        gameObject = customRenderer.gameObject;
        transform = null;
        renType = RenType.Custom;
        rendType = RendType.Custom;
        isVisibleTracker = new IsVisibleTracker(renderer);
        isNullTracker = new FrameNullTracker();

        if (customRenderer != null)
            transform = customRenderer.transform;

        isNullTracker = new FrameNullTracker(nullableObject);

        TryAddDictionary();
    }

    public Renderizable(GameObject gameObject)
    {
        this.gameObject = gameObject;
        filter = gameObject.GetComponent<MeshFilter>();
        renderer = null;
        customRenderer = null;
        transform = gameObject.transform;
        isVisibleTracker = new IsVisibleTracker(renderer);
        isNullTracker = new FrameNullTracker();

        if (filter != null)
        {
            renderer = gameObject.GetComponent<MeshRenderer>();
            renType = RenType.Filter;
            rendType = RendType.Renderer;
        }
        else
        {
            renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                customRenderer = gameObject.GetComponent<CustomRenderer>();
                renType = RenType.Custom;
                rendType = RendType.Custom;
            }
            else
            {
                renType = (renderer is SpriteRenderer) ? RenType.Sprite : RenType.Renderer;
                rendType = RendType.Renderer;
            }
        }

        isNullTracker = new FrameNullTracker(nullableObject);

        TryAddDictionary();
    }

    void TryAddDictionary()
    {
        //if (!isNull)
        {
            renderizables = renderizables.CreateIfNull_StaticPersistent();
            renderizables.Set(gameObject, isNull ? null : this);
        }
    }

    public Matrix4x4 LocalToWorldMatrix(int id = 0)
    {
        return (renType != RenType.Custom) ? transform.localToWorldMatrix :
            customRenderer.LocalToWorldMatrix(id);
    }

    public Matrix4x4 WorldToLocalMatrix(int id = 0)
    {
        return (renType != RenType.Custom) ? transform.worldToLocalMatrix :
            customRenderer.WorldToLocalMatrix(id);
    }

    public void GetPropertyBlock(ref MaterialPropertyBlock block, int material)
    {
        switch (rendType)
        {
            case RendType.Custom:
                if (customRenderer != null)
                    customRenderer.GetPropertyBlock(ref block, material);
                break;
            default:
                if (renderer != null)
                    renderer.GetPropertyBlock(block, material);
                break;
        }
    }

    public void SetPropertyBlock(MaterialPropertyBlock block, int material)
    {
        switch (rendType)
        {
            case RendType.Custom:
                if (customRenderer != null)
                    customRenderer.SetPropertyBlock(block, material);
                break;
            default:
                if (renderer != null)
                    renderer.SetPropertyBlock(block, material);
                break;
        }
    }

    public bool IsVisible(float maxFar)
    {
        switch (rendType)
        {
            case RendType.Custom:
                return customRenderer.IsVisible(maxFar); //TO DO
            default:
                return internalIsVisible ? renderer.IsVisibleBySceneCameras(maxFar, gameObject, true) : false;
        }
    }

    public bool IsVisible(bool excludeShadowCasters = false)
    {
        switch (rendType)
        {
            case RendType.Custom:
                return customRenderer.IsVisible(excludeShadowCasters); //TO DO
            default:
                if (excludeShadowCasters)
                    return internalIsVisible ? renderer.IsVisibleBySceneCameras(gameObject, true) : false;
                else
                    return internalIsVisible;
        }
    }

    struct IsVisibleTracker
    {
        public Renderer rend;
        PerFrameTracker tracker;
        bool last;

        public IsVisibleTracker(Renderer rend)
        {
            this.rend = rend;
            tracker = new PerFrameTracker();
            last = false;
        }

        public bool Get()
        {
            if (rend == null)
                return false;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                return rend.isVisible;
#endif

            if (tracker.Simple())
                last = rend.isVisible;
            return last;
        }
    }

    #region Obsolete ClearNulls
    protected static void ClearNulls()
    {
        if ((!wasCleaned) && (renderizables != null))
        {
            renderizables = renderizables.ClearNulls();
            auxGOs = auxGOs.ClearOrCreate();
            foreach (KeyValuePair<GameObject, Renderizable> pair in renderizables)
                if (pair.Value.IsNull())
                    auxGOs.Add(pair.Key);
            foreach (GameObject go in auxGOs)
                renderizables.Remove(go);

            wasCleaned = true;
            Application.onBeforeRender += ResetCleaner;
        }
    }

    protected static void ResetCleaner()
    {
        wasCleaned = false;
        Application.onBeforeRender -= ResetCleaner;
    }
    #endregion

    public static Renderizable Get(Component component)
    {
        return Get(component.gameObject);
    }

    public static Renderizable[] GetAll(Component component, Scope where, bool includeInactive)
    {
        return GetAll(component.gameObject, where, includeInactive);
    }

    public static Renderizable Get(GameObject gameObject)
    {
        if ((!renderizables.SmartGetValue(gameObject, out Renderizable holder)) || holder.IsNull())
            holder = new Renderizable(gameObject);
        return holder.IsNull() ? null : holder;
    }

    //TO DO: Support for multiple renderers in same object?
    public static Renderizable[] GetAll(GameObject gameObject, Scope where, bool includeInactive)
    {
        if (!(includeInactive || gameObject.activeInHierarchy))
            return new Renderizable[0];

        List<Renderizable> all;
        switch (where)
        {
            case Scope.inParents:
                all = new List<Renderizable>();
                all.AddRange(GetAll(gameObject, Scope.inThis, includeInactive));
                Transform parent = gameObject.transform.parent;
                if (parent != null)
                    all.AddRange(GetAll(parent.gameObject, Scope.inParents, includeInactive));
                return all.ToArray();
            case Scope.inChildren:
                all = new List<Renderizable>();
                all.AddRange(GetAll(gameObject, Scope.inThis, includeInactive));
                Transform tr = gameObject.transform;
                for (int i = 0; i < tr.childCount; i++)
                {
                    GameObject go = tr.GetChild(i).gameObject;
                    if (includeInactive || go.activeInHierarchy)
                        all.AddRange(GetAll(go, Scope.inChildren, includeInactive));
                }
                return all.ToArray();
            default:
                Renderizable rend = Get(gameObject);
                return rend.IsNull() ? new Renderizable[0] : new Renderizable[] { rend };
        }
    }
}

public static class RenderizableExtensions
{
    public static bool IsNull(this Renderizable rend)
    {
        return (rend == null) || rend.isNull;
    }
}

public enum RendType
{
    Renderer,
    Custom,
    Null
}

public enum RenType
{
    Filter,
    Sprite,
    Renderer,
    Custom,
    Null
}
