using UnityEngine;

[CreateAssetMenu(fileName = "BonusItemConfig", menuName = "Configs/BonusItemConfig")]
public class BonusItemConfig : ScriptableObject {
    public string Id => Bonus.GetType().Name;
    [SerializeReference, SubclassSelector] public IBonus Bonus;
    [field: SerializeField] public int Price { get; private set; }
    
}