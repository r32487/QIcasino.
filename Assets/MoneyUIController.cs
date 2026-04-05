using UnityEngine;
using TMPro;
using Fusion;

public class MoneyUIController : MonoBehaviour
{
    public TextMeshProUGUI moneyText; 
    public NetworkGameManager gameManager; 

    void Update()
    {
        if (gameManager == null || gameManager.Runner == null || !gameManager.Runner.IsRunning) return;

        // 👈 修正紅字：直接檢查 Key，不比對字典本身是否為 null
        if (gameManager.AllPlayerMoney.ContainsKey(gameManager.Runner.LocalPlayer))
        {
            moneyText.text = "$ " + gameManager.AllPlayerMoney[gameManager.Runner.LocalPlayer];
        }
    }
}