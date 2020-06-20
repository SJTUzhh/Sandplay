//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
#if AMBIENT_SKIES
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace AmbientSkies
{
    [CustomEditor(typeof(AmientSkiesAPI))]
    public class AmientSkiesAPIEditor : Editor
    {
        //Box style
        private GUIStyle m_boxStyle;
        //Local variables for functions
        private AmientSkiesAPI m_settings;

        private CreationToolsSettings m_creationToolSettings;

        //HDRP Changed
        private bool m_updateVisualEnvironment = true;
        private bool m_updateFog = true;
        private bool m_updateShadows = true;
        private bool m_updateAmbientLight = true;
        private bool m_updateScreenSpaceReflections = true;
        private bool m_updateScreenSpaceRefractions = true;
        private bool m_updateSun = true;

        //Post Processing Changed
        public bool m_updateAO = true;
        public bool m_updateAutoExposure = true;
        public bool m_updateBloom = true;
        public bool m_updateChromatic = true;
        public bool m_updateColorGrading = true;
        public bool m_updateDOF = true;
        public bool m_updateGrain = true;
        public bool m_updateLensDistortion = true;
        public bool m_updateMotionBlur = true;
        public bool m_updateSSR = true;
        public bool m_updateVignette = true;
        public bool m_updatePanini = true;
        public bool m_updateTargetPlaform = true;

        #region On GUI Inspector

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

            //Get AmientSkiesOrigamiPlugin object
            AmientSkiesAPI ambientSkiesSettings = (AmientSkiesAPI)target;
            m_settings = ambientSkiesSettings;

            m_creationToolSettings = AssetDatabase.LoadAssetAtPath<CreationToolsSettings>(SkyboxUtils.GetAssetPath("Ambient Skies Creation Tool Settings"));

            //Monitor for changes
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("Profile Configuration", m_boxStyle);
            GUILayout.Space(25f);

            //Load settings
            LoadAmbientSkiesProfiles();

            //Sets scripting define and sets selected pipeline
            ApplyScriptingDefine();


            #region Local Variables

            //Values
            List<AmbientSkyProfiles> profileList = ambientSkiesSettings.m_profileList;
            int newProfileListIndex = ambientSkiesSettings.newProfileListIndex;
            int profileListIndex = ambientSkiesSettings.m_profileListIndex;

            int newSkyboxSelection = m_creationToolSettings.m_selectedHDRI;
            int newProceduralSkyboxSelection = m_creationToolSettings.m_selectedProcedural;
            int newGradientSkyboxSelection = m_creationToolSettings.m_selectedGradient;
            int newppSelection = m_creationToolSettings.m_selectedPostProcessing;

            List<string> profileChoices = ambientSkiesSettings.profileChoices;
            List<string> skyboxChoices = ambientSkiesSettings.skyboxChoices;
            List<string> proceduralSkyboxChoices = ambientSkiesSettings.proceduralSkyboxChoices;
#if UNITY_2018_3_OR_NEWER
            List<string> gradientSkyboxChoices = ambientSkiesSettings.gradientSkyboxChoices;
#endif
            List<string> ppChoices = ambientSkiesSettings.ppChoices;
            List<string> lightmappingChoices = ambientSkiesSettings.lightmappingChoices;

            AmbientSkyProfiles profiles = ambientSkiesSettings.m_profiles;
            AmbientSkiesConsts.SystemTypes systemtype = profiles.m_systemTypes;
            AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings = ambientSkiesSettings.m_renderPipelineSelected;

            #endregion

            #region System selection

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select System", GUILayout.Width(146f));
            newProfileListIndex = EditorGUILayout.Popup(profileListIndex, profileChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (newProfileListIndex != profileListIndex)
            {
                profileListIndex = newProfileListIndex;
                profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath(profileList[profileListIndex].name));

                EditorPrefs.SetString("AmbientSkiesActiveProfile_", profileList[profileListIndex].name);

                m_creationToolSettings.m_selectedSystem = profileListIndex;

                newSkyboxSelection = 0;
                newProceduralSkyboxSelection = 0;
                newGradientSkyboxSelection = 0;

                #region Load List Profiles

                //Add skies profile names
                m_settings.skyboxChoices.Clear();
                foreach (var profile in m_settings.m_profiles.m_skyProfiles)
                {
                    m_settings.skyboxChoices.Add(profile.name);
                }
                //Add procedural skies profile names
                m_settings.proceduralSkyboxChoices.Clear();
                foreach (var profile in m_settings.m_profiles.m_proceduralSkyProfiles)
                {
                    m_settings.proceduralSkyboxChoices.Add(profile.name);
                }
#if UNITY_2018_3_OR_NEWER
                //Add gradient skies profile names
                m_settings.gradientSkyboxChoices.Clear();
                foreach (var profile in m_settings.m_profiles.m_gradientSkyProfiles)
                {
                    m_settings.gradientSkyboxChoices.Add(profile.name);
                }
#endif
                //Add post processing profile names
                m_settings.ppChoices.Clear();
                foreach (var profile in m_settings.m_profiles.m_ppProfiles)
                {
                    m_settings.ppChoices.Add(profile.name);
                }
                //Add lightmaps profile names
                m_settings.lightmappingChoices.Clear();
                foreach (var profile in m_settings.m_profiles.m_lightingProfiles)
                {
                    m_settings.lightmappingChoices.Add(profile.name);
                }

                #endregion

                Repaint();
            }

            #endregion

            #region Profile Selection

            GUILayout.BeginVertical("Skybox Profile Selection", m_boxStyle);
            GUILayout.Space(25f);

            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Select Skybox", GUILayout.Width(146f));
                newSkyboxSelection = EditorGUILayout.Popup(ambientSkiesSettings.m_selectedSkyboxProfileIndex, skyboxChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
                EditorGUILayout.EndHorizontal();
                //Prev / Next
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (newSkyboxSelection == 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Prev"))
                {
                    newSkyboxSelection--;
                }
                GUI.enabled = true;
                if (newSkyboxSelection == skyboxChoices.Count - 1)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Next"))
                {
                    newSkyboxSelection++;
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Select Skybox", GUILayout.Width(146f));
                newProceduralSkyboxSelection = EditorGUILayout.Popup(ambientSkiesSettings.m_selectedProceduralSkyboxProfileIndex, proceduralSkyboxChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
                EditorGUILayout.EndHorizontal();
                //Prev / Next
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (newProceduralSkyboxSelection == 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Prev"))
                {
                    newProceduralSkyboxSelection--;
                }
                GUI.enabled = true;
                if (newProceduralSkyboxSelection == skyboxChoices.Count - 1)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Next"))
                {
                    newProceduralSkyboxSelection++;
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }
            else
            {
#if UNITY_2018_3_OR_NEWER
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Select Skybox", GUILayout.Width(146f));
                newGradientSkyboxSelection = EditorGUILayout.Popup(ambientSkiesSettings.m_selectedGradientSkyboxProfileIndex, gradientSkyboxChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
                EditorGUILayout.EndHorizontal();
                //Prev / Next
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (newGradientSkyboxSelection == 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Prev"))
                {
                    newGradientSkyboxSelection--;
                }
                GUI.enabled = true;
                if (newGradientSkyboxSelection == gradientSkyboxChoices.Count - 1)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Next"))
                {
                    newGradientSkyboxSelection++;
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
#endif
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Post Processing", GUILayout.Width(146f));
            newppSelection = EditorGUILayout.Popup(ambientSkiesSettings.m_selectedPostProcessingProfileIndex, ppChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
            EditorGUILayout.EndHorizontal();
            //Prev / Next
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (newppSelection == 0)
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Prev"))
            {
                newppSelection--;
            }
            GUI.enabled = true;
            if (newppSelection == ppChoices.Count - 1)
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Next"))
            {
                newppSelection++;
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            #endregion

            GUILayout.EndVertical();

            #region Buttons

            if (GUILayout.Button("Apply Changes"))
            {
                //Apply sky settings
                ApplySkies();
                //Apply post processing settings
                ApplyPostProcessing();
            }

            if (GUILayout.Button("Open Ambient Skies Editor"))
            {
                //Opens up Ambient Skies window
                ShowMenu();
            }

            #endregion

            GUILayout.EndHorizontal();

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                #region Apply Changes

                Undo.RecordObject(ambientSkiesSettings, "Made camera culling changes");
                EditorUtility.SetDirty(ambientSkiesSettings);

                ambientSkiesSettings.m_profileList = profileList;
                ambientSkiesSettings.newProfileListIndex = newProfileListIndex;
                ambientSkiesSettings.m_profileListIndex = profileListIndex;

                m_creationToolSettings.m_selectedSystem = profileListIndex;

                ambientSkiesSettings.newSkyboxSelection = newSkyboxSelection;
                ambientSkiesSettings.newProceduralSkyboxSelection = newProceduralSkyboxSelection;
                ambientSkiesSettings.newGradientSkyboxSelection = newGradientSkyboxSelection;
                ambientSkiesSettings.m_selectedSkyboxProfileIndex = newSkyboxSelection;
                ambientSkiesSettings.m_selectedProceduralSkyboxProfileIndex = newProceduralSkyboxSelection;
                ambientSkiesSettings.m_selectedGradientSkyboxProfileIndex = newGradientSkyboxSelection;
                ambientSkiesSettings.m_selectedPostProcessingProfileIndex = newppSelection;

                m_creationToolSettings.m_selectedHDRI = newSkyboxSelection;
                m_creationToolSettings.m_selectedProcedural = newProceduralSkyboxSelection;
                m_creationToolSettings.m_selectedGradient = newGradientSkyboxSelection;
                m_creationToolSettings.m_selectedPostProcessing = newppSelection;

                ambientSkiesSettings.profileChoices = profileChoices;
                ambientSkiesSettings.skyboxChoices = skyboxChoices;
                ambientSkiesSettings.proceduralSkyboxChoices = proceduralSkyboxChoices;
#if UNITY_2018_3_OR_NEWER
                ambientSkiesSettings.gradientSkyboxChoices = gradientSkyboxChoices;
#endif
                ambientSkiesSettings.ppChoices = ppChoices;
                ambientSkiesSettings.lightmappingChoices = lightmappingChoices;

                ambientSkiesSettings.m_profiles = profiles;
                profiles.m_systemTypes = systemtype;
                ambientSkiesSettings.m_renderPipelineSelected = renderPipelineSettings;

                #endregion
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// Applies the skies settings 
        /// </summary>
        public void ApplySkies()
        {
            //Check if all profiles exist
            if (m_settings.m_profiles != null && m_settings.m_selectedSkyboxProfileIndex >= 0 && m_settings.m_selectedProceduralSkyboxProfileIndex >= 0 && m_settings.m_selectedGradientSkyboxProfileIndex >= 0)
            {
                //Apply profile settings
                SkyboxUtils.SetFromProfileIndex(m_settings.m_profiles, m_settings.m_selectedSkyboxProfileIndex, m_settings.m_selectedProceduralSkyboxProfileIndex, m_settings.m_selectedGradientSkyboxProfileIndex, false, m_updateVisualEnvironment, m_updateFog, m_updateShadows, m_updateAmbientLight, m_updateScreenSpaceReflections, m_updateScreenSpaceRefractions, m_updateSun);
            }
        }

        /// <summary>
        /// Applies the post processing settings
        /// </summary>
        public void ApplyPostProcessing()
        {
            //Check if all profiles exist
            if (m_settings.m_profiles != null && m_settings.m_selectedPostProcessingProfileIndex >= 0)
            {
                //Update post processing
                PostProcessingUtils.SetFromProfileIndex(m_settings.m_profiles, m_settings.m_selectedPostProcessingProfile, m_settings.m_selectedPostProcessingProfileIndex, false, m_updateAO, m_updateAutoExposure, m_updateBloom, m_updateChromatic, m_updateColorGrading, m_updateDOF, m_updateGrain, m_updateLensDistortion, m_updateMotionBlur, m_updateSSR, m_updateVignette, m_updatePanini, m_updateTargetPlaform);
            }
        }

        /// <summary>
        /// Open the ambient skies window
        /// </summary>
        public void ShowMenu()
        {
            //Ambient Skies Editor Window
            var mainWindow = EditorWindow.GetWindow<AmbientSkiesEditorWindow>(false, "Ambient Skies");
            //Show window
            mainWindow.Show();
        }

        /// <summary>
        /// Loads up the profiles and systems
        /// </summary>
        public void LoadAmbientSkiesProfiles()
        {
            #region Load Profile

            m_settings.m_profileList = GetAllSkyProfilesProjectSearch("t:AmbientSkyProfiles");

            //Add global profile names
            m_settings.profileChoices.Clear();
            foreach (var profile in m_settings.m_profileList)
            {
                m_settings.profileChoices.Add(profile.name);
            }

            m_settings.newProfileListIndex = 0;

            if (EditorPrefs.GetString("AmbientSkiesActiveProfile_") != null)
            {
                m_settings.newProfileListIndex = GetSkyProfile(m_settings.m_profileList, EditorPrefs.GetString("AmbientSkiesActiveProfile_"));
            }
            else
            {
                m_settings.newProfileListIndex = GetSkyProfile(m_settings.m_profileList, "Ambient Skies Volume 1");
            }

            //Get main Ambient Skies Volume 1 asset
            m_settings.m_profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath(m_settings.m_profileList[m_settings.newProfileListIndex].name));

            if (m_settings.m_profiles == null)
            {
                m_settings.m_profileListIndex = 0;
                m_settings.newProfileListIndex = 0;
                m_settings.m_profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath("Ambient Skies Volume 1"));
            }

            if (m_settings.m_profiles.name == "Ambient Skies Volume 1 Dev")
            {
                m_settings.m_profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath("Ambient Skies Volume 1"));
            }

            #endregion

            #region Load List Profiles

            //Add skies profile names
            m_settings.skyboxChoices.Clear();
            foreach (var profile in m_settings.m_profiles.m_skyProfiles)
            {
                m_settings.skyboxChoices.Add(profile.name);
            }
            //Add procedural skies profile names
            m_settings.proceduralSkyboxChoices.Clear();
            foreach (var profile in m_settings.m_profiles.m_proceduralSkyProfiles)
            {
                m_settings.proceduralSkyboxChoices.Add(profile.name);
            }
#if UNITY_2018_3_OR_NEWER
            //Add gradient skies profile names
            m_settings.gradientSkyboxChoices.Clear();
            foreach (var profile in m_settings.m_profiles.m_gradientSkyProfiles)
            {
                m_settings.gradientSkyboxChoices.Add(profile.name);
            }
#endif
            //Add post processing profile names
            m_settings.ppChoices.Clear();
            foreach (var profile in m_settings.m_profiles.m_ppProfiles)
            {
                m_settings.ppChoices.Add(profile.name);
            }
            //Add lightmaps profile names
            m_settings.lightmappingChoices.Clear();
            foreach (var profile in m_settings.m_profiles.m_lightingProfiles)
            {
                m_settings.lightmappingChoices.Add(profile.name);
            }

            #endregion

            #region Load Index

            //Get Current Saved
            if (m_settings.m_selectedSkyboxProfileIndex >= 0)
            {
                m_settings.m_selectedSkyboxProfile = m_settings.m_profiles.m_skyProfiles[m_settings.m_selectedSkyboxProfileIndex];
            }
            else
            {
                if (m_settings.m_profiles.m_showDebug)
                {
                    Debug.Log("Skybox Profile Empty");
                }

                m_settings.m_selectedSkyboxProfile = null;
            }

            if (m_settings.m_selectedProceduralSkyboxProfileIndex >= 0)
            {
                m_settings.m_selectedProceduralSkyboxProfile = m_settings.m_profiles.m_proceduralSkyProfiles[m_settings.m_selectedProceduralSkyboxProfileIndex];
            }
            else
            {
                if (m_settings.m_profiles.m_showDebug)
                {
                    Debug.Log("Skybox Profile Empty");
                }

                m_settings.m_selectedProceduralSkyboxProfile = null;
            }

            if (m_settings.m_selectedGradientSkyboxProfileIndex >= 0)
            {
                m_settings.m_selectedGradientSkyboxProfile = m_settings.m_profiles.m_gradientSkyProfiles[m_settings.m_selectedGradientSkyboxProfileIndex];
            }
            else
            {
                if (m_settings.m_profiles.m_showDebug)
                {
                    Debug.Log("Skybox Profile Empty");
                }

                m_settings.m_selectedGradientSkyboxProfile = null;
            }

            if (m_settings.m_selectedPostProcessingProfileIndex >= 0)
            {
                m_settings.m_selectedPostProcessingProfile = m_settings.m_profiles.m_ppProfiles[m_settings.m_selectedPostProcessingProfileIndex];
            }
            else
            {
                if (m_settings.m_profiles.m_showDebug)
                {
                    Debug.Log("Skybox Profile Empty");
                }

                m_settings.m_selectedPostProcessingProfile = null;
            }

            #endregion
        }

        /// <summary>
        /// Finds all profiles, to find type search t:OBJECT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeToSearch"></param>
        /// <returns></returns>
        public List<AmbientSkyProfiles> GetAllSkyProfilesProjectSearch(string typeToSearch)
        {
            string[] skyProfilesGUIDs = AssetDatabase.FindAssets(typeToSearch);
            List<AmbientSkyProfiles> newSkyProfiles = new List<AmbientSkyProfiles>(skyProfilesGUIDs.Length);
            for (int x = 0; x < skyProfilesGUIDs.Length; ++x)
            {
                string path = AssetDatabase.GUIDToAssetPath(skyProfilesGUIDs[x]);
                AmbientSkyProfiles data = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(path);
                if (data == null)
                    continue;
                newSkyProfiles.Add(data);
            }

            return newSkyProfiles;
        }

        /// <summary>
        /// Loads the first is or is not PW Profile
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="currentProfileName"></param>
        /// <returns></returns>
        public static int GetSkyProfile(List<AmbientSkyProfiles> profiles, string currentProfileName)
        {
            for (int idx = 0; idx < profiles.Count; idx++)
            {
                if (profiles[idx].name == currentProfileName)
                {
                    return idx;
                }
            }
            return 0;
        }

        /// <summary>
        /// Sets scripting defines for pipeline
        /// </summary>
        public bool ApplyScriptingDefine()
        {
            bool isChanged = false;
            if (GraphicsSettings.renderPipelineAsset == null)
            {
                m_settings.m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;

                string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                if (currBuildSettings.Contains("LWPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("LWPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("LWPipeline", "");
                    isChanged = true;
                }
                if (currBuildSettings.Contains("HDPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("HDPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("HDPipeline", "");
                    isChanged = true;
                }
                if (!currBuildSettings.Contains("AMBIENT_SKIES"))
                {
                    currBuildSettings += "AMBIENT_SKIES;";
                    isChanged = true;
                }
                //Check for enviro plugin is there
                bool enviroPresent = Directory.Exists(SkyboxUtils.GetAssetPath("Enviro - Sky and Weather"));
                if (enviroPresent)
                {
                    if (!currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings += "AMBIENT_SKIES_ENVIRO";
                        isChanged = true;
                    }
                }
                else
                {
                    if (currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings = currBuildSettings.Replace("AMBIENT_SKIES_ENVIRO;", "");
                        currBuildSettings = currBuildSettings.Replace("AMBIENT_SKIES_ENVIRO", "");
                        isChanged = true;
                    }
                }

                if (isChanged)
                {
                    if (EditorUtility.DisplayDialog("Status Changed", "The scripting defines need to updated this will cause a code recompile. Depending on how big your project is this could take a few minutes", "Ok"))
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
                    }

                    return true;
                }
            }
            else if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
            {
                m_settings.m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.HighDefinition;

                if (GraphicsSettings.renderPipelineAsset.name != "Procedural Worlds HDRPRenderPipelineAsset")
                {
                    GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SkyboxUtils.GetAssetPath("Procedural Worlds HDRPRenderPipelineAsset"));
                }

                string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                if (!currBuildSettings.Contains("HDPipeline"))
                {
                    if (string.IsNullOrEmpty(currBuildSettings))
                    {
                        currBuildSettings = "HDPipeline";
                    }
                    else
                    {
                        currBuildSettings += ";HDPipeline";
                    }
                    isChanged = true;
                }
                if (currBuildSettings.Contains("LWPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("LWPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("LWPipeline", "");
                    isChanged = true;
                }
                if (!currBuildSettings.Contains("AMBIENT_SKIES"))
                {
                    currBuildSettings += "AMBIENT_SKIES;";
                    isChanged = true;
                }
                //Check for enviro plugin is there
                bool enviroPresent = Directory.Exists(SkyboxUtils.GetAssetPath("Enviro - Sky and Weather"));
                if (enviroPresent)
                {
                    if (!currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings += "AMBIENT_SKIES_ENVIRO";
                        isChanged = true;
                    }
                }
                else
                {
                    if (currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings = currBuildSettings.Replace("AMBIENT_SKIES_ENVIRO;", "");
                        currBuildSettings = currBuildSettings.Replace("AMBIENT_SKIES_ENVIRO", "");
                        isChanged = true;
                    }
                }

                if (isChanged)
                {
                    if (EditorUtility.DisplayDialog("Status Changed", "The scripting defines need to updated this will cause a code recompile. Depending on how big your project is this could take a few minutes", "Ok"))
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
                    }

                    return true;
                }
            }
            else
            {
                m_settings.m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.Lightweight;

                if (GraphicsSettings.renderPipelineAsset.name != "Procedural Worlds Lightweight Pipeline Profile Ambient Skies")
                {
                    GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SkyboxUtils.GetAssetPath("Procedural Worlds Lightweight Pipeline Profile Ambient Skies"));
                }

                string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                if (!currBuildSettings.Contains("LWPipeline"))
                {
                    if (string.IsNullOrEmpty(currBuildSettings))
                    {
                        currBuildSettings = "LWPipeline";
                    }
                    else
                    {
                        currBuildSettings += ";LWPipeline";
                    }
                    isChanged = true;
                }
                if (currBuildSettings.Contains("HDPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("HDPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("HDPipeline", "");
                    isChanged = true;
                }
                if (!currBuildSettings.Contains("AMBIENT_SKIES"))
                {
                    currBuildSettings += "AMBIENT_SKIES;";
                    isChanged = true;
                }
                //Check for enviro plugin is there
                bool enviroPresent = Directory.Exists(SkyboxUtils.GetAssetPath("Enviro - Sky and Weather"));
                if (enviroPresent)
                {
                    if (!currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings += "AMBIENT_SKIES_ENVIRO";
                        isChanged = true;
                    }
                }
                else
                {
                    if (currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings = currBuildSettings.Replace("AMBIENT_SKIES_ENVIRO;", "");
                        currBuildSettings = currBuildSettings.Replace("AMBIENT_SKIES_ENVIRO", "");
                        isChanged = true;
                    }
                }

                if (isChanged)
                {
                    if (EditorUtility.DisplayDialog("Status Changed", "The scripting defines need to updated this will cause a code recompile. Depending on how big your project is this could take a few minutes", "Ok"))
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
                    }

                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
#endif