using SanyaBeerExtension;
using UnityEngine;


public class PlayerTrailManager : MonoBehaviour {
    [SerializeField] private GameObject[] _defaultTrails;
    [SerializeField] private GameObject[] _speedTrails;
    [SerializeField] private GameObject[] _jumpTrails;
    [SerializeField] private GameObject[] _invincibleTrails;
    [SerializeField] private BotStateManager _botStateManager;
    [SerializeField] private PlayerMovement _playerMovement;
    
    private IPassBombPlayer _passBombPlayer;
    
    private MoveStatus _currentTrailType = MoveStatus.Default;

    private void Start() {
        OffAll();
        _defaultTrails.ActiveSelf();
    }

    
    private void OnEnable() {
        TryInit();
        _passBombPlayer.MoveStatusChanged += OnMoveStatusChanged;
    }

    
    private void OnDisable() {
        if (_passBombPlayer != null)
            _passBombPlayer.MoveStatusChanged -= OnMoveStatusChanged;
    }

    
    private void TryInit() {
        if (_passBombPlayer == null) {
            if (_botStateManager != null) {
                _passBombPlayer = _botStateManager;
            }
            else {
                _passBombPlayer = _playerMovement;
            }
        }
    }

    private void OnMoveStatusChanged(MoveStatus status, bool enable) {
        // Eсли чето врубаем обязательно все предыдущие вырубаем
        if (enable) {
            OffAll();
            _currentTrailType = status;
            if (status == MoveStatus.SuperSpeed) {
                _speedTrails.ActiveSelf();
            }
            else if(status == MoveStatus.SuperJump) {
                _jumpTrails.ActiveSelf();
            }
            else if (status == MoveStatus.Invincible) {
                _invincibleTrails.ActiveSelf();
            }
        }
        // Если вырубился ласт бонус, включаем дефолт
        else {
            if (status == _currentTrailType) {
                _currentTrailType = MoveStatus.Default;
                OffAll();
                _defaultTrails.ActiveSelf();
            }
        }
        
    }

    
    private void OffAll() {
        _speedTrails.DisactiveSelf();
        _jumpTrails.DisactiveSelf();
        _invincibleTrails.DisactiveSelf();
        _defaultTrails.DisactiveSelf();
    }
    
}
