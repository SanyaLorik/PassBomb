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
    [field:  SerializeField] public bool IsInvincibleAfterBonus { get; private set; }
    [field:  SerializeField] public bool PlayerHandle { get; private set; }
    [field:  SerializeField] public BotRoleInGame CurrentRole { get; private set; }
    
    private CancellationTokenSource _tokenSource;
    
    // Для асинхронной передачи
    private static float _lastPassTime = -999f;
    private const float PASS_COOLDOWN = 0.5f;
    
    [Inject] private GameData _gameData;
    [Inject] private Bomb _bomb;
    
    
    private void Awake() {
        SetColliderEnable(false);
    }

    
    public void NewRoundStarted(bool started) {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        CurrentRole = BotRoleInGame.Wanderer;
        SetColliderEnable(started);
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
        SetColliderEnable(false);
        Debug.Log($"StartInvinsibleTimer, PlayerHandle = {PlayerHandle}");
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