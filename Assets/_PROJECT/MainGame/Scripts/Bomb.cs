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
    [SerializeField] private GameObject _allBomb;
    [SerializeField] private GameObject _bombModel;
    [SerializeField] private Transform _permanentBombParent;
    
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
    [Inject] private TutorialManager _tutorialManager;
    [Inject] private MainGameStarter _gameStarter;
    [Inject] private BattleManager _battleManager;

    
    private void OnEnable() {
        _gameStarter.GameStarted += OnGameStarted;
        _battleManager.ForceStartedNewGame += StopBomb; 
    }

    
    private void Start() {
        _allBomb.DisactiveSelf();
        _barWidth = RectTransformHelper.CalculateXEnd(_parentProgressToExplode);
    }
    
    
    public void InitBombToNewPlayer(Transform playerTransform, PlayerRoleBehaviour playerRoleBehaviour) {
        PlayerBecameHunter?.Invoke(playerRoleBehaviour);
        
        SetNewBombParent(playerTransform, false);
        
        _allBomb.ActiveSelf();
    }
    
    
    public void StartNewBombTimer() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
        _tokenSource = new CancellationTokenSource();
        BombExplode = false;
        BombTimerAsync(_tokenSource.Token).Forget();
    }

    public void ExplodeBombLater() {
        if(BombExplode) return;
        Debug.Log("Преждевременный взрыв бомбы");
        UniTaskHelper.DisposeTask(ref _tokenSource);
        Explode();
        _timerContainer.DisactiveSelf();
    }
    
    
    private void StopBomb() {
        _allBomb.DisactiveSelf();
        UniTaskHelper.DisposeTask(ref  _tokenSource);
    }


    private void OnGameStarted(bool started) {
        if (!started) {
            _allBomb.DisactiveSelf();
        }
    }


    
    public void TeleportBombToSpawn(Transform spawn) {
        _allBomb.ActiveSelf();
        SetNewBombParent(spawn, false);
    }

    
    private void SetNewBombParent(Transform spawn, bool worldPositionStays) {
        _allBomb.transform.SetParent(spawn, worldPositionStays);
        if (!worldPositionStays) {
            _allBomb.transform.localPosition = Vector3.zero;
        }
    }



    
    private async UniTask BombTimerAsync(CancellationToken token) {
        _timerContainer.ActiveSelf();
        _bombModel.ActiveSelf();

        float timeToBombExplode = _tutorialManager.TutorialPassed ? _gameData.TimeToBombExplode : 10000000f;
        
        if(!_tutorialManager.TutorialPassed) _timerContainer.DisactiveSelf();
        
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
        if (BombExplode) return;
        SetNewBombParent(_permanentBombParent, true);
        
        Debug.Log("Взрыв БОМБЫ!");
        BombExplode = true;
        BombExploded?.Invoke();
        _bombModel.DisactiveSelf();
        _cartoonExplosionFX.Play();
    }
    
    
    private void OnDestroy() {
        UniTaskHelper.DisposeTask(ref _tokenSource);
    }
}