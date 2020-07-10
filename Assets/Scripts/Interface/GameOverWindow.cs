using UnityEngine;
using System.Collections;
using TMPro;

public class GameOverWindow : DropDownWindow
{
    [SerializeField] private TextMeshProUGUI _headerText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    private void Start()
    {
        GameLogic.Instance.OnGameOver += Win;

        Init();
    }

    private void OnDisable()
    {
        GameLogic.Instance.OnGameOver -= Win;
    }

    public void Win()
    {
        Request();
        _headerText.text = $"GAme Over";
        _descriptionText.text = $"Tap to\nReload table";
    }

    public void Rewarding()
    {
        Next();
        GameLogic.Instance.ReloadTable();
    }
}
