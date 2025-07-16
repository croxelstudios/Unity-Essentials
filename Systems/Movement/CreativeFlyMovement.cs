using UnityEngine;

public class CreativeFlyMovement : MonoBehaviour
{
    [SerializeField]
    Transform referenceTransform = null;
    [SerializeField]
    float speed = 5f;
    [SerializeField]
    Vector3 downNormal = Vector3.down;
    [SerializeField]
    Vector3 planeForward = Vector3.forward;
    TimeMode timeMode = TimeMode.FixedUpdate;

    public void Move(Vector2 input)
    {
        if (this.IsActiveAndEnabled())
        {
            referenceTransform.TransformPlaneData(downNormal, planeForward,
                out Vector3 normal, out Vector3 up);

            Vector2 spd = input * speed * timeMode.DeltaTime();

            transform.Translate(spd.InterpretVector2(normal, up), Space.World);
        }
    }

    public void MoveVertical(float input)
    {
        if (this.IsActiveAndEnabled())
        {
            referenceTransform.TransformPlaneData(downNormal, planeForward,
                out Vector3 normal, out Vector3 up);
            Vector3 spd = input * speed * timeMode.DeltaTime() * (-normal);
            transform.Translate(spd, Space.World);
        }
    }
}
