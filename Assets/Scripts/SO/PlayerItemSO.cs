using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerItem
{
    public int month;                  // ī�� �� �̸�
    public Sprite sprite;              // ī�� �̹��� 
    public string type;                // ī�� ����
    public int count;                  // ī�� ���� ����
    public double percent;                // ī�� ���� Ȯ��
}

[CreateAssetMenu(fileName = "PlayerItemSo", menuName = "Scriptable Object/PlayerItemSo")]
public class PlayerItemSO : ScriptableObject
{
    public PlayerItem[] playeritems;
}

