using UnityEngine;
using Sirenix.OdinInspector;
using static SpeedBehaviour;

public class RectBoundsToTarget : BToTarget<Vector4, Movement4DPath, Vector4>
{
    [SerializeField]
    bool leftOrWidth = true;
    [SerializeField]
    bool topOrHeight = true;
    [SerializeField]
    bool right = true;
    [SerializeField]
    bool bottom = true;
    [SerializeField]
    [ShowIf("leftOrWidth")]
    DXFloatEvent leftOrWidthValue = null;
    [SerializeField]
    [ShowIf("topOrHeight")]
    DXFloatEvent topOrHeightValue = null;
    [SerializeField]
    [ShowIf("right")]
    DXFloatEvent rightValue = null;
    [SerializeField]
    [ShowIf("bottom")]
    DXFloatEvent bottomValue = null;
    [SerializeField]
    DXFloatEvent speedPercent = null;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override Movement4DPath GetPath()
    {
        Vector4 oPos = Current();
        Vector4 tPos = Target();
        if (speedBehaviour.MoveAway())
            tPos = oPos - (tPos - oPos);

        Movement4DPath path = new Movement4DPath(oPos, tPos);

        return path;
    }

    protected override Vector4 Accelerated(Movement4DPath path, ref DynamicInfo dynamicInfo, float deltaTime)
    {
        float inverseDeltaTime = deltaTime.Reciprocal();
        Vector4 accelerationHalf0 = Vector4.zero;
        if (doAccelerate)
        {
            float accel = speedBehaviour.acceleration * deltaTime * 0.5f;
            //if (keepInStraightPath)
            {
                //TO DO: This is quite complicated because I need to somehow figure out the magnitude
                //of the speed projected in the previous path to avoid the object slowing down on corners.
                //I also need to account for externel forces, which shouldn't deviate the object but should
                //accelerete it or decelerate it by the projected variation vector.
            }
            //else
            {
                accelerationHalf0 = path.Direction(accel);
                dynamicInfo.accelHalf = accelerationHalf0;
            }
        }
        else
        {
            //if (keepInStraightPath)
            {
                //TO DO: This is quite complicated because I need to somehow figure out the magnitude
                //of the speed projected in the previous path to avoid the object slowing down on corners.
                //I also need to account for externel forces, which shouldn't deviate the object but should
                //accelerete it or decelerate it by the projected variation vector.
            }
            //else
            {
                //Calculate both halfs. They might be different.
                float accelMag = speedBehaviour.friction * deltaTime;
                float accelMagHalf1 = accelMag * 0.5f;
                float accelMagHalf2 = accelMagHalf1;
                if (accelMag > dynamicInfo.speed.magnitude)
                {
                    float rest = dynamicInfo.speed.magnitude - accelMagHalf1;
                    if (rest < 0f)
                    {
                        accelMagHalf1 = dynamicInfo.speed.magnitude;
                        accelMagHalf2 = 0f;
                    }
                    else accelMagHalf2 = rest;
                }

                accelerationHalf0 = -dynamicInfo.speed.normalized * accelMagHalf1;
                dynamicInfo.accelHalf = -dynamicInfo.speed.normalized * accelMagHalf2;
            }
        }

        dynamicInfo.speed += accelerationHalf0;
        Vector4 spd = dynamicInfo.speed * deltaTime;

        //Limit speed by maximum speed
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        if (spd.magnitude > maxDTSpeed)
            spd = spd.normalized * maxDTSpeed;

        //Limit speed by target point
        float moveAmount = path.magnitude;
        if ((spd.magnitude > moveAmount) && (targetMode != TargetMode.NeverStop))
            spd = spd.normalized * moveAmount;

        //Add the other acceleration half to the global speed for use in next frame
        dynamicInfo.speed += dynamicInfo.accelHalf;

        return spd;
    }

    protected override Vector4 SmoothDamp(Movement4DPath path, ref DynamicInfo dynamicInfo, float deltaTime)
    {
        //TO DO: Doesn't work with keepinpath feature
        return path.SmoothDamp(ref dynamicInfo.speed,
            speedBehaviour.smoothTime, speedBehaviour.maxSpeed, deltaTime)
            - path.origin;
    }

    protected override Vector4 LerpSmooth(Movement4DPath path, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        return path.Displacement(
            Mathf.Min(0f.LerpSmooth(path.magnitude, speedBehaviour.decay, deltaTime), maxDTSpeed));
    }

    protected override Vector4 Teleport(Movement4DPath path, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed;
        float moveAmount = path.magnitude;
        if ((moveAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            moveAmount = maxDTSpeed;
        return path.Displacement(moveAmount);
    }

    protected override Vector4 Linear(Movement4DPath path, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        float moveAmount = path.magnitude;
        if (targetMode == TargetMode.StopAtMargin)
            moveAmount -= margin;
        if ((moveAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            moveAmount = maxDTSpeed;
        return path.Displacement(moveAmount);
    }

    protected override void Execute(Vector4 speedPerThisFrame, float deltaTime)
    {
        float unitsPerSecondSpeed = speedPerThisFrame.magnitude * deltaTime.Reciprocal();

        CheckStartStop(unitsPerSecondSpeed);

        if ((unitsPerSecondSpeed > 0f) || sendWhenZeroToo)
        {
            //Calculate and send percent of speed from zero to max speed for things like animation syncing
            float percent = (speedBehaviour.speedMode == SpeedMode.Teleport) ?
                speedPerThisFrame.magnitude / speedBehaviour.unsignedMaxSpeed :
                unitsPerSecondSpeed / speedBehaviour.unsignedMaxSpeed;
            speedPercent?.Invoke(percent);

            Vector4 direction = speedPerThisFrame.normalized;

            //Calculate and send vector with direction and amount of speed
            Vector4 result = direction * unitsPerSecondSpeed;
            Vector4 r = sendFrameMovement ? speedPerThisFrame : result;

            if (leftOrWidth) leftOrWidthValue?.Invoke(r.x);
            if (topOrHeight) topOrHeightValue?.Invoke(r.y);
            if (right) rightValue?.Invoke(r.z);
            if (bottom) bottomValue?.Invoke(r.w);

            if (applyInTransform)
                Apply(speedPerThisFrame, locally);
        }
    }

    public override void Set(Vector4 target, bool isLocal = false)
    {
        Vector4 dif = target - Current();
        Apply(dif, isLocal);
        ResetSpeed();
    }

    public override void Apply(Vector4 speed, bool isLocal = false)
    {
        RectTransform tr = origin as RectTransform;
        if (leftOrWidth)
            AddLeftOrWidth(tr, speed.x);
        if (topOrHeight)
            AddTopOrHeight(tr, speed.y);
        if (right)
            AddRight(tr, speed.z);
        if (bottom)
            AddBottom(tr, speed.w);
    }

    public override Vector4 GetGlobal(Transform tr)
    {
        RectTransform rt = tr as RectTransform;
        return new Vector4(GetLeftOrWidth(rt), GetTopOrHeight(rt), GetRight(rt), GetBottom(rt));
    }

    public override Vector4 ToLocal(Vector4 value)
    {
        //TO DO
        return value;
    }

    public override void UpdateSpeed(ref Vector4 speed,
        Vector4 accelHalf, Vector4 prev, float deltaTime)
    {
        speed = ((Current() - prev) / deltaTime) + accelHalf;
    }

    bool IsHorizontalCompacted(RectTransform tr)
    {
        return tr.anchorMin.x == tr.anchorMax.x;
    }

    bool IsVerticalCompacted(RectTransform tr)
    {
        return tr.anchorMin.y == tr.anchorMax.y;
    }

    float GetLeftOrWidth(RectTransform tr)
    {
        return IsHorizontalCompacted(tr) ? tr.sizeDelta.x : tr.offsetMin.x;
    }

    float GetTopOrHeight(RectTransform tr)
    {
        return IsVerticalCompacted(tr) ? tr.sizeDelta.y : tr.offsetMax.y;
    }

    float GetRight(RectTransform tr)
    {
        return tr.offsetMax.x;
    }

    float GetBottom(RectTransform tr)
    {
        return tr.offsetMin.y;
    }

    //TO DO: This needs to be improved and generalized for more cases.
    // This will fail if anchors are different in each rect transform.
    void SetLeftOrWidth(RectTransform tr, float value)
    {
        if (IsHorizontalCompacted(tr))
            tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
        else
            tr.offsetMin = new Vector2(value, tr.offsetMin.y);
    }

    void SetTopOrHeight(RectTransform tr, float value)
    {
        if (IsVerticalCompacted(tr))
            tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
        else
            tr.offsetMax = new Vector2(tr.offsetMax.x, value);
    }

    void SetRight(RectTransform tr, float value)
    {
        tr.offsetMax = new Vector2(value, tr.offsetMax.y);
    }

    void SetBottom(RectTransform tr, float value)
    {
        tr.offsetMin = new Vector2(tr.offsetMin.x, value);
    }

    void AddLeftOrWidth(RectTransform tr, float value)
    {
        SetLeftOrWidth(tr, GetLeftOrWidth(tr) + value);
    }

    void AddTopOrHeight(RectTransform tr, float value)
    {
        SetTopOrHeight(tr, GetTopOrHeight(tr) + value);
    }

    void AddRight(RectTransform tr, float value)
    {
        SetRight(tr, GetRight(tr) + value);
    }

    void AddBottom(RectTransform tr, float value)
    {
        SetBottom(tr, GetBottom(tr) + value);
    }
}
