using UnityEngine;


public interface IPassBombPlayer {
    public void SetPlayStatus(bool goPlay);
    public void TeleportToPoint(Vector3 point);
    public void RotateToTarget(Vector3 point);
    public void SetMovingStatus(bool enable);
    public void SetDefaultRoundSpeed();
    public void SetHunterSpeed();
    public void SetBonusSpeed();
    public void SetDefaultSpeed();
    public void SetBigJump(bool state);
    public void SetInvinsible(bool invnincible);
    public PlayerRoleBehaviour RoleBehaviour { get; }
}