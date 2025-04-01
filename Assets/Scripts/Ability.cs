using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // UI 관련 네임스페이스 추가
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
        UpdatePlayerCoinText(); // 초기 코인 UI 업데이트

    }
    // Die 버튼 클릭 시 호출되는 메서드
    public void OnDieButtonClick()
    {
        // CardManager 인스턴스 가져오기
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null)
        {
            cardManager.FlipAllCards(); // FlipAllCards 메서드 호출
        }

        // Coin 값 감소
        Coin -= 500;

        // Monster 인스턴스 가져오기
        Monster monster = FindObjectOfType<Monster>();
        if (monster != null)
        {
            monster.MonsterCoins += 500; // MonsterCoin 값 증가
            monster.UpdateMonsterCoinText(); // 몬스터 코인 UI 업데이트 호출
        }

        // UI 업데이트 (필요 시)
        UpdatePlayerCoinText();
    }

    public void UpdatePlayerCoinText()
    {
        PlayerCoin.text = Coin.ToString(); // 플레이어 코인 UI 업데이트
    }
}

