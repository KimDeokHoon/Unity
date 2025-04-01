using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// ġƮ, UI, ��ŷ, ���ӿ���
public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] CoinFlip coinFlip; // CoinFlip ������Ʈ ���� �߰�
    [SerializeField] GameObject turnOptionUI; // ���� ������ UI
    [SerializeField] GameObject functionUI; // ���� ��ư ���� UI
    [SerializeField] public TMP_Text BattingCoin;    // �÷��̾�, ���� ������ �Ⱦ� �ջ�
    

    public int BattingCharge;
    WaitForSeconds delay03 = new WaitForSeconds(0.3f);

    void Start()
    {
        // ���� ���� ��ư ��Ȱ��ȭ (������ ó�� ������ �� ������ ������ �𸣱� ����)
        functionUI.SetActive(false);
        StartCoinFlip();
        StartGame();
    }

    void Update()
    {
#if UNITY_EDITOR
        InputCheatKey();
#endif
    }

    void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
            TurnManager.OnAddCard?.Invoke(true);

        if (Input.GetKeyDown(KeyCode.End))
            TurnManager.OnAddCard?.Invoke(false);

        if (Input.GetKeyDown(KeyCode.Insert))
            TurnManager.Inst.EndTurn();

        if (Input.GetKeyDown(KeyCode.Home))
            CardManager.Inst.FlipAllCards(); // FlipAllCards �޼��� ȣ��
    }

    public void StartGame()
    {
        CollectEntryFees(); // ������ ���� �޼��� ȣ��
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }

    public void CollectEntryFees()
    {
        // �÷��̾�� ���� �ν��Ͻ� ��������
        Ability playerAbility = FindObjectOfType<Ability>();
        Monster monster = FindObjectOfType<Monster>();

        if (playerAbility != null && monster != null)
        {
            BattingCharge += 2000; // ������ �ջ�
            playerAbility.Coin -= 1000; // �÷��̾��� �ܿ� ����
            monster.MonsterCoins -= 1000; // ������ �ܿ� ����

            // UI ������Ʈ
            BattingCoin.text = BattingCharge.ToString();
            playerAbility.UpdatePlayerCoinText(); // �÷��̾� ���� UI ������Ʈ
            monster.UpdateMonsterCoinText(); // ���� ���� UI ������Ʈ
        }
    }

    public void Notification(string message)
    {
        notificationPanel.Show(message);
    }

    public void StartCoinFlip()
    {
        // ���� ������ UI Ȱ��ȭ
        turnOptionUI.SetActive(true);
        coinFlip.FlipCoin(OnCoinFlipComplete); // ���� ������ �޼��� ȣ��
    }

    private void OnCoinFlipComplete(bool isFront)
    {
        // ���� ������ ����� ���� ó��
        if (isFront)
        {
            Notification("�÷��̾��� ��");
            StartCoroutine(delaytime());
        }
        else
        {
            Notification("������ ��");
        }

        // ���� ������ UI ��Ȱ��ȭ
        turnOptionUI.SetActive(false);
    }

    IEnumerator delaytime()
    {
        yield return new WaitForSeconds(1.5f);
        functionUI.SetActive(true);
    }
}


