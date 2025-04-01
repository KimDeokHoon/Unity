using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerItem
{
    public int month;                  // 카드 월 이름
    public Sprite sprite;              // 카드 이미지 
    public string type;                // 카드 유형
    public int count;                  // 카드 생성 개수
    public double percent;                // 카드 뽑힐 확률
}

[CreateAssetMenu(fileName = "PlayerItemSo", menuName = "Scriptable Object/PlayerItemSo")]
public class PlayerItemSO : ScriptableObject
{
    public PlayerItem[] playeritems;
}

