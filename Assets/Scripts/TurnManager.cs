using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("Develop")]
    [SerializeField][Tooltip("시작 턴 모드를 정합니다")] ETurnMode eTurnMode;
    [SerializeField][Tooltip("카드 배분이 매우 빨라집니다")] bool fastMode;
    [SerializeField][Tooltip("시작 카드 개수를 정합니다")] int startCardCount;

    [Header("Properties")]
    public bool isLoading; // 게임 끝나면 isLoading을 true로 하면 카드와 엔티티 클릭방지
    public bool myTurn;

    enum ETurnMode { My, Other }
    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);

    public static Action<bool> OnAddCard;

    void GameSetup()
    {
        if (fastMode)
            delay05 = new WaitForSeconds(0.05f);

        switch (eTurnMode)
        {
            case ETurnMode.My:
                myTurn = true;
                break;
            case ETurnMode.Other:
                myTurn = false;
                break;
        }
    }

    public IEnumerator StartGameCo()
    {
        GameSetup();
        isLoading = true;

        for (int i = 0; i < startCardCount; i++)
        {
            yield return delay05;
            OnAddCard?.Invoke(false);
            yield return delay05;
            OnAddCard?.Invoke(true);
        }

        // 플레이어 카드 추가 (모두 앞면)
        //for (int i = 0; i < startCardCount; i++)
        //{
        //    yield return delay05;
        //    OnAddCard?.Invoke(true); // 플레이어 카드는 항상 앞면
        //}

        //// 몬스터 카드 추가 (첫 번째는 앞면, 두 번째는 뒷면)
        //bool isMonsterFront = false; // 첫 번째 몬스터 카드는 앞면

        //for (int i = 0; i < startCardCount; i++)
        //{
        //    yield return delay05;
        //    OnAddCard?.Invoke(isMonsterFront);
        //}
        StartCoroutine(StartTurnCo());
    }

    IEnumerator StartTurnCo()
    {
        isLoading = true;
        //if (myTurn)
        //    GameManager.Inst.Notification("나의 턴");

        yield return delay07;
        // 플레이어의 카드 리스트가 비어 있을 때만 카드 2장을 뽑기
        if (CardManager.Inst.MyCards.Count == 0)
        {
             yield return delay07; // 뽑기 전 대기
             OnAddCard?.Invoke(myTurn); // 플레이어 카드 뽑기 이벤트 호출
             OnAddCard?.Invoke(false);  // 몬스터 카드 뽑기 이벤트 호출
        }
        yield return delay07;
        isLoading = false;
        //OnTurnStarted?.Invoke(myTurn);
    }

    public void EndTurn()
    {
        myTurn = !myTurn;
        StartCoroutine(StartTurnCo());
    }
}
