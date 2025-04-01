using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum HandType { Gwang, Ddaeng, Middle, Kkeut, Special, None } // 족보 타입 정의

//public enum GwangType { SamPal, IlPal, IlSam } // 광 족보 세부 종류
//public enum DdaengType { One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten } // 땡 족보 세부 종류
//public enum MiddleType { Ali, Dogsa, GuPing, JangPing, JangSa, SeRyuk } // 중간 족보 세부 종류
//public enum KkeutType { GapO, Eight, Seven, Six, Five, Four, Three, Two, One, MangTong } // 끗 족보 세부 종류
//public enum SpecialType { AmhaengEosa, DdaengJabi, MeongTeongGuri, GuSa } // 특수 족보 세부 종류

[System.Serializable]
public class Item
{
    public int month;                  // 카드 월 이름
    public Sprite sprite;              // 카드 이미지 
    public string type;
    public int percent;
    //public HandType handType;          // 족보 타입
    //public GwangType? gwangType;       // 광 족보 세부 종류 (nullable)
    //public DdaengType? ddaengType;     // 땡 족보 세부 종류 (nullable)
    //public MiddleType? middleType;     // 중간 족보 세부 종류 (nullable)
    //public KkeutType? kkeutType;       // 끗 족보 세부 종류 (nullable)
    //public SpecialType? specialType;   // 특수 족보 세부 종류 (nullable)
}

[CreateAssetMenu(fileName = "ItemSo", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Item[] items;
}
