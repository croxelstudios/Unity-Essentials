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
        filtersProcessor = new();
        spritesProcessor = new();
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
        return Get(comp, null, gameObject, reinitialize, nameSufix);
    }

    public static ComputableMesh[] Get(Component comp, string reusableKey, GameObject gameObject,
        bool reinitialize, string nameSufix = SUFFIX)
    {
        if (comp == null)
            return null;

        Renderizable filter = Renderizable.Get(gameObject);

        if (filter == null)
            return null;

        switch (filter.renType)
        {
            case RenType.Filter:
                return new ComputableMesh[] { Get(filter.filter, reusableKey, comp) };
            case RenType.Sprite:
                return new ComputableMesh[] { Get(filter.renderer as SpriteRenderer, reusableKey, comp).mesh };
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
        Renderizable filter = Renderizable.Get(gameObject);

        if (filter != null)
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
        Renderizable filter = Renderizable.Get(obj);
        return filter.LocalToWorldMatrix(id);
    }

    public static Matrix4x4 WorldToLocalMatrix(GameObject obj, int id)
    {
        Renderizable filter = Renderizable.Get(obj);
        return filter.WorldToLocalMatrix(id);
    }

    public static bool IsRenderingAgentEnabled(GameObject obj)
    {
        Renderizable filter = Renderizable.Get(obj);

        if (filter == null)
            return false;

        switch (filter.rendType)
        {
            case RendType.Renderer:
                return filter.renderer.enabled;
            case RendType.Custom:
                return filter.customRenderer.enabled;
            default:
                return false;
        }
    }

    public static bool HasCustomRenderTime(GameObject obj)
    {
        Renderizable renderizable = Renderizable.Get(obj);
        return renderizable.HasCustomRenderTime();
    }

    public static RenType RendererType(GameObject obj)
    {
        Renderizable filter = Renderizable.Get(obj);

        return filter.renType;
    }

    public static bool FilterMeshChanged(GameObject obj)
    {
        Renderizable filter = Renderizable.Get(obj);

        if (filter == null)
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
        Renderizable filter = Renderizable.Get(obj);
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
        Renderizable filter = Renderizable.Get(obj);
        finishActions = finishActions.CreateAdd(comp, method);

        if (filter.renType == RenType.Custom)
        {
            filter.customRenderer.RemoveFinishAction(method);
            //^ In case it's already added
            filter.customRenderer.AddFinishAction(method);
        }
    }

    public static bool IsVisible(GameObject obj, float maxFar)
    {
        Renderizable filter = Renderizable.Get(obj);
        return filter.IsVisible(maxFar);
    }

    public static bool IsVisible(GameObject obj, bool excludeShadowCasters = false)
    {
        Renderizable filter = Renderizable.Get(obj);
        return filter.IsVisible(excludeShadowCasters);
    }

    public static void SetVisible(GameObject obj, bool visible)
    {
        Renderizable element = Renderizable.Get(obj);
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

    static ComputableMesh Get(MeshFilter filter, string reusableKey, Component comp, string nameSuffix = SUFFIX)
    {
        return Get(filter, reusableKey, comp, false, nameSuffix);
    }

    static ComputableMesh Get(MeshFilter filter, Component comp, bool reinitialize, string nameSuffix = SUFFIX)
    {
        return Get(filter, null, comp, reinitialize, nameSuffix);
    }

    static ComputableMesh Get(MeshFilter filter, string reusableKey, Component comp, bool reinitialize, string nameSuffix = SUFFIX)
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
                    filter.sharedMesh, filter.name + nameSuffix, reusableKey);
            }
            else return null;
        }
    }

    static ComputableSprite Get(SpriteRenderer sprRenderer, Component comp, string nameSuffix = SUFFIX)
    {
        return Get(sprRenderer, comp, false, nameSuffix);
    }

    static ComputableSprite Get(SpriteRenderer sprRenderer, string reusableKey, Component comp, string nameSuffix = SUFFIX)
    {
        return Get(sprRenderer, reusableKey, comp, false, nameSuffix);
    }

    static ComputableSprite Get(SpriteRenderer sprRenderer, Component comp, bool reinitialize, string nameSuffix = SUFFIX)
    {
        return Get(sprRenderer, null, comp, reinitialize, nameSuffix);
    }

    static ComputableSprite Get(SpriteRenderer sprRenderer, string reusableKey, Component comp, bool reinitialize, string nameSuffix = SUFFIX)
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
                    sprRenderer.sprite, sprRenderer.name + nameSuffix, reusableKey);
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
        Dictionary<Holder, HolderData> visibleElements;
        Dictionary<Holder, HolderData> allElements;
        Dictionary<Holder, Value> last;
        public bool isInitialized;
        public int count { get { return (allElements != null) ? allElements.Count : 0; } }
        PerFrameTracker cleaningFrame;

        class HolderData : Wrapper
        {
            public Computable element;
            public List<Component> usedBy;
            public Value original;

            public HolderData(Computable element,
                IEnumerable<Component> usedBy, Value original)
            {
                this.element = element;
                this.usedBy = new List<Component>();
                this.usedBy.AddRange(usedBy);
                this.original = original;
            }

            public HolderData(Computable element, Value original)
            {
                this.element = element;
                usedBy = new List<Component>();
                this.original = original;
            }

            public HolderData(Computable element)
            {
                this.element = element;
                usedBy = new List<Component>();
                original = default;
            }

            protected override bool IsNull()
            {
                return element == null;
            }

            public void Clear()
            {
                element = null;
                usedBy.Clear();
                usedBy = null;
                original = null;
            }
        }

        //Reusables
        Dictionary<Holder, ValueKey> keys;
        Dictionary<ValueKey, Computable> reusable;
        Dictionary<ValueKey, int> uses;

        static List<Holder> auxHoldersList;

        public Computable Create(Holder holder, Component comp, Value value, string name, string reusableKey = null)
        {
            Computable computable = null;
            if (!reusableKey.IsNullOrEmpty())
            {
                ValueKey key = new ValueKey(value, reusableKey);
                if (!reusable.SmartGetValue(key, out computable))
                {
                    computable = New(value, name);
                    keys = keys.CreateIfNull();
                    reusable = reusable.CreateAdd(key, computable);
                    uses = uses.CreateAdd(key, 1);
                }
                else uses[key]++;
                keys.Set(holder, key);
            }
            else computable = New(value, name);
            Create(holder, comp, computable);
            return computable;
        }

        protected void Create(Holder holder, Component comp, Computable element)
        {
            Value current = GetCurrent(holder);

            if (ListContainsHolderData(holder, out HolderData data))
            {
                data.element = element;
                data.original = current;
            }
            else allElements =
                    allElements.CreateAdd(holder, new HolderData(element, current));
            SetUseByComponent(holder, comp);
            isInitialized = true;
        }

        protected virtual Value GetCurrent(Holder holder)
        {
            return null;
        }

        protected virtual void SetValue(Holder holder, Value value)
        {
        }

        public virtual Computable New(Value value, string name)
        {
            return new ComputableBase<Value>(value, name) as Computable;
        }

        public bool ElementChanged(Holder holder)
        {
            if (!ListContainsHolderData(holder, out HolderData data))
                return true;

            if (!last.SmartGetValue(holder, out Value value))
                value = GetCurrent(holder);

            if (data.original != value)
                return true;

            return false;
        }

        public void SmartRemove(Holder holder)
        {
            if (ListContainsHolderData(holder, out HolderData data))
            {
                if (last.SmartGetValue(holder, out Value value))
                    SetValue(holder, value);

                allElements.SmartRemove(holder);
                if (keys.SmartGetValue(holder, out ValueKey key))
                {
                    uses[key]--;
                    if (uses[key] <= 0)
                    {
                        reusable.Remove(key);
                        uses.Remove(key);
                    }
                    keys.Remove(holder);
                }

                if ((data.element != null) && (!allElements.Values.Any(x => x.element == data.element)))
                    data.element.Dispose();

                data.Clear();
            }
        }

        public void CleanNullValues()
        {
            cleaningFrame = cleaningFrame.CreateIfNull();
            if (cleaningFrame.Simple() && (!allElements.IsNullOrEmpty()))
            {
                //Remove null holders
                foreach (KeyValuePair<Holder, HolderData> pair in allElements)
                    if (pair.Key == null) pair.Value.Clear();
                allElements = allElements.ClearNulls();

                //Remove holders with null data
                auxHoldersList = auxHoldersList.ClearOrCreate();
                foreach (KeyValuePair<Holder, HolderData> pair in allElements)
                {
                    HolderData data = pair.Value;
                    if ((data == null) || (data.original == null) ||
                        (data.element == null))
                        auxHoldersList.Add(pair.Key);
                }
                foreach (Holder element in auxHoldersList)
                    allElements.Remove(element);
            }
        }

        bool ListContainsHolderData(Holder holder, out HolderData data)
        {
            data = null;
            if (!allElements.NotNullContainsKey(holder))
                return false;
            data = allElements[holder];
            return data != null;
        }

        public Computable GetComputable(Holder holder)
        {
            if (ListContainsHolderData(holder, out HolderData data))
                return data.element;
            else return null;
        }

        public void SetUseByComponent(Holder holder, Component comp)
        {
            if (ListContainsHolderData(holder, out HolderData data))
                data.usedBy.Add(comp);
        }

        public void StopUsing(Holder holder, Component comp)
        {
            if (ListContainsHolderData(holder, out HolderData data))
            {
                List<Component> list = data.usedBy;
                list.SmartRemove(comp);
                if (list.Count <= 0)
                    SmartRemove(holder);
            }
        }

        public void ResetCompletely()
        {
            auxHoldersList = auxHoldersList.ClearOrCreate();
            auxHoldersList.AddRange(allElements.Keys);
            foreach (Holder filter in auxHoldersList)
            {
                if (ListContainsHolderData(filter, out HolderData data))
                {
                    List<Component> list = data.usedBy;
                    for (int i = list.Count - 1; i >= 0; i--)
                        StopUsing(filter, list[i]);
                }
            }
        }

        public void SubstituteElements()
        {
            if (!visibleElements.IsNullOrEmpty())
                foreach (KeyValuePair<Holder, HolderData> pair in visibleElements)
                {
                    Holder holder = pair.Key;
                    if (holder != null)
                    {
                        HolderData data = pair.Value;
                        if (data.element != null)
                        {
                            last = last.CreateAdd(holder, GetCurrent(holder));
                            SetValue(holder, data.element.GetValue());
                        }
                    }
                }
        }

        public void RestoreElements()
        {
            if (!visibleElements.IsNullOrEmpty())
                foreach (KeyValuePair<Holder, HolderData> pair in visibleElements)
                {
                    Holder holder = pair.Key;
                    if (holder != null)
                    {
                        HolderData data = pair.Value;
                        if (data.element != null)
                        {
                            last.SmartRemove(holder);
                            SetValue(holder, data.element.GetOriginal());
                        }
                    }
                }
        }

        public Value OriginalMesh(Holder holder)
        {
            if (ListContainsHolderData(holder, out HolderData data))
                return data.original;
            else return null;
        }

        public void SetVisible(Holder holder, bool visible)
        {
            bool contains = visibleElements.NotNullContainsKey(holder);
            if (contains != visible)
            {
                if (visible)
                    visibleElements = visibleElements.CreateAdd(holder, allElements[holder]);
                else visibleElements.Remove(holder);
            }
        }

        struct ValueKey : IEquatable<ValueKey>
        {
            public Value reference;
            public string key;

            public ValueKey(Value reference, string key)
            {
                this.reference = reference;
                this.key = key;
            }

            public override bool Equals(object other)
            {
                if (!(other is ValueKey)) return false;
                return Equals((ValueKey)other);
            }

            public bool Equals(ValueKey other)
            {
                return (reference == other.reference)
                    && (key == other.key);
            }

            public override int GetHashCode()
            {
                return HashMaker.Elements(reference, key);
            }

            public static bool operator ==(ValueKey o1, ValueKey o2)
            {
                return o1.Equals(o2);
            }

            public static bool operator !=(ValueKey o1, ValueKey o2)
            {
                return !o1.Equals(o2);
            }
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

        public override ComputableMesh New(Mesh value, string name)
        {
            return new ComputableMesh(value, name);
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

        public override ComputableSprite New(Sprite value, string name)
        {
            return new ComputableSprite(value, name);
        }

        protected override void SetValue(SpriteRenderer renderer, Sprite sprite)
        {
            renderer.sprite = sprite;
        }
    }
}
