//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmbientSkies
{
    [CustomEditor(typeof(HorizonSky))]
    public class HorizonSkyEditor : Editor
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

            //Get Horizon Sky object
            HorizonSky horizonSky = (HorizonSky)target;

            //Monitor for changes
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("Horizon Sky Configuration", m_boxStyle);
            GUILayout.Space(20);
            bool showOptions = m_showOptions;
            showOptions = EditorGUILayout.Toggle("Show Options", showOptions);

            float positionUpdate = horizonSky.m_positionUpdate;
            bool followCamera = horizonSky.m_followsCameraPosition;

            if (showOptions)
            {
                positionUpdate = EditorGUILayout.FloatField("Position Update Time", positionUpdate);
                followCamera = EditorGUILayout.Toggle("Follow Camera", followCamera);
            }
            GUILayout.EndVertical();

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(horizonSky, "Made camera culling changes");
                EditorUtility.SetDirty(horizonSky);

                m_showOptions = showOptions;
                horizonSky.m_positionUpdate = positionUpdate;
                horizonSky.m_followsCameraPosition = followCamera;
            }
        }
    }
}