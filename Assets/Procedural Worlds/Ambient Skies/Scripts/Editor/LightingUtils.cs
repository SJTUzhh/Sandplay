//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif

namespace AmbientSkies
{
    /// <summary>
    /// Handy lighting utilities
    /// </summary>

    public static class LightingUtils
    {
        #region Utils

        #region Get/Set Profile

        /// <summary>
        /// Load the selected profile and apply to the lightmap settings
        /// </summary>
        /// <param name="profiles">The profiles object</param>
        /// <param name="profileIndex">The zero based index to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileIndex(AmbientSkyProfiles skyProfiles, AmbientLightingProfile profiles, int profileName, bool useDefaults, bool updateRealtime, bool updateBaked)
        {
            if (skyProfiles == null)
            {
                Debug.LogError("Missing the sky profiles (m_profiles from Ambient Skies) try reopening ambient skies and try again. Exiting");
                return;
            }

            if (profiles == null)
            {
                Debug.LogError("Missing the sky profiles (m_selectedLightingProfile from Ambient Skies) try reopening ambient skies and try again. Exiting");
                return;
            }

            AmbientLightingProfile p = profiles;
            if (p == null)
            {
                Debug.LogWarning("Invalid profile index selected, can not apply lightmapping settings!");
                return;
            }

            SetLightmapSettings(skyProfiles, p, false, updateRealtime, updateBaked);
        }

        /// <summary>
        /// Get current profile index that has the profile name
        /// </summary>
        /// <param name="profiles">Profile list to search</param>
        /// <param name="profileName">Name of profile to find</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromProfileName(AmbientSkyProfiles profiles, string profileName)
        {
            if (profiles.m_lightingProfiles.Count < 0)
            {
                return -1;
            }
            else
            {
                for (int idx = 0; idx < profiles.m_lightingProfiles.Count; idx++)
                {
                    if (profiles.m_lightingProfiles[idx].name == profileName)
                    {
                        return idx;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Load the selected profile and apply it
        /// </summary>
        /// <param name="profile">The profiles object</param>
        /// <param name="profileName">The name of the profile to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileName(AmbientSkyProfiles skyProfiles, AmbientSkyProfiles profile, string profileName, bool useDefaults, bool updateRealtime, bool updateBaked)
        {
            AmbientLightingProfile p = profile.m_lightingProfiles.Find(x => x.name == profileName);
            if (p == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply post processing profile!");
                return;
            }
            SetLightmapSettings(skyProfiles, p, useDefaults, updateRealtime, updateBaked);
        }

        #endregion

        #region Set Lightmap Settings

        /// <summary>
        /// Sets up the lighting
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="useDefaults"></param>
        public static void SetLightmapSettings(AmbientSkyProfiles skyProfiles, AmbientLightingProfile profile, bool useDefaults, bool updateRealtime, bool updateBaked)
        {
            //Clear console if required
            if (skyProfiles.m_smartConsoleClean)
            {
                SkyboxUtils.ClearLog();
                Debug.Log("Console cleared successfully");
            }

            if (profile.autoLightmapGeneration)
            {
                Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.Iterative;
            }
            else
            {
                Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
            }

            Lightmapping.bounceBoost = profile.lightBoostIntensity;
            Lightmapping.indirectOutputScale = profile.lightIndirectIntensity;

            Lightmapping.realtimeGI = profile.realtimeGlobalIllumination;
            Lightmapping.bakedGI = profile.bakedGlobalIllumination;

            if (profile.realtimeGlobalIllumination)
            {
                if (updateRealtime)
                {
                    if (skyProfiles.m_showFunctionDebugsOnly)
                    {
                        Debug.Log("Updating: SetRealtimeGISettings()");
                    }

                    SetRealtimeGISettings(profile);
                }
            }

            if (profile.bakedGlobalIllumination)
            {
                if (updateBaked)
                {
                    if (skyProfiles.m_showFunctionDebugsOnly)
                    {
                        Debug.Log("Updating: SetBakedGISettings()");
                    }

                    SetBakedGISettings(profile);
                }
            }
        }

        /// <summary>
        /// Sets the realtime lightmapping settings
        /// </summary>
        /// <param name="profile"></param>
        public static void SetRealtimeGISettings(AmbientLightingProfile profile)
        {
            LightmapEditorSettings.realtimeResolution = profile.indirectRelolution;
            if (profile.useDirectionalMode)
            {
                LightmapSettings.lightmapsMode = LightmapsMode.CombinedDirectional;
            }
            else if (!profile.useDirectionalMode)
            {
                LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
            }
        }

        /// <summary>
        /// Sets the baked lightmapping settings
        /// </summary>
        /// <param name="profile"></param>
        public static void SetBakedGISettings(AmbientLightingProfile profile)
        {
            LightmapEditorSettings.bakeResolution = profile.lightmapResolution;
            LightmapEditorSettings.padding = profile.lightmapPadding;
#if UNITY_2018_3_OR_NEWER
            LightmapEditorSettings.filteringMode = profile.filterMode;
#endif

#if !UNITY_2018_3_OR_NEWER
            if (profile.lightmappingMode == AmbientSkiesConsts.LightmapperMode.Enlighten)
            {
                LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.Enlighten;
            }

            else if (profile.lightmappingMode == AmbientSkiesConsts.LightmapperMode.ProgressiveCPU)
            {
                LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveCPU;
            }
#else
            if (profile.lightmappingMode == AmbientSkiesConsts.LightmapperMode.Enlighten)
            {
                LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.Enlighten;
            }

            else if (profile.lightmappingMode == AmbientSkiesConsts.LightmapperMode.ProgressiveCPU)
            {
                LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveCPU;
            }
            else
            {
                LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveGPU;
            }
#endif

            if (profile.useHighResolutionLightmapSize)
            {
#if !UNITY_2018_1_OR_NEWER
                LightmapEditorSettings.maxAtlasHeight = 4096;
#endif
                LightmapEditorSettings.maxAtlasSize = 4096;
            }
            else if (!profile.useHighResolutionLightmapSize)
            {
#if !UNITY_2018_1_OR_NEWER
                LightmapEditorSettings.maxAtlasHeight = 1024;
#endif
                LightmapEditorSettings.maxAtlasSize = 1024;
            }

            LightmapEditorSettings.textureCompression = profile.compressLightmaps;
            LightmapEditorSettings.enableAmbientOcclusion = profile.ambientOcclusion;
            LightmapEditorSettings.aoMaxDistance = profile.maxDistance;
            LightmapEditorSettings.aoExponentIndirect = profile.indirectContribution;
            LightmapEditorSettings.aoExponentDirect = profile.directContribution;
        }

        #endregion

        #region Lighting Buttons

        /// <summary>
        /// Clears lightmap data from active scene
        /// </summary>
        public static void ClearLightmapData()
        {
            Lightmapping.Clear();
        }

        /// <summary>
        /// Cancel lighting bake
        /// </summary>
        public static void CancelLighting()
        {
            Lightmapping.Cancel();
        }

        /// <summary>
        /// Set linear deffered lighting (the best for outdoor scenes)
        /// </summary>
        /// <param name="ambientSkiesEditor"></param>
        public static void SetLinearDeferredLighting(AmbientSkiesEditorWindow ambientSkiesEditor)
        {
            if (ambientSkiesEditor != null)
            {
                ambientSkiesEditor.Close();
            }

            PlayerSettings.colorSpace = ColorSpace.Linear;
#if UNITY_5_5_OR_NEWER
            var tier1 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
            tier1.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1, tier1);
            var tier2 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2);
            tier2.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2, tier2);
            var tier3 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3);
            tier3.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3, tier3);
#else
            PlayerSettings.renderingPath = RenderingPath.DeferredShading;
#endif
        }

        /// <summary>
        /// Removes the global reflection probe
        /// </summary>
        public static void RemoveGlobalReflectionProbe()
        {
            GameObject reflectionProbeObject = GameObject.Find("Global Reflection Probe");
            if (reflectionProbeObject != null)
            {
                Object.DestroyImmediate(reflectionProbeObject);
            }
        }

        /// <summary>
        /// Add static global reflection probes
        /// </summary>
        public static void AddGlobalReflectionProbeStatic()
        {
            GameObject theParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);

            //Get the camera
            GameObject mainCamera = SkyboxUtils.GetOrCreateMainCamera();

            //Sets up the probe to terrain size & location
            Vector3 probeLocation = Vector3.zero;
            Vector3 probeSize = new Vector3(3000f, 1250f, 3000f);
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                probeSize = terrain.terrainData.size;
                probeSize.x += 50f;
                probeSize.z += 50f;

                probeLocation = terrain.transform.position + (probeSize * 0.5f);
                probeLocation.y = terrain.SampleHeight(probeLocation) + 100f;
            }

            GameObject globalProbeGo = GameObject.Find("Global Reflection Probe");
            if (globalProbeGo == null)
            {
                globalProbeGo = new GameObject("Global Reflection Probe");
                globalProbeGo.AddComponent<ReflectionProbe>();
            }

            globalProbeGo.transform.parent = theParent.transform;
            if (terrain != null)
            {
                globalProbeGo.transform.localPosition = probeLocation;
            }
            else
            {
                globalProbeGo.transform.localPosition = new Vector3(0f, 60f, 0f);
            }

            globalProbeGo.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
            ReflectionProbe globalProbe = globalProbeGo.GetComponent<ReflectionProbe>();
            Camera camera = mainCamera.GetComponent<Camera>();

            //Configure probe settings
            if (globalProbe != null)
            {
                globalProbe.size = probeSize;
                globalProbe.farClipPlane = camera.farClipPlane / 1.4f;
            }

            //Configures all probes
            if (Object.FindObjectsOfType<ReflectionProbe>() != null)
            {
                ReflectionProbe[] allProbes = Object.FindObjectsOfType<ReflectionProbe>();
                foreach (ReflectionProbe theReflectionProbes in allProbes)
                {
                    theReflectionProbes.mode = ReflectionProbeMode.Realtime;
                    theReflectionProbes.refreshMode = ReflectionProbeRefreshMode.OnAwake;
                }
            }

            //Updated reflection probes to realtime support
            QualitySettings.realtimeReflectionProbes = true;
        }

        /// <summary>
        /// Add realtime global reflection probes
        /// </summary>
        public static void AddGlobalReflectionProbeRealtime()
        {
            AmbientSkyProfiles profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath("Ambient Skies Volume 1"));

            GameObject theParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);

            //Get the camera
            GameObject mainCamera = SkyboxUtils.GetOrCreateMainCamera();

            //Sets up the probe to terrain size & location
            Vector3 probeLocation = Vector3.zero;
            Vector3 probeSize = new Vector3(3000f, 1250f, 3000f);
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null && GameObject.Find("Ambient Skies Water System") == null)
            {
                probeSize = terrain.terrainData.size;
                probeSize.x += 50f;
                probeSize.z += 50f;

                probeLocation = terrain.transform.position + (probeSize * 0.5f);
                probeLocation.y = terrain.SampleHeight(probeLocation) + 100f;
            }

            GameObject globalProbeGo = GameObject.Find("Global Reflection Probe");
            if (globalProbeGo == null)
            {
                globalProbeGo = new GameObject("Global Reflection Probe");
                globalProbeGo.AddComponent<ReflectionProbe>();
            }

            ReflectionProbe globalProbe = globalProbeGo.GetComponent<ReflectionProbe>();
            Camera camera = mainCamera.GetComponent<Camera>();

            globalProbeGo.transform.parent = theParent.transform;
            if (terrain != null)
            {
                globalProbeGo.transform.localPosition = probeLocation;
            }
            else
            {
                globalProbeGo.transform.localPosition = new Vector3(0f, 60f, 0f);
            }
            globalProbeGo.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);

            //Configure probe settings
            if (globalProbe != null)
            {
                if (terrain != null)
                {
                    globalProbe.size = new Vector3(8f, terrain.terrainData.size.y / 2, 8f);
                }
                else
                {
                    globalProbe.size = new Vector3(8f, 400f, 8f);
                }
                globalProbe.farClipPlane = camera.farClipPlane / 1.4f;
            }

            //Configures all probes
            if (Object.FindObjectsOfType<ReflectionProbe>() != null)
            {
                if (profiles != null)
                {
                    if (profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
#if HDPipeline && UNITY_2018_3_OR_NEWER
                        HDAdditionalReflectionData[] reflectionData = Object.FindObjectsOfType<HDAdditionalReflectionData>();
                        foreach (HDAdditionalReflectionData data in reflectionData)
                        {

#if UNITY_2019_1_OR_NEWER
                            data.mode = ProbeSettings.Mode.Baked;
#elif UNITY_2018_3_OR_NEWER
                            data.mode = ReflectionProbeMode.Baked;
#endif
                        }
#endif
                    }
                    else
                    {
                        ReflectionProbe[] allProbes = Object.FindObjectsOfType<ReflectionProbe>();
                        foreach (ReflectionProbe theReflectionProbes in allProbes)
                        {
                            theReflectionProbes.clearFlags = ReflectionProbeClearFlags.Skybox;
                            theReflectionProbes.mode = ReflectionProbeMode.Baked;
                        }
                    }
                }
                else
                {
                    ReflectionProbe[] allProbes = Object.FindObjectsOfType<ReflectionProbe>();
                    foreach (ReflectionProbe theReflectionProbes in allProbes)
                    {
                        theReflectionProbes.clearFlags = ReflectionProbeClearFlags.Skybox;
                        theReflectionProbes.mode = ReflectionProbeMode.Baked;
                    }
                }
            }

            //Updated reflection probes to realtime support
            QualitySettings.realtimeReflectionProbes = true;
        }

        /// <summary>
        /// Bake our global reflection probe - if no lighting baked yet and requested then a global bake is kicked off
        /// </summary>
        /// <param name="doGlobalBakeIfNecessary">If no previous bake has been done then a global bake will be kicked off</param>
        public static void BakeGlobalReflectionProbe(bool doGlobalBakeIfNecessary)
        {
            if (Lightmapping.isRunning)
            {
                return;
            }

            //Get global reflection probe
            GameObject globalReflectionProbeObj = null;
            globalReflectionProbeObj = GameObject.Find("Global Reflection Probe");
            if (globalReflectionProbeObj == null)
            {
                return;
            }

            //Get the probe itself
            ReflectionProbe probe = globalReflectionProbeObj.GetComponent<ReflectionProbe>();
            if (probe == null)
            {
                return;
            }

            //Process based on type of probe
            if (probe.mode == ReflectionProbeMode.Realtime)
            {
                probe.RenderProbe();
            }
            else if (probe.mode == ReflectionProbeMode.Baked)
            {
                if (probe.bakedTexture == null)
                {
                    if (doGlobalBakeIfNecessary)
                    {
                        BakeGlobalLighting();
                    }
                    return;
                }

                string bakedTexturePath = AssetDatabase.GetAssetPath(probe.bakedTexture);
                Lightmapping.BakeReflectionProbe(probe, bakedTexturePath);
            }
        }

        /// <summary>
        /// Bakes global lighting
        /// </summary>
        public static void BakeGlobalLighting()
        {
            //Bakes the lightmaps
            if (!Application.isPlaying && !Lightmapping.isRunning)
            {
                Lightmapping.BakeAsync();
            }
        }

        #endregion

        #endregion
    }
}