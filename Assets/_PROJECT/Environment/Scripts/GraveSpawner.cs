using System.Collections.Generic;
using UnityEngine;
using Zenject;


public class GraveSpawner : MonoBehaviour {
    [SerializeField] private GameObject _gravePrefab;

    [Inject] private BattleManager _battleManager;  
    [Inject] private MainGameStarter _gameStarter;

    private readonly List<GameObject> _gravesInstances = new();
    
    private void OnEnable() {
        _battleManager.PlayerDied += OnPlayerDied;
        _gameStarter.GameStarted += OnGameStarted;
    }

    private void OnGameStarted(bool started) {
        RemoveAllGraves();
    }


    private void OnPlayerDied(string playerName, Vector3 position) {
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit)) {
            GameObject grave = Instantiate(_gravePrefab, hit.point, Quaternion.identity);
            _gravesInstances.Add(grave);
        }
    }
    
    
    private void RemoveAllGraves() {
        _gravesInstances.ForEach(Destroy);
        _gravesInstances.Clear();
    } 

        
        
    
}