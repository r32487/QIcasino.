using UnityEngine;
using UnityEngine.SceneManagement; // 呼叫 Unity 的「場景管理」工具箱

public class SceneLoader : MonoBehaviour
{
    // 這個功能用來載入老虎機場景
    public void GoToSlotMachine()
    {
        // 括號裡的名字必須跟你剛剛建立的場景名稱一模一樣
        SceneManager.LoadScene("slot machine"); 
    }
    public void GoToDragonGate()
    {
        // 這裡的名字一定要跟你 Build Profiles 裡的龍門場景名稱一模一樣
        SceneManager.LoadScene("DragonGate");
    }

    // 這個功能用來回到大廳 (我們先寫好備用)
    public void GoToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}