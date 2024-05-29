using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class AndOrOp : MonoBehaviour
{    
    [SerializeField]
    int andArguments = 1;
    [SerializeField]
    bool clampConditionsValue = false;
    [SerializeField]
    DXEvent becomesTrue = null;
    [SerializeField]
    DXEvent becomesFalse = null;

    [DisplayAsString]
    public
        int dictionaryLenght = 0;
    [DisplayAsString]
    public
        int dictionaryTotalLength = 0;

    Dictionary<int, int> argumentCount;

    void OnDisable()
    {

    }

    public void Add(int argument)
    {
        if (this.IsActiveAndEnabled())
        {
            if (argumentCount == null) argumentCount = new Dictionary<int, int>();
            if (!argumentCount.ContainsKey(argument))
            {
                argumentCount.Add(argument, 1);
                if (argumentCount.Count >= andArguments)
                    becomesTrue?.Invoke();
            }
            else if (!clampConditionsValue)
                argumentCount[argument]++;

            dictionaryLenght = argumentCount.Count;
            dictionaryTotalLength = CalculateConditionsLength();
        }
    }

    public void Subtract(int argument)
    {
        if (this.IsActiveAndEnabled() && (argumentCount != null) && argumentCount.ContainsKey(argument))
        {
            argumentCount[argument]--;
            if (argumentCount[argument] <= 0)
            {
                argumentCount.Remove(argument);
                if (argumentCount.Count < andArguments)
                    becomesFalse?.Invoke();
            }

            dictionaryLenght = argumentCount.Count;
            dictionaryTotalLength = CalculateConditionsLength();
        }
    }

    int CalculateConditionsLength()
    {
        int result = 0;

        foreach (KeyValuePair<int, int> value in argumentCount)
        {
            result += value.Value;
        }

        return result;
    }
}
