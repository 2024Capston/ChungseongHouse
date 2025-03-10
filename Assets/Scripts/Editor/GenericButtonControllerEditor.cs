using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenericButtonController)), CanEditMultipleObjects]
public class GenericButtonControllerEditor : Editor
{
    public SerializedProperty _color, _buttonType, _temporaryCooldown, _requiresBoth, _detectionRadius, _activatables, _events;

    private void OnEnable()
    {
        _color = serializedObject.FindProperty("_color");
        _buttonType = serializedObject.FindProperty("_buttonType");
        _temporaryCooldown = serializedObject.FindProperty("_temporaryCooldown");
        _requiresBoth = serializedObject.FindProperty("_requiresBoth");
        _detectionRadius = serializedObject.FindProperty("_detectionRadius");
        _activatables = serializedObject.FindProperty("_activatables");
        _events = serializedObject.FindProperty("_events");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_color);
        EditorGUILayout.PropertyField(_buttonType);

        if (_buttonType.enumValueIndex == (int)ButtonType.Temporary)
        {
            EditorGUILayout.PropertyField(_temporaryCooldown);
        }

        EditorGUILayout.PropertyField(_requiresBoth);
        if (_requiresBoth.boolValue)
        {
            EditorGUILayout.PropertyField(_detectionRadius);
        }

        EditorGUILayout.PropertyField(_activatables);

        EditorGUILayout.PropertyField(_events);

        serializedObject.ApplyModifiedProperties();
    }
}
