using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class FusionLauncher : MonoBehaviour
{
    [Header("--- UI 面板控制 ---")]
    public GameObject mainMenuPanel;
    public GameObject choicePanel;
    public GameObject inputPanel;

    [Header("--- 連線組件 ---")]
    public TMP_InputField roomNameInputField; 
    public NetworkRunner runnerPrefab;

    [Header("--- 場景設定 ---")]
    // 👈 直接在這裡改！看你的「射龍門」在 Build Settings 是第幾個，就填幾
    [Tooltip("請填入射龍門場景在 Build Settings 中的編號")]
    public int gameSceneIndex = 1; 

    private GameMode _selectedMode;

    void Start()
    {
        ShowMainMenu();
    }

    public void UI_OpenChoiceMenu()
    {
        mainMenuPanel.SetActive(false);
        choicePanel.SetActive(true);
        inputPanel.SetActive(false);
    }

    public void UI_SelectCreate()
    {
        _selectedMode = GameMode.Host; 
        GoToInput();
    }

    public void UI_SelectJoin()
    {
        _selectedMode = GameMode.Client; 
        GoToInput();
    }

    private void GoToInput()
    {
        choicePanel.SetActive(false);
        inputPanel.SetActive(true);
        roomNameInputField.ActivateInputField(); 
    }

    public void UI_OnInputEnter(string text)
    {
        if (Keyboard.current != null && 
           (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
        {
            StartGame(_selectedMode);
        }
    }

    private async void StartGame(GameMode mode)
    {
        var oldRunner = FindAnyObjectByType<NetworkRunner>();
        if (oldRunner != null)
        {
            await oldRunner.Shutdown();
            Destroy(oldRunner.gameObject);
        }

        var runner = Instantiate(runnerPrefab);
        runner.ProvideInput = true;

        string roomName = string.IsNullOrEmpty(roomNameInputField.text) ? "DefaultRoom" : roomNameInputField.text;

        Debug.Log($"[Fusion] 正在以 {mode} 模式進入房間: {roomName}，目標場景編號: {gameSceneIndex}");

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            // 👈 這裡改掉！不再用 +1，直接用你填的編號
            Scene = SceneRef.FromIndex(gameSceneIndex),
            SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        choicePanel.SetActive(false);
        inputPanel.SetActive(false);
    }
}