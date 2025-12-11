using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ButtonAttributeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var monoBehaviour = target as MonoBehaviour;
        if (monoBehaviour == null) return;

        var methods = monoBehaviour.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            var attributes = method.GetCustomAttributes(typeof(ButtonAttribute), true);
            if (attributes.Length > 0)
            {
                if (GUILayout.Button(method.Name))
                {
                    method.Invoke(monoBehaviour, null);
                }
            }
        }
    }
}