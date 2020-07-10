using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellLightning : MonoBehaviour
{
    [SerializeField] private Color _selectedRow = Color.green, _selectedCell = Color.green;
    //[SerializeField] private SpriteRenderer[] _cells = null;
    [SerializeField] private GameObject _cellPrefab = null;

    private SpriteRenderer[,] _cells = null;
    private int TableSize { get => GameLogic.Instance.TableSize; }
    private int FieldSize { get => GameLogic.Instance.FieldSize; }

    private void OnEnable()
    {
        GameLogic.Instance.OnTableInstantiated += Init;
        GameLogic.Instance.OnUnitInstantiated += SetLight;
        GameLogic.Instance.OnUnitScrolled += SetLight;
        GameLogic.Instance.OnUnitLaunched += LightOff;
    }

    private void OnDisable()
    {
        GameLogic.Instance.OnTableInstantiated -= Init;
        GameLogic.Instance.OnUnitInstantiated -= SetLight;
        GameLogic.Instance.OnUnitScrolled -= SetLight;
        GameLogic.Instance.OnUnitScrolled -= LightOff;
    }

    private void Init(TableInfo info)
    {
        _cells = new SpriteRenderer[info.fieldSize, info.fieldSize];
        for (int i = 0; i < _cells.Length; i++)
        {
            int x = i % info.fieldSize, y = i / info.fieldSize;
            _cells[x, y] = Instantiate(_cellPrefab, transform).GetComponent<SpriteRenderer>();
            _cells[x, y].color = _selectedRow;
            Vector3 pos = new Vector3(x - (info.fieldSize - 1) / 2f, y - (info.fieldSize - 1) / 2f);
            _cells[x, y].transform.position = pos;
            _cells[x, y].enabled = false;
        }
    }

    private void SetLight(Unit unit)
    {
        LightOff();

        int tableOffset = ((TableSize - FieldSize) / 2);
        int selectedX = GameLogic.Instance.GetFirstPoint(unit).X - tableOffset;
        int selectedY = unit.Distance - tableOffset + 1;

        for(int y = 0; y <= selectedY; y++)
        {
            _cells[selectedX, y].enabled = true;
            _cells[selectedX, y].color = _selectedRow;
        }

        _cells[selectedX, selectedY].color = _selectedCell;
        _cells[selectedX, selectedY - 1].color = _selectedCell;
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
