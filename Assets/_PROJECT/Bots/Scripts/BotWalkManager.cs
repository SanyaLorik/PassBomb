using System;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using SanyaBeerExtension;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Random = UnityEngine.Random;

public class BotWalkManager : MonoBehaviour {
    private const float DESTINATION_CHANGE_THRESHOLD = 0.5f;
    
    
    [Header("Партиклы")]
    [SerializeField] private JumpParticlesController _jumpParticlesController;
    [SerializeField] private JumpParticlesController _landParticleController;
    [SerializeField] private DualLegParticles _walkingParticles;
    [SerializeField] private Transform[] _spawnPlaces;
    [SerializeField] private float _yToFind;
    [SerializeField] private AnimatedLinkTraversal _animatedLinkTraversal;

    
    
    public Action<bool> StartWandering;
    public Action<bool> Grounded;
    public Action OnJump;
    public Action OnDoubleJump;

    private Transform _chooseCube;
    private CancellationTokenSource _botTokenSource;
    private CancellationTokenSource _jumpTokenSource; 
    private NavMeshAgent _agent;
    private Vector3 _lastDestination;
    private float _jumpForce;
    
    [Inject] private GameData _gameData;
    [Inject] private NavMeshHelper _navMeshHelper;
    
    
    private void Awake() {
        _agent = GetComponent<NavMeshAgent>();
    }

    
    private void Start() {
        SetBigJump(false);
        _agent.updateRotation = false;
    }

    
    private void Update() {
        RotateByVelocity();
        MonitorMovement();
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

    
    public Vector3 GetTargetPoint(Transform point, float yToFind) {
        return _navMeshHelper.CalculateBotTargetPoint(point, yToFind);
    }
    
    
    public Vector3 GetTargetPoint(Transform[] points, float yToFind) {
        Transform point =  points.GetRandomElement();
        return _navMeshHelper.CalculateBotTargetPoint(point, yToFind);
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
            Vector3 target = GetTargetPoint(_spawnPlaces, _yToFind);
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


    private void RotateByVelocity() {
        Vector3 velocity = _agent.velocity;
        velocity.y = 0;
    
        float sqrMag = velocity.sqrMagnitude;
    
        // Ранний выход если стоим (уже есть)
        if (sqrMag < 0.01f && !_animatedLinkTraversal.IsJumpingTraversal)
            return;
    
        // ДОПОЛНИТЕЛЬНО: не вращать если почти смотрим куда надо
        Quaternion targetRotation = Quaternion.LookRotation(velocity);
    
        // Если уже почти повернуты - пропускаем Slerp
        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
            return;
    
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            _gameData.RotationSpeed * Time.deltaTime
        );
    }
    
    
    private void MonitorMovement() {
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
        while (t < _gameData.BotJumpDuration && !token.IsCancellationRequested) {
            t += Time.deltaTime;
            float normalized = t / _gameData.BotJumpDuration;
            float yOffset = Mathf.Sin(normalized * Mathf.PI) * height;

            Vector3 pos = transform.position;
            pos.y = _agent.nextPosition.y + yOffset;

            transform.position = pos;

            await UniTask.Yield(token);
        }
        Grounded?.Invoke(true);
        _landParticleController.Play();
    }
    
    
    private void OnDestroy() {
        UniTaskHelper.DisposeTask(ref _botTokenSource);
        UniTaskHelper.DisposeTask(ref _jumpTokenSource);
    }
    
}