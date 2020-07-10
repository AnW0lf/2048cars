using UnityEngine;
using System.Collections;
using TMPro;

public class WinWindow : DropDownWindow
{
    [SerializeField] private TextMeshProUGUI _tableText = null;
    [SerializeField] private TextMeshProUGUI _rewardText = null;

    private int _tableReward = 0;

    private void Start()
    {
        GameLogic.Instance.OnGameWin += Win;

        Init();
    }

    private void OnDisable()
    {
        GameLogic.Instance.OnGameWin -= Win;
    }

    public void Win()
    {
        Request();
        _tableReward = GameLogic.Instance.GetTableInfo(Player.Instance.TableNumber).moneyReward;
        _tableText.text = $"Table {Player.Instance.TableNumber + 1} Complete";
        _rewardText.text = $" Money reward\n{_tableReward}$";
    }

    public void Rewarding()
    {
        Next();
        MoneyCounter moneyCounter = FindObjectOfType<MoneyCounter>();
        moneyCounter.ChangeCounter(Player.Instance.Money + _tableReward, _rewardText.transform.position);

        GameLogic.Instance.NextTable();
    }
}
