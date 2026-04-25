using System.Collections;
using SanyaBeerExtension;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

[RequireComponent(typeof(NavMeshAgent))]
public class BotStateManager : MonoBehaviour, IPassBombPlayer {
    [field: SerializeField] public bool ShowInSpawn { get; private set; }
    [field: SerializeField] public Transform Transform { get; private set; }
    [field: SerializeField] public BotWalkManager BotWalkManager { get; private set; }
    [SerializeField] private Transform _skinParent;
    [SerializeField] private BotAnimator _botAnimator;
    [SerializeField] private GameObject _skinInstance;
    [SerializeField] private BotMonolog _botMonolog;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private PlayerRoleBehaviour _roleBehaviour;
    
    private Vector3 _posBeforeTeleport;

    public bool IsPlaying { get; private set; }
    public string Nickname => _botMonolog.NickName;
    public PlayerRoleBehaviour RoleBehaviour => _roleBehaviour;
    
    
    public string SkinId { get; private set; }
    
    
    [Inject] private GameData _gameData;

    
    private void Awake() {
        Destroy(_skinInstance);
    }
    
    
    private void Start() {
        SetStartWanderIfActive(true);
    }
    

    private void SetStartWanderIfActive(bool startWander) {
        if (ShowInSpawn == false) return;
        
        if(startWander) BotWalkManager.StartWanderSpawn();
        else BotWalkManager.StopWanderSpawn();
    }


    public void SetPlayStatus(bool goPlay) {
        IsPlaying = goPlay;
        _agent.enabled = true;
        BotWalkManager.StopPhys();
        RoleBehaviour.DisposeAllLogic();
        
        gameObject.SetActive(ShowInSpawn || goPlay);
        
        if (goPlay) {
            ActiveBotInGame();
        }
        // Возвращение на спавн
        else {
            Debug.Log("Возвращение на спавн игрока " + _botMonolog.NickName);
            SetBotStateBeforeGame();
            TeleportToPoint(_posBeforeTeleport);
        }
        SetStartWanderIfActive(!goPlay);
    }

    public void SetPlayStatusSilent(bool goPlay) {
        IsPlaying = goPlay;
    }


    public void TeleportToPoint(Vector3 pos) {
        Debug.Log($"TeleportToPoint {pos}  игрока {_botMonolog.NickName}");
        _posBeforeTeleport = transform.position;
        if (NavMesh.SamplePosition(pos, out var hit, 10f, NavMesh.AllAreas)) {
            _agent.Warp(hit.position);
        }
    }

    
    public void SetMovingStatus(bool enable) {
        BotWalkManager.SetMovingStatus(enable);
    }

    public void SetDefaultRoundSpeed() {
        _agent.speed = _gameData.DefaultSpeedInRound;
    }
    
    public void SetHunterSpeed() {
        _agent.speed = _gameData.HunterSpeed;
    }

    public void SetBonusSpeed() {
        _agent.speed = _gameData.VelocityBonusSpeed;
    }

    public void SetDefaultSpeed() {
        _agent.speed = _gameData.BotSpeed;
    }

    public void SetBigJump(bool state) {
        BotWalkManager.SetBigJump(state);
    }


    public void PushAway(Vector3 direction) {
        BotWalkManager.PushAway(direction);
    }

    public bool IsPushed => BotWalkManager.IsPushed;

    public void SetInvinsible(bool invnincible) {
        _roleBehaviour.SetInvincibleAfterBonus(invnincible);
    }


    public void RotateToTarget(Vector3 targetPosition) {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }
    

    public void InitAnimator() {
        _botAnimator.InitAnimator(BotWalkManager);
    }
    
    
    private void SetBotStateBeforeGame() {
        if (ShowInSpawn) {
            _agent.ActiveSelf();
        }
        else {
            _agent.DisactiveSelf();
        }
    }

    private void ActiveBotInGame() {
        if (ShowInSpawn == false) {
            _agent.ActiveSelf();
        }
    }
    

    public void SetBotSpeak() {
        _botMonolog.SaySomething();
    }

    public void SetBotStfu() {
        _botMonolog.Stfu();
    }

    public void SetBotSkin(SkinItemConfig skinItemConfig) {
        SkinId = skinItemConfig.Id;
        gameObject.ActiveSelf();
        StartCoroutine(ChangeSkinRoutine(skinItemConfig));
    }

    
    private IEnumerator ChangeSkinRoutine(SkinItemConfig skin) {
        if (_skinInstance != null) {
            Destroy(_skinInstance);
            _botAnimator.SetModelData(null, null);
        }
        yield return null; // дождаться конца кадра

        _skinInstance = Instantiate(skin.SkinPrefab, _skinParent);
        var skinItem = _skinInstance.GetComponent<SkinElementsController>();
        _botAnimator.SetModelData(skin.Avatar, skinItem);
        gameObject.SetActive(ShowInSpawn);
    }

}