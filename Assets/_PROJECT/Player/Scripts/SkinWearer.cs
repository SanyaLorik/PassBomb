using System;
using UnityEngine;

[Serializable]
public struct ClothInfo {
    public string ClothId;
    public GameObject ClothObject;
    public GameObject BodyPartObject;
    // Если не нужна боди парт снимаем все их с TabId
}


[Serializable]
public struct BodyPart {
    public int TabId;
    public GameObject[] BodyPartObjects;
}


[Serializable]
public struct Clothes {
    public int TabId;
    public ClothInfo[] ClothesInfo;
}


public class SkinWearer : MonoBehaviour {
    [SerializeField] private Clothes[] Clothes;
    [SerializeField] private BodyPart[] BodyParts;

}