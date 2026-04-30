using System;
using SanyaBeerExtension;
using UnityEngine;
using Zenject;

public class LineToObjects : MonoBehaviour {
    [SerializeField] private Transform _lineTransform; // - 3.33
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _speed = 1f;
    
    private Transform _target;
    private float _offset;
    
    [Inject] private PlayerMovement _player;

    private void Start() {
        HideArrow();
    }

    private void Update() {
        if (_target != null) {
            // Обновляем позиции линии
            _lineRenderer.SetPosition(0, _player.Transform.position); // от игрока
            _lineRenderer.SetPosition(1, _target.position); // до цели
            // UpdateOffset();
        }
    }


    // Метод для изменения цели
    public void SetTarget(Transform newTarget) {
        Debug.Log("SetTarget " + newTarget);
        _target = newTarget;
        _lineRenderer.enabled = _target != null;
        if (_target != null) {
            _lineTransform.ActiveSelf();
        }
        else {
            HideArrow();
        }
    }

    public void HideArrow() {
        _target = null;
        _lineTransform.DisactiveSelf();
    }
    
}


