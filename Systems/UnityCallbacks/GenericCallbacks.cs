using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericCallbacks : MonoBehaviour
{
    public event Action onAwake;
    public event Action onStart;
    public event Action onEnable;
    public event Action onDisable;
    public event Action onDestroy;

    void Awake()
    {
        onAwake?.Invoke();
    }

    void OnEnable()
    {
        onEnable?.Invoke();
    }

    void Start()
    {
        onStart?.Invoke();       
    }

    void OnDisable()
    {
        onDisable?.Invoke();
    }

    void OnDestroy()
    {
        onDestroy?.Invoke();
    }
}
