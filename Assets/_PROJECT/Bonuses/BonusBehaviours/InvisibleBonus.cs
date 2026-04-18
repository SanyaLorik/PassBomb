using System;
using UnityEngine;

[Serializable]
public class InvisibleBonus : IBonus {
    public void Use(IPassBombPlayer passBombPlayer) {
        passBombPlayer.SetInvinsible(true);
        Debug.Log("Включена невидимость");
    }

    public void StopWork(IPassBombPlayer passBombPlayer) {
        passBombPlayer.SetInvinsible(false);
        Debug.Log("Невидимость выключена");
    }
}