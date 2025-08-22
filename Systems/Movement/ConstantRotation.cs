using UnityEngine;
using Sirenix.OdinInspector;

public class ConstantRotation : MonoBehaviour
{
    [SerializeField]
    float _speed = 50f;
    public float speed { get { return _speed; } set { _speed = value; } }
    [SerializeField]
    protected Vector3 axis = Vector3.back;
    [SerializeField]
    bool worldSpace = false;
    [SerializeField]
    bool randomize = false;
    [SerializeField]
    [ShowIf("@randomize")]
    bool randomAxis = true;
    [SerializeField]
    [ShowIf("@randomize")]
    Vector2 speedRange = new Vector2(0f, 1f);
    [SerializeField]
    bool deactivationResetsRotation = false;
    [SerializeField]
    protected TimeMode timeMode = TimeMode.FixedUpdate;

    Quaternion accumulatedRotation;

    void OnEnable()
    {
        accumulatedRotation = Quaternion.identity;
        if (randomize)
        {
            if (randomAxis)
            {
                Vector3 randomAxis = Vector3.one.GetRandom();
                axis = new Vector3(randomAxis.x, randomAxis.y, randomAxis.z);
            }
            speed = Random.Range(speedRange.x, speedRange.y);
        }
        axis = axis.normalized;
    }

    void OnDisable()
    {
        if (deactivationResetsRotation)
            transform.Rotate(Quaternion.Inverse(accumulatedRotation).eulerAngles);
    }

    void Update()
    {
        if (timeMode.IsSmooth())
            UpdateActions(timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed())
            UpdateActions(timeMode.DeltaTime());
    }

    void UpdateActions(float deltaTime)
    {
        Vector3 finalRotation = axis * speed * deltaTime;
        accumulatedRotation = Quaternion.Euler(finalRotation) * accumulatedRotation;
        transform.Rotate(finalRotation, worldSpace ? Space.World : Space.Self);
    }

    public void Restart()
    {
        if (this.IsActiveAndEnabled())
        {
            OnDisable();
            OnEnable();
        }
    }
}
