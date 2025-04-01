using System;
using System.Collections;
using UnityEngine;
using DG.Tweening; // DOTween�� ����ϱ� ���� �߰�
using Random = UnityEngine.Random;

public class CoinFlip : MonoBehaviour
{
    public float throwForce = 5f; // ���� ������ ��
    private Rigidbody2D rb;
    private bool isFlipping;

    public Sprite frontSprite; // �ո� ��������Ʈ
    public Sprite backSprite;  // �޸� ��������Ʈ
    private SpriteRenderer spriteRenderer;

    private bool isFront = true; // �ʱ� ���´� �ո�

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = frontSprite; // �ʱ� �̹��� ����
    }

    public void FlipCoin(System.Action<bool> onComplete)
    {
        if (isFlipping)
            return; // �̹� ȸ�� ���̸� ����

        isFlipping = true; // ȸ�� �� ����

        int flipCount = Random.Range(10, 21); // 10���� 20 ������ ȸ�� Ƚ��
        float totalRotation = flipCount * 180f; // �� ȸ�� ���� ���
        float duration = 0.5f * flipCount; // �� ȸ�� �ð�

        // ȸ�� �ִϸ��̼�
        transform.DORotate(new Vector3(0, totalRotation, 0), duration, RotateMode.WorldAxisAdd)
            .OnUpdate(() =>
            {
                // ȸ�� �߿� ��������Ʈ ����
                spriteRenderer.sprite = isFront ? backSprite : frontSprite;
                isFront = !isFront; // ���� ����
            })
            .OnComplete(() =>
            {
                // ���� ��� ����
                bool isFrontSide = (flipCount % 2 == 0); // flipCount�� ¦���� true, Ȧ���� false

                //Debug.Log(isFrontSide ? "������ �ո��Դϴ�." : "������ �޸��Դϴ�.");

                onComplete?.Invoke(isFrontSide); // ����� GameManager�� ����

                isFlipping = false; // ȸ�� ���� ����
            });
    }


}



