using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleEffectRadius : MonoBehaviour
{
    [SerializeField]
    Mode mode = Mode.ScaleLifetime;
    [SerializeField]
    float lifetime = 0.1f;
    [SerializeField]
    float speed = -1.428571f;
    [SerializeField]
    float size = 0.1f;
    [SerializeField]
    float shapeRadius = 1f;
    [Space]
    [SerializeField]
    float _radius = 1f;
    public float radius { get { return _radius; } set { _radius = value; } }

    ParticleSystem p;
    enum Mode { ScaleLifetime, ScaleSpeed }

    void OnEnable()
    {
        p = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        ParticleSystem.MainModule main = p.main;
        ParticleSystem.ShapeModule shape = p.shape;
        switch (mode)
        {
            case Mode.ScaleSpeed:
                main.startSpeed = speed * radius;
                break;
            default:
                main.startLifetime = lifetime * radius;
                break;
        }
        main.startSize = size * radius;
        shape.radius = shapeRadius * radius;
    }

    public void SetRadius(float value)
    {
        radius = value;
    }
}
