using TMPro;
using UnityEngine;

public class PetStationViewPurchase : PetStationViewBase {
    [SerializeField] private TMP_Text _priceText;
    private bool _setZeroPrice;

    private void Awake() {
        SetPrice(_config.Price);
    }

    private void SetPrice(long price) {
        _priceText.text = _formatter.ValuteFormatter(price);
    }

    private void OnEnable() {
        _bank.BankNewMoneyMinus += BankOnBankChanged;
        _bank.BankNewMoneyPlus += BankOnBankChanged;
    }

    public void SetZeroPrice() {
        _setZeroPrice = true;
        _allowToUse = true;
        SetPrice(0);
        _customTrigger.SetAvailable();
    }
    
    
    private void BankOnBankChanged(long _) {
        CheckAvailable();
    }
    

    protected override void StartInit() {
        CheckAvailable();
    }

    private void CheckAvailable() {
        if (_bank.PlayerCapital < _config.Price) {
            _customTrigger.SetUnvailable();
            AllowToGetPet = false;
        }
        else {
            _customTrigger.SetAvailable();
            AllowToGetPet = true;
        }
    }
    

    protected override void AddPet() {
        Debug.Log("Buy pet");
        PetChance pet = GetRandomPet(_config);
        if (_setZeroPrice) {
            _setZeroPrice = false;
            _allowToUse = false;
            CheckAvailable();
            SetPrice(_config.Price);
        }
        else {
            _bank.SpendMoney(_config.Price);
        }
        _petsManager.AddPet(pet.PetItemConfig);
        _petOpenView.ShowOpenPetView(pet, _config.EggIcon);
    }
    
}