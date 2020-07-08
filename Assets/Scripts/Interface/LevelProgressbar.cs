using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelProgressbar : MonoBehaviour
{
    [SerializeField] private Image _filler = null;
    [SerializeField] private TextMeshProUGUI _levelNumber = null;
    [SerializeField] private TextMeshProUGUI _moneyReward = null;

    public float Progress
    {
        get => _filler.fillAmount;
        set
        {
            float fill = Mathf.Clamp01(value);
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(FillProgress(fill));
        }
    }

    private Coroutine _coroutine = null;

    private IEnumerator FillProgress(float end)
    {
        float start = _filler.fillAmount;
        float timer = 0f, duration = 0.5f; //duration = Mathf.Abs(start - end);
        while(timer <= duration)
        {
            timer += Time.deltaTime;
            _filler.fillAmount = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }
        _coroutine = null;
    }

    private void Start()
    {
        SetLevel(Player.Instance.Level, Player.Instance.Experience);
        Player.Instance.onChangeLevel += LevelUp;
        Player.Instance.onChangeExperience += SetExperience;
    }

    private void OnDisable()
    {
        Player.Instance.onChangeLevel -= LevelUp;
        Player.Instance.onChangeExperience -= SetExperience;
    }

    private void LevelUp(int value)
    {
        SetLevel(value, Player.Instance.Experience);
    }

    private void SetLevel(int level, int experience)
    {
        _filler.fillAmount = 0f;
        Progress = (float)experience / Player.Instance.GetLevel(level).experience;
        _levelNumber.text = level.ToString();
        _moneyReward.text = Player.Instance.GetLevel(level).moneyReward.ToString();
    }

    private void SetExperience(int value)
    {
        Progress = (float)value / Player.Instance.CurrentLevel.experience;
    }
}
