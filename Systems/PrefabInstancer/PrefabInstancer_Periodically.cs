using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PrefabInstancer_Periodically : PrefabInstancer
{
    [SerializeField]
    [Min(0.02f)]
    float seconds = 0.02f;
    [SerializeField]
    int maxEntityCount = 5;
    [SerializeField]
    DXEvent entitySpawned = null;

    Coroutine co;

    void OnEnable()
    {
        co = StartCoroutine(LaunchPeriodicEvent());
    }

    void OnDisable()
    {
        if (co != null) StopCoroutine(co);
    }

    public void SetSeconds(float newSeconds) //TO DO: Should be a variable setter
    {
        seconds = newSeconds;
    }

    IEnumerator LaunchPeriodicEvent()
    {
        while (true)
        {
            if (entities == null) entities = new List<SpawnedEntity>();
            if (entities.Count < maxEntityCount)
            {
                InstantiateRandom();
                entitySpawned?.Invoke();
            }
            else while (entities.Count >= maxEntityCount) yield return null;

            yield return new WaitForSeconds(seconds);
        }
    }
}
