using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class ComputableModule
{
    static MeshFilterCollection filtersProcessor;
    static SpriteRendererCollection spritesProcessor;
    static Dictionary<Component, UnityAction> startActions;
    static Dictionary<Component, UnityAction> finishActions;
    static bool initialized = false;

    const string SUFFIX = "_Computable";

    static ComputableModule()
    {
        filtersProcessor = new MeshFilterCollection();
        spritesProcessor = new SpriteRendererCollection();
    }

    /// <summary>
    /// Gets the ComputableMesh or ComputableMeshes associated with the specified Component.
    /// If the Rendering Agent is a MeshFilter, automatically sets up the mesh replacement on rendering.
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="nameSufix"></param>
    /// <returns></returns>
    public static ComputableMesh[] Get(Component comp, string nameSufix = SUFFIX)
    {
        return Get(comp, comp.gameObject, false, nameSufix);
    }

    /// <summary>
    /// Gets the ComputableMesh or ComputableMeshes associated with the specified Component.
    /// If the Rendering Agent is a MeshFilter, automatically sets up the mesh replacement on rendering.
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="nameSufix"></param>
    /// <returns></returns>
    public static ComputableMesh[] Get(Component comp, bool reinitialize, string nameSufix = SUFFIX)
    {
        return Get(comp, comp.gameObject, reinitialize, nameSufix);
    }

    /// <summary>
    /// Gets the ComputableMesh or ComputableMeshes associated with the specified Component and GameObject.
    /// If the Rendering Agent is a MeshFilter, automatically sets up the mesh replacement on rendering.
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="nameSufix"></param>
    /// <returns></returns>
    public static ComputableMesh[] Get(Component comp, GameObject gameObject, string nameSufix = SUFFIX)
    {
        return Get(comp, gameObject, false, nameSufix);
    }

    public static ComputableMesh[] Get(Component comp, GameObject gameObject,
        bool reinitialize, string nameSufix = SUFFIX)
    {
        if (comp == null)
            return null;

        ComputableElement filter = ComputableElement.Get(gameObject);

        if (filter.isNull)
            return null;

        switch (filter.renType)
        {
            case RenType.Filter:
                return new ComputableMesh[] { Get(filter.filter, comp) };
            case RenType.Sprite:
                return new ComputableMesh[] { Get(filter.renderer as SpriteRenderer, comp).mesh };
            case RenType.Custom:
                return filter.customRenderer.enabled ?
                    filter.customRenderer.GetComputables(comp) : new ComputableMesh[0];
            default:
                return null;
        }
    }

    public static void StopUsing(Component comp)
    {
        StopUsing(comp, comp.gameObject);
    }
    
    public static void StopUsing(Component comp, GameObject gameObject)
    {
        ComputableElement filter = ComputableElement.Get(gameObject);

        if (!filter.isNull)
        {
            switch (filter.renType)
            {
                case RenType.Filter:
                    StopUsing(filter.filter, comp);
                    startActions.SmartRemove(comp);
                    finishActions.SmartRemove(comp);
                    break;
                case RenType.Sprite:
                    StopUsing(filter.renderer as SpriteRenderer, comp);
                    startActions.SmartRemove(comp);
                    finishActions.SmartRemove(comp);
                    break;
                case RenType.Custom:
                    if (startActions.SmartGetValue(comp, out UnityAction staction))
                    {
                        filter.customRenderer.RemoveStartAction(staction);
                        startActions.Remove(comp);
                    }
                    if (finishActions.SmartGetValue(comp, out UnityAction fiaction))
                    {
                        filter.customRenderer.RemoveFinishAction(fiaction);
                        finishActions.Remove(comp);
                    }
                    filter.customRenderer.StopUseByComponent(comp);
                    break;
                default:
                    break;
            }
        }
    }

    public static Matrix4x4 LocalToWorldMatrix(GameObject obj, int id)
    {
        ComputableElement filter = ComputableElement.Get(obj);
        return filter.LocalToWorldMatrix(id);
    }

    public static Matrix4x4 WorldToLocalMatrix(GameObject obj, int id)
    {
        ComputableElement filter = ComputableElement.Get(obj);
        return filter.WorldToLocalMatrix(id);
    }

    public static bool IsRenderingAgentEnabled(GameObject obj)
    {
        ComputableElement filter = ComputableElement.Get(obj);

        if (filter.isNull)
            return false;

        switch (filter.renType)
        {
            case RenType.Filter:
                return true; //TO DO
            case RenType.Sprite:
                return true; //TO DO
            case RenType.Custom:
                return filter.customRenderer.enabled;
            default:
                return false;
        }
    }

    public static bool IsRendererCullable(GameObject obj)
    {
        switch (RendererType(obj))
        {
            case RenType.Filter:
                return true;
            case RenType.Sprite:
                return true;
            case RenType.Custom:
                return false;
            default:
                return false;
        }
    }

    public static RenType RendererType(GameObject obj)
    {
        ComputableElement filter = ComputableElement.Get(obj);

        return filter.renType;
    }

    public static bool FilterMeshChanged(GameObject obj)
    {
        ComputableElement filter = ComputableElement.Get(obj);

        if (filter.isNull)
            return true;

        switch (filter.renType)
        {
            case RenType.Filter:
                return filtersProcessor.ElementChanged(filter.filter);
            case RenType.Sprite:
                return spritesProcessor.ElementChanged(filter.renderer as SpriteRenderer);
            case RenType.Custom:
                return false;
            default:
                return false;
        }
    }

    /// <summary>
    /// Tracks the start rendering event for the specified Component.
    /// If the rendering agent is a CustomRenderer, it assigns the method to be called when rendering starts.
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="method"></param>
    public static void SetRenderingEvent_Start(Component comp, UnityAction method)
    {
        SetRenderingEvent_Start(comp, comp.gameObject, method);
    }

    public static void SetRenderingEvent_Start(Component comp, GameObject obj, UnityAction method)
    {
        ComputableElement filter = ComputableElement.Get(obj);
        startActions = startActions.CreateAdd(comp, method);

        if (filter.renType == RenType.Custom)
        {
            filter.customRenderer.RemoveStartAction(method);
            //^ In case it's already added
            filter.customRenderer.AddStartAction(method);
        }
    }

    /// <summary>
    /// Tracks the finished rendering event for the specified Component.
    /// If the rendering agent is a CustomRenderer, it assigns the method to be called when rendering is finished.
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="method"></param>
    public static void SetRenderingEvent_Finished(Component comp, UnityAction method)
    {
        SetRenderingEvent_Finished(comp, comp.gameObject, method);
    }

    public static void SetRenderingEvent_Finished(Component comp, GameObject obj, UnityAction method)
    {
        ComputableElement filter = ComputableElement.Get(obj);
        finishActions = finishActions.CreateAdd(comp, method);

        if (filter.renType == RenType.Custom)
        {
            filter.customRenderer.RemoveFinishAction(method);
            //^ In case it's already added
            filter.customRenderer.AddFinishAction(method);
        }
    }

    public static bool IsVisible(GameObject obj, bool excludeShadowCasters = false)
    {
        ComputableElement filter = ComputableElement.Get(obj);
        return filter.IsVisible(excludeShadowCasters);
    }

    public static void SetVisible(GameObject obj, bool visible)
    {
        ComputableElement element = ComputableElement.Get(obj);
        switch (element.renType)
        {
            case RenType.Filter:
                filtersProcessor.SetVisible(element.filter, visible);
                break;
            case RenType.Sprite:
                spritesProcessor.SetVisible(element.renderer as SpriteRenderer, visible);
                break;
            default:
                break;
        }
    }

    //By MeshFilter
    static ComputableMesh Get(MeshFilter filter, Component comp, string nameSuffix = SUFFIX)
    {
        return Get(filter, comp, false, nameSuffix);
    }

    static ComputableMesh Get(MeshFilter filter, Component comp, bool reinitialize, string nameSuffix = SUFFIX)
    {
        if (!initialized)
        {
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            RenderPipelineManager.endCameraRendering += EndCameraRendering;
            initialized = true;
        }

        if (filtersProcessor.isInitialized)
            filtersProcessor.CleanNullValues();

        bool validMeshExists = false;
        if (filtersProcessor.ElementChanged(filter))
            filtersProcessor.SmartRemove(filter);
        else validMeshExists = true;

        ComputableMesh mesh;
        if (validMeshExists)
        {
            mesh = filtersProcessor.GetComputable(filter);

            if (reinitialize || (mesh.mesh == null))
                mesh.Initialize(filter.sharedMesh, filter.name + nameSuffix);
            else mesh.name = filter.name + nameSuffix;

            filtersProcessor.SetUseByComponent(filter, comp);

            return mesh;
        }
        else
        {
            if (filter.sharedMesh != null)
            {
                return filtersProcessor.Create(filter, comp,
                    filter.sharedMesh, filter.name + nameSuffix);
            }
            else return null;
        }
    }

    static ComputableSprite Get(SpriteRenderer sprRenderer, Component comp, string nameSuffix = SUFFIX)
    {
        return Get(sprRenderer, comp, false, nameSuffix);
    }

    static ComputableSprite Get(SpriteRenderer sprRenderer, Component comp, bool reinitialize, string nameSuffix = SUFFIX)
    {
        if (!initialized)
        {
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            RenderPipelineManager.endCameraRendering += EndCameraRendering;
            initialized = true;
        }

        if (spritesProcessor.isInitialized)
            spritesProcessor.CleanNullValues();

        bool validSpriteExists = false;
        if (spritesProcessor.ElementChanged(sprRenderer))
            spritesProcessor.SmartRemove(sprRenderer);
        else validSpriteExists = true;

        ComputableSprite spr;
        if (validSpriteExists)
        {
            spr = spritesProcessor.GetComputable(sprRenderer);

            if (reinitialize)
                spr.Initialize(sprRenderer.sprite, sprRenderer.name + nameSuffix);
            else spr.name = sprRenderer.name + nameSuffix;

            spritesProcessor.SetUseByComponent(sprRenderer, comp);

            return spr;
        }
        else
        {
            if (sprRenderer.sprite != null)
            {
                return spritesProcessor.Create(sprRenderer, comp,
                    sprRenderer.sprite, sprRenderer.name + nameSuffix);
            }
            else return null;
        }
    }

    static void StopUsing(MeshFilter filter, Component comp)
    {
        filtersProcessor.StopUsing(filter, comp);
    }

    static void StopUsing(SpriteRenderer renderer, Component comp)
    {
        spritesProcessor.StopUsing(renderer, comp);
    }

    static List<Camera> renderingCameras;

    static void BeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (renderingCameras.IsNullOrEmpty())
        {
            if (filtersProcessor.isInitialized)
                filtersProcessor.SubstituteElements();

            if (spritesProcessor.isInitialized)
                spritesProcessor.SubstituteElements();
        }

        renderingCameras = renderingCameras.CreateAdd(cam);
    }

    static void EndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        renderingCameras.SmartRemove(cam);

        if (renderingCameras.IsNullOrEmpty())
        {
            if (filtersProcessor.isInitialized)
                filtersProcessor.RestoreElements();

            if (spritesProcessor.isInitialized)
                spritesProcessor.RestoreElements();
        }
    }

    class ReplaceElementCollection<Holder, Value, Computable>
        where Value : Object where Computable : ComputableBase<Value>
    {
        Dictionary<Holder, bool> visible;
        Dictionary<Holder, Computable> elements;
        Dictionary<Holder, List<Component>> usedBy;
        Dictionary<Holder, Value> originals;
        Dictionary<Holder, Value> last;
        public bool isInitialized;
        public int count { get { return (elements != null) ? elements.Count : 0; } }
        bool wasCleaned;

        static List<Holder> auxElementList;

        public Computable Create(Holder element, Component comp, Value value, string name)
        {
            Computable computable = New(value, name);
            Create(element, comp, computable);
            return computable;
        }

        protected void Create(Holder filter, Component comp, Computable mesh)
        {
            visible = visible.CreateAdd(filter, false);
            originals = originals.CreateAdd(filter, GetCurrent(filter));
            elements = elements.CreateAdd(filter, mesh);
            SetUseByComponent(filter, comp);
            isInitialized = true;
        }

        protected virtual Value GetCurrent(Holder element)
        {
            return null;
        }

        protected virtual void SetValue(Holder element, Value value)
        {
        }

        public virtual Computable New(Value value, string name)
        {
            return null;
        }

        public bool ElementChanged(Holder element)
        {
            if (!originals.NotNullContainsKey(element))
                return true;

            if (last.NotNullContainsKey(element))
            {
                if (originals[element] != last[element])
                    return true;
            }
            else if (GetCurrent(element) != originals[element])
                return true;

            return false;
        }

        public void SmartRemove(Holder element)
        {
            if (last.NotNullContainsKey(element))
                SetValue(element, last[element]);

            if (originals.SmartGetValue(element, out Value original))
            {
                originals.Remove(element);
                usedBy.SmartRemove(element);
                elements.SmartRemove(element);
                last.SmartRemove(element);
            }
        }

        public void CleanNullValues()
        {
            if ((!wasCleaned) && (!originals.IsNullOrEmpty()))
            {
                //Remove null filters
                originals = originals.ClearNulls();
                usedBy = usedBy.ClearNulls();
                elements = elements.ClearNulls();
                last = last.ClearNulls();

                //Remove filters with null meshes
                auxElementList = auxElementList.ClearOrCreate();
                foreach (KeyValuePair<Holder, Value> pair in originals)
                    if (pair.Value == null)
                        auxElementList.Add(pair.Key);
                foreach (Holder element in auxElementList)
                {
                    originals.Remove(element);
                    elements.Remove(element);
                }

                //Remove filters with null computables
                auxElementList = auxElementList.ClearOrCreate(); //To remove
                foreach (KeyValuePair<Holder, Computable> pair in elements)
                    if ((pair.Value == null) || (pair.Value.IsNull()))
                        auxElementList.Add(pair.Key);
                foreach (Holder element in auxElementList)
                {
                    originals.Remove(element);
                    elements.Remove(element);
                }

                wasCleaned = true;
                Application.onBeforeRender += ResetCleaner;
            }
        }

        void ResetCleaner()
        {
            wasCleaned = false;
            Application.onBeforeRender -= ResetCleaner;
        }

        public Computable GetComputable(Holder element)
        {
            if (!originals.NotNullContainsKey(element))
                return null;

            if (!elements.NotNullContainsKey(element))
                return null;

            return elements[element];
        }

        public void SetUseByComponent(Holder filter, Component comp)
        {
            usedBy = usedBy.CreateAdd(filter, comp);
        }

        public void StopUsing(Holder filter, Component comp)
        {
            if (usedBy.SmartGetValue(filter, out List<Component> list))
            {
                list.SmartRemove(comp);
                if (list.Count <= 0)
                    SmartRemove(filter);
            }
        }

        public void ResetCompletely()
        {
            Holder[] filters = originals.Keys.ToArray();
            foreach (Holder filter in filters)
                for (int i = usedBy[filter].Count - 1; i >= 0; i--)
                    StopUsing(filter, usedBy[filter][i]);
        }

        public void SubstituteElements()
        {
            Holder[] elements = originals.Keys.ToArray();

            foreach (Holder element in elements)
                if ((element != null) && visible[element])
                {
                    last = last.CreateAdd(element, GetCurrent(element));
                    SetValue(element, this.elements[element].GetValue());
                }
        }

        public void RestoreElements()
        {
            Holder[] elements = originals.Keys.ToArray();

            foreach (Holder element in elements)
                if ((element != null) && visible[element])
                {
                    last.SmartRemove(element);
                    SetValue(element, this.elements[element].GetOriginal());
                }
        }

        public Value OriginalMesh(Holder element)
        {
            if (!originals.NotNullContainsKey(element))
                return null;
            return originals[element];
        }

        public void SetVisible(Holder element, bool visible)
        {
            if (this.visible.NotNullContainsKey(element))
                this.visible[element] = visible;
        }
    }

    class MeshFilterCollection : ReplaceElementCollection<MeshFilter, Mesh, ComputableMesh>
    {
        protected override Mesh GetCurrent(MeshFilter filter)
        {
            return filter.sharedMesh;
        }

        protected override void SetValue(MeshFilter filter, Mesh mesh)
        {
            filter.sharedMesh = mesh;
        }

        public override ComputableMesh New(Mesh mesh, string name)
        {
            return new ComputableMesh(mesh, name);
        }

        public ComputableMesh Create(MeshFilter filter, Component comp, string name, int vCount, int tCount)
        {
            ComputableMesh computable = new ComputableMesh(name, vCount, tCount);
            Create(filter, comp, computable);
            return computable;
        }
    }

    class SpriteRendererCollection : ReplaceElementCollection<SpriteRenderer, Sprite, ComputableSprite>
    {
        protected override Sprite GetCurrent(SpriteRenderer renderer)
        {
            return renderer.sprite;
        }

        protected override void SetValue(SpriteRenderer renderer, Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        public override ComputableSprite New(Sprite sprite, string name)
        {
            return new ComputableSprite(sprite, name);
        }
    }

    struct ComputableElement : IEquatable<ComputableElement>
    {
        static Dictionary<GameObject, ComputableElement> filters = null;
        static bool wasCleaned;
        public MeshFilter filter;
        public Renderer renderer;
        public CustomRenderer customRenderer;
        public Transform transform;
        public RenType renType;
        public bool isNull
        {
            get
            {
                switch (renType)
                {
                    case RenType.Filter:
                        return filter == null;
                    case RenType.Sprite:
                        return renderer == null;
                    case RenType.Custom:
                        return customRenderer == null;
                    default:
                        return renderer == null;
                }
            }
        }
        public Matrix4x4 LocalToWorldMatrix(int id)
        {
            return (renType != RenType.Custom) ? transform.localToWorldMatrix :
                customRenderer.LocalToWorldMatrix(id);
        }
        public Matrix4x4 WorldToLocalMatrix(int id)
        {
            return (renType != RenType.Custom) ? transform.worldToLocalMatrix :
                customRenderer.WorldToLocalMatrix(id);
        }

        public bool IsVisible(bool excludeShadowCasters = false)
        {
            switch (renType)
            {
                case RenType.Custom:
                    return customRenderer.IsVisible(excludeShadowCasters); //TO DO
                default:
                    if (excludeShadowCasters)
                        return renderer.IsVisibleBySceneCameras();
                    else
                        return renderer.isVisible;
            }
        }

        static List<GameObject> auxGOs;

        public ComputableElement(MeshFilter filter)
        {
            this.filter = filter;
            renderer = filter.gameObject.GetComponent<MeshRenderer>();
            customRenderer = null;
            transform = null;
            renType = RenType.Filter;

            if (filter != null)
                transform = filter.transform;
        }

        public ComputableElement(SpriteRenderer spriteRenderer)
        {
            filter = null;
            renderer = spriteRenderer;
            customRenderer = null;
            transform = null;
            renType = RenType.Sprite;

            if (spriteRenderer != null)
                transform = spriteRenderer.transform;
        }

        public ComputableElement(CustomRenderer customRenderer)
        {
            filter = null;
            renderer = null;
            this.customRenderer = customRenderer;
            transform = null;
            renType = RenType.Custom;

            if (customRenderer != null)
                transform = customRenderer.transform;
        }

        public ComputableElement(GameObject gameObject)
        {
            filter = gameObject.GetComponent<MeshFilter>();
            renderer = null;
            customRenderer = null;
            transform = gameObject.transform;

            if (filter != null)
            {
                renderer = filter.gameObject.GetComponent<MeshRenderer>();
                renType = RenType.Filter;
            }
            else
            {
                renderer = gameObject.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    customRenderer = gameObject.GetComponent<CustomRenderer>();
                    renType = RenType.Custom;
                }
                else renType = RenType.Sprite;
            }
        }

        public static ComputableElement Get(GameObject gameObject)
        {
            ClearNulls();

            ComputableElement filter;
            if (!filters.SmartGetValue(gameObject, out filter))
            {
                filter = new ComputableElement(gameObject);
                filters = filters.CreateAdd(gameObject, filter);
            }
            else
                if (filter.isNull)
            {
                filters.Remove(gameObject);
                filter = new ComputableElement(gameObject);
                filters = filters.CreateAdd(gameObject, filter);
            }
            return filter;
        }

        static void ClearNulls()
        {
            if ((!wasCleaned) && (filters != null))
            {
                filters = filters.ClearNulls();
                auxGOs = auxGOs.ClearOrCreate();
                foreach (KeyValuePair<GameObject, ComputableElement> pair in filters)
                    if (pair.Value.isNull)
                        auxGOs.Add(pair.Key);
                foreach (GameObject go in auxGOs)
                    filters.Remove(go);

                wasCleaned = true;
                Application.onBeforeRender += ResetCleaner;
            }
        }

        static void ResetCleaner()
        {
            wasCleaned = false;
            Application.onBeforeRender -= ResetCleaner;
        }

        public override bool Equals(object other)
        {
            if (!(other is ComputableElement)) return false;
            return Equals((ComputableElement)other);
        }

        public bool Equals(ComputableElement other)
        {
            return (filter == other.filter)
                && (customRenderer == other.customRenderer);
        }

        public override int GetHashCode()
        {
            switch (renType)
            {
                case RenType.Filter:
                    return filter.GetHashCode();
                case RenType.Sprite:
                    return renderer.GetHashCode();
                case RenType.Custom:
                    return customRenderer.GetHashCode();
                default:
                    return 0;
            }
        }

        public static bool operator ==(ComputableElement o1, ComputableElement o2)
        {
            return o1.Equals(o2);
        }

        public static bool operator !=(ComputableElement o1, ComputableElement o2)
        {
            return !o1.Equals(o2);
        }
    }
}

public enum RenType
{
    Filter,
    Sprite,
    Custom,
    Null
}
