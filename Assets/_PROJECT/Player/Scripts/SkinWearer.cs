using System;
using System.Linq;
using SanyaBeerExtension;
using UnityEngine;

[Serializable]
public struct ClothInfo {
    public int ClothId;
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
    
    
    public void WearCloth(int tabId, int clothId) {
        Debug.Log($"Надевание tabId {tabId} одежду {clothId} ");
        // Снимаем части тела и выбираем нужную
        Clothes clothes = GetClothesByTabId(tabId);
        DisactivePreviousCloth(clothes);
        DisactiveBodyPartByTabId(tabId);
        ActivateClothById(clothes, clothId);
    }
    
    private void DisactiveBodyPartByTabId(int tabId) {
        BodyPart bodyPart = BodyParts.FirstOrDefault(b => b.TabId == tabId);
        if(bodyPart.BodyPartObjects == null) return;
        
        foreach (var bp in bodyPart.BodyPartObjects) {
            if (bp != null) {
                bp.DisactiveSelf();
            }
        }
    }

    private void DisactivePreviousCloth(Clothes clothes) {
        foreach (var cloth in clothes.ClothesInfo) {
            if (cloth.ClothObject.activeSelf) {
                cloth.ClothObject.DisactiveSelf();
            }
        }   
    }


    private void ActivateClothById(Clothes clothes, int clothId) {
        ClothInfo cloth = clothes.ClothesInfo.First(c => c.ClothId  == clothId);
        cloth.ClothObject.ActiveSelf();
        Debug.Log("Надеваем " + cloth.ClothObject);
        if (cloth.BodyPartObject != null) {
            cloth.BodyPartObject.ActiveSelf();
        }
    }

    
    private Clothes GetClothesByTabId(int tabId) 
        => Clothes.First(c => c.TabId == tabId);

    

    
}