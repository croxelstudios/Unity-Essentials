using UnityEngine;

public class FloatSignalShop : MonoBehaviour
{
    [SerializeField]
    float cost = 3;
    [SerializeField]
    FloatSignal moneyHolder = null;
    [SerializeField]
    DXEvent successPurchase = null;
    [SerializeField]
    DXEvent failPurchase = null;

    public void TryBuy()
    {
        if (moneyHolder.currentValue >= cost)
        {
            moneyHolder.Subtract(cost);
            successPurchase?.Invoke();
        }
        else failPurchase?.Invoke();
    }
}
