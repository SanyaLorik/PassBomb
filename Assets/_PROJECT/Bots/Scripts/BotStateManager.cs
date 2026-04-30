using System;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using SanyaBeerExtension;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

[RequireComponent(typeof(NavMeshAgent))]
public class BotStateManager : MonoBehaviour, IPassBombPlayer {
    [field: SerializeField] public bool ShowInSpawn { get; private set; }
    [field: SerializeField] public Transform Transform { get; private set; }
    [field: SerializeField] public BotWalkManager BotWalkManager { get; private set; }
    [SerializeField] private BotAnimator _botAnimator;
    [SerializeField] private BotMonolog _botMonolog;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private PlayerRoleBehaviour _roleBehaviour;

    public bool IsPlaying { get; private set; }
    public event Action<MoveStatus, bool> MoveStatusChanged;
    public event Action<bool> PlayerStatusChanged;
    private CancellationTokenSource _teleportTokenSource;
    
    
    public string Nickname => _botMonolog.NickName;
    public PlayerRoleBehaviour RoleBehaviour => _roleBehaviour;
    
    
    [Inject] private GameData _gameData;
    [Inject] private SpawnManager _spawn;

    
    
    private void Start() {
        if (!ShowInSpawn) 
            gameObject.DisactiveSelf();
        else
            SetStartWanderIfActive(true);
    }
    

    private void SetStartWanderIfActive(bool startWander) {
        if (ShowInSpawn == false) return;
        
        if(startWander) BotWalkManager.StartWanderSpawn();
        else BotWalkManager.StopWanderSpawn();
    }


    public void SetPlayStatus(bool goPlay) {
        PlayerStatusChanged?.Invoke(goPlay);
        IsPlaying = goPlay;
        _agent.enabled = true;
        BotWalkManager.StopPhys();
        RoleBehaviour.DisposeAllLogic();
        
        gameObject.SetActive(ShowInSpawn || goPlay);
        
        if (goPlay) {
            ActiveBotInGame();
        }
        // Возвращение на спавн
        else {
            Debug.Log($"Возвращение на спавн игрока {_botMonolog.NickName} in {_spawn.SpawnPoint.position}");
            SetBotStateBeforeGame();
            
            UniTaskHelper.DisposeTask(ref _teleportTokenSource);
            _teleportTokenSource = new CancellationTokenSource();
            TpToPointAsync(_spawn.SpawnPoint.position, _teleportTokenSource.Token).Forget();
        }
        SetStartWanderIfActive(!goPlay);
    }

    
    private async UniTask TpToPointAsync(Vector3 point, CancellationToken token) {
        // Имитация задержки + меня заебало что боты не могут вернуться на спавн без траблов
        await UniTask.WaitForSeconds(.3f, cancellationToken: token);
        TeleportToPoint(point);
    }

    public void SetPlayStatusSilent(bool goPlay) {
        IsPlaying = goPlay;
    }


    public void TeleportToPoint(Vector3 pos) {
        // Отменяем тп на спавн
        UniTaskHelper.DisposeTask(ref _teleportTokenSource);
        
        if (NavMesh.SamplePosition(pos, out var hit, 1f, NavMesh.AllAreas)) {
            _agent.Warp(hit.position);
        }
        else {
            if (NavMesh.SamplePosition(pos, out var fallbackHit, 7f, NavMesh.AllAreas)) {
                _agent.Warp(fallbackHit.position);
                Debug.LogWarning($"Спавн не на NavMesh, телепорт рядом: {fallbackHit.position}");
            }
            else {
                Debug.LogError($"Вообще не можем найти NavMesh рядом с {pos}");
            }
        }
    }

    
    public void SetMovingStatus(bool enable) {
        BotWalkManager.SetMovingStatus(enable);
    }

    
    public void SetDefaultSpeed() {
        _agent.speed = _gameData.BotSpeed;
        MoveStatusChanged?.Invoke(MoveStatus.SuperSpeed, false);
    }
    
    public void SetHunterSpeed() {
        _agent.speed = _gameData.HunterSpeed;
        MoveStatusChanged?.Invoke(MoveStatus.SuperSpeed, true);

    }

    
    public void SetBonusSpeed() {
        _agent.speed = _gameData.VelocityBonusSpeed;
        MoveStatusChanged?.Invoke(MoveStatus.SuperSpeed, true);
    }


    
    public void SetBigJump(bool bigJump) {
        BotWalkManager.SetBigJump(bigJump);
        MoveStatusChanged?.Invoke(MoveStatus.SuperJump, bigJump);
    }


    public void PushAway(Vector3 direction) {
        BotWalkManager.PushAway(direction);
    }

    
    public bool IsPushed => BotWalkManager.IsPushed;

    
    public void SetInvinsible(bool invnincible) {
        _roleBehaviour.SetInvincibleAfterBonus(invnincible);
        MoveStatusChanged?.Invoke(MoveStatus.Invincible, invnincible);
    }


    public void RotateToTarget(Vector3 targetPosition) {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }
    
    
    private void SetBotStateBeforeGame() {
        if (ShowInSpawn) {
            _agent.ActiveSelf();
        }
        else {
            _agent.DisactiveSelf();
        }
    }

    
    private void ActiveBotInGame() {
        if (ShowInSpawn == false) {
            _agent.ActiveSelf();
        }
    }
    

    public void SetBotSpeak() {
        _botMonolog.SaySomething();
    }

    public void SetBotStfu() {
        _botMonolog.Stfu();
    }

}