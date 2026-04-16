using Architecture_M;
using Cysharp.Threading.Tasks;
using SanyaBeerExtension;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


// Настройка положения игроков во время игры, поворот и тп
// Битва, смена ходов, уведомления о ходах которая будет слухать камера!


public class BattleManager : MonoBehaviour {
    // Будет выбираться рулеткой шо кинуть может
    [SerializeField] private Button _giveUpButton;
    
    [Inject] private CameraOrbitalController _camera;
    public bool MainPlayerPlay { get; private set; }
    public bool AllowToPlay { get; private set; }

    public void InitForNewGame() {
    }

    public void SetGameOver() {
        
    }
    
    
    private void FocusCamera(Transform obj) {
    }
    
    
    
}