using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmbientSkies
{
    [CustomEditor(typeof(RealtimeEmissionSetup))]
    public class RealtimeEmissionSetupEditor : Editor
    {
        private GUIStyle m_boxStyle;

        List<Material> emissiveMaterials = new List<Material>();
        List<Renderer> sceneRenders = new List<Renderer>();

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
            RealtimeEmissionSetup realtimeEmission = (RealtimeEmissionSetup)target;

            //Monitor for changes
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("Global Settings", m_boxStyle);
            GUILayout.Space(20);

            bool syncTOD = realtimeEmission.m_syncToASTimeOfDay;
            syncTOD = EditorGUILayout.Toggle("Sync To Time Of Day", syncTOD);

            KeyCode overwriteActivation = realtimeEmission.m_emissionOverwriteActivation;
            overwriteActivation = (KeyCode)EditorGUILayout.EnumPopup("Emission Overwrite Key", overwriteActivation);

            sceneRenders = realtimeEmission.m_sceneRenderers;
            int sceneRendersCount = Mathf.Max(0, EditorGUILayout.IntField("Scene Renders", sceneRenders.Count));
            while (sceneRendersCount < sceneRenders.Count)
            {
                sceneRenders.RemoveAt(sceneRenders.Count - 1);
            }

            while (sceneRendersCount > sceneRenders.Count)
            {
                sceneRenders.Add(null);
            }

            for (int i = 0; i < sceneRenders.Count; i++)
            {
                sceneRenders[i] = (Renderer)EditorGUILayout.ObjectField(sceneRenders[i], typeof(Renderer), true);
            }

            emissiveMaterials = realtimeEmission.m_emissiveMaterials;
            int emissiveMaterialsCount = Mathf.Max(0, EditorGUILayout.IntField("Emissive Mateirals", emissiveMaterials.Count));
            while (emissiveMaterialsCount < emissiveMaterials.Count)
            {
                emissiveMaterials.RemoveAt(emissiveMaterials.Count - 1);
            }

            while (emissiveMaterialsCount > emissiveMaterials.Count)
            {
                emissiveMaterials.Add(null);
            }

            for (int i = 0; i < emissiveMaterials.Count; i++)
            {
                emissiveMaterials[i] = (Material)EditorGUILayout.ObjectField(emissiveMaterials[i], typeof(Material), true);
            }

            GUILayout.EndVertical();

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(realtimeEmission, "Made changes");
                EditorUtility.SetDirty(realtimeEmission);

                realtimeEmission.m_syncToASTimeOfDay = syncTOD;
                realtimeEmission.m_emissionOverwriteActivation = overwriteActivation;
                realtimeEmission.m_emissiveMaterials = emissiveMaterials;
                realtimeEmission.m_sceneRenderers = sceneRenders;
            }
        }
    }
}