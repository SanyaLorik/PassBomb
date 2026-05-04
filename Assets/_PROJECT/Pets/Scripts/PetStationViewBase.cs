using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Zenject;
using Random = UnityEngine.Random;


public abstract class PetStationViewBase : MonoBehaviour {
    
    [SerializeField] protected TextMeshProUGUI _statonNameText;
    [SerializeField] protected DelayedTrigger _customTrigger;
    [SerializeField] protected PetStationConfig _config;
    [SerializeField] protected PetEntityView[] _views;
    [SerializeField] private bool _sortByUp;

    [Inject] protected NumberFormatter _formatter;
    [Inject] protected PlayerBank _bank;
    [Inject] protected PetOpenView _petOpenView;
    [Inject] protected PlayerPetsManager PlayerPetsManager;
    [Inject] protected LocalizationData _localization;

    protected bool AllowToGetPet = true;
    protected bool _allowToUse = true;
    protected int _showedReward = 0;
    private float _divider;

    public void SetAllowUse(bool use) {
        _allowToUse = use;
    }

    
    protected void Start() {
        _divider = ChanceSum(_config.Pets);
        Initialize();
        StartInit();
        _statonNameText.text = _localization.GetTranslatedText(_config.StationNameId, _localization.EggStationNameTranslates);
    }


    private void OnTriggerEnter(Collider collider) {
        if (!_allowToUse) return;
        if (!collider.TryGetComponent(out PlayerMovement _)) return;
        if (AllowToGetPet) {
            _customTrigger.DelayedTriggerAction(AddPet);
        }
    }

    protected abstract void AddPet();
    protected virtual void StartInit(){}

    protected void OnTriggerExit(Collider collider) {
        if (!collider.TryGetComponent(out PlayerMovement _)) return;
        _customTrigger.CancelTriggerAction();
        Debug.Log("Операция по получению пета отменена");
    }
    
    private void Initialize() {
        List<PetChance> sortedPets;
        if (_sortByUp) {
            sortedPets = _config.Pets.ToList().OrderBy(a => a.Chance).ToList();
        }
        else {
            sortedPets = _config.Pets.ToList().OrderByDescending(a => a.Chance).ToList();
        }

        int length = Mathf.Min(_views.Length, sortedPets.Count);
        for (int i = 0; i < sortedPets.Count; i++)
            _views[i].Percentage.text = $"+{sortedPets[i].PetItemConfig.Modifier.ToString()}";

        //for (int i = 0; i < sortedPets.Count; i++) {
        //    _views[i].Percentage.text = $"{sortedPets[i].Chance / _divider * 100f:#0}%";
        //}

            //float totalPercentage = 0f;

            //for (int i = 0; i < sortedPets.Count - 1; i++) {
            //    float percentage = Mathf.Round(sortedPets[i].Chance / _divider * 100f);
            //    totalPercentage += percentage;
            //    _views[i].Icon.sprite = sortedPets[i].PetItemConfig.Sprite;
            //    _views[i].Percentage.text = $"{percentage:F0}%";
            //}

            //// Последний элемент получает остаток
            //float lastPercentage = 100f - totalPercentage;
            //_views[^1].Percentage.text = $"{lastPercentage:F0}%";
            //_views[^1].Icon.sprite = sortedPets[^1].PetItemConfig.Sprite;

    }


    protected PetChance GetRandomPet(PetStationConfig config) {
        float random = Random.Range(0f, _divider);
        float cumulative = 0f;

        foreach (var pet in config.Pets) {
            cumulative += pet.Chance;
            if (random <= cumulative)
                return pet;
        }
        
        return config.Pets[^1];
    }



    private float ChanceSum(PetChance[] pets) {
        float sum = 0f;
        foreach (var petChance in pets) {
            sum+= petChance.Chance;
        }
        return sum;
    }
}