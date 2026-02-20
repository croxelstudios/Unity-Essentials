using Sirenix.OdinInspector;
using UnityEngine;
using System;

[Serializable]
public struct SpeedBehaviour
{
    public SpeedMode speedMode;
    [ShowIf("speedMode", SpeedMode.SmoothDamp)]
    public float smoothTime;
    [ShowIf("speedMode", SpeedMode.LerpSmooth)]
    public float decay;
    [SerializeField]
    [ShowIf("@(speedMode == SpeedMode.Accelerated) || (speedMode == SpeedMode.SmoothDamp)")]
    bool accountForCurrentSpeed;
    [Tooltip("Start speed in units per second")]
    [ShowIf("@(speedMode == SpeedMode.Linear) || (speedMode == SpeedMode.LerpSmooth)")]
    public float speed;
    [ShowIf("speedMode", SpeedMode.Accelerated)]
    public float acceleration;
    [SerializeField]
    [ShowIf("speedMode", SpeedMode.Accelerated)]
    [Tooltip("Friction multiplier relative to acceleration. Applied when doAccelerate is false. Greater = stops faster")]
    float frictionBias;
    [Tooltip("Maximum movement speed in units per second")]
    [ShowIf("@(speedMode == SpeedMode.Accelerated) || (speedMode == SpeedMode.SmoothDamp) || (speedMode == SpeedMode.Teleport)")]
    public float maxSpeed;
    [SerializeField]
    [ShowIf("speedMode", SpeedMode.Accelerated)]
    [Tooltip("Wether the object is accelerating towards the target or slowly stopping")]
    bool _doAccelerate;
    public bool doAccelerate { get { return _doAccelerate; } set { _doAccelerate = value; } }
    public float friction { get { return acceleration + frictionBias; } }
    public float unsignedMaxSpeed { get { return Mathf.Abs(GetSpeed()); } }

    public SpeedBehaviour(SpeedMode speedMode)
    {
        this.speedMode = speedMode;
        smoothTime = 0.1f;
        decay = 5f;
        accountForCurrentSpeed = true;
        speed = 1f;
        maxSpeed = Mathf.Infinity;
        acceleration = 1f;
        frictionBias = 0f;
        _doAccelerate = true;
    }

    public SpeedBehaviour(SpeedMode speedMode, float smoothTime, float decay, bool accountForCurrentSpeed,
        float speed, float maxSpeed, float acceleration, float frictionBias, bool doAccelerate)
    {
        this.speedMode = speedMode;
        this.smoothTime = smoothTime;
        this.decay = decay;
        this.accountForCurrentSpeed = accountForCurrentSpeed;
        this.speed = speed;
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
        this.frictionBias = frictionBias;
        _doAccelerate = doAccelerate;
    }

    public enum SpeedMode { Linear, Accelerated, SmoothDamp, LerpSmooth, Teleport }

    public float GetSpeed()
    {
        switch (speedMode)
        {
            case SpeedMode.Accelerated:
            case SpeedMode.SmoothDamp:
            case SpeedMode.Teleport:
                return maxSpeed;
            default:
                return speed;
        }
    }

    public void SetMaxSpeed(float speed)
    {
        switch (speedMode)
        {
            case SpeedMode.Accelerated:
            case SpeedMode.SmoothDamp:
            case SpeedMode.Teleport:
                maxSpeed = speed;
                break;
            default:
                this.speed = speed;
                break;
        }
    }

    public bool IsInstantaneous()
    {
        switch (speedMode)
        {
            case SpeedMode.Linear:
                return speed >= Mathf.Infinity;
            case SpeedMode.SmoothDamp:
                return smoothTime <= 0f;
            case SpeedMode.Teleport:
                return true;
            default:
                return false;
        }
    }

    public bool IsDynamic()
    {
        return (speedMode == SpeedMode.Accelerated) || (speedMode == SpeedMode.SmoothDamp) || (speedMode == SpeedMode.LerpSmooth);
    }

    public bool AffectedByCurrentSpeed()
    {
        return ((speedMode == SpeedMode.Accelerated) || (speedMode == SpeedMode.SmoothDamp)) && accountForCurrentSpeed;
    }

    public bool MoveAway()
    {
        switch (speedMode)
        {
            case SpeedMode.Linear:
                return (maxSpeed * speed) < 0f;
            case SpeedMode.Accelerated:
                return (maxSpeed * acceleration) < 0f;
            case SpeedMode.LerpSmooth:
                return speed < 0f;
            default:
                return maxSpeed < 0f;
        }
    }
}
