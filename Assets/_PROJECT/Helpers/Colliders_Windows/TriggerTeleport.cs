using UnityEngine;

public class ColliderTeleport : MonoBehaviour {
    [SerializeField] private Transform _target;
    
     private void OnTriggerEnter(Collider collider){
        if (collider.TryGetComponent(out IPassBombPlayer player)) {
            player.TeleportToPoint(_target.position);
        }
     }
}