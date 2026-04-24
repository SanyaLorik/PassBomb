using UnityEngine;
using Zenject;

public class GameOverTrigger : MonoBehaviour {
    [Inject] BattleManager _battleManager;
    [Inject] IPassBombPlayer _mainPlayer;
    
    
    private void OnTriggerEnter(Collider collider){
        if (collider.TryGetComponent(out IPassBombPlayer falledPlayer)) {
            if (falledPlayer == _mainPlayer) {
                _battleManager.SetLooseMainPlayer();
            }
            else {
                _battleManager.SetGameOverToBot(falledPlayer);
            }
        }
    }

    private void OnCollisionEnter(Collision collider) {
        if (collider.gameObject.TryGetComponent(out IPassBombPlayer falledPlayer)) {
            if (falledPlayer == _mainPlayer) {
                _battleManager.SetLooseMainPlayer();
            }
            else {
                _battleManager.SetGameOverToBot(falledPlayer);
            }
        }
    }
}