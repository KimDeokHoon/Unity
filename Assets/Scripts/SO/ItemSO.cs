using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum HandType { Gwang, Ddaeng, Middle, Kkeut, Special, None } // ���� Ÿ�� ����

//public enum GwangType { SamPal, IlPal, IlSam } // �� ���� ���� ����
//public enum DdaengType { One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten } // �� ���� ���� ����
//public enum MiddleType { Ali, Dogsa, GuPing, JangPing, JangSa, SeRyuk } // �߰� ���� ���� ����
//public enum KkeutType { GapO, Eight, Seven, Six, Five, Four, Three, Two, One, MangTong } // �� ���� ���� ����
//public enum SpecialType { AmhaengEosa, DdaengJabi, MeongTeongGuri, GuSa } // Ư�� ���� ���� ����

[System.Serializable]
public class Item
{
    public int month;                  // ī�� �� �̸�
    public Sprite sprite;              // ī�� �̹��� 
    public string type;
    public int percent;
    //public HandType handType;          // ���� Ÿ��
    //public GwangType? gwangType;       // �� ���� ���� ���� (nullable)
    //public DdaengType? ddaengType;     // �� ���� ���� ���� (nullable)
    //public MiddleType? middleType;     // �߰� ���� ���� ���� (nullable)
    //public KkeutType? kkeutType;       // �� ���� ���� ���� (nullable)
    //public SpecialType? specialType;   // Ư�� ���� ���� ���� (nullable)
}

[CreateAssetMenu(fileName = "ItemSo", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Item[] items;
}
