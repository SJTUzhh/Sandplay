//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AmbientSkies
{
    public class AmbientSkiesProfilesList : ScriptableObject
    {
        [Header("Profile List")]
        public List<AmbientSkyProfiles> m_skyProfiles = new List<AmbientSkyProfiles>();

        /// <summary>
        /// Create sky profile asset
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Ambient Skies/Sky Profiles List")]
        public static void CreateSkyProfiles()
        {
            AmbientSkiesProfilesList asset = ScriptableObject.CreateInstance<AmbientSkiesProfilesList>();
            AssetDatabase.CreateAsset(asset, "Assets/Ambient Skies Profile List.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
#endif
    }
}