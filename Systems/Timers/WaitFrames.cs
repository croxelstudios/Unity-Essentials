using System.Collections;
using UnityEngine;

public class WaitFrames : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    protected int frames = 1;
    [SerializeField]
    bool fixedTime = false;
    [SerializeField]
    DXEvent actions = null;
    [SerializeField]
    StartMode startMode = StartMode.onEnable;

    enum StartMode { onStart, onEnable, whenCalled }

    void OnEnable()
    {
        if (startMode == StartMode.onEnable)
            StartRoutine();
    }

    void Start()
    {
        if (startMode == StartMode.onStart)
            StartRoutine();
    }

    void OnDisable()
    {
        StopRoutine();
    }

    public void StopRoutine()
    {
        StopAllCoroutines();
    }

    public virtual void StartRoutine()
    {
        if (this.IsActiveAndEnabled() && gameObject.activeInHierarchy)
        {
            if (frames > 0) StartCoroutine(Alarm());
            else StartCoroutine(AfterUpdate());
        }
    }

    public void SetFrames(int newFrames)
    {
        frames = newFrames;
    }

    IEnumerator Alarm()
    {
        if (fixedTime) yield return WaitFor.FixedFrames(frames);
        else yield return WaitFor.Frames(frames);
        actions?.Invoke();
    }

    IEnumerator AfterUpdate()
    {
        if (fixedTime) yield return new WaitForFixedUpdate();
        else yield return null;
        actions?.Invoke();
    }
}
