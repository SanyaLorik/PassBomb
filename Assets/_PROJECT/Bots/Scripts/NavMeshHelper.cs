using UnityEngine;
using UnityEngine.AI;

public class NavMeshHelper {
    private readonly GameData _gameData;

    
    public NavMeshHelper(GameData gameData) {
        _gameData = gameData;
    }
    
    
    public Vector3 CalculateBotTargetPoint(Transform point, float yToFind) {
        Vector3 size = point.localScale;
        float offsetX = Random.Range(-size.x/2f, size.x/2f);
        float offsetZ = Random.Range(-size.z/2f, size.z/2f);

        Vector3 target = point.position + new Vector3(offsetX, yToFind, offsetZ);

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, _gameData.DistanceToFloor, NavMesh.AllAreas)) {
            return hit.position;
        }

        return point.position;
    }
}