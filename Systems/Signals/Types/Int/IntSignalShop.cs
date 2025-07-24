using UnityEngine;

public class IntSignalShop : MonoBehaviour
{
    [SerializeField]
    int cost = 3;
    [SerializeField]
    IntSignal moneyHolder = null;
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
