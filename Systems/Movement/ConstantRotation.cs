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
    bool randomize = false;
    [SerializeField]
    [ShowIf("@randomize")]
    bool randomAxis = true;
    [SerializeField]
    [ShowIf("@randomize")]
    Vector2 speedRange = new Vector2(0f, 1f);
    [SerializeField]
    protected TimeMode timeMode = TimeMode.FixedUpdate;

    void OnEnable()
    {
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
        transform.Rotate(axis * speed * deltaTime);
    }
}
