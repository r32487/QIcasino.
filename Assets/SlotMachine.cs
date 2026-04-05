using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SlotMachine : MonoBehaviour
{
    [Header("--- 外部系統連結 ---")]
    public DragonGateManager dragonGate; 

    [Header("--- UI 零件 ---")]
    public Image slot1; public Image slot2; public Image slot3;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI winMessageText;
    public RectTransform leverTransform;

    [Header("--- 設定 ---")]
    public List<Sprite> allSymbols;
    public int costPerSpin = 100;
    public float pullSpeed = 0.2f;
    public float resetSpeed = 0.7f;

    private bool isSpinning = false;
    private Vector3 originalScale;

    void Start()
    {
        // 👈 初始化時直接同步最新的錢
        UpdateUI();
        originalScale = leverTransform.localScale;
        leverTransform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public void TriggerLeverAnimation()
    {
        if (isSpinning) return;

        bool isFree = (dragonGate != null && dragonGate.IsInFreeSpin);

        // 👈 修改：使用 PlayerData.GetMoney() 判斷餘額
        int currentMoney = PlayerData.GetMoney();

        if (isFree || currentMoney >= costPerSpin)
        {
            StartCoroutine(AnimateAndSpin(isFree));
        }
        else
        {
            if (winMessageText != null) winMessageText.text = "<color=red>資金不足！</color>";
        }
    }

    IEnumerator AnimateAndSpin(bool isFree)
    {
        isSpinning = true;

        if (isFree) {
            if (dragonGate != null) dragonGate.UseFreeSpin();
        } else {
            // 👈 修改：扣錢並存檔
            int currentMoney = PlayerData.GetMoney();
            PlayerData.SaveMoney(currentMoney - costPerSpin); 
            UpdateUI();
        }

        // --- 拉桿動畫 ---
        float t = 0;
        while (t < 1f) {
            t += Time.deltaTime / pullSpeed;
            leverTransform.localRotation = Quaternion.Euler(Mathf.SmoothStep(0, 180, t), 0, 0);
            leverTransform.localScale = originalScale * (1f + Mathf.Sin(t * Mathf.PI) * 0.25f);
            yield return null;
        }
        
        yield return StartCoroutine(SpinRoutine()); 

        // 回彈動畫
        t = 0;
        while (t < 1f) {
            t += Time.deltaTime / resetSpeed;
            float spring = Mathf.Sin(t * Mathf.PI * 5f) * (15f * (1f - t));
            leverTransform.localRotation = Quaternion.Euler(Mathf.Lerp(180, 0, t) + spring, 0, 0);
            yield return null;
        }
        isSpinning = false;
    }

    IEnumerator SpinRoutine()
    {
        int f1 = Random.Range(0, allSymbols.Count);
        int f2 = Random.Range(0, allSymbols.Count);
        int f3 = Random.Range(0, allSymbols.Count);

        float timer = 0f;
        while (timer < 1.0f) {
            slot1.sprite = allSymbols[Random.Range(0, allSymbols.Count)];
            slot2.sprite = allSymbols[Random.Range(0, allSymbols.Count)];
            slot3.sprite = allSymbols[Random.Range(0, allSymbols.Count)];
            timer += 0.1f; yield return new WaitForSeconds(0.1f);
        }
        slot1.sprite = allSymbols[f1]; slot2.sprite = allSymbols[f2]; slot3.sprite = allSymbols[f3];

        CheckResult(f1, f2, f3);
    }

    void CheckResult(int i1, int i2, int i3)
    {
        if (i1 == 0 && i2 == 0 && i3 == 0 && dragonGate != null) {
            dragonGate.StartDragonMode(10);
            return;
        }

        int win = 0;
        if (i1 == i2 && i2 == i3) win = 1000;
        else if (i1 == i2 || i2 == i3 || i1 == i3) win = 200;

        // 👈 修改：贏錢後存入 PlayerData
        if (win > 0)
        {
            int currentMoney = PlayerData.GetMoney();
            PlayerData.SaveMoney(currentMoney + win);
        }

        if (winMessageText != null) 
            winMessageText.text = win > 0 ? "中獎! +" + win : "沒中獎";
        
        UpdateUI();
    }

    // 👈 修改：隨時從 PlayerData 讀取最新餘額
    void UpdateUI() 
    { 
        if (moneyText != null)
            moneyText.text = "剩餘資金: " + PlayerData.GetMoney(); 
    }
}