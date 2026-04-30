using System.Collections.Generic;
using SanyaBeerExtension;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class MapsToBattleChanger : MonoBehaviour {
    [field: SerializeField] public Transform CentralTeleport { get; private set; }
    [SerializeField] private List<MapItem> _mapitems;
    [Header("Ставить 0 и карту ДЛЯ ТУТОРА первой!")]
    [SerializeField] private int _tutorialMapIndex;

    private int MapIndex { get; set; }

    public Transform[] CurrentMapSpawnPoints => _mapitems[MapIndex].SpawnPoints;
    public Transform GetCurrentBombSpawn => _mapitems[MapIndex].BombCenterSpawn;
    public Transform GetCurrentMapFloor => _mapitems[MapIndex].Floor;
    public float GetCurrentMapHeight => _mapitems[MapIndex].YToFind;
    public float FallBotFindSamplePosition => _mapitems[MapIndex].FallBotFindSamplePosition;
    
    
    
    [Inject] private MainGameStarter _mainGameStarter;
    [Inject] private TutorialManager _tutorialManager;

    
    
    private void OnEnable() {
        _mainGameStarter.GameStarted += OnGameStarted;
    }
    
    
    private void Start() {
        if (_tutorialManager.TutorialPassed) {
            _mapitems[_tutorialMapIndex].DisactiveSelf();
            _mapitems.RemoveAt(_tutorialMapIndex);
        }
        MapIndex = Random.Range(0, _mapitems.Count);
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
                MapIndex = 1;
            }
        }
        else {
            MapIndex = _tutorialMapIndex;
        }
        
        
        _mapitems.ForEach(m => m.DisactiveSelf());
        _mapitems[MapIndex].gameObject.ActiveSelf();
    }

}