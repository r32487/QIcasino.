using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class LeaveGameHandler : MonoBehaviour
{
    public async void UI_LeaveToLobby()
    {
        // 👈 修正：改用 FindAnyObjectByType 消除警告
        NetworkRunner runner = FindAnyObjectByType<NetworkRunner>();

        if (runner != null)
        {
            await runner.Shutdown();
        }

        SceneManager.LoadScene("Lobby"); 
    }
}