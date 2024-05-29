using UnityEngine;
using System.Collections;

public class DartMechanics : MonoBehaviour
{
    [SerializeField]
    Vector3 dartDirection = Vector3.forward;
    [SerializeField]
    float minForce = 1f;
    [SerializeField]
    float maxForce = 5f;
    [SerializeField]
    float chargeTime = 1f;
    [SerializeField]
    AnimationCurve chargeCurve = null;
    [SerializeField]
    bool pingPongCharge = false;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;
    [SerializeField]
    DXFloatEvent currentCharge = null;

    NDRigidbody rigid;
    Coroutine co;
    float _currentForceSecond;

    float currentForceSecond
    {
        get
        {
            if (pingPongCharge) return Mathf.PingPong(_currentForceSecond, 1f);
            else return _currentForceSecond;
        }
        set
        {
            _currentForceSecond = value;
            if (pingPongCharge) _currentForceSecond = Mathf.Repeat(_currentForceSecond, 2f);
            else _currentForceSecond = Mathf.Repeat(_currentForceSecond, 1f);
            currentForce = chargeCurve.Evaluate(currentForceSecond);
        }
    }
    float currentForce;

    void Awake()
    {
        rigid = NDRigidbody.GetNDRigidbodyFrom(gameObject);
    }

    public void StartCharging()
    {
        ResetForce();
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Charge());
    }

    public void StopCharging()
    {
        if (co != null) StopCoroutine(co);
    }

    public void ResumeCharging()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Charge());
    }

    public void Launch()
    {
        if (co != null) StopCoroutine(co);

        rigid.isKinematic = false;
        if (rigid.is2D) rigid.rigid2.gravityScale = 1f;
        else rigid.rigid3.useGravity = true;

        if (rigid.is2D) rigid.rigid2.AddRelativeForce(
            ((Vector2)dartDirection).normalized * Mathf.Lerp(minForce, maxForce, currentForce));
        else rigid.rigid3.AddRelativeForce(dartDirection.normalized * Mathf.Lerp(minForce, maxForce, currentForce));
    }

    public void ResetRigidbody()
    {
        rigid.isKinematic = true;
        if (rigid.is2D) rigid.rigid2.gravityScale = 0f;
        else rigid.rigid3.useGravity = false;
    }

    public void ResetForce()
    {
        currentForceSecond = 0f;
    }

    IEnumerator Charge()
    {
        currentForceSecond = _currentForceSecond + (timeMode.DeltaTime() / chargeTime);
        currentCharge?.Invoke(currentForce);
        yield return timeMode.DeltaTime();
        co = StartCoroutine(Charge());
    }
}
