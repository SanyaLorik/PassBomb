using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


public class BattleManager : MonoBehaviour {
    
    public bool MainPlayerPlay { get; private set; }
    public bool PlayerReturnToSpawn => _mainPlayerMovement.PlayerInSpawn;

    public int CountPlayersToNewBattle => _mapsToBattleChanger.CurrentMapSpawnPoints.Length;
    public int AllRoundsCount => CountPlayersToNewBattle - 1;

    public int RoundNumber { get; private set; }

    public Transform[] PlayersSpawnPoints => _mapsToBattleChanger.CurrentMapSpawnPoints;
    public Transform BombSpawnPoint => _mapsToBattleChanger.GetCurrentBombSpawn;

    
    public IReadOnlyCollection<IPassBombPlayer> Players => _players;
    
    private readonly List<IPassBombPlayer> _players = new(8);
    
    public event Action<string> PlayerDied;
    public event Action<int> PlayersCountChanged;
    public event Action<int> NewRoundStarted;
    public event Action GameReadyToPlay;
    public event Action<bool> MainPlayerWin;
    public event Action ForceStartedNewGame;

    private CancellationTokenSource _tokenSource;
    private int PlayersCount => _players.Count;
    
    
    [Inject] private PlayerMovement _mainPlayerMovement;
    [Inject] private BotsMainManager _botsMainManager;
    [Inject] private Bomb _bomb;
    [Inject] private BattleStartVisualizer _battleStartVisualizer;
    [Inject] private MainGameStarter _gameStarter;
    [Inject] private GameData _gameData;
    [Inject] private MapsToBattleChanger _mapsToBattleChanger;
    [Inject] private PlayersRoleManager _playersRoleManager;
    [Inject] private IPassBombPlayer _mainPlayer;
    [Inject] private GameOverView _gameOverView;
    
    
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

    
    public void ForceEndNewGame() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        ForceStartedNewGame?.Invoke();
        GameEnded(false);
    }


    public void PlayerFalled(IPassBombPlayer passBombPlayer) {
        if (passBombPlayer.RoleBehaviour.CurrentRole == PlayerRoleInGame.Hunter) {
            _bomb.ExlodeBombLater();
            return;
        }
        
        if (passBombPlayer == _mainPlayer) {
            SetLooseMainPlayer();
        }
        else {
            SetLooseBot(passBombPlayer);
        }
    }
    
    private void SetLooseMainPlayer() {
        if(!MainPlayerPlay) return;
        MainPlayerPlay = false;
        MainPlayerWin?.Invoke(false);
        RemovePlayer(_mainPlayer);
        WaitPlayerPressGameOverAsync().Forget();
        Debug.Log("Вы выбыли из игры");
    }

    
    private void SetLooseBot(IPassBombPlayer player) {
        BotMonolog botMonolog = player.RoleBehaviour.gameObject.GetComponentInParent<BotMonolog>();
        if (botMonolog != null) {
            PlayerDied?.Invoke(botMonolog.NickName);
            Debug.Log($"{botMonolog.NickName} проиграл");
            player.SetPlayStatus(false);
            RemovePlayer(player);
        }
    }
    
    
    private void GetNewPlayers(bool mainPlayerPlay) {
        int countBots = CountPlayersToNewBattle;
        if (mainPlayerPlay) {
            _players.Add(_mainPlayerMovement);
            countBots--;
        }
        IEnumerable<IPassBombPlayer> bots = _botsMainManager.GetBotsToGame(countBots);
        // Debug.Log("Кол-во доп игроков: " + bots.Count());
        _players.AddRange(bots);
    }

    
    private void InitPlayers() {
        foreach (var player in _players) {
            player.SetPlayStatus(true);
            player.RoleBehaviour.SetInvinsibleAfterBomb(false);
        }
        TeleportPlayersToPoints(_players, PlayersSpawnPoints);
        PlayersCountChanged?.Invoke(_players.Count);
        _tokenSource = new CancellationTokenSource();
        GoBattleAsync(_tokenSource.Token).Forget();
    }

    
    private async UniTask GoBattleAsync(CancellationToken token) {
        RotatePlayersToBomb();
        await ShowStartAnimation(true, token);
        GameReadyToPlay?.Invoke();

        RoundNumber = 1;
        while (!token.IsCancellationRequested && PlayersCount > 1) {
            NewRoundStarted?.Invoke(RoundNumber);
            
            _playersRoleManager.InitNewPlayersToRound(_players);
            _bomb.StartNewBombTimer();
            
            await UniTask.WaitUntil(() => _bomb.BombExplode || PlayersCount == 1, cancellationToken: token);
            
            await UniTask.WaitForSeconds(_gameData.TimeAfterBombExplode, cancellationToken: token);
            
            _bomb.TeleportBombToSpawn(BombSpawnPoint);
            if (PlayersCount != 1) {
                await ShowStartAnimation(false, token);
            }
            RoundNumber++;
        }
        
        if (MainPlayerPlay) {
            MainPlayerWin?.Invoke(true);
            await WaitPlayerPressGameOverAsync();
        }
        GameEnded();
    }

    
    
    private async UniTask ShowStartAnimation(bool forbidMove, CancellationToken token) {
        await UniTask.Yield();
        
        if(forbidMove) EnablePlayersMove(false);
        _battleStartVisualizer.ShowAnimation(forbidMove);
        await UniTask.WaitWhile(() => _battleStartVisualizer.AnimationPlay, cancellationToken: token);
        if(forbidMove) EnablePlayersMove(true);
    }

    
    private async UniTask WaitPlayerPressGameOverAsync() {
        _mainPlayerMovement.SetMovingStatus(false);
        await UniTask.WaitWhile(() => _gameOverView.ResultWindowShowing);
        _mainPlayer.SetPlayStatus(false);
        _mainPlayerMovement.SetMovingStatus(true);
    }


    private void GameEnded(bool setGameOver = true) {
        Debug.Log("Игра кончилась");
        foreach (IPassBombPlayer player in _players) {
            player.RoleBehaviour.NewRoundStart(false);
            player.SetPlayStatus(false);
        }
        _players.Clear();
        
        if (setGameOver) {
            _gameStarter.GameOver();
        }
    }

    
    private void CheckPlayers() {
        foreach (IPassBombPlayer player in _players) {
            if (player.RoleBehaviour.CurrentRole == PlayerRoleInGame.Hunter) {
                Debug.Log("Игрок выбыл!");
                player.RoleBehaviour.SetInvinsibleAfterBomb(true);
                BotMonolog botMonolog = player.RoleBehaviour.gameObject.GetComponentInParent<BotMonolog>();
                if (botMonolog != null) {
                    PlayerDied?.Invoke(botMonolog.NickName);
                    player.SetPlayStatus(false);
                    RemovePlayer(player);
                }
                else if (player == _mainPlayer) {
                    SetLooseMainPlayer();
                }
                return;
            }
        }
        Debug.LogError("Игрок не сдох после взрыва бомбы, WTF");
    }

    private void RemovePlayer(IPassBombPlayer player) {
        player.RoleBehaviour.NewRoundStart(false);
        _players.Remove(player);
        PlayersCountChanged?.Invoke(PlayersCount);
        Debug.Log("Игроков: " + PlayersCount);
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
    
    private void EnablePlayersMove(bool enable) {
        _players.ForEach(p => p.SetMovingStatus(enable));
    }

    
    private void RotatePlayersToBomb() {
        _players.ForEach(p => p.RotateToTarget(BombSpawnPoint.position));
    }
    
    
    public void SetGameOverToBots() {
        _playersRoleManager.SetGameOver(_players);
    }
}