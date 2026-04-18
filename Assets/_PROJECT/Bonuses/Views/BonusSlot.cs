using System.Threading;
using _PROJECT.Scripts.Helpers;
using Architecture_M;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


public class BonusSlot : MonoBehaviour {
    [field: SerializeField] public BonusItemConfig BonusItem { get; private set; }
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private TextMeshProUGUI _bonusNameText;
    [SerializeField] private Image _reloadProgress;
    [SerializeField] private Button _button;


    public bool IsAvailable { get; private set; }
    public IBonus Bonus => BonusItem.Bonus;
    public int BonusCount => Saves.GetBonusCount(BonusItem.Id);
    private GameSave Saves => _saver.GetSave<GameSave>();
    
    private CancellationTokenSource _tokenSource;
    
    
    [Inject] private BonusManager _bonusManager;
    [Inject] private DiContainer _diContainer;
    [Inject] private IGameSave _saver; 
    [Inject] LocalizationData _localization;
    [Inject] IPassBombPlayer _mainPlayer;
    [Inject] GameData _gameData;
    
    
    [Inject]
    private void Init() {
        _diContainer.QueueForInject(Bonus);
    }
    

    private void OnEnable() {
        CheckAvailable();
        _button.onClick.AddListener(TryUse);
    }

    
    private void Start() {
        CheckAvailable();
        _bonusNameText.text =
            _localization.GetTranslatedText(BonusItem, _localization.BonusesTranslates);
    }


    public void TryUse() {
        if (!IsAvailable) {
            Debug.Log("Бонус на перезарядке именно что");
            return;
        }

        if (BonusCount == 0) {
            Debug.Log("Бонусов нема");
            return;
        }
        _bonusManager.UseBonusByClick(BonusItem.Bonus, this);
    }

    public void UseBonusAfterCheck() {
        Bonus.Use(_mainPlayer);
        GetOneBonus(true);
        
        UniTaskHelper.DisposeTask(ref _tokenSource);
        _tokenSource = new  CancellationTokenSource();
        DisableBonusTimerAsync(_tokenSource.Token).Forget();
    }
    
    
    public void StopBonusWork() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        CheckAvailable();
        Bonus.StopWork(_mainPlayer);
    }

    private async UniTask DisableBonusTimerAsync(CancellationToken token) {
        float duration = _gameData.BonusDuration;
        float elapsedTime = _gameData.BonusDuration;
        while (elapsedTime > 0 && !token.IsCancellationRequested) {
            _reloadProgress.fillAmount = elapsedTime/duration;
            
            elapsedTime -= Time.deltaTime;
            await UniTask.Yield(cancellationToken: token);
        }
        StopBonusWork();
    }

    
    public void CheckAvailable() {
        _countText.text = BonusCount.ToString();
        if (BonusCount == 0) {
            IsAvailable = false;
            _reloadProgress.fillAmount = 1f;
        }
        else {
            IsAvailable = true;
            _reloadProgress.fillAmount = 0f;
        }
    }


    
    public void GetOneBonus(bool useSaves = false) {
        if (BonusCount != 0) {
            if (useSaves) {
                _saver.GetSave<GameSave>().SetMinusOneBonus(BonusItem.Id);
                _saver.Save();
            }
        }
        CheckAvailable();
    }


    private void OnDisable() {
        StopBonusWork();
    }
}
