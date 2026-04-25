using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Zenject;
using Random = UnityEngine.Random;

public enum PlayerRoleInGame {
    Hunter, 
    Victim,
    Wanderer
}


[RequireComponent(typeof(Collider))]
public class PlayerRoleBehaviour : MonoBehaviour {
    [SerializeField] private BotStateManager _botStateManager;
    
    
    [SerializeField] public Transform _pointToHoldBomb;
    [SerializeField] private Collider _collider;

    [field:  SerializeField] public bool IsInvincibleAfterBomb { get; private set; }
    [field:  SerializeField] public bool IsInvincibleAfterBonus { get; private set; }
    [field:  SerializeField] public bool PlayerHandle { get; private set; }
    [field:  SerializeField] public PlayerRoleInGame CurrentRole { get; private set; }

    public PlayerRoleBehaviour LastPlayerContact { get; private set; }


    private CancellationTokenSource _tokenSource;
    private CancellationTokenSource _hunterTokenSource;
    private IPassBombPlayer _targetToHunt;
    
    
    // Для асинхронной передачи
    private static float _lastPassTime = -999f;
    private const float PASS_COOLDOWN = 0.5f;
    private float _lastRepulseTime = -999f;
    private float REPULSE_COOLDOWN = 0.5f;
    private BotWalkManager _botWalkManager;

    private IPassBombPlayer PassBombPlayer;

    public event Action<PlayerRoleInGame> PlayerRoleChanged;

    [Inject] private Bomb _bomb;
    [Inject] private MapsToBattleChanger _mapsChanger;
    [Inject] private BattleManager _battleManager;
    [Inject] private GameData _gameData;
    [Inject] private IPassBombPlayer _mainPlayer;
    
    
    private List<IPassBombPlayer> _otherPlayers = new();
    
    
    private void Awake() {
        SetColliderEnable(false);
        InitPassBomb();
    }

    private void InitPassBomb() {
        if (_botStateManager != null) {
            _botWalkManager = _botStateManager.BotWalkManager;
            PassBombPlayer = _botStateManager;
        }
        else {
            PassBombPlayer = _mainPlayer;
        }
    }


    private void OnTriggerEnter(Collider collider) {
        if(IsInvincibleAfterBonus || IsInvincibleAfterBomb) return; 
        // Если просто бродилка то никак не влияет на триггеры,
        if (!collider.TryGetComponent(out PlayerRoleBehaviour player)) return;
        if(player == this) return;
        if(player.PassBombPlayer == PassBombPlayer) return;
        
        if (player.IsInvincibleAfterBonus) return;
        
        if (Time.time - _lastRepulseTime > REPULSE_COOLDOWN) {
            Vector3 direction = (player.PassBombPlayer.Transform.position - transform.position).normalized;
            if (direction.sqrMagnitude < 0.001f)
                direction = new Vector3(Random.value, Random.value, Random.value);

            LastPlayerContact = player;
            player.PassBombPlayer.PushAway(direction);
            _lastRepulseTime = Time.time;
        }
        
        
        if(CurrentRole != PlayerRoleInGame.Hunter) return;
        
        if (Time.time - _lastPassTime < PASS_COOLDOWN) {
            // Debug.Log("Передача заблокирована глобальным кулдауном");
            return;
        }
        
        // Debug.Log($"Охотник передал бомбу, PlayerHandle = {PlayerHandle}");
        
        player.SetRole(PlayerRoleInGame.Hunter);
        SetRole(PlayerRoleInGame.Wanderer);
        
        StartInvinsibleTimer(_gameData.TimeToInvinsibleAfterPass).Forget();
        _lastPassTime = Time.time;
        
    }


    public void DisposeAllLogic() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        UniTaskHelper.DisposeTask(ref _hunterTokenSource);
    }


    public void NewRoundStart(bool started) {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        CurrentRole = PlayerRoleInGame.Wanderer;
        SetColliderEnable(started);
        _otherPlayers.Clear();
        _otherPlayers = _battleManager.Players.Where(p => p.RoleBehaviour != this).ToList();
        if (started) {
            SetRole(PlayerRoleInGame.Wanderer);
        }
    }
    

    public void SetRole(PlayerRoleInGame role) {
        CurrentRole = role;
        PlayerRoleChanged?.Invoke(role);
        
        UniTaskHelper.DisposeTask(ref _tokenSource);
        UniTaskHelper.DisposeTask(ref _hunterTokenSource);
        _tokenSource = new CancellationTokenSource();
        
        switch (role) {
            case PlayerRoleInGame.Hunter:
                _bomb.InitBombToNewPlayer(_pointToHoldBomb, this);
                StartHunting(_tokenSource.Token).Forget();
                break;
            case PlayerRoleInGame.Victim:
                Run(_tokenSource.Token).Forget();
                break;
            case PlayerRoleInGame.Wanderer:
                WanderingInPlace(_tokenSource.Token).Forget();
                break;
        }
    }


    public void SetInvincibleAfterBonus(bool invincible) {
        IsInvincibleAfterBonus = invincible;
    }
    
    public void SetInvinsibleAfterBomb(bool invinsible) {
        IsInvincibleAfterBomb = invinsible;
    }

    
    
    private async UniTask StartHunting(CancellationToken token) {
        GameEvents.PlayerStayHunterInvoke(this);
        if(PlayerHandle) return;
        GetNextPlayerVictim();
        // Запускаем таймер каждый раз в фоне просто чекать ближайшего
        GetNextVictimByTimerAsync(token).Forget();
        while (!token.IsCancellationRequested) {
            // За типом бегаем постоянно выбранным
            _botWalkManager.SetAgentGoToPoint(GetNavMeshPosition(_targetToHunt.Transform.position));
            await UniTask.WaitForSeconds(_gameData.DurationToGoInPoint ,cancellationToken: token);
            if (_targetToHunt.RoleBehaviour.IsInvincibleAfterBomb || _targetToHunt.RoleBehaviour.IsInvincibleAfterBonus) {
                GetNextPlayerVictim();
            }
        }
    }

    private Vector3 GetNavMeshPosition(Vector3 target) {
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, _gameData.DistanceToFloor, NavMesh.AllAreas)) {
            return hit.position;
        }
        return target;
    }

    private async UniTask GetNextVictimByTimerAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            await UniTask.WaitForSeconds(_gameData.DurationToHuntWithoutCheck, cancellationToken: token);
            GetNextPlayerVictim();
        }
    }

    
    private void GetNextPlayerVictim() {
        if (Random.value < _gameData.ChanceToGoPlayerInHunt && _battleManager.MainPlayerPlay) {
            _targetToHunt = _mainPlayer;
            return;
        }
        IPassBombPlayer closest = _otherPlayers[0];
        float minSqrDistance = float.MaxValue;
    
        foreach (var victim in _otherPlayers) {
            if(victim.RoleBehaviour.IsInvincibleAfterBomb || victim.RoleBehaviour.IsInvincibleAfterBonus) continue;
            Vector3 offset = victim.Transform.position - transform.position;
            float sqrDist = offset.sqrMagnitude; // БЕЗ КОРНЯ!
        
            if (sqrDist < minSqrDistance) {
                minSqrDistance = sqrDist;
                closest = victim;
            }
        }
        // Debug.Log("Найдена жертва: " +  closest.);
        _targetToHunt = closest;
    }

    
    private async UniTask Run(CancellationToken token) {
        if(PlayerHandle) return;
        Debug.Log("Run");
        // Пока просто бегает по площади
        while (!token.IsCancellationRequested) {
            Vector3 target = _botWalkManager.GetTargetPoint(_mapsChanger.GetCurrentMapFloor, _mapsChanger.GetCurrentMapHeight);
            await UniTask.WaitWhile(() => _botWalkManager.IsPushed, cancellationToken: token);
            await _botWalkManager.SetAgentGoToPointAsync(target, token);
        }
    }
    
    
    private async UniTask WanderingInPlace(CancellationToken token) {
        if(PlayerHandle) return;
        while (!token.IsCancellationRequested) {
            await UniTask.WaitWhile(() => _botWalkManager.IsPushed, cancellationToken: token);
            Vector3 target = _botWalkManager.GetTargetPoint(_mapsChanger.GetCurrentMapFloor, _mapsChanger.GetCurrentMapHeight);
            await _botWalkManager.SetAgentGoToPointAsync(target, token);
        }
    }

    
    private async UniTask StartInvinsibleTimer(float time) {
        SetColliderEnable(false);
        await UniTask.WaitForSeconds(time);
        SetColliderEnable(true);
    }

    
    private void SetColliderEnable(bool enable) {
        _collider.enabled = enable;
        IsInvincibleAfterBomb = !enable;
    }


    private void OnDestroy() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
    }
}