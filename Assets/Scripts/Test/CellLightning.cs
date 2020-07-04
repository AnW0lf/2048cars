using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellLightning : MonoBehaviour
{
    [SerializeField] private Table _table;
    [SerializeField] private Color _selectedRow, _selectedCell;
    [SerializeField] private SpriteRenderer[] _cells;

    private int _size;

    private void Awake()
    {
        _size = (int)Mathf.Sqrt(_cells.Length);
    }

    private void OnEnable()
    {
        _table.OnUnitInstantiated += SetLight;
        _table.OnUnitScrolled += SetLight;
        _table.OnUnitLaunched += LightOff;
    }

    private void OnDisable()
    {
        _table.OnUnitInstantiated -= SetLight;
        _table.OnUnitScrolled -= SetLight;
        _table.OnUnitScrolled -= LightOff;
    }

    private void SetLight(Unit unit)
    {
        LightOff();

        int selectedX = _table.GetFirstPoint(unit).X - 2;
        int selectedY1 = unit.Distance - 1, selectedY2 = unit.Distance - 2;

        for(int i = 0; i < _size; i++)
        {
            int id = i + selectedY1 * _size;
            _cells[id].enabled = true;
            _cells[id].color = _selectedRow;
            id = i + selectedY2 * _size;
            _cells[id].enabled = true;
            _cells[id].color = _selectedRow;
        }

        _cells[selectedX + selectedY1 * _size].color = _selectedCell;
        _cells[selectedX + selectedY2 * _size].color = _selectedCell;
    }

    private void LightOff()
    {
        foreach (var cell in _cells) cell.enabled = false;
    }

    private void LightOff(Unit unit)
    {
        LightOff();
    }
}
