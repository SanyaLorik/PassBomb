using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


public class BotBonusIniter : MonoBehaviour {
    [SerializeField] private BotStateManager _bot;
    [SerializeField] private List<ItemValueBase<IBonus>> _bonusesWeights;
   
    private CancellationTokenSource _tokenSource;
    private IBonus _currentBonus;
    private float _totalWeight;

    [Inject] private MainGameStarter _gameStarter;
    [Inject] private BattleManager _battleManager;
    [Inject] private GameData _gameData;
    [Inject] private Bomb _bomb;

    
    private void Start() {
        CalculateValueDivider();
    }

    
    private void OnEnable() {
        _gameStarter.GameStarted += GameStarted;
        _battleManager.GameReadyToPlay += OnGameReadyToPlay;
        _bomb.PlayerBecameHunter += CheckPlayerHunter;
        _bot.PlayerStatusChanged += BotOnPlayerStatusChanged;
    }

    
    private void OnDisable() {
        _gameStarter.GameStarted -= GameStarted;
        _battleManager.GameReadyToPlay -= OnGameReadyToPlay;
        _bomb.PlayerBecameHunter -= CheckPlayerHunter;
        _bot.PlayerStatusChanged -= BotOnPlayerStatusChanged;
    }
    
    
    private void BotOnPlayerStatusChanged(bool changed) {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        StopPreviousBonus();
    }

    
    private void OnGameReadyToPlay() {
        if(!_bot.IsPlaying) return;
        UniTaskHelper.DisposeTask(ref _tokenSource);
        _tokenSource = new CancellationTokenSource();
        BonusesEnableAsync(_tokenSource.Token).Forget();
    }

    
    private void GameStarted(bool started) {
        if (!started) {
            UniTaskHelper.DisposeTask(ref _tokenSource);
            StopPreviousBonus();
        }
    }
    
    
    private void CheckPlayerHunter(PlayerRoleBehaviour player) {
        if (player == _bot.RoleBehaviour) {
            StopPreviousBonus(true);
        }
    }
    

    private async UniTask BonusesEnableAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            float waitTime = Random.Range(_gameData.BotUseNewBonusTime.From, _gameData.BotUseNewBonusTime.To);
            await UniTask.WaitForSeconds(waitTime, cancellationToken: token);
            if (_bot.RoleBehaviour.CurrentRole != PlayerRoleInGame.Hunter) {
                UseRandomBonus();
            }
        }
    }
    
    
    private void UseRandomBonus() {
        StopPreviousBonus();
        _currentBonus = ItemValueBase.GetRandomItemByWeight(_bonusesWeights, _totalWeight);
        // Debug.Log("Бот юзает " + _currentBonus.GetType());
        _currentBonus.Use(_bot);
    }
    
    
    private void StopPreviousBonus(bool stayHunter = false) {
        if (_currentBonus != null) {
            _currentBonus.StopWork(_bot);
            _currentBonus = null;
        }
        // На всякий
        if(stayHunter) _bot.SetHunterSpeed();
    }
    
    
    private void CalculateValueDivider() {
        _totalWeight = _bonusesWeights.Sum(m => m.Weight);
    }


    private void OnDestroy() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
    }
}
