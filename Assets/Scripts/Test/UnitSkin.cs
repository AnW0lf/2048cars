using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Unit))]
public class UnitSkin : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private TextMesh _text;
    [SerializeField] private Sprite[] skins;
    private Unit _unit;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _unit.onCostChanged += SetSkin;
        SetSkin(_unit.Cost);
    }

    private void SetSkin(int cost)
    {
        _renderer.sprite = skins[Mathf.Clamp(cost - 1, 0, skins.Length - 1)];
        _text.text = (Mathf.Pow(2, cost)).ToString();
    }
}
