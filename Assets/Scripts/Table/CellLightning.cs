using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellLightning : MonoBehaviour
{
    [SerializeField] private Color _selectedRow, _selectedCell;
    [SerializeField] private SpriteRenderer[] _cells;

    private int TableSize { get => GameLogic.Instance.TableSize; }
    private int FieldSize { get => GameLogic.Instance.FieldSize; }

    private void OnEnable()
    {
        GameLogic.Instance.OnUnitInstantiated += SetLight;
        GameLogic.Instance.OnUnitScrolled += SetLight;
        GameLogic.Instance.OnUnitLaunched += LightOff;
    }

    private void OnDisable()
    {
        GameLogic.Instance.OnUnitInstantiated -= SetLight;
        GameLogic.Instance.OnUnitScrolled -= SetLight;
        GameLogic.Instance.OnUnitScrolled -= LightOff;
    }

    private void SetLight(Unit unit)
    {
        LightOff();

        int tableOffset = ((TableSize - FieldSize) / 2);
        int selectedX = GameLogic.Instance.GetFirstPoint(unit).X - tableOffset;
        int selectedY1 = unit.Distance - tableOffset + 1, selectedY2 = unit.Distance - tableOffset;

        for(int i = 0; i < FieldSize; i++)
        {
            int id = i + selectedY1 * FieldSize;
            _cells[id].enabled = true;
            _cells[id].color = _selectedRow;
            id = i + selectedY2 * FieldSize;
            _cells[id].enabled = true;
            _cells[id].color = _selectedRow;
        }

        _cells[selectedX + selectedY1 * FieldSize].color = _selectedCell;
        _cells[selectedX + selectedY2 * FieldSize].color = _selectedCell;
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
