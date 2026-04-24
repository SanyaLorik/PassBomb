using System;
using UnityEngine;

[Serializable]
public class SpeedBonus : IBonus {
    public void Use(IPassBombPlayer passBombPlayer) {
        passBombPlayer.SetBonusSpeed();
        // Debug.Log("Включена суперскорость");
    }

    public void StopWork(IPassBombPlayer passBombPlayer) {
        passBombPlayer.SetDefaultSpeed();
        // Debug.Log("Суперскорость выключена");
    }
}