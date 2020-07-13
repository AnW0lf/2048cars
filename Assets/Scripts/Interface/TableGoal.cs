using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class TableGoal : MonoBehaviour
{
    [SerializeField] private GameObject effectPrefab = null;
    [SerializeField] private Image _shadowMin = null;
    [SerializeField] private Image _shadowMiddle = null;
    [SerializeField] private Image _shadowMax = null;
    [SerializeField] private TextMeshProUGUI _costMin = null;
    [SerializeField] private TextMeshProUGUI _costMiddle = null;
    [SerializeField] private TextMeshProUGUI _costMax = null;
    [SerializeField] private Sprite[] _skins = null;

    private GoalIcon[] goals;

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
            foreach (GoalIcon goal in goals) goal.OnUnlock += InstantiateEffect;

            goals[0].SetIcon(_info.winCostMin, GetSkin(_info.winCostMin));
            goals[1].SetIcon(_info.winCostMiddle, GetSkin(_info.winCostMiddle));
            goals[2].SetIcon(_info.winCostMax, GetSkin(_info.winCostMax));
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
            if (goal.Cost <= unit.Cost)
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
        foreach (GoalIcon goal in goals) goal.OnUnlock -= InstantiateEffect;
    }

    private void SetInfo(TableInfo info) => Info = info;

    private void InstantiateEffect(Transform container) => StartCoroutine(InstantiateEffectCoroutiner(container));

    private IEnumerator InstantiateEffectCoroutiner(Transform container)
    {
        GameObject effect = Instantiate(effectPrefab, container);
        yield return new WaitForSeconds(2f);
        Destroy(effect);
    }

    public void WinEffect()
    {
        foreach (GoalIcon goal in goals) InstantiateEffect(goal.transform);
    }

    private class GoalIcon
    {
        private Image _icon;
        private TextMeshProUGUI _counter;
        private bool _unlocked = false;

        public int Cost { get; private set; }
        public UnityAction<Transform> OnUnlock { get; set; }

        public Transform transform => _icon.transform;

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
                if (!_unlocked && value) OnUnlock?.Invoke(_icon.transform);
                _unlocked = value;
                _icon.color = _unlocked ? Color.white : Color.black;
                _counter.gameObject.SetActive(!_unlocked);
            }
        }
    }
}