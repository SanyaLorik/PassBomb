using System;
using UnityEngine;
using Zenject;

public class BotSkinChooser : MonoBehaviour {
    [SerializeField] private SkinWearer _skinWearer;

    [Inject] MainGameStarter _gameStarter; 
    [Inject] BattleManager _battleManager;


    private void Start() {
        OnGameStarted(true);
    }

    private void OnDisable() {
        _gameStarter.GameStarted -= OnGameStarted;
    }

    private void OnEnable() {
        _gameStarter.GameStarted += OnGameStarted;
    }

    private void OnGameStarted(bool started) {
        if (started) {
            _skinWearer.WearRandomSkin();
            _skinWearer.ChangeFaceGlovesRandom();
        }
    }


}