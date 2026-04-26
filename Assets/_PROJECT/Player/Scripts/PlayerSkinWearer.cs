using System.Collections.Generic;
using Architecture_M;
using MediaKit_M.SkinChanger;
using UnityEngine;
using Zenject;

public class PlayerSkinWearer : MonoBehaviour {
    [Inject] IGameSave gameSave;
    [Inject] private ISkinSaveLoader _skinSaveLoader;
    
    private SkinSave skinSave => _skinSaveLoader.Load();

    private void OnEnable() {
        skinSave.OnWearUpdated += WearUpdate;
    }

    private void Start() {
        WearUpdate(skinSave.WearSkins);
        
    }

    private void WearUpdate(IReadOnlyList<KeyValuePair<int, SkinData>> allSkins) {
        // Tab и Скин, мне нужно снимать текущий скин и надевать новый,
        // можно пробежаться по всем скинам из таба
        foreach (KeyValuePair<int, SkinData> skin in allSkins) {
            
        }
    }
}