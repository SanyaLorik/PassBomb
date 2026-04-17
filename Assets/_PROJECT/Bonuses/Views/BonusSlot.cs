using System;
using Architecture_M;
using TMPro;
using UnityEngine;
using Zenject;

public class BonusSlot : UsableItemBase {
    [field: SerializeField] public BonusItemConfig BonusItem { get; private set; }
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private TextMeshProUGUI _bonusNameText;
    
    public IBonus Bonus => BonusItem.Bonus;
    public int BonusCount => Saves.GetBonusCount(BonusItem.Id);
    private GameSave Saves => _saver.GetSave<GameSave>();
    
    [Inject] private BonusManager _bonusManager;
    [Inject] private DiContainer _diContainer;
    [Inject] private IGameSave _saver; 
    [Inject] LocalizationData _localization;
    
    
    [Inject]
    private void Init() {
        _diContainer.QueueForInject(Bonus);
    }
    

    private void OnEnable() {
        CheckVisualCount();
    }

    
    private void Start() {
        CheckVisualCount();
        _bonusNameText.text =
            _localization.GetTranslatedText(BonusItem, _localization.BonusesTranslates);
    }


    public override void TryUse() {
        if (!IsAvailable) {
            Debug.Log("Бонус на перезарядке именно что");
            return;
        }

        if (BonusCount == 0) {
            Debug.Log("Бонусов нема");
            return;
        }
        // _bonusManager.UseBonusByClick(BonusItem.Bonus, this);
    }

    public void CheckAvailable() {
        if (BonusCount == 0) {
            SetUnvailable();
        }
    }

    
    public void AddBonusCount(int newBonusCount) {
        Saves.AddNewBonusCounts(BonusItem.Id, newBonusCount);
        CheckVisualCount();
    }

    
    public void SetBonusCount(int newBonusCount) {
        Saves.AddNewBonusCounts(BonusItem.Id, newBonusCount, true);
        CheckVisualCount();
    }

    
    public void GetOneBonus(bool useSaves = false) {
        if (BonusCount != 0) {
            if (useSaves) {
                _saver.GetSave<GameSave>().SetMinusOneBonus(BonusItem.Id);
                _saver.Save();
            }
        }
        CheckVisualCount();
    }

    
    private void CheckVisualCount() {
        _countText.text = BonusCount.ToString();
        CheckAvailable();
    }
    
}