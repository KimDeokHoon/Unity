using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Card : MonoBehaviour
{
    public static Card Inst { get; private set; }
    void Awake() => Inst = this;

    public SpriteRenderer spriteRenderer; // SpriteRenderer ������Ʈ
    public Sprite cardFront; // ī�� �ո� �̹���
    public Sprite cardBack; // ī�� �޸� �̹���
    public Item item;
    public PlayerItem playeritem; // PlayerItem Ÿ��
    bool isFront;
    public PRS originPRS;
    public bool attackable;

    public void Setup(bool isFront)
    {
        //this.isFront = isFront;

        //if (cardManager == null)
        //{
        //    Debug.LogError("CardManager is not initialized!");
        //    return; // �ʱ�ȭ���� �ʾҴٸ� �Լ� ����
        //}


        if (isFront == true)
        {
            // PlayerPopItem() ȣ���� ���� ī�� �̱�
            this.playeritem = CardManager.Inst.PlayerPopItem(); // PlayerItem�� ����

            // ī�� �ո� �̹��� �� �Ӽ� ����
            spriteRenderer.sprite = this.playeritem.sprite; // PlayerItemSO���� ������ ī�� �̹���
            int monthValue = this.playeritem.month;
            string typeText = this.playeritem.type;
            double percentValue = this.playeritem.percent;

            // �ʿ��ϴٸ� ���⼭ monthValue, typeText, percentValue�� �����ϰų� �߰� �۾��� �� �� �ֽ��ϴ�.
        }
        else
        {
            // ���� ī�� �̱� ����
            this.item = CardManager.Inst.PopItem(); // CardManager�� PopItem() ȣ��

            // ī�� �޸� �̹��� �� �Ӽ� ����
            spriteRenderer.sprite = this.item.sprite; // ItemSO���� ������ ī�� �̹���
            int monthValue = this.item.month;
            string typeText = this.item.type;

            // �ʿ��ϴٸ� ���⼭ monthValue, typeText�� �����ϰų� �߰� �۾��� �� �� �ֽ��ϴ�.
        }
    }

    public void FlipCard()
    {
        // ī�尡 �޸��� ���� �ո����� ������
        if (!isFront)
        {
            isFront = true; // ī�� ���¸� �ո����� ����
                            // 180�� ȸ�� �ִϸ��̼�
            transform.DORotate(new Vector3(0, -180, 0), 0.5f) // 0.5�� ���� �������� ȸ��
                .OnComplete(() =>
                {
                    // �ո� �̹��� ����
                    spriteRenderer.sprite = this.item.sprite; 
                    // �ٽ� 0���� ȸ���Ͽ� ���� ��ġ�� ���ƿ��� ��
                    transform.DORotate(Vector3.zero, 0.5f);
                });
        }
        // ī�尡 �̹� �ո��� ���� �ƹ� ��ȭ�� ����
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
