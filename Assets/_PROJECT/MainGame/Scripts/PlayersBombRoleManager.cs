using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


public class PlayersBombRoleManager : MonoBehaviour {
    [Inject] private BattleManager _battleManager;
    [Inject] private Bomb _bomb;
    

    public void InitNewPlayers(IReadOnlyCollection<IPassBombPlayer> players) {
        foreach (var player in players) {
            player.RoleBehaviour.GameStarted(true);
            player.SetDefaultSpeed();
        }
        
        // Назначение роли у типочка
        int randomPlayer = Random.Range(0, players.Count);
        players.ElementAt(randomPlayer).RoleBehaviour.SetRole(BotRoleInGame.Hunter);
        _bomb.StartNewBombTimer();
        Debug.Log("назначение охотника");
    }

    public void SetGameOver(IReadOnlyCollection<IPassBombPlayer> players) {
        foreach (var player in players) {
            player.RoleBehaviour.GameStarted(false);
            player.SetDefaultSpeed();
        }
    }
    
}

