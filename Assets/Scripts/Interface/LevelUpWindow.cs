using UnityEngine;
using System.Collections;
using TMPro;

public class LevelUpWindow : ScalledWindow
{
    [SerializeField] private TextMeshProUGUI _levelText = null;
    [SerializeField] private TextMeshProUGUI _rewardText = null;

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
        Request();
        _levelReward = Player.Instance.GetLevel(level - 1).moneyReward;
        _levelText.text = $" New Level {level}";
        _rewardText.text = $"{_levelReward}";
    }

    public void Click()
    {
        StartCoroutine(Closing());
    }

    private IEnumerator Closing()
    {
        Close();
        yield return new WaitForSeconds(_duration);
        Next();
        MoneyCounter moneyCounter = FindObjectOfType<MoneyCounter>();
        moneyCounter.ChangeCounter(Player.Instance.Money + _levelReward, _rewardText.transform.position);
    }
}
