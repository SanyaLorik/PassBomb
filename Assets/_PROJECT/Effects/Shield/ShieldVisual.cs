using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SanyaBeerExtension;
using TMPro;
using UnityEngine;

public class ShieldVisual : MonoBehaviour {
    [SerializeField] private Transform _shield;
    
    [Header("Анимация щита")]
    [SerializeField] private PairedValue<float> _shieldShowDurations;
    [SerializeField] private PairedValue<Ease> _shieldShowEase;
    
    [Header("Бар")]
    [SerializeField] private RectTransform _bar;
    [SerializeField] private RectTransform _barParent;
    [SerializeField] private float _changeBarDuration = 1f;
    [SerializeField] private TextMeshProUGUI _shieldHp;

    private CancellationTokenSource _tokenSource;
    private Sequence _shieldSequence;

    private void OnEnable() {
        DisposeShield();
    }
    

    public void HideShieldFast() {
        DisposeShield();
    }
    
    public void SetShieldPercentage(float percentage, int hp) {
        percentage = Mathf.Clamp01(percentage);
        UniTaskHelper.DisposeTask(ref _tokenSource);
        _tokenSource = new CancellationTokenSource();
        ChangeShieldPercentageAsync(percentage, _tokenSource.Token).Forget();
        _shieldHp.text = hp.ToString();
    }
    
    private async UniTask ChangeShieldPercentageAsync(float percentage, CancellationToken token) {
        float elapsedTime = 0f;

        Vector2 initPos = _bar.offsetMax;
        Vector2 targetPos = new Vector2(GetXPoseByPercent(percentage, _barParent), 0);
        
        // Debug.Log("percentage = " + percentage);
        // Debug.Log("initPos = " + initPos);
        // Debug.Log("targetPos = " + targetPos);
        
        
        while (!token.IsCancellationRequested && elapsedTime < _changeBarDuration) {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / _changeBarDuration;
            Vector2 interp = Vector2.Lerp(initPos, targetPos, progress);
            _bar.offsetMax = interp;
            // Debug.Log("interp = " + interp);
            await UniTask.Yield();
        }
        _bar.offsetMax = targetPos;
        if (percentage == 0f) {
            SetShieldAnimation(false);
        }
    }
    
    
    public void ShieldEnableAnimate(bool enable) {
        if (enable) {
            SetShieldAnimation(true);
        }
        else {
            SetShieldAnimation(false);
        }
    }
    
    public void ShieldShowFast(bool show) {
        _shield.localScale = show ? Vector3.one : Vector3.zero;
    }

    private void SetShieldAnimation(bool show) {
        // Целевой масштаб
        float targetScale = show ? 1f : 0f;
    
        // Если уже в нужном состоянии — выходим
        if(_shield.localScale.x == targetScale) return;
    
        // Убиваем старую анимацию (если есть)
    
        // Выбираем длительность и ease в зависимости от show
        float duration = show ? _shieldShowDurations.From : _shieldShowDurations.To;
        Ease ease = show ? _shieldShowEase.From : _shieldShowEase.To;
    
        // Запускаем новую
        _shieldSequence?.Kill();
        _shieldSequence = DOTween.Sequence();
        _shieldSequence.Append(_shield.DOScale(targetScale, duration).SetEase(ease));
    
        // Необязательно: чистим ссылку после завершения
        _shieldSequence.OnComplete(() => {
            if(_shieldSequence != null && _shieldSequence.active == false)
                _shieldSequence = null;
        });
    }

    
    private void DisposeShield() {
        _shieldSequence?.Kill();
        _shield.localScale = Vector3.zero;
        UniTaskHelper.DisposeTask(ref _tokenSource);
    }

    
    /// <summary>
    /// есть в RectTransformHelper просто приватное, саня верни доступ(((
    /// </summary>
    /// <param name="percent"></param>
    /// <param name="xEnd"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private static float GetXPoseByPercent(float percent, RectTransform parent)
    {
        float xEnd =  parent.rect.width;
        if (xEnd < 0)
        {
            Canvas.ForceUpdateCanvases();
            xEnd = parent.rect.width;
        }
        return -xEnd * (1f - percent);
    }


}