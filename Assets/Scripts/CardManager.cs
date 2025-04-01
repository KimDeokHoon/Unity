using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;                 // ������ ī�带 �̴� �迭
    [SerializeField] PlayerItemSO playeritemSO;     // �÷��̾��� ī�带 �̴� �迭
    [SerializeField] GameObject cardPrefab;
    //[SerializeField] List<Card> myCards;
    [SerializeField] private List<Card> myCards;
    [SerializeField] List<Card> otherCards;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cemeteryPoint; // CemeteryPoint�� Transform�� �����Ϳ��� �Ҵ�
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] Transform otherCardLeft;
    [SerializeField] Transform otherCardRight;

    public Item item;
    public PlayerItem playeritem;

    List<Item> itemBuffer;
    List<PlayerItem> playeritemBuffer;

    bool isMyCardDrag;
    bool onMyCardArea;
    int myPutCount;
    bool CanMouseInput => TurnManager.Inst.myTurn && !TurnManager.Inst.isLoading;

    Card selectCard;
    private Monster targetPick; // targetPick�� Monster Ÿ������ ����

    public List<Card> MyCards => myCards; // �б� ���� ������Ƽ

    int addcount = 0;   // AddCard�޼��� ȣ���� 2���Ǿ��� �� ���� ���� �Ǻ��ϱ� ���� ����
    //private MessageDisplay messageDisplay; // MessageDisplay ���� �߰�

    // ī�� ������ �޼��� �߰�
    public void FlipAllCards()
    {
        foreach (Card card in otherCards)
        {
            if (card != null)
            {
                card.FlipCard(); // FlipCard �޼��� ȣ��
            }
        }
    }

    public PlayerItem PlayerPopItem()
    {
        // �÷��̾� ������ ���۰� ��������� �������� ����
        if (playeritemBuffer.Count == 0)
            PlayerSetupItemBuffer();

        // Ȯ�� ������� ī�� �̱�
        // ��ü Ȯ�� �ջ�
        double totalPercent = playeritemBuffer.Sum(playeritem => playeritem.percent);
        // 0�� totalPercent ������ ���� �� ����
        double randomValue = Random.Range(0f, 1f) * totalPercent;

        double cumulativePercent = 0; // ���� Ȯ�� �ʱ�ȭ
        PlayerItem selectedItem = null; // ���õ� ������ �ʱ�ȭ

        // ������ ���۸� ��ȸ�ϸ� ���� ���� ���� Ȯ�� ��
        foreach (var playeritem in playeritemBuffer)
        {
            cumulativePercent += playeritem.percent; // ���� Ȯ�� ������Ʈ
            if (randomValue < cumulativePercent) // ���� ���� ���� Ȯ������ ������
            {
                selectedItem = playeritem; // ���õ� ī�� ����
                break; // �ݺ��� ����
            }
        }

        // ���õ� �������� ���� ���
        if (selectedItem != null)
        {
            playeritemBuffer.Remove(selectedItem); // ���õ� ī�带 ���ۿ��� ����
            AdjustProbabilities(); // ���� ī�� Ȯ�� ����
            return selectedItem;   // ���õ� ī�� ��ȯ
        }

        return null; // �⺻������ null ��ȯ (���� ��)
    }


    private void AdjustProbabilities()
    {
        double totalPercent = playeritemBuffer.Sum(playeritem => playeritem.percent);
        if (totalPercent == 0)  // ������ 0�̸� ������ �ʿ� ����
            return; 

        foreach (var playeritem in playeritemBuffer)
        {
            playeritem.percent /= totalPercent; // ���ο� Ȯ�� ���
        }
    }

    void PlayerSetupItemBuffer()
    {
        playeritemBuffer = new List<PlayerItem>(100);

        // ��� ī�带 ���������� ����Ʈ�� �߰�
        for (int i = 0; i < playeritemSO.playeritems.Length; i++)
        {
            PlayerItem playeritem = playeritemSO.playeritems[i];
            playeritemBuffer.Add(playeritem);
        }
    }

    public Item PopItem()
    {
        if (itemBuffer.Count == 0)
            SetupItemBuffer();

        Item item = itemBuffer[0];
        itemBuffer.RemoveAt(0);
        return item;
    }

    void SetupItemBuffer()
    {
        itemBuffer = new List<Item>(100);
        for (int i = 0; i < itemSO.items.Length; i++)
        {
            Item item = itemSO.items[i];
            for (int j = 0; j < item.percent; j++)
                itemBuffer.Add(item);
        }

        for (int i = 0; i < itemBuffer.Count; i++)
        {
            int rand = Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
    }

    void Start()
    {
        PlayerSetupItemBuffer(); // �� �޼��带 ȣ���Ͽ� ���۸� ����
        SetupItemBuffer();
        TurnManager.OnAddCard += AddCard;
        //messageDisplay = FindObjectOfType<MessageDisplay>();
    }
    void OnDestory()
    {
        TurnManager.OnAddCard -= AddCard;
    }

    void Update()
    {
        if (isMyCardDrag)
            CardDrag();

        DetectCardArea();
    }

    void AddCard(bool isMine)
    {
        if (isMine == true)
        {
            // ���� ī�� ����
            var cardObjectLeft = Instantiate(cardPrefab, myCardLeft.position, Utils.QI);
            var cardLeft = cardObjectLeft.GetComponent<Card>();
            cardLeft.Setup(isMine);
            myCards.Add(cardLeft); // �÷��̾� ī�� ����Ʈ�� �߰�

            // ������ ī�� ����
            var cardObjectRight = Instantiate(cardPrefab, myCardRight.position, Utils.QI);
            var cardRight = cardObjectRight.GetComponent<Card>();
            cardRight.Setup(isMine);
            myCards.Add(cardRight); // �÷��̾� ī�� ����Ʈ�� �߰�

            // ���� �Ǻ� ȣ�� �� ��� Ȯ��
            bool isGwangDdaeng = CheckForGwangDdaeng();
            bool isDdaeng = CheckForDdaeng();
            bool isMiddle = CheckForMiddle();
            bool isSpecial = CheckForSpecial();

            // �� ���� �Ǻ� ȣ�� (���� ������ �ش���� ���� ���)
            if (!isGwangDdaeng && !isDdaeng && !isMiddle && !isSpecial)
            {
                CheckForKkeut();
            }
        }
        else
        {
            // ���� ī�� ����
            var MonstercardObjectLeft = Instantiate(cardPrefab, otherCardLeft.position, Utils.QI);
            var MonstercardLeft = MonstercardObjectLeft.GetComponent<Card>();
            MonstercardLeft.Setup(false);
            otherCards.Add(MonstercardLeft); // �÷��̾� ī�� ����Ʈ�� �߰�

            // ������ ī�� ����
            var MonstercardObjectRight = Instantiate(cardPrefab, otherCardRight.position, Utils.QI);
            var MonstercardRight = MonstercardObjectRight.GetComponent<Card>();
            MonstercardRight.Setup(false);
            otherCards.Add(MonstercardRight); // �÷��̾� ī�� ����Ʈ�� �߰�

            // ���� �Ǻ� ȣ�� �� ��� Ȯ��
            bool isMonsterGwangDdaeng = CheckForMonsterGwangDdaeng();
            bool isMonsterDdaeng = CheckForMonsterDdaeng();
            bool isMonsterMiddle = CheckForMonsterMiddle();
            bool isMonsterSpecial = CheckForMonsterSpecial();

            // �� ���� �Ǻ� ȣ�� (���� ������ �ش���� ���� ���)
            if (!isMonsterGwangDdaeng && !isMonsterDdaeng && !isMonsterMiddle && !isMonsterSpecial)
            {
                CheckForMonsterKkeut();
            }
        }

        addcount++;

        // ī�� �߰� �� ���� �Ǻ�
        if (addcount == 2) 
        {
            Verdict();
        }
    }

    void SetOriginOrder(bool isMine)
    {
        int count = isMine ? myCards.Count : otherCards.Count;
        for (int i = 0; i < count; i++)
        {
            var targetCard = isMine ? myCards[i] : otherCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }

    // �÷��̾� ī�� ���� �Ǻ� �޼��� ���� 
    public bool CheckForGwangDdaeng()
    {
        if (MyCards.Count < 2) 
            return false;

        var cardLeft = MyCards[MyCards.Count - 2];
        var cardRight = MyCards[MyCards.Count - 1];

        if (cardLeft.playeritem.type == "��" && cardRight.playeritem.type == "��")
        {
            int monthLeft = cardLeft.playeritem.month;
            int monthRight = cardRight.playeritem.month;

            if ((monthLeft == 3 && monthRight == 8) || (monthLeft == 8 && monthRight == 3))
            {
                Debug.Log("38 �����Դϴ�!");
                Ability.scores = 28;
                Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                return true; // ������ �ش�
            }
            else if ((monthLeft == 1 && monthRight == 8) || (monthLeft == 8 && monthRight == 1))
            {
                Debug.Log("18 �����Դϴ�!");
                Ability.scores = 27;
                Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                return true; // ������ �ش�
            }
            else if ((monthLeft == 1 && monthRight == 3) || (monthLeft == 3 && monthRight == 1))
            {
                Debug.Log("13 �����Դϴ�!");
                Ability.scores = 27;
                Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                return true; // ������ �ش�
            }
        }
        return false; // ������ �ش����� ����
    }


    public bool CheckForDdaeng()
    {
        if (MyCards.Count < 2) 
            return false;

        var cardLeft = MyCards[MyCards.Count - 2];
        var cardRight = MyCards[MyCards.Count - 1];

        if (cardLeft.playeritem.month == cardRight.playeritem.month)
        {
            int month = cardLeft.playeritem.month;

            // �� �Ǻ�
            if (month >= 1 && month <= 10) // 1���� 10������ ���� üũ
            {
                Debug.Log($"{month} ���Դϴ�!");
                if (month == 10)
                {
                    Ability.scores = 26;
                    Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                }

                else if (month == 9)
                {
                    Ability.scores = 25;
                    Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                }

                else if (month == 8)
                {
                    Ability.scores = 24;
                    Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                }

                else if (month == 7)
                {
                    Ability.scores = 23;
                    Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                }

                else if (month == 6)
                {
                    Ability.scores = 22;
                    Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                }

                else if (month == 5)
                {
                    Ability.scores = 21;
                    Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                }

                else if (month == 4)
                {
                    Ability.scores = 20;
                    Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                }

                else if (month == 3)
                {
                    Ability.scores = 19;
                    Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                }

                else if (month == 2)
                {
                    Ability.scores = 18;
                    Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                }

                else
                {
                    Ability.scores = 17;
                    Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
                }
                return true; // ���� �ش�
            }
        }
        return false; // ���� �ش����� ����
    }

    public bool CheckForMiddle()
    {
        if (MyCards.Count < 2) 
            return false;

        var cardLeft = MyCards[MyCards.Count - 2];
        var cardRight = MyCards[MyCards.Count - 1];

        int monthLeft = cardLeft.playeritem.month;
        int monthRight = cardRight.playeritem.month;

        // �߰� ���� �Ǻ�
        if ((monthLeft == 1 && monthRight == 2) || (monthLeft == 2 && monthRight == 1))
        {
            Debug.Log("�˸��Դϴ�!");
            Ability.scores = 16;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }
        else if ((monthLeft == 1 && monthRight == 4) || (monthLeft == 4 && monthRight == 1))
        {
            Debug.Log("�����Դϴ�!");
            Ability.scores = 15;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }
        else if ((monthLeft == 1 && monthRight == 9) || (monthLeft == 9 && monthRight == 1))
        {
            Debug.Log("�����Դϴ�!");
            Ability.scores = 14;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }
        else if ((monthLeft == 1 && monthRight == 10) || (monthLeft == 10 && monthRight == 1))
        {
            Debug.Log("����Դϴ�!");
            Ability.scores = 13;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }
        else if ((monthLeft == 4 && monthRight == 10) || (monthLeft == 10 && monthRight == 4))
        {
            Debug.Log("����Դϴ�!");
            Ability.scores = 12;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }
        else if ((monthLeft == 4 && monthRight == 6) || (monthLeft == 6 && monthRight == 4))
        {
            Debug.Log("�����Դϴ�!");
            Ability.scores = 11;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }

        return false; // �߰� ������ �ش����� ����
    }


    public bool CheckForSpecial()
    {
        if (MyCards.Count < 2) 
            return false;

        var cardLeft = MyCards[MyCards.Count - 2];
        var cardRight = MyCards[MyCards.Count - 1];

        int monthLeft = cardLeft.playeritem.month;
        int monthRight = cardRight.playeritem.month;
        string typeLeft = cardLeft.playeritem.type;
        string typeRight = cardRight.playeritem.type;

        // Ư�� ���� �Ǻ�
        if ((monthLeft == 4 && typeLeft == "Ư��1" && monthRight == 7 && typeRight == "Ư��") ||
            (monthLeft == 7 && typeLeft == "Ư��" && monthRight == 4 && typeRight == "Ư��1"))
        {
            Debug.Log("�������Դϴ�!");
            if (Monster.monsterscores == 27) // ������ 27���̶�� ���� 1 3, 3 8 ���� �� �ϳ���� ���� �ǹ�
            {
                Ability.scores = 28; // ������ �¸�
                Debug.Log("�����簡 �¸�! ���� ����: " + Ability.scores);
            }
            else
            {
                Ability.scores = 2; // �ٸ� ���տ� ���� �Ѳ����� ���
                Debug.Log("���� ����: " + Ability.scores);
            }
            return true; // Ư�� ������ �ش�
        }
        else if ((monthLeft == 3 && typeLeft == "��" && monthRight == 7 && typeRight == "Ư��") ||
                 (monthLeft == 7 && typeLeft == "Ư��" && monthRight == 3 && typeRight == "��"))
        {
            Debug.Log("�������Դϴ�!");
            if (Monster.monsterscores >= 17 && Monster.monsterscores <= 25) // ������ 17 ~ 25�� ���̶�� ���� 1 ~ 9�� �� �ϳ���� ���� �ǹ�
            {
                Ability.scores = 26; // ������ �¸�
                Debug.Log("������ �¸�! ���� ����: " + Ability.scores);
            }
            else
            {
                Ability.scores = 1; // �ٸ� ���տ� ���� ����(0��)���� ���
                Debug.Log("���� ����: " + Ability.scores);
            }
            return true; // Ư�� ������ �ش�
        }
        else if ((monthLeft == 4 && typeLeft == "Ư��1" && monthRight == 9 && typeRight == "Ư��1") ||
                 (monthLeft == 9 && typeLeft == "Ư��1" && monthRight == 4 && typeRight == "Ư��1"))
        {
            Debug.Log("���ֱ��������Դϴ�!");
            // ��� ������ 1 ~ 25 ������ ��, ����
            if (Monster.monsterscores >= 1 && Monster.monsterscores <= 25)
            {
                Debug.Log("���ֱ������� �¸�! ����!");
            }
            else
            {
                Debug.Log("���ֱ������� �й�!");
            }
            return true; // Ư�� ������ �ش�
        }
        else if ((monthLeft == 4 && typeLeft == "Ư��" && monthRight == 9 && typeRight == "Ư��") ||
                 (monthLeft == 9 && typeLeft == "Ư��" && monthRight == 4 && typeRight == "Ư��") ||
                 (monthLeft == 4 && typeLeft == "Ư��1" && monthRight == 9 && typeRight == "Ư��") ||
                 (monthLeft == 9 && typeLeft == "Ư��" && monthRight == 4 && typeRight == "Ư��1") ||
                 (monthLeft == 4 && typeLeft == "Ư��" && monthRight == 9 && typeRight == "Ư��1") ||
                 (monthLeft == 9 && typeLeft == "Ư��1" && monthRight == 4 && typeRight == "Ư��"))
        {
            Debug.Log("�����Դϴ�!");
            // ��� ������ 1 ~ 16 ������ ��, ����
            if (Monster.monsterscores >= 1 && Monster.monsterscores <= 16)
            {
                Debug.Log("���� �¸�! ����!");
            }
            else
            {
                Debug.Log("���� �й�!");
            }
            return true; // Ư�� ������ �ش�
        }

        return false; // Ư�� ������ �ش����� ����
    }


    public void CheckForKkeut()
    {
        if (MyCards.Count < 2) // ī�尡 2�� �̻��̾�� ��
            return;

        // �� ���� ī�� �������� (��: ������ �� ��)
        var cardLeft = MyCards[MyCards.Count - 2];
        var cardRight = MyCards[MyCards.Count - 1];

        int monthLeft = cardLeft.playeritem.month;
        int monthRight = cardRight.playeritem.month;

        // �� ī���� �� �� �ջ�
        int totalMonth = monthLeft + monthRight;
        int lastDigit = totalMonth % 10; // ���� �ڸ� ����

        // �� ���� �Ǻ�
        if (lastDigit == 9)
        {
            Debug.Log("����(��ȩ��)�Դϴ�!");
            Ability.scores = 10;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
        }
        else if (lastDigit == 8)
        {
            Debug.Log("�������Դϴ�!");
            Ability.scores = 9;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
        }
        else if (lastDigit == 7)
        {
            Debug.Log("�ϰ����Դϴ�!");
            Ability.scores = 8;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
        }
        else if (lastDigit == 6)
        {
            Debug.Log("�������Դϴ�!");
            Ability.scores = 7;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
        }
        else if (lastDigit == 5)
        {
            Debug.Log("�ټ����Դϴ�!");
            Ability.scores = 6;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
        }
        else if (lastDigit == 4)
        {
            Debug.Log("�ײ��Դϴ�!");
            Ability.scores = 5;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
        }
        else if (lastDigit == 3)
        {
            Debug.Log("�����Դϴ�!");
            Ability.scores = 4;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
        }
        else if (lastDigit == 2)
        {
            Debug.Log("�β��Դϴ�!");
            Ability.scores = 3;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
        }
        else if (lastDigit == 1)
        {
            Debug.Log("�Ѳ��Դϴ�!");
            Ability.scores = 2;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
        }
        else if (lastDigit == 0)
        {
            Debug.Log("�����Դϴ�! (�� ���� ���� ���� 0�Դϴ�.)");
            Ability.scores = 1;
            Debug.Log($"���� �÷��̾� ����: {Ability.scores}"); // ���� ���
        }
        else
        {
            Debug.Log("�� �� ���� ���Դϴ�.");
        }
    }

    // �÷��̾� ���� �Ǻ� �޼��� ��

    // ====================================================== // 

    // ���� ���� �Ǻ� �޼��� ����
    public bool CheckForMonsterGwangDdaeng()
    {
        if (otherCards.Count < 2)
            return false;

        var cardLeft = otherCards[otherCards.Count - 2];
        var cardRight = otherCards[otherCards.Count - 1];

        if (cardLeft.item.type == "��" && cardRight.item.type == "��")
        {
            int monthLeft = cardLeft.item.month;
            int monthRight = cardRight.item.month;

            if ((monthLeft == 3 && monthRight == 8) || (monthLeft == 8 && monthRight == 3))
            {
                Debug.Log("38 �����Դϴ�! (����)");
                Monster.monsterscores = 28;
                Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                return true;
            }
            else if ((monthLeft == 1 && monthRight == 8) || (monthLeft == 8 && monthRight == 1))
            {
                Debug.Log("18 �����Դϴ�! (����)");
                Monster.monsterscores = 27;
                Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                return true;
            }
            else if ((monthLeft == 1 && monthRight == 3) || (monthLeft == 3 && monthRight == 1))
            {
                Debug.Log("13 �����Դϴ�! (����)");
                Monster.monsterscores = 26;
                Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                return true;
            }
        }
        return false;
    }


    public bool CheckForMonsterDdaeng()
    {
        if (otherCards.Count < 2)
            return false;

        var cardLeft = otherCards[otherCards.Count - 2];
        var cardRight = otherCards[otherCards.Count - 1];

        if (cardLeft.item.month == cardRight.item.month)
        {
            int month = cardLeft.item.month;

            // �� �Ǻ�
            if (month >= 1 && month <= 10) // 1���� 10������ ���� üũ
            {
                Debug.Log($"{month} ���Դϴ�! (����)");
                if (month == 10)
                {
                    Monster.monsterscores = 26;
                    Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                }

                else if (month == 9)
                {
                    Monster.monsterscores = 25;
                    Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                }

                else if (month == 8)
                {
                    Monster.monsterscores = 24;
                    Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                }

                else if (month == 7)
                {
                    Monster.monsterscores = 23;
                    Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                }

                else if (month == 6)
                {
                    Monster.monsterscores = 22;
                    Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                }

                else if (month == 5)
                {
                    Monster.monsterscores = 21;
                    Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                }

                else if (month == 4)
                {
                    Monster.monsterscores = 20;
                    Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                }

                else if (month == 3)
                {
                    Monster.monsterscores = 19;
                    Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                }

                else if (month == 2)
                {
                    Monster.monsterscores = 18;
                    Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                }

                else
                {
                    Monster.monsterscores = 17;
                    Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
                }
                return true; // ���� �ش�
            }
        }
        return false; // ���� �ش����� ����
    }

    public bool CheckForMonsterMiddle()
    {
        if (otherCards.Count < 2)
            return false;

        var cardLeft = otherCards[otherCards.Count - 2];
        var cardRight = otherCards[otherCards.Count - 1];

        int monthLeft = cardLeft.item.month;
        int monthRight = cardRight.item.month;

        // �߰� ���� �Ǻ�
        if ((monthLeft == 1 && monthRight == 2) || (monthLeft == 2 && monthRight == 1))
        {
            Debug.Log("�˸��Դϴ�! (����)");
            Monster.monsterscores = 16;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }
        else if ((monthLeft == 1 && monthRight == 4) || (monthLeft == 4 && monthRight == 1))
        {
            Debug.Log("�����Դϴ�! (����)");
            Monster.monsterscores = 15;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }
        else if ((monthLeft == 1 && monthRight == 9) || (monthLeft == 9 && monthRight == 1))
        {
            Debug.Log("�����Դϴ�! (����)");
            Monster.monsterscores = 14;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }
        else if ((monthLeft == 1 && monthRight == 10) || (monthLeft == 10 && monthRight == 1))
        {
            Debug.Log("����Դϴ�! (����)");
            Monster.monsterscores = 13;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }
        else if ((monthLeft == 4 && monthRight == 10) || (monthLeft == 10 && monthRight == 4))
        {
            Debug.Log("����Դϴ�! (����)");
            Monster.monsterscores = 12;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }
        else if ((monthLeft == 4 && monthRight == 6) || (monthLeft == 6 && monthRight == 4))
        {
            Debug.Log("�����Դϴ�! (����)");
            Monster.monsterscores = 11;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
            return true; // �߰� ������ �ش�
        }

        return false; // �߰� ������ �ش����� ����
    }


    public bool CheckForMonsterSpecial()
    {
        if (otherCards.Count < 2)
            return false;

        var cardLeft = otherCards[otherCards.Count - 2];
        var cardRight = otherCards[otherCards.Count - 1];

        int monthLeft = cardLeft.item.month;
        int monthRight = cardRight.item.month;
        string typeLeft = cardLeft.item.type;
        string typeRight = cardRight.item.type;

        // Ư�� ���� �Ǻ�
        if ((monthLeft == 4 && typeLeft == "Ư��1" && monthRight == 7 && typeRight == "Ư��") ||
            (monthLeft == 7 && typeLeft == "Ư��" && monthRight == 4 && typeRight == "Ư��1"))
        {
            Debug.Log("�������Դϴ�! (����)");
            if (Ability.scores == 27) // ������ 27���̶�� ���� 1 3, 3 8 ���� �� �ϳ���� ���� �ǹ�
            {
                Monster.monsterscores = 28; // ������ �¸�
                Debug.Log("(����) �����簡 �¸�! ���� ����: " + Monster.monsterscores);
            }
            else
            {
                Monster.monsterscores = 2; // �ٸ� ���տ� ���� �Ѳ����� ���
                Debug.Log("(����) ���� ����: " + Monster.monsterscores);
            }
            return true; // Ư�� ������ �ش�
        }
        else if ((monthLeft == 3 && typeLeft == "��" && monthRight == 7 && typeRight == "Ư��") ||
                 (monthLeft == 7 && typeLeft == "Ư��" && monthRight == 3 && typeRight == "��"))
        {
            Debug.Log("�������Դϴ�! (����)");
            if (Ability.scores >= 17 && Ability.scores <= 25) // ������ 17 ~ 25�� ���̶�� ���� 1 ~ 9�� �� �ϳ���� ���� �ǹ�
            {
                Monster.monsterscores = 26; // ������ �¸�
                Debug.Log("(����) ������ �¸�! ���� ����: " + Monster.monsterscores);
            }
            else
            {
                Monster.monsterscores = 1; // �ٸ� ���տ� ���� ����(0��)���� ���
                Debug.Log("(����) ���� ����: " + Monster.monsterscores);
            }
            return true; // Ư�� ������ �ش�
        }
        else if ((monthLeft == 4 && typeLeft == "Ư��1" && monthRight == 9 && typeRight == "Ư��1") ||
                 (monthLeft == 9 && typeLeft == "Ư��1" && monthRight == 4 && typeRight == "Ư��1"))
        {
            Debug.Log("���ֱ��������Դϴ�! (����)");
            // ��� ������ 1 ~ 25 ������ ��, ����
            if (Ability.scores >= 1 && Ability.scores <= 25)
            {
                Debug.Log("���ֱ������� �¸�! ����! (����)");
            }
            else
            {
                Debug.Log("���ֱ������� �й�! (����)");
            }
            return true; // Ư�� ������ �ش�
        }
        else if ((monthLeft == 4 && typeLeft == "Ư��" && monthRight == 9 && typeRight == "Ư��") ||
                 (monthLeft == 9 && typeLeft == "Ư��" && monthRight == 4 && typeRight == "Ư��") ||
                 (monthLeft == 4 && typeLeft == "Ư��1" && monthRight == 9 && typeRight == "Ư��") ||
                 (monthLeft == 9 && typeLeft == "Ư��" && monthRight == 4 && typeRight == "Ư��1") ||
                 (monthLeft == 4 && typeLeft == "Ư��" && monthRight == 9 && typeRight == "Ư��1") ||
                 (monthLeft == 9 && typeLeft == "Ư��1" && monthRight == 4 && typeRight == "Ư��"))
        {
            Debug.Log("�����Դϴ�! (����)");
            // ��� ������ 1 ~ 16 ������ ��, ����
            if (Ability.scores >= 1 && Ability.scores <= 16)
            {
                Debug.Log("���� �¸�! ����! (����)");
            }
            else
            {
                Debug.Log("���� �й�! (����)");
            }
            return true; // Ư�� ������ �ش�
        }

        return false; // Ư�� ������ �ش����� ����
    }


    public void CheckForMonsterKkeut()
    {
        if (otherCards.Count < 2)
            return;

        var cardLeft = otherCards[otherCards.Count - 2];
        var cardRight = otherCards[otherCards.Count - 1];

        int monthLeft = cardLeft.item.month;
        int monthRight = cardRight.item.month;

        // �� ī���� �� �� �ջ�
        int totalMonth = monthLeft + monthRight;
        int lastDigit = totalMonth % 10; // ���� �ڸ� ����

        // �� ���� �Ǻ�
        if (lastDigit == 9)
        {
            Debug.Log("����(��ȩ��)�Դϴ�! (����)");
            Monster.monsterscores = 10;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
        }
        else if (lastDigit == 8)
        {
            Debug.Log("�������Դϴ�! (����)");
            Monster.monsterscores = 9;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
        }
        else if (lastDigit == 7)
        {
            Debug.Log("�ϰ����Դϴ�! (����)");
            Monster.monsterscores = 8;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
        }
        else if (lastDigit == 6)
        {
            Debug.Log("�������Դϴ�! (����)");
            Monster.monsterscores = 7;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
        }
        else if (lastDigit == 5)
        {
            Debug.Log("�ټ����Դϴ�! (����)");
            Monster.monsterscores = 6;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
        }
        else if (lastDigit == 4)
        {
            Debug.Log("�ײ��Դϴ�! (����)");
            Monster.monsterscores = 5;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
        }
        else if (lastDigit == 3)
        {
            Debug.Log("�����Դϴ�! (����)");
            Monster.monsterscores = 4;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
        }
        else if (lastDigit == 2)
        {
            Debug.Log("�β��Դϴ�! (����)");
            Monster.monsterscores = 3;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
        }
        else if (lastDigit == 1)
        {
            Debug.Log("�Ѳ��Դϴ�! (����)");
            Monster.monsterscores = 2;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
        }
        else if (lastDigit == 0)
        {
            Debug.Log("�����Դϴ�! (�� ���� ���� ���� 0�Դϴ�.) (����)");
            Monster.monsterscores = 1;
            Debug.Log($"���� ����: {Monster.monsterscores}"); // ���� ���
        }
        else
        {
            Debug.Log("�� �� ���� ���Դϴ�.");
        }
    }

    public void Verdict()
    {
        if (Ability.scores > Monster.monsterscores)
        {
            Debug.Log("�÷��̾��� �¸��Դϴ�!");
        }

        else if (Ability.scores < Monster.monsterscores)
        {
            Debug.Log("������ �¸��Դϴ�!");
        }

        else
        {
            Debug.Log("�����ϴ�!");
        }
    }
    //void CardAlignment(bool isMine)
    //{
    //    List<PRS> originCardPRSs = new List<PRS>();
    //    if (isMine)
    //        originCardPRSs = Alignment(myCardLeft, myCardRight, myCards.Count, -10f, new Vector3(6f, 8f, 1f));
    //    else
    //        originCardPRSs = Alignment(otherCardLeft, otherCardRight, otherCards.Count, 30f, new Vector3(8f, 10f, 1f));

    //    var targetCards = isMine ? myCards : otherCards;
    //    for (int i = 0; i < targetCards.Count; i++)
    //    {
    //        var targetCard = targetCards[i];

    //        targetCard.originPRS = originCardPRSs[i];
    //        targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
    //    }
    //}

    //List<PRS> Alignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    //{
    //    // ī���� ���� ������ ������ �迭
    //    float[] objLerps = new float[objCount];
    //    List<PRS> results = new List<PRS>(objCount);

    //    // ī�� ���� ���� ���� ���� ����
    //    switch (objCount)
    //    {
    //        case 1:
    //            objLerps = new float[] { 0.5f };
    //            break;
    //        case 2:
    //            objLerps = new float[] { 0.25f, 0.75f };
    //            break;
    //        case 3:
    //            objLerps = new float[] { 0.1f, 0.55f, 1f };
    //            break;
    //    }

    //    // �� ī���� ��ġ, ȸ�� �� ������ ���
    //    for (int i = 0; i < objCount; i++)
    //    {
    //        var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
    //        var targetRot = Utils.QI; // ȸ���� �⺻�� ���

    //        // ī�带 ���������� ��ġ�� ���� ���� ������ �ʿ� �����Ƿ� y ��ǥ�� ����
    //        targetPos.y = height; // ���ϴ� ���̷� ����

    //        results.Add(new PRS(targetPos, targetRot, scale));
    //    }
    //    return results;
    //}

    public bool TryPutCard(bool isMine)
    {
        // �÷��̾ ī�带 �� �常 ����� �� �ֵ��� �ϴ� ����
        if (isMine && myPutCount >= 1)
            return false;

        if (!isMine && otherCards.Count <= 0)
            return false;

        return false;
    }

    #region MyCard

    public void CardMouseOver(Card card)
    {
        selectCard = card;
        CardPosition(true, card);
    }

    public void CardMouseExit(Card card)
    {
        CardPosition(false, card);
    }

    public void CardMouseDown(Card card)
    {
        isMyCardDrag = true;

        if (!CanMouseInput)
            return;

        selectCard = card;

    }

    public void CardMouseUp()
    {
        isMyCardDrag = false;
        
        if (!CanMouseInput)
            return;       

        selectCard = null; // ���õ� ī�� �ʱ�ȭ
        targetPick = null;
    }

    public void CardDrag()
    {
        // selectCard�� null���� Ȯ��
        if (selectCard == null)
        {
            //Debug.Log("selectCard is null, exiting CardDrag");
            return; // selectCard�� null�̸� �޼��� ����
        }

        if (!onMyCardArea)
        {
            selectCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, selectCard.originPRS.scale), false);
            //EntityManager.Inst.InsertMyEmptyEntity(Utils.MousePos.x);
        }

        // monster Ÿ�� ã��
        bool existTarget = false;
        foreach (var hit in Physics2D.RaycastAll(Utils.MousePos, Vector3.forward))
        {
            Monster monster = hit.collider?.GetComponent<Monster>();
            if (monster != null && !monster.isMine && selectCard.attackable)
            {
                targetPick = monster; // ���⿡�� targetPick�� Ÿ���� Monster�� ����
                existTarget = true;
                break;
            }
        }
        if (!existTarget)
            targetPick = null;
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("MyCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    // ���콺 Ŀ���� ī�忡 �巡�� �Ǿ��ٰ� �������� �� ī�� ���� ��ġ�� ���ư����� �ϴ� �޼���
    void CardPosition(bool isEnlarge, Card card)
    {
        if (isEnlarge)
        {
            // �ƹ��͵� ������� �ʰ� ī�尡 �巡�� �ǵ��� ��
        }
        else
            card.MoveTransform(card.originPRS, false);

        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }

    #endregion
}

