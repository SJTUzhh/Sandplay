//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using AmbientSkies.Internal;
using PWCommon1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif
#if LWPipeline && UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering.LWRP;
#elif LWPipeline && UNITY_2018_3_OR_NEWER
using UnityEngine.Experimental.Rendering.LightweightPipeline;
#endif

using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEditor.Rendering;

namespace AmbientSkies
{
    /// <summary>
    /// Handles all pipeline related code
    /// </summary>
    [Serializable]
    public class AmbientSkiesPipelineUtilsEditor : EditorWindow
    {
        #region Persisent Variables

        //Potential pipeline update states
        public enum PipelineStatus
        {
            Idle,
            Compiling,
            LoadPackageList,
            ProcessInstalledPackageList,
            InstallingPackages,
            InstallingWaterShaders,
            UpdatingMaterials,
            DeletingPackages,
            UpdatingBuildSettings,
            FinalizingEnvironment,
            FailedWithErrors,
            RemovingUnusedComponents,
            CleanUp,
            Success
        };

        [SerializeField]
        private static AmbientSkiesEditorWindow m_ambientSkiesEditorWindow;

        [SerializeField]
        private AmbientSkyProfiles m_profiles;

        [SerializeField]
        private bool m_finalizeEnvironment;

        [SerializeField]
        private AmbientSkyboxProfile m_selectedSkyboxProfile;

        [SerializeField]
        private int m_selectedSkyboxProfileIndex;

        [SerializeField]
        private AmbientProceduralSkyboxProfile m_selectedProceduralSkyboxProfile;

        [SerializeField]
        private AmbientGradientSkyboxProfile m_selectedGradientSkyboxProfile;

        [SerializeField]
        private int m_selectedProceduralSkyboxProfileIndex;

        [SerializeField]
        private int m_selectedPostProcessingProfileIndex;

        [SerializeField]
        private AmbientPostProcessingProfile m_selectedPostProcessingProfile;

        //Original pipeline - where we are coming from
        [SerializeField]
        private AmbientSkiesConsts.RenderPipelineSettings m_originalPipeline = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;

        //New pipeline - where we are moving to
        [SerializeField]
        private AmbientSkiesConsts.RenderPipelineSettings m_pipelineToInstall = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;

        //Whether we should update materials or not
        [SerializeField]
        private bool m_updateMaterials = true;

        //What is the process we are executing
        [SerializeField]
        public string m_currentTask; 

        //Our current status
        [SerializeField]
        private PipelineStatus m_currentStatus = PipelineStatus.Idle;

        //Our previous status
        [SerializeField]
        private PipelineStatus m_previousStatus = PipelineStatus.Idle;

        //Our current status message
        [SerializeField]
        private string m_currentStatusMessage;

        //HDRP Changed
        [SerializeField]
        private static bool m_updateVisualEnvironment = false;
        [SerializeField]
        private static bool m_updateFog = false;
        [SerializeField]
        private static bool m_updateShadows = false;
        [SerializeField]
        private static bool m_updateAmbientLight = false;
        [SerializeField]
        private static bool m_updateScreenSpaceReflections = false;
        [SerializeField]
        private static bool m_updateScreenSpaceRefractions = false;
        [SerializeField]
        private static bool m_updateSun = false;

        //Post Processing Changed
        [SerializeField]
        public bool m_updateAO = false;
        [SerializeField]
        public bool m_updateAutoExposure = false;
        [SerializeField]
        public bool m_updateBloom = false;
        [SerializeField]
        public bool m_updateChromatic = false;
        [SerializeField]
        public bool m_updateColorGrading = false;
        [SerializeField]
        public bool m_updateDOF = false;
        [SerializeField]
        public bool m_updateGrain = false;
        [SerializeField]
        public bool m_updateLensDistortion = false;
        [SerializeField]
        public bool m_updateMotionBlur = false;
        [SerializeField]
        public bool m_updateSSR = false;
        [SerializeField]
        public bool m_updateVignette = false;
        [SerializeField]
        public bool m_updatePanini = false;
        [SerializeField]
        public bool m_updateTargetPlaform = false;

        //Package management related
#if UNITY_2018_1_OR_NEWER

        [SerializeField]
        private bool m_isAddingPackage = false;

        [SerializeField]
        private bool m_isRemovingPackage = false;

        [Serializable]
        public class PackageEntry
        {
            public string name;
            public string version;
        }
        [SerializeField]
        private List<PackageEntry> m_packagesToAdd;
        [SerializeField]
        private List<PackageEntry> m_packagesToRemove;
        [SerializeField]
        private Stack<int> m_missingPackages = new Stack<int>();
#endif

        #endregion

        #region Non Persisent Variables     

        private GUIStyle m_boxStyle;
        private bool m_showDebug = false;
        private DateTime m_startTime;

#if UNITY_2018_1_OR_NEWER
        private AddRequest m_pmAddRequest = null;
        private RemoveRequest m_pmRemoveRequest = null;
        private ListRequest m_pmListRequest = null;
#endif

#endregion

        #region External interface

        /// <summary>
        /// Show the pipeline utils editor
        /// </summary>
        public static void ShowAmbientSkiesPipelineUtilsEditor(AmbientSkiesConsts.RenderPipelineSettings oldRenderer, AmbientSkiesConsts.RenderPipelineSettings newRenderer, bool updateMaterials, bool finalizeEnvironment, AmbientSkiesEditorWindow ambientSkiesEditor)
        {
            m_ambientSkiesEditorWindow = ambientSkiesEditor;

            //var utilsWindow = EditorWindow.GetWindow<Gaia.GaiaPipelineUtilsEditor>(false, "Gaia Pipeline Utils");
            var utilsWindow = ScriptableObject.CreateInstance<AmbientSkiesPipelineUtilsEditor>();

            if (utilsWindow != null)
            {
                utilsWindow.m_finalizeEnvironment = finalizeEnvironment;
                utilsWindow.m_originalPipeline = oldRenderer;
                utilsWindow.m_pipelineToInstall = newRenderer;
                utilsWindow.m_currentTask = oldRenderer.ToString() + " --> " + newRenderer.ToString();
                utilsWindow.m_currentStatus = PipelineStatus.Idle;
                utilsWindow.m_previousStatus = PipelineStatus.Idle;
                utilsWindow.m_updateMaterials = updateMaterials;

                utilsWindow.minSize = new Vector2(300f, 50f);
                var position = utilsWindow.position;
                position.width = 460f;
                position.height = 90f;
                position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height - 280f).center;
                utilsWindow.position = position;

                /*
                m_updateVisualEnvironment = ambientSkiesEditor.m_updateVisualEnvironment;
                m_updateFog = ambientSkiesEditor.m_updateFog;
                m_updateShadows = ambientSkiesEditor.m_updateShadows;
                m_updateAmbientLight = ambientSkiesEditor.m_updateAmbientLight;
                m_updateScreenSpaceReflections = ambientSkiesEditor.m_updateScreenSpaceReflections;
                m_updateScreenSpaceRefractions = ambientSkiesEditor.m_updateScreenSpaceRefractions;
                m_updateSun = ambientSkiesEditor.m_updateSun;
                */

                utilsWindow.ShowPopup();
            }
        }

        #endregion

        #region Unity Methods

        /// <summary>
        /// See if we can preload the manager with existing settings
        /// </summary>
        void OnEnable()
        {
            if (m_showDebug)
            {
                Debug.Log("Loading Up");
            }

            m_profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath("Ambient Skies Volume 1"));
            if (m_profiles == null)
            {
                if (m_showDebug)
                {
                    Debug.LogError("Sky Profiles not found, this will result in the pipeline switching to fail");
                }
            }
            else
            {
                //Assigns skybox profile
                string key = "AmbientSkiesProfile_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                if (EditorPrefs.HasKey(key))
                {
                    int idx = SkyboxUtils.GetProfileIndexFromProfileName(m_profiles, EditorPrefs.GetString(key));
                    if (idx >= 0)
                    {
                        m_selectedSkyboxProfileIndex = idx;
                    }
                    else
                    {
                        m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromActiveSkybox(m_profiles, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, true);
                    }

                }
                else
                {
                    m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromActiveSkybox(m_profiles, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, true);
                }

                if (m_selectedSkyboxProfileIndex >= 0)
                {
                    m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
                }
                else
                {
                    if (m_showDebug)
                    {
                        Debug.Log("Skybox Profile Empty");
                    }
                    m_selectedSkyboxProfile = null;
                }

                if (m_selectedProceduralSkyboxProfile == null)
                {
                    m_selectedProceduralSkyboxProfile = m_profiles.m_proceduralSkyProfiles[0];
                }

                if (m_selectedGradientSkyboxProfile == null)
                {
                    m_selectedGradientSkyboxProfile = m_profiles.m_gradientSkyProfiles[0];
                }

                //Assigns post processing profile
                string postProcessingName = "AmbientSkiesPostProcessing_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode();
                if (EditorPrefs.HasKey(postProcessingName))
                {
                    int idx = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, EditorPrefs.GetString(postProcessingName));
                    if (idx >= 0)
                    {
                        m_selectedPostProcessingProfileIndex = idx;
                    }
                }
                else
                {
                    m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromPostProcessing(m_profiles);
                }
                    
                if (m_selectedPostProcessingProfileIndex >= 0)
                {
                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                }
                else
                {
                    if (m_showDebug)
                    {
                        Debug.Log("Post Processing Profile Empty");
                    }
                    m_selectedPostProcessingProfile = null;
                }
            }

            if (m_showDebug)
            {
                Debug.Log("Enabling : " + m_currentStatus.ToString());
            }

            //Start editor updates
            if (!Application.isPlaying)
            {
                StartEditorUpdates();
            }
        }

        /// <summary>
        /// On Disable
        /// </summary>
        void OnDisable()
        {
            StopEditorUpdates();
            if (m_showDebug)
            {
                Debug.Log("Disabling : " + m_currentStatus.ToString());
            }
        }

        /// <summary>
        /// On Destroy
        /// </summary>
        private void OnDestroy()
        {
            if (m_showDebug)
            {
                Debug.Log("Destroying");
            }

            if (EditorUtility.DisplayDialog("Clear Lightmaps!", "You've switched pipeline, lighting will behave differently in this render pipeline. We recommend clearing the lightmap data in your scene. Would you like to clear it?", "Yes", "No"))
            {
                LightingUtils.ClearLightmapData();
            }

            //Ambient Skies Editor Window
            //var mainWindow = GetWindow<AmbientSkiesEditorWindow>(false, "Ambient Skies");
            //Show window
            //mainWindow.Show();
        }

        /// <summary>
        /// Start editor updates
        /// </summary>
        public void StartEditorUpdates()
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        /// <summary>
        /// Stop editor updates
        /// </summary>
        public void StopEditorUpdates()
        {
            EditorApplication.update -= EditorUpdate;
        }

        /// <summary>
        /// Drive the update process
        /// </summary>
        void EditorUpdate()
        {
            //Check for state of compiler
            if (EditorApplication.isCompiling)
            {
                //Step into compilaton mode
                if (m_currentStatus != PipelineStatus.Compiling)
                {
                    m_previousStatus = m_currentStatus;
                    m_currentStatus = PipelineStatus.Compiling;
                    Repaint();
                }
            }
            else
            {
                //Step out of compilation mode
                if (m_currentStatus == PipelineStatus.Compiling)
                {
                    m_currentStatus = m_previousStatus;
                    m_startTime = DateTime.Now;
                    Repaint();
                }

                //Update state change timer
                if (m_currentStatus != m_previousStatus)
                {
                    m_startTime = DateTime.Now;
                    Repaint();
                }

                //Handle current state
                switch (m_currentStatus)
                {
                    case PipelineStatus.Idle:
                        m_previousStatus = m_currentStatus;
                        if (m_ambientSkiesEditorWindow != null)
                        {
                            m_ambientSkiesEditorWindow.Close();
                            m_ambientSkiesEditorWindow = null;
                        }
                        m_currentStatus = PipelineStatus.RemovingUnusedComponents;
                        break;
#if UNITY_2018_1_OR_NEWER
                    case PipelineStatus.RemovingUnusedComponents:
                        RemoveObjectComponents();
                        break;
                    case PipelineStatus.LoadPackageList:
                        LoadPackageList();
                        break;
                    case PipelineStatus.ProcessInstalledPackageList:
                        ProcessInstalledPackageList();
                        break;
                    case PipelineStatus.InstallingPackages:
                        InstallPackages();
                        break;
                    case PipelineStatus.InstallingWaterShaders:
                        InstallWaterShaders();
                        break;
                    case PipelineStatus.UpdatingMaterials:
                        UpdateMaterials();
                        break;
                    case PipelineStatus.DeletingPackages:
                        DeletePackages();
                        break;
                    case PipelineStatus.UpdatingBuildSettings:
                        UpdateBuildSettings();
                        break;
                    case PipelineStatus.FinalizingEnvironment:
                        FinalizeEnvironment();
                        break;
                    case PipelineStatus.CleanUp:
                        CleanUpScene();
                        break;
#endif
                    case PipelineStatus.Success:
                        m_previousStatus = m_currentStatus;
                        Repaint();
                        if ((DateTime.Now - m_startTime).TotalSeconds > 5f)
                        {                           
                            m_profiles.m_selectedRenderPipeline = m_pipelineToInstall;
                            EditorUtility.SetDirty(m_profiles);
                            EditorSceneManager.MarkAllScenesDirty();
                            AssetDatabase.Refresh();
                            Close();
                        }
                        break;
                    case PipelineStatus.FailedWithErrors:
                        m_previousStatus = m_currentStatus;
                        Repaint();
                        if ((DateTime.Now - m_startTime).TotalSeconds > 5f)
                        {
                            Close();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Draw UX
        /// </summary>
        void OnGUI()
        {
            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }

            //Draw title
            GUILayout.BeginHorizontal("Pipeline Installer", m_boxStyle);
            EditorGUILayout.LabelField("");
            GUILayout.EndHorizontal();

            //Draw content
            EditorGUILayout.LabelField(new GUIContent("Current Task"), new GUIContent(m_currentTask));
            EditorGUILayout.LabelField(new GUIContent("Current Status"), new GUIContent(Regex.Replace(m_currentStatus.ToString(), "(\\B[A-Z])", " $1")));
            if (m_currentStatus == PipelineStatus.Success)
            {
                m_currentStatusMessage = "Exiting in " + ((int)(5 - (DateTime.Now - m_startTime).TotalSeconds)).ToString();
            }
            EditorGUILayout.LabelField(new GUIContent(" "), new GUIContent(m_currentStatusMessage));
        }

        #endregion

        #region Pipeline Installation Methods & Data

#if UNITY_2018_1_OR_NEWER

        /// <summary>
        /// Cleans up script components on the scene
        /// </summary>
        /// <param name="renderPipelineSettings"></param>
        private void RemoveObjectComponents()
        {
            switch (m_pipelineToInstall)
            {
                case AmbientSkiesConsts.RenderPipelineSettings.BuiltIn:
                    #region Built-In

                    SkyboxUtils.DestroyParent("Ambient Skies Environment");

#if HDPipeline
                    HDAdditionalLightData hDAdditionalLightData = FindObjectOfType<HDAdditionalLightData>();
                    if (hDAdditionalLightData != null)
                    {
                        DestroyImmediate(hDAdditionalLightData);
                    }

                    GameObject underwaterProbe = GameObject.Find("Underwater Reflection Probe");
                    if (underwaterProbe != null)
                    {
                        DestroyImmediate(underwaterProbe);
                    }

                    AdditionalShadowData additionalShadowData = FindObjectOfType<AdditionalShadowData>();
                    if (additionalShadowData != null)
                    {
                        DestroyImmediate(additionalShadowData);
                    }

                    HDAdditionalCameraData hDAdditionalCameraData = FindObjectOfType<HDAdditionalCameraData>();
                    if (hDAdditionalCameraData != null)
                    {
                        DestroyImmediate(hDAdditionalCameraData);
                    }

                    GameObject planarReflectionProbeObject = GameObject.Find("Water Planar Reflections");
                    if (planarReflectionProbeObject != null)
                    {
                        DestroyImmediate(planarReflectionProbeObject);
                    }

                    GameObject densityVolume = GameObject.Find("Density Volume");
                    if (densityVolume != null)
                    {
                        DestroyImmediate(densityVolume);
                    }

                    GameObject hdWindSettings = GameObject.Find("High Definition Wind");
                    if (hdWindSettings != null)
                    {
                        DestroyImmediate(hdWindSettings);
                    }
#endif

#if LWPipeline && UNITY_2018_3_OR_NEWER
                    LWRPAdditionalCameraData lWRPAdditionalCameraData = FindObjectOfType<LWRPAdditionalCameraData>();
                if (lWRPAdditionalCameraData != null)
                {
                    DestroyImmediate(lWRPAdditionalCameraData);
                }

                LWRPAdditionalLightData lWRPAdditionalLightData = FindObjectOfType<LWRPAdditionalLightData>();
                if (lWRPAdditionalLightData != null)
                {
                    DestroyImmediate(lWRPAdditionalLightData);
                }
#endif
                    #endregion
                    break;
                case AmbientSkiesConsts.RenderPipelineSettings.Lightweight:
                    #region Lightweight

                    GameObject horizonObject = GameObject.Find("Ambient Skies Horizon");
                    if (horizonObject != null)
                    {
                        DestroyImmediate(horizonObject);
                    }

                    SkyboxUtils.DestroyParent("Ambient Skies Environment");
#if HDPipeline
                    HDAdditionalLightData hDAdditionalLightDataLW = FindObjectOfType<HDAdditionalLightData>();
                    if (hDAdditionalLightDataLW != null)
                    {
                        DestroyImmediate(hDAdditionalLightDataLW);
                    }

                    GameObject underwaterProbeLW = GameObject.Find("Underwater Reflection Probe");
                    if (underwaterProbeLW != null)
                    {
                        DestroyImmediate(underwaterProbeLW);
                    }

                    AdditionalShadowData additionalShadowDataLW = FindObjectOfType<AdditionalShadowData>();
                    if (additionalShadowDataLW != null)
                    {
                        DestroyImmediate(additionalShadowDataLW);
                    }

                    HDAdditionalCameraData hDAdditionalCameraDataLW = FindObjectOfType<HDAdditionalCameraData>();
                    if (hDAdditionalCameraDataLW != null)
                    {
                        DestroyImmediate(hDAdditionalCameraDataLW);
                    }

                    GameObject planarReflectionProbeObjectLW = GameObject.Find("Water Planar Reflections");
                    if (planarReflectionProbeObjectLW != null)
                    {
                        DestroyImmediate(planarReflectionProbeObjectLW);
                    }

                    GameObject densityVolumeLW = GameObject.Find("Density Volume");
                    if (densityVolumeLW != null)
                    {
                        DestroyImmediate(densityVolumeLW);
                    }

                    GameObject hdWindSettingsLW = GameObject.Find("High Definition Wind");
                    if (hdWindSettingsLW != null)
                    {
                        DestroyImmediate(hdWindSettingsLW);
                    }
#endif
                    #endregion
                    break;
                case AmbientSkiesConsts.RenderPipelineSettings.HighDefinition:
                    #region High Definition

                    GameObject horizonHDObject = GameObject.Find("Ambient Skies Horizon");
                    if (horizonHDObject != null)
                    {
                        DestroyImmediate(horizonHDObject);
                    }

                    SkyboxUtils.DestroyParent("Ambient Skies Environment");

#if LWPipeline && UNITY_2018_3_OR_NEWER
                    LWRPAdditionalCameraData lWRPAdditionalCameraDataHD =FindObjectOfType<LWRPAdditionalCameraData>();
                if (lWRPAdditionalCameraDataHD != null)
                {
                    DestroyImmediate(lWRPAdditionalCameraDataHD);
                }
                LWRPAdditionalLightData lWRPAdditionalLightDataHD = FindObjectOfType<LWRPAdditionalLightData>();
                if (lWRPAdditionalLightDataHD != null)
                {
                    DestroyImmediate(lWRPAdditionalLightDataHD);
                }
#endif
                #endregion
                    break;
            }

            if (m_showDebug)
            {
                Debug.Log("Components Removed Successfully");
            }
            m_currentStatus = PipelineStatus.LoadPackageList;
        }

        /// <summary>
        /// List packages via package manager and starts update
        /// </summary>
        private void LoadPackageList()
        {
            string[] packages = new string[0];

#if UNITY_2018_1
            packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2018_1.txt", SearchOption.AllDirectories);
#elif UNITY_2018_2
            packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2018_2.txt", SearchOption.AllDirectories);
#elif UNITY_2018_3
            if (m_pipelineToInstall == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2018_3_Builtin.txt", SearchOption.AllDirectories);
            }
            else if (m_pipelineToInstall == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
            {
                packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2018_3_LW.txt", SearchOption.AllDirectories);
            }
            else if (m_pipelineToInstall == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2018_3_HD.txt", SearchOption.AllDirectories);
            }
#elif UNITY_2018_4
            if (m_pipelineToInstall == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2018_4_Builtin.txt", SearchOption.AllDirectories);
            }
            else if (m_pipelineToInstall == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
            {
                packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2018_4_LW.txt", SearchOption.AllDirectories);
            }
            else if (m_pipelineToInstall == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2018_4_HD.txt", SearchOption.AllDirectories);
            }
#elif UNITY_2019_1_OR_NEWER
            if (m_pipelineToInstall == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2019_1_Builtin.txt", SearchOption.AllDirectories);
            }
            else if (m_pipelineToInstall == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
            {
                packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2019_1_LW.txt", SearchOption.AllDirectories);
            }
            else if (m_pipelineToInstall == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                packages = Directory.GetFiles(Application.dataPath, "Ambient Skies PackageImportList2019_1_HD.txt", SearchOption.AllDirectories);
            }
#endif

            if (packages.Length == 0)
            {
                Debug.LogError("[Pipeline Utils] Couldn't find packages list.");
                m_currentStatusMessage = "Couldn't find packages list.";
                m_currentStatus = PipelineStatus.FailedWithErrors;
                return;
            }

            string packageListPath = packages[0];
            m_packagesToAdd = new List<PackageEntry>();
            string[] content = File.ReadAllLines(packageListPath);
            foreach (var line in content)
            {
                var split = line.Split('@');
                PackageEntry entry = new PackageEntry();

                entry.name = split[0];
                entry.version = split.Length > 1 ? split[1] : null;

                m_packagesToAdd.Add(entry);
            }
            m_currentStatus = PipelineStatus.ProcessInstalledPackageList;
            m_pmListRequest = Client.List();
        }

        /// <summary>
        /// Process the installed package list
        /// </summary>
        private void ProcessInstalledPackageList()
        {
            if (m_pmListRequest != null)
            {
                if (m_pmListRequest.IsCompleted)
                {
                    bool[] foundPackages = new bool[m_packagesToAdd.Count];
                    for (int i = 0; i < foundPackages.Length; ++i)
                    {
                        foundPackages[i] = false;
                    }
                    foreach (var package in m_pmListRequest.Result)
                    {
                        for (int i = 0; i < foundPackages.Length; ++i)
                        {
                            if (package.packageId.Contains(m_packagesToAdd[i].name))
                            {
                                foundPackages[i] = true;
                                if (m_showDebug)
                                {
                                    Debug.Log("[Pipeline Utils] " + m_packagesToAdd[i].name + " already imported.");
                                }
                            }
                        }
                    }
                    for (int i = 0; i < foundPackages.Length; ++i)
                    {
                        if (!foundPackages[i])
                        {
                            m_missingPackages.Push(i);
                        }
                    }
                    m_pmListRequest = null;
                    m_currentStatus = PipelineStatus.InstallingPackages;
                }
                else if (m_pmListRequest.Error != null)
                {
                    Debug.LogError("[Pipeline Utils] Error : " + m_pmListRequest.Error.message);
                    m_currentStatus = PipelineStatus.FailedWithErrors;
                    m_currentStatusMessage = m_pmListRequest.Error.message;
                    m_pmListRequest = null;
                }
            }
        }

        /// <summary>
        /// Install packages
        /// </summary>
        private void InstallPackages()
        {
            if (!m_isAddingPackage)
            {
                if (m_missingPackages.Count == 0)
                {
                    m_currentStatus = PipelineStatus.InstallingWaterShaders;
                    m_currentStatusMessage = "";
                }
                else
                {
                    int package = m_missingPackages.Pop();
                    string name = m_packagesToAdd[package].name;
                    if (m_packagesToAdd[package].version != null)
                    {
                        name += "@" + m_packagesToAdd[package].version;
                    }

                    if (m_showDebug)
                    {
                        Debug.Log("[Pipeline Utils] - Adding package " + name);
                    }
                    m_isAddingPackage = true;
                    m_currentStatusMessage = name;
                    m_pmAddRequest = Client.Add(name);
                    Repaint();
                }
            }
            else
            {
                if (m_pmAddRequest.Status == StatusCode.Failure)
                {
                    m_currentStatus = PipelineStatus.FailedWithErrors;
                    if (m_pmAddRequest.Error != null)
                    {
                        if (!string.IsNullOrEmpty(m_pmAddRequest.Error.message))
                        {
                            Debug.LogError("[Pipeline Utils] - Add Package Error : " + m_pmAddRequest.Error.message);
                            m_currentStatusMessage = m_pmAddRequest.Error.message;
                        }
                        else
                        {
                            Debug.LogError("[Pipeline Utils] - Add Package Failure");
                            m_currentStatusMessage = "Unknown failure";
                        }
                    }
                    else
                    {
                        Debug.LogError("[Pipeline Utils] - Add Package Failure");
                        m_currentStatusMessage = "Unknown failure";
                    }
                    m_isAddingPackage = false;
                    m_pmAddRequest = null;
                }
                else if (m_pmAddRequest.Status == StatusCode.Success)
                {
                    if (m_showDebug && m_pmAddRequest.Result != null)
                    {
                        Debug.Log("[Pipeline Utils] - Added package " + m_pmAddRequest.Result.displayName);
                    }
                    m_currentStatusMessage = "";
                    m_isAddingPackage = false;
                    m_pmAddRequest = null;
                }
            }
        }

        /// <summary>
        /// Install / deinstall LW and HD water shaders
        /// </summary>
        private void InstallWaterShaders()
        {
            string waterLW = GetAssetPath("Simple Water Sample LW");
            string waterHD = GetAssetPath("Simple Water Sample HD");
            string water2019HD = GetAssetPath("2019 Water Sample HD");

            switch (m_pipelineToInstall)
            {
                case AmbientSkiesConsts.RenderPipelineSettings.BuiltIn:
                    {
                        if (!string.IsNullOrEmpty(waterLW))
                        {
                            if (waterLW.Contains(".shader"))
                            {
                                FileUtil.MoveFileOrDirectory(waterLW, waterLW.Replace(".shader", ".txt"));
                            }
                        }
                        if (!string.IsNullOrEmpty(waterHD))
                        {
                            if (waterHD.Contains(".shader"))
                            {
                                FileUtil.MoveFileOrDirectory(waterHD, waterHD.Replace(".shader", ".txt"));
                            }
                        }
                        if (!string.IsNullOrEmpty(water2019HD))
                        {
                            if (water2019HD.Contains(".shader"))
                            {
                                FileUtil.MoveFileOrDirectory(water2019HD, water2019HD.Replace(".shader", ".txt"));
                            }
                        }
                        break;
                    }
                case AmbientSkiesConsts.RenderPipelineSettings.Lightweight:
                    {
                        if (!string.IsNullOrEmpty(waterLW))
                        {
                            if (waterLW.Contains(".txt"))
                            {
                                FileUtil.MoveFileOrDirectory(waterLW, waterLW.Replace(".txt", ".shader"));
                            }
                        }
                        if (!string.IsNullOrEmpty(waterHD))
                        {
                            if (waterHD.Contains(".shader"))
                            {
                                FileUtil.MoveFileOrDirectory(waterHD, waterHD.Replace(".shader", ".txt"));
                            }
                        }
                        if (!string.IsNullOrEmpty(water2019HD))
                        {
                            if (water2019HD.Contains(".shader"))
                            {
                                FileUtil.MoveFileOrDirectory(water2019HD, water2019HD.Replace(".shader", ".txt"));
                            }
                        }
                        break;
                    }
                case AmbientSkiesConsts.RenderPipelineSettings.HighDefinition:
                    {
                        if (!string.IsNullOrEmpty(waterLW))
                        {
                            if (waterLW.Contains(".shader"))
                            {
                                FileUtil.MoveFileOrDirectory(waterLW, waterLW.Replace(".shader", ".txt"));
                            }
                        }
#if !UNITY_2019_1_OR_NEWER
                        if (!string.IsNullOrEmpty(waterHD))
                        {
                            if (waterHD.Contains(".txt"))
                            {
                                FileUtil.MoveFileOrDirectory(waterHD, waterHD.Replace(".txt", ".shader"));
                            }
                        }
#else
                        if (!string.IsNullOrEmpty(water2019HD))
                        {
                            if (water2019HD.Contains(".txt"))
                            {
                                FileUtil.MoveFileOrDirectory(water2019HD, water2019HD.Replace(".txt", ".shader"));
                            }
                        }
#endif
                        break;
                    }
            }

            m_currentStatus = PipelineStatus.UpdatingMaterials;
            m_currentStatusMessage = "";
        }

        /// <summary>
        /// Update materials
        /// </summary>
        private void UpdateMaterials()
        {
            if (!m_updateMaterials)
            {
                m_currentStatus = PipelineStatus.DeletingPackages;
                m_currentStatusMessage = "";
                return;
            }

            //First check if all the required shaders can be found in the project
            Shader shaderBuiltInStandard = Shader.Find("Standard");
            Shader shaderBuiltInStandardSpecular = Shader.Find("Standard (Specular setup)");

            Shader shaderLWRPLit = Shader.Find("Lightweight Render Pipeline/Lit");
            Shader shaderLWRPSimpleLit = Shader.Find("Lightweight Render Pipeline/Simple Lit");

            Shader shaderHDRPLit = Shader.Find("HDRP/Lit");
            Shader shaderHDRPLitTesselation = Shader.Find("HDRP/LitTessellation");

            bool builtInShadersPresent = false;
            bool LWRPShadersPresent = false;
            bool HDRPShadersPresent = false;

            if (shaderBuiltInStandard != null && shaderBuiltInStandardSpecular != null)
            {
                builtInShadersPresent = true;
            }

            if (shaderLWRPLit != null && shaderLWRPSimpleLit != null)
            {
                LWRPShadersPresent = true;
            }

            if (shaderHDRPLit != null && shaderHDRPLitTesselation != null)
            {
                HDRPShadersPresent = true;
            }

            var allMaterialGUIDS = AssetDatabase.FindAssets("t:Material");

            if (m_originalPipeline == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                switch(m_pipelineToInstall)
                {
                    case AmbientSkiesConsts.RenderPipelineSettings.Lightweight:
                        if (LWRPShadersPresent)
                        {
#if UNITY_2018_3_OR_NEWER
                            EditorApplication.ExecuteMenuItem("Edit/Render Pipeline/Upgrade Project Materials to LightweightRP Materials");
#endif                          
                        }
                        else
                        {
                            m_currentStatus = PipelineStatus.FailedWithErrors;
                            m_currentStatusMessage = "Missing LWRP shaders";
                            Debug.LogError("Trying to switch to LWRP rendering, but the LWRP standard shaders are not present in this project! Shaders can't be updated.");
                        }
                        break;
                    case AmbientSkiesConsts.RenderPipelineSettings.HighDefinition:
                        if (HDRPShadersPresent)
                        {
#if UNITY_2018_3_OR_NEWER
                            EditorApplication.ExecuteMenuItem("Edit/Render Pipeline/Upgrade Project Materials to High Definition Materials");
#endif
                        }
                        else
                        {
                            m_currentStatus = PipelineStatus.FailedWithErrors;
                            m_currentStatusMessage = "Missing HDRP shaders";
                            Debug.LogError("Trying to switch to HDRP rendering, but the HDRP standard shaders are not present in this project! Shaders can't be updated.");
                        }
                        break;
                }
            }
            else
            {
                switch (m_pipelineToInstall)
                {
                    case AmbientSkiesConsts.RenderPipelineSettings.BuiltIn:
                        if (builtInShadersPresent)
                        {
                            foreach (string materialGuid in allMaterialGUIDS)
                            {
                                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialGuid));
                                if (LWRPShadersPresent)
                                {
                                    if (material.shader == shaderLWRPLit) material.shader = shaderBuiltInStandard;
                                    if (material.shader == shaderLWRPSimpleLit) material.shader = shaderBuiltInStandardSpecular;
                                }
                                if (HDRPShadersPresent)
                                {
                                    if (material.shader == shaderHDRPLit) material.shader = shaderBuiltInStandard;
                                    if (material.shader == shaderHDRPLitTesselation) material.shader = shaderBuiltInStandardSpecular;
                                }
                            }
                        }
                        else
                        {
                            m_currentStatus = PipelineStatus.FailedWithErrors;
                            m_currentStatusMessage = "Missing builtin shaders";
                            Debug.LogError("Trying to switch to built-in rendering, but the built in shaders are not present in this project! Shaders can't be updated.");
                            return;
                        }
                        break;
                    case AmbientSkiesConsts.RenderPipelineSettings.Lightweight:
                        if (LWRPShadersPresent)
                        {
                            foreach (string materialGuid in allMaterialGUIDS)
                            {
                                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialGuid));
                                if (builtInShadersPresent)
                                {
                                    if (material.shader == shaderBuiltInStandard) material.shader = shaderLWRPLit;
                                    if (material.shader == shaderBuiltInStandardSpecular) material.shader = shaderLWRPSimpleLit;
                                }
                                if (HDRPShadersPresent)
                                {
                                    if (material.shader == shaderHDRPLit) material.shader = shaderLWRPLit;
                                    if (material.shader == shaderHDRPLitTesselation) material.shader = shaderLWRPSimpleLit;
                                }
                            }
                        }
                        else
                        {
                            m_currentStatus = PipelineStatus.FailedWithErrors;
                            m_currentStatusMessage = "Missing LWRP shaders";
                            Debug.LogError("Trying to switch to LWRP rendering, but the LWRP standard shaders are not present in this project! Shaders can't be updated.");
                        }
                        break;
                    case AmbientSkiesConsts.RenderPipelineSettings.HighDefinition:
                        if (HDRPShadersPresent)
                        {
                            foreach (string materialGuid in allMaterialGUIDS)
                            {
                                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialGuid));
                                if (builtInShadersPresent)
                                {
                                    if (material.shader == shaderBuiltInStandard) material.shader = shaderHDRPLit;
                                    if (material.shader == shaderBuiltInStandardSpecular) material.shader = shaderHDRPLit;
                                }
                                if (LWRPShadersPresent)
                                {
                                    if (material.shader == shaderLWRPLit) material.shader = shaderHDRPLit;
                                    if (material.shader == shaderLWRPSimpleLit) material.shader = shaderHDRPLit;
                                }
                            }
                        }
                        else
                        {
                            m_currentStatus = PipelineStatus.FailedWithErrors;
                            m_currentStatusMessage = "Missing HDRP shaders";
                            Debug.LogError("Trying to switch to HDRP rendering, but the HDRP standard shaders are not present in this project! Shaders can't be updated.");
                        }
                        break;
                }
            }

            AssetDatabase.Refresh();
            m_currentStatus = PipelineStatus.DeletingPackages;
            m_currentStatusMessage = "";
        }

        /// <summary>
        /// Determine what to delete and then delete it
        /// </summary>
        private void DeletePackages()
        {
            if (!m_isRemovingPackage)
            {
                bool removeLW = false;
                bool removeHD = false;
                switch (m_pipelineToInstall)
                {                   
                    case AmbientSkiesConsts.RenderPipelineSettings.BuiltIn:
                        removeLW = true;
                        removeHD = true;
                        break;
                    case AmbientSkiesConsts.RenderPipelineSettings.Lightweight:
                        removeHD = true;
                        break;
                    case AmbientSkiesConsts.RenderPipelineSettings.HighDefinition:
                        removeLW = true;
                        break;
                }

                //Check for & remove LWRP
                if (removeLW && Shader.Find("Lightweight Render Pipeline/Lit"))
                {
                    if (m_showDebug)
                    {
                        Debug.Log("[Pipeline Utils] - Removing com.unity.render-pipelines.lightweight");
                    }
                    m_isRemovingPackage = true;
                    m_currentStatusMessage = "com.unity.render-pipelines.lightweight";
                    m_pmRemoveRequest = Client.Remove("com.unity.render-pipelines.lightweight");
                    Repaint();
                    return;
                }

                //Check for remove HDRP
                if (removeHD && Shader.Find("HDRP/Lit"))
                {
                    if (m_showDebug)
                    {
                        Debug.Log("[Pipeline Utils] - Removing com.unity.render-pipelines.high-definition");
                    }
                    m_isRemovingPackage = true;
                    m_currentStatusMessage = "com.unity.render-pipelines.high-definition";
                    m_pmRemoveRequest = Client.Remove("com.unity.render-pipelines.high-definition");
                    Repaint();
                    return;
                }

                //Outa here
                m_isRemovingPackage = false;
                m_currentStatus = PipelineStatus.UpdatingBuildSettings;
                m_currentStatusMessage = "";
            }
            else
            {
                if (m_pmRemoveRequest.Status == StatusCode.Failure)
                {
                    m_currentStatus = PipelineStatus.FailedWithErrors;
                    if (m_pmRemoveRequest.Error != null)
                    {
                        Debug.LogError("[Pipeline Utils] - Remove Package Error : " + m_pmRemoveRequest.Error.message);
                        m_currentStatusMessage = m_pmRemoveRequest.Error.message;
                    }
                    else
                    {
                        Debug.LogError("[Pipeline Utils] - Remove Package Failure");
                        m_currentStatusMessage = "Unknown failure";
                    }
                    m_isRemovingPackage = false;
                    m_pmRemoveRequest = null;
                }
                else if (m_pmRemoveRequest.Status == StatusCode.Success)
                {
                    if (m_showDebug && !string.IsNullOrEmpty(m_pmRemoveRequest.PackageIdOrName))
                    {
                        Debug.Log("[Pipeline Utils] - Removed package " + m_pmRemoveRequest.PackageIdOrName);
                    }
                    m_currentStatusMessage = "";
                    m_isRemovingPackage = false;
                    m_pmRemoveRequest = null;
                }
            }
        }

        /// <summary>
        /// Update the build settings
        /// </summary>
        private void UpdateBuildSettings()
        {
            bool isChanged = false;
            switch (m_pipelineToInstall)
            {
                case AmbientSkiesConsts.RenderPipelineSettings.BuiltIn:
                    {
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
                        if (isChanged)
                        {
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
                            return;
                        }
                        break;
                    }
                case AmbientSkiesConsts.RenderPipelineSettings.Lightweight:
                    {
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
                        if (isChanged)
                        {
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
                            return;
                        }
                        break;
                    }
                case AmbientSkiesConsts.RenderPipelineSettings.HighDefinition:
                    {
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
                        if (isChanged)
                        {
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
                            return;
                        }
                        break;
                    }
            }

            m_currentStatus = PipelineStatus.FinalizingEnvironment;
            m_currentStatusMessage = "";
        }

        /// <summary>
        /// Now perform final version specific environmental setup
        /// </summary>
        private void FinalizeEnvironment()
        {
            m_finalizeEnvironment = false;
            if (m_finalizeEnvironment)
            {
                switch (m_pipelineToInstall)
                {
                    case AmbientSkiesConsts.RenderPipelineSettings.BuiltIn:
                        #region Built-In                                       

                        if (m_showDebug)
                        {
                            Debug.Log(m_profiles);
                            Debug.Log(m_selectedSkyboxProfile);
                            Debug.Log(m_selectedPostProcessingProfile);
                        }
                        if (m_profiles == null)
                        {
                            if (m_showDebug)
                            {
                                Debug.LogError("Unable to finalise the environment, missing Ambient Skies Profiles references");
                            }
                        }
                        if (m_finalizeEnvironment)
                        {
                            if (m_selectedSkyboxProfile != null)
                            {
                                AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, m_profiles, m_selectedSkyboxProfileIndex, m_pipelineToInstall, "High Definition Environment Volume", "Ambient Skies HD Volume Profile", m_updateVisualEnvironment, m_updateFog, m_updateShadows, m_updateAmbientLight, m_updateScreenSpaceReflections, m_updateScreenSpaceRefractions, m_updateSun);
                            }
                            else
                            {
                                if (m_showDebug)
                                {
                                    Debug.LogError("Unable to finalise the environment, missing Skybox references");
                                }
                            }
                            if (m_selectedPostProcessingProfile != null)
                            {
                                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfile, m_selectedPostProcessingProfileIndex, false, m_updateAO, m_updateAutoExposure, m_updateBloom, m_updateChromatic, m_updateColorGrading, m_updateDOF, m_updateGrain, m_updateLensDistortion, m_updateMotionBlur, m_updateSSR, m_updateVignette, m_updatePanini, m_updateTargetPlaform);
                            }
                            else
                            {
                                if (m_showDebug)
                                {
                                    Debug.LogError("Unable to finalise the environment, missing Post Porocessing references");
                                }
                            }
                        }

                        #endregion
                        break;
                    case AmbientSkiesConsts.RenderPipelineSettings.Lightweight:
                        #region Lightweight                    

                        if (m_showDebug)
                        {
                            Debug.Log(m_profiles);
                            Debug.Log(m_selectedSkyboxProfile);
                            Debug.Log(m_selectedPostProcessingProfile);
                        }
                        if (m_profiles == null)
                        {
                            if (m_showDebug)
                            {
                                Debug.LogError("Unable to finalise the environment, missing Ambient Skies Profiles references");
                            }
                        }
                        if (m_finalizeEnvironment)
                        {
                            if (m_selectedSkyboxProfile != null)
                            {
                                AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, m_profiles, m_selectedSkyboxProfileIndex, m_pipelineToInstall, "High Definition Environment Volume", "Ambient Skies HD Volume Profile", m_updateVisualEnvironment, m_updateFog, m_updateShadows, m_updateAmbientLight, m_updateScreenSpaceReflections, m_updateScreenSpaceRefractions, m_updateSun);
                            }
                            else
                            {
                                if (m_showDebug)
                                {
                                    Debug.LogError("Unable to finalise the environment, missing Skybox references");
                                }
                            }
                            if (m_selectedPostProcessingProfile != null)
                            {
                                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfile, m_selectedPostProcessingProfileIndex, false, m_updateAO, m_updateAutoExposure, m_updateBloom, m_updateChromatic, m_updateColorGrading, m_updateDOF, m_updateGrain, m_updateLensDistortion, m_updateMotionBlur, m_updateSSR, m_updateVignette, m_updatePanini, m_updateTargetPlaform);
                            }
                            else
                            {
                                if (m_showDebug)
                                {
                                    Debug.LogError("Unable to finalise the environment, missing Post Porocessing references");
                                }
                            }
                        }

                        #endregion
                        break;
                    case AmbientSkiesConsts.RenderPipelineSettings.HighDefinition:
                        #region High Definition    

                        if (m_showDebug)
                        {
                            Debug.Log(m_profiles);
                            Debug.Log(m_selectedSkyboxProfile);
                            Debug.Log(m_selectedPostProcessingProfile);
                        }
                        if (m_profiles == null)
                        {
                            if (m_showDebug)
                            {
                                Debug.LogError("Unable to finalise the environment, missing Ambient Skies Profiles references");
                            }
                        }
                        if (m_finalizeEnvironment)
                        {
                            if (m_selectedSkyboxProfile != null)
                            {
                                AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, m_profiles, m_selectedSkyboxProfileIndex, m_pipelineToInstall, "High Definition Environment Volume", "Ambient Skies HD Volume Profile", m_updateVisualEnvironment, m_updateFog, m_updateShadows, m_updateAmbientLight, m_updateScreenSpaceReflections, m_updateScreenSpaceRefractions, m_updateSun);
                            }
                            else
                            {
                                if (m_showDebug)
                                {
                                    Debug.LogError("Unable to finalise the environment, missing Skybox references");
                                }
                            }
                            if (m_selectedPostProcessingProfile != null)
                            {
                                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfile, m_selectedPostProcessingProfileIndex, false, m_updateAO, m_updateAutoExposure, m_updateBloom, m_updateChromatic, m_updateColorGrading, m_updateDOF, m_updateGrain, m_updateLensDistortion, m_updateMotionBlur, m_updateSSR, m_updateVignette, m_updatePanini, m_updateTargetPlaform);
                            }
                            else
                            {
                                if (m_showDebug)
                                {
                                    Debug.LogError("Unable to finalise the environment, missing Post Porocessing references");
                                }
                            }
                        }

                        m_profiles.m_selectedRenderPipeline = m_pipelineToInstall;
                        #endregion
                        break;
                }
            }

            m_currentStatus = PipelineStatus.CleanUp;
        }

        /// <summary>
        /// Only if pipeline is HD it'll recall the setup for the lighting setup to fix any bugs that wasn't called during the process
        /// </summary>
        private void CleanUpScene()
        {
            if (m_pipelineToInstall == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                //Get main probe and check if HD data is on the probe
                GameObject reflectionProbe = GameObject.Find("Global Reflection Probe");
                if (reflectionProbe != null)
                {
#if HDPipeline
                    if (reflectionProbe.GetComponent<HDAdditionalReflectionData>() == null)
                    {
                        reflectionProbe.AddComponent<HDAdditionalReflectionData>();
                    }
#endif
                }

                //Reapply lighting to fix glitches and bugs to ambient lighting
                if (m_profiles != null && m_selectedSkyboxProfile != null && m_selectedPostProcessingProfile != null)
                {
                    if (m_showDebug)
                    {
                        Debug.Log(m_profiles);
                        Debug.Log(m_selectedSkyboxProfile);
                        Debug.Log(m_selectedPostProcessingProfile);
                    }

                    if (m_finalizeEnvironment)
                    {
                        AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, m_profiles, m_selectedSkyboxProfileIndex, m_pipelineToInstall, "High Definition Environment Volume", "Ambient Skies HD Volume Profile", m_updateVisualEnvironment, m_updateFog, m_updateShadows, m_updateAmbientLight, m_updateScreenSpaceReflections, m_updateScreenSpaceRefractions, m_updateSun);
                        PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfile, m_selectedPostProcessingProfileIndex, false, m_updateAO, m_updateAutoExposure, m_updateBloom, m_updateChromatic, m_updateColorGrading, m_updateDOF, m_updateGrain, m_updateLensDistortion, m_updateMotionBlur, m_updateSSR, m_updateVignette, m_updatePanini, m_updateTargetPlaform);
                    }
                }
                else
                {
                    if (m_showDebug)
                    {
                        Debug.LogError("Unable to finalise the environment, missing references");
                    }
                }

#if HDPipeline
                //Bake reflection probes
                HDAdditionalReflectionData[] reflectionData = FindObjectsOfType<HDAdditionalReflectionData>();
                foreach (HDAdditionalReflectionData data in reflectionData)
                {
#if UNITY_2018_3 || UNITY_2018_4
                    data.reflectionProbe.RenderProbe();
#endif
                }
#endif              
            }
            else
            {
                //Bake reflection probes
                ReflectionProbe[] probes = FindObjectsOfType<ReflectionProbe>();
                foreach (ReflectionProbe probe in probes)
                {
                    probe.RenderProbe();
                }

                GameObject volumeObject = GameObject.Find("High Definition Environment Volume");
                if (volumeObject != null)
                {
                    DestroyImmediate(volumeObject);
                }
            }

            Component[] components = FindObjectsOfType<Component>();
            foreach (Component component in components)
            {
                if (component == null)
                {
                    DestroyImmediate(component);
                }
            }

            m_currentStatus = PipelineStatus.Success;
            m_currentStatusMessage = "";
        }
#endif

        #endregion

        #region Generic Utils

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>The path or null</returns>
        private string GetAssetPath(string name)
        {
            string[] assets = AssetDatabase.FindAssets(name, null);
            if (assets.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assets[0]);
            }
            return null;
        }
        #endregion
    }
}