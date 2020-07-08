using UnityEngine;
using System.Collections;
using TMPro;

public class LevelUpWindow : DropDownWindow
{
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _rewardText;

    private int _levelReward = 0;

    private void Start()
    {
        Player.Instance.onChangeLevel += LevelUp;

        Init();
    }

    private void OnDisable()
    {
        Player.Instance.onChangeLevel -= LevelUp;
    }

    public void LevelUp(int level)
    {
        Visible = true;
        _levelReward = Player.Instance.GetLevel(level - 1).moneyReward;
        _levelText.text = $" New Level {level}";
        _rewardText.text = $" Money reward\n{_levelReward}$";
    }

    public void Rewarding()
    {
        Visible = false;
        MoneyCounter moneyCounter = FindObjectOfType<MoneyCounter>();
        moneyCounter.ChangeCounter(Player.Instance.Money + _levelReward, _rewardText.transform.position);
    }
}
