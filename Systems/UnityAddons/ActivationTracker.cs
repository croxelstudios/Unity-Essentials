using System;
using System.Collections.Generic;
using UnityEngine;

public class ActivationTracker : MonoBehaviour
{
    static Dictionary<GameObject, ActivationTracker> trackers;
    List<Action> deleteAfter;

    event Action activation;

    void OnEnable()
    {
        activation?.Invoke();
        if (!deleteAfter.IsNullOrEmpty())
        {
            foreach (Action action in deleteAfter)
                activation -= action;
            deleteAfter.Clear();
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
        ActivationTracker tracker;
        trackers = trackers.CreateIfNull();
        if (!trackers.TryGetValue(gameObject, out tracker))
        {
            tracker = gameObject.AddComponent<ActivationTracker>();
            trackers.Add(gameObject, tracker);
        }
        tracker.activation += action;
        if (deleteAfter)
            tracker.deleteAfter = tracker.deleteAfter.CreateAdd(action);
    }
}
