using UnityEngine;
using UnityEngine.Events;
using System;

public class TriggerEvents_Collider : MonoBehaviour
{
    [SerializeField]
    ColliderEvent entered = null;
    [SerializeField]
    ColliderEvent exited = null;

    [SerializeField]
    Collider2DEvent entered2D = null;
    [SerializeField]
    Collider2DEvent exited2D = null;

    [Serializable] //TO DO: Implement DXColliderEvent
    class ColliderEvent : UnityEvent<Collider> { }
    [Serializable]
    class Collider2DEvent : UnityEvent<Collider2D> { }

    void OnTriggerEnter(Collider collision)
    {
        entered?.Invoke(collision);
    }

    void OnTriggerExit(Collider collision)
    {
        exited?.Invoke(collision);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        entered2D?.Invoke(collision);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        exited2D?.Invoke(collision);
    }
}
