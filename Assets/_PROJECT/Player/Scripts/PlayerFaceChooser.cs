using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerFaceChooser : MonoBehaviour {
    [SerializeField] private Button _button;
    [SerializeField] private SkinWearer _randomWearer;

    /// <summary>
    /// Это запоминаем т.к выбирается не в магазе а во время боя
    /// </summary>
    public int GlovesIndex { get; private set; }
    /// <summary>
    /// Лицо: 1 - глаза, 2 - ротик
    /// </summary>
    public (int, int) FaceIndexes { get; private set; }

    
    public event Action<int, int> FaceChangeButtonPressed;
    public event Action<int> GlovesChanged;
    
    
    [Inject] MainGameStarter _mainGameStarter;
    [Inject] BattleManager _battleManager;
    
    
    private void OnEnable() {
        _button.onClick.AddListener(ChooseNextFaceIndexes);
        _mainGameStarter.GameStarted += ChooseRandomGlovesToBattle;
    }

    
    private void Start() {
        // Просто при старте пусть чето будет
        GlovesIndex = _randomWearer.GetNextGlovesIndex(GlovesIndex);
        GlovesChanged?.Invoke(GlovesIndex);
    }
    
    
    private void ChooseNextFaceIndexes() {
        FaceIndexes = _randomWearer.GetNextFaceIndexes(FaceIndexes.Item1, FaceIndexes.Item2);
        FaceChangeButtonPressed?.Invoke(FaceIndexes.Item1, FaceIndexes.Item2);
    }

    
    private void ChooseRandomGlovesToBattle(bool started) {
        if(!started || !_battleManager.MainPlayerPlay) return;
        
        GlovesIndex = _randomWearer.GetNextGlovesIndex(GlovesIndex);
        GlovesChanged?.Invoke(GlovesIndex);
    }
    
}