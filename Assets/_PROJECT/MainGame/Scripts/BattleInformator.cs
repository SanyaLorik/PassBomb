using System;
using System.Collections.Generic;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using SanyaBeerExtension;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BattleInformator : MonoBehaviour {
    [SerializeField] private GameObject _textFieldContainer;
    [SerializeField] private TextMeshProUGUI _textFieldToInformate;
    [SerializeField] private TextMeshProUGUI _firstPlayerNickname;
    [SerializeField] private TextMeshProUGUI _secondPlayerNickname;
    [SerializeField] private float _timeToShowInfo;

    [Header("Авы ботов!")]
    [SerializeField] private Image _leftAva;
    [SerializeField] private Image _rightAva;
    
    private CancellationTokenSource _tokenSource;
    
    [Inject] List<SkinItemConfig> _skins;
    [Inject] private BattleManager _battleManager;
    [Inject] private MainGameStarter _gameStarter;
    [Inject] private LocalizationData _localization;
    
    
    private void OnEnable() {
        _gameStarter.GameStarted += OnGameStarted;
    }



    private void OnGameStarted(bool started) {
        // Игра началась выводим имена
        if (started) {
           
        }
        // Игра кончилась епта выводим инфу о победителе по кол-ву хп видимо...
        else {
            if (_battleManager.MainPlayerPlay) {
               
            }
            else {
               
            }
        }
    }

}
