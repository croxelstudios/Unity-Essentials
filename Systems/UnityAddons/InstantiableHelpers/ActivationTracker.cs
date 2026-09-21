using System;
using System.Collections.Generic;
using UnityEngine;

public class ActivationTracker : MonoBehaviour
{
    static Dictionary<GameObject, ActivationTracker> trackers;
    static Dictionary<Component, ActivationTracker> cTrackers;
    List<Action> deleteAfter_activation;
    List<Action> deleteAfter_deactivation;

    event Action activation;
    event Action deactivation;

    void OnEnable()
    {
        activation?.Invoke();
        if (!deleteAfter_activation.IsNullOrEmpty())
        {
            foreach (Action action in deleteAfter_activation)
                activation -= action;
            deleteAfter_activation.Clear();
        }
    }

    void OnDisable()
    {
        deactivation?.Invoke();
        if (!deleteAfter_deactivation.IsNullOrEmpty())
        {
            foreach (Action action in deleteAfter_deactivation)
                deactivation -= action;
            deleteAfter_deactivation.Clear();
        }
    }

    void OnDestroy()
    {
        trackers.SmartRemove(gameObject);
    }

    public static void TrackActivation(Transform transform, Action action, bool deleteAfter = true)
    {
        TrackActivation(transform.gameObject, action, deleteAfter);
    }

    public static void TrackActivation(GameObject gameObject, Action action, bool deleteAfter = true)
    {
        trackers = trackers.CreateIfNull();
        if (!trackers.TryGetValue(gameObject, out ActivationTracker tracker))
        {
            tracker = gameObject.AddComponent<ActivationTracker>();
            trackers.Add(gameObject, tracker);
        }
        tracker.activation += action;
        if (deleteAfter)
            tracker.deleteAfter_activation = tracker.deleteAfter_activation.CreateAdd(action);
    }

    public static void TrackDeactivation(Transform transform, Action action, bool deleteAfter = true)
    {
        TrackDeactivation(transform.gameObject, action, deleteAfter);
    }

    public static void TrackDeactivation(GameObject gameObject, Action action, bool deleteAfter = true)
    {
        trackers = trackers.CreateIfNull();
        if (!trackers.TryGetValue(gameObject, out ActivationTracker tracker))
        {
            tracker = gameObject.AddComponent<ActivationTracker>();
            trackers.Add(gameObject, tracker);
        }
        tracker.deactivation += action;
        if (deleteAfter)
            tracker.deleteAfter_deactivation = tracker.deleteAfter_deactivation.CreateAdd(action);
    }

    public static void TrackActivation(Transform transform, Action<Transform> action, bool deleteAfter = true)
    {
        TrackActivation(transform.gameObject, action, deleteAfter);
    }

    public static void TrackActivation(GameObject gameObject, Action<Transform> action, bool deleteAfter = true)
    {
        void Wrapped() => action(gameObject.transform);
        TrackActivation(gameObject, Wrapped, deleteAfter);
    }

    public static void TrackDeactivation(Transform transform, Action<Transform> action, bool deleteAfter = true)
    {
        TrackDeactivation(transform.gameObject, action, deleteAfter);
    }

    public static void TrackDeactivation(GameObject gameObject, Action<Transform> action, bool deleteAfter = true)
    {
        void Wrapped() => action(gameObject.transform);
        TrackDeactivation(gameObject, Wrapped, deleteAfter);
    }

    public static void TrackActivation(Transform transform, Action<GameObject> action, bool deleteAfter = true)
    {
        TrackActivation(transform.gameObject, action, deleteAfter);
    }

    public static void TrackActivation(GameObject gameObject, Action<GameObject> action, bool deleteAfter = true)
    {
        void Wrapped() => action(gameObject);
        TrackActivation(gameObject, Wrapped, deleteAfter);
    }

    public static void TrackDeactivation(Transform transform, Action<GameObject> action, bool deleteAfter = true)
    {
        TrackDeactivation(transform.gameObject, action, deleteAfter);
    }

    public static void TrackDeactivation(GameObject gameObject, Action<GameObject> action, bool deleteAfter = true)
    {
        void Wrapped() => action(gameObject);
        TrackDeactivation(gameObject, Wrapped, deleteAfter);
    }

    public static void TrackActivation<T>(T component, Action action, bool deleteAfter = true) where T : Component
    {
        cTrackers = cTrackers.CreateIfNull();
        if (!cTrackers.TryGetValue(component, out ActivationTracker tracker))
        {
            tracker = component.gameObject.AddComponent<ActivationTracker>();
            cTrackers.Add(component, tracker);
        }
        tracker.activation += action;
        if (deleteAfter)
            tracker.deleteAfter_activation = tracker.deleteAfter_activation.CreateAdd(action);
    }

    public static void TrackDeactivation<T>(T component, Action action, bool deleteAfter = true) where T : Component
    {
        cTrackers = cTrackers.CreateIfNull();
        if (!cTrackers.TryGetValue(component, out ActivationTracker tracker))
        {
            tracker = component.gameObject.AddComponent<ActivationTracker>();
            cTrackers.Add(component, tracker);
        }
        tracker.deactivation += action;
        if (deleteAfter)
            tracker.deleteAfter_deactivation = tracker.deleteAfter_deactivation.CreateAdd(action);
    }

    public static void TrackActivation<T>(T component, Action<T> action, bool deleteAfter = true) where T : Component
    {
        void Wrapped() => action(component);
        TrackActivation(component, Wrapped, deleteAfter);
    }

    public static void TrackDeactivation<T>(T component, Action<T> action, bool deleteAfter = true) where T : Component
    {
        void Wrapped() => action(component);
        TrackDeactivation(component, Wrapped, deleteAfter);
    }
}
