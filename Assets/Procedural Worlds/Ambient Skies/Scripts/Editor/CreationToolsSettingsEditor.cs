//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEditor;
using AmbientSkies.Internal;

namespace AmbientSkies
{
    [CustomEditor(typeof(CreationToolsSettings))]
    public class CreationToolsSettingsEditor : Editor
    {
        private GUIStyle m_boxStyle;
        private bool m_lockValues = true;

        //Inspector GUI
        public override void OnInspectorGUI()
        {
            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }

            //Get Auto Focus Sky object
            CreationToolsSettings creation = (CreationToolsSettings)target;

            //Monitor for changess
            EditorGUI.BeginChangeCheck();

            //Locked values
            bool lockValues = m_lockValues;

            GUILayout.BeginVertical("Creation Tools Settings", m_boxStyle);
            GUILayout.Space(20);

            string version = PWApp.CONF.Version;

            EditorGUILayout.LabelField("Version: " + version);
            EditorGUILayout.Space();

            //Repairs the profile values
            if (GUILayout.Button("Repair Creation Tools Settings"))
            {
                creation.RepairScriptableObject();
            }

            //If not locked
            if (!lockValues)
            {
                //Button to lock the profile
                if (GUILayout.Button("Lock Settings"))
                {
                    lockValues = true;
                }
            }
            else
            {
                //Button to unlcok the profile
                if (GUILayout.Button("Unlock Settings"))
                {
                    lockValues = false;
                }
            }

            //Selected values
            int selectedSystem = creation.m_selectedSystem;
            int selectedHDRI = creation.m_selectedHDRI;
            int selectedProcedural = creation.m_selectedProcedural;
            int selectedGradient = creation.m_selectedGradient;
            int selectedPostProcessing = creation.m_selectedPostProcessing;
            int selectedLighting = creation.m_selectedLighting;
            //If not locked
            if (!lockValues)
            {
                EditorGUILayout.LabelField("Selected System: " + selectedSystem.ToString(), m_boxStyle);

                EditorGUILayout.LabelField("Selected HDRI Skybox: " + selectedHDRI.ToString(), m_boxStyle);
                EditorGUILayout.LabelField("Selected Procedural Skybox: " + selectedProcedural.ToString(), m_boxStyle);
                EditorGUILayout.LabelField("Selected Gradient Skybox: " + selectedGradient.ToString(), m_boxStyle);

                EditorGUILayout.LabelField("Selected Post Processing: " + selectedPostProcessing.ToString(), m_boxStyle);

                EditorGUILayout.LabelField("Selected Lighting: " + selectedLighting.ToString(), m_boxStyle);

            }

            GUILayout.EndVertical();

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(creation, "Made camera culling changes");
                EditorUtility.SetDirty(creation);

                m_lockValues = lockValues;
            }
        }
    }
}
#endif