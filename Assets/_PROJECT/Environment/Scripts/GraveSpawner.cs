using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;


public class GraveSpawner : MonoBehaviour {
    [SerializeField] private GameObject _gravePrefab;

    [Inject] private BattleManager _battleManager;  
    [Inject] private MainGameStarter _gameStarter;
    [Inject] private SpawnManager _spawnManager;

    private readonly List<GameObject> _gravesInstances = new();
    
    private void OnEnable() {
        _battleManager.PlayerDied += OnPlayerDied;
        _gameStarter.GameStarted += OnGameStarted;
    }

    private void OnGameStarted(bool started) {
        RemoveAllGraves();
    }


    private void OnPlayerDiedOld(string playerName, Vector3 position) {
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit)) {
            GameObject grave = Instantiate(_gravePrefab, hit.point, Quaternion.identity);
            _gravesInstances.Add(grave);
        }
    }
    
    private void OnPlayerDied(string _, Vector3 position) {
        SpawnGraveAsync(position).Forget();
    }

    
    private async UniTask SpawnGraveAsync(Vector3 position) {
        await UniTask.DelayFrame(5);
        Vector3 rayStart = position + Vector3.up * 10f;
    
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 100f)) {
            GameObject grave = Instantiate(_gravePrefab, hit.point, Quaternion.identity);
            _gravesInstances.Add(grave);
        }
        else {
            // Фолбэк — если совсем не попали, ставим на оригинальную позицию но чуть выше
            rayStart = _spawnManager.SpawnPoint.position + Vector3.up * 10f;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 100f)) {
                GameObject grave = Instantiate(_gravePrefab, hit.point, Quaternion.identity);
                _gravesInstances.Add(grave);
                Debug.LogWarning($"GraveSpawner: рейкаст не попал, ставим фолбэк на спавн {rayStart}");
            }
            else {
                Debug.LogWarning($"GraveSpawner: рейкаст не попал никуда");
            }
           
        }
    }
    
    
    private void RemoveAllGraves() {
        _gravesInstances.ForEach(Destroy);
        _gravesInstances.Clear();
    } 

        
        
    
}