using UnityEngine;
using UnityEngine.SceneManagement; // 切換場景必備

public class LobbyNavigation : MonoBehaviour
{
    // 按鈕 A 專用：前往普通老虎機
    public void GoToSlotMachine()
    {
        // 括號內的名字必須跟 Build Profiles 裡的場景名稱完全一致
        SceneManager.LoadScene("SlotMachine");
    }

    // 按鈕 B 專用：前往龍門模式
    public void GoToDragonGate()
    {
        SceneManager.LoadScene("DragonGate");
    }
}