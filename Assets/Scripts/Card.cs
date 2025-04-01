using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Card : MonoBehaviour
{
    public static Card Inst { get; private set; }
    void Awake() => Inst = this;

    public SpriteRenderer spriteRenderer; // SpriteRenderer 컴포넌트
    public Sprite cardFront; // 카드 앞면 이미지
    public Sprite cardBack; // 카드 뒷면 이미지
    public Item item;
    public PlayerItem playeritem; // PlayerItem 타입
    bool isFront;
    public PRS originPRS;
    public bool attackable;

    public void Setup(bool isFront)
    {
        //this.isFront = isFront;

        //if (cardManager == null)
        //{
        //    Debug.LogError("CardManager is not initialized!");
        //    return; // 초기화되지 않았다면 함수 종료
        //}


        if (isFront == true)
        {
            // PlayerPopItem() 호출을 통해 카드 뽑기
            this.playeritem = CardManager.Inst.PlayerPopItem(); // PlayerItem을 뽑음

            // 카드 앞면 이미지 및 속성 설정
            spriteRenderer.sprite = this.playeritem.sprite; // PlayerItemSO에서 가져온 카드 이미지
            int monthValue = this.playeritem.month;
            string typeText = this.playeritem.type;
            double percentValue = this.playeritem.percent;

            // 필요하다면 여기서 monthValue, typeText, percentValue를 저장하거나 추가 작업을 할 수 있습니다.
        }
        else
        {
            // 몬스터 카드 뽑기 로직
            this.item = CardManager.Inst.PopItem(); // CardManager의 PopItem() 호출

            // 카드 뒷면 이미지 및 속성 설정
            spriteRenderer.sprite = this.item.sprite; // ItemSO에서 가져온 카드 이미지
            int monthValue = this.item.month;
            string typeText = this.item.type;

            // 필요하다면 여기서 monthValue, typeText를 저장하거나 추가 작업을 할 수 있습니다.
        }
    }

    public void FlipCard()
    {
        // 카드가 뒷면일 때만 앞면으로 뒤집기
        if (!isFront)
        {
            isFront = true; // 카드 상태를 앞면으로 변경
                            // 180도 회전 애니메이션
            transform.DORotate(new Vector3(0, -180, 0), 0.5f) // 0.5초 동안 왼쪽으로 회전
                .OnComplete(() =>
                {
                    // 앞면 이미지 설정
                    spriteRenderer.sprite = this.item.sprite; 
                    // 다시 0도로 회전하여 원래 위치로 돌아오게 함
                    transform.DORotate(Vector3.zero, 0.5f);
                });
        }
        // 카드가 이미 앞면인 경우는 아무 변화도 없음
    }

    void OnMouseOver()
    {
        if (isFront)
            CardManager.Inst.CardMouseOver(this);
    }

    void OnMouseExit()
    {
        if (isFront)
            CardManager.Inst.CardMouseExit(this);
    }

    void OnMouseDown()
    {
        if (isFront)
            CardManager.Inst.CardMouseDown(this);
    }

    void OnMouseUp()
    {
        if (isFront)
        {
            CardManager.Inst.CardMouseUp();
        }
    }

    void OnMouseDrag()
    {
        if (isFront)
            CardManager.Inst.CardDrag();
    }

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
        {
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }
}
