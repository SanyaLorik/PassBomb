using UnityEngine;
using Zenject;

public class ShieldEnabler : MonoBehaviour {
    [SerializeField] private ShieldVisual _shieldVisual;
    [SerializeField] private PlayerRoleBehaviour _playerRoleBehaviour;
    [SerializeField] private bool _shieldIsEnabled;

    
    [Inject] BattleManager _battleManager;
    
    private void Start() {
        _shieldVisual.ShieldShowFast(false);
    }

    private void OnEnable() {
        _battleManager.GameReadyToPlay += HideShield;
        _shieldVisual.ShieldShowFast(false);
        _playerRoleBehaviour.InvinsibleStatusChanged += OnInvinsibleStatusChanged;
    }


    private void OnDisable() {
        _battleManager.GameReadyToPlay -= HideShield;
        _shieldVisual.ShieldShowFast(false);
        _playerRoleBehaviour.InvinsibleStatusChanged -= OnInvinsibleStatusChanged;
    }
    
    
    private void HideShield() {
        _shieldVisual.HideShieldFast();
    }
    
    
    private void OnInvinsibleStatusChanged(bool enable) {
        if (!_playerRoleBehaviour.PassBombPlayer.IsPlaying) {
            _shieldIsEnabled = false;
            _shieldVisual.ShieldEnableAnimate(false);
            return;
        }
        
        // Повторно не анимируем
        if (enable == _shieldIsEnabled) {
            return;
        }
        
        // Если хочет вырубить но 1 включен не трогаем
        if (!enable) {
            if (_playerRoleBehaviour.IsInvincibleAfterBomb || _playerRoleBehaviour.IsInvincibleAfterBonus) {
                return;
            }
        }
        
        // Debug.Log("Shield enable: " + enable);
        _shieldIsEnabled =  enable;
        _shieldVisual.ShieldEnableAnimate(enable);
    }
}