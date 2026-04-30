using System;
using Architecture_M;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public enum TutorialStep {
    PassBombToEnemy,
    RunAwayFromEnemy,
    CatchUpEnemyWithSpeedBonus
}


public class TutorialManager : MonoBehaviour {
    [SerializeField] private Narrator _narrator;

    
    public bool InitBombToMainPlayer { get; private set; } = true;
    public bool TutorialPassed => Saves.TutorialPassed;
    public event Action NewTutorialStep; 


    private GameSave Saves => _saver.GetSave<GameSave>();
    
    [Inject] private IGameSave _saver; 
    [Inject] private MainGameStarter _gameStarter; 
    [Inject] private BattleManager _battleManager; 
    [Inject] private MainGameStarter _mainGameStarter; 
    [Inject] private IPassBombPlayer _mainPlayer; 
    [Inject] private BonusManager _bonusManager;
    [Inject] private Bomb _bomb;
    
    
    
    public void OnEnable() {
        if (!TutorialPassed) {
            _battleManager.GameReadyToPlay += StartTutorial;
            GameEvents.BonusUsed += OnBonusUsed;
        }
    }
    
    
    private void Start() {
        _narrator.Disactive();
    }
    
    
    private void StartTutorial() {
        TutorialStartAsync().Forget();
    }
    
    
    private async UniTask TutorialStartAsync() {
        // Догони врага и передай бомбу
        NewTutorialStep?.Invoke();
        await PassBombToEnemyStep();
        
        // Убегай от врага с бомбой
        NewTutorialStep?.Invoke();
        await RunAwayFromEnemyStep();
        
        // Догони врага, передай бомбу и выиграй!  
        NewTutorialStep?.Invoke();
        await CatchUpEnemyWithSpeedBonusStep();
    }
    
    
    // Взрыв бота 1
    private async UniTask PassBombToEnemyStep() {
        _bonusManager.SetAvailableToUseBonuses(false);
        
        _narrator.Active();
        _narrator.SetTutorialText(TutorialStep.PassBombToEnemy);
        
        await UniTask.WaitWhile(() => _mainPlayer.RoleBehaviour.CurrentRole == PlayerRoleInGame.Hunter);
        await UniTask.WaitForSeconds(1f);
        InitBombToMainPlayer = false;
        _bomb.ExplodeBombLater();
    }   
    
    
    // Бомба не взрывается а передается просто игроку 
    private async UniTask RunAwayFromEnemyStep() {
        _bonusManager.SetAvailableToUseBonuses(true);
        
        _narrator.SetTutorialText(TutorialStep.RunAwayFromEnemy);
        
        await UniTask.WaitWhile(() => _mainPlayer.RoleBehaviour.CurrentRole != PlayerRoleInGame.Hunter);
    }  
    
    
    private async UniTask CatchUpEnemyWithSpeedBonusStep() {
        _narrator.ShowScreenFinger();
        _narrator.SetTutorialText(TutorialStep.CatchUpEnemyWithSpeedBonus);
        
        await UniTask.WaitWhile(() => _mainPlayer.RoleBehaviour.CurrentRole == PlayerRoleInGame.Hunter);
        await UniTask.WaitForSeconds(1f);
        _bomb.ExplodeBombLater();
        OnTutorialEnd();
    }  
    
    
    private void OnBonusUsed(IBonus bonus) {
        _narrator.HideScreenFinger();
        _bonusManager.SetAvailableToUseBonuses(false);
    }
    
    
    private void OnTutorialEnd() {
        Debug.Log("OnTutorialEnd");
        Saves.TutorialPassed = true;
        _bonusManager.SetAvailableToUseBonuses(true);
        _saver.Save();
        _narrator.DisableNarrator();
       // Отписаться не забудь
    }

}
