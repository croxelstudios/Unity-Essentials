using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMovementDelay : MonoBehaviour
{
    [SerializeField]
    float maxDistance = 1f;
    [SerializeField]
    float multiplier = 1f;
    [SerializeField]
    float smoothTime = 0.1f;
    [SerializeField]
    bool reverseInputVector = true;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;

    Vector3 currentSpeed;
    Vector3 currentDif;
    Vector3 tmpSpd;

    void Update()
    {
        if (timeMode.IsSmooth())
            OnUpdate(timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed())
            OnUpdate(timeMode.DeltaTime());
    }

    void OnUpdate(float deltaTime)
    {
        Vector3 oldDif = currentDif;

        if (reverseInputVector) currentSpeed = -currentSpeed;
        currentSpeed *= multiplier;
        if (currentSpeed.magnitude > maxDistance) currentSpeed = currentSpeed.normalized * maxDistance;

        currentDif = Vector3.SmoothDamp(oldDif, currentSpeed, ref tmpSpd, smoothTime, Mathf.Infinity, deltaTime);
        transform.Translate(currentDif - oldDif, Space.World);
        currentSpeed = Vector3.zero;
    }

    public void Move(Vector2 movement)
    {
        Move((Vector3)movement);
    }

    public void Move(Vector3 movement)
    {
        currentSpeed += movement;
    }

    public void MoveHorizontal(float movement)
    {
        currentSpeed.x += movement;
    }

    public void MoveVertical(float movement)
    {
        currentSpeed.y += movement;
    }

    public void MoveDepth(float movement)
    {
        currentSpeed.z += movement;
    }
}
