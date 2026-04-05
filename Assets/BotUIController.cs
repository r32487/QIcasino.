using UnityEngine;
using UnityEngine.UI;
using Fusion; // 必須引用 Fusion

public class BotUIController : NetworkBehaviour
{
    [Header("--- 綁定元件 ---")]
    public Slider botSlider;               // 畫面上那個拉桿
    public NetworkBotManager botManager;   // 負責生機器人的大腦

    // Spawned() 在 Fusion 裡就等於 Start()，當物件連上線時會觸發
    public override void Spawned()
    {
        // Object.HasStateAuthority 代表「我是不是主機？」
        // 如果是主機，拉桿可以動；如果是連進來的玩家，拉桿會被鎖死 (變成灰色)
        botSlider.interactable = Object.HasStateAuthority;

        // 當拉桿被拖動時，執行改變數量的指令
        botSlider.onValueChanged.AddListener(OnSliderChanged);
    }

   // 在 BotUIController.cs 的上方加入對 GameManager 的引用
    public NetworkGameManager gameManager; 

// 然後把 OnSliderChanged 修改成這樣：
    void OnSliderChanged(float value)
    {
        if (Object.HasStateAuthority)
        {
        // 【新增這行防呆】只有在 Waiting 狀態才能調整機器人！
            if (gameManager.CurrentState == NetworkGameManager.GameState.Waiting)
            {
                botManager.RPC_SetBotCount((int)value);
            }
            else
            {
                Debug.LogWarning("遊戲進行中，無法調整機器人數量！");
                botSlider.value = botManager.SyncedBotCount; // 把拉桿強制彈回原本的數字
            }
        }
    }
}