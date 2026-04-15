using UnityEngine;

public interface IGamePlayer {
    public void TpInPoint(Vector3 pos);
    public void RotateToTarget(Vector3 targetPosition);
    public void SetPlayStatus(bool goPlay);

    public bool IsPlaying { get; }
}