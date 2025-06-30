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
    float jumpMaxHeight;
    [SerializeField]
    [OnValueChanged("ValidateData")]
    bool variableHeight;
    [SerializeField]
    [MinValue(0f)]
    [ShowIf("variableHeight")]
    [OnValueChanged("ValidateData")]
    float jumpMinHeight;
    [SerializeField]
    [MinValue(0f)]
    [DisableIf("sameAsFallTime")]
    [OnValueChanged("ValidateData")]
    float timeToApex;
    [SerializeField]
    [OnValueChanged("ValidateData")]
    bool sameAsFallTime;
    [SerializeField]
    [OnValueChanged("ValidateData")]
    bool useGravity;
    [SerializeField]
    [ShowIf("useGravity")]
    [OnValueChanged("ValidateData")]
    GravityDriver gravityDriver;
    [SerializeField]
    [ShowIf("useGravity")]
    [EnableIf("@gravityDriver == GravityDriver.ByFallTime")]
    [OnValueChanged("ValidateData")]
    [MinValue(0f)]
    float fallTime;
    [SerializeField]
    [ShowIf("useGravity")]
    [EnableIf("@gravityDriver == GravityDriver.Manual")]
    [OnValueChanged("ValidateData")]
    [MinValue(0f)]
    float gravity;
    //[SerializeField] 
    //[ReadOnly]
    //MinMaxCurve jumpCurve;
    //TO DO: Rendering the curve in the inspector causes an error when closing and opening the struct's foldout.
    //This error apparently occurs because the inspector loses the reference to the internal
    //properties of the MinMaxCurve when it is reconstructed.

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

    [HideIf("@true")]
    public float heightModifier;
    [HideIf("@true")]
    public float speedModifier;

    public enum GravityDriver { Project, Manual, ByFallTime}

    public JumpSettings(bool variableHeight)
    {
        jumpMaxHeight = 1f;
        this.variableHeight = variableHeight;
        jumpMinHeight = 0.2f;
        timeToApex = 0.5f;
        sameAsFallTime = true;
        useGravity = true;
        fallTime = 0.5f;
        gravityDriver = GravityDriver.Project;
        gravity = 9.8f;
        //jumpCurve = new MinMaxCurve(1f);

        jumpSpeed = 0f;
        jumpGravity = 0f;
        jumpHeavyGravity = 0f;
        timeToMinApex = 0f;

        heightModifier = 1f;
        speedModifier = 1f;

        ValidateData();
    }

    public JumpSettings(float jumpMaxHeight, bool variableHeight, float jumpMinHeight, float timeToApex,
        bool sameAsFallTime, bool useGravity, float fallTime, GravityDriver gravityDriver, float gravity)
    {
        this.jumpMaxHeight = jumpMaxHeight;
        this.variableHeight = variableHeight;
        this.jumpMinHeight = jumpMinHeight;
        this.timeToApex = timeToApex;
        this.sameAsFallTime = sameAsFallTime;
        this.useGravity = useGravity;
        this.fallTime = fallTime;
        this.gravityDriver = gravityDriver;
        this.gravity = gravity;
        //jumpCurve = new MinMaxCurve(1f);

        jumpSpeed = 0f;
        jumpGravity = 0f;
        jumpHeavyGravity = 0f;
        timeToMinApex = 0f;

        heightModifier = 1f;
        speedModifier = 1f;

        ValidateData();
    }

    void ProccessGravity()
    {
        switch (gravityDriver)
        {
            case GravityDriver.Manual:
                fallTime = Mathf.Sqrt(jumpMaxHeight * 2f / gravity);
                break;
            case GravityDriver.Project:
                gravity = Physics.gravity.magnitude;
                fallTime = Mathf.Sqrt(jumpMaxHeight * 2f / gravity);
                break;
            case GravityDriver.ByFallTime:
                gravity = Physics.gravity.magnitude;
                break;
        }
    }

    void ProccessJumpData()
    {
        if (sameAsFallTime)
            timeToApex = fallTime;

        float njumpMaxHeight = jumpMaxHeight * heightModifier;
        float njumpMinHeight = jumpMinHeight * heightModifier;
        float nTimeToApex = timeToApex * Mathf.Sqrt(1f / speedModifier);
        jumpGravity = (2f * njumpMaxHeight) / (nTimeToApex * nTimeToApex);
        jumpSpeed = jumpGravity * nTimeToApex;
        jumpHeavyGravity = (jumpSpeed * jumpSpeed) / (2f * jumpMinHeight);
        timeToMinApex = jumpSpeed / jumpHeavyGravity;
    }

    public MinMaxCurve GetCurve()
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
        return new MinMaxCurve(1f, min, max);
    }

    void UpdateCurve()
    {
        //jumpCurve = GetCurve();
    }

    void ValidateData()
    {
        ProccessGravity();
        ProccessJumpData();
        UpdateCurve();
    }

    public float StartSpeed()
    {
        ProccessGravity();
        ProccessJumpData();
        return jumpSpeed;
    }

    public float Gravity(bool falling, bool jumpButtonPressed)
    {
        ProccessGravity();
        ProccessJumpData();
        if (!useGravity) return 0f;
        else return falling ? (gravity * speedModifier) : ((jumpButtonPressed || (!variableHeight)) ? jumpGravity : jumpHeavyGravity);
    }

    public bool UsesGravity()
    {
        return useGravity && (gravity > 0f);
    }
}
