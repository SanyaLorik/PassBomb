using SanyaBeerExtension;
using TMPro;
using UnityEngine;
using Zenject;

public class BattleCanvasVisualiser : MonoBehaviour {
    [Header("Данные по канвасу")]
    [SerializeField] private GameObject _youHaveBomb;
    [SerializeField] private GameObject _bombPointer;
    [SerializeField] private TextMeshProUGUI _countPlayersText;

    
    [Inject] private Bomb _bomb;
    [Inject] private MainGameStarter _gameStarter;
    [Inject] private BattleManager _battleManager;
    [Inject] IPassBombPlayer _mainPlayer;


    
    private void OnEnable() {
        _bomb.PlayerBecameHunter += BombOnPlayerBecameHunter;
        _gameStarter.GameStarted += OnGameStarted;
        _battleManager.PlayersCountChanged += OnChangePlayersCount;
    }

    private void OnChangePlayersCount(int count) {
        _countPlayersText.text = $"{count}/{_battleManager.CountPlayersToNewBattle}";
    }

    
    
    private void OnGameStarted(bool started) {
        if (started) {
            Debug.Log("OnGameStarted " + started);
            _youHaveBomb.DisactiveSelf();
            _bombPointer.DisactiveSelf();
        }
    }


    private void BombOnPlayerBecameHunter(PlayerRoleBehaviour player) {
        if (!_battleManager.MainPlayerPlay) return;
        if (player == _mainPlayer.RoleBehaviour) {
            _youHaveBomb.ActiveSelf();
            _bombPointer.DisactiveSelf();
        }
        else {
            _youHaveBomb.DisactiveSelf();
            _bombPointer.ActiveSelf();
        }
    }
}