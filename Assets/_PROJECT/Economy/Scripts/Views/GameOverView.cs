using System;
using Architecture_M;
using SanyaBeerExtension;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameOverView : MonoBehaviour {
    [SerializeField] private GameObject _allContainer;
    [SerializeField] private GameObject _playgroundContainer;
    [Header("PVB")]
    [SerializeField] private GameObject _winContainer;
    [SerializeField] private GameObject _loseContainer;
    [Header("Buttons")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _continue2xButton;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private TextMeshProUGUI _doubleRewardText;
    
    public bool ResultWindowShowing { get; private set; }
    private int _roundMoney;
    
    [Inject] private MainGameStarter _gameStarter;
    [Inject] private BattleManager _battleManager;
    [Inject] private PlayerSkinInventory _skinInventory;
    [Inject] private BotsMainManager _botsMainManager;
    [Inject] private IGameSave _gameSave;
    [Inject] private AdvHelper _advHelper;
    [Inject] private PlayerBank _bank;
    [Inject] private EconomyCalculator _calculator;
    [Inject] private NumberFormatter _formatter;


    private void OnEnable() {
        _battleManager.MainPlayerWin += ShowResultWindow;
        
        _continueButton.onClick.AddListener(() => GetReward(false));
        _advHelper.AddToButtonAdvRewardListener(_continue2xButton, () => GetReward(true));
        
    }

    
    private void ShowResultWindow(bool mainPlayerWin) {
        ResultWindowShowing = true;
        
        _playgroundContainer.DisactiveSelf();
        _allContainer.ActiveSelf();
        
        _winContainer.SetActive(mainPlayerWin);
        _loseContainer.SetActive(!mainPlayerWin);
        
        _roundMoney = _calculator.CalculateGameReward();
        _rewardText.text = _formatter.ValuteFormatter(_roundMoney);
        _doubleRewardText.text = _formatter.ValuteFormatter(_roundMoney*2);
    }

    
    private void CloseResultWindow() {
        _playgroundContainer.ActiveSelf();
        ResultWindowShowing = false;
        _allContainer.DisactiveSelf();
    }
    

    private void GetReward(bool doubleReward) {
        if (!doubleReward) {
            _advHelper.ShowAdv();
        }
        _roundMoney = doubleReward ? _roundMoney*2 : _roundMoney;
        _bank.AddMoney(_roundMoney);
        _roundMoney = 0;
        CloseResultWindow();
    }
  
}