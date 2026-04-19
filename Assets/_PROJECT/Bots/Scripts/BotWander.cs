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
    [SerializeField] private AnimatedLinkTraversal _animatedLinkTraversal;


    private float _jumpForce;
    
    
    public Action<bool> StartWandering;
    public Action OnJump;
    public Action OnDoubleJump;
    public Action<bool> Grounded;
    private Transform _chooseCube;
    private CancellationTokenSource _botTokenSource;
    private CancellationTokenSource _jumpTokenSource; 
    private NavMeshAgent _agent;
    
    
    [Inject] private PlayerMovement _playerMovement;
    [Inject] private GameData _gameData;
    

    private async UniTask MonitorMovementAsync() {
        while (true) {
            if (_agent.enabled && _agent.velocity.sqrMagnitude > 0.05f) {
                if (!_walkingParticles.IsPlaying) {
                    _walkingParticles.Play();
                    StartWandering?.Invoke(true);
                }
            }
            else {
                if (_walkingParticles.IsPlaying && !_animatedLinkTraversal.IsJumpingTraversal) {
                    _walkingParticles.Stop();
                    StartWandering?.Invoke(false);
                }
            }
   
            await UniTask.Yield();
        }
    }
    
    
    private void Awake() {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start() {
        MonitorMovementAsync().Forget();
        SetBigJump(false);
        _agent.updateRotation = false;
    }


    public void StopWanderSpawn() {
        Debug.Log("StopWanderSpawn");
        UniTaskHelper.DisposeTask(ref _botTokenSource);
        UniTaskHelper.DisposeTask(ref _jumpTokenSource);
        
        _agent.ResetPath();
        _walkingParticles.Stop();
        StartWandering?.Invoke(false);
    }
    
    
    public void StartWanderSpawn() {
        Debug.Log("StartWanderSpawn");
        UniTaskHelper.DisposeTask(ref _botTokenSource);
        UniTaskHelper.DisposeTask(ref _jumpTokenSource);
        
        _botTokenSource = new CancellationTokenSource();
        _agent.ResetPath();
        _agent.isStopped = false;
        StartWanderingCycleAsync().Forget();
        _walkingParticles.Stop();
        StartWandering?.Invoke(false);
    }

    public void SetMovingStatus(bool enable) {
        _agent.isStopped = !enable;
        Debug.Log("SetMovingStatus " + enable);
    }

    public void SetBigJump(bool bigJump) {
        _jumpForce = bigJump ? _gameData.BotJumpBonusHeight : _gameData.BotDefaultJumpHeight;
    }
    
    private async UniTask StartWanderingCycleAsync() {
        float durationToStay = Random.Range(_gameData.TimeToStayAfterSpawn.From, _gameData.TimeToStayAfterSpawn.To);
        await UniTask.WaitForSeconds(durationToStay, cancellationToken: _botTokenSource.Token);
        await LifeCycleAsync(_botTokenSource.Token);
    }
    
    
    private async UniTask LifeCycleAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            Debug.Log("LifeCycleAsync");
            Vector3 target = GetTargetPoint(_spawnPlace);
            _agent.SetDestination(target);
            
            await UniTask.WaitUntil(() => !_agent.pathPending && _agent.hasPath, cancellationToken: token);
            Jump(token).Forget();

            await UniTask.WaitUntil(() => 
                !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance,
                cancellationToken: token);

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
        
        // // Запускаем поворот В ФОНЕ — теперь крутимся ПО СКОРОСТИ, а не на точку
        await UniTask.WaitUntil(() => !_agent.pathPending, cancellationToken: token);
        await Jump(token);
    
        if (_agent.pathStatus != NavMeshPathStatus.PathComplete) {
            return;
        }
    
        await UniTask.WaitUntil(
            () => !_agent.pathPending && _agent.remainingDistance <= _gameData.RunStoppingDistance,
            cancellationToken: token
        );

    }

    // Новый метод — крутит по velocity (туда, куда РЕАЛЬНО бежит)
    private void Update() {
        RotateByVelocity();
    }

    private void RotateByVelocity()
    {
        Vector3 velocity = _agent.velocity;
        velocity.y = 0;

        if (velocity.sqrMagnitude < 0.01f && !_animatedLinkTraversal.IsJumpingTraversal)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(velocity);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            _gameData.RotationSpeed * Time.deltaTime
        );
    }

    
    private async UniTask Jump(CancellationToken token) {
        if (Random.value > _gameData.ChanceToJump) return;
        
        float startPathLength = _agent.remainingDistance;
        float jumpLength = startPathLength / Random.Range(1.5f, 2f);


        UniTaskHelper.DisposeTask(ref _jumpTokenSource);
        _jumpTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        CancellationToken jumpToken = _jumpTokenSource.Token;
        
        await UniTask.WaitUntil(() => 
                !_agent.pathPending &&
                _agent.remainingDistance <= jumpLength &&
                _agent.remainingDistance > _agent.stoppingDistance, 
            cancellationToken: jumpToken);

        FakeJump(jumpToken).Forget();
    }
    
    
    [SerializeField] private float _jumpDuration;
    private async UniTask FakeJump(CancellationToken token) {
        float height = _jumpForce;
        float t = 0f;

        _jumpParticlesController.Play();
        if (Random.value > 0.7f) {
            OnJump?.Invoke();
        }
        else {
            OnDoubleJump?.Invoke();
        }
        
        Grounded?.Invoke(false);
        while (t < _jumpDuration && !token.IsCancellationRequested) {
            t += Time.deltaTime;
            float normalized = t / _jumpDuration;
            float yOffset = Mathf.Sin(normalized * Mathf.PI) * height;

            Vector3 pos = transform.position;
            pos.y = _agent.nextPosition.y + yOffset;

            transform.position = pos;

            await UniTask.Yield(token);
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

    
    
    private void OnDestroy() {
        UniTaskHelper.DisposeTask(ref _botTokenSource);
        UniTaskHelper.DisposeTask(ref _jumpTokenSource);
    }
    
}