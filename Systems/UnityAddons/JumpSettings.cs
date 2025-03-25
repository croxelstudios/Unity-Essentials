using Sirenix.OdinInspector;
using System;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[Serializable]
public struct JumpSettings
{
    [SerializeField]
    [MinValue(0f)]
    [OnValueChanged("ValidateData")]
    float jumpMinHeight;
    [SerializeField]
    [MinValue(0f)]
    [OnValueChanged("ValidateData")]
    float jumpMaxHeight;
    [SerializeField]
    [MinValue(0f)]
    [OnValueChanged("ValidateData")]
    float timeToApex;
    [SerializeField]
    [OnValueChanged("ValidateData")]
    bool useGravity;
    [SerializeField]
    [ShowIf("useGravity")]
    [DisableIf("controlWithGravity")]
    [OnValueChanged("ValidateData")]
    [MinValue(0f)]
    float fallTime;
    [SerializeField]
    [OnValueChanged("ValidateData")]
    bool controlWithGravity;
    [SerializeField]
    [ShowIf("useGravity")]
    [EnableIf("controlWithGravity")]
    [MinValue(0f)]
    [OnValueChanged("ValidateData")]
    float gravity;
    [SerializeField]
    [ReadOnly]
    MinMaxCurve jumpCurve;

    [SerializeField]
    [HideInInspector]
    float jumpSpeed;
    [SerializeField]
    [HideInInspector]
    float jumpGravity;
    [SerializeField]
    [HideInInspector]
    float jumpHeavyGravity;
    [SerializeField]
    [HideInInspector]
    float timeToMinApex;

    public JumpSettings(bool useGravity)
    {
        jumpMinHeight = 0.2f;
        jumpMaxHeight = 1f;
        timeToApex = 0.5f;
        timeToApex = 0.5f;
        this.useGravity = useGravity;
        fallTime = 0.5f;
        controlWithGravity = true;
        gravity = 9.8f;
        jumpCurve = new MinMaxCurve(1f, null, null);

        jumpSpeed = 0f;
        jumpGravity = 0f;
        jumpHeavyGravity = 0f;
        timeToMinApex = 0f;

        ValidateData();
    }

    public JumpSettings(float jumpMinHeight, float jumpMaxHeight, float timeToApex,
        bool useGravity, float fallTime, bool controlWithGravity, float gravity)
    {
        this.jumpMinHeight = jumpMinHeight;
        this.jumpMaxHeight = jumpMaxHeight;
        this.timeToApex = timeToApex;
        this.useGravity = useGravity;
        this.fallTime = fallTime;
        this.controlWithGravity = controlWithGravity;
        this.gravity = gravity;
        jumpCurve = new MinMaxCurve(1f, null, null);

        jumpSpeed = 0f;
        jumpGravity = 0f;
        jumpHeavyGravity = 0f;
        timeToMinApex = 0f;

        ValidateData();
    }

    void ProccessGravity()
    {
        jumpGravity = (2f * jumpMaxHeight) / (timeToApex * timeToApex);
        jumpSpeed = jumpGravity * timeToApex;
        jumpHeavyGravity = (jumpSpeed * jumpSpeed) / (2f * jumpMinHeight);
        timeToMinApex = jumpSpeed / jumpHeavyGravity;

        if (controlWithGravity)
            fallTime = Mathf.Sqrt(jumpMaxHeight * 2f / gravity);
        else gravity = (jumpMaxHeight * 2f) / (fallTime * fallTime);
    }

    void UpdateCurve()
    {
        float minFallTime = Mathf.Sqrt(jumpMinHeight * 2f / gravity);
        float totalMinTime = timeToMinApex + minFallTime;
        float fallMinFinalSpd = -minFallTime * gravity;

        float totalTime = timeToApex + fallTime;
        float fallFinalSpd = -fallTime * gravity;

        AnimationCurve min = new AnimationCurve(
            new Keyframe(0f, 0f, jumpSpeed, jumpSpeed),
            new Keyframe(timeToMinApex, jumpMinHeight, 0f, 0f),
            useGravity ? new Keyframe(totalMinTime, 0f, fallMinFinalSpd, fallMinFinalSpd) :
            new Keyframe(totalMinTime, jumpMinHeight, 0f, 0f)
            );
        AnimationCurve max = new AnimationCurve(
            new Keyframe(0f, 0f, jumpSpeed, jumpSpeed),
            new Keyframe(timeToApex, jumpMaxHeight, 0f, 0f),
            useGravity ? new Keyframe(totalTime, 0f, fallFinalSpd, fallFinalSpd) :
            new Keyframe(totalTime, jumpMaxHeight, 0f, 0f)
            );
        jumpCurve = new MinMaxCurve(1f, min, max);
    }

    void ValidateData()
    {
        ProccessGravity();
        UpdateCurve();
    }

    public float StartSpeed()
    {
        return jumpSpeed;
    }

    public float Gravity(bool falling, bool jumpButtonPressed)
    {
        if (!useGravity) return 0f;
        else return falling ? gravity : (jumpButtonPressed ? jumpGravity : jumpHeavyGravity);
    }

    public bool UsesGravity()
    {
        return useGravity && (gravity > 0f);
    }
}
