using System;
using SanyaBeerExtension;
using UnityEngine;

public class SkinShadow : MonoBehaviour {
    [SerializeField] private GameObject _shadow;

    private void OnEnable() {
        EnableShadow();
    }

    public void DisableShadow() {
        if (_shadow.activeSelf) {
            _shadow.DisactiveSelf();
        }
    }
    
    public void EnableShadow() {
        if (!_shadow.activeSelf) {
            _shadow.ActiveSelf();
        }

    }
    
}