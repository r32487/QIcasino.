using UnityEngine;
using UnityEngine.SceneManagement; // 載入場景必備

public class SceneBackLoader : MonoBehaviour
{
    public void BackToLobby()
    {
        // 這裡的名字一定要跟你在 Build Profiles 裡看到的 "Lobby" 一模一樣
        SceneManager.LoadScene("Lobby"); 
    }
}