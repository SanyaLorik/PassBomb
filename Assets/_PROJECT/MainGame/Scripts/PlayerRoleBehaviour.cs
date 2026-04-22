using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Zenject;

public enum BotRoleInGame {
    Hunter, 
    Victim,
    Wanderer
}


[RequireComponent(typeof(Collider))]
public class PlayerRoleBehaviour : MonoBehaviour {
    [SerializeField] public Transform _pointToHoldBomb;
    [SerializeField] private Collider _collider;
    [FormerlySerializedAs("_botWander")] [SerializeField] private BotWalkManager botWalkManager;

    [field:  SerializeField] public bool IsInvincibleAfterBomb { get; private set; }
    [field:  SerializeField] public bool IsInvincibleAfterBonus { get; private set; }
    [field:  SerializeField] public bool PlayerHandle { get; private set; }
    [field:  SerializeField] public BotRoleInGame CurrentRole { get; private set; }
    
    private CancellationTokenSource _tokenSource;
    private CancellationTokenSource _hunterTokenSource;
    
    // Для асинхронной передачи
    private static float _lastPassTime = -999f;
    private const float PASS_COOLDOWN = 0.5f;
    
    [Inject] private Bomb _bomb;
    [Inject] private MapsToBattleChanger _mapsChanger;
    [Inject] private BattleManager _battleManager;
    [Inject] private GameData _gameData;
    
    private List<IPassBombPlayer> _otherPlayers = new();
    
    
    private void Awake() {
        SetColliderEnable(false);
    }

    
    public void NewRoundStarted(bool started) {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        CurrentRole = BotRoleInGame.Wanderer;
        SetColliderEnable(started);
        _otherPlayers.Clear();
        _otherPlayers = _battleManager.Players.Where(p => p.RoleBehaviour != this).ToList();
        if (started) {
            SetRole(BotRoleInGame.Wanderer);
        }
    }

    
    private void OnTriggerEnter(Collider collider) {
        if(IsInvincibleAfterBonus) return; 
        if(IsInvincibleAfterBomb) return;
        // Если просто бродилка то никак не влияет на триггеры,
        if(CurrentRole != BotRoleInGame.Hunter) return;
        
        if (Time.time - _lastPassTime < PASS_COOLDOWN) {
            // Debug.Log("Передача заблокирована глобальным кулдауном");
            return;
        }
        
        if (!collider.TryGetComponent(out PlayerRoleBehaviour player)) return;
        if (player.IsInvincibleAfterBonus) return;
        
        // Debug.Log($"Охотник передал бомбу, PlayerHandle = {PlayerHandle}");
        
        player.SetRole(BotRoleInGame.Hunter);
        SetRole(BotRoleInGame.Wanderer);
        
        StartInvinsibleTimer(_gameData.TimeToInvinsibleAfterPass).Forget();
        _lastPassTime = Time.time;
        
    }

    public void SetRole(BotRoleInGame role) {
        CurrentRole = role;
        UniTaskHelper.DisposeTask(ref _tokenSource);
        UniTaskHelper.DisposeTask(ref _hunterTokenSource);
        _tokenSource = new CancellationTokenSource();
        
        switch (role) {
            case BotRoleInGame.Hunter:
                _bomb.InitBombToNewPlayer(_pointToHoldBomb, this);
                StartHunting(_tokenSource.Token).Forget();
                break;
            case BotRoleInGame.Victim:
                Run(_tokenSource.Token).Forget();
                break;
            case BotRoleInGame.Wanderer:
                WanderingInPlace(_tokenSource.Token).Forget();
                break;
        }
    }


    public void SetInvincibleAfterBonus(bool invincible) {
        IsInvincibleAfterBonus = invincible;
    }

    
    private IPassBombPlayer _targetToHunt;
    
    private async UniTask StartHunting(CancellationToken token) {
        GameEvents.PlayerStayHunterInvoke(this);
        if(PlayerHandle) return;
        GetNextPlayerVictim();
        // Запускаем таймер каждый раз в фоне просто чекать ближайшего
        GetNextVictimByTimerAsync(token).Forget();
        while (!token.IsCancellationRequested) {
            // За типом бегаем постоянно выбранным
            botWalkManager.SetAgentGoToPoint(GetNavMeshPosition(_targetToHunt.Transform.position));
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

    private CancellationToken GetNewHuntToken() {
        UniTaskHelper.DisposeTask(ref _hunterTokenSource);
        _hunterTokenSource = new CancellationTokenSource();
        return _hunterTokenSource.Token;
    }
    
    private void GetNextPlayerVictim() {
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
            Vector3 target = botWalkManager.GetTargetPoint(_mapsChanger.GetCurrentMapFloor, _mapsChanger.GetCurrentMapHeight);
            await botWalkManager.SetAgentGoToPointAsync(target, token);
        }
    }
    
    
    private async UniTask WanderingInPlace(CancellationToken token) {
        if(PlayerHandle) return;
        while (!token.IsCancellationRequested) {
            Vector3 target = botWalkManager.GetTargetPoint(_mapsChanger.GetCurrentMapFloor, _mapsChanger.GetCurrentMapHeight);
            await botWalkManager.SetAgentGoToPointAsync(target, token);
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