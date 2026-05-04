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
        
        if (player == null || !player.IsPlaying) {
            Debug.LogWarning("Игрок стал хантером, но не найден в системе чи мёртв");
            return;
        }
        
        _currentHunter = player;
        Debug.Log("SetHunterSpeed in BombOnPlayerBecameHunter");
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

    private void InitBombToRandomPlayer(IReadOnlyCollection<IPassBombPlayer> players) {
        if (Random.value < _gameData.ChanceInitBombToPlayer && _mainPlayer.IsPlaying) {
            InitBombToMainPlayer();
        }
        else {
            int randomPlayer = Random.Range(0, players.Count);
            players.ElementAt(randomPlayer).RoleBehaviour.SetRole(PlayerRoleInGame.Hunter);
        }
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

