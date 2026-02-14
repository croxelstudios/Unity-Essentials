using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public static class ComputableModule
{
    static MeshFilterCollection filtersProcessor;
    static SpriteRendererCollection spritesProcessor;
    static Dictionary<Component, UnityAction> startActions;
    static Dictionary<Component, UnityAction> finishActions;
    static bool initialized = false;

    /// <summary>
    /// Gets the ComputableMesh or ComputableMeshes associated with the specified component.
    /// If the Rendering Agent is a MeshFilter, automatically sets up the mesh replacement on rendering.
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="nameSufix"></param>
    /// <returns></returns>
    public static ComputableMesh[] Get(Component comp, string nameSufix = "_Computable")
    {
        return Get(comp, false, nameSufix);
    }

    public static ComputableMesh[] Get(Component comp, bool reinitialize, string nameSufix = "_Computable")
    {
        if (comp == null)
            return null;

        ComputableElement filter = ComputableElement.Get(comp.gameObject);

        if (filter.isNull)
            return null;

        switch (filter.renType)
        {
            case RenType.Filter:
                return new ComputableMesh[] { Get(filter.filter, comp) };
            case RenType.Sprite:
                return new ComputableMesh[] { Get(filter.spriteRenderer, comp).mesh };
            case RenType.Custom:
                return filter.customRenderer.enabled ?
                    filter.customRenderer.GetComputables(comp) : new ComputableMesh[0];
            default:
                return null;
        }
    }

    public static void StopUsing(Component comp)
    {
        ComputableElement filter = ComputableElement.Get(comp.gameObject);

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
                    StopUsing(filter.spriteRenderer, comp);
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

    public static Matrix4x4 LocalToWorldMatrix(Component comp, int id)
    {
        ComputableElement filter = ComputableElement.Get(comp.gameObject);
        return filter.LocalToWorldMatrix(id);
    }

    public static Matrix4x4 WorldToLocalMatrix(Component comp, int id)
    {
        ComputableElement filter = ComputableElement.Get(comp.gameObject);
        return filter.WorldToLocalMatrix(id);
    }

    public static bool IsRenderingAgentEnabled(Component comp)
    {
        ComputableElement filter = ComputableElement.Get(comp.gameObject);

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

    public static bool IsRendererCullable(Component comp)
    {
        switch (RendererType(comp))
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

    public static RenType RendererType(Component comp)
    {
        ComputableElement filter = ComputableElement.Get(comp.gameObject);

        return filter.renType;
    }

    public static bool FilterMeshChanged(Component comp)
    {
        ComputableElement filter = ComputableElement.Get(comp.gameObject);

        if (filter.isNull)
            return true;

        switch (filter.renType)
        {
            case RenType.Filter:
                return filtersProcessor.MeshChanged(filter.filter);
            case RenType.Sprite:
                return spritesProcessor.SpriteChanged(filter.spriteRenderer);
            case RenType.Custom:
                return false;
            default:
                return false;
        }
    }


    /// <summary>
    /// Tracks the start rendering event for the specified component.
    /// If the rendering agent is a CustomRenderer, it assigns the method to be called when rendering starts.
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="method"></param>
    public static void SetRenderingEvent_Start(Component comp, UnityAction method)
    {
        ComputableElement filter = ComputableElement.Get(comp.gameObject);
        startActions = startActions.CreateAdd(comp, method);

        if (filter.renType == RenType.Custom)
        {
            filter.customRenderer.RemoveStartAction(method);
            //^ In case it's already added
            filter.customRenderer.AddStartAction(method);
        }
    }

    /// <summary>
    /// Tracks the finished rendering event for the specified component.
    /// If the rendering agent is a CustomRenderer, it assigns the method to be called when rendering is finished.
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="method"></param>
    public static void SetRenderingEvent_Finished(Component comp, UnityAction method)
    {
        ComputableElement filter = ComputableElement.Get(comp.gameObject);
        finishActions = finishActions.CreateAdd(comp, method);

        if (filter.renType == RenType.Custom)
        {
            filter.customRenderer.RemoveFinishAction(method);
            //^ In case it's already added
            filter.customRenderer.AddFinishAction(method);
        }
    }

    //By MeshFilter
    static ComputableMesh Get(MeshFilter filter, Component comp, string nameSuffix = "_Computable")
    {
        return Get(filter, comp, false, nameSuffix);
    }

    static ComputableMesh Get(MeshFilter filter, Component comp, bool reinitialize, string nameSuffix = "_Computable")
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
        if (filtersProcessor.MeshChanged(filter))
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

    static ComputableSprite Get(SpriteRenderer sprRenderer, Component comp, string nameSuffix = "_Computable")
    {
        return Get(sprRenderer, comp, false, nameSuffix);
    }

    static ComputableSprite Get(SpriteRenderer sprRenderer, Component comp, bool reinitialize, string nameSuffix = "_Computable")
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
        if (spritesProcessor.SpriteChanged(sprRenderer))
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
                filtersProcessor.SubstituteFilterMeshes();

            if (spritesProcessor.isInitialized)
                spritesProcessor.SubstituteSprites();
        }

        renderingCameras = renderingCameras.CreateAdd(cam);
    }

    static void EndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        renderingCameras.SmartRemove(cam);

        if (renderingCameras.IsNullOrEmpty())
        {
            if (filtersProcessor.isInitialized)
                filtersProcessor.RestoreFilterMeshes();

            if (spritesProcessor.isInitialized)
                spritesProcessor.RestoreSprites();
        }
    }

    struct MeshFilterCollection
    {
        Dictionary<MeshFilter, ComputableMesh> meshes;
        Dictionary<MeshFilter, List<Component>> usedBy;
        Dictionary<MeshFilter, Mesh> originals;
        Dictionary<MeshFilter, Mesh> last;
        public bool isInitialized;
        public int count { get { return (meshes != null) ? meshes.Count : 0; } }
        bool wasCleaned;

        static List<MeshFilter> auxFilterList;

        public bool MeshChanged(MeshFilter filter)
        {
            if (!originals.NotNullContainsKey(filter))
                return true;

            if (last.NotNullContainsKey(filter))
            {
                if (originals[filter] != last[filter])
                    return true;
            }
            else if (filter.sharedMesh != originals[filter])
                return true;

            return false;
        }

        public void SmartRemove(MeshFilter filter)
        {
            if (last.NotNullContainsKey(filter))
                filter.sharedMesh = last[filter];

            if (originals.SmartGetValue(filter, out Mesh original))
            {
                originals.Remove(filter);
                usedBy.SmartRemove(filter);
                meshes.SmartRemove(filter);
                last.SmartRemove(filter);
            }
        }

        public void CleanNullValues()
        {
            if ((!wasCleaned) && (!originals.IsNullOrEmpty()))
            {
                //Remove null filters
                originals = originals.ClearNulls();
                usedBy = usedBy.ClearNulls();
                meshes = meshes.ClearNulls();
                last = last.ClearNulls();

                //Remove filters with null meshes
                auxFilterList = auxFilterList.ClearOrCreate();
                foreach (KeyValuePair<MeshFilter, Mesh> pair in originals)
                    if (pair.Value == null)
                        auxFilterList.Add(pair.Key);
                foreach (MeshFilter filter in auxFilterList)
                {
                    originals.Remove(filter);
                    meshes.Remove(filter);
                }

                //Remove filters with null computables
                auxFilterList = auxFilterList.ClearOrCreate(); //To remove
                foreach (KeyValuePair<MeshFilter, ComputableMesh> pair in meshes)
                    if ((pair.Value == null) || (pair.Value.mesh == null))
                        auxFilterList.Add(pair.Key);
                foreach (MeshFilter filter in auxFilterList)
                {
                    originals.Remove(filter);
                    meshes.Remove(filter);
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

        public ComputableMesh GetComputable(MeshFilter filter)
        {
            if (!originals.NotNullContainsKey(filter))
                return null;

            if (!meshes.NotNullContainsKey(filter))
                return null;

            return meshes[filter];
        }

        public void SetUseByComponent(MeshFilter filter, Component comp)
        {
            usedBy = usedBy.CreateAdd(filter, comp);
        }

        public ComputableMesh Create(MeshFilter filter, Component comp, string name, int vCount, int tCount)
        {
            ComputableMesh mesh = new ComputableMesh(name, vCount, tCount);
            Create(filter, comp, mesh);
            return mesh;
        }

        public ComputableMesh Create(MeshFilter filter, Component comp, Mesh mesh, string name)
        {
            ComputableMesh computable = new ComputableMesh(mesh, name);
            Create(filter, comp, computable);
            return computable;
        }

        public void StopUsing(MeshFilter filter, Component comp)
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
            MeshFilter[] filters = originals.Keys.ToArray();
            foreach (MeshFilter filter in filters)
                for (int i = usedBy[filter].Count - 1; i >= 0; i--)
                    StopUsing(filter, usedBy[filter][i]);
        }

        public void SubstituteFilterMeshes()
        {
            MeshFilter[] filters = originals.Keys.ToArray();

            foreach (MeshFilter filter in filters)
                if (filter != null)
                {
                    last = last.CreateAdd(filter, filter.sharedMesh);
                    filter.sharedMesh = meshes[filter];
                }
        }

        public void RestoreFilterMeshes()
        {
            MeshFilter[] filters = originals.Keys.ToArray();

            foreach (MeshFilter filter in filters)
                if (filter != null)
                {
                    last.SmartRemove(filter);
                    filter.sharedMesh = meshes[filter].original;
                }
        }

        public Mesh OriginalMesh(MeshFilter filter)
        {
            if (!originals.NotNullContainsKey(filter))
                return null;
            return originals[filter];
        }

        void Create(MeshFilter filter, Component comp, ComputableMesh mesh)
        {
            originals = originals.CreateAdd(filter, filter.sharedMesh);
            meshes = meshes.CreateAdd(filter, mesh);
            SetUseByComponent(filter, comp);
            isInitialized = true;
        }
    }

    struct SpriteRendererCollection
    {
        Dictionary<SpriteRenderer, ComputableSprite> sprites;
        Dictionary<SpriteRenderer, List<Component>> usedBy;
        Dictionary<SpriteRenderer, Sprite> originals;
        Dictionary<SpriteRenderer, Sprite> last;
        public bool isInitialized;
        public int count { get { return (sprites != null) ? sprites.Count : 0; } }
        bool wasCleaned;

        static List<SpriteRenderer> auxRendererList;

        public bool SpriteChanged(SpriteRenderer renderer)
        {
            if (!originals.NotNullContainsKey(renderer))
                return true;

            if (last.NotNullContainsKey(renderer))
            {
                if (originals[renderer] != last[renderer])
                    return true;
            }
            else if (renderer.sprite != originals[renderer])
                return true;

            return false;
        }

        public void SmartRemove(SpriteRenderer renderer)
        {
            if (last.NotNullContainsKey(renderer))
                renderer.sprite = last[renderer];

            if (originals.SmartGetValue(renderer, out Sprite original))
            {
                originals.Remove(renderer);
                usedBy.SmartRemove(renderer);
                sprites.SmartRemove(renderer);
                last.SmartRemove(renderer);
            }
        }

        public void CleanNullValues()
        {
            if ((!wasCleaned) && (!originals.IsNullOrEmpty()))
            {
                //Remove null filters
                originals = originals.ClearNulls();
                usedBy = usedBy.ClearNulls();
                sprites = sprites.ClearNulls();
                last = last.ClearNulls();

                //Remove renderers with null meshes
                auxRendererList = auxRendererList.ClearOrCreate();
                foreach (KeyValuePair<SpriteRenderer, Sprite> pair in originals)
                    if (pair.Value == null)
                        auxRendererList.Add(pair.Key);
                foreach (SpriteRenderer renderer in auxRendererList)
                {
                    originals.Remove(renderer);
                    sprites.Remove(renderer);
                }

                //Remove renderers with null computables
                auxRendererList = auxRendererList.ClearOrCreate(); //To remove
                foreach (KeyValuePair<SpriteRenderer, ComputableSprite> pair in sprites)
                    if (pair.Value == null)
                        auxRendererList.Add(pair.Key);
                foreach (SpriteRenderer renderer in auxRendererList)
                {
                    originals.Remove(renderer);
                    sprites.Remove(renderer);
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

        public ComputableSprite GetComputable(SpriteRenderer renderer)
        {
            if (!originals.NotNullContainsKey(renderer))
                return null;

            if (!sprites.NotNullContainsKey(renderer))
                return null;

            return sprites[renderer];
        }

        public void SetUseByComponent(SpriteRenderer renderer, Component comp)
        {
            usedBy = usedBy.CreateAdd(renderer, comp);
        }

        public ComputableSprite Create(SpriteRenderer renderer, Component comp, Sprite sprite, string name)
        {
            ComputableSprite computable = new ComputableSprite(sprite, name);
            Create(renderer, comp, computable);
            return computable;
        }

        public void StopUsing(SpriteRenderer renderer, Component comp)
        {
            if (usedBy.SmartGetValue(renderer, out List<Component> list))
            {
                list.SmartRemove(comp);
                if (list.Count <= 0)
                    SmartRemove(renderer);
            }
        }

        public void ResetCompletely()
        {
            SpriteRenderer[] renderers = originals.Keys.ToArray();
            foreach (SpriteRenderer renderer in renderers)
                for (int i = usedBy[renderer].Count - 1; i >= 0; i--)
                    StopUsing(renderer, usedBy[renderer][i]);
        }

        public void SubstituteSprites()
        {
            SpriteRenderer[] renderers = originals.Keys.ToArray();

            foreach (SpriteRenderer renderer in renderers)
                if (renderer != null)
                {
                    last = last.CreateAdd(renderer, renderer.sprite);
                    renderer.sprite = sprites[renderer].GetSprite();
                }
        }

        public void RestoreSprites()
        {
            SpriteRenderer[] renderers = originals.Keys.ToArray();

            foreach (SpriteRenderer renderer in renderers)
                if (renderer != null)
                {
                    last.SmartRemove(renderer);
                    renderer.sprite = sprites[renderer].original;
                }
        }

        public Sprite OriginalSprite(SpriteRenderer renderer)
        {
            if (!originals.NotNullContainsKey(renderer))
                return null;
            return originals[renderer];
        }

        void Create(SpriteRenderer renderer, Component comp, ComputableSprite sprite)
        {
            originals = originals.CreateAdd(renderer, renderer.sprite);
            sprites = sprites.CreateAdd(renderer, sprite);
            SetUseByComponent(renderer, comp);
            isInitialized = true;
        }
    }

    struct ComputableElement : IEquatable<ComputableElement>
    {
        static Dictionary<GameObject, ComputableElement> filters = null;
        static bool wasCleaned;
        public MeshFilter filter;
        public MeshRenderer renderer;
        public SpriteRenderer spriteRenderer;
        public CustomRenderer customRenderer;
        public Transform transform;
        public RenType renType
        {
            get
            {
                if (filter != null)
                    return RenType.Filter;
                else if (spriteRenderer != null)
                    return RenType.Sprite;
                else if (customRenderer != null)
                    return RenType.Custom;
                else
                    return RenType.Null;
            }
        }
        public bool isNull
        {
            get
            {
                return
                    (filter == null) &&
                    (spriteRenderer == null) &&
                    (customRenderer == null);
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

        static List<GameObject> auxGOs;

        public ComputableElement(MeshFilter filter)
        {
            this.filter = filter;
            renderer = filter.gameObject.GetComponent<MeshRenderer>();
            customRenderer = null;
            spriteRenderer = null;
            transform = null;

            if (filter != null)
                transform = filter.transform;
        }

        public ComputableElement(SpriteRenderer spriteRenderer)
        {
            this.spriteRenderer = spriteRenderer;
            filter = null;
            renderer = null;
            customRenderer = null;
            transform = null;

            if (spriteRenderer != null)
                transform = spriteRenderer.transform;
        }

        public ComputableElement(CustomRenderer customRenderer)
        {
            filter = null;
            renderer = null;
            spriteRenderer = null;
            this.customRenderer = customRenderer;
            transform = null;

            if (customRenderer != null)
                transform = customRenderer.transform;
        }

        public ComputableElement(GameObject gameObject)
        {
            filter = gameObject.GetComponent<MeshFilter>();
            renderer = null;
            customRenderer = null;
            spriteRenderer = null;
            transform = gameObject.transform;

            if (filter != null)
                renderer = filter.gameObject.GetComponent<MeshRenderer>();
            else
            {
                spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                    customRenderer = gameObject.GetComponent<CustomRenderer>();
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
                    return spriteRenderer.GetHashCode();
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
