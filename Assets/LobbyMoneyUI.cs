using UnityEngine;
using TMPro;

public class LobbyMoneyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;

    // 每一幀或是啟動時都去讀取最新的本地數據
    void OnEnable()
    {
        RefreshUI();
    }

    void Update()
    {
        // 持續刷新，這樣在商店買完的一瞬間，大廳的錢就會跳動
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "$ " + PlayerData.GetMoney().ToString();
        }
    }
}