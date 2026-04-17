using UnityEngine;
using Zenject;

public class TeleporterTrigger : MonoBehaviour {
    [Inject] private MapsToBattleChanger _mapsToBattleChanger;
    
    private void OnTriggerEnter(Collider collider) {
        if (collider.TryGetComponent(out IPassBombPlayer passBombPlayer)) return;
        passBombPlayer.TeleportToPoint(_mapsToBattleChanger.CentralTeleport.position);
    }
}