using System;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Architecture_M;
using Cysharp.Threading.Tasks;
using SanyaBeerExtension;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

[Serializable]
public enum KeyBoardKey {
    One,
    Two,
    Three
}


public class BonusSlot : MonoBehaviour {
    [field: SerializeField] public BonusItemConfig BonusItem { get; private set; }
    [field: SerializeField] public KeyBoardKey KeyBoardKey { get; private set; }
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private TextMeshProUGUI _bonusNameText;
    [SerializeField] private Image _reloadProgress;
    [Header("Время использования")]
    [SerializeField] private GameObject _useContainer;
    [SerializeField] private RectTransform _useTimeProgress;
    [SerializeField] private Button _button;
    [Header("Выключать на мобил.")]
    [SerializeField] private GameObject _desktopObject;
    
    
    public bool IsAvailable { get; private set; }
    public IBonus Bonus => BonusItem.Bonus;
    public int BonusCount => Saves.GetBonusCount(BonusItem.Id);
    
    private GameSave Saves => _saver.GetSave<GameSave>();
    private CancellationTokenSource _tokenSource;
    private Vector2 _startOffset;
    
    [Inject] private BonusManager _bonusManager;
    [Inject] private DiContainer _diContainer;
    [Inject] private IGameSave _saver; 
    [Inject] LocalizationData _localization;
    [Inject] IPassBombPlayer _mainPlayer;
    [Inject] GameData _gameData;
    [Inject] IDeviceTypeProvider _deviceTypeProvider;
    
    [Inject]
    private void Init() {
        _diContainer.QueueForInject(Bonus);
    }
    

    private void OnEnable() {
        CheckAvailable();
        _button.onClick.AddListener(TryUse);
    }

    
    
    private void Start() {
        _mainPlayer.RoleBehaviour.PlayerRoleChanged += HideShowElementsByRole;
        
        
        _startOffset = _useTimeProgress.offsetMax;
        CheckAvailable();
        SetProgressBarVisible(false);
        _bonusNameText.text =
            _localization.GetTranslatedText(BonusItem, _localization.BonusesTranslates);
        
        if (_deviceTypeProvider.DeviceType == DeviceTypeEnum.Mobile)
            _desktopObject.DisactiveSelf();
    }

    private void Update() {
        if (CheckKey()) {
            TryUse();
        }
    }
    
    private bool CheckKey() {
        var keyboard = Keyboard.current;
        return KeyBoardKey switch
        {
            KeyBoardKey.One => keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame,
            KeyBoardKey.Two => keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame,
            KeyBoardKey.Three => keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame,
            _ => false
        };
    }
    
    
    private void HideShowElementsByRole(PlayerRoleInGame role) {
        if (role == PlayerRoleInGame.Hunter) {
            IsAvailable = false;
            _reloadProgress.fillAmount = 1f;
        }
        else {
            CheckAvailable(); 
        }
    }


    private void TryUse() {
        if (!IsAvailable) {
            Debug.Log("Бонус на перезарядке или уже юзается ");
            return;
        }

        if (BonusCount == 0) {
            Debug.Log("Бонусов нема");
            return;
        }
        
        if(_mainPlayer.RoleBehaviour.CurrentRole == PlayerRoleInGame.Hunter){ 
            Debug.Log("Игрок хантер, он не может юзать бонусы");
            return;
        }
        UseBonus();
    }
    
    
    private void SetProgressBarVisible(bool visible) {
        _useContainer.SetActive(visible);
        _useTimeProgress.offsetMax = _startOffset;
    }
    

    private void UseBonus() {
        GameEvents.BonusUseInvoke();
        Bonus.Use(_mainPlayer);
        GetOneBonus(true);
        IsAvailable = false;
        
        
        UniTaskHelper.DisposeTask(ref _tokenSource);
        _tokenSource = new  CancellationTokenSource();
        StartUseTimerAsync(_tokenSource.Token).Forget();
    }
    
    
    public void StopBonusWork() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        CheckAvailable();
        Bonus.StopWork(_mainPlayer);
        SetProgressBarVisible(false);
    }


    private async UniTask StartUseTimerAsync(CancellationToken token) {
        SetProgressBarVisible(true);

        float yEnd = _useTimeProgress.rect.height;
        
        float duration = _gameData.BonusDuration;
        float elapsedTime = _gameData.BonusDuration;
        
        
        while (elapsedTime > 0 && !token.IsCancellationRequested) {
            float progress = elapsedTime/duration;

            float y = GetYPoseByPercent(progress, yEnd, _useTimeProgress);
            _useTimeProgress.offsetMax = new Vector2(_useTimeProgress.offsetMax.x, y);
            
            elapsedTime -= Time.deltaTime;
            await UniTask.Yield(cancellationToken: token);
        }

        SetProgressBarVisible(false);
        _useTimeProgress.offsetMax = _startOffset;
        
        SetReloadBonusTimerAsync(token).Forget();
    }
    
    private static float GetYPoseByPercent(float percent, float yEnd, RectTransform parent) {
        if (yEnd < 0) {
            Canvas.ForceUpdateCanvases();
            yEnd = parent.rect.height;
        }
        return -yEnd * (1f - percent);
    }


    private async UniTask SetReloadBonusTimerAsync(CancellationToken token) {
        float duration = _gameData.BonusReload;
        float elapsedTime = _gameData.BonusReload;
        while (elapsedTime > 0 && !token.IsCancellationRequested) {
            _reloadProgress.fillAmount = elapsedTime/duration;
            
            elapsedTime -= Time.deltaTime;
            await UniTask.Yield(cancellationToken: token);
        }

        if (BonusCount != 0) {
            GameEvents.BonusReloadedInvoke();
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
