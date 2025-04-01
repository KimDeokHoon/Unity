using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // UI 관련 네임스페이스 추가
using TMPro;            // TMP_Text 관련 네임스페이스 추가

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
        UpdateMonsterCoinText(); // 초기 몬스터 코인 UI 업데이트
    }

    // UI 업데이트 메서드 (필요 시)
    public void UpdateMonsterCoinText()
    {
        MonsterCoin.text = MonsterCoins.ToString(); // 몬스터 코인 UI 업데이트
    }
}

