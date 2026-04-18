using Cysharp.Threading.Tasks;
using SanyaBeerExtension;
using TMPro;
using UnityEngine;
using Zenject;

public class BattleStartVisualizer : MonoBehaviour {
    [SerializeField] private GameObject _timerContainer;
    [SerializeField] private GameObject _timerShadow;
    [SerializeField] private TextMeshProUGUI _timerText;
    
    
    [Inject] private GameData _gameData;
    [Inject] private LocalizationData _localization;
    
    
    public bool AnimationPlay { get; private set; }

    
    public void ShowAnimation(bool showShadow) {
        _timerShadow.SetActive(showShadow);
        AnimationPlay = true;
        PlayAnimation().Forget();
    }

    
    private async UniTaskVoid PlayAnimation() {
        float elapsedTime = _gameData.ColldownToStartGame;
        
        _timerContainer.ActiveSelf();
        while (elapsedTime != 0) {
            _timerText.text = elapsedTime.ToString();
            await UniTask.WaitForSeconds(1f);
            elapsedTime--;
        }
        _timerContainer.DisactiveSelf();
        AnimationPlay = false;
    }
}