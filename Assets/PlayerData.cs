using UnityEngine;

public static class PlayerData
{
    private const string MONEY_KEY = "UserMoney";

    // 取得目前本地存儲的錢，預設 1000
    public static int GetMoney() => PlayerPrefs.GetInt(MONEY_KEY, 1000);

    // 儲存錢到本地
    public static void SaveMoney(int amount)
    {
        PlayerPrefs.SetInt(MONEY_KEY, amount);
        PlayerPrefs.Save(); // 👈 強制寫入硬碟，防止掉檔
        Debug.Log("[Local Data] Money Updated: " + amount);
    }
}