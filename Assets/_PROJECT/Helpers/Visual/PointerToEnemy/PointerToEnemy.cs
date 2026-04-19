using System;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using SanyaBeerExtension;
using UnityEngine;
using Zenject;

public class PointerToEnemy : MonoBehaviour  {
    [SerializeField] private GameObject _ememyPointer;
    [SerializeField] private RectTransform _containerRect;


    private Camera _camera;
    private CancellationTokenSource _tokenSource;
    private Transform _playerTransform;
    
    
    [Inject] PlayerMovement _playerMovement;
    [Inject] BattleManager _battleManager;
    [Inject] MainGameStarter _mainGameStarter;
    [Inject] Bomb _bomb;
    
    
    private void Awake() {
        _camera = Camera.main;
        _playerTransform = _playerMovement.Transform;
    }


    private void OnEnable() {
        _battleManager.NewRoundStarted += BattleManagerOnNewRoundStarted;
        _bomb.BombExploded += DisposeTask;
    }

    
    private void DisposeTask() {
        if(!_battleManager.MainPlayerPlay) return; 
        
        UniTaskHelper.DisposeTask(ref _tokenSource);
        Debug.Log("MonitorMovementAsync остановлен");
        _containerRect.DisactiveSelf();
    }

    private void BattleManagerOnNewRoundStarted(int _) {
        if(!_battleManager.MainPlayerPlay) return; 
        
        _containerRect.ActiveSelf();
        UniTaskHelper.DisposeTask(ref _tokenSource);
        _tokenSource = new CancellationTokenSource();
        
        Debug.Log("MonitorMovementAsync запущен");
        MonitorMovementAsync(_tokenSource.Token).Forget();
    }

    
    private async UniTask MonitorMovementAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            Vector3 direction = transform.position - _playerTransform.transform.position;
            Ray ray = new Ray(_playerTransform.position, direction);
        
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_camera);
        
            float minDistance = float.MaxValue;
        
            for (int i = 0; i < planes.Length; i++) {
                if (planes[i].Raycast(ray, out float distance)) {
                    if (minDistance > distance) {
                        minDistance = distance;
                    }
                }
            }
            minDistance = Mathf.Clamp(minDistance, 0, direction.magnitude);
            if (direction.magnitude > minDistance) {
                _ememyPointer.ActiveSelf();
        
                Vector3 worldPos = ray.GetPoint(minDistance);
                Vector2 screenPos = _camera.WorldToScreenPoint(worldPos);
        
                // Ограничиваем внутри контейнера
                Vector2 clampedPos = ClampToContainer(screenPos);
                _ememyPointer.transform.position = clampedPos;
            }
            else {
                _ememyPointer.DisactiveSelf();
            }
            await UniTask.Yield();
        }
    }


    private Vector2 ClampToContainer(Vector2 screenPos) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _containerRect, screenPos, null, out Vector2 localPoint
        );
    
        Rect rect = _containerRect.rect;
        localPoint.x = Mathf.Clamp(localPoint.x, rect.xMin, rect.xMax);
        localPoint.y = Mathf.Clamp(localPoint.y, rect.yMin, rect.yMax);
    
        Vector2 worldPoint = _containerRect.TransformPoint(localPoint);
        return worldPoint;
    }
}
