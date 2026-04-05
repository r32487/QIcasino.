using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    // 按鈕 A 呼叫：去普通老虎機
    public void GoToSlotMachine()
    {
        SceneManager.LoadScene("SlotMachine");
    }

    // 按鈕 B 呼叫：去龍門模式
    public void GoToDragonGate()
    {
        SceneManager.LoadScene("DragonGate");
    }
}