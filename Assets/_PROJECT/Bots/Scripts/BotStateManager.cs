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
        _roleBehaviour.SetInvincibleAfterBonus(false);
        _roleBehaviour.SetInvincibleAfterBomb(false);
        
        PlayerStatusChanged?.Invoke(goPlay);
        IsPlaying = goPlay;
        _agent.enabled = true;
        BotWalkManager.StopPhys();
        BotWalkManager.DisposeAllLogic();
        
        gameObject.SetActive(ShowInSpawn || goPlay);
        
        if (goPlay) {
            ActiveBotInGame();
        }
        // Возвращение на спавн
        else {
            Debug.Log($"Возвращение на спавн игрока {_botMonolog.NickName} in {_spawn.SpawnPoint.position}");
            Debug.Log($"Игрок play статус {IsPlaying} in {_spawn.SpawnPoint.position}");
            SetBotStateBeforeGame();
            TeleportToPoint(_spawn.SpawnPoint.position);
        }
        SetStartWanderIfActive(!goPlay);
    }

    

    public void SetPlayStatusSilent(bool goPlay) {
        IsPlaying = goPlay;
    }


    public void TeleportToPoint(Vector3 pos) {
            // 1. СНАЧАЛА отменяем всё у BotWalkManager
            BotWalkManager.ResetLogic(); 
        
            if (NavMesh.SamplePosition(pos, out var hit, 5f, NavMesh.AllAreas)) {
                _agent.enabled = false;
                transform.position = hit.position;
                _agent.enabled = true;
            
                // После включения агент может ещё не быть isOnNavMesh
                // Даём кадр на инициализацию через ForceUpdateCanvases не поможет,
                // лучше просто проверить
                if (_agent.isOnNavMesh) {
                    _agent.isStopped = true;
                }
                // Debug.Log($"Телепорт: {transform.position}");
            } 
            else
            {
                Debug.LogError($"SamplePosition НЕ нашел точку рядом с {pos}");
            }
    }

    
    public void SetMovingStatus(bool enable) {
        BotWalkManager.SetMovingStatus(enable);
    }

    
    public void SetDefaultSpeed() {
        _agent.speed = _gameData.BotSpeed;
        MoveStatusChanged?.Invoke(MoveStatus.SuperSpeed, false);
        // Debug.Log($"SetDefaultSpeed {_botMonolog.NickName}");
    }
    
    
    public void SetHunterSpeed() {
        _agent.speed = _gameData.HunterSpeed;
        MoveStatusChanged?.Invoke(MoveStatus.SuperSpeed, true);
        // Debug.Log($"SetHunterSpeed {_botMonolog.NickName}");
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

    
    public void SetInvincible(bool invincible) {
        _roleBehaviour.SetInvincibleAfterBonus(invincible);
        MoveStatusChanged?.Invoke(MoveStatus.Invincible, invincible);
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