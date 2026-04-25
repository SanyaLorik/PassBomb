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
    [SerializeField] private Rigidbody _rb;
    
    
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

    private CancellationTokenSource _botTokenSource;
    private CancellationTokenSource _jumpTokenSource; 
    private CancellationTokenSource _pushTokenSource;
    private NavMeshAgent _agent;
    private Vector3 _lastDestination;
    
    private float _jumpForce;
    private float _jumpDuration;
    public bool IsPushed { get; private set; }
    private bool _isJumping;
    
    [Inject] private GameData _gameData;
    [Inject] private NavMeshHelper _navMeshHelper;
    [Inject] private BotsMainManager _mainManager;
    
    
    private void Awake() {
        _agent = GetComponent<NavMeshAgent>();
    }

    private bool CanUseAgent =>
        _agent != null &&
        _agent.enabled &&
        _agent.isOnNavMesh;
    
    private void Start() {
        SetBigJump(false);
        _agent.updateRotation = false;
    }

    
    private void Update() {
        if (IsPushed) return;
        
        RotateByVelocity();
        MonitorMovement();
    }
    
        
    private async UniTask StartWanderingCycleAsync() {
        if (!gameObject.activeSelf) return;
        
        _botTokenSource = new CancellationTokenSource();

        float durationToStay = 0f;
        if (Random.value > 0.5f) {
            durationToStay = Random.Range(_gameData.TimeToStayAfterSpawn.From, _gameData.TimeToStayAfterSpawn.To);
        }
        
        await UniTask.WaitForSeconds(durationToStay, cancellationToken: _botTokenSource.Token);
        await LifeCycleAsync(_botTokenSource.Token);
    }
    
    
    private async UniTask LifeCycleAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            await UniTask.WaitUntil(() => CanUseAgent, cancellationToken: token);
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



    private async UniTask EnterPushModeAsync()
    {
        IsPushed = true;

        // стопаем всё игровое поведение
        UniTaskHelper.DisposeTask(ref _jumpTokenSource);
        UniTaskHelper.DisposeTask(ref _botTokenSource);

        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;

        _agent.enabled = false;

        await UniTask.Yield(PlayerLoopTiming.Update);
    }

    public void PushAway(Vector3 direction) {
        if (IsPushed) return;

        UniTaskHelper.DisposeTask(ref _pushTokenSource);
        _pushTokenSource = new CancellationTokenSource();

        PushAwayAsync(direction, _pushTokenSource.Token).Forget();
    }
    
    private async UniTask PushAwayAsync(Vector3 direction, CancellationToken token) {
        await EnterPushModeAsync();

        await PushJump(direction, token);
    }
    
    
    private async UniTask PushJump(Vector3 direction, CancellationToken token) {
        float height = _gameData.BotUpPushRatio;
        float duration = _gameData.PushTime;
        float force = _gameData.BotPushForce;

        Vector3 startPos = transform.position;
        Vector3 velocity = direction.normalized * force;

        float startY = startPos.y;
        float t = 0f;

        Grounded?.Invoke(false);
        _jumpParticlesController.Play();

        // ПАРАБОЛА
        _rb.isKinematic = true;
        _rb.useGravity = false;

        while (t < duration && !token.IsCancellationRequested) {
            t += Time.deltaTime;
            float n = t / duration;

            Vector3 pos = startPos + velocity * t;
            pos.y = startY + Mathf.Sin(n * Mathf.PI) * height;

            transform.position = pos;

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        // ВКЛЮЧАЕМ ФИЗИКУ
        await FallWithPhysics(token);
    }
    
    private async UniTask FallWithPhysics(CancellationToken token) {
        float maxTime = 5f;
        float t = 0f;

        // включаем физику
        _rb.isKinematic = false;
        _rb.useGravity = true;

        // важно: задаём начальную скорость вниз
        _rb.angularVelocity = Vector3.zero;
        _rb.linearVelocity = Vector3.down * _gameData.BotFallSpeed;

        while (!token.IsCancellationRequested) {
            t += Time.fixedDeltaTime;

            Vector3 pos = _rb.position;

            // проверка: коснулись ли чего-то
            bool grounded = Physics.SphereCast(
                pos + Vector3.up * 0.2f, 
                0.3f,
                Vector3.down,
                out RaycastHit hit,
                1.5f
            );

            if (grounded && _rb.linearVelocity.y <= 0f)
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
                {
                    _rb.position = hit.point;
                    FinishLanding(navHit.position);
                    return;
                }
            }

            // улетел в бездну
            if (t > maxTime || pos.y < -200f) {
                Debug.Log("Bot fell into void");
                _mainManager.FellInVoidWanderer(this);
                StopPhys();
                Grounded?.Invoke(true);
                return;
            }

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }
    
    
    private void FinishLanding(Vector3 navMeshPos)
    {
        StopPhys();

        _agent.enabled = true;

        if (NavMesh.SamplePosition(navMeshPos, out var hit, 1f, NavMesh.AllAreas))
        {
            _agent.Warp(hit.position);
        }

        // ВАЖНО: проверка перед любыми действиями
        if (_agent.isOnNavMesh) {
            _agent.nextPosition = _agent.transform.position;
            _agent.ResetPath();
            _agent.isStopped = false;
        }
        else {
            Debug.LogWarning("Agent not on NavMesh after landing");
            _mainManager.FellInVoidWanderer(this);
            return;
        }

        Grounded?.Invoke(true);
        _landParticleController.Play();
    }

    public void StopPhys() {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        
        _rb.isKinematic = true;
        _rb.useGravity = false;
        IsPushed = false;
    }


    public void StopWanderSpawn() {
        ResetLogic();
    }
    
    
    public void StartWanderSpawn() {
        ResetLogic();

        _agent.isStopped = false;
        StartWanderingCycleAsync().Forget();
    }

    
    private void ResetLogic() {
        Debug.Log("StartWanderSpawn");
        UniTaskHelper.DisposeTask(ref _botTokenSource);
        UniTaskHelper.DisposeTask(ref _jumpTokenSource);
        
        _agent.velocity = Vector3.zero;
        _agent.ResetPath();
        _agent.nextPosition = transform.position;

        _walkingParticles.Stop();
        StartWandering?.Invoke(false);
    }


    public void SetMovingStatus(bool enable) {
        if(!gameObject.activeSelf) return;
        _agent.isStopped = !enable;
        _agent.ResetPath();
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
        _jumpDuration = bigJump ? _gameData.BotJumpBonusDuration : _gameData.BotJumpDuration;
    }
    


    public void SetAgentGoToPoint(Vector3 point) {
        if (!CanUseAgent) return;
        // Проверяем, реально ли изменилась цель
        if (Vector3.Distance(_lastDestination, point) > DESTINATION_CHANGE_THRESHOLD) {
            _agent.SetDestination(point);
            _lastDestination = point;
        }
        if (CanUseAgent)
            _agent.isStopped = false;
    }
    
    
    public async UniTask SetAgentGoToPointAsync(Vector3 point, CancellationToken token) {
        if (!CanUseAgent) return;

        _agent.SetDestination(point);

        await UniTask.WaitUntil(
            () => !token.IsCancellationRequested && CanUseAgent && !_agent.pathPending,
            cancellationToken: token
        );

        if (!CanUseAgent) return;

        await Jump(token);

        if (!CanUseAgent) return;

        if (_agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        await UniTask.WaitUntil(
            () => !token.IsCancellationRequested &&
                  CanUseAgent &&
                  !_agent.pathPending &&
                  _agent.remainingDistance <= _gameData.RunStoppingDistance,
            cancellationToken: token
        );
    }


    private void RotateByVelocity() {
        Vector3 velocity = _agent.velocity;
        velocity.y = 0;
        
        
        if (velocity.sqrMagnitude < 0.001f)
            return;
    
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

        _isJumping = true;
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

        float startY = transform.position.y;
        Grounded?.Invoke(false);
        while (t < _jumpDuration && !token.IsCancellationRequested) {
            t += Time.deltaTime;
            float normalized = t / _jumpDuration;
            float yOffset = Mathf.Sin(normalized * Mathf.PI) * height;

            Vector3 pos = transform.position;
            pos.y = startY + yOffset;

            transform.position = pos;

            await UniTask.Yield(token);
        }
        Grounded?.Invoke(true);
        _landParticleController.Play();
        _isJumping = false;
    }
    
    
    private void OnDestroy() {
        UniTaskHelper.DisposeTask(ref _botTokenSource);
        UniTaskHelper.DisposeTask(ref _jumpTokenSource);
    }
    
}