using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


public class BattleManager : MonoBehaviour {
    
    public bool MainPlayerPlay { get; private set; }
    public bool AllowToPlay { get; private set; }

    public int CountPlayersToBattle => _mapsToBattleChanger.CurrentMapSpawnPoints.Length;
    public Transform[] PlayersSpawnPoints => _mapsToBattleChanger.CurrentMapSpawnPoints;
    public Transform BombSpawnPoint => _mapsToBattleChanger.GetCurrentBombSpawn;

    
    public IReadOnlyCollection<IPassBombPlayer> Players => _players;
    
    private readonly List<IPassBombPlayer> _players = new(8);
    
    public event Action<string> PlayedDied;
    public event Action<int> PlayersCountChanged;
    public event Action GameReadyToPlay;
    
    
    [Inject] private PlayerMovement _mainPlayerMovement;
    [Inject] private BotsMainManager _botsMainManager;
    [Inject] private PlayersRoleManager _playersRoleManager;
    [Inject] private Bomb _bomb;
    [Inject] private BattleStartVisualizer _battleStartVisualizer;
    [Inject] private MainGameStarter _gameStarter;
    [Inject] private MapsToBattleChanger _mapsToBattleChanger;
    
    
    private void OnEnable() {
        _bomb.BombExploded += CheckPlayers;
    }
    
    
    private void OnDisable() {
        _bomb.BombExploded -= CheckPlayers;
    }
    
    
    public void InitForNewGame(bool mainPlayerPlay) {
        _mapsToBattleChanger.ChooseNextMap();
        _bomb.TeleportBombToSpawn(BombSpawnPoint);
        
        
        MainPlayerPlay = mainPlayerPlay;
        GetNewPlayers(MainPlayerPlay);
        InitPlayers();
    }


    private void GetNewPlayers(bool mainPlayerPlay) {
        int countBots = CountPlayersToBattle;
        if (mainPlayerPlay) {
            _players.Add(_mainPlayerMovement);
            countBots--;
        }
        IEnumerable<IPassBombPlayer> bots = _botsMainManager.GetBotsToGame(countBots);
        Debug.Log("Кол-во доп игроков: " + bots.Count());
        _players.AddRange(bots);
    }

    
    private void InitPlayers() {
        foreach (var player in _players) {
            player.SetPlayStatus(true);
        }
        TeleportPlayersToPoints(_players, PlayersSpawnPoints);
        PlayersCountChanged?.Invoke(_players.Count);
        GoBattleAsync().Forget();
    }

    private async UniTask GoBattleAsync() {
        RotatePlayersToBomb();
        await ShowStartAnimation();
        GameReadyToPlay?.Invoke();
        
        while (_players.Count > 1) {
            Debug.Log("Игроков: " + _players.Count);
            _playersRoleManager.InitNewPlayers(_players);
            _bomb.StartNewBombTimer();
            await UniTask.WaitUntil(() => _bomb.BombExplode);
            await ShowStartAnimation();
        }
        GameEnded();
    }

    private async UniTask ShowStartAnimation() {
        _bomb.TeleportBombToSpawn(BombSpawnPoint);
        
        await UniTask.Yield();
        
        EnablePlayersMove(false);
        _battleStartVisualizer.ShowAnimation();
        await UniTask.WaitWhile(() => _battleStartVisualizer.AnimationPlay);
        EnablePlayersMove(true);
    }

    private void EnablePlayersMove(bool enable) {
        _players.ForEach(p => p.SetMovingStatus(enable));
    }

    private void RotatePlayersToBomb() {
        _players.ForEach(p => p.RotateToTarget(BombSpawnPoint.position));
    }


    private void GameEnded() {
        Debug.Log("Игра кончилась");
        foreach (IPassBombPlayer player in _players) {
            player.RoleBehaviour.GameStarted(false);
            player.SetPlayStatus(false);
        }
        _players.Clear();
        _gameStarter.GameOver();
    }

    private void CheckPlayers() {
        foreach (IPassBombPlayer player in _players) {
            if (player.RoleBehaviour.CurrentRole == BotRoleInGame.Hunter) {
                Debug.Log("Игрок сдох!");
                if (player.RoleBehaviour.gameObject.TryGetComponent(out BotMonolog botMonolog)) {
                    PlayedDied?.Invoke(botMonolog.NickName);
                }
                player.RoleBehaviour.GameStarted(false);
                _players.Remove(player);
                player.SetPlayStatus(false);
                PlayersCountChanged?.Invoke(_players.Count);
                return;
            }
        }
        Debug.LogError("Игрок не сдох после взрыва бомбы, WTF");
    }
    
    
    private void TeleportPlayersToPoints(List<IPassBombPlayer> players, Transform[] points) {
        if (players.Count < points.Length) {
            Debug.LogWarning("Кол-во игроков < кол-ва точек спавна");
            return;
        } 
        int randomStartIndex = Random.Range(0, points.Length);
        for (int i = 0; i < points.Length; i++) {
            int index = (i + randomStartIndex) % points.Length;
            players[i].TeleportToPoint(points[index].position);
            players[i].RotateToTarget(BombSpawnPoint.position);
        }
    }
    
    
    
    public void SetGameOverToBots() {
        _playersRoleManager.SetGameOver(_players);
    }

    
    


}