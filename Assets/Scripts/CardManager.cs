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

    [SerializeField] ItemSO itemSO;                 // 몬스터의 카드를 뽑는 배열
    [SerializeField] PlayerItemSO playeritemSO;     // 플레이어의 카드를 뽑는 배열
    [SerializeField] GameObject cardPrefab;
    //[SerializeField] List<Card> myCards;
    [SerializeField] private List<Card> myCards;
    [SerializeField] List<Card> otherCards;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cemeteryPoint; // CemeteryPoint의 Transform을 에디터에서 할당
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
    private Monster targetPick; // targetPick을 Monster 타입으로 변경

    public List<Card> MyCards => myCards; // 읽기 전용 프로퍼티

    int addcount = 0;   // AddCard메서드 호출이 2번되었을 때 승패 여부 판별하기 위해 선언
    //private MessageDisplay messageDisplay; // MessageDisplay 변수 추가

    // 카드 뒤집기 메서드 추가
    public void FlipAllCards()
    {
        foreach (Card card in otherCards)
        {
            if (card != null)
            {
                card.FlipCard(); // FlipCard 메서드 호출
            }
        }
    }

    public PlayerItem PlayerPopItem()
    {
        // 플레이어 아이템 버퍼가 비어있으면 아이템을 설정
        if (playeritemBuffer.Count == 0)
            PlayerSetupItemBuffer();

        // 확률 기반으로 카드 뽑기
        // 전체 확률 합산
        double totalPercent = playeritemBuffer.Sum(playeritem => playeritem.percent);
        // 0과 totalPercent 사이의 랜덤 값 생성
        double randomValue = Random.Range(0f, 1f) * totalPercent;

        double cumulativePercent = 0; // 누적 확률 초기화
        PlayerItem selectedItem = null; // 선택된 아이템 초기화

        // 아이템 버퍼를 순회하며 랜덤 값과 누적 확률 비교
        foreach (var playeritem in playeritemBuffer)
        {
            cumulativePercent += playeritem.percent; // 누적 확률 업데이트
            if (randomValue < cumulativePercent) // 랜덤 값이 누적 확률보다 작으면
            {
                selectedItem = playeritem; // 선택된 카드 저장
                break; // 반복문 종료
            }
        }

        // 선택된 아이템이 있을 경우
        if (selectedItem != null)
        {
            playeritemBuffer.Remove(selectedItem); // 선택된 카드를 버퍼에서 제거
            AdjustProbabilities(); // 남은 카드 확률 조정
            return selectedItem;   // 선택된 카드 반환
        }

        return null; // 기본적으로 null 반환 (실패 시)
    }


    private void AdjustProbabilities()
    {
        double totalPercent = playeritemBuffer.Sum(playeritem => playeritem.percent);
        if (totalPercent == 0)  // 총합이 0이면 조정할 필요 없음
            return; 

        foreach (var playeritem in playeritemBuffer)
        {
            playeritem.percent /= totalPercent; // 새로운 확률 계산
        }
    }

    void PlayerSetupItemBuffer()
    {
        playeritemBuffer = new List<PlayerItem>(100);

        // 모든 카드를 순차적으로 리스트에 추가
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
        PlayerSetupItemBuffer(); // 이 메서드를 호출하여 버퍼를 설정
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
            // 왼쪽 카드 생성
            var cardObjectLeft = Instantiate(cardPrefab, myCardLeft.position, Utils.QI);
            var cardLeft = cardObjectLeft.GetComponent<Card>();
            cardLeft.Setup(isMine);
            myCards.Add(cardLeft); // 플레이어 카드 리스트에 추가

            // 오른쪽 카드 생성
            var cardObjectRight = Instantiate(cardPrefab, myCardRight.position, Utils.QI);
            var cardRight = cardObjectRight.GetComponent<Card>();
            cardRight.Setup(isMine);
            myCards.Add(cardRight); // 플레이어 카드 리스트에 추가

            // 족보 판별 호출 및 결과 확인
            bool isGwangDdaeng = CheckForGwangDdaeng();
            bool isDdaeng = CheckForDdaeng();
            bool isMiddle = CheckForMiddle();
            bool isSpecial = CheckForSpecial();

            // 끗 족보 판별 호출 (위의 족보에 해당되지 않을 경우)
            if (!isGwangDdaeng && !isDdaeng && !isMiddle && !isSpecial)
            {
                CheckForKkeut();
            }
        }
        else
        {
            // 왼쪽 카드 생성
            var MonstercardObjectLeft = Instantiate(cardPrefab, otherCardLeft.position, Utils.QI);
            var MonstercardLeft = MonstercardObjectLeft.GetComponent<Card>();
            MonstercardLeft.Setup(false);
            otherCards.Add(MonstercardLeft); // 플레이어 카드 리스트에 추가

            // 오른쪽 카드 생성
            var MonstercardObjectRight = Instantiate(cardPrefab, otherCardRight.position, Utils.QI);
            var MonstercardRight = MonstercardObjectRight.GetComponent<Card>();
            MonstercardRight.Setup(false);
            otherCards.Add(MonstercardRight); // 플레이어 카드 리스트에 추가

            // 족보 판별 호출 및 결과 확인
            bool isMonsterGwangDdaeng = CheckForMonsterGwangDdaeng();
            bool isMonsterDdaeng = CheckForMonsterDdaeng();
            bool isMonsterMiddle = CheckForMonsterMiddle();
            bool isMonsterSpecial = CheckForMonsterSpecial();

            // 끗 족보 판별 호출 (위의 족보에 해당되지 않을 경우)
            if (!isMonsterGwangDdaeng && !isMonsterDdaeng && !isMonsterMiddle && !isMonsterSpecial)
            {
                CheckForMonsterKkeut();
            }
        }

        addcount++;

        // 카드 추가 후 승패 판별
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

    // 플레이어 카드 족보 판별 메서드 시작 
    public bool CheckForGwangDdaeng()
    {
        if (MyCards.Count < 2) 
            return false;

        var cardLeft = MyCards[MyCards.Count - 2];
        var cardRight = MyCards[MyCards.Count - 1];

        if (cardLeft.playeritem.type == "광" && cardRight.playeritem.type == "광")
        {
            int monthLeft = cardLeft.playeritem.month;
            int monthRight = cardRight.playeritem.month;

            if ((monthLeft == 3 && monthRight == 8) || (monthLeft == 8 && monthRight == 3))
            {
                Debug.Log("38 광땡입니다!");
                Ability.scores = 28;
                Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                return true; // 광땡에 해당
            }
            else if ((monthLeft == 1 && monthRight == 8) || (monthLeft == 8 && monthRight == 1))
            {
                Debug.Log("18 광땡입니다!");
                Ability.scores = 27;
                Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                return true; // 광땡에 해당
            }
            else if ((monthLeft == 1 && monthRight == 3) || (monthLeft == 3 && monthRight == 1))
            {
                Debug.Log("13 광땡입니다!");
                Ability.scores = 27;
                Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                return true; // 광땡에 해당
            }
        }
        return false; // 광땡에 해당하지 않음
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

            // 땡 판별
            if (month >= 1 && month <= 10) // 1부터 10까지의 월만 체크
            {
                Debug.Log($"{month} 땡입니다!");
                if (month == 10)
                {
                    Ability.scores = 26;
                    Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                }

                else if (month == 9)
                {
                    Ability.scores = 25;
                    Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                }

                else if (month == 8)
                {
                    Ability.scores = 24;
                    Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                }

                else if (month == 7)
                {
                    Ability.scores = 23;
                    Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                }

                else if (month == 6)
                {
                    Ability.scores = 22;
                    Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                }

                else if (month == 5)
                {
                    Ability.scores = 21;
                    Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                }

                else if (month == 4)
                {
                    Ability.scores = 20;
                    Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                }

                else if (month == 3)
                {
                    Ability.scores = 19;
                    Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                }

                else if (month == 2)
                {
                    Ability.scores = 18;
                    Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                }

                else
                {
                    Ability.scores = 17;
                    Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
                }
                return true; // 땡에 해당
            }
        }
        return false; // 땡에 해당하지 않음
    }

    public bool CheckForMiddle()
    {
        if (MyCards.Count < 2) 
            return false;

        var cardLeft = MyCards[MyCards.Count - 2];
        var cardRight = MyCards[MyCards.Count - 1];

        int monthLeft = cardLeft.playeritem.month;
        int monthRight = cardRight.playeritem.month;

        // 중간 족보 판별
        if ((monthLeft == 1 && monthRight == 2) || (monthLeft == 2 && monthRight == 1))
        {
            Debug.Log("알리입니다!");
            Ability.scores = 16;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }
        else if ((monthLeft == 1 && monthRight == 4) || (monthLeft == 4 && monthRight == 1))
        {
            Debug.Log("독사입니다!");
            Ability.scores = 15;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }
        else if ((monthLeft == 1 && monthRight == 9) || (monthLeft == 9 && monthRight == 1))
        {
            Debug.Log("구삥입니다!");
            Ability.scores = 14;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }
        else if ((monthLeft == 1 && monthRight == 10) || (monthLeft == 10 && monthRight == 1))
        {
            Debug.Log("장삥입니다!");
            Ability.scores = 13;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }
        else if ((monthLeft == 4 && monthRight == 10) || (monthLeft == 10 && monthRight == 4))
        {
            Debug.Log("장사입니다!");
            Ability.scores = 12;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }
        else if ((monthLeft == 4 && monthRight == 6) || (monthLeft == 6 && monthRight == 4))
        {
            Debug.Log("세륙입니다!");
            Ability.scores = 11;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }

        return false; // 중간 족보에 해당하지 않음
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

        // 특수 족보 판별
        if ((monthLeft == 4 && typeLeft == "특수1" && monthRight == 7 && typeRight == "특수") ||
            (monthLeft == 7 && typeLeft == "특수" && monthRight == 4 && typeRight == "특수1"))
        {
            Debug.Log("암행어사입니다!");
            if (Monster.monsterscores == 27) // 점수가 27점이라는 것은 1 3, 3 8 광땡 중 하나라는 것을 의미
            {
                Ability.scores = 28; // 암행어사 승리
                Debug.Log("암행어사가 승리! 현재 점수: " + Ability.scores);
            }
            else
            {
                Ability.scores = 2; // 다른 조합에 대해 한끗으로 취급
                Debug.Log("현재 점수: " + Ability.scores);
            }
            return true; // 특수 족보에 해당
        }
        else if ((monthLeft == 3 && typeLeft == "광" && monthRight == 7 && typeRight == "특수") ||
                 (monthLeft == 7 && typeLeft == "특수" && monthRight == 3 && typeRight == "광"))
        {
            Debug.Log("땡잡이입니다!");
            if (Monster.monsterscores >= 17 && Monster.monsterscores <= 25) // 점수가 17 ~ 25점 사이라는 것은 1 ~ 9땡 중 하나라는 것을 의미
            {
                Ability.scores = 26; // 땡잡이 승리
                Debug.Log("땡잡이 승리! 현재 점수: " + Ability.scores);
            }
            else
            {
                Ability.scores = 1; // 다른 조합에 대해 망통(0끗)으로 취급
                Debug.Log("현재 점수: " + Ability.scores);
            }
            return true; // 특수 족보에 해당
        }
        else if ((monthLeft == 4 && typeLeft == "특수1" && monthRight == 9 && typeRight == "특수1") ||
                 (monthLeft == 9 && typeLeft == "특수1" && monthRight == 4 && typeRight == "특수1"))
        {
            Debug.Log("멍텅구리구사입니다!");
            // 상대 점수가 1 ~ 25 사이일 때, 재대결
            if (Monster.monsterscores >= 1 && Monster.monsterscores <= 25)
            {
                Debug.Log("멍텅구리구사 승리! 재대결!");
            }
            else
            {
                Debug.Log("멍텅구리구사 패배!");
            }
            return true; // 특수 족보에 해당
        }
        else if ((monthLeft == 4 && typeLeft == "특수" && monthRight == 9 && typeRight == "특수") ||
                 (monthLeft == 9 && typeLeft == "특수" && monthRight == 4 && typeRight == "특수") ||
                 (monthLeft == 4 && typeLeft == "특수1" && monthRight == 9 && typeRight == "특수") ||
                 (monthLeft == 9 && typeLeft == "특수" && monthRight == 4 && typeRight == "특수1") ||
                 (monthLeft == 4 && typeLeft == "특수" && monthRight == 9 && typeRight == "특수1") ||
                 (monthLeft == 9 && typeLeft == "특수1" && monthRight == 4 && typeRight == "특수"))
        {
            Debug.Log("구사입니다!");
            // 상대 점수가 1 ~ 16 사이일 때, 재대결
            if (Monster.monsterscores >= 1 && Monster.monsterscores <= 16)
            {
                Debug.Log("구사 승리! 재대결!");
            }
            else
            {
                Debug.Log("구사 패배!");
            }
            return true; // 특수 족보에 해당
        }

        return false; // 특수 족보에 해당하지 않음
    }


    public void CheckForKkeut()
    {
        if (MyCards.Count < 2) // 카드가 2장 이상이어야 함
            return;

        // 두 장의 카드 가져오기 (예: 마지막 두 장)
        var cardLeft = MyCards[MyCards.Count - 2];
        var cardRight = MyCards[MyCards.Count - 1];

        int monthLeft = cardLeft.playeritem.month;
        int monthRight = cardRight.playeritem.month;

        // 두 카드의 월 값 합산
        int totalMonth = monthLeft + monthRight;
        int lastDigit = totalMonth % 10; // 일의 자리 추출

        // 끗 족보 판별
        if (lastDigit == 9)
        {
            Debug.Log("갑오(아홉끗)입니다!");
            Ability.scores = 10;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
        }
        else if (lastDigit == 8)
        {
            Debug.Log("여덟끗입니다!");
            Ability.scores = 9;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
        }
        else if (lastDigit == 7)
        {
            Debug.Log("일곱끗입니다!");
            Ability.scores = 8;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
        }
        else if (lastDigit == 6)
        {
            Debug.Log("여섯끗입니다!");
            Ability.scores = 7;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
        }
        else if (lastDigit == 5)
        {
            Debug.Log("다섯끗입니다!");
            Ability.scores = 6;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
        }
        else if (lastDigit == 4)
        {
            Debug.Log("네끗입니다!");
            Ability.scores = 5;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
        }
        else if (lastDigit == 3)
        {
            Debug.Log("세끗입니다!");
            Ability.scores = 4;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
        }
        else if (lastDigit == 2)
        {
            Debug.Log("두끗입니다!");
            Ability.scores = 3;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
        }
        else if (lastDigit == 1)
        {
            Debug.Log("한끗입니다!");
            Ability.scores = 2;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
        }
        else if (lastDigit == 0)
        {
            Debug.Log("망통입니다! (두 패의 월의 합이 0입니다.)");
            Ability.scores = 1;
            Debug.Log($"현재 플레이어 점수: {Ability.scores}"); // 점수 출력
        }
        else
        {
            Debug.Log("알 수 없는 끗입니다.");
        }
    }

    // 플레이어 족보 판별 메서드 끝

    // ====================================================== // 

    // 몬스터 족보 판별 메서드 시작
    public bool CheckForMonsterGwangDdaeng()
    {
        if (otherCards.Count < 2)
            return false;

        var cardLeft = otherCards[otherCards.Count - 2];
        var cardRight = otherCards[otherCards.Count - 1];

        if (cardLeft.item.type == "광" && cardRight.item.type == "광")
        {
            int monthLeft = cardLeft.item.month;
            int monthRight = cardRight.item.month;

            if ((monthLeft == 3 && monthRight == 8) || (monthLeft == 8 && monthRight == 3))
            {
                Debug.Log("38 광땡입니다! (몬스터)");
                Monster.monsterscores = 28;
                Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                return true;
            }
            else if ((monthLeft == 1 && monthRight == 8) || (monthLeft == 8 && monthRight == 1))
            {
                Debug.Log("18 광땡입니다! (몬스터)");
                Monster.monsterscores = 27;
                Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                return true;
            }
            else if ((monthLeft == 1 && monthRight == 3) || (monthLeft == 3 && monthRight == 1))
            {
                Debug.Log("13 광땡입니다! (몬스터)");
                Monster.monsterscores = 26;
                Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
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

            // 땡 판별
            if (month >= 1 && month <= 10) // 1부터 10까지의 월만 체크
            {
                Debug.Log($"{month} 땡입니다! (몬스터)");
                if (month == 10)
                {
                    Monster.monsterscores = 26;
                    Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                }

                else if (month == 9)
                {
                    Monster.monsterscores = 25;
                    Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                }

                else if (month == 8)
                {
                    Monster.monsterscores = 24;
                    Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                }

                else if (month == 7)
                {
                    Monster.monsterscores = 23;
                    Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                }

                else if (month == 6)
                {
                    Monster.monsterscores = 22;
                    Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                }

                else if (month == 5)
                {
                    Monster.monsterscores = 21;
                    Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                }

                else if (month == 4)
                {
                    Monster.monsterscores = 20;
                    Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                }

                else if (month == 3)
                {
                    Monster.monsterscores = 19;
                    Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                }

                else if (month == 2)
                {
                    Monster.monsterscores = 18;
                    Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                }

                else
                {
                    Monster.monsterscores = 17;
                    Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
                }
                return true; // 땡에 해당
            }
        }
        return false; // 땡에 해당하지 않음
    }

    public bool CheckForMonsterMiddle()
    {
        if (otherCards.Count < 2)
            return false;

        var cardLeft = otherCards[otherCards.Count - 2];
        var cardRight = otherCards[otherCards.Count - 1];

        int monthLeft = cardLeft.item.month;
        int monthRight = cardRight.item.month;

        // 중간 족보 판별
        if ((monthLeft == 1 && monthRight == 2) || (monthLeft == 2 && monthRight == 1))
        {
            Debug.Log("알리입니다! (몬스터)");
            Monster.monsterscores = 16;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }
        else if ((monthLeft == 1 && monthRight == 4) || (monthLeft == 4 && monthRight == 1))
        {
            Debug.Log("독사입니다! (몬스터)");
            Monster.monsterscores = 15;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }
        else if ((monthLeft == 1 && monthRight == 9) || (monthLeft == 9 && monthRight == 1))
        {
            Debug.Log("구삥입니다! (몬스터)");
            Monster.monsterscores = 14;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }
        else if ((monthLeft == 1 && monthRight == 10) || (monthLeft == 10 && monthRight == 1))
        {
            Debug.Log("장삥입니다! (몬스터)");
            Monster.monsterscores = 13;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }
        else if ((monthLeft == 4 && monthRight == 10) || (monthLeft == 10 && monthRight == 4))
        {
            Debug.Log("장사입니다! (몬스터)");
            Monster.monsterscores = 12;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }
        else if ((monthLeft == 4 && monthRight == 6) || (monthLeft == 6 && monthRight == 4))
        {
            Debug.Log("세륙입니다! (몬스터)");
            Monster.monsterscores = 11;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
            return true; // 중간 족보에 해당
        }

        return false; // 중간 족보에 해당하지 않음
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

        // 특수 족보 판별
        if ((monthLeft == 4 && typeLeft == "특수1" && monthRight == 7 && typeRight == "특수") ||
            (monthLeft == 7 && typeLeft == "특수" && monthRight == 4 && typeRight == "특수1"))
        {
            Debug.Log("암행어사입니다! (몬스터)");
            if (Ability.scores == 27) // 점수가 27점이라는 것은 1 3, 3 8 광땡 중 하나라는 것을 의미
            {
                Monster.monsterscores = 28; // 암행어사 승리
                Debug.Log("(몬스터) 암행어사가 승리! 현재 점수: " + Monster.monsterscores);
            }
            else
            {
                Monster.monsterscores = 2; // 다른 조합에 대해 한끗으로 취급
                Debug.Log("(몬스터) 현재 점수: " + Monster.monsterscores);
            }
            return true; // 특수 족보에 해당
        }
        else if ((monthLeft == 3 && typeLeft == "광" && monthRight == 7 && typeRight == "특수") ||
                 (monthLeft == 7 && typeLeft == "특수" && monthRight == 3 && typeRight == "광"))
        {
            Debug.Log("땡잡이입니다! (몬스터)");
            if (Ability.scores >= 17 && Ability.scores <= 25) // 점수가 17 ~ 25점 사이라는 것은 1 ~ 9땡 중 하나라는 것을 의미
            {
                Monster.monsterscores = 26; // 땡잡이 승리
                Debug.Log("(몬스터) 땡잡이 승리! 현재 점수: " + Monster.monsterscores);
            }
            else
            {
                Monster.monsterscores = 1; // 다른 조합에 대해 망통(0끗)으로 취급
                Debug.Log("(몬스터) 현재 점수: " + Monster.monsterscores);
            }
            return true; // 특수 족보에 해당
        }
        else if ((monthLeft == 4 && typeLeft == "특수1" && monthRight == 9 && typeRight == "특수1") ||
                 (monthLeft == 9 && typeLeft == "특수1" && monthRight == 4 && typeRight == "특수1"))
        {
            Debug.Log("멍텅구리구사입니다! (몬스터)");
            // 상대 점수가 1 ~ 25 사이일 때, 재대결
            if (Ability.scores >= 1 && Ability.scores <= 25)
            {
                Debug.Log("멍텅구리구사 승리! 재대결! (몬스터)");
            }
            else
            {
                Debug.Log("멍텅구리구사 패배! (몬스터)");
            }
            return true; // 특수 족보에 해당
        }
        else if ((monthLeft == 4 && typeLeft == "특수" && monthRight == 9 && typeRight == "특수") ||
                 (monthLeft == 9 && typeLeft == "특수" && monthRight == 4 && typeRight == "특수") ||
                 (monthLeft == 4 && typeLeft == "특수1" && monthRight == 9 && typeRight == "특수") ||
                 (monthLeft == 9 && typeLeft == "특수" && monthRight == 4 && typeRight == "특수1") ||
                 (monthLeft == 4 && typeLeft == "특수" && monthRight == 9 && typeRight == "특수1") ||
                 (monthLeft == 9 && typeLeft == "특수1" && monthRight == 4 && typeRight == "특수"))
        {
            Debug.Log("구사입니다! (몬스터)");
            // 상대 점수가 1 ~ 16 사이일 때, 재대결
            if (Ability.scores >= 1 && Ability.scores <= 16)
            {
                Debug.Log("구사 승리! 재대결! (몬스터)");
            }
            else
            {
                Debug.Log("구사 패배! (몬스터)");
            }
            return true; // 특수 족보에 해당
        }

        return false; // 특수 족보에 해당하지 않음
    }


    public void CheckForMonsterKkeut()
    {
        if (otherCards.Count < 2)
            return;

        var cardLeft = otherCards[otherCards.Count - 2];
        var cardRight = otherCards[otherCards.Count - 1];

        int monthLeft = cardLeft.item.month;
        int monthRight = cardRight.item.month;

        // 두 카드의 월 값 합산
        int totalMonth = monthLeft + monthRight;
        int lastDigit = totalMonth % 10; // 일의 자리 추출

        // 끗 족보 판별
        if (lastDigit == 9)
        {
            Debug.Log("갑오(아홉끗)입니다! (몬스터)");
            Monster.monsterscores = 10;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
        }
        else if (lastDigit == 8)
        {
            Debug.Log("여덟끗입니다! (몬스터)");
            Monster.monsterscores = 9;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
        }
        else if (lastDigit == 7)
        {
            Debug.Log("일곱끗입니다! (몬스터)");
            Monster.monsterscores = 8;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
        }
        else if (lastDigit == 6)
        {
            Debug.Log("여섯끗입니다! (몬스터)");
            Monster.monsterscores = 7;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
        }
        else if (lastDigit == 5)
        {
            Debug.Log("다섯끗입니다! (몬스터)");
            Monster.monsterscores = 6;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
        }
        else if (lastDigit == 4)
        {
            Debug.Log("네끗입니다! (몬스터)");
            Monster.monsterscores = 5;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
        }
        else if (lastDigit == 3)
        {
            Debug.Log("세끗입니다! (몬스터)");
            Monster.monsterscores = 4;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
        }
        else if (lastDigit == 2)
        {
            Debug.Log("두끗입니다! (몬스터)");
            Monster.monsterscores = 3;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
        }
        else if (lastDigit == 1)
        {
            Debug.Log("한끗입니다! (몬스터)");
            Monster.monsterscores = 2;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
        }
        else if (lastDigit == 0)
        {
            Debug.Log("망통입니다! (두 패의 월의 합이 0입니다.) (몬스터)");
            Monster.monsterscores = 1;
            Debug.Log($"몬스터 점수: {Monster.monsterscores}"); // 점수 출력
        }
        else
        {
            Debug.Log("알 수 없는 끗입니다.");
        }
    }

    public void Verdict()
    {
        if (Ability.scores > Monster.monsterscores)
        {
            Debug.Log("플레이어의 승리입니다!");
        }

        else if (Ability.scores < Monster.monsterscores)
        {
            Debug.Log("몬스터의 승리입니다!");
        }

        else
        {
            Debug.Log("비겼습니다!");
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
    //    // 카드의 간격 비율을 저장할 배열
    //    float[] objLerps = new float[objCount];
    //    List<PRS> results = new List<PRS>(objCount);

    //    // 카드 수에 따라 간격 비율 설정
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

    //    // 각 카드의 위치, 회전 및 스케일 계산
    //    for (int i = 0; i < objCount; i++)
    //    {
    //        var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
    //        var targetRot = Utils.QI; // 회전은 기본값 사용

    //        // 카드를 일직선으로 배치할 때는 높이 조절이 필요 없으므로 y 좌표는 고정
    //        targetPos.y = height; // 원하는 높이로 설정

    //        results.Add(new PRS(targetPos, targetRot, scale));
    //    }
    //    return results;
    //}

    public bool TryPutCard(bool isMine)
    {
        // 플레이어가 카드를 한 장만 사용할 수 있도록 하는 구문
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

        selectCard = null; // 선택된 카드 초기화
        targetPick = null;
    }

    public void CardDrag()
    {
        // selectCard가 null인지 확인
        if (selectCard == null)
        {
            //Debug.Log("selectCard is null, exiting CardDrag");
            return; // selectCard가 null이면 메서드 종료
        }

        if (!onMyCardArea)
        {
            selectCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, selectCard.originPRS.scale), false);
            //EntityManager.Inst.InsertMyEmptyEntity(Utils.MousePos.x);
        }

        // monster 타겟 찾기
        bool existTarget = false;
        foreach (var hit in Physics2D.RaycastAll(Utils.MousePos, Vector3.forward))
        {
            Monster monster = hit.collider?.GetComponent<Monster>();
            if (monster != null && !monster.isMine && selectCard.attackable)
            {
                targetPick = monster; // 여기에서 targetPick의 타입을 Monster로 유지
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

    // 마우스 커서가 카드에 드래그 되었다가 떨어졌을 때 카드 원래 위치로 돌아가도록 하는 메서드
    void CardPosition(bool isEnlarge, Card card)
    {
        if (isEnlarge)
        {
            // 아무것도 수행되지 않고 카드가 드래그 되도록 함
        }
        else
            card.MoveTransform(card.originPRS, false);

        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }

    #endregion
}

