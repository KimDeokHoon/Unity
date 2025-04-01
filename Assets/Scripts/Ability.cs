using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // UI ���� ���ӽ����̽� �߰�
using TMPro;

public class Ability : MonoBehaviour
{
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text PlayerCoin;

    public int Coin;
    public bool isMine;
    public bool isDie;
    public static int scores;

    void Start()
    {
        UpdatePlayerCoinText(); // �ʱ� ���� UI ������Ʈ

    }
    // Die ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    public void OnDieButtonClick()
    {
        // CardManager �ν��Ͻ� ��������
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null)
        {
            cardManager.FlipAllCards(); // FlipAllCards �޼��� ȣ��
        }

        // Coin �� ����
        Coin -= 500;

        // Monster �ν��Ͻ� ��������
        Monster monster = FindObjectOfType<Monster>();
        if (monster != null)
        {
            monster.MonsterCoins += 500; // MonsterCoin �� ����
            monster.UpdateMonsterCoinText(); // ���� ���� UI ������Ʈ ȣ��
        }

        // UI ������Ʈ (�ʿ� ��)
        UpdatePlayerCoinText();
    }

    public void UpdatePlayerCoinText()
    {
        PlayerCoin.text = Coin.ToString(); // �÷��̾� ���� UI ������Ʈ
    }
}

