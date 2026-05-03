using System;
using UnityEngine;

[Serializable]
public class InvisibleBonus : IBonus {
    public void Use(IPassBombPlayer passBombPlayer) {
        passBombPlayer.SetInvincible(true);
        // Debug.Log("Включена невидимость");
    }

    public void StopWork(IPassBombPlayer passBombPlayer) {
        passBombPlayer.SetInvincible(false);
        // Debug.Log("Невидимость выключена");
    }
}