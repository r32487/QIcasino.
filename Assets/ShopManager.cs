using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public void BuyStarterPack() { AddMoney(5000); }
    public void BuyTycoonPack() { AddMoney(22000); }
    public void BuyGodOfGamblersPack() { AddMoney(40000); }

    void AddMoney(int amount)
    {
        PlayerData.SaveMoney(PlayerData.GetMoney() + amount);
        Debug.Log("[Shop] Purchase Successful");
    }
}