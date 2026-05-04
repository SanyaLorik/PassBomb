using System.Collections.Generic;
using Architecture_M;
using MediaKit_M.SkinChanger;
using UnityEngine;
using Zenject;

public class PlayerSkinWearer : MonoBehaviour {
    [SerializeField] private bool _playerImitate;
    
    [SerializeField] private SkinWearer _skinWearer;
    
    
    [Inject] IGameSave gameSave;
    [Inject] private ISkinSaveLoader _skinSaveLoader;
    [Inject] private PlayerFaceChooser _playerFaceChooser;
    
    
    private SkinSave skinSave => _skinSaveLoader.Load();

    
    private void OnEnable() {
        skinSave.OnWearUpdated += WearUpdate;
        _playerFaceChooser.FaceChangeButtonPressed += ChangeFace;
        _playerFaceChooser.GlovesChanged += ChangeGloves;
        if (_playerImitate) {
            // Надеваем после включения скрипта
            WearUpdate(skinSave.WearSkins);
            // перчатки врубаем сука 
            ChangeGloves(_playerFaceChooser.GlovesIndex);
        }
    }
    
    
    private void OnDisable() {
        skinSave.OnWearUpdated -= WearUpdate;
        _playerFaceChooser.FaceChangeButtonPressed -= ChangeFace;
        _playerFaceChooser.GlovesChanged -= ChangeGloves;
    }

    
    private void ChangeGloves(int index) {
        Debug.Log("Change Gloves index " + index);
       _skinWearer.ChooseGlovesByIndex(_playerFaceChooser.GlovesIndex);
    }

    
    private void ChangeFace(int eyes, int mouth) {
        _skinWearer.ChangeFaceByIndexes(eyes, mouth);
    }




    private void WearUpdate(IReadOnlyList<KeyValuePair<int, SkinData>> allSkins) {
        foreach (KeyValuePair<int, SkinData> skin in allSkins) {
            int tabId = skin.Key;
            int skinId = skin.Value.Id;
            _skinWearer.WearCloth(tabId, skinId);
        }
    }
}