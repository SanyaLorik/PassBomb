using TMPro;
using UnityEngine;
using Zenject;


public class EconomyCalculator : MonoBehaviour {
    [Header("Экономкика конца игры")]
    [SerializeField] public int _minimalReward;
    [SerializeField] private float _ratioForGame;
    [SerializeField] private int _curveRatio;
    [SerializeField] private AnimationCurve _animationCurve;

    [Header("Экономкика конца игры")] 
    [SerializeField] private int _pushToVoidReward;

    
    [Inject] BattleManager _battleManager;
    
    
    public int CalculateGameReward() {
        float roundProgress = (float)_battleManager.RoundNumber / _battleManager.AllRoundsCount;
        int reward = (int)(
            _ratioForGame * roundProgress 
            + 
            _curveRatio * _animationCurve.Evaluate(roundProgress)
        );
        return Mathf.Max(reward, _minimalReward);
    }

    public int CalcRewardToFall() {
        return _pushToVoidReward;
    }
    
    

    
    
}
