using UnityEngine;

[System.Serializable]
public class BoolGrid
{
    public int rows;
    public int columns;
    public bool[] values;

    public BoolGrid(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        values = new bool[rows * columns];
    }

    public bool GetValue(int row, int column)
    {
        return values[row * columns + column];
    }

    public void SetValue(int row, int column, bool value)
    {
        values[row * columns + column] = value;
    }
}
