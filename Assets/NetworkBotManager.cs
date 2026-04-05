using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class NetworkBotManager : NetworkBehaviour
{
    [Header("--- 多人連線設定 ---")]
    [Networked, OnChangedRender(nameof(OnBotCountChanged))]
    public int SyncedBotCount { get; set; } // 這個數值會在網路上自動同步

    public GameObject botPrefab;
    public Transform[] spawnPoints;
    private List<GameObject> activeBots = new List<GameObject>();

    // 只有主機可以呼叫此函數（例如透過 UI Slider）
    public void RPC_SetBotCount(int count)
    {
        if (Object.HasStateAuthority)
        {
            SyncedBotCount = Mathf.Clamp(count, 0, spawnPoints.Length);
        }
    }

    // 當 SyncedBotCount 在網路上改變時，所有玩家都會執行這個函數
    void OnBotCountChanged()
    {
        UpdateBotVisuals();
    }

    void UpdateBotVisuals()
    {
        // 刪除舊的
        foreach (var bot in activeBots) Destroy(bot);
        activeBots.Clear();

        // 根據同步的數值生成新的
        for (int i = 0; i < SyncedBotCount; i++)
        {
            GameObject newBot = Instantiate(botPrefab, spawnPoints[i]);
            newBot.transform.localPosition = Vector3.zero;
            newBot.transform.localScale = Vector3.one;
            activeBots.Add(newBot);
        }
    }
}