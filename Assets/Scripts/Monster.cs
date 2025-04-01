using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // UI ���� ���ӽ����̽� �߰�
using TMPro;            // TMP_Text ���� ���ӽ����̽� �߰�

public class Monster : MonoBehaviour
{
    [SerializeField] SpriteRenderer monster;
    [SerializeField] TMP_Text MonsterCoin;

    public int MonsterCoins;
    public bool isMine;
    public bool isDie;
    public static int monsterscores;

    void Start()
    {
        UpdateMonsterCoinText(); // �ʱ� ���� ���� UI ������Ʈ
    }

    // UI ������Ʈ �޼��� (�ʿ� ��)
    public void UpdateMonsterCoinText()
    {
        MonsterCoin.text = MonsterCoins.ToString(); // ���� ���� UI ������Ʈ
    }
}

