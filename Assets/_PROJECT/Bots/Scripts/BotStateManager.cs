using System.Collections;
using SanyaBeerExtension;
using UnityEngine;
using UnityEngine.AI;
using Zenject;


[RequireComponent(typeof(NavMeshAgent))]
public class BotStateManager : MonoBehaviour, IPassBombPlayer {
    [field: SerializeField] public Transform Transform { get; private set; }
    [SerializeField] private Transform _skinParent;
    [SerializeField] private BotAnimator _botAnimator;
    [SerializeField] private GameObject _skinInstance;
    [SerializeField] private BotWalkManager botWalkManager;
    [SerializeField] private BotMonolog _botMonolog;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private PlayerRoleBehaviour _roleBehaviour;
    
    private Vector3 _posBeforeTeleport;

    public bool IsPlaying { get; private set; }
    public string Nickname => _botMonolog.NickName;
    
    
    public string SkinId { get; private set; }
    
    [Inject] private GameData _gameData;

    private void Awake() {
        Destroy(_skinInstance);
    }
    
    
    private void Start() {
        botWalkManager.StartWanderSpawn();
    }
    
    
    public void SetPlayStatus(bool goPlay) {
        IsPlaying = goPlay;
        if (goPlay) {
            botWalkManager.StopWanderSpawn();
        }
        // Возвращение на спавн
        else {
            TeleportToPoint(_posBeforeTeleport);
            botWalkManager.StartWanderSpawn();
        }
    }

    public void SetPlayStatusSilent(bool goPlay) {
        IsPlaying = goPlay;
    }


    public void TeleportToPoint(Vector3 pos) {
        _posBeforeTeleport = transform.position;
        if (NavMesh.SamplePosition(pos, out var hit, 5f, NavMesh.AllAreas)) {
            _agent.Warp(hit.position);
        }
    }

    
    public void SetMovingStatus(bool enable) {
        botWalkManager.SetMovingStatus(enable);
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
        botWalkManager.SetBigJump(state);
    }

    public void SetInvinsible(bool invnincible) {
        _roleBehaviour.SetInvincibleAfterBonus(invnincible);
    }

    public PlayerRoleBehaviour RoleBehaviour => _roleBehaviour;

    public void RotateToTarget(Vector3 targetPosition) {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }


    public void ChangeNickname() {
        // _botMonolog.ChangeNickname();
    }


    private bool _previousBotState;
    public void InitAnimator() {
        _botAnimator.InitAnimator(botWalkManager);
    }
    public void SetBotSkin(SkinItemConfig skinItemConfig) {
        SkinId = skinItemConfig.Id;
        _previousBotState = gameObject.activeSelf;
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
        gameObject.SetActive(_previousBotState);
    }


    public void EnableBot(bool state) {
        _agent.enabled = state;
        _agent.gameObject.SetActive(state);
    }
    

    public void SetBotSpeak() {
        _botMonolog.SaySomething();
    }

    public void SetBotStfu() {
        _botMonolog.Stfu();
    }

}