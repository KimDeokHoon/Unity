using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button battingButton;     // Batting 버튼
    public Button dieButton;         // Die 버튼
    public Button harpButton;        // Harp 버튼
    public Button quarterButton;     // Quarter 버튼
    public Button PoolButton;        // Pool 버튼

    public Button[] activeButtons; // 활성화할 버튼 배열

    // Batting 버튼 클릭 시 호출되는 메서드
    public void OnBattingButtonClick()
    {
        // Batting 버튼과 Die 버튼 비활성화
        battingButton.gameObject.SetActive(false);
        dieButton.gameObject.SetActive(false);

        // Call, Harp, Quarter, Tadang, Pool, Max 버튼 활성화
        foreach (Button button in activeButtons)
        {
            button.gameObject.SetActive(true);
        }
    }

    // 2. Harp 버튼 클릭 시 호출되는 메서드
    public void OnHarpButtonClick()
    {
        // BattingCharge의 50%를 베팅
        int halfBet = GameManager.Inst.BattingCharge / 2;

        // 플레이어 코인 업데이트
        Ability playerAbility = FindObjectOfType<Ability>();

        if (playerAbility != null)
        {
            // 참가비 차감
            playerAbility.Coin -= halfBet;

            // BattingCharge 업데이트
            GameManager.Inst.BattingCharge += halfBet;

            // UI 업데이트
            GameManager.Inst.BattingCoin.text = GameManager.Inst.BattingCharge.ToString();
            playerAbility.UpdatePlayerCoinText();
        }
    }

    // 3. Quarter 버튼 클릭 시 호출되는 메서드
    public void OnQuarterButtonClick()
    {
        // BattingCharge의 25%를 베팅
        int halfBet = GameManager.Inst.BattingCharge / 4;

        // 플레이어 코인 업데이트
        Ability playerAbility = FindObjectOfType<Ability>();

        if (playerAbility != null)
        {
            // 참가비 차감
            playerAbility.Coin -= halfBet;

            // BattingCharge 업데이트
            GameManager.Inst.BattingCharge += halfBet;

            // UI 업데이트
            GameManager.Inst.BattingCoin.text = GameManager.Inst.BattingCharge.ToString();
            playerAbility.UpdatePlayerCoinText();
        }
    }

    // 5. Pool 버튼 클릭 시 호출되는 메서드
    public void OnPoolButtonClick()
    {
        // BattingCharge의 2배를 베팅
        int halfBet = GameManager.Inst.BattingCharge;

        // 플레이어 코인 업데이트
        Ability playerAbility = FindObjectOfType<Ability>();

        if (playerAbility != null)
        {
            // 참가비 차감
            playerAbility.Coin -= halfBet;

            // BattingCharge 업데이트
            GameManager.Inst.BattingCharge += halfBet;

            // UI 업데이트
            GameManager.Inst.BattingCoin.text = GameManager.Inst.BattingCharge.ToString();
            playerAbility.UpdatePlayerCoinText();
        }
    }
}