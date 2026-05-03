using UnityEngine;

public class ShieldEnabler : MonoBehaviour {
    [SerializeField] private ShieldVisual _shieldVisual;
    [SerializeField] private PlayerRoleBehaviour _playerRoleBehaviour;
    [SerializeField] private bool _shieldIsEnabled;

    private void Start() {
        _shieldVisual.ShieldShowFast(false);
    }

    private void OnEnable() {
        _shieldVisual.ShieldShowFast(false);
        _playerRoleBehaviour.InvinsibleStatusChanged += OnInvinsibleStatusChanged;
    }

    
    private void OnDisable() {
        _playerRoleBehaviour.InvinsibleStatusChanged -= OnInvinsibleStatusChanged;
    }
    
    
    private void OnInvinsibleStatusChanged(bool enable) {
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
        
        Debug.Log("Shield enable: " + enable);
        _shieldIsEnabled =  enable;
        _shieldVisual.ShieldEnableAnimate(enable);
    }
}