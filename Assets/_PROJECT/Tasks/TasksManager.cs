using System;
using System.Collections.Generic;
using System.Linq;
using Architecture_M;
using LuringPlayer_M;
using MirraSDK_M;
using SanyaBeerExtension;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


[Serializable]
public enum TaskType {
   PassBomb,
   PassInVoid,
   LifeSec,
   LifeRound,
   WinCount,
   UseSpeedBonus,
   UseSuperJumpBonus,
   UseInvincibleBonus,
   ParkourCount,
}

[Serializable]
public class TaskInfo {
    public int Count;
    public int RewardMoney;
    public TaskType TaskType;
    public string TaskId;
}


public class TasksManager : MonoBehaviour {
    [Header("Набор заданий")]
    [SerializeField] private List<TaskInfo> _tasksInfo;
    
    [Header("Визуалы")]
    [SerializeField] private List<TaskVisual> _tasksVisual;    
    [Header("Визуалы")]
    [SerializeField] private GameObject _canvas;
    [SerializeField] private Button _openCanvasButton;
    [SerializeField] private Button _closeCanvasButton;
    [SerializeField] private Button _resetButton;

    [Header("Синглтоны")]
    [SerializeField] private ParkourCompleteTrigger _parkourCompleteTrigger;
    [SerializeField] private TaskCompleteCountView _taskCountView;
    [SerializeField] private DailyQuest _dailyQuest;
    
    
    
    // Инфа по заданию и росту
    private readonly Dictionary<string, TaskInfo> _taskIdToInfoDictionary = new ();
    private readonly Dictionary<string, TaskVisual> _taskVisualIdToViewDictionary = new ();
        
    
    // Стата игрока в данный момент 
    private int _passBomb;
    private int _passInVoid;
    private int _lifeRound;
    private int _winCount;
    private int _useSpeedBonus;
    private int _useSuperJumpBonus;
    private int _useInvincibleBonus;
    private int _parkourCount;

    private int _lifeSec;
    private DateTime _timeGoPlay;
    
    public event Action TaskComplete;
    private GameSave Saver => _gameSave.GetSave<GameSave>();

    
    [Inject] private PlayerBank _bank;
    [Inject] private NumberFormatter _formatter; 
    [Inject] private LocalizationData _localization; 
    [Inject] private BattleManager _battleManager; 
    [Inject] private MainGameStarter _mainGameStarter; 
    [Inject] private GameOverView _gameOverView; 
    [Inject] private IGameSave _gameSave; 
    [Inject] private AdvertisingMonetizationMirra _advertisingMonetization;
    [Inject] private AdvHelper _advHelper;
    [Inject] private FallVoidCollider _fallVoidCollider;
    [Inject] private PlayerMovement _mainPlayer;

    
    
    private void Start() {
        CreateTaskInfoDictionary();
        TableInitialize();
        if (_dailyQuest.IsTimePassed) {
            ResetCompletedTasks();
        }
        CheckTaskCount();
    }
    
    private void OnEnable() {
        _openCanvasButton.onClick.AddListener(() => _canvas.ActiveSelf());
        _closeCanvasButton.onClick.AddListener(() => _canvas.DisactiveSelf());
        _advHelper.AddToButtonAdvRewardListener(_resetButton, ResetCompletedTasks);
        _dailyQuest.OnTimerPassed += ResetCompletedTasks;
        
        // Подписка на тасочки
        _battleManager.MainPlayerWin += PlayerWinCheck;
        _parkourCompleteTrigger.ParkourCompleted += UpdateParkourTask;
        GameEvents.BonusUsed += OnBonusUsed;
        GameEvents.PlayerPassedBomb += OnPlayerPassedBomb;
        _fallVoidCollider.PlayerFalledInVoid += OnPlayerFalledInVoid;
        _battleManager.NewRoundStarted += OnNewRoundStarted;
        _mainPlayer.InitedToPlay += CalculatePlayerLifeTime;
    }

    
    private void CalculatePlayerLifeTime(bool goPlay) {
        if (goPlay) {
            _timeGoPlay =  DateTime.Now;
        }
        else {
            _lifeSec += DateTime.Now.Subtract(_timeGoPlay).Seconds;
            Debug.Log("Игрок сыграл " + _lifeSec);
            UpdateTaskProgress(TaskType.LifeSec);
        }
    }


    // Обновления --------------------------
    
    private void OnBonusUsed(IBonus bonus) {
        if (bonus is SpeedBonus) {
            _useSpeedBonus++;
            UpdateTaskProgress(TaskType.UseSpeedBonus);
        }
        else if (bonus is BigJumpBonus) {
            _useSuperJumpBonus++;
            UpdateTaskProgress(TaskType.UseSuperJumpBonus);
        }
        else if (bonus is InvisibleBonus) {
            _useInvincibleBonus++;
            UpdateTaskProgress(TaskType.UseInvincibleBonus);
        }
    }

    
    private void OnNewRoundStarted(int roundNumber) {
        if(roundNumber == 1 || !_battleManager.MainPlayerPlay) return;
        _lifeRound++;
        UpdateTaskProgress(TaskType.LifeRound);
    }
    
    
    private void OnPlayerPassedBomb(PlayerRoleBehaviour player) {
        if(player != _mainPlayer.RoleBehaviour) return;
        _passBomb++;
        UpdateTaskProgress(TaskType.PassBomb);
    }
    
    
    private void OnPlayerFalledInVoid(IPassBombPlayer bedolaga) {
        if (bedolaga.RoleBehaviour.LastPlayerContact == _mainPlayer.RoleBehaviour) {
            _passInVoid++;
            UpdateTaskProgress(TaskType.PassInVoid);
        }
    }
    
    private void PlayerWinCheck(bool winner) {
        if (winner) {
            _winCount++;
            UpdateTaskProgress(TaskType.WinCount);
            // Debug.Log($"Игрок выиграл {_winCount} раз");
        }
    }

    
    private void UpdateParkourTask() {
        _parkourCount++;
        UpdateTaskProgress(TaskType.ParkourCount);
    }

    
    
    // Логика работы --------------------------
    
    private void TableInitialize() {
        int iterator = 0;
        foreach (var taskInfoPair in _taskIdToInfoDictionary) {
            // Initialize
            TaskInfo taskInfo = taskInfoPair.Value;
            string taskId = taskInfo.TaskId;
            _taskVisualIdToViewDictionary[taskId].InitTask(taskId, taskInfo.TaskType);
            
            // Get save data
            TaskItem taskSaveInfo = Saver.GetTaskInfo(taskId);
            
            if (!taskSaveInfo.IsGetReward) {
                _taskVisualIdToViewDictionary[taskId].SetTaskVisual(taskInfo, taskSaveInfo.Count);
                if (taskSaveInfo.Count >= taskInfo.Count) {
                    _taskCountView.PlusOne();
                }
                SetPlayerValue(taskInfo.TaskType, taskSaveInfo.Count, taskInfo.TaskId);
            }
            else {
                // Debug.Log($"Задача {taskSaveInfo.Id} загрузилась как выполненная");
                _taskVisualIdToViewDictionary[taskId].DisableTask();
            }
        }
    }

    
    private void CreateTaskInfoDictionary() {
        int iterator = 0;
        foreach (var task in _tasksInfo) {
            if (_taskIdToInfoDictionary.ContainsKey(task.TaskId)) {
                // Debug.LogWarning($"Повтор ключа! {task.TaskType}");
                continue;
            }
            _taskIdToInfoDictionary[task.TaskId] = task;
            _taskVisualIdToViewDictionary[task.TaskId] = _tasksVisual[iterator++]; 
        }
    }
    

    private int GetPlayerValue(TaskType taskType) {
        return taskType switch {
            TaskType.PassBomb => _passBomb,
            TaskType.PassInVoid => _passInVoid,
            TaskType.LifeSec => _lifeSec,
            TaskType.LifeRound => _lifeRound,
            TaskType.WinCount => _winCount,
            TaskType.UseSpeedBonus => _useSpeedBonus,
            TaskType.UseSuperJumpBonus => _useSuperJumpBonus,
            TaskType.UseInvincibleBonus => _useInvincibleBonus,
            TaskType.ParkourCount => _parkourCount,
            _ => 0
        };

    }

    
    private void SetPlayerValue(TaskType taskType, int count, string id) {
        Debug.Log($"SetPlayerValue {id} {count} {false}");
        Saver.UpdateTaskInfo(id, count, false);
        switch (taskType) {
            case TaskType.PassBomb:
                _passBomb = count;
                break;
            case TaskType.PassInVoid:
                _passInVoid = count;
                break;
            case TaskType.LifeSec:
                _lifeSec = count;
                break;
            case TaskType.LifeRound:
                _lifeRound = count;
                break;
            case TaskType.WinCount:
                _winCount = count;
                break;
            case TaskType.UseSpeedBonus:
                _useSpeedBonus = count;
                break;  
            case TaskType.UseSuperJumpBonus:
                _useSuperJumpBonus = count;
                break;
            case TaskType.UseInvincibleBonus:
                _useInvincibleBonus = count;
                break;
            case TaskType.ParkourCount:
                _parkourCount = count;
                break;
        }
    }
    
    
    private void UpdateTaskProgress(TaskType type) {
        foreach (var taskVisualPair in _taskVisualIdToViewDictionary) {
            int currentValue = GetPlayerValue(type);
            TaskInfo taskInfo = _taskIdToInfoDictionary[taskVisualPair.Value.TaskId];
            
            if(taskVisualPair.Value.TaskType != type) continue;
            
            TaskVisual taskVisual = taskVisualPair.Value;
            
            if (taskVisual.TaskIsComplete) continue;
            
            Saver.UpdateTaskInfo(taskInfo.TaskId, currentValue, false );
            _gameSave.Save();
        
            if (currentValue >= taskInfo.Count && !taskVisual.TaskIsComplete) {
                taskVisual.SetTaskCompleteVisual(currentValue, taskInfo.Count);
                _taskCountView.PlusOne();
                ShowNotification(taskInfo);
            }
            else {
                taskVisual.UpdateTaskScoreVisual(currentValue, taskInfo.Count);
            }
            Saver.UpdateTaskInfo(taskInfo.TaskId, currentValue, false );
            _gameSave.Save();
        }
    }
    
    
    public void SetCompleteTask(string taskId) {
        // Обновляем данные
        TaskInfo taskInfo = _taskIdToInfoDictionary[taskId];
        Saver.UpdateTaskInfo(taskInfo.TaskId, taskInfo.Count, true);
        _bank.AddMoney(taskInfo.RewardMoney);
        _taskCountView.MinusOne();
        CheckTaskCount();
    }

    
    private void CheckTaskCount() {
        foreach (var taskVisual in _taskVisualIdToViewDictionary) {
            TaskInfo taskInfo = _taskIdToInfoDictionary[taskVisual.Value.TaskId];
            
            TaskItem taskSaveInfo = Saver.GetTaskInfo(taskInfo.TaskId);
            if(!taskSaveInfo.IsGetReward) return;
        }
        _dailyQuest.ShowAllDaliesDone();
    }

    private void ResetCompletedTasks() {
        _dailyQuest.ShowDailies();
        foreach (var taskVisual in _taskVisualIdToViewDictionary) {
            // Debug.Log(taskVisual.Value.TaskId);
            if (Saver.GetTaskInfo(taskVisual.Value.TaskId).IsGetReward) {
                TaskInfo taskInfo = _taskIdToInfoDictionary[taskVisual.Value.TaskId];
                SetPlayerValue(taskVisual.Value.TaskType, 0, taskVisual.Value.TaskId);
                taskVisual.Value.EnableTask(taskInfo);
            }
        }
        _gameSave.Save();
    }
    
    private void ShowNotification(TaskInfo taskInfo) {
        TaskComplete?.Invoke();
        // Debug.LogWarning("Таска выполнена!");
        // _taskNotification.ShowNotification("+"+ _formatter.ValuteFormatter(taskInfo.RewardMoney));
    }

}