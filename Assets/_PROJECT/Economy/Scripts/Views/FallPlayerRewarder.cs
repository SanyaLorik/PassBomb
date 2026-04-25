using UnityEngine;
using Zenject;

public class FallPlayerRewarder : MonoBehaviour {
    [Inject] private FallVoidCollider _fallVoidCollider;
    [Inject] private IPassBombPlayer _mainPlayer;
    [Inject] private PlayerBank _playerBank;
    [Inject] private EconomyCalculator _economyCalculator;


    private void OnEnable() {
        _fallVoidCollider.PlayerFalledInVoid += FallVoidColliderOnPlayerFalledInVoid;
    }

    private void FallVoidColliderOnPlayerFalledInVoid(IPassBombPlayer faller) {
        if(faller == _mainPlayer) return; // Соболезнуем
        
        if (faller.RoleBehaviour.LastPlayerContact == _mainPlayer.RoleBehaviour) {
            RewardPlayerToKillInnocentBot();
        }
    }

    private void RewardPlayerToKillInnocentBot() {
        _playerBank.AddMoney(_economyCalculator.CalcRewardToFall());
    }
}