using UnityEngine;
using Zenject;

public class GameOverTrigger : MonoBehaviour {
    [Inject] BattleManager _battleManager;
    
    
    private void OnTriggerEnter(Collider collider){
        if (collider.TryGetComponent(out IPassBombPlayer falledPlayer)) {
            _battleManager.PlayerFalled(falledPlayer);
        }
    }

    private void OnCollisionEnter(Collision collider) {
        if (collider.gameObject.TryGetComponent(out IPassBombPlayer falledPlayer)) {
            _battleManager.PlayerFalled(falledPlayer);
        }
    }
}