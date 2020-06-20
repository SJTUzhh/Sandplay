//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Rendering;

namespace AmbientSkies
{
    /// <summary>
    /// This class wraps a list of ambient sky profiles
    /// </summary>
    [System.Serializable]
    public class AmbientSkyProfiles : ScriptableObject
    {
        #region Variables

        [Header("Profile Settings")]

        [SerializeField]
        public bool m_editSettings = false;

        [SerializeField]
        public int m_inspectorUpdateLimit = 2;

        [SerializeField]
        [HideInInspector]
        public string m_version = "Version 1.0";

#if UNITY_EDITOR
        [SerializeField]
        public CreationToolsSettings m_CreationToolSettings;
#endif

        [Header("Debug Settings")]

        [SerializeField]
        public bool m_showDebug = false;

        [SerializeField]
        public bool m_showTimersInDebug = false;

        [SerializeField]
        public bool m_showHasChangedDebug = false;

        [SerializeField]
        public bool m_showFunctionDebugsOnly = false;

        [SerializeField]
        public bool m_smartConsoleClean = false;

        [SerializeField]
        [HideInInspector]
        public bool m_isProceduralCreatedProfile = true;

        #region Global Settings

        [Header("Global Settings")]

        [SerializeField]
        public AmbientSkiesConsts.SystemTypes m_systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.EnvironmentLightingEditMode m_lightingEditMode = AmbientSkiesConsts.EnvironmentLightingEditMode.Exterior;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.RenderPipelineSettings m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.PlatformTarget m_targetPlatform = AmbientSkiesConsts.PlatformTarget.DesktopAndConsole;

        [SerializeField]
        public AmbientSkiesConsts.DisableAndEnable m_useTimeOfDay;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.VSyncMode m_vSyncMode = AmbientSkiesConsts.VSyncMode.DontSync;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.AutoConfigureType m_configurationType = AmbientSkiesConsts.AutoConfigureType.Terrain;

        [SerializeField]
        [HideInInspector]
        public bool m_autoMatchProfile = true;

        #endregion

        #region Time Of Day

        [Header("Time Of Day Settings")]

        [SerializeField]
        [HideInInspector]
        public bool m_syncPostProcessing = true;

        [SerializeField]
        [HideInInspector]
        public bool m_realtimeGIUpdate = false;

        [SerializeField]
        [HideInInspector]
        public int m_gIUpdateInterval = 15;

        [SerializeField]
        [HideInInspector]
        public bool m_realtimeEmission = false;

        [SerializeField]
        [HideInInspector]
        public bool m_syncRealtimeEmissionToTimeOfDay;

        [SerializeField]
        [HideInInspector]
        public float m_dayLengthInSeconds = 120f;

        [SerializeField]
        [HideInInspector]
        public int m_dayDate = 18;

        [SerializeField]
        [HideInInspector]
        public int m_monthDate = 5;

        [SerializeField]
        [HideInInspector]
        public int m_yearDate = 2019;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.CurrentSeason m_currentSeason;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.HemisphereOrigin m_hemisphereOrigin;

        [SerializeField]
        //[HideInInspector]
        public AmbientSkiesTimeOfDayProfile m_timeOfDayProfile;

        [SerializeField]
        [HideInInspector]
        public KeyCode m_pauseTimeKey = KeyCode.P;

        [SerializeField]
        [HideInInspector]
        public KeyCode m_incrementUpKey = KeyCode.Q;

        [SerializeField]
        [HideInInspector]
        public KeyCode m_incrementDownKey = KeyCode.E;

        [SerializeField]
        [HideInInspector]
        public KeyCode m_rotateSunLeftKey = KeyCode.I;

        [SerializeField]
        [HideInInspector]
        public KeyCode m_rotateSunRightKey = KeyCode.O;

        [SerializeField]
        [HideInInspector]
        public float m_timeToAddOrRemove = 0.025f;

        [SerializeField]
        [HideInInspector]
        public float m_sunRotationAmount = 15f;

        [SerializeField]
        [HideInInspector]
        public bool m_pauseTime = false;

        [SerializeField]
        [HideInInspector]
        public float m_currentTimeOfDay = 0.5f;

        [SerializeField]
        [HideInInspector]
        public float m_skyboxRotation = 0f;

        [SerializeField]
        [HideInInspector]
        public float m_nightLengthInSeconds = 150f;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_daySunIntensity;

        [SerializeField]
        [HideInInspector]
        public Gradient m_daySunGradientColor;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_nightSunIntensity;

        [SerializeField]
        [HideInInspector]
        public Gradient m_nightSunGradientColor;

        [SerializeField]
        [HideInInspector]
        public float m_startFogDistance = 20f;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_dayFogDensity;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_nightFogDensity;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_dayFogDistance;

        [SerializeField]
        [HideInInspector]
        public Gradient m_dayFogColor;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_nightFogDistance;

        [SerializeField]
        [HideInInspector]
        public Gradient m_nightFogColor;

        [SerializeField]
        [HideInInspector]
        public Gradient m_dayPostFXColor;

        [SerializeField]
        [HideInInspector]
        public Gradient m_nightPostFXColor;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_dayTempature;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_nightTempature;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_lightAnisotropy;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_lightProbeDimmer;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_lightDepthExtent;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_sunSize;

        [SerializeField]
        [HideInInspector]
        public AnimationCurve m_skyExposure;

        #endregion

        #region System Enable Bools

        [Header("Systems Enabled")]

        [SerializeField]
        public bool m_useSkies;

        [SerializeField]
        public bool m_usePostFX;

        [SerializeField]
        public bool m_useLighting;

        #endregion

        #region Profiles

        [Header("Profiles")]

        [SerializeField]
        public List<AmbientSkyboxProfile> m_skyProfiles = new List<AmbientSkyboxProfile>();

        [SerializeField]
        public List<AmbientProceduralSkyboxProfile> m_proceduralSkyProfiles = new List<AmbientProceduralSkyboxProfile>();

        [SerializeField]
        public List<AmbientGradientSkyboxProfile> m_gradientSkyProfiles = new List<AmbientGradientSkyboxProfile>();

        [SerializeField]
        public List<AmbientPostProcessingProfile> m_ppProfiles = new List<AmbientPostProcessingProfile>();

        [SerializeField]
        public List<AmbientLightingProfile> m_lightingProfiles = new List<AmbientLightingProfile>();

        #endregion

        #endregion

        #region Utils

        /// <summary>
        /// Create sky profile asset
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Ambient Skies/Sky Profiles")]
        public static void CreateSkyProfiles()
        {
            AmbientSkyProfiles asset = ScriptableObject.CreateInstance<AmbientSkyProfiles>();
            #if UNITY_EDITOR
            AssetDatabase.CreateAsset(asset, "Assets/Ambient Skies Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            #endif
        }
#endif

        /// <summary>
        /// Revert back to default settings
        /// </summary>
        /// <param name="profileIdx"></param>
        public void RevertSkyboxSettingsToDefault(int hdriProfileIdx, int proceduralProfileIdx, int gradientProceduralIdx)
        {
            if (m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                if (hdriProfileIdx >= 0 && hdriProfileIdx < m_skyProfiles.Count)
                {
                    m_skyProfiles[hdriProfileIdx].RevertToDefault();
                }
            }
            else if (m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                if (proceduralProfileIdx >= 0 && proceduralProfileIdx < m_proceduralSkyProfiles.Count)
                {
                    m_proceduralSkyProfiles[proceduralProfileIdx].RevertToDefault();
                }
            }
            else
            {
                if (gradientProceduralIdx >= 0 && gradientProceduralIdx < m_gradientSkyProfiles.Count)
                {
                    m_gradientSkyProfiles[gradientProceduralIdx].RevertToDefault();
                }
            }
        }

        /// <summary>
        /// Copy settings to defaults
        /// </summary>
        /// <param name="profileIdx"></param>
        public void SaveSkyboxSettingsToDefault(int profileIdx)
        {
            if (m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                if (profileIdx >= 0 && profileIdx < m_skyProfiles.Count)
                {
                    m_skyProfiles[profileIdx].SaveCurrentToDefault();
                }
            }
            else if (m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                if (profileIdx >= 0 && profileIdx < m_proceduralSkyProfiles.Count)
                {
                    m_proceduralSkyProfiles[profileIdx].SaveCurrentToDefault();
                }
            }
            else
            {
                if (profileIdx >= 0 && profileIdx < m_gradientSkyProfiles.Count)
                {
                    m_gradientSkyProfiles[profileIdx].SaveCurrentToDefault();
                }
            }
        }

        /// <summary>
        /// Copy settings to defaults
        /// </summary>
        /// <param name="profileIdx"></param>
        public void SaveLightingSettingsToDefault(int profileIdx)
        {
            if (profileIdx >= 0 && profileIdx < m_lightingProfiles.Count)
            {
                m_lightingProfiles[profileIdx].SaveCurrentToDefault();
            }
        }

        /// <summary>
        /// Copy settings to defaults
        /// </summary>
        /// <param name="profileIdx"></param>
        public void RevertLightingSettingsToDefault(int profileIdx)
        {
            if (profileIdx >= 0 && profileIdx < m_lightingProfiles.Count)
            {
                m_lightingProfiles[profileIdx].RevertToDefault();
            }
        }

        /// <summary>
        /// Revert back to default settings
        /// </summary>
        /// <param name="profileIdx"></param>
        public void RevertPostProcessingSettingsToDefault(int profileIdx, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
            if (profileIdx >= 0 && profileIdx < m_ppProfiles.Count)
            {
                m_ppProfiles[profileIdx].RevertToDefault(renderPipelineSettings);
            }
        }

        /// <summary>
        /// Copy settings to defaults
        /// </summary>
        /// <param name="profileIdx"></param>
        public void SavePostProcessingSettingsToDefault(int profileIdx, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
            if (profileIdx >= 0 && profileIdx < m_ppProfiles.Count)
            {
                m_ppProfiles[profileIdx].SaveCurrentToDefault(renderPipelineSettings);
            }
        }

        /// <summary>
        /// Saves all the settings in all profiles
        /// </summary>
        /// <param name="renderPipelineSettings"></param>
        public void SaveAllDefaults(AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
            foreach (AmbientSkyboxProfile skyboxProfile in m_skyProfiles)
            {
                skyboxProfile.SaveCurrentToDefault();
            }

            foreach (AmbientProceduralSkyboxProfile proceduralProfile in m_proceduralSkyProfiles)
            {
                proceduralProfile.SaveCurrentToDefault();
            }

            foreach (AmbientGradientSkyboxProfile gradientProfile in m_gradientSkyProfiles)
            {
                gradientProfile.SaveCurrentToDefault();
            }

            foreach (AmbientPostProcessingProfile processProfile in m_ppProfiles)
            {
                processProfile.SaveCurrentToDefault(renderPipelineSettings);
            }

            foreach (AmbientLightingProfile lightingProfile in m_lightingProfiles)
            {
                lightingProfile.SaveCurrentToDefault();
            }

            Debug.Log("Saved all profiles to defaults successfully");
        }

        /// <summary>
        /// Saves all the settings in all profiles
        /// </summary>
        /// <param name="renderPipelineSettings"></param>
        public void RevertAllDefaults(AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);

            m_systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
            m_useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
            m_useSkies = false;
            m_usePostFX = false;
            m_useLighting = false;
            m_editSettings = false;
            m_showDebug = false;
            m_showFunctionDebugsOnly = false;
            m_showHasChangedDebug = false;
            m_showTimersInDebug = false;
            m_smartConsoleClean = false;

            GameObject postProcessObject = GameObject.Find("Global Post Processing");
            if (postProcessObject != null)
            {
                DestroyImmediate(postProcessObject);
            }

            GameObject hdrpPPVolume = GameObject.Find("Post Processing HDRP Volume");
            if (hdrpPPVolume != null)
            {
                DestroyImmediate(hdrpPPVolume);
            }

            foreach (AmbientSkyboxProfile skyboxProfile in m_skyProfiles)
            {
                skyboxProfile.RevertToDefault();
            }

            foreach (AmbientProceduralSkyboxProfile proceduralProfile in m_proceduralSkyProfiles)
            {
                proceduralProfile.RevertToDefault();
            }

            foreach (AmbientGradientSkyboxProfile gradientProfile in m_gradientSkyProfiles)
            {
                gradientProfile.RevertToDefault();
            }

            foreach (AmbientPostProcessingProfile processProfile in m_ppProfiles)
            {
                processProfile.RevertToDefault(renderPipelineSettings);
            }

            foreach (AmbientLightingProfile lightingProfile in m_lightingProfiles)
            {
                lightingProfile.RevertToDefault();
            }

            if (m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                UpdateAllFogType(AmbientSkiesConsts.VolumeFogType.Linear);
            }
            else if (m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
            {
                UpdateAllFogType(AmbientSkiesConsts.VolumeFogType.Linear);
            }
            else
            {
                UpdateAllFogType(AmbientSkiesConsts.VolumeFogType.Volumetric);
            }

            m_CreationToolSettings = AssetDatabase.LoadAssetAtPath<CreationToolsSettings>(GetAssetPath("Ambient Skies Creation Tool Settings"));
            if (m_CreationToolSettings != null)
            {
                EditorUtility.SetDirty(m_CreationToolSettings);
                m_CreationToolSettings.RevertToFactorySettings();
            }

            Material skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Ambient Skies Skybox"));
            if (skyMaterial != null)
            {
                skyMaterial.shader = Shader.Find("Skybox/Procedural");
                skyMaterial.SetFloat("_SunDisk", 2f);
                skyMaterial.EnableKeyword("_SUNDISK_HIGH_QUALITY");
            }

            DestroyParent("Ambient Skies Environment");

            Debug.Log("Reverted to factory settings successfully");
#endif
        }

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>The path or null</returns>
        public static string GetAssetPath(string name)
        {
#if UNITY_EDITOR
            string[] assets = AssetDatabase.FindAssets(name, null);
            if (assets.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assets[0]);
            }
#endif
            return null;
        }

        /// <summary>
        /// Update all the fog modes in the active profile
        /// </summary>
        /// <param name="fogmode"></param>
        public void UpdateAllFogType(AmbientSkiesConsts.VolumeFogType fogmode)
        {
            if (m_showDebug)
            {
                Debug.Log("Updating fog modes to " + fogmode.ToString());
            }

            //Get each skybox profile
            foreach (AmbientSkyboxProfile profile in m_skyProfiles)
            {
                //Change fog mode to Linear
                profile.fogType = fogmode;
            }
            //Get each procedural profile
            foreach (AmbientProceduralSkyboxProfile profile in m_proceduralSkyProfiles)
            {
                //Change fog mode to Linear
                profile.fogType = fogmode;
            }
            //Get each gradient profile
            foreach (AmbientGradientSkyboxProfile profile in m_gradientSkyProfiles)
            {
                //Change fog mode to Linear
                profile.fogType = fogmode;
            }
        }

        /// <summary>
        /// Find parent object and destroys it if it's empty
        /// </summary>
        /// <param name="parentGameObject"></param>
        public static void DestroyParent(string parentGameObject)
        {
            //If string isn't empty
            if (!string.IsNullOrEmpty(parentGameObject))
            {
                //If string doesn't = Ambient Skies Environment
                if (parentGameObject != "Ambient Skies Environment")
                {
                    //Sets the paramater to Ambient Skies Environment
                    parentGameObject = "Ambient Skies Environment";
                }

                //Find parent object
                GameObject parentObject = GameObject.Find(parentGameObject);
                if (parentObject != null)
                {
                    //Find parents in parent object
                    Transform[] parentChilds = parentObject.GetComponentsInChildren<Transform>();
                    if (parentChilds.Length == 1)
                    {
                        //Destroy object if object is empty
                        Object.DestroyImmediate(parentObject);
                    }
                }
            }
        }

        #endregion
    }
}