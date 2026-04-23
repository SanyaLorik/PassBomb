using System;
using SanyaBeerExtension;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class GameData : GameDataBase
{
    [field: Header("Player")]
    [field: SerializeField] public float WalkSpeed { get; private set; }
    [field: SerializeField] public float JumpForce { get; private set; }
    [field: SerializeField] public float SecondJumpForce { get; private set; }
    [field: SerializeField] public float RotateSpeed { get; private set; }
    [field: SerializeField] public float GravityScale { get; private set; }
    [field: SerializeField] public int InitBonusCounts { get; private set; }
    
    
    [field: Header("Camera")]
    [field: Header("Дефолтные значения в процентах")]
    [field: SerializeField, Range(0,1)] public float MobileCameraFov { get; private set; }
    [field: SerializeField, Range(0,1)] public float DesktopCameraFov { get; private set; }
    [field: SerializeField, Range(0,1)] public float DefaultCameraSens { get; private set; }
    [field: SerializeField, Range(0,1)] public float ZoomSpeed { get; private set; }
    
    [field: Header("Множители сенсы")]
    [field: SerializeField] public float JoystickSensivityMultiplier  { get; private set; }
    [field: SerializeField] public float MouseSensivityMultiplier { get; private set; }
    
    [field: Header("Ограничители")]
    [field: SerializeField] public PairedValue<float> ZoomDiapasone  { get; private set; }
    [field: SerializeField] public float MinSensValue  { get; private set; }
    
    

    [field: Header("Movement")]
    [field: SerializeField] public float RotationSpeed { get; private set; }
    [field: SerializeField, Range(0,1)] public float ChanceToJump { get; private set; }
    [field: SerializeField] public PairedValue<float> TimeToStayOnPoint { get; private set; }
    
    [field: Header("Главная Игра")]
    [field: SerializeField] public float TimeToBombExplode { get; private set; }
    [field: SerializeField] public float TimeToInvinsibleAfterPass { get; private set; }
    [field: SerializeField] public float DefaultSpeedInRound { get; private set; }
    [field: SerializeField] public float HunterSpeed { get; private set; }
    [field: SerializeField] public float VelocityBonusSpeed { get; private set; }
    [field: SerializeField] public float JumpBonusHeight { get; private set; }
    [field: SerializeField] public float DoubleJumpBonusHeight { get; private set; }
    [field: SerializeField] public float BonusReload { get; private set; }
    [field: SerializeField] public float BonusDuration { get; private set; }
    
    [field: Header("Тайминги")]
    [field: SerializeField] public float ColldownToStartGame { get; private set; }
    [field: SerializeField] public float TimeAfterBombExplode { get; private set; }
    [field: Header("Птенцы")]
    [field: SerializeField] public int MaxPetsCount { get; private set; }
    [field: SerializeField] public PairedValue<int> BotPetCountDiapasone { get; private set; }
    
    
    [field: Header("БОТЫ")]
    [field: SerializeField] public float BotSpeed { get; private set; }
    [field: SerializeField] public PairedValue<int> CountSpeakingBotsPerTime  { get; private set; }
    [field: SerializeField] public PairedValue<float> TimeToSpeak { get; private set; }
    [field: SerializeField] public PairedValue<float> TimeToStayAfterSpawn { get; private set; }
    
    [field: Header("Боты в игре")]
    [field: SerializeField] public float DistanceToFloor { get; private set; }
    [field: SerializeField] public float DurationToHuntWithoutCheck { get; private set; }
    [field: SerializeField] public float DurationToGoInPoint { get; private set; }
    [field: SerializeField] public float RunStoppingDistance { get; private set; }
    [field: SerializeField] public float BotJumpDuration { get; private set; }
    [field: SerializeField] public float BotJumpBonusDuration { get; private set; }
    [field: SerializeField] public float BotDefaultJumpHeight { get; private set; }
    [field: SerializeField] public float BotJumpBonusHeight { get; private set; }
    [field: SerializeField, Range(0,1)] public float ChanceToGoPlayerInHunt { get; private set; }
    [field: SerializeField] public PairedValue<float> BotUseNewBonusTime { get; private set; }
    
}