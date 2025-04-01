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
    [SerializeField][Tooltip("���� �� ��带 ���մϴ�")] ETurnMode eTurnMode;
    [SerializeField][Tooltip("ī�� ����� �ſ� �������ϴ�")] bool fastMode;
    [SerializeField][Tooltip("���� ī�� ������ ���մϴ�")] int startCardCount;

    [Header("Properties")]
    public bool isLoading; // ���� ������ isLoading�� true�� �ϸ� ī��� ��ƼƼ Ŭ������
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

        // �÷��̾� ī�� �߰� (��� �ո�)
        //for (int i = 0; i < startCardCount; i++)
        //{
        //    yield return delay05;
        //    OnAddCard?.Invoke(true); // �÷��̾� ī��� �׻� �ո�
        //}

        //// ���� ī�� �߰� (ù ��°�� �ո�, �� ��°�� �޸�)
        //bool isMonsterFront = false; // ù ��° ���� ī��� �ո�

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
        //    GameManager.Inst.Notification("���� ��");

        yield return delay07;
        // �÷��̾��� ī�� ����Ʈ�� ��� ���� ���� ī�� 2���� �̱�
        if (CardManager.Inst.MyCards.Count == 0)
        {
             yield return delay07; // �̱� �� ���
             OnAddCard?.Invoke(myTurn); // �÷��̾� ī�� �̱� �̺�Ʈ ȣ��
             OnAddCard?.Invoke(false);  // ���� ī�� �̱� �̺�Ʈ ȣ��
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
