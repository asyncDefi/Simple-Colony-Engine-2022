using System;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Progress
{
    [SerializeField] private float _goal;
    [SerializeField] private float _value;

    public Progress(float goal, float init = 0)
    {
        _goal = Mathf.Max(0, goal);
        _value = Mathf.Clamp(init, 0, _goal);
    }


    public float Goal
    {
        get => _goal;
    }

    public float Value
    {
        get => _value;
        set
        {
            _value = Mathf.Clamp(value, 0, _goal);
            OnChanged?.Invoke();

            if (_value >= _goal)
                OnCompleted?.Invoke();
        }
    }

    public event Action OnChanged;
    public event Action OnCompleted;


    public float Percentage => _goal > 0 ? (_value / _goal) * 100f : 0f;
    public float NormalizedValue => _goal > 0 ? _value / _goal : 0f;

    public bool IsComplete => _value >= _goal;
    public float Remaining => Mathf.Max(0, _goal - _value);


    public void SetValue(float value)
    {
        _value = value;
    }
    public void SetGoal(float value)
    {
        _goal = value;
    }

    public void Add(float amount)
    {
        Value += amount;
    }
    public void Subtract(float amount)
    {
        Value -= amount;
    }

    public void SetComplete()
    {
        _value = _goal;
    }

    public void Reset()
    {
        _value = 0;
    }
    public void Annul()
    {
        _value = 0;
        _goal = 0;
    }

    public override string ToString()
    {
        return $"{_value:F1}/{_goal:F1} ({Percentage:F1}%)";
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Progress))]
public class ProgressDrawer : PropertyDrawer
{
    // Design Constants
    private const float FieldWidth = 50f;
    private const float Spacing = 4f;
    private readonly Color _backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);
    private readonly Color _fillColor = new Color(0.25f, 0.65f, 0.85f, 1f); // Soft Blue

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 1. Get Properties
        SerializedProperty valueProp = property.FindPropertyRelative("_value");
        SerializedProperty goalProp = property.FindPropertyRelative("_goal");

        float current = valueProp.floatValue;
        float max = goalProp.floatValue;

        // 2. Draw Label
        Rect contentRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // 3. Calculate Layout Rects
        // Structure: [Value Field] [ -- Visual Bar -- ] [Goal Field]
        Rect valRect = new Rect(contentRect.x, contentRect.y, FieldWidth, contentRect.height);

        Rect goalRect = new Rect(contentRect.x + contentRect.width - FieldWidth, contentRect.y, FieldWidth, contentRect.height);

        Rect barRect = new Rect(
            contentRect.x + FieldWidth + Spacing,
            contentRect.y,
            contentRect.width - (FieldWidth * 2) - (Spacing * 2),
            contentRect.height
        );

        // 4. Draw Input Fields (Editable)
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        EditorGUI.BeginChangeCheck();
        float newVal = EditorGUI.FloatField(valRect, current);
        if (EditorGUI.EndChangeCheck())
        {
            valueProp.floatValue = Mathf.Clamp(newVal, 0, max);
        }

        EditorGUI.BeginChangeCheck();
        float newGoal = EditorGUI.FloatField(goalRect, max);
        if (EditorGUI.EndChangeCheck())
        {
            goalProp.floatValue = Mathf.Max(0, newGoal);
        }

        // 5. Draw Visual Bar (No Interaction)
        DrawBar(barRect, current, max);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    private void DrawBar(Rect rect, float current, float max)
    {
        if (max <= 0) return;

        // Note: I removed the Event.current logic here. 
        // The bar is now purely visual and ignores mouse clicks.

        float fillPercent = Mathf.Clamp01(current / max);

        // Draw Background
        EditorGUI.DrawRect(rect, _backgroundColor);

        // Draw Fill
        Rect fillRect = new Rect(rect.x, rect.y, rect.width * fillPercent, rect.height);
        EditorGUI.DrawRect(fillRect, _fillColor);

        // Draw Text Overlay
        string overlayText = $"{current:0.##}/{max:0.##}";

        GUIStyle textStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        // Draw Shadow (for readability)
        Rect shadowRect = new Rect(rect);
        shadowRect.x += 1f;
        shadowRect.y += 1f;

        GUIStyle shadowStyle = new GUIStyle(textStyle);
        shadowStyle.normal.textColor = new Color(0, 0, 0, 0.6f);

        GUI.Label(shadowRect, overlayText, shadowStyle);
        GUI.Label(rect, overlayText, textStyle);
    }
}
#endif