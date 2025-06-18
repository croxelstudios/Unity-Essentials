using UnityEngine;
using UnityEngine.Events;

public class CollisionManager_Detector : MonoBehaviour
{
    public event NDCollisionEvent enter = null;
    public event NDCollisionEvent stay = null;
    public event NDCollisionEvent exit = null;

    public delegate void NDCollisionEvent(NDCollision collision);

    void OnCollisionEnter(Collision collision)
    {
        enter?.Invoke(collision.ND());
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        enter?.Invoke(collision.ND());
    }

    void OnCollisionStay(Collision collision)
    {
        stay?.Invoke(collision.ND());
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        stay?.Invoke(collision.ND());
    }

    void OnCollisionExit(Collision collision)
    {
        exit?.Invoke(collision.ND());
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        exit?.Invoke(collision.ND());
    }
}
