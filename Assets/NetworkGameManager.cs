using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class NetworkGameManager : NetworkBehaviour, IPlayerLeft
{
    public enum GameState { Waiting, Dealing, Result, Bankrupt }
    
    [Networked] public GameState CurrentState { get; set; }
    [Networked] public int CurrentPot { get; set; } 
    [Networked] public PlayerRef CurrentTurnPlayer { get; set; } 
    [Networked] public int CurrentBotTurnIndex { get; set; } 
    [Networked] public int ReadyCount { get; set; } 
    [Networked] public TickTimer TurnTimer { get; set; } 

    [Networked, Capacity(10)] public NetworkDictionary<PlayerRef, int> AllPlayerMoney => default;
    [Networked, Capacity(10)] public NetworkDictionary<PlayerRef, NetworkBool> PlayerReadyStatus => default;

    [Header("Money and UI")]
    public TMP_Text moneyText;          
    public TMP_Text potText;            
    public TMP_InputField betInputField; 
    public TMP_Text resultText;         
    public TMP_Text turnInfoText;       

    [Header("Bot Settings")]
    public GameObject botSliderObj;     
    public Slider botSlider;            
    public TMP_Text botCountText;       
    public NetworkObject botPrefab;     
    public Transform[] botSpawnPoints;  
    private List<NetworkObject> spawnedBots = new List<NetworkObject>(); 

    [Header("Game Panels")]
    public Button readyButton;          
    public Button hostStartButton;      
    public GameObject actionPanel;      
    public GameObject highLowPanel;     

    [Header("Card Settings")]
    public NetworkObject cardPrefab;     
    public Sprite[] cardFaces;           
    
    [Header("Positions")]
    public Transform leftPostPoint;      
    public Transform rightPostPoint;     
    public Transform centerPoint;        

    private NetworkObject leftCardObj, rightCardObj, centerCardObj;
    private int leftValue, rightValue, centerValue;
    private bool isHighLowMode = false;
    private bool isGameLoopRunning = false; 
    private const int MIN_BET = 10;

    // --- 連線初始化與金錢同步 ---

    public override void Spawned() 
    { 
        if (Object.HasStateAuthority)
        {
            if (CurrentPot < MIN_BET) CurrentPot = MIN_BET;
            CurrentBotTurnIndex = -1;
            ReadyCount = 0;
        }

        // 👈 進場時：抓取本地 PlayerPrefs (商店買好的錢)，傳給伺服器字典
        RPC_RegisterPlayer(Runner.LocalPlayer, PlayerData.GetMoney());
        ResetUI(); 
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // 👈 退場時：將網路字典中的當前餘額存回本地硬碟，這樣大廳 UI 就會顯示贏到的錢
        if (AllPlayerMoney.ContainsKey(Runner.LocalPlayer))
        {
            PlayerData.SaveMoney(AllPlayerMoney[Runner.LocalPlayer]);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RegisterPlayer(PlayerRef player, int initialMoney)
    {
        // 👈 更新或新增玩家金錢，確保進場時是商店買好的金額
        if (AllPlayerMoney.ContainsKey(player)) AllPlayerMoney.Set(player, initialMoney);
        else AllPlayerMoney.Add(player, initialMoney);

        if (!PlayerReadyStatus.ContainsKey(player)) PlayerReadyStatus.Add(player, false);
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!Object.HasStateAuthority) return;
        if (AllPlayerMoney.ContainsKey(player)) AllPlayerMoney.Remove(player);
        if (PlayerReadyStatus.ContainsKey(player)) 
        {
            if(PlayerReadyStatus[player]) ReadyCount--;
            PlayerReadyStatus.Remove(player);
        }
        if (CurrentTurnPlayer == player) SwitchToNextTurn();
        if (Runner.ActivePlayers.Count() == 0) { isGameLoopRunning = false; CurrentState = GameState.Waiting; }
    }

    // --- 每幀更新 ---

    private void Update()
    {
        if (Runner == null || !Runner.IsRunning) return;

        if (AllPlayerMoney.ContainsKey(Runner.LocalPlayer))
        {
            int myNetMoney = AllPlayerMoney[Runner.LocalPlayer];
            if (moneyText != null) moneyText.text = "Balance: $" + myNetMoney;
            if (myNetMoney <= 0 && CurrentState != GameState.Bankrupt) HandleBankruptcy();
        }

        if (potText != null) potText.text = "Pot: $" + CurrentPot;
        UpdateTurnDisplay();

        if (Object.HasStateAuthority && TurnTimer.Expired(Runner))
        {
            TurnTimer = TickTimer.None;
            SwitchToNextTurn();
        }
    }

    private void UpdateTurnDisplay()
    {
        if (turnInfoText == null) return;
        if (CurrentState == GameState.Waiting)
        {
            var players = Runner.ActivePlayers.ToList();
            turnInfoText.text = "Ready: " + ReadyCount + " / " + players.Count;
            if (Object.HasStateAuthority && hostStartButton != null)
                hostStartButton.interactable = (ReadyCount >= players.Count && players.Count > 0);
        }
        else
        {
            if (CurrentBotTurnIndex == -1)
                turnInfoText.text = (CurrentTurnPlayer == Runner.LocalPlayer) ? "YOUR TURN!" : "Waiting Player " + CurrentTurnPlayer.PlayerId;
            else
                turnInfoText.text = "Bot " + (CurrentBotTurnIndex + 1) + " Action...";
        }
    }

    // --- 準備與開始 ---

    public void UI_ReadyButton()
    {
        RPC_SetReady(Runner.LocalPlayer, true);
        if (readyButton) readyButton.interactable = false;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetReady(PlayerRef player, bool status)
    {
        if (PlayerReadyStatus.ContainsKey(player) && !PlayerReadyStatus[player] && status)
        {
            PlayerReadyStatus.Set(player, true);
            ReadyCount++;
        }
    }

    public void UI_StartGameButton()
    {
        if (!Object.HasStateAuthority) return;
        var players = Runner.ActivePlayers.OrderBy(p => p.PlayerId).ToList();
        if (players.Count == 0) return;
        isGameLoopRunning = true;
        CurrentTurnPlayer = players.First();
        CurrentBotTurnIndex = -1;
        RPC_BroadcastGameStart();
        StartNextTurn();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastGameStart()
    {
        if (readyButton) readyButton.gameObject.SetActive(false);
        if (hostStartButton) hostStartButton.gameObject.SetActive(false);
        if (botSliderObj) botSliderObj.SetActive(false);
        CurrentState = GameState.Dealing;
    }

    // --- 遊戲邏輯與結算 ---

    private void StartNextTurn()
    {
        if (!Object.HasStateAuthority) return;
        DespawnOldCards();
        ShuffleDeck();
        leftValue = DrawCard(); rightValue = DrawCard();
        if ((leftValue % 13) == (rightValue % 13)) isHighLowMode = true;
        else
        {
            isHighLowMode = false;
            if ((leftValue % 13) > (rightValue % 13)) { int t = leftValue; leftValue = rightValue; rightValue = t; }
        }
        leftCardObj = SpawnPhysicalCard(leftValue, leftPostPoint.position, true);
        rightCardObj = SpawnPhysicalCard(rightValue, rightPostPoint.position, true);
        centerValue = DrawCard();
        centerCardObj = SpawnPhysicalCard(centerValue, centerPoint.position, false);
        RPC_SyncUI(CurrentTurnPlayer, CurrentBotTurnIndex, isHighLowMode);
        if (CurrentBotTurnIndex >= 0) StartCoroutine(BotAIThinking());
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SyncUI(PlayerRef player, int botIdx, bool highLow)
    {
        bool isMyTurn = (botIdx == -1 && player == Runner.LocalPlayer);
        if (actionPanel) actionPanel.SetActive(isMyTurn && !highLow);
        if (highLowPanel) highLowPanel.SetActive(isMyTurn && highLow); 
        if (betInputField) betInputField.gameObject.SetActive(isMyTurn);
    }

    public void UI_SubmitAction(int choice) 
    {
        int amt = 0;
        if (choice != -1 && (!int.TryParse(betInputField.text, out amt) || amt < MIN_BET)) return;
        RPC_PlayerAction(Runner.LocalPlayer, choice, (choice == -1), amt);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_PlayerAction(PlayerRef player, int choice, bool skip, int bet)
    {
        if (CurrentBotTurnIndex == -1 && player != CurrentTurnPlayer) return;
        if (!skip && (bet < MIN_BET || (AllPlayerMoney.ContainsKey(player) && bet > AllPlayerMoney[player]))) return;
        ProcessResult(player, choice, skip, bet);
    }

    private void ProcessResult(PlayerRef player, int choice, bool skip, int bet)
    {
        if (!Object.HasStateAuthority) return;
        Rpc_SetCardFace(centerCardObj, centerValue);

        string name = (player == PlayerRef.None) ? "Bot " + (CurrentBotTurnIndex + 1) : "Player " + player.PlayerId;

        if (skip) { RPC_ShowResult(name + " Skipped."); }
        else {
            int vL = leftValue % 13; int vR = rightValue % 13; int vC = centerValue % 13;
            bool win = false; bool tsu = false;

            if (isHighLowMode) {
                int gate = leftValue % 13;
                if (vC == gate) tsu = true;
                else if (choice == 1 && vC > gate) win = true;
                else if (choice == 2 && vC < gate) win = true;
            } else {
                if (vC == vL || vC == vR) tsu = true;
                else if (vC > vL && vC < vR) win = true;
            }

            string resultMsg = "";
            int amt = 0;

            if (tsu) { amt = bet * 2; resultMsg = "BUST! -$" + amt; CurrentPot += amt; }
            else if (win) { amt = Mathf.Min(bet, CurrentPot); resultMsg = "WIN! +$" + amt; CurrentPot -= amt; }
            else { amt = bet; resultMsg = "LOSE! -$" + amt; CurrentPot += amt; }

            if (player != PlayerRef.None && AllPlayerMoney.ContainsKey(player)) {
                int m = AllPlayerMoney[player];
                if (tsu) m -= (bet * 2); else if (win) m += amt; else m -= bet;
                AllPlayerMoney.Set(player, m);
            }

            // 👈 即時廣播：誰贏/輸了多少，包含機器人
            RPC_ShowResult(name + " " + resultMsg);

            if (CurrentPot < 10) CurrentPot = 10;
        }
        TurnTimer = TickTimer.CreateFromSeconds(Runner, 3.5f);
    }

    private void SwitchToNextTurn()
    {
        if (!Object.HasStateAuthority) return;
        var players = Runner.ActivePlayers.OrderBy(p => p.PlayerId).ToList();
        if (players.Count == 0) { ResetUI(); return; }

        if (CurrentBotTurnIndex == -1) {
            int idx = players.IndexOf(CurrentTurnPlayer);
            if (idx != -1 && idx < players.Count - 1) CurrentTurnPlayer = players[idx + 1];
            else { if (spawnedBots.Count > 0) CurrentBotTurnIndex = 0; else CurrentTurnPlayer = players[0]; }
        } else {
            if (CurrentBotTurnIndex < spawnedBots.Count - 1) CurrentBotTurnIndex++;
            else { CurrentBotTurnIndex = -1; CurrentTurnPlayer = players[0]; }
        }
        if (isGameLoopRunning) StartNextTurn();
        else ResetUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowResult(string m) { if(resultText) resultText.text = m; }

    private void ResetUI()
    {
        CurrentState = GameState.Waiting;
        if (Object.HasStateAuthority) { 
            if(hostStartButton) hostStartButton.gameObject.SetActive(true); 
            if(botSliderObj) botSliderObj.SetActive(true); 
            ReadyCount = 0;
            foreach (var p in Runner.ActivePlayers.ToList()) PlayerReadyStatus.Set(p, false);
        }
        if (readyButton) { readyButton.gameObject.SetActive(true); readyButton.interactable = true; }
        if (actionPanel) actionPanel.SetActive(false); 
        if (highLowPanel) highLowPanel.SetActive(false); 
        if (betInputField) betInputField.gameObject.SetActive(false);
    }

    private void HandleBankruptcy() { CurrentState = GameState.Bankrupt; isGameLoopRunning = false; ResetUI(); }
    IEnumerator BotAIThinking() { yield return new WaitForSeconds(2f); if (Object.HasStateAuthority) ProcessResult(PlayerRef.None, Random.Range(1,3), false, 10); }
    public void UI_OnBotSliderChanged(float v) { if (Object.HasStateAuthority) RefreshBots((int)v); }
    private void RefreshBots(int c) {
        foreach (var b in spawnedBots.ToList()) if (b != null) Runner.Despawn(b);
        spawnedBots.Clear();
        for (int i = 0; i < c; i++) spawnedBots.Add(Runner.Spawn(botPrefab, botSpawnPoints[i].position, botSpawnPoints[i].rotation));
    }
    private NetworkObject SpawnPhysicalCard(int id, Vector3 p, bool f) { var c = Runner.Spawn(cardPrefab, p, Quaternion.identity); if (f) Rpc_SetCardFace(c, id); return c; }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_SetCardFace(NetworkObject o, int id) { if (o != null) { var sr = o.GetComponent<SpriteRenderer>(); sr.sprite = cardFaces[id]; sr.sortingOrder = 10; } }
    private void DespawnOldCards() { if (leftCardObj) Runner.Despawn(leftCardObj); if (rightCardObj) Runner.Despawn(rightCardObj); if (centerCardObj) Runner.Despawn(centerCardObj); }
    private List<int> deck = new List<int>();
    private void ShuffleDeck() { deck.Clear(); for (int i = 0; i < 52; i++) deck.Add(i); for (int i = 0; i < deck.Count; i++) { int t = deck[i]; int r = Random.Range(i, deck.Count); deck[i] = deck[r]; deck[r] = t; } }
    private int DrawCard() { int d = deck[0]; deck.RemoveAt(0); return d; }
}