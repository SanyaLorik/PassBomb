using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SanyaBeerExtension;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

public class SoundManager : MonoBehaviour {
    [Header("Звук накладывается то сколько ждать")]
    [SerializeField] private float _soundDelay = 0.5f;
    
    [Header("Конфиги")]
    [SerializeField] private List<SoundConfig> soundConfigs;
    [Header("Тонкая настройка")]
    [SerializeField] private int _poolSize; 
    [SerializeField] private float _fadeTime = 1f;
    [SerializeField] private float _stepTiming = 0.2f;
    [Header("Background Music")]
    [SerializeField] private AudioSource _walkMusicSource;
    [SerializeField] private AudioSource _playMusicSource;
    
    [Header("Mixer")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioMixerGroup _musicMixerGroup;
    [SerializeField] private AudioMixerGroup _soundMixerGroup;
    
    
    private Dictionary<SoundType, SoundConfig> _soundConfigDict = new ();
    private List<AudioSource> _sources = new();
    private bool _playGame;
    private bool _allowToSound = true;
    private GameObject _audioSourcesContainer;
    
    
    [Inject] private PlayerStateManager _stateManager;
    [Inject] private PlayerMovement _playerMovement;
    [Inject] private PlayerBank _bank;
    [Inject] private SettingsManager _settings;
    [Inject] private BattleManager _battleManager;
    [Inject] private BonusManager _bonusManager;
    [Inject] private GameOverView _gameOverView;
    [Inject] private Bomb _bomb;
    [Inject] private PlayerPetsManager _playerPetsManager;
    [Inject] private PetOpenView _petOpenView;
    
    
    private void Awake() {
        CreateAudioSourceContainer();
        foreach (var _sound in soundConfigs) {
            _soundConfigDict[_sound.SoundType] = _sound;
        }
       
        // создаём пул
        for (int i = 0; i < _poolSize; i++) {
            CreateNewAudioSource();
        }
        PlayMusic(_soundConfigDict[SoundType.MainBackground]);
    }
    

    private void OnEnable() {
        // STATE CHANGES
        _stateManager.StateChanged += StateManagerOnChangeState;
        // PLAYER MOVE
        _playerMovement.JumpPressed += PlayerMovementOnJumpPressed;
        _playerMovement.DoubleJumpPressed += PlayerMovementOnJumpPressed;
        _playerMovement.RunningStateChanged += PlayerMovementOnRunningStateChanged;
        _playerMovement.Floored += PlayerMovementOnFloored;
        _playerMovement.PlayerHit += OnPlayerHit;
        // BANK / WEAR
        _bank.BankNewMoneyPlus += OnMoneyPlus;
        _bank.BankNewMoneyMinus += OnMoneyPlus;
        // Settings
        _settings.MusicValueChanged += SettingsOnMusicValueChanged;
        _settings.EffectsValueChanged += SettingsOnEffectsValueChanged;
        // Play Events
        GameEvents.BonusUsed += PlayBonuseUse;
        _battleManager.MainPlayerWin += PlayerWin;
        GameEvents.BonusReloaded += BonusReloaded;
        GameEvents.TriggerUsed += UiButtonClick;
        GameEvents.PlayerStayHunter += StayHunter;
        _bomb.BombExploded += BombExplode;
        _bomb.BombExploded += PlayerAugh;
        _petOpenView.PetCanasOpen += PlayerMovementOnJumpPressed;
        _petOpenView.PetNewOpen += () => OnMoneyPlus(0);
    }

    


    private void OnDisable() {
        // STATE CHANGES
        _stateManager.StateChanged -= StateManagerOnChangeState;
        // PLAYER MOVE
        _playerMovement.JumpPressed -= PlayerMovementOnJumpPressed;
        _playerMovement.DoubleJumpPressed -= PlayerMovementOnJumpPressed;
        _playerMovement.RunningStateChanged -= PlayerMovementOnRunningStateChanged;
        _playerMovement.Floored -= PlayerMovementOnFloored;
        _playerMovement.PlayerHit -= OnPlayerHit;
        // BANK / WEAR
        _bank.BankNewMoneyPlus -= OnMoneyPlus;
        _bank.BankNewMoneyMinus -= OnMoneyPlus;
        // Settings
        _settings.MusicValueChanged -= SettingsOnMusicValueChanged;
        _settings.EffectsValueChanged -= SettingsOnEffectsValueChanged;
        // Play Events
        GameEvents.BonusUsed -= PlayBonuseUse;
        _battleManager.MainPlayerWin -= PlayerWin;
        GameEvents.BonusReloaded -= BonusReloaded;
        GameEvents.TriggerUsed -= UiButtonClick;
        GameEvents.PlayerStayHunter -= StayHunter;
        _bomb.BombExploded -= BombExplode;
        _bomb.BombExploded -= PlayerAugh;
        _petOpenView.PetCanasOpen -= PlayerMovementOnJumpPressed;
    }
    
    
    private void Start() {
        SettingsOnMusicValueChanged(_settings.MusicValue);
        SettingsOnEffectsValueChanged(_settings.EffectsValue);
    }
    
    
    private void StayHunter(PlayerRoleBehaviour obj) {
        if(_battleManager.PlayerReturnToSpawn) return;
        PlaySoundByType(SoundType.PlayerStayHunter);
    }

    private void OnPlayerHit() {
        PlaySoundByType(SoundType.HitPlayer);
    }
    
    private void CreateAudioSourceContainer() {
        _audioSourcesContainer = new GameObject("AudioSources");
        _audioSourcesContainer.transform.SetParent(transform);
    }


    private void PlayerWin(bool win) {
        PlaySoundByType(win ? SoundType.Win : SoundType.Loose);
    }

    private void PlayBonuseUse(IBonus _) {
        PlaySoundByType(SoundType.BonusUse);
    }

    private void BonusReloaded() {
        PlaySoundByType(SoundType.ReloadBonus);
    }

    
    private void PlayerAugh() {
        if(_battleManager.PlayerReturnToSpawn) return;
        PlaySoundByType(SoundType.PlayerAugh);
    }
    
    
    private void BombExplode() {
        if(_battleManager.PlayerReturnToSpawn) return;
        PlaySoundByType(SoundType.Explosion);
    }
    

    private AudioSource CreateNewAudioSource() {
        if (_audioSourcesContainer == null) {
            CreateAudioSourceContainer();
        }
        AudioSource source = _audioSourcesContainer.AddComponent<AudioSource>();
        _sources.Add(source);
        return source;
    }
    
    private void PlaySoundByType(SoundType type) {
        if (!_soundConfigDict.TryGetValue(type, out var config)) {
            Debug.Log("Нет звука с типом " + type);
            return;
        }
        // Debug.Log("Приогрывание " + type);
        AudioClip clip = GetSource(config);
        AudioSource source = GetFreeSource(type);
        
        PlayInSource(source, clip, config);
    }
    
    // 3D
    // private void PlayTrampolineSound(Trampoline trampoline) {
    //     if (!_soundConfigDict.TryGetValue(SoundType.Trampoline, out var config)) {
    //         Debug.Log("Нет звука с типом " + SoundType.Trampoline);
    //         return;
    //     }
    //     AudioClip clip = GetSource(config);
    //     AudioSource source = trampoline.AudioSource;
    //     
    //     PlayInSource(source, clip, config);
    // }
    
    private void SettingsOnMusicValueChanged(float value) {
        float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        _audioMixer.SetFloat("MusicVolume", db);
    }
    
    private void SettingsOnEffectsValueChanged(float value) {
        float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        _audioMixer.SetFloat("EffectsVolume", db);
    }
    
    public void UiButtonClick() {
        PlaySoundByType(SoundType.UIButton);
    }
    
    private void OnMoneyPlus(long _) {
        PlaySoundByType(SoundType.Money);
        // Debug.Log("OnMoneyPlus");
    }
    
    private void BuyOrUnlock(long _) {
        if(!_allowToSound) return;
        // Debug.Log("BuyOrUnlock sound");
        PlaySoundByType(SoundType.Money);
        StartCoroutine(WaitForSoundDelay(_soundDelay));
    }
    
    private IEnumerator WaitForSoundDelay(float time) {
        _allowToSound = false;
        yield return new WaitForSeconds(time);
        _allowToSound = true;
    }
    
    private void PlayerMovementOnFloored() {
        // Можно звук приземления
        PlaySoundByType(SoundType.Step);
        PlayerMovementOnRunningStateChanged(_playerMovement.IsRunning);
    }
    
    private CancellationTokenSource _cancellationTokenSource;
    private void PlayerMovementOnRunningStateChanged(bool isRunning) {
        _cancellationTokenSource?.Cancel();
        if (isRunning) {
            _cancellationTokenSource = new CancellationTokenSource();
            StepCycleAsync(_cancellationTokenSource.Token).Forget();
        }
    }
    
    private async UniTask StepCycleAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            if (!_playerMovement.IsGrounded) {
                return;
            }
            PlaySoundByType(SoundType.Step);
            await UniTask.Delay(TimeSpan.FromSeconds(_stepTiming), cancellationToken: token);
        }
    }
    
    private void PlayerMovementOnJumpPressed() {
        PlaySoundByType(SoundType.Jump);
    }
    
    
    private static void PlayInSource(AudioSource source, AudioClip clip, SoundConfig config) {
        source.clip = clip;
        source.volume = config.Volume;
        source.pitch = UnityEngine.Random.Range(config.PitchDiapasone.From, config.PitchDiapasone.To);
        source.loop = config.Loop;
        source.spatialBlend = config.SpatialBlend;
        source.outputAudioMixerGroup = config.MixerGroup;
        source.Play();
    }
    
    private static AudioClip GetSource(SoundConfig config) {
        return config.AudioClips.GetRandomElement();
    }
    
    
    private AudioSource GetFreeSource(SoundType type) {
        _sources.RemoveAll(s => s == null);
        foreach (var source in _sources) {
            if (!source.isPlaying)
                return source;
        }
    
        var newSource = CreateNewAudioSource();
        return newSource;
    }
    
    
    
    private void StateManagerOnChangeState(PlayerState state) {
        if (state == PlayerState.InSpawn) {
            // Проигрывание MainBackground
            _playGame = false;
            PlayMusic(_soundConfigDict[SoundType.MainBackground]);
        }
        else if (state == PlayerState.Play) {
            // Проигрывание FlyBackground
            _playGame = true;
            PlayMusic(_soundConfigDict[SoundType.PlayBackground]);
        }
    }
    
    
    private void PlayMusic(SoundConfig config) 
    {
        AudioSource targetSource = config.SoundType switch 
        {
            SoundType.MainBackground => _walkMusicSource,
            SoundType.PlayBackground => _playMusicSource,
            _ => null
        };
        if (targetSource == null) return;
    
        // Если этот источник уже играет нужный клип - выходим
        if (targetSource.isPlaying && targetSource.clip == config.AudioClips[0])
            return;
    
        // Настраиваем источник
        targetSource.clip = config.AudioClips[0];
        targetSource.volume = 0f;
        targetSource.loop = config.Loop;
        targetSource.outputAudioMixerGroup = config.MixerGroup;
        targetSource.Play();
    
        // Плавно затухаем все другие источники (кроме target)
        foreach (var src in new[] { _walkMusicSource, _playMusicSource })
        {
            if (src == targetSource) continue;
    
            if (src.isPlaying) 
            {
                src.DOFade(0f, _fadeTime).OnComplete(() =>
                {
                    src.Stop();
                    src.volume = 1f;
                });
            }
        }
    
        // Плавное появление нового трека
        targetSource.DOFade(config.Volume, _fadeTime);
    }
    
}
