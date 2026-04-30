using System;
using System.Linq;
using SanyaBeerExtension;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

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
    [SerializeField] private GameObject[] Mouthes;
    [SerializeField] private GameObject[] Eyes;
    [SerializeField] private GameObject[] Gloves;
    
    [Inject] private GameData _gameData;

    
    public void  WearRandomSkin() {
        foreach (var clothByTab in Clothes) {
            WearCloth(
                clothByTab.TabId, 
                clothByTab.ClothesInfo[Random.Range(0, clothByTab.ClothesInfo.Length)].ClothId
            );
        }
    }
    
        
    public void ChangeFaceGlovesRandom() {
        Eyes.DisactiveSelf();
        Mouthes.DisactiveSelf();
        Gloves.DisactiveSelf();
        Eyes.GetRandomElement().ActiveSelf();
        Mouthes.GetRandomElement().ActiveSelf();
        Gloves.GetRandomElement().ActiveSelf();
    }

    /// <summary>
    /// Face - т.е порядок сверху вниз глаза лицо
    /// </summary>
    /// <param name="mouthId"></param>
    /// <param name="eyeId"></param>
    public void ChangeFaceByIndexes(int eyeId, int mouthId) {
        Eyes.DisactiveSelf();
        Mouthes.DisactiveSelf();
        Eyes[eyeId].ActiveSelf();
        Mouthes[mouthId].ActiveSelf();
    }

    
    public (int, int) GetNextFaceIndexes(int prevEyesIndex, int prevMouthIndex) {
        int eyesIndex = GetNextCollectionIndex(Eyes.Length, prevEyesIndex);
        int mouthIndex = GetNextCollectionIndex(Mouthes.Length, prevMouthIndex);
        return (eyesIndex, mouthIndex);
    }

    
    
    public void ChooseGlovesByIndex(int glovesId) {
        Gloves.DisactiveSelf();
        Gloves[glovesId].ActiveSelf();
    }

    
    public int GetNextGlovesIndex(int previousIndex) {
        int newRandomGlovesIndex = GetNextCollectionIndex(Gloves.Length, previousIndex);
        return newRandomGlovesIndex;
    }

    
    public void WearCloth(int tabId, int clothId) {
        // Debug.Log($"Надевание tabId {tabId} одежду {clothId} ");
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
        ClothInfo cloth = clothes.ClothesInfo.FirstOrDefault(c => c.ClothId  == clothId);
        if (cloth.ClothObject == null) {
            Debug.Log($"Для clothId {clothId} не выбран скин");
            return;
        }
        
        cloth.ClothObject.ActiveSelf();
        // Debug.Log("Надеваем " + cloth.ClothObject);
        if (cloth.BodyPartObject != null) {
            cloth.BodyPartObject.ActiveSelf();
        }
    }

    
    private Clothes GetClothesByTabId(int tabId) 
        => Clothes.First(c => c.TabId == tabId);

    
    private int GetNextCollectionIndex(int collectionSize, int prevIndex) {
        int newIndex = Random.Range(0, collectionSize);
        if (newIndex == prevIndex) {
            newIndex++;
            if (newIndex > collectionSize - 1) {
                newIndex = 0;
            }
        }
        return newIndex;
    }
    
}