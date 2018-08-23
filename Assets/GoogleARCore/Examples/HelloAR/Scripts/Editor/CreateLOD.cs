using GoogleARCore.Examples.Common;
using Reconstruction.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PointcloudVisualizer))]
public class CreateLOD : Editor {

    PointcloudVisualizer m_Target;
    SerializedProperty m_Property;

    public override void OnInspectorGUI() {
        m_Target = (PointcloudVisualizer) target;
        DrawDefaultInspector();
        DrawDivisionsInspector();

    }

    private void DrawDivisionsInspector() {
        GUILayout.Space(5);
        if (m_Target.GetDivisions().Count > 0) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Divisions", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label("Percent", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.EndHorizontal();

            for (int i = 0; i < m_Target.GetDivisions().Count; i++) {
                DrawLOD(i);
            }
        }

        DrawNewLODButton();
    }

    private void DrawLOD(int i) {
        if (i < 0 || i >= m_Target.GetDivisions().Count) return;

        SerializedProperty listIterator = serializedObject.FindProperty("Divisions");

        GUILayout.BeginHorizontal();
        GUILayout.Label("LOD-" + (i+1), GUILayout.Width(80));

        EditorGUI.BeginChangeCheck();

        int divisionNumber = EditorGUILayout.IntField(m_Target.GetDivisions()[i].numberOfDivisions, GUILayout.Width(100));
        float percentNumber = GUILayout.HorizontalSlider(m_Target.GetDivisions()[i].maxPercent, i == 0 ? 0.0F : m_Target.GetDivisions()[i - 1].maxPercent, 1F);
        float percent = EditorGUILayout.FloatField("", (float) Math.Round(percentNumber, 2), GUILayout.Width(80));

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(m_Target, "Modify Division");
            m_Target.Divisions[i] = new Division { numberOfDivisions = divisionNumber, maxPercent = percentNumber };
            EditorUtility.SetDirty(m_Target);
        }

        if (GUILayout.Button("Remove")) {
            Undo.RecordObject(m_Target, "Remove Division");
            EditorApplication.Beep();
            m_Target.RemoveDivisionByIndex(i);
            EditorUtility.SetDirty(m_Target);
        }
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawNewLODButton() {
        if (GUILayout.Button("New LOD")) {
            Undo.RecordObject(m_Target, "New Division");
            m_Target.CreateNewLOD();
            EditorUtility.SetDirty(m_Target);
        }
    }
}