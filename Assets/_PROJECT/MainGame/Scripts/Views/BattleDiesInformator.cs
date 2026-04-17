using System.Threading;
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

    private void OnEnable() {
        _battleManager.PlayedDied += BattleManagerOnPlayedDied;
    }

    private void BattleManagerOnPlayedDied(string nickName) {
        if(_battleManager.MainPlayerPlay) return; 
        _textFieldContainer.ActiveSelf();
        _textFieldToInformate.text = string.Format(
            _localization.PlayerExploded,
            nickName
        );
    }
}