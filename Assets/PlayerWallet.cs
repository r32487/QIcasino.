using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerWallet : NetworkBehaviour
{
    // 這裡設為 Owner 寫入是為了讓你測試方便，如果以後要防作弊再改回 Server
    public NetworkVariable<int> Credits = new NetworkVariable<int>(1000, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    public Action<int> OnCreditsChanged;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            DontDestroyOnLoad(gameObject);
            // 每次出生時，去抓存檔裡的錢
            Credits.Value = PlayerData.GetMoney();
        }

        Credits.OnValueChanged += (oldValue, newValue) => 
        {
            OnCreditsChanged?.Invoke(newValue);
            // 當錢變動時，順便存進存檔
            if (IsOwner) PlayerData.SaveMoney(newValue);
        };
    }

    [ServerRpc]
    public void AddCreditsServerRpc(int amount)
    {
        Credits.Value += amount;
    }

    [ServerRpc]
    public void DeductCreditsServerRpc(int amount)
    {
        if (Credits.Value >= amount) Credits.Value -= amount;
    }
}