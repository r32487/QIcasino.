using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationController : MonoBehaviour
{
    public void BackToLobby()
    {
        // 不管在哪，點了就回大廳
        SceneManager.LoadScene("Lobby");
    }
}