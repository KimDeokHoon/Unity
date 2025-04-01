using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button battingButton;     // Batting ��ư
    public Button dieButton;         // Die ��ư
    public Button harpButton;        // Harp ��ư
    public Button quarterButton;     // Quarter ��ư
    public Button PoolButton;        // Pool ��ư

    public Button[] activeButtons; // Ȱ��ȭ�� ��ư �迭

    // Batting ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    public void OnBattingButtonClick()
    {
        // Batting ��ư�� Die ��ư ��Ȱ��ȭ
        battingButton.gameObject.SetActive(false);
        dieButton.gameObject.SetActive(false);

        // Call, Harp, Quarter, Tadang, Pool, Max ��ư Ȱ��ȭ
        foreach (Button button in activeButtons)
        {
            button.gameObject.SetActive(true);
        }
    }

    // 2. Harp ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    public void OnHarpButtonClick()
    {
        // BattingCharge�� 50%�� ����
        int halfBet = GameManager.Inst.BattingCharge / 2;

        // �÷��̾� ���� ������Ʈ
        Ability playerAbility = FindObjectOfType<Ability>();

        if (playerAbility != null)
        {
            // ������ ����
            playerAbility.Coin -= halfBet;

            // BattingCharge ������Ʈ
            GameManager.Inst.BattingCharge += halfBet;

            // UI ������Ʈ
            GameManager.Inst.BattingCoin.text = GameManager.Inst.BattingCharge.ToString();
            playerAbility.UpdatePlayerCoinText();
        }
    }

    // 3. Quarter ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    public void OnQuarterButtonClick()
    {
        // BattingCharge�� 25%�� ����
        int halfBet = GameManager.Inst.BattingCharge / 4;

        // �÷��̾� ���� ������Ʈ
        Ability playerAbility = FindObjectOfType<Ability>();

        if (playerAbility != null)
        {
            // ������ ����
            playerAbility.Coin -= halfBet;

            // BattingCharge ������Ʈ
            GameManager.Inst.BattingCharge += halfBet;

            // UI ������Ʈ
            GameManager.Inst.BattingCoin.text = GameManager.Inst.BattingCharge.ToString();
            playerAbility.UpdatePlayerCoinText();
        }
    }

    // 5. Pool ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    public void OnPoolButtonClick()
    {
        // BattingCharge�� 2�踦 ����
        int halfBet = GameManager.Inst.BattingCharge;

        // �÷��̾� ���� ������Ʈ
        Ability playerAbility = FindObjectOfType<Ability>();

        if (playerAbility != null)
        {
            // ������ ����
            playerAbility.Coin -= halfBet;

            // BattingCharge ������Ʈ
            GameManager.Inst.BattingCharge += halfBet;

            // UI ������Ʈ
            GameManager.Inst.BattingCoin.text = GameManager.Inst.BattingCharge.ToString();
            playerAbility.UpdatePlayerCoinText();
        }
    }
}