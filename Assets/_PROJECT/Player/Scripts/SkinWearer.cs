using System;
using System.Linq;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
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
    
    [Inject] private GameData _gameData;
    private CancellationTokenSource _tokenSource;

    
    private void OnEnable() {
        ChangeFaceRandom();
        UniTaskHelper.DisposeTask(ref  _tokenSource);
        _tokenSource = new CancellationTokenSource();
        UpdateFaceAsync(_tokenSource.Token).Forget();
    }

    private void OnDisable() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
    }

    
    private async UniTask UpdateFaceAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            float waitTime = Random.Range(_gameData.TimeToChangeFace.From, _gameData.TimeToChangeFace.To);
            await UniTask.WaitForSeconds(waitTime, cancellationToken: token);
            ChangeFaceRandom(); 
        }
    }

    
    private void ChangeFaceRandom() {
        Mouthes.DisactiveSelf();
        Eyes.DisactiveSelf();
        Mouthes.GetRandomElement().ActiveSelf();
        Eyes.GetRandomElement().ActiveSelf();
    }

    public void WearCloth(int tabId, int clothId) {
        Debug.Log($"Надевание tabId {tabId} одежду {clothId} ");
        // Снимаем части тела и выбираем нужную
        Clothes clothes = GetClothesByTabId(tabId);
        DisactivePreviousCloth(clothes);
        DisactiveBodyPartByTabId(tabId);
        ActivateClothById(clothes, clothId);
    }


    public void  WearRandomSkinForBot() {
        foreach (var clothByTab in Clothes) {
            WearCloth(
                clothByTab.TabId, 
                clothByTab.ClothesInfo[Random.Range(0, clothByTab.ClothesInfo.Length)].ClothId
            );
        }

        
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
            Debug.Log($"для clothId {clothId} не выбран скин");
            return;
        }
        
        cloth.ClothObject.ActiveSelf();
        Debug.Log("Надеваем " + cloth.ClothObject);
        if (cloth.BodyPartObject != null) {
            cloth.BodyPartObject.ActiveSelf();
        }
    }

    
    private Clothes GetClothesByTabId(int tabId) 
        => Clothes.First(c => c.TabId == tabId);

    

    
}