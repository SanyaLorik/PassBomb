using System;
using UnityEngine;

[Serializable]
public class BigJumpBonus : IBonus {
    public void Use(IPassBombPlayer passBombPlayer) {
        passBombPlayer.SetBigJump(true);
        // Debug.Log("Включен супер прыжок");
    }

    public void StopWork(IPassBombPlayer passBombPlayer) {
        passBombPlayer.SetBigJump(false);
        // Debug.Log("Выключен супер прыжок");
    }
}