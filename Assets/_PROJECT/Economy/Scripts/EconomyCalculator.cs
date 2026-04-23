using TMPro;
using UnityEngine;
using Zenject;


public class EconomyCalculator : MonoBehaviour {
    [Header("Economy")]
    [SerializeField] public int _minimalReward;
    [SerializeField] private float _ratioForGame;
    [SerializeField] private int _curveRatio;
    [SerializeField] private AnimationCurve _animationCurve;

    
    [Inject] BattleManager _battleManager;
    
    
    public int CalculateGameReward() {
        float roundProgress = (float)_battleManager.RoundNumber / _battleManager.AllRoundsCount;
        int reward = (int)(
            _ratioForGame * roundProgress 
            + 
            _curveRatio * _animationCurve.Evaluate(roundProgress)
        );
        return reward;
    }
    
    

    
    
}
