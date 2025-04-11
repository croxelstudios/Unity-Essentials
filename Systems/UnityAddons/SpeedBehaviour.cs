using Micosmo.SensorToolkit;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public struct SpeedBehaviour
{
    public SpeedMode speedMode;
    [ShowIf("@speedMode == SpeedMode.SmoothDamp")]
    public float smoothTime;
    [ShowIf("@speedMode == SpeedMode.LerpSmooth")]
    public float decay;
    [ShowIf("@speedMode == SpeedMode.Accelerated || speedMode == SpeedMode.SmoothDamp")]
    public bool accountForCurrentSpeed;
    [Tooltip("Start speed in units per second")]
    [ShowIf("@speedMode == SpeedMode.Linear || speedMode == SpeedMode.LerpSmooth")]
    public float speed;
    [Tooltip("Maximum movement speed in units per second")]
    [ShowIf("@speedMode == SpeedMode.Accelerated || speedMode == SpeedMode.SmoothDamp || speedMode == SpeedMode.Teleport")]
    public float maxSpeed;
    [ShowIf("@speedMode == SpeedMode.Accelerated")]
    public float acceleration;
    [ShowIf("@speedMode == SpeedMode.Accelerated")]
    [Tooltip("Friction multiplier relative to acceleration. Applied when doAccelerate is false. Greater = stops faster")]
    public float frictionBias;
    [ShowIf("@speedMode == SpeedMode.Accelerated")]
    [Tooltip("Wether the object is accelerating towards the target or slowly stopping")]
    bool _doAccelerate;
    public bool doAccelerate { get { return _doAccelerate; } set { _doAccelerate = value; } }

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
}
