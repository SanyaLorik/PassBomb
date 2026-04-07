using Architecture_M;
using LuringPlayer_M;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Architecture_M/Localization/Localization Data")]
public class LocalizationData : LocalizationDataBase,
    IDailyRewardLocalization
{
    [Header("Для  больших чисел")]
    public string[] Suffixies = {"", "K", "M", "B", "T", "Кв", "Ка"};

    public string[] BotsPhrases;

    public string Timer;
    public string Enemy;
    // Battle info
    public string PlayerHit;
    public string PlayerWinner;
    
    public DailyRewardLocaliation DailyReward;

    DailyRewardLocaliation IDailyRewardLocalization.DailyReward => DailyReward;
}