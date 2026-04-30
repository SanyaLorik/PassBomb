using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using SanyaBeerExtension;
using TMPro;
using UnityEngine;
using Zenject;

public class BattleDiesInformator : MonoBehaviour {
    [SerializeField] private GameObject _textFieldContainer;
    [SerializeField] private TextMeshProUGUI _textFieldToInformate;
    
    private CancellationTokenSource _tokenSource;
    
    [Inject] private BattleManager _battleManager;
    [Inject] private MainGameStarter _gameStarter;
    [Inject] private LocalizationData _localization;
    [Inject] private GameData _gameData;

    private void OnEnable() {
        _battleManager.PlayerDied += BattleManagerOnPlayerDied;
    }

    private void BattleManagerOnPlayerDied(string nickName, Vector3 _) {
        if(!_battleManager.PlayerReturnToSpawn) return; 
        _textFieldContainer.ActiveSelf();
        _textFieldToInformate.text = string.Format(
            _localization.PlayerExploded,
            nickName
        );
        UniTaskHelper.DisposeTask(ref _tokenSource);
        _tokenSource = new CancellationTokenSource();
        WaitToHideInfoAsync(_tokenSource.Token).Forget();
    }
    
    private async UniTask WaitToHideInfoAsync(CancellationToken token) {
        await UniTask.WaitForSeconds(_gameData.TimeToShowDieInfo, cancellationToken: token);
        _textFieldContainer.DisactiveSelf();
    }
}