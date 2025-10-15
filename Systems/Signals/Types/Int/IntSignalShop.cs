using UnityEngine;

public class IntSignalShop : MonoBehaviour
{
    [SerializeField]
    int _cost = 3;
    public int cost { get { return _cost; } set { _cost = value; } }
    [SerializeField]
    bool subtractAmount = true;
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
            if (subtractAmount) moneyHolder.Subtract(cost);
            successPurchase?.Invoke();
        }
        else failPurchase?.Invoke();
    }
}
