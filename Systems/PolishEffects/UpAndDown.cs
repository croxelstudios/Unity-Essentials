using UnityEngine;

public class UpAndDown : MonoBehaviour
{
    [SerializeField]
    Vector3 axis = Vector3.up;
    [SerializeField]
    bool worldSpace = false;
    [SerializeField]
    float amount = 1f;
    public float _amount { get { return amount; } set { amount = value; } }
    [SerializeField]
    [Tooltip("Cycles per second")]
    float speed = 1f;
    public float _speed { get { return speed; } set { speed = value; } }
    [SerializeField]
    [Range(0f, 1f)]
    float startTime = 0f;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;
    [SerializeField]
    bool resetOnEnable = false;

    float currentAngle;
    float currentValue;
    float amountMult;

    public void SetAmountMultiplier(float value)
    {
        amountMult = value;
    }

    void Awake()
    {
        amountMult = 1f;
        if (!resetOnEnable)
        {
            currentAngle = startTime * 360f;
            currentValue = 0f;
        }
        //if (worldSpace) axis = transform.InverseTransformDirection(axis);
    }

    void OnEnable()
    {
        if (resetOnEnable)
        {
            currentAngle = startTime * 360f;
            currentValue = 0f;
        }
    }

    void Update()
    {
        if (timeMode.IsSmooth()) UpdatePosition(timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) UpdatePosition(Time.fixedDeltaTime);
    }

    void UpdatePosition(float deltaTime)
    {
        float dif = SineWave(ref currentAngle, deltaTime * speed) - currentValue;
        currentValue += dif;
        transform.Translate(axis.normalized * dif * amount * amountMult,
            worldSpace ? Space.World : Space.Self);
    }

    float SineWave(ref float currentAngle, float deltaTime)
    {
        currentAngle = Mathf.Repeat(currentAngle + (360f * deltaTime), 360f);
        return Mathf.Sin(currentAngle * Mathf.Deg2Rad);
    }

    public void ResetPosition()
    {
        transform.Translate(-currentValue * axis.normalized * amount * amountMult,
            worldSpace ? Space.World : Space.Self);
        currentAngle = startTime * 360f;
        currentValue = 0f;
    }
}
