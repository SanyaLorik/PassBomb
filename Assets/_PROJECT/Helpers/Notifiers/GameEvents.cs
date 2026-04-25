using System;

public static class GameEvents {
    public static event Action<IBonus> BonusUsed;
    public static event Action BonusReloaded;
    public static event Action TriggerUsed;
    public static event Action ShakeCamera;
    public static event Action NewItemReceived;
    public static event Action<PlayerRoleBehaviour> PlayerStayHunter;
    public static event Action<PlayerRoleBehaviour> PlayerPassedBomb;


    public static void BonusReloadedInvoke() {
        BonusReloaded?.Invoke();
    }
    
    public static void BonusUseInvoke(IBonus bonus) {
        BonusUsed?.Invoke(bonus);
    }
    
    public static void TriggerUseInvoke() {
        TriggerUsed?.Invoke();
    }
    
    public static void ShakeCameraInvoke() {
        ShakeCamera?.Invoke();
    }
    
    public static void NewItemReceiveInvoke() {
        NewItemReceived?.Invoke();
    }

    public static void PlayerStayHunterInvoke(PlayerRoleBehaviour player) {
        PlayerStayHunter?.Invoke(player);
    } 
    
    public static void PlayerPassBombInvoke(PlayerRoleBehaviour player) {
        PlayerPassedBomb?.Invoke(player);
    } 
    
}
