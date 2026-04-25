using UnityEngine;

public class MapItem : MonoBehaviour {
    [field: SerializeField] public Transform[] SpawnPoints;
    [field: SerializeField] public Transform BombCenterSpawn;
    [field: SerializeField] public Transform Floor;
    [field: SerializeField] public float YToFind;
    [field: SerializeField] public float FallBotFindSamplePosition;
}