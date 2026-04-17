using Unity.AI.Navigation;
using UnityEngine;

public class MapItem : MonoBehaviour {
    [field: SerializeField] public Transform[] SpawnPoints;
    [field: SerializeField] public Transform BombCenterSpawn;
    [field: SerializeField] public NavMeshSurface Surface;
}