using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using SanyaBeerExtension;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Random = UnityEngine.Random;

public class BotWander : MonoBehaviour {
    [Header("Партиклы")]
    [SerializeField] private JumpParticlesController _jumpParticlesController;
    [SerializeField] private JumpParticlesController _landParticleController;
    [SerializeField] private DualLegParticles _walkingParticles;
    [SerializeField] private Transform _spawnPlace;
    
    
    public Action<bool> StartWandering;
    public Action OnJump;
    public Action<bool> Grounded;
    private Transform _chooseCube;
    private CancellationTokenSource _botTokenSource;
    private NavMeshAgent _agent;
    
    
    [Inject] private PlayerMovement _playerMovement;
    [Inject] private GameData _gameData;
    

    private void Awake() {
        _agent = GetComponent<NavMeshAgent>();
    }

    
    
    public void StopWanderSpawn() {
        UniTaskHelper.DisposeTask(ref _botTokenSource);
        _botTokenSource = new CancellationTokenSource();
        MonitorMovementAsync(_botTokenSource.Token).Forget();
        _walkingParticles.Stop();
        StartWandering?.Invoke(false);
    }
    
    
    public void StartWanderSpawn() {
        // _botTokenSource = new CancellationTokenSource();
        // StartWanderingCycleAsync().Forget();
        // _walkingParticles.Stop();
        // StartWandering?.Invoke(false);
    }

    
    private async UniTask StartWanderingCycleAsync() {
        UniTaskHelper.DisposeTask(ref _botTokenSource);
        _botTokenSource = new CancellationTokenSource();
        float durationToStay = Random.Range(_gameData.TimeToStayAfterSpawn.From, _gameData.TimeToStayAfterSpawn.To);
        await UniTask.WaitForSeconds(durationToStay, cancellationToken: _botTokenSource.Token);
        LifeCycleAsync(_botTokenSource.Token).Forget();
        MonitorMovementAsync(_botTokenSource.Token).Forget();
    }
    

    private async UniTask MonitorMovementAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            if (_agent.enabled && _agent.velocity.sqrMagnitude > 0.05f) {
                if (!_walkingParticles.IsPlaying) {
                    _walkingParticles.Play();
                    StartWandering?.Invoke(true);
                }
            }
            else {
                if (_walkingParticles.IsPlaying) {
                    _walkingParticles.Stop();
                    StartWandering?.Invoke(false);
                }
            }

            await UniTask.Yield(token);
        }
    }

    
    private async UniTask LifeCycleAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            
            Vector3 target = GetTargetPoint(_spawnPlace);
            _agent.SetDestination(target);
            _agent.stoppingDistance = Random.Range(
                _gameData.StoppingDistance.From,
                _gameData.StoppingDistance.To);
            
            
            await UniTask.WaitUntil(() => !_agent.pathPending && _agent.hasPath, cancellationToken: token);
            Jump().Forget();

            await UniTask.WaitUntil(() => 
                !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance,
                cancellationToken: token);
            
            await RotateTowardsAsync(target, _gameData.RotationSpeed, token);

            float waitTime = Random.Range(
                _gameData.TimeToStayOnPoint.From, 
                _gameData.TimeToStayOnPoint.To);
            await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
        }
    }


    
    private Vector3 _lastDestination;
    private const float DESTINATION_CHANGE_THRESHOLD = 0.5f;

    public void SetAgentGoToPoint(Vector3 point) {
        // Проверяем, реально ли изменилась цель
        if (Vector3.Distance(_lastDestination, point) > DESTINATION_CHANGE_THRESHOLD) {
            _agent.SetDestination(point);
            _lastDestination = point;
        }
        _agent.isStopped = false;
    }
    
    
    public async UniTask SetAgentGoToPointAsync(Vector3 point, CancellationToken token) {
        _agent.SetDestination(point);

        await UniTask.WaitUntil(() => !_agent.pathPending, cancellationToken: token);

        if (_agent.pathStatus != NavMeshPathStatus.PathComplete) {
            return;
        }

        // Запускаем поворот В ФОНЕ — теперь крутимся ПО СКОРОСТИ, а не на точку
        var rotationTask = RotateByVelocityAsync(_gameData.RotationSpeed, token);

        try {
            await UniTask.WaitUntil(
                () => !_agent.pathPending && _agent.remainingDistance <= _gameData.RunStoppingDistance,
                cancellationToken: token
            );
        }
        catch (OperationCanceledException) {
            _agent.ResetPath();
            throw;
        }

        _agent.ResetPath();
    }

    // Новый метод — крутит по velocity (туда, куда РЕАЛЬНО бежит)
    private async UniTask RotateByVelocityAsync(float rotationSpeed, CancellationToken token) {
        while (!token.IsCancellationRequested) {
            Vector3 velocity = _agent.velocity;
            velocity.y = 0;

            // Если скорость почти нулевая — ждем
            if (velocity.sqrMagnitude < 0.1f) {
                await UniTask.Yield(token);
                continue;
            }

            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
            float angle = Quaternion.Angle(transform.rotation, targetRotation);

            if (angle <= 0.5f) {
                await UniTask.Yield(token);
                continue;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            await UniTask.Yield(token);
        }
    }

    
    private async UniTask Jump() {
        if (Random.value > _gameData.ChanceToJump) return;
        
        float startPathLength = _agent.remainingDistance;
        float jumpLength = startPathLength / Random.Range(1.5f, 2f);

        await UniTask.WaitUntil(() => 
                !_agent.pathPending &&
                _agent.remainingDistance <= jumpLength &&
                _agent.remainingDistance > _agent.stoppingDistance 
        );

        FakeJump().Forget();
    }
    
    [SerializeField] private float _jumpDuration;
    private async UniTask FakeJump() {
        float height = _gameData.JumpForce / 2f;
        float t = 0f;

        _jumpParticlesController.Play();
        OnJump?.Invoke();
        Grounded?.Invoke(false);
        while (t < _jumpDuration) {
            t += Time.deltaTime;
            float normalized = t / _jumpDuration;
            float yOffset = Mathf.Sin(normalized * Mathf.PI) * height;

            Vector3 pos = transform.position;
            pos.y = _agent.nextPosition.y + yOffset;

            transform.position = pos;

            await UniTask.Yield();
        }
        Grounded?.Invoke(true);
        _landParticleController.Play();
    }

    
    public Vector3 GetTargetPoint(Transform point) {
        Vector3 size = point.localScale;

        float offsetX = Random.Range(-size.x/2f, size.x/2f);
        float offsetZ = Random.Range(-size.z/2f, size.z/2f);

        Vector3 target = point.position + new Vector3(offsetX, 0f, offsetZ);

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, _gameData.DistanceToFloor, NavMesh.AllAreas)) {
            return hit.position;
        }

        return point.position;
    }

    
    private async UniTask RotateTowardsAsync(Vector3 target, float rotationSpeed, CancellationToken token) {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Игнорируем разницу по высоте
    
        if (direction == Vector3.zero) return;
    
        Quaternion targetRotation = Quaternion.LookRotation(direction);
    
        // Плавный поворот
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f && !token.IsCancellationRequested) {
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
            await UniTask.Yield(token);
        }
    }
    
    private void OnDestroy() {
        _botTokenSource?.Cancel();
        _botTokenSource?.Dispose();
    }
    
}