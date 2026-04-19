using SanyaBeerExtension;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MapsToBattleChanger : MonoBehaviour {
    [field: SerializeField] public Transform CentralTeleport { get; private set; }
    [SerializeField] private MapItem[] _mapitems;

    public int MapIndex { get; private set; }

    private void Start() {
        MapIndex = Random.Range(0, _mapitems.Length);
    }

    public void ChooseNextMap() {
        MapIndex++;
        if (MapIndex >= _mapitems.Length-1) {
            MapIndex = 0;
        }
        _mapitems.ForEach(m => m.DisactiveSelf());
        _mapitems[MapIndex].gameObject.ActiveSelf();
    }

    public Transform[] CurrentMapSpawnPoints => _mapitems[MapIndex].SpawnPoints;
    public Transform GetCurrentBombSpawn => _mapitems[MapIndex].BombCenterSpawn;
    public Transform GetCurrentMapFloor => _mapitems[MapIndex].Floor;
}