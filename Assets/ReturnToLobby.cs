using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement; // ⚠️ 控制場景切換必備

public class ReturnToLobby : MonoBehaviour
{
    [Header("大廳場景編號")]
    public int lobbySceneIndex = 0; // 通常大廳在 Build Settings 裡都是 0 號

    public void UI_LeaveRoomButton()
    {
        Debug.Log("🚪 準備離開房間，正在關閉網路連線...");

        // 1. 抓出負責連線的總機 (NetworkRunner)
        var runner = FindAnyObjectByType<NetworkRunner>();
        
        if (runner != null)
        {
            // 2. 斷開魂結！關閉網路連線
            runner.Shutdown();
        }

        // 3. 強制載入大廳場景
        Debug.Log($"🚗 連線已關閉，正在搭車前往場景 {lobbySceneIndex} (大廳)...");
        SceneManager.LoadScene(lobbySceneIndex);
    }
}