using System;
using UnityEngine;

public enum MoveStatus {
    Default,
    SuperSpeed,
    SuperJump,
    Invincible,
}

public interface IPassBombPlayer {
    public void SetPlayStatus(bool goPlay);
    public void TeleportToPoint(Vector3 point);
    public void RotateToTarget(Vector3 point);
    public void SetMovingStatus(bool enable);
    public void SetDefaultSpeed();
    public void SetHunterSpeed();
    public void SetBonusSpeed();
    public void SetBigJump(bool state);
    public void SetInvinsible(bool invnincible);
    public void PushAway(Vector3 direction);
    public bool IsPlaying { get; }
    public event Action<MoveStatus, bool> MoveStatusChanged;

    public PlayerRoleBehaviour RoleBehaviour { get; }
    Transform Transform { get; }
}