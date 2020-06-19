//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AmbientSkies
{
    [CustomEditor(typeof(AutoDepthOfField))]
    public class AutoDepthOfFieldEditor : Editor
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

            //Get Auto Focus Sky object
            AutoDepthOfField autoFocus = (AutoDepthOfField)target;

            //Monitor for changes
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("Auto Focus Settings", m_boxStyle);
            GUILayout.Space(20);
            bool showOptions = m_showOptions;
            showOptions = EditorGUILayout.Toggle("Show Options", showOptions);

            AmbientSkiesConsts.DOFTrackingType trackingType = autoFocus.m_trackingType;
            GameObject targetObject = autoFocus.m_targetObject;
            Camera sourceCamera = autoFocus.m_sourceCamera;
            LayerMask targetLayer = autoFocus.m_targetLayer;

            float focusOffset = autoFocus.m_focusOffset;
            float maxFocusDistance = autoFocus.m_maxFocusDistance;
            float actualFocusDistance = autoFocus.m_actualFocusDistance;

            AmbientPostProcessingProfile postProcessProfile = autoFocus.m_processingProfile;

            GameObject postProcessingObject = GameObject.Find("Global Post Processing");

            if (showOptions)
            {
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Auto Focus Dependencies");
                GUILayout.Space(5);
                sourceCamera = (Camera)EditorGUILayout.ObjectField("Source Camera", sourceCamera, typeof(Camera), true);
                if (sourceCamera == null)
                {
                    EditorGUILayout.HelpBox("Source Camera is missing. Either add it manually or it will be added on start of application if present", MessageType.Error);
                }
                targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);
                if (targetObject == null)
                {
                    EditorGUILayout.HelpBox("Target Object is missing. Add it to use target focus mode", MessageType.Error);
                }
                targetLayer = LayerMaskField("Target Layer", targetLayer);

                GUILayout.Space(20);
                EditorGUILayout.LabelField("Auto Focus Configuration");
                GUILayout.Space(5);
                trackingType = (AmbientSkiesConsts.DOFTrackingType)EditorGUILayout.EnumPopup("Tracking Mode", trackingType);
                focusOffset = EditorGUILayout.FloatField("Focus Offset", focusOffset);
                maxFocusDistance = EditorGUILayout.FloatField("Max Focus Distance", maxFocusDistance);
                actualFocusDistance = EditorGUILayout.FloatField("Actual Focus Distance", actualFocusDistance);

                if (postProcessProfile == null || postProcessingObject == null)
                {
                    EditorGUILayout.HelpBox("Ambient Skies Post Processing Profile is not connected. Please load up Ambient Skies window and setup Post Processing", MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox("Ambient Skies Post Processing Profile is connected. You can now run the scene to use Auto Focus features", MessageType.Info);
                }
            }
            GUILayout.EndVertical();

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(autoFocus, "Made camera culling changes");
                EditorUtility.SetDirty(autoFocus);

                m_showOptions = showOptions;
                autoFocus.m_trackingType = trackingType;
                autoFocus.m_targetObject = targetObject;
                autoFocus.m_sourceCamera = sourceCamera;
                autoFocus.m_targetLayer = targetLayer;
                autoFocus.m_focusOffset = focusOffset;
                autoFocus.m_maxFocusDistance = maxFocusDistance;
            }
        }

        /// <summary>
        /// Handy layer mask interface
        /// </summary>
        /// <param name="label"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        private LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }
    }
}
#endif