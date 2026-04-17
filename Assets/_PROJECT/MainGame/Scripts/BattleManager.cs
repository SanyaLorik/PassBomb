using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;


// Настройка положения игроков во время игры, поворот и тп
// Битва, смена ходов, уведомления о ходах которая будет слухать камера!


public class BattleManager : MonoBehaviour {
    // Будет выбираться рулеткой шо кинуть может
    
    // Прокидывать потом 
    [SerializeField] private List<Transform> _gameSpawnPoints;
    
    [Inject] private PlayerMovement _mainPlayerMovement;
    [Inject] private BotsMainManager _botsMainManager;

    
    public bool MainPlayerPlay { get; private set; }
    public bool AllowToPlay { get; private set; }
    
    public int CountPlayersToBattle => _gameSpawnPoints.Count;
    

    private readonly List<IPassBombPlayer> _players = new (8);
    
    public void InitForNewGame(bool mainPlayerPlay) {
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
        TeleportPlayersToPoints(_players, _gameSpawnPoints);
    }
    
    
    
    public void SetGameOverToBots() {
        
    }

    private void TeleportPlayersToPoints(List<IPassBombPlayer> players, List<Transform> points) {
        if (players.Count < points.Count) {
            Debug.LogWarning("Кол-во игроков < кол-ва точек спавна");
            return;
        } 
        int randomStartIndex = Random.Range(0, _gameSpawnPoints.Count);
        for (int i = 0; i < points.Count; i++) {
            int index = (i + randomStartIndex) % _gameSpawnPoints.Count;
            players[i].TeleportToPoint(points[index].position);
        }
    }
    
}