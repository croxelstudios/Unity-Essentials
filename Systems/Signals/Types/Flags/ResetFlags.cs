using UnityEngine;

public class ResetFlags : MonoBehaviour
{
    [SerializeField]
    Flag[] flags = null;
    [SerializeField]
    bool onEnable = false;

    void OnEnable()
    {
        if (onEnable)
            DoReset();
    }

    public void DoReset()
    {
        for (int i = 0; i < flags.Length; i++)
            flags[i].DoReset();
    }
}
