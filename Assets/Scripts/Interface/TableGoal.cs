using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class TableGoal : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label = null;
    [SerializeField] private Image _shadowMin = null;
    [SerializeField] private Image _shadowMiddle = null;
    [SerializeField] private Image _shadowMax = null;
    [SerializeField] private TextMeshProUGUI _costMin = null;
    [SerializeField] private TextMeshProUGUI _costMiddle = null;
    [SerializeField] private TextMeshProUGUI _costMax = null;
    [SerializeField] private Sprite[] _skins = null;

    private GoalIcon[] goals;
    private int _stage = 1;

    private TableInfo _info;
    public TableInfo Info
    {
        get => _info;
        set
        {
            _info = value;

            goals = new GoalIcon[3];
            goals[0] = new GoalIcon(_shadowMin, _costMin);
            goals[1] = new GoalIcon(_shadowMiddle, _costMiddle);
            goals[2] = new GoalIcon(_shadowMax, _costMax);
            foreach (GoalIcon goal in goals)
            {
                goal.color = Color.black;
                goal.OnUnlock += UnlockEffect;
            }

            goals[0].SetIcon(_info.winCostMin, GetSkin(_info.winCostMin));
            goals[1].SetIcon(_info.winCostMiddle, GetSkin(_info.winCostMiddle));
            goals[2].SetIcon(_info.winCostMax, GetSkin(_info.winCostMax));
            _stage = 1;
            StartCoroutine(SetLabel($"Stage {Player.Instance.TableNumber + 1} Goals"));
        }
    }

    private Sprite GetSkin(int id) => _skins[Mathf.Clamp(id - 1, 0, _skins.Length - 1)];

    private void Awake()
    {
        GameLogic.Instance.OnTableInstantiated += SetInfo;
        GameLogic.Instance.OnMerge += UpdateGoals;
        GameLogic.Instance.OnGameWin += WinEffect;
    }

    private void UpdateGoals(Unit unit) => StartCoroutine(UpdateGoalsCoroutine(unit));

    private IEnumerator UpdateGoalsCoroutine(Unit unit)
    {
        foreach (GoalIcon goal in goals)
        {
            if (goal.Cost <= unit.Cost && !goal.IsUnlocked)
            {
                yield return new WaitForSeconds(0.8f);
                goal.IsUnlocked = true;
            }
        }
    }

    private void OnDisable()
    {
        GameLogic.Instance.OnTableInstantiated -= SetInfo;
        GameLogic.Instance.OnMerge -= UpdateGoals;
        GameLogic.Instance.OnGameWin -= WinEffect;
        foreach (GoalIcon goal in goals) goal.OnUnlock -= UnlockEffect;
    }

    private void SetInfo(TableInfo info) => Info = info;

    private void UnlockEffect(GoalIcon icon) => StartCoroutine(UnlockEffectCoroutiner(icon));

    private IEnumerator UnlockEffectCoroutiner(GoalIcon icon)
    {
        StartCoroutine(Scaling(icon.transform, Vector3.zero, 0.3f));
        yield return new WaitForSeconds(0.3f);

        icon.color = Color.white;

        StartCoroutine(Scaling(icon.transform, Vector3.one * 1.5f, 0.3f));
        yield return new WaitForSeconds(0.3f);

        StartCoroutine(Scaling(icon.transform, Vector3.one, 0.1f));
    }

    private IEnumerator Scaling(Transform target, Vector3 end, float duration)
    {
        Vector3 start;
        float timer;
        start = target.localScale;
        timer = 0f;
        while (timer <= duration)
        {
            timer += Time.deltaTime;
            target.localScale = Vector3.Lerp(start, end, timer / duration);
            yield return null;
        }
    }

    private IEnumerator SetLabel(string text)
    {
        Vector3 start, end;
        float timer, duration;

        start = _label.transform.localScale;
        end = Vector3.zero;
        timer = 0f;
        duration = 0.3f;

        while (timer <= duration)
        {
            timer += Time.deltaTime;
            _label.transform.localScale = Vector3.Lerp(start, end, timer / duration);
            yield return null;
        }

        _label.text = text;

        start = Vector3.zero;
        end = Vector3.one;
        timer = 0f;
        duration = 0.3f;

        while (timer <= duration)
        {
            timer += Time.deltaTime;
            _label.transform.localScale = Vector3.Lerp(start, end, timer / duration);
            yield return null;
        }
    }

    public void WinEffect() => StartCoroutine(WinEffectCoroutine());

    private IEnumerator WinEffectCoroutine()
    {
        yield return new WaitForSeconds(0.8f);
        //foreach (GoalIcon goal in goals) UnlockEffect(goal);
    }

    private class GoalIcon
    {
        private Image _icon;
        private TextMeshProUGUI _counter;
        private bool _unlocked = false;

        public int Cost { get; private set; }
        public UnityAction<GoalIcon> OnUnlock { get; set; }

        public Transform transform => _icon.transform;

        public Color color { get => _icon.color; set => _icon.color = value; }

        public GoalIcon(Image icon, TextMeshProUGUI counter)
        {
            _icon = icon;
            _counter = counter;
            IsUnlocked = false;
        }

        public void SetIcon(int cost, Sprite sprite)
        {
            Cost = cost;
            _icon.sprite = sprite;
            _counter.text = Mathf.Pow(2f, Cost).ToString();
        }

        public bool IsUnlocked
        {
            get => _unlocked;
            set
            {
                if (!_unlocked && value) OnUnlock?.Invoke(this);
                _unlocked = value;
                _counter.gameObject.SetActive(!_unlocked);
            }
        }
    }
}