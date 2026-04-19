using UnityEngine;
using Zenject;

public class BattleRewarder : MonoBehaviour {
    [Header("Множитель награды за раунд")]
    [SerializeField] private float _baseReward;
    [SerializeField] private AnimationCurve _rewardCurve;
    [SerializeField] private float _curveMultiplier;
    
    
    [Inject] private BattleManager _battleManager;
    [Inject] private PlayerBank _bank;


    private void OnEnable() {
        _battleManager.NewRoundStarted += BattleManagerOnNewRoundStarted;
    }

    private void BattleManagerOnNewRoundStarted(int number) {
        if(!_battleManager.MainPlayerPlay || number == 1) return;
        float roundProgress = (float) number / _battleManager.AllRoundsCount;
        float roundReward = _baseReward * number + _rewardCurve.Evaluate(roundProgress) * _curveMultiplier;
        Debug.Log("roundReward = " + roundReward);
        _bank.AddMoney((int)roundReward);
        
    }
}