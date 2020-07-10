using UnityEngine;

[CreateAssetMenu(fileName = "NewTableInfo", menuName = "Table/TableInfo", order = 1)]
public class TableInfo : ScriptableObject
{
    public int minCost, maxCost, winCost, moneyReward;
    [HideInInspector]public int fieldSize = 6;
}
