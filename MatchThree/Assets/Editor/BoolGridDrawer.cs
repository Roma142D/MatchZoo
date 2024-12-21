using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BoolGrid))]
public class BoolGridDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Отримання властивостей
        SerializedProperty rowsProp = property.FindPropertyRelative("rows");
        SerializedProperty columnsProp = property.FindPropertyRelative("columns");
        SerializedProperty valuesProp = property.FindPropertyRelative("values");

        int rows = rowsProp.intValue;
        int columns = columnsProp.intValue;

        // Відображення розміру сітки
        Rect sizeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(sizeRect, label);
        position.y += EditorGUIUtility.singleLineHeight + 2;

        // Редагування кількості рядків і стовпців
        Rect rowsRect = new Rect(position.x, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
        Rect columnsRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);

        rowsProp.intValue = Mathf.Max(1, EditorGUI.IntField(rowsRect, "Rows", rows));
        columnsProp.intValue = Mathf.Max(1, EditorGUI.IntField(columnsRect, "Columns", columns));

        // Перевірка чи змінилась кількість елементів у сітці
        int newGridSize = rows * columns;
        if (valuesProp.arraySize != newGridSize)
        {
            valuesProp.arraySize = newGridSize;
        }

        position.y += EditorGUIUtility.singleLineHeight + 2;

        // Відображення сітки
        for (int row = 0; row < rows; row++)
        {
            Rect rowRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            float cellWidth = position.width / columns;

            for (int column = 0; column < columns; column++)
            {
                Rect cellRect = new Rect(position.x + column * cellWidth, position.y, cellWidth - 2, EditorGUIUtility.singleLineHeight);
                int index = row * columns + column;

                SerializedProperty cellProp = valuesProp.GetArrayElementAtIndex(index);
                cellProp.boolValue = EditorGUI.Toggle(cellRect, cellProp.boolValue);
            }

            position.y += EditorGUIUtility.singleLineHeight + 2;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty rowsProp = property.FindPropertyRelative("rows");
        int rows = rowsProp.intValue;

        // Висота залежить від кількості рядків + додаткові елементи
        return EditorGUIUtility.singleLineHeight * (2 + rows) + 4;
    }
}
