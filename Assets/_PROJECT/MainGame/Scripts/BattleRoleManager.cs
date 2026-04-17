using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class BattleRoleManager : MonoBehaviour {
    [Inject] private BattleManager _battleManager;

    public void StartNewGame() {
        BattleCycle().Forget();
    }

    private async UniTask BattleCycle() {
        
    }
        
}

[RequireComponent(typeof(BattleRoleManager))]
public class BombPassTrigger : MonoBehaviour {
    private MainGameRoleBehaviour _mainGameRoleBehaviour;

    private void Awake() {
        _mainGameRoleBehaviour = GetComponent<MainGameRoleBehaviour>();
    }

    private void OnTriggerEnter(Collider collider) {
        if (!collider.TryGetComponent(out MainGameRoleBehaviour roleBehaviour)) return;
            
        
    }
}
