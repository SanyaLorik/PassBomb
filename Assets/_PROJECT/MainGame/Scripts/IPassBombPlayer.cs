using UnityEngine;


public interface IPassBombPlayer {
    public void SetPlayStatus(bool goPlay);
    public void TeleportToPoint(Vector3 point);
    public void SetMovingStatus(bool enable);
    public void SetBiggerSpeed(float speed);
    public void SetDefaultSpeed();
    public MainGameRoleBehaviour RoleBehaviour();
}