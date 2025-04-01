using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 치트, UI, 랭킹, 게임오버
public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] CoinFlip coinFlip; // CoinFlip 컴포넌트 참조 추가
    [SerializeField] GameObject turnOptionUI; // 동전 던지기 UI
    [SerializeField] GameObject functionUI; // 배팅 버튼 관련 UI
    [SerializeField] public TMP_Text BattingCoin;    // 플레이어, 몬스터 참가비 걷어 합산
    

    public int BattingCharge;
    WaitForSeconds delay03 = new WaitForSeconds(0.3f);

    void Start()
    {
        // 배팅 관련 버튼 비활성화 (게임을 처음 시작할 때 누구의 턴인지 모르기 때문)
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
            CardManager.Inst.FlipAllCards(); // FlipAllCards 메서드 호출
    }

    public void StartGame()
    {
        CollectEntryFees(); // 참가비 수집 메서드 호출
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }

    public void CollectEntryFees()
    {
        // 플레이어와 몬스터 인스턴스 가져오기
        Ability playerAbility = FindObjectOfType<Ability>();
        Monster monster = FindObjectOfType<Monster>();

        if (playerAbility != null && monster != null)
        {
            BattingCharge += 2000; // 참가비 합산
            playerAbility.Coin -= 1000; // 플레이어의 잔여 코인
            monster.MonsterCoins -= 1000; // 몬스터의 잔여 코인

            // UI 업데이트
            BattingCoin.text = BattingCharge.ToString();
            playerAbility.UpdatePlayerCoinText(); // 플레이어 코인 UI 업데이트
            monster.UpdateMonsterCoinText(); // 몬스터 코인 UI 업데이트
        }
    }

    public void Notification(string message)
    {
        notificationPanel.Show(message);
    }

    public void StartCoinFlip()
    {
        // 동전 던지기 UI 활성화
        turnOptionUI.SetActive(true);
        coinFlip.FlipCoin(OnCoinFlipComplete); // 동전 던지기 메서드 호출
    }

    private void OnCoinFlipComplete(bool isFront)
    {
        // 동전 던지기 결과에 따라 처리
        if (isFront)
        {
            Notification("플레이어의 턴");
            StartCoroutine(delaytime());
        }
        else
        {
            Notification("몬스터의 턴");
        }

        // 동전 던지기 UI 비활성화
        turnOptionUI.SetActive(false);
    }

    IEnumerator delaytime()
    {
        yield return new WaitForSeconds(1.5f);
        functionUI.SetActive(true);
    }
}


