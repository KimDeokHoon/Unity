using System;
using System.Collections;
using UnityEngine;
using DG.Tweening; // DOTween을 사용하기 위해 추가
using Random = UnityEngine.Random;

public class CoinFlip : MonoBehaviour
{
    public float throwForce = 5f; // 동전 던지는 힘
    private Rigidbody2D rb;
    private bool isFlipping;

    public Sprite frontSprite; // 앞면 스프라이트
    public Sprite backSprite;  // 뒷면 스프라이트
    private SpriteRenderer spriteRenderer;

    private bool isFront = true; // 초기 상태는 앞면

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = frontSprite; // 초기 이미지 설정
    }

    public void FlipCoin(System.Action<bool> onComplete)
    {
        if (isFlipping)
            return; // 이미 회전 중이면 리턴

        isFlipping = true; // 회전 중 상태

        int flipCount = Random.Range(10, 21); // 10에서 20 사이의 회전 횟수
        float totalRotation = flipCount * 180f; // 총 회전 각도 계산
        float duration = 0.5f * flipCount; // 총 회전 시간

        // 회전 애니메이션
        transform.DORotate(new Vector3(0, totalRotation, 0), duration, RotateMode.WorldAxisAdd)
            .OnUpdate(() =>
            {
                // 회전 중에 스프라이트 변경
                spriteRenderer.sprite = isFront ? backSprite : frontSprite;
                isFront = !isFront; // 상태 반전
            })
            .OnComplete(() =>
            {
                // 최종 결과 결정
                bool isFrontSide = (flipCount % 2 == 0); // flipCount가 짝수면 true, 홀수면 false

                //Debug.Log(isFrontSide ? "동전의 앞면입니다." : "동전의 뒷면입니다.");

                onComplete?.Invoke(isFrontSide); // 결과를 GameManager에 전달

                isFlipping = false; // 회전 종료 상태
            });
    }


}



