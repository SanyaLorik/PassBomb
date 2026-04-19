using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class BotBonusMonitorer : MonoBehaviour {
    [SerializeField] private IPassBombPlayer _botPlayer;
    private CancellationTokenSource _botTokenSource;
    
    [Inject] GameData _gameData;
    
    
    private async UniTask MonitorBonusAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            await UniTask.WaitForSeconds(RandomTiming, cancellationToken: token);
            if (_botPlayer.RoleBehaviour.CurrentRole != BotRoleInGame.Hunter) {
                // USE BONUS
            }
        }
    }

    private float RandomTiming =>
         Random.Range(_gameData.BotBonusUseCheckDiapasone.From, _gameData.BotBonusUseCheckDiapasone.To);
    
    
}
