using UnityEngine;
using TMPro;

public class DragonGateManager : MonoBehaviour
{
    public GameObject dragonEffectUI;    // 龍門特效背景
    public TextMeshProUGUI statusText;   // 顯示剩餘次數的文字
    
    private int freeSpinsLeft = 0;

    // 提供給拉霸機檢查：現在是不是免費時間？
    public bool IsInFreeSpin => freeSpinsLeft > 0;

    void Start() => dragonEffectUI.SetActive(false);

    // 啟動龍門模式
    public void StartDragonMode(int spins)
    {
        freeSpinsLeft = spins;
        dragonEffectUI.SetActive(true);
        UpdateStatus("🐉 龍門開啟！獲得 " + spins + " 次免費");
    }

    // 消耗一次次數
    public void UseFreeSpin()
    {
        if (freeSpinsLeft > 0)
        {
            freeSpinsLeft--;
            UpdateStatus("🐉 龍門模式 🐉 剩餘: " + freeSpinsLeft + " 次");
            
            if (freeSpinsLeft <= 0)
            {
                dragonEffectUI.SetActive(false);
                UpdateStatus("龍門已關閉");
            }
        }
    }

    void UpdateStatus(string msg) { if (statusText != null) statusText.text = msg; }
}