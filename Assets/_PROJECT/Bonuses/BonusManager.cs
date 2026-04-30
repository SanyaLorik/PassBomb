using System.Collections.Generic;
using System.Linq;
using Architecture_M;
using UnityEngine;
using Zenject;


public class BonusManager : MonoBehaviour {
    [SerializeField] private List<BonusSlot> _bonusSlots;
    private BonusSlot _choosedModifierChanger;
    
    
    private GameSave _saves => _saver.GetSave<GameSave>();
    
    
    [Inject] private IGameSave _saver; 
    [Inject] private MainGameStarter _gameStarter;
    [Inject] private BattleManager _battleManager;
    [Inject] private GameData _data;
    [Inject] private TutorialManager _tutorialManager;
    [Inject] private IPassBombPlayer _mainPlayer;

    
    private void OnEnable() {
        GameEvents.PlayerStayHunter += PlayerStayHunter;
    }

    
    private void Start() {
        if (!_tutorialManager.TutorialPassed) {
            InitBonusesCount(_data.InitBonusCounts);
        }
    }


    public void SetAvailableToUseBonuses(bool available) {
        _bonusSlots.ForEach(bs => bs.SetStateAvailable(available));
    }
    
    
    private void InitBonusesCount(int count) {
        // clear - tru, чтоб игрок не нафармил перезаходами в игру бонусов
        foreach (var bonusSlot in _bonusSlots) {
            _saves.AddNewBonusCounts(bonusSlot.BonusItem.Id, count, true);
        }
    }
    
    
    private void PlayerStayHunter(PlayerRoleBehaviour player) {
        IPassBombPlayer passPlayer = _battleManager.Players.First(p => p.RoleBehaviour == player);
        passPlayer.SetBigJump(false);
        passPlayer.SetInvinsible(false);
    }

  
}