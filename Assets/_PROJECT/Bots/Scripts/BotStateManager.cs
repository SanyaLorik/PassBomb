using System.Collections;
using SanyaBeerExtension;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public enum BotState {
    Wandering
}

[RequireComponent(typeof(NavMeshAgent))]
public class BotStateManager : MonoBehaviour, IPassBombPlayer {
    [SerializeField] private Transform _skinParent;
    [SerializeField] private BotAnimator _botAnimator;
    [SerializeField] private GameObject _skinInstance;
    [SerializeField] private BotWander _botWander;
    [SerializeField] private BotMonolog _botMonolog;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private PlayerRoleBehaviour _roleBehaviour;
    
    private Vector3 _posBeforeTeleport;
    private IBotBehaviour _currentBotBehaviour;

    public bool IsPlaying { get; private set; }
    public string Nickname => _botMonolog.NickName;
    
    public BotState State { get; private set; }
    
    
    public string SkinId { get; private set; }
    
    [Inject] private GameData _gameData;

    private void Awake() {
        Destroy(_skinInstance);
    }
    
    
    private void Start() {
        ChangeBotState(BotState.Wandering);
    }
    
    
    public void SetPlayStatus(bool goPlay) {
        // if (goPlay) {
        //     _botMonolog.HideNickname();
        // }
        // else {
        //     _botMonolog.ShowNickname();
        // }
        // Debug.Log("SetPlayStatus: " + goPlay);
        // Debug.Log("_posBeforeTeleport: " + _posBeforeTeleport);
        IsPlaying = goPlay;
        if (goPlay) {
            _currentBotBehaviour?.Exit();
        }
        // Возвращение на спавн
        else {
            TeleportToPoint(_posBeforeTeleport);
            ChangeBotState(BotState.Wandering);
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
        if (!enable) {
            // Это фигня поидее не надо итак
            _currentBotBehaviour.Exit();
            // допом вырубать их движ
        }
    }

    public void SetBiggerSpeed(float speed) {
        _agent.speed = speed;
    }

    public void SetDefaultSpeed() {
        _agent.speed = _gameData.BotSpeed;
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


    public void ChangeBotState(BotState newState) {
        _currentBotBehaviour?.Exit();
        
        State = newState;
        _currentBotBehaviour = State switch {
            BotState.Wandering => _botWander,
            _ => _currentBotBehaviour
        };

        // Debug.Log(_currentBotBehaviour);
        _currentBotBehaviour?.Enter();
    }
    
    public void ChangeNickname() {
        // _botMonolog.ChangeNickname();
    }


    private bool _previousBotState;
    public void InitAnimator() {
        _botAnimator.InitAnimator(_botWander);
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