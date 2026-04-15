using System;
using Architecture_M;
using MirraSDK_M;
using SanyaBeerExtension;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameOverShower : MonoBehaviour {
    [SerializeField] private GameObject _allContainer;
    [SerializeField] private GameObject _playgroundContainer;
    // PVP
    [Header("PVB")]
    [SerializeField] private GameObject _pvbContainer;
    [SerializeField] private GameObject _winContainer;
    [SerializeField] private GameObject _loseContainer;
    [Header("PVP")]
    [SerializeField] private GameObject _pvpContainer;
    
    [SerializeField] private TextMeshProUGUI _winnerNumberText;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _continue2xButton;

    public bool ResultWindowShowing { get; private set; }
    public event Action<bool> PlayerWin;

    [Inject] private MainGameStarter _gameStarter;
    [Inject] private BattleManager _battleManager;
    [Inject] private PlayerSkinInventory _skinInventory;
    [Inject] private BotsMainManager _botsMainManager;
    [Inject] private IGameSave _gameSave;
    [Inject] private CameraOrbitalController _camera;
    [Inject] private AdvertisingMonetizationMirra _advertisingMonetization;
    [Inject] private EconomyCalculator _economyCalculator;


    private void OnEnable() {
        _continueButton.onClick.AddListener(() => GetReward(false));
        _continue2xButton.onClick.AddListener(TryShowAdv);
    }

    private void HidePlayCanvas() {
        _playgroundContainer.DisactiveSelf();
    } 
    
    public void ShowResults() {
        ResultWindowShowing = true;
        MoveCameraToWinner();
        HidePlayCanvas();
        _allContainer.ActiveSelf();
        
    }

    
    private void SetPvbState(bool mainPlayerWin) {
        _pvbContainer.ActiveSelf();
        _pvpContainer.DisactiveSelf();
        
        _winContainer.SetActive(mainPlayerWin);
        _loseContainer.SetActive(!mainPlayerWin);
        
        PlayerWin?.Invoke(mainPlayerWin);
    }
    
    private void SetResultState(int winnerNumber) {
        _pvbContainer.DisactiveSelf();
        _pvpContainer.ActiveSelf();
        
        _winnerNumberText.text = winnerNumber.ToString();
    }
    
    private void MoveCameraToWinner() {
    }

    private void CloseResultWindow() {
        _playgroundContainer.ActiveSelf();
        ResultWindowShowing = false;
        _allContainer.DisactiveSelf();
    }

        
    
    private void TryShowAdv() {
        _advertisingMonetization.InvokeRewarded(
            null,
            (isSuccess) => 
            {
                if (isSuccess) {
                    GetReward(true);
                }
            }
        );
    }

    private void GetReward(bool doubleReward) {
        _economyCalculator.GetReward(doubleReward);
        CloseResultWindow();
    }

}
