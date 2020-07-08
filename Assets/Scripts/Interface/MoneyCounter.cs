using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyCounter : MonoBehaviour
{
    [SerializeField] private Image _icon = null;
    [SerializeField] private TextMeshProUGUI _counter = null;
    [SerializeField] private string _prefix = "";
    [SerializeField] private string _suffix = "";
    [SerializeField] private GameObject _flyingMoneyPrefab = null;

    private bool _visible;
    private Coroutine _changeAlpha = null;
    private float _speed = 1f;
    private int _value = 0;

    public bool Visible
    {
        get => _visible;
        set
        {
            if(_visible != value)
            {
                _visible = value;
                if (_changeAlpha != null) StopCoroutine(_changeAlpha);
                _changeAlpha = StartCoroutine(ChangeAlpha(value ? 1f : 0f, _speed));
            }
        }
    }
    public float Alpha
    {
        get => (_icon.color.a + _counter.color.a) / 2f;
        set
        {
            Color color = _icon.color;
            color.a = value;
            _icon.color = color;
            color = _counter.color;
            color.a = value;
            _counter.color = color;
        }
    }

    private void Start()
    {
        _value = Player.Instance.Money;
        SetCounter(_value);
    }

    private IEnumerator ChangeAlpha(float end, float speed)
    {
        float start = Alpha;
        float timer = 0f, duration = Mathf.Abs(end - start) / speed;

        while(timer <= duration)
        {
            timer += Time.deltaTime;
            Alpha = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }

        _changeAlpha = null;
    }

    public void SetCounter(int value)
    {
        _value = value;
        _counter.text = string.Format("{0}{1}{2}", _prefix, _value, _suffix);
    }

    public void ChangeCounter(int value, Vector3 start)
    {
        FlyingCounter flyingMoney = Instantiate(_flyingMoneyPrefab, transform).GetComponent<FlyingCounter>();
        flyingMoney.Text = (value - _value).ToString();
        flyingMoney.Fly(start, _icon.transform.position, 0.8f);
        flyingMoney.onComplete += () =>
        {
            Player.Instance.Money = value;
            SetCounter(value);
        };
    }
}
