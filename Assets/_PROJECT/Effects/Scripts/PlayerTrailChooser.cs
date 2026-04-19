using SanyaBeerExtension;
using UnityEngine;

public class PlayerTrailChooser : MonoBehaviour {
    [SerializeField] private GameObject[] _defaultTrails;
    [SerializeField] private GameObject[] _speedTrails;
    [SerializeField] private GameObject[] _jumpTrails;
    [SerializeField] private GameObject[] _invincibleTrails;

    private GameObject[] _previousTrails;

    
    public void SetDefaultTrails() {
        _previousTrails.DisactiveSelf();
        _defaultTrails.ActiveSelf();
        _previousTrails = _defaultTrails;
    }
    
    public void SetSpeedTrails() {
        _previousTrails.DisactiveSelf();
        _speedTrails.ActiveSelf();
        _previousTrails = _speedTrails;
    }

    public void SetJumpTrails() {
        _previousTrails.DisactiveSelf();
        _jumpTrails.ActiveSelf();
        _previousTrails = _jumpTrails;
    }
    
    public void SetInvincibleTrails() {
        _previousTrails.DisactiveSelf();
        _invincibleTrails.ActiveSelf();
        _previousTrails = _invincibleTrails;
    }
}
