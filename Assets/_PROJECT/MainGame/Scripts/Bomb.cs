using System;
using System.Threading;
using _PROJECT.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using RavingBots.CartoonExplosion;
using SanyaBeerExtension;
using UnityEngine;
using Zenject;

public class Bomb : MonoBehaviour {
    [SerializeField] private GameObject _bombInstance;
    [SerializeField] private CartoonExplosionFX _cartoonExplosionFX;

    private CancellationTokenSource _tokenSource;
    [Inject] private GameData _gameData;

    public event Action BombExploded;
    public bool BombExplode { get; private set; }

    private void Start() {
        _bombInstance.DisactiveSelf();
    }

    public void InitBombToNewPlayer(Transform playerTransform) {
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
        float elapsedTime = _gameData.TimeToBombExplode;
        while (!token.IsCancellationRequested && elapsedTime > 0) {
            elapsedTime -= Time.deltaTime;
            await UniTask.Yield();
        }

        if (!token.IsCancellationRequested) {
            Explode();
        }
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