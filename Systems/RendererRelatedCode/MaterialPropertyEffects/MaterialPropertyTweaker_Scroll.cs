using UnityEngine;

[RequireComponent(typeof(MaterialPropertyTweaker))]
public class MaterialPropertyTweaker_Scroll : MonoBehaviour
{
    [SerializeField]
    bool isCustom2DVector = false;
    [Range(-180f, 180f)]
    public float angle = 0f;
    [SerializeField]
    float _moveSpeed = 1f;
    public float moveSpeed
    {
        get { return _moveSpeed; }
        set { _moveSpeed = value; }
    }
    public Vector2 referenceOffset = Vector2.zero;
    [SerializeField]
    RenderingTimeMode timeMode = RenderingTimeMode.Update;

    MaterialPropertyTweaker tweaker;
    Vector2 direction;
    Vector2 currentOffset;

    void OnEnable()
    {
        tweaker = GetComponent<MaterialPropertyTweaker>();
    }

    void Update()
    {
        UpdateBehaviour();
    }

    void UpdateBehaviour()
    {
        float deltaTime = timeMode.DeltaTime();

        float angleRad = Mathf.Deg2Rad * angle;
        direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * moveSpeed;

        currentOffset.x += deltaTime * direction.x;
        currentOffset.y += deltaTime * direction.y;
        currentOffset.x %= 1f;
        currentOffset.y %= 1f;
        Vector4 ST = tweaker.GetVector();
        if (isCustom2DVector)
        {
            ST.x = currentOffset.x;
            ST.y = currentOffset.y;
        }
        else
        {
            ST.z = currentOffset.x;
            ST.w = currentOffset.y;
        }
        tweaker.SetVector(ST);
    }
}
