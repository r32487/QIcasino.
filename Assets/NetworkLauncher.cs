using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class NetworkLauncher : MonoBehaviour
{
    private NetworkRunner _runner;

    async void Start()
    {
        // 1. 準備好連線核心 (Runner)
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // 2. 自動連線：如果有房間就加入，沒房間就自己當主機開一間
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "LobbyRoom", // 大家都會進到這個大廳房間
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log("連線成功！已進入大廳伺服器。");
    }
}