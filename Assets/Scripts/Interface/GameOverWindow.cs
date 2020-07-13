using UnityEngine;
using System.Collections;
using TMPro;

public class GameOverWindow : ScalledWindow
{
    private void Start()
    {
        GameLogic.Instance.OnGameOver += GameOver;

        Init();
    }

    private void OnDisable()
    {
        GameLogic.Instance.OnGameOver -= GameOver;
    }

    public void GameOver()
    {
        Request();
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
        GameLogic.Instance.ReloadTable();
    }
}
