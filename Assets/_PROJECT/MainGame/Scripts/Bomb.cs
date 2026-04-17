using System;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using RavingBots.CartoonExplosion;
using SanyaBeerExtension;
using TMPro;
using UnityEngine;
using Zenject;

public class Bomb : MonoBehaviour {
    [SerializeField] private GameObject _bombInstance;
    [SerializeField] private CartoonExplosionFX _cartoonExplosionFX;
    [Header("Визуал таймера")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private RectTransform _bombBar;
    [SerializeField] private RectTransform _parentProgressToExplode;
    [SerializeField] private GameObject _timerContainer;
    
    public event Action BombExploded;
    public event Action<PlayerRoleBehaviour> PlayerBecameHunter;
    public bool BombExplode { get; private set; }
    

    private CancellationTokenSource _tokenSource;
    private float _barWidth;
    
    
    [Inject] private GameData _gameData;
    [Inject] private MainGameStarter _gameStarter;

    private void OnEnable() {
        _gameStarter.GameStarted += OnGameStarted;
    }

    private void OnGameStarted(bool started) {
        if (!started) {
            _bombInstance.DisactiveSelf();
        }
    }

    private void Start() {
        _bombInstance.DisactiveSelf();
        _barWidth = RectTransformHelper.CalculateXEnd(_parentProgressToExplode);
    }

    
    public void InitBombToNewPlayer(Transform playerTransform, PlayerRoleBehaviour playerRoleBehaviour) {
        PlayerBecameHunter?.Invoke(playerRoleBehaviour);
        _bombInstance.transform.SetParent(playerTransform, false); 
        _bombInstance.ActiveSelf();
        _bombInstance.transform.localPosition = Vector3.zero;
    }

    
    public void StartNewBombTimer() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        _tokenSource = new CancellationTokenSource();
        BombExplode = false;
        BombTimerAsync(_tokenSource.Token).Forget();
    }
    
    
    private async UniTask BombTimerAsync(CancellationToken token) {
        _timerContainer.ActiveSelf();
        float timeToBombExplode = _gameData.TimeToBombExplode;
        float elapsedTime = _gameData.TimeToBombExplode;
        SetFullBar();
        
        Vector2 startSize = new Vector2(-_barWidth,  _bombBar.offsetMax.y);
        Vector2 targetSize = new Vector2(0,  _bombBar.offsetMax.y);
        
        while (!token.IsCancellationRequested && elapsedTime > 0) {
            float progress = elapsedTime / timeToBombExplode;
            _timerText.text = elapsedTime.ToString("F0");
            _bombBar.offsetMax = Vector2.Lerp(startSize, targetSize, progress);
            elapsedTime -= Time.deltaTime;
            await UniTask.Yield();
        }

        if (!token.IsCancellationRequested) {
            Explode();
            _timerContainer.DisactiveSelf();
        }
    }

    private void SetFullBar() {
        _bombBar.offsetMax = new Vector2(0, _bombBar.offsetMax.y);
    }


    private void Explode() {
        Debug.Log("Взрыв БОМБЫ!");
        BombExploded?.Invoke();
        BombExplode = true;
        _cartoonExplosionFX.Play();
    }
    
    
    private void OnDestroy() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
    }
}