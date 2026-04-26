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
    
    private SkinSave skinSave => _skinSaveLoader.Load();

    private void OnEnable() {
        skinSave.OnWearUpdated += WearUpdate;
        if (_playerImitate) {
            WearUpdate(skinSave.WearSkins);
        }
    }
    
    private void OnDisable() {
        skinSave.OnWearUpdated -= WearUpdate;
    }


    private void WearUpdate(IReadOnlyList<KeyValuePair<int, SkinData>> allSkins) {
        foreach (KeyValuePair<int, SkinData> skin in allSkins) {
            int tabId = skin.Key;
            int skinId = skin.Value.Id;
            _skinWearer.WearCloth(tabId, skinId);
        }
    }
}