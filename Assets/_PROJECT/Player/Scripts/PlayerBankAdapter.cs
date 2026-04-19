using MediaKit_M.SkinChanger;
using UnityEngine;

public class PlayerBankAdapter : PurchaseAdapter
{
    [SerializeField] private PlayerBank _playerBank;

    public override bool CanSpend(int money)
    {
        return _playerBank.CanBuy(money);
    }

    public override void Spend(int money)
    {
        _playerBank.SpendMoney(money);
    }
}