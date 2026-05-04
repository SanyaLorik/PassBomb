using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SanyaBeerExtension;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class MapsToBattleChanger : MonoBehaviour {
    [field: SerializeField] public Transform CentralTeleport { get; private set; }
    [SerializeField] private List<MapItem> _mapitems;
    [Header("Ставить 0 и карту Tutorial первой!")]
    [SerializeField] private int _tutorialMapIndex;

    private int MapIndex { get; set; }

    public Transform[] CurrentMapSpawnPoints => _mapitems[MapIndex].SpawnPoints;
    public Transform GetCurrentBombSpawn => _mapitems[MapIndex].BombCenterSpawn;
    public Transform GetCurrentMapFloor => _mapitems[MapIndex].Floor;
    public float GetCurrentMapHeight => _mapitems[MapIndex].YToFind;
    public float FallBotFindSamplePosition => _mapitems[MapIndex].FallBotFindSamplePosition;
    
    
    
    [Inject] private MainGameStarter _mainGameStarter;
    [Inject] private TutorialManager _tutorialManager;
    [Inject] private PlayerMovement _playerMovement;

    
    
    private void OnEnable() {
        _mainGameStarter.GameStarted += OnGameStarted;
        _tutorialManager.TutorialStarted += OnTutorialStarted;
    }

    private void OnTutorialStarted(bool started) {
        if (!started) {
            RemoveTutorMapAsync().Forget();
        }
    }


    private void Start() {
        TryToRemoveTutorialMap();
        MapIndex = Random.Range(0, _mapitems.Count);
    }
    
    
    private async UniTask RemoveTutorMapAsync() {
        await UniTask.WaitUntil(() => _playerMovement.PlayerInSpawn);
        TryToRemoveTutorialMap();
    }

    
    private void TryToRemoveTutorialMap() {
        if (_tutorialManager.TutorialPassed) {
            _mapitems[_tutorialMapIndex].DisactiveSelf();
            _mapitems.RemoveAt(_tutorialMapIndex);
        }
    }


    private void OnGameStarted(bool started) {
        if (started) {
            ChooseNextMap();
        }
    }

    
    private void ChooseNextMap() {
        if (_tutorialManager.TutorialPassed) {
            MapIndex++;
            if (MapIndex > _mapitems.Count-1) {
                MapIndex = 0;
            }
        }
        else {
            MapIndex = _tutorialMapIndex;
        }
        
        
        _mapitems.ForEach(m => m.DisactiveSelf());
        _mapitems[MapIndex].gameObject.ActiveSelf();
    }

}