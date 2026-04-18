using System;
using System.Collections.Generic;
using Architecture_M;
using UnityEngine;
using Zenject;


public class BonusManager : MonoBehaviour {
    // [SerializeField] private List<WeightedItem<IBonus>> _bonusValuesForBots;
    [SerializeField] private List<BonusSlot> _bonusSlots;
    // [Range(0,1), SerializeField] private float _chanseToTryAgainFindBonusBot;
    private BonusSlot _choosedModifierChanger;
    
    
    private GameSave _saves => _saver.GetSave<GameSave>();
    
    [Inject] private IGameSave _saver; 
    [Inject] private MainGameStarter _gameStarter;
    [Inject] private BattleManager _battleManager;
    [Inject] private GameData _data;
    [Inject] private BonusesLoader _bonusesLoader;
    [Inject] private TutorialManager _tutorialManager;
    
    public event Action<IBonus> BonusUsed; 

    
    /// <summary>
    /// Загружает TutorialManager при старте в игру
    /// </summary>
    /// <param name="count">Кол-во каждого бонуса во время туториала</param>
    public void InitBonusesCount(int count) {
        // clear - tru, чтоб игрок не нафармил перезаходами в игру бонусов
        foreach (var bonusSlot in _bonusSlots) {
            _saves.AddNewBonusCounts(bonusSlot.BonusItem.Id, count, true);
        }
    }
    
    
    public void UseBonusForBot() {
        
    }
    
    
    public void UseBonusByClick(IBonus bonus, BonusSlot bonusSlot) {
        TryUseBonus(bonus, bonusSlot);
    }

    public void DisableAllBonuses() {
        // Пока ток у игрока
        _bonusSlots.ForEach(b => b.StopBonusWork());
    }
    
    private void TryUseBonus(IBonus bonus, BonusSlot bonusSlot) {
        bonusSlot.UseBonusAfterCheck();
        BonusUsed?.Invoke(bonus);
    }
    
  
}