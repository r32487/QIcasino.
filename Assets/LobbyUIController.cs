using UnityEngine;
using TMPro;

public class LobbyUIController : MonoBehaviour
{
    public TextMeshProUGUI lobbyMoneyText;

    void Start()
    {
        UpdateLobbyUI();
    }

    // 每次載入大廳時刷新數字
    public void UpdateLobbyUI()
    {
        if (lobbyMoneyText != null)
            lobbyMoneyText.text = "剩餘資金: " + GameDataManager.TotalMoney;
    }
}