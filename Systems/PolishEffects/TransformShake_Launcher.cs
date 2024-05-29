using UnityEngine;

public class TransformShake_Launcher : BRemoteLauncher
{
    TransformShake[] shakes;

    [SerializeField]
    [Min(0.001f)]
    float amount = 1f;
    [SerializeField]
    float smooth = 0.1f;
    [SerializeField]
    [Min(0.02f)]
    float _targetChangeFrecuency = 0.02f;
    public float targetChangeFrecuency
    {
        get { return _targetChangeFrecuency; }
        set { _targetChangeFrecuency = value; }
    }

    void Awake()
    {
        FillArrayAwake(ref shakes);
    }

    public void Shake(float time)
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref shakes);
            foreach (TransformShake shake in shakes)
                if (shake != null) shake.Shake(time, targetChangeFrecuency, amount, smooth);
        }
    }
}
