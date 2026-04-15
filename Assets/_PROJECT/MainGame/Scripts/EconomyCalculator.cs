using TMPro;
using UnityEngine;
using Zenject;


public class EconomyCalculator : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private TextMeshProUGUI _doubleRewardText;
    [SerializeField] public int _minimalReward;
    
    
    [SerializeField] private float _ratioForWin;
    [SerializeField] private float _ratioForLoose;
    
    

    private float _accumulateBodyPercentage;
    private int _rewardMoney;
    
    
    [Inject] PlayerBank _bank;
    [Inject] BattleManager _battleManager;
    [Inject] MainGameStarter _gameStarter;
    [Inject] GameOverShower _gameOverShower;
    [Inject] GameData _gameData;
    [Inject] NumberFormatter _formatter;
    
    
    private void OnEnable() {
        _gameStarter.GameStarted += GameStarterOnGameStarted;
        _gameOverShower.PlayerWin += SetResult;
        
    }

    private void SetResult(bool playerWin) {
       
    }

    private void GameStarterOnGameStarted(bool started) {

    }
    

    
    
    public void GetReward(bool doubleReward) {
        int reward = doubleReward ? _rewardMoney * 2 : _rewardMoney;
        _bank.AddMoney(reward);
        _rewardMoney = 0;
    }
    
    
}
