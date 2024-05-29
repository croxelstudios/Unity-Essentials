using UnityEngine;
using UnityEngine.UI;
using System;

[DefaultExecutionOrder(1000)]
public class DiscreteRenderer : MonoBehaviour
{
    [SerializeField]
    MovementScaleProperties movement = new MovementScaleProperties();
    [SerializeField]
    RotationProperties rotation = new RotationProperties();
    [SerializeField]
    MovementScaleProperties scale = new MovementScaleProperties();
    [SerializeField]
    bool updateChildrenRenderers = false;

    DiscreteRenderer[] childRenderers;
    bool isRendering;
    Graphic gr;
    Vector3 prevPos;
    Vector3 prevRotation; //TO DO: Maybe use quaternions
    Vector3 prevScale;
    bool wasRendered;
    bool childRenderersFilled;

    [HideInInspector]
    public bool lookForChilds = true;

    void Awake()
    {
        Renderer rend = GetComponent<Renderer>();
        gr = GetComponent<Graphic>(); //TO DO: Doesn't work on UI
        if (rend || gr) isRendering = true;
    }

    void Update()
    {
        if (lookForChilds)
        {
            if (childRenderersFilled)
            {
                if (updateChildrenRenderers)
                    foreach (DiscreteRenderer dr in childRenderers)
                        dr.Settings(movement, rotation, scale);
            }
            else
            {
                Renderer[] rends = GetComponentsInChildren<Renderer>(true);
                Graphic[] grs = GetComponentsInChildren<Graphic>(true);
                childRenderers = new DiscreteRenderer[rends.Length + grs.Length];
                for (int i = 0; i < rends.Length; i++)
                {
                    childRenderers[i] = rends[i].GetComponent<DiscreteRenderer>();
                    if (childRenderers[i] == null)
                    {
                        childRenderers[i] = rends[i].gameObject.AddComponent<DiscreteRenderer>();
                        childRenderers[i].Settings(movement, rotation, scale);
                        childRenderers[i].lookForChilds = false;
                    }
                }
                for (int i = 0; i < grs.Length; i++)
                {
                    childRenderers[i] = grs[i].GetComponent<DiscreteRenderer>();
                    if (childRenderers[i] == null)
                    {
                        childRenderers[grs.Length + i] = grs[i].gameObject.AddComponent<DiscreteRenderer>();
                        childRenderers[grs.Length + i].Settings(movement, rotation, scale);
                        childRenderers[grs.Length + i].lookForChilds = false;
                    }
                }
                childRenderersFilled = true;
            }
        }
        //if (gr) PreparePosition();
    }

    void OnWillRenderObject()
    {
        PreparePosition();
    }

    void OnRenderObject()
    {
        RestorePosition();
    }

    void PreparePosition()
    {
        if (this.IsActiveAndEnabled() && isRendering)
        {
            if (movement.enable)
            {
                prevPos = transform.localPosition;
                if (movement.locally) transform.localPosition =
                        RoundUpPosition(transform.localPosition, movement.gridSize, movement.roundingMode);
                else transform.position =
                        RoundUpPosition(transform.position, movement.gridSize, movement.roundingMode);
            }

            if (rotation.enable)
            {
                prevRotation = transform.localEulerAngles;
                if (rotation.locally) transform.localEulerAngles =
                        RoundUpRotation(transform.localEulerAngles, rotation.angleDivisions, rotation.roundingMode);
                else transform.eulerAngles =
                        RoundUpRotation(transform.eulerAngles, rotation.angleDivisions, rotation.roundingMode);
            }

            if (scale.enable)
            {
                prevScale = transform.localScale;
                if (scale.locally) transform.localScale =
                        RoundUpPosition(transform.localScale, scale.gridSize, scale.roundingMode);
                else transform.localScale = //TO DO: Untested
                        LossyToLocalScale(RoundUpPosition(transform.lossyScale, scale.gridSize, scale.roundingMode), transform, true);
            }
            wasRendered = true;
        }
    }

    void RestorePosition()
    {
        if (this.IsActiveAndEnabled() && isRendering && wasRendered)
        {
            if (movement.enable) transform.localPosition = prevPos;
            if (rotation.enable) transform.localEulerAngles = prevRotation;
            if (scale.enable) transform.localScale = prevScale;
            wasRendered = false;
        }
    }

    public void Settings(MovementScaleProperties movement, RotationProperties rotation, MovementScaleProperties scale)
    {
        this.movement = movement;
        this.rotation = rotation;
        this.scale = scale;
    }

    //void OnGUI()
    //{
    //    switch (Event.current.type)
    //    {
    //        case EventType.Repaint:
    //            PreparePosition();
    //            break;
    //    }
    //}

    Vector3 RoundUpPosition(Vector3 original, Vector3 gridSize, RoundingMode mode = RoundingMode.Floor)
    {
        original.x = gridSize.x > 0 ? Round(original.x / gridSize.x, mode) * gridSize.x : original.x;
        original.y = gridSize.y > 0 ? Round(original.y / gridSize.y, mode) * gridSize.y : original.y;
        original.z = gridSize.z > 0 ? Round(original.z / gridSize.z, mode) * gridSize.z : original.z;
        return original;
    }

    Vector3 RoundUpRotation(Vector3 original, int divisions, RoundingMode mode = RoundingMode.Floor)
    {
        if (divisions <= 0) return original;

        int mul = 360 / divisions;
        original.x = Round(original.x / mul, mode) * mul;
        original.y = Round(original.y / mul, mode) * mul;
        original.z = Round(original.z / mul, mode) * mul;
        return original;
    }
    
    Vector3 LossyToLocalScale(Vector3 lossyScale, Transform transform, bool setScaleOnGlobalAxes = false)
    {
        Vector3 localScale = Vector3.one;
        var m = transform.worldToLocalMatrix;
        if (setScaleOnGlobalAxes)
        {
            m.SetColumn(0, new Vector4(m.GetColumn(0).magnitude, 0f));
            m.SetColumn(1, new Vector4(0f, m.GetColumn(1).magnitude));
            m.SetColumn(2, new Vector4(0f, 0f, m.GetColumn(2).magnitude));
        }
        m.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
        localScale = m.MultiplyPoint(lossyScale);
        return localScale;
    }

    float Round(float original, RoundingMode mode = RoundingMode.Floor)
    {
        switch (mode)
        {
            case RoundingMode.Round:
                return Mathf.Round(original);
            case RoundingMode.Ceil:
                return Mathf.Ceil(original);
            default:
                return Mathf.Floor(original);
        }
    }

    public enum RoundingMode{ Round, Floor, Ceil }

    [Serializable]
    public struct MovementScaleProperties
    {
        public bool enable;
        public bool locally;
        public RoundingMode roundingMode;
        public Vector3 gridSize;

        public MovementScaleProperties(bool enable, bool locally, RoundingMode roundingMode, Vector3 gridSize)
        {
            this.enable = enable;
            this.locally = locally;
            this.roundingMode = roundingMode;
            this.gridSize = gridSize;
        }
    }

    [Serializable]
    public struct RotationProperties
    {
        public bool enable;
        public bool locally;
        public RoundingMode roundingMode;
        public int angleDivisions;

        public RotationProperties(bool enable, bool locally, RoundingMode roundingMode, int angleDivisions)
        {
            this.enable = enable;
            this.locally = locally;
            this.roundingMode = roundingMode;
            this.angleDivisions = angleDivisions;
        }
    }
}
