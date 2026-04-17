using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum BotRoleInGame {
    Hunter, 
    Victim,
    Wanderer
}


/// <summary>
/// Класс получает инфу об охотнике и жертве, а также карте по которой он носится
/// </summary>
public class MainGameRoleBehaviour : MonoBehaviour {
    [field: SerializeField] public bool PlayerHandle { get; private set; }
    private CancellationTokenSource _tokenSource;

    public BotRoleInGame CurrentRole { get; private set; }

    public void StopDoingRole() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
    }

    
    public void StartDoingRole(BotRoleInGame role) {
        CurrentRole = role;
        if(PlayerHandle) return;
        
        UniTaskHelper.DisposeTask(ref _tokenSource);
        _tokenSource = new CancellationTokenSource();
        
        switch (role) {
            case BotRoleInGame.Hunter: 
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
        Debug.Log("Starting hunting...");
        while (!token.IsCancellationRequested) {
            
        }
    }
    
    
    private async UniTask Run(CancellationToken token) {
        Debug.Log("Starting running...");
    }
    
    
    private async UniTask WanderingInPlace(CancellationToken token) {
        Debug.Log("WanderingInPlace");
    }
}