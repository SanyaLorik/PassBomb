using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


public class PlayersRoleManager : MonoBehaviour {
    [Inject] private BattleManager _battleManager;
    [Inject] private Bomb _bomb;
    [Inject] private GameData _gameData;

    private IPassBombPlayer _currentHunter;
    
    
    private void OnEnable() {
        _bomb.PlayerBecameHunter += BombOnPlayerBecameHunter;
    }

    
    private void BombOnPlayerBecameHunter(PlayerRoleBehaviour playerRole) {
        if (_currentHunter != null) {
            _currentHunter.SetDefaultSpeed();
        }
        
        IPassBombPlayer player = _battleManager.Players
            .FirstOrDefault(p => p.RoleBehaviour == playerRole);
        
        if (player == null) {
            Debug.LogError("Игрок стал хантером, но не найден в системе");
            return;
        }
        
        _currentHunter = player;
        player.SetHunterSpeed();
    }
    
    

    public void InitNewPlayersToRound(IReadOnlyCollection<IPassBombPlayer> players) {
        foreach (var player in players) {
            player.RoleBehaviour.NewRoundStart(true);
            // player.SetDefaultRoundSpeed();
        }
        
        // Назначение роли у типочка
        int randomPlayer = Random.Range(0, players.Count);
        players.ElementAt(randomPlayer).RoleBehaviour.SetRole(PlayerRoleInGame.Hunter);
        // Debug.Log("назначение охотника");
    }

    
    public void SetGameOver(IReadOnlyCollection<IPassBombPlayer> players) {
        foreach (var player in players) {
            player.RoleBehaviour.NewRoundStart(false);
            player.SetDefaultSpeed();
        }
    }
    
}

