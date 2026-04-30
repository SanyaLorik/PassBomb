using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


public class PlayersRoleManager : MonoBehaviour {
    [Inject] private BattleManager _battleManager;
    [Inject] private Bomb _bomb;
    [Inject] private GameData _gameData;
    [Inject] private TutorialManager _tutorialManager;
    [Inject] private IPassBombPlayer _mainPlayer;


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
        if (!_tutorialManager.TutorialPassed) {
            if (_tutorialManager.InitBombToMainPlayer) {
                InitBombToMainPlayer();
            }
            else {
                InitBombToBot(players);
            }
        }
        else {
            InitBombToRandomPlayer(players);
        }
    }

    private static void InitBombToRandomPlayer(IReadOnlyCollection<IPassBombPlayer> players) {
        int randomPlayer = Random.Range(0, players.Count);
        players.ElementAt(randomPlayer).RoleBehaviour.SetRole(PlayerRoleInGame.Hunter);
    }

    private void InitBombToMainPlayer() {
        _mainPlayer.RoleBehaviour.SetRole(PlayerRoleInGame.Hunter);
    }

    private void InitBombToBot(IReadOnlyCollection<IPassBombPlayer> players) {
        foreach (IPassBombPlayer player in players) {
            if (player != _mainPlayer) {
                player.RoleBehaviour.SetRole(PlayerRoleInGame.Hunter);
                return;
            }
        }
        Debug.LogError("Игрок для роли хантера не найден");
    }


    public void SetGameOver(IReadOnlyCollection<IPassBombPlayer> players) {
        foreach (var player in players) {
            player.RoleBehaviour.NewRoundStart(false);
            player.SetDefaultSpeed();
        }
    }
    
}

