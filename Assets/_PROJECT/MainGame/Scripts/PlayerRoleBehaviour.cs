using System;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using UnityEngine;
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

    [field:  SerializeField] public bool IsInvincibleAfterBomb { get; private set; }
    [field:  SerializeField] public bool PlayerHandle { get; private set; }
    [field:  SerializeField] public BotRoleInGame CurrentRole { get; private set; }
    
    private CancellationTokenSource _tokenSource;

    
    [Inject] private GameData _gameData;
    [Inject] private Bomb _bomb;
    
    
    private void Awake() {
        SetColliderEnableIfBot(false);
    }

    private void OnTriggerEnter(Collider collider) {
        if(IsInvincibleAfterBomb) return;
        Debug.Log($"Игроки столкнулись, PlayerHandle =  + {PlayerHandle}, Role = {CurrentRole}");
        // Если просто бродилка то никак не влияет на триггеры,
        if(CurrentRole == BotRoleInGame.Wanderer || CurrentRole == BotRoleInGame.Victim) return;

        if (collider.TryGetComponent(out PlayerRoleBehaviour player)) {
            Debug.Log("Охотник передал бомбу");
            SetColliderEnableIfBot(false);
            player.SetRole(BotRoleInGame.Hunter);
            SetRole(BotRoleInGame.Wanderer);
            StartInvinsibleTimer(_gameData.TimeToInvinsibleAfterPass).Forget();
        }
        else {
           Debug.Log("Контакт с " + collider); 
        }

    }
    
    
    public void GameStarted(bool started) {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        CurrentRole = BotRoleInGame.Wanderer;
        SetColliderEnableIfBot(started);
    }
    
   
    public void SetRole(BotRoleInGame role) {
        CurrentRole = role;
        
        UniTaskHelper.DisposeTask(ref _tokenSource);
        _tokenSource = new CancellationTokenSource();
        
        switch (role) {
            case BotRoleInGame.Hunter:
                _bomb.InitBombToNewPlayer(_pointToHoldBomb);
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
    
    
    
    private async UniTask StartHunting(CancellationToken token) {
        if(PlayerHandle) return;
        Debug.Log("Starting hunting...");
        // while (!token.IsCancellationRequested) {
        //     
        // }
    }
    
    
    private async UniTask Run(CancellationToken token) {
        if(PlayerHandle) return;
        Debug.Log("Starting running...");
    }
    
    
    private async UniTask WanderingInPlace(CancellationToken token) {
        if(PlayerHandle) return;
        Debug.Log("WanderingInPlace");
    }

    private async UniTask StartInvinsibleTimer(float time) {
        await UniTask.WaitForSeconds(time);
        SetColliderEnableIfBot(true);
    }

    private void SetColliderEnableIfBot(bool enable) {
        _collider.enabled = enable;
        IsInvincibleAfterBomb = !enable;
    }


    private void OnDestroy() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
    }
}