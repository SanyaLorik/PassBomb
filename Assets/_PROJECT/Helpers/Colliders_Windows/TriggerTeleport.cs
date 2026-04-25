using System;
using UnityEngine;
using Zenject;

public class FallVoidCollider : MonoBehaviour {
    [SerializeField] private Transform _spawnPoint;
    
    public event Action<IPassBombPlayer> PlayerFalledInVoid; 
    
    [Inject] BattleManager _battleManager;
    
    
     private void OnTriggerEnter(Collider collider){
        if (collider.TryGetComponent(out IPassBombPlayer player)) {
            player.TeleportToPoint(_spawnPoint.position);
            if (player.IsPlaying) {
                _battleManager.PlayerFalled(player);
                PlayerFalledInVoid?.Invoke(player);
            }
        }
     }
}