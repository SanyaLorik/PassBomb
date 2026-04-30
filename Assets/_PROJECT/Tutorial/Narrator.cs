using Architecture_M;
using DG.Tweening;
using SanyaBeerExtension;
using TMPro;
using UnityEngine;
using Zenject;

public class Narrator : MonoBehaviour {
    [SerializeField] private GameObject _tutorialContainer;
    [SerializeField] private GameObject _narratorContainer;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Transform _bonusFinger;
    
    [Header("Анимация пальчика")]
    [SerializeField] private float _fingerDuration;
    [SerializeField] private Ease _fingerEase;
    [SerializeField] private ScaleDownUpAnimation[] _fingersInfinitAnimations;
    
    [Header("Длина подьема пальчика")]
    [SerializeField] private float _bonusFingerDeltaX;
    
    
    private Transform _currentFinger;
    private Vector2 _screenFingerStartPosition;
    
    
    [Inject] private LocalizationData _localization;
    [Inject] private TutorialManager _tutorialManager;
    
    
    private void Start() {
        _screenFingerStartPosition = _bonusFinger.localPosition;
        if (_tutorialManager.TutorialPassed) {
            _tutorialContainer.DisactiveSelf();
        }
    }
    
    
    public void Disactive() {
        _tutorialContainer.DisactiveSelf();
        _bonusFinger.DisactiveSelf();
    }
    
    public void Active() {
        _tutorialContainer.ActiveSelf();
        _narratorContainer.ActiveSelf();
    }
    
    public void DisableNarrator() {
        Disactive();
        _tutorialManager.NewTutorialStep -= CheckToDestroyCurrentFinger;
        _fingersInfinitAnimations.ForEach(a => a.Kill());
    }
    
    
    private void CheckToDestroyCurrentFinger() {
        if (_currentFinger != null && _currentFinger.gameObject.activeSelf) {
            DOTween.Kill(_currentFinger); // Останавливаем анимацию
            _currentFinger.gameObject.DisactiveSelf();
        }
    }
    
    
    public void SetTutorialText(TutorialStep step) {
        _narratorContainer.ActiveSelf();
        _text.text = _localization.GetTranslatedText(step, _localization.TutorialTranslates);
    }

    public void HideScreenFinger() {
        _bonusFinger.ActiveSelf();
    }
    
    public void ShowScreenFinger() {
        _bonusFinger.localPosition = _screenFingerStartPosition;
        _bonusFinger.ActiveSelf();
        AnimateFinger(_bonusFinger, _bonusFingerDeltaX);
    }
    
    
    
    private void AnimateFinger(Transform finger, float _fingerDelta) {
        _currentFinger = finger;
        finger
            .DOMoveX(finger.position.x + _fingerDelta, _fingerDuration)
            .SetEase(_fingerEase)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(finger.gameObject);
    }

}