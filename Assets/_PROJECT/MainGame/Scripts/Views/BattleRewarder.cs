using UnityEngine;
using Zenject;

public class BattleRewarder : MonoBehaviour {
    [Header("Множитель награды за раунд")]
    
    
    
    [Inject] private BattleManager _battleManager;
    [Inject] private PlayerBank _bank;


    private void OnEnable() {
        // _battleManager.On
    }

    
    
    
}