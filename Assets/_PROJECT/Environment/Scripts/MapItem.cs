using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Serialization;

public class MapItem : MonoBehaviour {
    [field: SerializeField] public Transform[] SpawnPoints;
    [field: SerializeField] public Transform BombCenterSpawn;
    [field: SerializeField] public Transform Floor;
    [field: SerializeField] public float YToFind;
}