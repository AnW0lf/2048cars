using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class TableGoal : MonoBehaviour
{
    [SerializeField] private Image _shadow = null;
    [SerializeField] private TextMeshProUGUI _label = null;
    [SerializeField] private Sprite[] _skins = null;

    private TableInfo _info;
    public TableInfo Info
    {
        get => _info;
        set
        {
            _info = value;
            _shadow.color = Color.black;
            _shadow.sprite = _skins[Mathf.Clamp(_info.winCost - 1, 0, _skins.Length - 1)];
            _label.text = $"Table {Player.Instance.TableNumber + 1} Goal";
            print("Set new info");
        }
    }

    private void Start()
    {
        GameLogic.Instance.OnTableInstantiated += SetInfo;
    }

    private void OnDisable()
    {
        GameLogic.Instance.OnTableInstantiated -= SetInfo;
    }

    private void SetInfo(TableInfo info) => Info = info;
}
