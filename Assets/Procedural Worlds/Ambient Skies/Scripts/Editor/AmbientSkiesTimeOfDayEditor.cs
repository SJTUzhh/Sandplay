using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmbientSkies
{
    [CustomEditor(typeof(AmbientSkiesTimeOfDay))]
    public class AmbientSkiesTimeOfDayEditor : Editor
    {
        private GUIStyle m_boxStyle;
        private bool m_showOptions;

        public override void OnInspectorGUI()
        {
            //Initialization

            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }

            //Get Time Of Day object
            AmbientSkiesTimeOfDay timeOfDay = (AmbientSkiesTimeOfDay)target;

            //Monitor for changes
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("Global Settings", m_boxStyle);
            GUILayout.Space(20);
            bool showOptions = m_showOptions;
            showOptions = EditorGUILayout.Toggle("Show Options", showOptions);
            AmbientSkiesTimeOfDayProfile todProfile = timeOfDay.m_timeOfDayProfile;
            if (showOptions)
            {
                todProfile = (AmbientSkiesTimeOfDayProfile)EditorGUILayout.ObjectField("Time Of Day Profile", todProfile, typeof(AmbientSkiesTimeOfDayProfile), true);
            }
            GUILayout.EndVertical();

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(todProfile, "Made camera culling changes");
                EditorUtility.SetDirty(todProfile);

                timeOfDay.m_timeOfDayProfile = todProfile;
                m_showOptions = showOptions;
            }
        }
    }
}