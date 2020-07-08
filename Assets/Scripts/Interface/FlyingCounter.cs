using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FlyingCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counter = null;
    [SerializeField] private string _prefix = "";
    [SerializeField] private string _suffix = "";

    public UnityAction onComplete = null;

    public string Text
    {
        get => _counter.text;
        set => _counter.text = string.Format("{0}{1}{2}", _prefix, value, _suffix);
    }

    public void Fly(Vector3 start, Vector3 end, float duration)
    {
        StartCoroutine(FlyFromTo(start, end, duration));
    }

    private IEnumerator FlyFromTo(Vector3 start, Vector3 end, float duration)
    {
        float timer = 0f;
        transform.position = start;

        while (timer <= duration)
        {
            yield return null;
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, timer / duration);
        }

        onComplete?.Invoke();
        Destroy(gameObject);
    }
}