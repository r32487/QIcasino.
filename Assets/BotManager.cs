using UnityEngine;
using System.Collections.Generic;

public class BotManager : MonoBehaviour
{
    [Header("--- 機器人設定 ---")]
    public GameObject botPrefab;        // 機器人的模具 (你剛做好的那個頭像)
    public int botCount = 3;            // 想要生幾隻？(預設改為3)
    public Transform[] spawnPoints;     // 出生點清單
    public Transform parentCanvas;      // UI 機器人必須放在 Canvas 底下

    private List<GameObject> activeBots = new List<GameObject>();

    void Start()
    {
        // 稍微等個 1 秒鐘，等伺服器連好再噴機器人
        Invoke("SpawnBots", 1f); 
    }

    public void SpawnBots()
    {
        // 先把舊的機器人刪掉
        foreach (var bot in activeBots) Destroy(bot);
        
        // ✅ 修正這裡：清空清單，只要寫一次 activeBots
        activeBots.Clear();

        // 確保生成的數量不會超過出生點的數量 (最多3隻)
        int spawnLimit = Mathf.Clamp(botCount, 0, Mathf.Min(3, spawnPoints.Length));

        for (int i = 0; i < spawnLimit; i++)
        {
            Transform spawnPos = spawnPoints[i]; 
            
            // 直接把機器人生在「座位」底下
            GameObject newBot = Instantiate(botPrefab, spawnPos);
            
            // 取得 UI 專用的座標組件
            RectTransform rect = newBot.GetComponent<RectTransform>();
            
            // 座標歸零！讓它完美對齊這個座位的正中心
            rect.anchoredPosition = Vector2.zero; 
            
            // 恢復比例防呆
            newBot.transform.localScale = Vector3.one; 
            
            activeBots.Add(newBot);
        }
    }
}