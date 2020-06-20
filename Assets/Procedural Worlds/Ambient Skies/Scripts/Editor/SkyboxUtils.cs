//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System.Reflection;
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif
#if LWPipeline && UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering.LWRP;
#elif LWPipeline && UNITY_2018_1_OR_NEWER
using UnityEngine.Experimental.Rendering.LightweightPipeline;
#endif
using UnityEditor.SceneManagement;
#if GAIA_PRESENT
using Gaia;
using System.Collections.Generic;
#endif

namespace AmbientSkies
{
    /// <summary>
    /// Skybox utilities
    /// </summary>
    public static class SkyboxUtils
    {
        #region Variables

        public static GameObject directionalLight;
        public static Material skyMaterial;
        public static Light sunLight;
        public static Texture skyboxTexture;
        public static GameObject mainCamera;

        public static GameObject newParentObject;
        public static GameObject postProcessingObject;
        public static GameObject reflectionProbeObject;
        public static GameObject ambientAudioObject;
        public static GameObject oldGaiaAmbientParent;

        public static Object timeOfDaySystem;
        public static AmbientSkiesTimeOfDay timeOfDay;
        public static RealtimeEmissionSetup emissionSetup;

#if HDPipeline && UNITY_2018_3_OR_NEWER
        public static HDAdditionalLightData hDAdditionalLightData;
        public static VolumeProfile volumeProfile;
#endif

#if LWPipeline

#if !UNITY_2018_3_OR_NEWER
        public static LightweightPipelineAsset lightweightRender;
#else
        public static LightweightRenderPipelineAsset lightweightRender;
#endif

#endif

        #endregion

        #region Utils

        #region Get/Set From Profile

        /// <summary>
        /// Get current profile index of currently active skybox
        /// </summary>
        /// <param name="profiles">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromActiveSkybox(AmbientSkyProfiles profiles, AmbientSkyboxProfile skyboxProfile, AmbientProceduralSkyboxProfile skyboxProceduralProfile, AmbientGradientSkyboxProfile skyboxGradientProfile, bool isSkyboxLoaded)
        {
            if (profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline
                GameObject volumeObject = GameObject.Find("High Definition Environment Volume");
                if (volumeObject != null)
                {
                    VolumeProfile volumeProfile = GetVolumeProfile();
                    if (volumeProfile != null)
                    {
                        VisualEnvironment visualEnvironment;
                        if (volumeProfile.TryGet(out visualEnvironment))
                        {
                            if (visualEnvironment.skyType.value == 1)
                            {
                                UnityEngine.Experimental.Rendering.HDPipeline.HDRISky hDRISky;
                                if (volumeProfile.TryGet(out hDRISky))
                                {
                                    if (hDRISky != null)
                                    {
                                        //Grab the texture
                                        Texture skyTexture = hDRISky.hdriSky.value;
                                        for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                                        {
                                            if (profiles.m_skyProfiles[texIdx].isPWProfile == false)
                                            {
                                                return profiles.m_skyProfiles[texIdx].profileIndex;
                                            }
                                            if (profiles.m_skyProfiles[texIdx].assetName == skyTexture.name)
                                            {
                                                return profiles.m_skyProfiles[texIdx].profileIndex;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (visualEnvironment.skyType.value == 2)
                            {
                                for (int texIdx = 0; texIdx < profiles.m_proceduralSkyProfiles.Count; texIdx++)
                                {
                                    if (profiles.m_proceduralSkyProfiles[texIdx].name == skyboxProceduralProfile.name)
                                    {
                                        return profiles.m_proceduralSkyProfiles[texIdx].profileIndex;
                                    }
                                }
                            }
                            else
                            {
                                for (int texIdx = 0; texIdx < profiles.m_gradientSkyProfiles.Count; texIdx++)
                                {
                                    if (profiles.m_gradientSkyProfiles[texIdx].name == skyboxGradientProfile.name)
                                    {
                                        return profiles.m_gradientSkyProfiles[texIdx].profileIndex;
                                    }
                                }
                            }
                        }
                    }
                }

                return -1;
#endif
            }
            else
            {
                if (isSkyboxLoaded)
                {
                    Material skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Ambient Skies Skybox"));
                    //See if we have one of our own skyboxes, abort if not
                    if (skyMaterial == null)
                    {
                        return -1;
                    }
                    else
                    {
                        string skyboxMat = "";
                        if (RenderSettings.skybox != null)
                        {
                            skyboxMat = RenderSettings.skybox.name;
                        }

                        if (skyboxMat.Contains("Custom"))
                        {
                            for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                            {
                                if (profiles.m_skyProfiles[texIdx].isPWProfile == false)
                                {
                                    return profiles.m_skyProfiles[texIdx].profileIndex;
                                }
                            }
                        }
                        else
                        {
#if AMBIENT_SKIES_ENVIRO
                            if (profiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty || profiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.DefaultProcedural)
                            {
                                var enviroComponent = Object.FindObjectOfType<EnviroSkyMgr>();
                                if (enviroComponent == null)
                                {
                                    RenderSettings.skybox = skyMaterial;
                                }
                            }
#endif
                            if (skyMaterial.shader == Shader.Find("Skybox/Procedural"))
                            {
                                //Grab the texture
                                Texture skyTexture = skyMaterial.GetTexture("_Tex");
                                for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                                {
                                    if (profiles.m_skyProfiles[texIdx].isPWProfile == false)
                                    {
                                        Debug.Log("Setting User settings option");
                                        return profiles.m_skyProfiles[texIdx].profileIndex;
                                    }
                                    if (profiles.m_skyProfiles[texIdx].assetName == skyTexture.name)
                                    {
                                        return profiles.m_skyProfiles[texIdx].profileIndex;
                                    }
                                }
                            }
                            else
                            {
                                //Grab the texture
                                Texture skyTexture = skyMaterial.GetTexture("_Tex");
                                for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                                {
                                    if (profiles.m_skyProfiles[texIdx].isPWProfile == false)
                                    {
                                        Debug.Log("Setting User settings option");
                                        return profiles.m_skyProfiles[texIdx].profileIndex;
                                    }
                                    if (profiles.m_skyProfiles[texIdx].assetName == skyTexture.name)
                                    {
                                        return profiles.m_skyProfiles[texIdx].profileIndex;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Using user lighting and Skybox, changes will not be made");
                }
            }

            return -1;
        }

        /// <summary>
        /// Get current profile index that has the profile name
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="name"></param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromProfileName(AmbientSkyProfiles profiles, string name)
        {
            for (int idx = 0; idx < profiles.m_skyProfiles.Count; idx++)
            {
                if (profiles.m_skyProfiles[idx].name == name)
                {
                    return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get current profile index that has the profile name
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="name"></param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProceduralProfileIndexFromProfileName(AmbientSkyProfiles profiles, string name)
        {
            for (int idx = 0; idx < profiles.m_proceduralSkyProfiles.Count; idx++)
            {
                if (profiles.m_proceduralSkyProfiles[idx].name == name)
                {
                    return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Create a skybox profile from the active skybox
        /// </summary>
        /// <param name="profiles">Profiles list</param>
        /// <returns>True if sucessful</returns>
        public static bool CreateProfileFromActiveSkybox(AmbientSkyProfiles profiles)
        {
            //Check the profiles
            if (profiles == null)
            {
                Debug.LogError("AmbientSkies:CreateProfileFromActiveSkybox() : Profile list must not be null - Aborting!");
                return false;
            }

            //Get the skybox
            Material skyMaterial = RenderSettings.skybox;
            if (skyMaterial == null)
            {
                Debug.LogError("AmbientSkies:CreateProfileFromActiveSkybox() : Missing skybox - Aborting!");
                return false;
            }

            if (skyMaterial.name != "Ambient Skies Skybox")
            {
                Debug.LogError("AmbientSkies:CreateProfileFromActiveSkybox() : Can only create from ambient skies profile - Aborting!");
                return false;
            }

            //Conditionally load the skybox texture 
            Texture skyboxTexture = skyMaterial.GetTexture("_Tex");
            if (skyboxTexture == null)
            {
                Debug.LogError("AmbientSkies:CreateProfileFromActiveSkybox() : Missing skybox texture - Aborting!");
                return false;
            }

            //Start picking values out of the skybox
            AmbientSkyboxProfile profile = new AmbientSkyboxProfile();

            profile.name = skyboxTexture.name;
            profile.assetName = skyboxTexture.name;
            profile.skyboxTint = skyMaterial.GetColor("_Tint");
            profile.skyboxExposure = skyMaterial.GetFloat("_Exposure");
            profile.skyboxRotation = skyMaterial.GetFloat("_Rotation");
            profile.skyboxGroundIntensity = RenderSettings.ambientIntensity;
            profile.fogColor = RenderSettings.fogColor;
            profile.fogDistance = RenderSettings.fogEndDistance;

            //Set up the light if we have one
            GameObject lightObj = GetMainDirectionalLight();

            if (lightObj != null)
            {
                //Get sun rotation
                profile.sunRotation = lightObj.transform.rotation.eulerAngles;

                //Now get the light values
                Light light = lightObj.GetComponent<Light>();
                if (light != null)
                {
                    profile.sunColor = light.color;
                    profile.sunIntensity = light.intensity;
                }
            }

            //Copy to defaults
            profile.SaveCurrentToDefault();

            //Add it and exit
            profiles.m_skyProfiles.Add(profile);
            return true;
        }

        /// <summary>
        /// Load the selected profile and apply to the skybox
        /// </summary>
        /// <param name="profiles">The profiles object</param>
        /// <param name="profileName">The name of the profile to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileName(AmbientSkyProfiles profiles, string profileName, bool useDefaults, bool updateVisualEnvironment, bool updateFog, bool updateShadows, bool updateAmbientLight, bool updateScreenReflections, bool updateScreenRefractions, bool updateSun)
        {
            AmbientSkyboxProfile skyboxProfile = profiles.m_skyProfiles.Find(x => x.name == profileName);
            if (skyboxProfile == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply skybox settings!");
                return;
            }

            AmbientProceduralSkyboxProfile proceduralSkyboxProfile = profiles.m_proceduralSkyProfiles.Find(x => x.name == profileName);
            if (proceduralSkyboxProfile == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply procedural skybox settings!");
                return;
            }

            AmbientGradientSkyboxProfile gradientSkyboxProfile = profiles.m_gradientSkyProfiles.Find(x => x.name == profileName);
            if (gradientSkyboxProfile == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply procedural skybox settings!");
                return;
            }

            SetSkybox(profiles, skyboxProfile, proceduralSkyboxProfile, gradientSkyboxProfile, useDefaults, profiles.m_selectedRenderPipeline, updateVisualEnvironment, updateFog, updateShadows, updateAmbientLight, updateScreenReflections, updateScreenRefractions, updateSun);            
        }

        /// <summary>
        /// Load the selected profile and apply to the skybox
        /// </summary>
        /// <param name="profiles">The profiles to get the asset from</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromAssetName(AmbientSkyProfiles profiles, string assetName, bool useDefaults, bool updateVisualEnvironment, bool updateFog, bool updateShadows, bool updateAmbientLight, bool updateScreenReflections, bool updateScreenRefractions, bool updateSun)
        {
            AmbientSkyboxProfile skyboxProfile = profiles.m_skyProfiles.Find(x => x.name == assetName);
            if (skyboxProfile == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply skybox settings!");
                return;
            }

            AmbientProceduralSkyboxProfile proceduralSkyboxProfile = profiles.m_proceduralSkyProfiles.Find(x => x.name == assetName);
            if (proceduralSkyboxProfile == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply procedural skybox settings!");
                return;
            }

            AmbientGradientSkyboxProfile gradientSkyboxProfile = profiles.m_gradientSkyProfiles.Find(x => x.name == assetName);
            if (gradientSkyboxProfile == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply procedural skybox settings!");
                return;
            }

            SetSkybox(profiles, skyboxProfile, proceduralSkyboxProfile, gradientSkyboxProfile, useDefaults, profiles.m_selectedRenderPipeline, updateVisualEnvironment, updateFog, updateShadows, updateAmbientLight, updateScreenReflections, updateScreenRefractions, updateSun);
        }

        /// <summary>
        /// Loads the first is or is not PW Profile
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="isPWProfile"></param>
        /// <returns></returns>
        public static int GetFromIsPWProfile(AmbientSkyProfiles profiles, bool isPWProfile)
        {
            for (int idx = 0; idx < profiles.m_skyProfiles.Count; idx++)
            {
                if (profiles.m_skyProfiles[idx].isPWProfile == isPWProfile)
                {
                    return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Loads the first is or is not PW Profile
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="isPWProfile"></param>
        /// <returns></returns>
        public static AmbientSkyboxProfile GetProfileFromIsPWProfile(AmbientSkyProfiles profiles, bool isPWProfile)
        {
            AmbientSkyboxProfile profile = profiles.m_skyProfiles.Find(x => x.isPWProfile == false);
            if (profile != null)
            {
                return profile;
            }
            return null;
        }

        /// <summary>
        /// Load the selected profile and apply to the skybox
        /// </summary>
        /// <param name="profiles">The profiles object</param>
        /// <param name="profileIndex">The zero based index to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileIndex(AmbientSkyProfiles profiles, int profileIndex, int proceduralProfileIndex, int gradientProfileIndex, bool useDefaults, bool updateVisualEnvironment, bool updateFog, bool updateShadows, bool updateAmbientLight, bool updateScreenReflections, bool updateScreenRefractions, bool updateSun)
        {
            if (profileIndex < 0 || profileIndex >= profiles.m_skyProfiles.Count)
            {
                Debug.LogWarning("Invalid profile index selected, can not apply skybox settings!");
                return;
            }

            if (profiles != null && profileIndex >= 0 && proceduralProfileIndex >= 0 && gradientProfileIndex >= 0)
            {
                SetSkybox(profiles, profiles.m_skyProfiles[profileIndex], profiles.m_proceduralSkyProfiles[proceduralProfileIndex], profiles.m_gradientSkyProfiles[gradientProfileIndex], useDefaults, profiles.m_selectedRenderPipeline, updateVisualEnvironment, updateFog, updateShadows, updateAmbientLight, updateScreenReflections, updateScreenRefractions, updateSun);
                if (profiles.m_showDebug)
                {
                    Debug.Log("SkyboxUtils.SetFromProfileIndex(): Apply skybox settings");
                }
            }
            else
            {
                Debug.LogError("Unable to apply settings from current profile, Index is missing");
            }
        }

        #endregion

        #region Skybox Setup

        /// <summary>
        /// This method will set up the skybox from the specified profile
        /// </summary>
        /// <param name="skyProfiles"></param>
        /// <param name="profile"></param>
        /// <param name="useDefaults"></param>
        /// <param name="renderPipelineSettings"></param>
        public static void SetSkybox(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, bool useDefaults, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings, bool updateVisualEnvironment, bool updateFog, bool updateShadows, bool updateAmbientLight, bool updateScreenReflections, bool updateScreenRefractions, bool updateSun)
        {
            //Check the profile
            if (skyProfiles == null)
            {
                Debug.LogError("AmbientSkies:SetSkybox() : Profile must not be null - Aborting!");
                return;
            }
            else
            {
                bool isHDRP = false;
                if (skyProfiles.m_useSkies)
                {                   
                    if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
                        isHDRP = true;
                    }

                    SetSkyboxSettings(skyProfiles, profile, gradientProfile, proceduralProfile, isHDRP, renderPipelineSettings, updateVisualEnvironment, updateFog, updateShadows, updateAmbientLight, updateScreenReflections, updateScreenRefractions, updateSun);

                    if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.ThirdParty || renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                    {
                        HorizonUtils.RemoveHorizonSky();
                    }
                }
                else
                {
                    RemoveTimeOfDay();

                    RemoveHorizonSky();

                    DestroyParent("Ambient Skies Environment");
                }
            }
        }

        /// <summary>
        /// Sets the skybox settings
        /// </summary>
        /// <param name="skyProfiles"></param>
        /// <param name="profile"></param>
        /// <param name="useDefaults"></param>
        /// <param name="renderPipelineSettings"></param>
        private static void SetSkyboxSettings(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientGradientSkyboxProfile gradientProfile, AmbientProceduralSkyboxProfile proceduralProfile, bool isHDRP, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings, bool updateVisualEnvironment, bool updateFog, bool updateShadows, bool updateAmbientLight, bool updateScreenReflections, bool updateScreenRefractions, bool updateSun)
        {
            //Clear console if required
            if (skyProfiles.m_smartConsoleClean)
            {               
                ClearLog();
                Debug.Log("Console cleared successfully");
            }

            CleanPipelineComponenets(skyProfiles);

            VSyncSettings(skyProfiles);

            if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
#if GAIA_PRESENT
                SetGaiaSetup(skyProfiles);
#endif
                if (RenderSettings.skybox == null)
                {
                    AddSkyboxIfNull("Ambient Skies Skybox", skyProfiles);
                }

                RemoveNewSceneObject(skyProfiles);

                RemoveEnviro(true);

                RemoveTimeOfDay();

                if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    if (updateShadows)
                    {
                        if (skyProfiles.m_showFunctionDebugsOnly)
                        {
                            Debug.Log("Updating SetQualitySettings()");
                        }

                        SetQualitySettings(skyProfiles, profile, proceduralProfile, gradientProfile);
                    }

                    if (updateFog || updateAmbientLight)
                    {
                        if (skyProfiles.m_showFunctionDebugsOnly)
                        {
                            Debug.Log("Updating SetAmbientLightingAndFog()");
                        }

                        SetAmbientLightingAndFog(skyProfiles, profile, proceduralProfile, gradientProfile);
                    }

                    if (updateSun)
                    {
                        if (skyProfiles.m_showFunctionDebugsOnly)
                        {
                            Debug.Log("Updating SetEnableSunDisk() + SetSunLightSettings()");
                        }

                        SetEnableSunDisk(skyProfiles, profile, proceduralProfile, gradientProfile);
                        SetSunLightSettings(skyProfiles, profile, proceduralProfile, gradientProfile);
                    }

                    if (updateVisualEnvironment)
                    {
                        if (skyProfiles.m_showFunctionDebugsOnly)
                        {
                            Debug.Log("Updating SetSkyboxRotation() + LoadSkyboxMaterial()");
                        }

                        SetSkyboxRotation(skyProfiles, profile, proceduralProfile, gradientProfile, renderPipelineSettings);

                        if (profile.isProceduralSkybox)
                        {
                            LoadSkyboxMaterial(skyProfiles, profile, proceduralProfile, gradientProfile, false);
                        }
                        else
                        {
                            LoadSkyboxMaterial(skyProfiles, profile, proceduralProfile, gradientProfile, true);
                        }
                    }

                    RemoveHDRPObjects();
                }
                else
                {
                    if (updateSun)
                    {
                        if (skyProfiles.m_showFunctionDebugsOnly)
                        {
                            Debug.Log("Updating SetSunLightSettings()");
                        }

                        SetSunLightSettings(skyProfiles, profile, proceduralProfile, gradientProfile);
                    }

                    if (isHDRP)
                    {
                        AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(profile, proceduralProfile, gradientProfile, skyProfiles, profile.profileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile", updateVisualEnvironment, updateFog, updateShadows, updateAmbientLight, updateScreenReflections, updateScreenRefractions, updateSun);
                    }
                }

                DestroyParent("Ambient Skies Environment");
            }
            else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
#if GAIA_PRESENT
                SetGaiaSetup(skyProfiles);
#endif
                if (RenderSettings.skybox == null)
                {
                    AddSkyboxIfNull("Ambient Skies Skybox", skyProfiles);
                }

                RemoveNewSceneObject(skyProfiles);

                if (skyProfiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                {
                    AddTimeOfDay(profile, skyProfiles);

                    SetTimeOfDaySettings(proceduralProfile, skyProfiles);

                    SetQualitySettings(skyProfiles, profile, proceduralProfile, gradientProfile);

                    if (isHDRP)
                    {
                        AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(profile, proceduralProfile, gradientProfile, skyProfiles, profile.profileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile", updateVisualEnvironment, updateFog, updateShadows, updateAmbientLight, updateScreenReflections, updateScreenRefractions, updateSun);
                    }
                }
                else
                {
                    RemoveEnviro(true);

                    RemoveTimeOfDay();

                    if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
                        if (updateShadows)
                        {
                            if (skyProfiles.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetQualitySettings()");
                            }

                            SetQualitySettings(skyProfiles, profile, proceduralProfile, gradientProfile);
                        }

                        if (updateFog || updateAmbientLight)
                        {
                            if (skyProfiles.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetAmbientLightingAndFog()");
                            }

                            SetAmbientLightingAndFog(skyProfiles, profile, proceduralProfile, gradientProfile);
                        }

                        if (updateSun)
                        {
                            if (skyProfiles.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetEnableSunDisk() + SetSunLightSettings()");
                            }

                            SetEnableSunDisk(skyProfiles, profile, proceduralProfile, gradientProfile);
                            SetSunLightSettings(skyProfiles, profile, proceduralProfile, gradientProfile);
                        }

                        if (updateVisualEnvironment)
                        {
                            if (skyProfiles.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetSkyboxRotation() + LoadSkyboxMaterial()");
                            }

                            SetSkyboxRotation(skyProfiles, profile, proceduralProfile, gradientProfile, renderPipelineSettings);
                            LoadSkyboxMaterial(skyProfiles, profile, proceduralProfile, gradientProfile, false);
                        }
                    }
                    else
                    {
                        if (updateSun)
                        {
                            if (skyProfiles.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetSunLightSettings()");
                            }

                            SetSunLightSettings(skyProfiles, profile, proceduralProfile, gradientProfile);
                        }

                        if (isHDRP)
                        {
                            AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(profile, proceduralProfile, gradientProfile, skyProfiles, profile.profileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile", updateVisualEnvironment, updateFog, updateShadows, updateAmbientLight, updateScreenReflections, updateScreenRefractions, updateSun);
                        }
                    }
                }

                DestroyParent("Ambient Skies Environment");
            }
            else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
            {
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
#if GAIA_PRESENT
                    SetGaiaSetup(skyProfiles);
#endif
                    RemoveNewSceneObject(skyProfiles);

                    RemoveEnviro(true);

                    RemoveTimeOfDay();

                    if (updateShadows)
                    {
                        if (skyProfiles.m_showFunctionDebugsOnly)
                        {
                            Debug.Log("Updating SetQualitySettings()");
                        }

                        SetQualitySettings(skyProfiles, profile, proceduralProfile, gradientProfile);
                    }

                    if (updateFog || updateAmbientLight)
                    {
                        if (skyProfiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                        {
                            if (skyProfiles.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetAmbientLightingAndFog()");
                            }

                            SetAmbientLightingAndFog(skyProfiles, profile, proceduralProfile, gradientProfile);
                        }
                    }

                    if (updateSun)
                    {
                        if (skyProfiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                        {
                            if (skyProfiles.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetEnableSunDisk() + SetSunLightSettings()");
                            }

                            SetEnableSunDisk(skyProfiles, profile, proceduralProfile, gradientProfile);
                            SetSunLightSettings(skyProfiles, profile, proceduralProfile, gradientProfile);
                        }
                    }

                    if (updateVisualEnvironment)
                    {
                        if (skyProfiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                        {
                            if (skyProfiles.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetSkyboxRotation() + LoadSkyboxMaterial()");
                            }

                            SetSkyboxRotation(skyProfiles, profile, proceduralProfile, gradientProfile, renderPipelineSettings);

                            if (profile.isProceduralSkybox)
                            {
                                LoadSkyboxMaterial(skyProfiles, profile, proceduralProfile, gradientProfile, false);
                            }
                            else
                            {
                                LoadSkyboxMaterial(skyProfiles, profile, proceduralProfile, gradientProfile, true);
                            }
                        }
                    }


                    if (isHDRP)
                    {
                        AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(profile, proceduralProfile, gradientProfile, skyProfiles, profile.profileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile", updateVisualEnvironment, updateFog, updateShadows, updateAmbientLight, updateScreenReflections, updateScreenRefractions, updateSun);
                    }
                }

                DestroyParent("Ambient Skies Environment");
            }
            else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.DefaultProcedural)
            {
                if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    RemoveSkyBox(skyProfiles);

                    RemoveTimeOfDay();

                    RemoveDensityFogVolume();

                    RemoveHorizonSky();
                }
                else
                {
                    RemoveHDSkybox(profile, skyProfiles, proceduralProfile, gradientProfile, renderPipelineSettings, updateVisualEnvironment, updateFog, updateShadows, updateAmbientLight, updateScreenReflections, updateScreenRefractions, updateSun);
                }

                DestroyParent("Ambient Skies Environment");
            }
            else
            {
                if (RenderSettings.skybox == null)
                {
                    AddSkyboxIfNull("Ambient Skies Skybox", skyProfiles);
                }

                DestroyParent("Ambient Skies Environment");
            }
        }

        /// <summary>
        /// Cleans up pipeline componenets on objects
        /// </summary>
        /// <param name="skyProfiles"></param>
        public static void CleanPipelineComponenets(AmbientSkyProfiles skyProfiles)
        {
            if (directionalLight == null)
            {
                directionalLight = GetMainDirectionalLight();
            }

            if (mainCamera == null)
            {
                mainCamera = GetOrCreateMainCamera();
            }

            if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                Component[] lightComponenets = directionalLight.GetComponents<Component>();
                if (lightComponenets != null)
                {
                    foreach(Component component in lightComponenets)
                    {
                        if (component.GetType().ToString().Contains("HDAdditionalLightData"))
                        {
                            if (skyProfiles.m_showDebug)
                            {
                                Debug.Log("HDAdditionalLightData on light it has now been removed");
                            }

                            Object.DestroyImmediate(component);
                        }

                        if (component.GetType().ToString().Contains("AdditionalShadowData"))
                        {
                            if (skyProfiles.m_showDebug)
                            {
                                Debug.Log("AdditionalShadowData on light it has now been removed");
                            }

                            Object.DestroyImmediate(component);
                        }

                        if (component.GetType().ToString().Contains("LWRPAdditionalLightData"))
                        {
                            if (skyProfiles.m_showDebug)
                            {
                                Debug.Log("LWRPAdditionalLightData on light it has now been removed");
                            }

                            Object.DestroyImmediate(component);
                        }
                    }
                }

                Component[] cameraComponents = mainCamera.GetComponents<Component>();
                if (cameraComponents != null)
                {
                    foreach (Component component in cameraComponents)
                    {
                        if (component.GetType().ToString().Contains("HDAdditionalCameraData"))
                        {
                            if (skyProfiles.m_showDebug)
                            {
                                Debug.Log("HDAdditionalCameraData on camera it has now been removed");
                            }

                            Object.DestroyImmediate(component);
                        }

                        if (component.GetType().ToString().Contains("LWRPAdditionalCameraData"))
                        {
                            if (skyProfiles.m_showDebug)
                            {
                                Debug.Log("LWRPAdditionalCameraData on camera it has now been removed");
                            }

                            Object.DestroyImmediate(component);
                        }
                    }
                }

                GameObject hdVolume = GameObject.Find("High Definition Environment Volume");
                if (hdVolume != null)
                {
                    Object.DestroyImmediate(hdVolume);
                }
            }
            else if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
            {
                Component[] lightComponenets = directionalLight.GetComponents<Component>();
                if (lightComponenets != null)
                {
                    foreach (Component component in lightComponenets)
                    {
                        if (component.GetType().ToString().Contains("HDAdditionalLightData"))
                        {
                            if (skyProfiles.m_showDebug)
                            {
                                Debug.Log("HDAdditionalLightData on light it has now been removed");
                            }

                            Object.DestroyImmediate(component);
                        }

                        if (component.GetType().ToString().Contains("AdditionalShadowData"))
                        {
                            if (skyProfiles.m_showDebug)
                            {
                                Debug.Log("AdditionalShadowData on light it has now been removed");
                            }

                            Object.DestroyImmediate(component);
                        }
                    }
                }

                Component[] cameraComponents = mainCamera.GetComponents<Component>();
                if (cameraComponents != null)
                {
                    foreach (Component component in cameraComponents)
                    {
                        if (component.GetType().ToString().Contains("HDAdditionalCameraData"))
                        {
                            if (skyProfiles.m_showDebug)
                            {
                                Debug.Log("HDAdditionalCameraData on camera it has now been removed");
                            }

                            Object.DestroyImmediate(component);
                        }
                    }
                }

                GameObject hdVolume = GameObject.Find("High Definition Environment Volume");
                if (hdVolume != null)
                {
                    Object.DestroyImmediate(hdVolume);
                }
            }
            else
            {                
                Component[] cameraComponents = mainCamera.GetComponents<Component>();
                if (cameraComponents != null)
                {
                    foreach (Component component in cameraComponents)
                    {                        
                        if (component.GetType().ToString().Contains("LWRPAdditionalCameraData"))
                        {
                            if (skyProfiles.m_showDebug)
                            {
                                Debug.Log("LWRPAdditionalCameraData on camera it has now been removed");
                            }

                            Object.DestroyImmediate(component);
                        }
                    }
                }

                Component[] lightComponenets = directionalLight.GetComponents<Component>();
                if (lightComponenets != null)
                {
                    foreach (Component component in lightComponenets)
                    {
                        if (component.GetType().ToString().Contains("LWRPAdditionalLightData"))
                        {
                            if (skyProfiles.m_showDebug)
                            {
                                Debug.Log("LWRPAdditionalLightData on light it has now been removed");
                            }

                            Object.DestroyImmediate(component);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove the skybox and set back to the procedural skybox
        /// </summary>
        public static void RemoveSkyBox(AmbientSkyProfiles skyProfiles)
        {
#if GAIA_PRESENT

#if AMBIENT_SKIES_ENVIRO
            if (skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty || skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.DefaultProcedural)
            {
                var enviroComponent = Object.FindObjectOfType<EnviroSkyMgr>();
                if (enviroComponent == null)
                {
                    if (RenderSettings.skybox.name != "Ambient Skies Skybox")
                    {
                        RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
                    }
                }
            }
#endif
          
#endif

            //Check for skybox material
            skyMaterial = RenderSettings.skybox;
            //If not empty
            if (skyMaterial != null)
            {
                //Set the material shader
                skyMaterial.shader = Shader.Find("Skybox/Procedural");
                skyMaterial.SetColor("_GroundColor", GetColorFromHTML("A6C3DB"));
                skyMaterial.SetFloat("_AtmosphereThickness", 0.7f);
                skyMaterial.SetColor("_SkyTint", Color.white);
                skyMaterial.SetColor("_Tint", Color.white);
                skyMaterial.SetFloat("_Exposure", 0.7f);
                skyMaterial.SetFloat("_SunDisk", 2f);
                skyMaterial.SetFloat("_SunSize", 0.05f);
                skyMaterial.SetFloat("_SunSizeConvergence", 10f);
            }

            if (directionalLight == null)
            {
                directionalLight = GetMainDirectionalLight();
            }
            else
            {
                directionalLight.transform.rotation = Quaternion.Euler(133f, 0f, 0f);

                if (sunLight == null)
                {
                    sunLight = directionalLight.GetComponent<Light>();
                    sunLight.color = GetColorFromHTML("FFEADD");
                    sunLight.intensity = 1.5f;
                }
                else
                {
                    sunLight.color = GetColorFromHTML("FFEADD");
                    sunLight.intensity = 1.5f;
                }
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.fogColor = GetColorFromHTML("879A9A");
        }

        /// <summary>
        /// If gaia is present set up parenting and skybox mateiral
        /// </summary>
        public static void SetGaiaSetup(AmbientSkyProfiles skyProfiles)
        {
#if AMBIENT_SKIES_ENVIRO

            var enviroComponent = Object.FindObjectOfType<EnviroSkyMgr>();
            if (enviroComponent != null)
            {
                return;
            }

#endif
            if (skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty || skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.DefaultProcedural)
            {
                if (RenderSettings.skybox == null || RenderSettings.skybox.name != "Ambient Skies Skybox")
                {
                    RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
                }
            }

            SetGaiaParenting(true);
        }

        /// <summary>
        /// Sets skybox if it is null
        /// </summary>
        /// <param name="skyboxName"></param>
        public static void AddSkyboxIfNull(string skyboxName, AmbientSkyProfiles skyProfiles)
        {
#if AMBIENT_SKIES_ENVIRO

            var enviroComponent = Object.FindObjectOfType<EnviroSkyMgr>();
            if (enviroComponent != null)
            {
                return;
            }

#endif
            //Get skybox material
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath(skyboxName));
            //If asset is found
            if (skyMaterial != null)
            {
                if (skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty || skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.DefaultProcedural)
                {
                    //If skybox in scene is empty
                    if (RenderSettings.skybox == null)
                    {
                        //Apply skybox
                        RenderSettings.skybox = skyMaterial;
                    }
                }
            }

            //Created object to resemble a new scene
            GameObject newSceneObject = GameObject.Find("Ambient Skies New Scene Object (Don't Delete Me)");
            //Parent object in the scene
            GameObject parentObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);
            //If the object to resemble a new scene is not there
            if (newSceneObject == null)
            {
                //Create it
                newSceneObject = new GameObject("Ambient Skies New Scene Object (Don't Delete Me)");
                //Parent it
                newSceneObject.transform.SetParent(parentObject.transform);
            }
        }

        /// <summary>
        /// Removes the skybox for HD Pipeline and sets it back to a procedural skybox
        /// </summary>
        public static void RemoveHDSkybox(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings, bool updateVisualEnvironment, bool updateFog, bool updateShadows, bool updateAmbientLight, bool updateScreenReflections, bool updateScreenRefractions, bool updateSun)
        {
#if HDPipeline && UNITY_2018_3_OR_NEWER
            //Get the main directional light
            if (directionalLight == null)
            {
                directionalLight = GetMainDirectionalLight();
            }
            else
            {
                //Set light rotation
                Vector3 rotation = new Vector3(profile.lightRotation, 0f, 0f);
                directionalLight.transform.SetPositionAndRotation(directionalLight.transform.position, Quaternion.Euler(rotation));

                //Now set the light values
                if (sunLight == null)
                {
                    sunLight = directionalLight.GetComponent<Light>();
                    sunLight.color = profile.sunColor;
                    sunLight.intensity = profile.sunIntensity * 2;
                }
                else
                {
                    sunLight.color = profile.sunColor;
                    sunLight.intensity = profile.sunIntensity * 2;
                }

                if (hDAdditionalLightData == null)
                {
                    hDAdditionalLightData = directionalLight.GetComponent<HDAdditionalLightData>();
                    if (hDAdditionalLightData == null)
                    {
                        hDAdditionalLightData = directionalLight.AddComponent<HDAdditionalLightData>();
                    }
                    hDAdditionalLightData.intensity = profile.sunIntensity * 2;
                    hDAdditionalLightData.intensity = proceduralProfile.proceduralSunIntensity * 2;
                }
                else
                {
                    hDAdditionalLightData.intensity = profile.sunIntensity * 2;
                    hDAdditionalLightData.intensity = proceduralProfile.proceduralSunIntensity * 2;
                }
            }

            if (volumeProfile == null)
            {
                volumeProfile = GetVolumeProfile();

                VisualEnvironment visualEnvironment;
                if (volumeProfile.TryGet(out visualEnvironment))
                {
                    visualEnvironment.skyType.value = 2;
                }

                HDRISky hDRISky;
                if (volumeProfile.TryGet(out hDRISky))
                {
                    hDRISky.active = false;
                }

                ProceduralSky proceduralSky;
                if (volumeProfile.TryGet(out proceduralSky))
                {
                    proceduralSky.active = true;
                    proceduralSky.enableSunDisk.value = true;
                    proceduralSky.sunSize.value = 0.015f;
                    proceduralSky.sunSizeConvergence.value = 10f;
                }

                GradientSky gradientSky;
                if (volumeProfile.TryGet(out gradientSky))
                {
                    gradientSky.active = false;
                }
            }
            else
            {
                VisualEnvironment visualEnvironment;
                if (volumeProfile.TryGet(out visualEnvironment))
                {
                    visualEnvironment.skyType.value = 2;
                }

                HDRISky hDRISky;
                if (volumeProfile.TryGet(out hDRISky))
                {
                    hDRISky.active = false;
                }

                ProceduralSky proceduralSky;
                if (volumeProfile.TryGet(out proceduralSky))
                {
                    proceduralSky.active = true;
                    proceduralSky.enableSunDisk.value = true;
                    proceduralSky.sunSize.value = 0.015f;
                    proceduralSky.sunSizeConvergence.value = 10f;
                }

                GradientSky gradientSky;
                if (volumeProfile.TryGet(out gradientSky))
                {
                    gradientSky.active = false;
                }
            }

            AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(profile, proceduralProfile, gradientProfile, skyProfiles, profile.profileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile", updateVisualEnvironment, updateFog, updateShadows, updateAmbientLight, updateScreenReflections, updateScreenRefractions, updateSun);

            DestroyParent("Ambient Skies Environment");
#endif
        }

        /// <summary>
        /// Looks for fog density volume then removes it fromt he scene
        /// </summary>
        public static void RemoveDensityFogVolume()
        {
            GameObject densityVolume = GameObject.Find("Density Volume");
            if (densityVolume != null)
            {
                Object.DestroyImmediate(densityVolume);
            }

            DestroyParent("Ambient Skies Environment");
        }

        /// <summary>
        /// Sets material to HDRI cubemap shader
        /// </summary>
        /// <param name="profile"></param>
        public static void HDRISky(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles)
        {
            if (skyMaterial == null)
            {
                AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Ambient Skies Skybox"));
            }
            else
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    skyMaterial.shader = Shader.Find("Skybox/Cubemap");
                }
                else
                {
                    skyMaterial.shader = Shader.Find("Skybox/Procedural");
                }
            }
        }

        /// <summary>
        /// Gets the skybox rotation
        /// </summary>
        /// <returns>Returns the rotation or zero if no skybox</returns>
        public static float GetSkyboxRotation()
        {
            if (RenderSettings.skybox != null)
            {
                return RenderSettings.skybox.GetFloat("_Rotation");
            }
            return 0f;
        }

        /// <summary>
        /// Gets and returns the volume profile
        /// </summary>
        /// <returns>Volume Profile</returns>
#if HDPipeline
        public static VolumeProfile GetVolumeProfile()
        {
            //Hd Pipeline Volume Setup
            GameObject parentObject = GetOrCreateParentObject("Ambient Skies Environment", false);
            GameObject volumeObject = GameObject.Find("High Definition Environment Volume");
            if (volumeObject == null)
            {
                volumeObject = new GameObject("High Definition Environment Volume");
                volumeObject.layer = LayerMask.NameToLayer("TransparentFX");
                volumeObject.transform.SetParent(parentObject.transform);
            }

            Volume volumeSettings = volumeObject.GetComponent<Volume>();
            if (volumeSettings == null)
            {
                volumeSettings = volumeObject.AddComponent<Volume>();
                volumeSettings.isGlobal = true;
                volumeSettings.blendDistance = 5f;
                volumeSettings.weight = 1f;
                volumeSettings.priority = 1f;
            }
            else
            {
                volumeSettings.isGlobal = true;
                volumeSettings.blendDistance = 5f;
                volumeSettings.weight = 1f;
                volumeSettings.priority = 1f;
            }

            //Finds the volume in the scene
            Volume volume = Object.FindObjectOfType<Volume>();
            if (volume != null)
            {
                //If Missing it'll add it to the volume
                volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(GetAssetPath("Ambient Skies HD Volume Profile"));
                //Gets the profile
                VolumeProfile volumeProfile = volume.sharedProfile;
                //Returns the profile
                return volumeProfile;
            }

            //Else return null
            return null;
        }
#endif

        /// <summary>
        /// Set the skybox rotation in degrees - will also rotate the directional light
        /// </summary>
        /// <param name="angleDegrees">Angle from 0..360.</param>
        public static void SetSkyboxRotation(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
            if (directionalLight == null)
            {
                directionalLight = GetMainDirectionalLight();
            }

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //Correct the angle
                    float angleDegrees = proceduralProfile.proceduralSkyboxRotation % 360f;

                    float sunAngle = proceduralProfile.proceduralSkyboxPitch;

                    //Set new directional light rotation
                    if (directionalLight != null)
                    {
                        Vector3 rotation = directionalLight.transform.rotation.eulerAngles;
                        rotation.y = angleDegrees;
                        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, rotation.y, 0f);
                    }
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //Correct the angle
                    float angleDegrees = profile.skyboxRotation % 360f;

                    float sunAngle = profile.skyboxPitch;

                    //Set new directional light rotation
                    if (directionalLight != null)
                    {
                        Vector3 rotation = directionalLight.transform.rotation.eulerAngles;
                        rotation.y = angleDegrees;
                        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, -rotation.y, 0f);
                    }
                }
                else
                {
                    //Correct the angle
                    float angleDegrees = gradientProfile.skyboxRotation % 360f;

                    float sunAngle = gradientProfile.skyboxPitch;

                    //Set new directional light rotation
                    if (directionalLight != null)
                    {
                        Vector3 rotation = directionalLight.transform.rotation.eulerAngles;
                        rotation.y = angleDegrees;
                        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, rotation.y, 0f);
                    }
                }
            }
            else
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //Correct the angle
                    float angleDegrees = proceduralProfile.proceduralSkyboxRotation % 360f;

                    float sunAngle = proceduralProfile.proceduralSkyboxPitch;

                    //Set new directional light rotation
                    if (directionalLight != null)
                    {
                        Vector3 rotation = directionalLight.transform.rotation.eulerAngles;
                        rotation.y = angleDegrees;
                        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, rotation.y, 0f);
                    }
                }
                else
                {
                    if (profile.isProceduralSkybox)
                    {
                        //Correct the angle
                        float angleDegrees = profile.customSkyboxRotation % 360f;

                        float sunAngle = profile.customSkyboxPitch;

                        //Set new directional light rotation
                        if (directionalLight != null)
                        {
                            Vector3 rotation = directionalLight.transform.rotation.eulerAngles;
                            rotation.y = angleDegrees;
                            directionalLight.transform.rotation = Quaternion.Euler(sunAngle, rotation.y, 0f);
                        }
                    }
                    else
                    {
                        //Correct the angle
                        float angleDegrees = profile.skyboxRotation % 360f;

                        float sunAngle = profile.skyboxPitch;

                        //Set new directional light rotation
                        if (directionalLight != null)
                        {
                            Vector3 rotation = directionalLight.transform.rotation.eulerAngles;
                            rotation.y = angleDegrees;
                            directionalLight.transform.rotation = Quaternion.Euler(sunAngle, rotation.y, 0f);
                        }
                    }
                    
                }
            }

            if (RenderSettings.skybox != null && renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                //Correct the angle
                float angleDegrees = proceduralProfile.proceduralSkyboxRotation % 360f;

                //Set new skybox rotation
                RenderSettings.skybox.SetFloat("_Rotation", angleDegrees);
            }
        }

        /// <summary>
        /// Get the skybox exposure
        /// </summary>
        /// <returns>Returns skybox exposure or 1f if none set</returns>
        public static float GetSkyboxExposure()
        {
            if (RenderSettings.skybox != null)
            {
                return RenderSettings.skybox.GetFloat("_Exposure");
            }
            return 1f;
        }

        /// <summary>
        /// Set the skybox exposure.
        /// </summary>
        /// <param name="exposure">Exposure</param>
        public static void SetSkyboxExposure(float exposure)
        {
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetFloat("_Exposure", exposure);
            }
        }

        /// <summary>
        /// Set the skybox tint.
        /// </summary>
        /// <param name="tint">Tint</param>
        public static void SetSkyboxTint(Color tint)
        {
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetColor("_Tint", tint);
            }
        }

        /// <summary>
        /// Enables and disables the sun disk
        /// </summary>
        /// <param name="sunDiskEnabled"></param>
        public static void SetEnableSunDisk(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            if (skyMaterial == null)
            {
                skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Ambient Skies Skybox"));
            }

            if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                if (skyMaterial == null)
                {
                    Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                    return;
                }
                if (profile.enableSunDisk)
                {
                    skyMaterial.SetFloat("_SunDisk", 2f);
                    skyMaterial.EnableKeyword("_SUNDISK_HIGH_QUALITY");
                    skyMaterial.DisableKeyword("_SUNDISK_NONE");
                    skyMaterial.SetFloat("_SunSize", profile.customProceduralSunSize);
                    skyMaterial.SetFloat("_SunSizeConvergence", profile.customProceduralSunSizeConvergence);
                }
                else
                {
                    skyMaterial.SetFloat("_SunDisk", 0f);
                    skyMaterial.DisableKeyword("_SUNDISK_HIGH_QUALITY");
                    skyMaterial.EnableKeyword("_SUNDISK_NONE");
                }
            }
            else
            {
                if (skyMaterial == null)
                {
                    Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                    return;
                }
                if (proceduralProfile.enableSunDisk)
                {
                    skyMaterial.SetFloat("_SunDisk", 2f);
                    skyMaterial.EnableKeyword("_SUNDISK_HIGH_QUALITY");
                    skyMaterial.DisableKeyword("_SUNDISK_NONE");
                    skyMaterial.SetFloat("_SunSize", proceduralProfile.proceduralSunSize);
                    skyMaterial.SetFloat("_SunSizeConvergence", proceduralProfile.proceduralSunSizeConvergence);
                }
                else
                {
                    skyMaterial.SetFloat("_SunDisk", 0f);
                    skyMaterial.DisableKeyword("_SUNDISK_HIGH_QUALITY");
                    skyMaterial.EnableKeyword("_SUNDISK_NONE");
                }
            }
        }

        /// <summary>
        /// Sets the quality settings
        /// </summary>
        /// <param name="profile"></param>
        public static void SetQualitySettings(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                QualitySettings.shadowmaskMode = profile.shadowmaskMode;

                QualitySettings.shadowResolution = profile.shadowResolution;
                QualitySettings.shadowProjection = profile.shadowProjection;

                QualitySettings.shadowDistance = profile.shadowDistance;
#if LWPipeline

#if !UNITY_2018_3_OR_NEWER
                if (lightweightRender == null)
                {
                    lightweightRender = AssetDatabase.LoadAssetAtPath<LightweightPipelineAsset>(GetAssetPath("Procedural Worlds Lightweight Pipeline Profile Ambient Skies"));
                if (skyProfiles.m_showDebug)
                {
                    Debug.LogWarning("Unable to set LWRP" + lightweightRender.ShadowDistance.ToString() + "distance. It's not a get; set variable");
                }
                }
                else
                {
                    if (skyProfiles.m_showDebug)
                    {
                        Debug.LogWarning("Unable to set LWRP" + lightweightRender.ShadowDistance.ToString() + "distance. It's not a get; set variable");
                    }
                }
#else
                if (lightweightRender == null)
                {
                    lightweightRender = AssetDatabase.LoadAssetAtPath<LightweightRenderPipelineAsset>(GetAssetPath("Procedural Worlds Lightweight Pipeline Profile Ambient Skies"));
                    EditorUtility.SetDirty(lightweightRender);
                    lightweightRender.shadowDistance = profile.shadowDistance;
                }
                else
                {
                    EditorUtility.SetDirty(lightweightRender);
                    lightweightRender.shadowDistance = profile.shadowDistance;
                }
#endif

#endif
                switch (profile.cascadeCount)
                {
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount1:
                        QualitySettings.shadowCascades = 1;
                        break;
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount2:
                        QualitySettings.shadowCascades = 2;
                        break;
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount3:
                        QualitySettings.shadowCascades = 3;
                        break;
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount4:
                        QualitySettings.shadowCascades = 4;
                        break;
                }
            }
            else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                QualitySettings.shadowmaskMode = proceduralProfile.shadowmaskMode;

                QualitySettings.shadowResolution = proceduralProfile.shadowResolution;
                QualitySettings.shadowProjection = proceduralProfile.shadowProjection;

                QualitySettings.shadowDistance = proceduralProfile.shadowDistance;
#if LWPipeline

#if !UNITY_2018_3_OR_NEWER
                if (lightweightRender == null)
                {
                    lightweightRender = AssetDatabase.LoadAssetAtPath<LightweightPipelineAsset>(GetAssetPath("Procedural Worlds Lightweight Pipeline Profile Ambient Skies"));
                if (skyProfiles.m_showDebug)
                {
                    Debug.LogWarning("Unable to set LWRP" + lightweightRender.ShadowDistance.ToString() + "distance. It's not a get; set variable");
                }
                }
                else
                {
                    if (skyProfiles.m_showDebug)
                    {
                        Debug.LogWarning("Unable to set LWRP" + lightweightRender.ShadowDistance.ToString() + "distance. It's not a get; set variable");
                    }
                }
#else
                if (lightweightRender == null)
                {
                    lightweightRender = AssetDatabase.LoadAssetAtPath<LightweightRenderPipelineAsset>(GetAssetPath("Procedural Worlds Lightweight Pipeline Profile Ambient Skies"));
                    EditorUtility.SetDirty(lightweightRender);
                    lightweightRender.shadowDistance = profile.shadowDistance;
                }
                else
                {
                    EditorUtility.SetDirty(lightweightRender);
                    lightweightRender.shadowDistance = profile.shadowDistance;
                }
#endif

#endif
                switch (proceduralProfile.cascadeCount)
                {
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount1:
                        QualitySettings.shadowCascades = 1;
                        break;
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount2:
                        QualitySettings.shadowCascades = 2;
                        break;
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount3:
                        QualitySettings.shadowCascades = 3;
                        break;
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount4:
                        QualitySettings.shadowCascades = 4;
                        break;
                }
            }
            else
            {
                QualitySettings.shadowmaskMode = gradientProfile.shadowmaskMode;

                QualitySettings.shadowResolution = gradientProfile.shadowResolution;
                QualitySettings.shadowProjection = gradientProfile.shadowProjection;

                QualitySettings.shadowDistance = gradientProfile.shadowDistance;
#if LWPipeline

#if !UNITY_2018_3_OR_NEWER
                if (lightweightRender == null)
                {
                    lightweightRender = AssetDatabase.LoadAssetAtPath<LightweightPipelineAsset>(GetAssetPath("Procedural Worlds Lightweight Pipeline Profile Ambient Skies"));
                if (skyProfiles.m_showDebug)
                {
                    Debug.LogWarning("Unable to set LWRP" + lightweightRender.ShadowDistance.ToString() + "distance. It's not a get; set variable");
                }
                }
                else
                {
                    if (skyProfiles.m_showDebug)
                    {
                        Debug.LogWarning("Unable to set LWRP" + lightweightRender.ShadowDistance.ToString() + "distance. It's not a get; set variable");
                    }
                }
#else
                if (lightweightRender == null)
                {
                    lightweightRender = AssetDatabase.LoadAssetAtPath<LightweightRenderPipelineAsset>(GetAssetPath("Procedural Worlds Lightweight Pipeline Profile Ambient Skies"));
                    EditorUtility.SetDirty(lightweightRender);
                    lightweightRender.shadowDistance = profile.shadowDistance;
                }
                else
                {
                    EditorUtility.SetDirty(lightweightRender);
                    lightweightRender.shadowDistance = profile.shadowDistance;
                }
#endif

#endif
                switch (gradientProfile.cascadeCount)
                {
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount1:
                        QualitySettings.shadowCascades = 1;
                        break;
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount2:
                        QualitySettings.shadowCascades = 2;
                        break;
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount3:
                        QualitySettings.shadowCascades = 3;
                        break;
                    case AmbientSkiesConsts.ShadowCascade.CascadeCount4:
                        QualitySettings.shadowCascades = 4;
                        break;
                }
            }
        }     

        /// <summary>
        /// Sets the ambient lighting and fog settings for Built-In and LWRP
        /// </summary>
        /// <param name="profile"></param>
        public static void SetAmbientLightingAndFog(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //Set ambient mode & intensity
                switch (profile.ambientMode)
                {
                    case AmbientSkiesConsts.AmbientMode.Color:
                        RenderSettings.ambientMode = AmbientMode.Flat;
                        if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                        {
                            RenderSettings.ambientSkyColor = profile.skyColor;
                        }
                        else
                        {
                            RenderSettings.ambientSkyColor = profile.lwrpSkyColor;
                        }
                        break;
                    case AmbientSkiesConsts.AmbientMode.Gradient:
                        RenderSettings.ambientMode = AmbientMode.Trilight;
                        if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                        {
                            RenderSettings.ambientSkyColor = profile.skyColor;
                            RenderSettings.ambientEquatorColor = profile.equatorColor;
                            RenderSettings.ambientGroundColor = profile.groundColor;
                        }
                        else
                        {
                            RenderSettings.ambientSkyColor = profile.lwrpSkyColor;
                            RenderSettings.ambientEquatorColor = profile.lwrpEquatorColor;
                            RenderSettings.ambientGroundColor = profile.lwrpGroundColor;
                        }
                        break;
                    case AmbientSkiesConsts.AmbientMode.Skybox:
                        RenderSettings.ambientMode = AmbientMode.Skybox;
                        RenderSettings.ambientIntensity = profile.skyboxGroundIntensity;
                        break;
                }

                //Setup the fog
                switch (profile.fogType)
                {
                    case AmbientSkiesConsts.VolumeFogType.None:
                        RenderSettings.fog = false;
                        break;
                    case AmbientSkiesConsts.VolumeFogType.Exponential:
                        RenderSettings.fog = true;
                        RenderSettings.fogMode = FogMode.Exponential;
                        RenderSettings.fogDensity = profile.fogDensity;
                        RenderSettings.fogColor = profile.fogColor;
                        break;
                    case AmbientSkiesConsts.VolumeFogType.ExponentialSquared:
                        RenderSettings.fog = true;
                        RenderSettings.fogMode = FogMode.ExponentialSquared;
                        RenderSettings.fogDensity = profile.fogDensity;
                        RenderSettings.fogColor = profile.fogColor;
                        break;
                    case AmbientSkiesConsts.VolumeFogType.Linear:
                        RenderSettings.fog = true;
                        RenderSettings.fogMode = FogMode.Linear;
                        RenderSettings.fogColor = profile.fogColor;
                        RenderSettings.fogStartDistance = profile.nearFogDistance;
                        RenderSettings.fogEndDistance = profile.fogDistance;
                        break;
                }

                if (profile.fogType != AmbientSkiesConsts.VolumeFogType.Volumetric)
                {
                    GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                    if (densityVolumeObject1 != null)
                    {
                        Object.DestroyImmediate(densityVolumeObject1);
                    }
                }
            }
            else
            {
                //Set ambient mode & intensity
                switch (proceduralProfile.ambientMode)
                {
                    case AmbientSkiesConsts.AmbientMode.Color:
                        RenderSettings.ambientMode = AmbientMode.Flat;
                        if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                        {
                            RenderSettings.ambientSkyColor = proceduralProfile.skyColor;
                        }
                        else
                        {
                            RenderSettings.ambientSkyColor = proceduralProfile.lwrpSkyColor;
                        }
                        break;
                    case AmbientSkiesConsts.AmbientMode.Gradient:
                        RenderSettings.ambientMode = AmbientMode.Trilight;
                        if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                        {
                            RenderSettings.ambientSkyColor = proceduralProfile.skyColor;
                            RenderSettings.ambientEquatorColor = proceduralProfile.equatorColor;
                            RenderSettings.ambientGroundColor = proceduralProfile.groundColor;
                        }
                        else
                        {
                            RenderSettings.ambientSkyColor = proceduralProfile.lwrpSkyColor;
                            RenderSettings.ambientEquatorColor = proceduralProfile.lwrpEquatorColor;
                            RenderSettings.ambientGroundColor = proceduralProfile.lwrpGroundColor;
                        }

                        break;
                    case AmbientSkiesConsts.AmbientMode.Skybox:
                        RenderSettings.ambientMode = AmbientMode.Skybox;
                        RenderSettings.ambientIntensity = proceduralProfile.skyboxGroundIntensity;
                        break;
                }

                //Setup the fog
                switch (proceduralProfile.fogType)
                {
                    case AmbientSkiesConsts.VolumeFogType.None:
                        RenderSettings.fog = false;
                        break;
                    case AmbientSkiesConsts.VolumeFogType.Exponential:
                        RenderSettings.fog = true;
                        RenderSettings.fogMode = FogMode.Exponential;
                        RenderSettings.fogDensity = proceduralProfile.proceduralFogDensity;
                        RenderSettings.fogColor = proceduralProfile.proceduralFogColor;
                        break;
                    case AmbientSkiesConsts.VolumeFogType.ExponentialSquared:
                        RenderSettings.fog = true;
                        RenderSettings.fogMode = FogMode.ExponentialSquared;
                        RenderSettings.fogDensity = proceduralProfile.proceduralFogDensity;
                        RenderSettings.fogColor = proceduralProfile.proceduralFogColor;
                        break;
                    case AmbientSkiesConsts.VolumeFogType.Linear:
                        RenderSettings.fog = true;
                        RenderSettings.fogMode = FogMode.Linear;
                        RenderSettings.fogColor = proceduralProfile.proceduralFogColor;
                        RenderSettings.fogStartDistance = proceduralProfile.proceduralNearFogDistance;
                        RenderSettings.fogEndDistance = proceduralProfile.proceduralFogDistance;
                        break;
                }

                if (proceduralProfile.fogType != AmbientSkiesConsts.VolumeFogType.Volumetric)
                {
                    GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                    if (densityVolumeObject1 != null)
                    {
                        Object.DestroyImmediate(densityVolumeObject1);
                    }
                }
            }
        }

        /// <summary>
        /// Sets up the sun light settings
        /// </summary>
        /// <param name="profile"></param>
        public static void SetSunLightSettings(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            if (directionalLight == null)
            {
                directionalLight = GetMainDirectionalLight();
            }
            //Set up the light if we have one
            else
            {
                //Now set the light values
                if (sunLight == null)
                {
                    sunLight = directionalLight.GetComponent<Light>();
                }
                else
                {
                    if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        if (skyProfiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                        {
                            sunLight.shadows = profile.shadowType;
                        }

                        sunLight.color = profile.sunColor;
                        if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                        {
                            sunLight.intensity = profile.sunIntensity;
                        }
                        else if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
                        {
                            sunLight.intensity = profile.LWRPSunIntensity;
                        }
                        sunLight.shadowStrength = profile.shadowStrength;
                        sunLight.bounceIntensity = profile.indirectLightMultiplier;
                    }
                    else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        if (skyProfiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                        {
                            sunLight.shadows = proceduralProfile.shadowType;
                        }

                        if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                        {
                            sunLight.intensity = proceduralProfile.proceduralSunIntensity;
                        }
                        else if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
                        {
                            sunLight.intensity = proceduralProfile.proceduralLWRPSunIntensity;
                        }
                        sunLight.color = proceduralProfile.proceduralSunColor;
                        sunLight.shadowStrength = proceduralProfile.shadowStrength;
                        sunLight.bounceIntensity = proceduralProfile.indirectLightMultiplier;
                    }
                }

                //Apply the light to the sky
                RenderSettings.sun = sunLight;
            }
        }

        /// <summary>
        /// Sets up the skyhbox loading and settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="isHDRI"></param>
        public static void LoadSkyboxMaterial(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, bool isHDRI)
        {
#if AMBIENT_SKIES_ENVIRO

            var enviroComponent = Object.FindObjectOfType<EnviroSkyMgr>();
            if (enviroComponent != null)
            {
                return;
            }

#endif
            if (RenderSettings.skybox != null)
            {
                if (RenderSettings.skybox.name != "Ambient Skies Skybox")
                {
                    skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Ambient Skies Skybox"));
                    RenderSettings.skybox = skyMaterial;
                    skyMaterial = RenderSettings.skybox;
                }
            }

            //Is a HDRI Skybox
            if (isHDRI)
            {
                //Conditionally load the skybox material
                bool loadSkyboxMaterial = false;
                if (skyMaterial == null)
                {
                    skyMaterial = RenderSettings.skybox;
                    loadSkyboxMaterial = true;
                }
                else
                {
                    if (skyMaterial.name != "Ambient Skies Skybox")
                    {
                        loadSkyboxMaterial = true;
                    }
                }

                if (loadSkyboxMaterial)
                {
                    //Get the skybox material path
                    string skyMaterialPath = GetAssetPath("Ambient Skies Skybox");
                    if (string.IsNullOrEmpty(skyMaterialPath))
                    {
                        Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                        return;
                    }
                    skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(skyMaterialPath);
                    if (skyMaterial == null)
                    {
                        Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                        return;
                    }

                    if (skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty || skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.DefaultProcedural)
                    {
                        RenderSettings.skybox = skyMaterial;
                    }
                }

                //Conditionally load the skybox texture 
                bool loadSkyboxTexture = false;
                if (skyboxTexture == null)
                {
                    skyboxTexture = skyMaterial.GetTexture("_Tex");
                    loadSkyboxTexture = true;
                }
                else
                {
                    if (skyboxTexture.name != profile.assetName)
                    {
                        loadSkyboxTexture = true;
                    }
                    else
                    {
                        loadSkyboxTexture = true;
                    }
                }
                if (loadSkyboxTexture)
                {
                    string hdrSkyPath;
                    if (!profile.isPWProfile)
                    {
                        if (profile.customSkybox == null)
                        {
                            hdrSkyPath = "";
                        }
                        else
                        {
                            hdrSkyPath = GetAssetPath(profile.customSkybox.name);
                            profile.assetName = profile.customSkybox.name + "Custom";
                        }
                    }
                    else
                    {
                        hdrSkyPath = GetAssetPath(profile.assetName);
                    }

                    //Get the skybox asset path                       
                    if (string.IsNullOrEmpty(hdrSkyPath))
                    {
                        if (skyProfiles.m_showDebug)
                        {
                            Debug.LogErrorFormat("AmbientSkies:SetSkybox : Unable to load '{0}' skybox asset - Aborting! ", profile.assetName, " Please ensure you have a cubemap selected");
                        }

                        return;
                    }

                    skyMaterial.SetTexture("_Tex", AssetDatabase.LoadAssetAtPath<Texture>(hdrSkyPath));
                }

                skyMaterial.shader = Shader.Find("Skybox/Cubemap");

                skyMaterial.SetColor("_Tint", profile.skyboxTint);
                skyMaterial.SetFloat("_Exposure", profile.skyboxExposure);
                skyMaterial.SetFloat("_Rotation", profile.skyboxRotation);

                //Do bake for reflection probe - bit only if a major load operation
                if (loadSkyboxMaterial || loadSkyboxTexture)
                {
                    LightingUtils.BakeGlobalReflectionProbe(false);
                }
            }
            //Is a Procedural Skybox
            else
            {
                //Conditionally load the skybox material
                bool loadSkyboxMaterial = false;
                if (skyMaterial == null)
                {
                    loadSkyboxMaterial = true;
                    skyMaterial = RenderSettings.skybox;
                }
                else
                {
                    if (skyMaterial.name != "Ambient Skies Skybox")
                    {
                        loadSkyboxMaterial = true;
                    }
                }

                if (loadSkyboxMaterial)
                {
                    //Get the skybox material path
                    string skyMaterialPath = GetAssetPath("Ambient Skies Skybox");
                    if (string.IsNullOrEmpty(skyMaterialPath))
                    {
                        Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                        return;
                    }

                    skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(skyMaterialPath);
                    if (skyMaterial == null)
                    {
                        Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                        return;
                    }

                    if (skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty || skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.DefaultProcedural)
                    {
                        RenderSettings.skybox = skyMaterial;
                    }
                }

                skyMaterial.shader = Shader.Find("Skybox/Procedural");

                if (!profile.isPWProfile)
                {
                    SetEnableSunDisk(skyProfiles, profile, proceduralProfile, gradientProfile);
                    skyMaterial.SetFloat("_SunSize", proceduralProfile.proceduralSunSize);
                    skyMaterial.SetFloat("_SunSizeConvergence", profile.customProceduralSunSizeConvergence);
                    skyMaterial.SetFloat("_AtmosphereThickness", profile.customProceduralAtmosphereThickness);
                    skyMaterial.SetColor("_SkyTint", profile.customSkyboxTint);
                    skyMaterial.SetColor("_GroundColor", profile.customProceduralGroundColor);
                    skyMaterial.SetFloat("_Exposure", profile.customSkyboxExposure);
                }
                else
                {
                    SetEnableSunDisk(skyProfiles, profile, proceduralProfile, gradientProfile);
                    skyMaterial.SetFloat("_SunSize", proceduralProfile.proceduralSunSize);
                    skyMaterial.SetFloat("_SunSizeConvergence", proceduralProfile.proceduralSunSizeConvergence);
                    skyMaterial.SetFloat("_AtmosphereThickness", proceduralProfile.proceduralAtmosphereThickness);
                    skyMaterial.SetColor("_SkyTint", proceduralProfile.proceduralSkyTint);
                    skyMaterial.SetColor("_GroundColor", proceduralProfile.proceduralGroundColor);
                    skyMaterial.SetFloat("_Exposure", proceduralProfile.proceduralSkyExposure);
                }
            }     
        }

        /// <summary>
        /// Unparents and reparents gameobject in Gaia to new Ambient Skies
        /// </summary>
        /// <param name="alsoReparent"></param>
        public static void SetGaiaParenting(bool alsoReparent)
        {
            if (newParentObject == null)
            {
                newParentObject = GetOrCreateParentObject("Ambient Skies Environment", false);
            }

            if (directionalLight == null)
            {
                directionalLight = GetMainDirectionalLight();
            }

            if (postProcessingObject == null)
            {
                postProcessingObject = GameObject.Find("Global Post Processing");
            }

            if (reflectionProbeObject == null)
            {
                reflectionProbeObject = GameObject.Find("Global Reflection Probe");
            }

            if (ambientAudioObject == null)
            {
                ambientAudioObject = GameObject.Find("Ambient Audio");
            }

            if (directionalLight != null)
            {
                directionalLight.transform.SetParent(null);
                if (alsoReparent)
                {
                    directionalLight.transform.SetParent(newParentObject.transform);
                }
            }
            if (postProcessingObject != null)
            {
                postProcessingObject.transform.SetParent(null);
                if (alsoReparent)
                {
                    postProcessingObject.transform.SetParent(newParentObject.transform);
                }
            }
            if (reflectionProbeObject != null)
            {
                reflectionProbeObject.transform.SetParent(null);
                if (alsoReparent)
                {
                    reflectionProbeObject.transform.SetParent(newParentObject.transform);
                }
            }
            if (ambientAudioObject != null)
            {
                ambientAudioObject.transform.SetParent(null);
                if (alsoReparent)
                {
                    ambientAudioObject.transform.SetParent(newParentObject.transform);
                }
            }

            //Find parent object
            if (oldGaiaAmbientParent == null)
            {
                oldGaiaAmbientParent = GameObject.Find("Ambient Skies Samples");
            }
            else
            {
                //Find parents in parent object
                Transform[] parentChilds = oldGaiaAmbientParent.GetComponentsInChildren<Transform>();
                if (parentChilds.Length == 1)
                {
                    //Destroy object if object is empty
                    Object.DestroyImmediate(oldGaiaAmbientParent);
                }
            }
        }

        /// <summary>
        /// Removes enviro from the scene
        /// </summary>
        /// <param name="removeAllObjects"></param>
        public static void RemoveEnviro(bool removeAllObjects)
        {
            if (removeAllObjects)
            {
                GameObject enviroObject1 = GameObject.Find("Enviro Sky Manager");
                if (enviroObject1 != null)
                {
                    Object.DestroyImmediate(enviroObject1);
                }
                GameObject enviroObject2 = GameObject.Find("EnviroSky Standard");
                if (enviroObject2 != null)
                {
                    Object.DestroyImmediate(enviroObject2);
                }
                GameObject enviroObject3 = GameObject.Find("EnviroSky Lite");
                if (enviroObject3 != null)
                {
                    Object.DestroyImmediate(enviroObject3);
                }
                GameObject enviroObject4 = GameObject.Find("EnviroSky Lite for Mobiles");
                if (enviroObject4 != null)
                {
                    Object.DestroyImmediate(enviroObject4);
                }
                GameObject enviroObject5 = GameObject.Find("EnviroSky Standard for VR");
                if (enviroObject5 != null)
                {
                    Object.DestroyImmediate(enviroObject5);
                }
                GameObject enviroObject6 = GameObject.Find("Enviro Effects");
                if (enviroObject6 != null)
                {
                    Object.DestroyImmediate(enviroObject6);
                }
                GameObject enviroObject7 = GameObject.Find("Enviro Directional Light");
                if (enviroObject7 != null)
                {
                    Object.DestroyImmediate(enviroObject7);
                }
                GameObject enviroObject8 = GameObject.Find("Enviro Sky Manager for GAIA");
                if (enviroObject8 != null)
                {
                    Object.DestroyImmediate(enviroObject8);
                }
                GameObject enviroObject9 = GameObject.Find("Enviro Effects LW");
                if (enviroObject9 != null)
                {
                    Object.DestroyImmediate(enviroObject9);
                }

                GameObject sun = GetMainDirectionalLight();
                if (sun != null)
                {
                    RenderSettings.sun = sun.GetComponent<Light>();
                }

                if (RenderSettings.skybox != null)
                {
                    if (RenderSettings.skybox.name != "Ambient Skies Skybox")
                    {
                        RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
                    }
                }

#if AMBIENT_SKIES_ENVIRO
                EnviroSkyRendering skyRendering = Object.FindObjectOfType<EnviroSkyRendering>();
                if (skyRendering != null)
                {
                    Object.DestroyImmediate(skyRendering);
                }

                EnviroSkyRenderingLW skyRenderingLW = Object.FindObjectOfType<EnviroSkyRenderingLW>();
                if (skyRenderingLW != null)
                {
                    Object.DestroyImmediate(skyRenderingLW);
                }

                EnviroPostProcessing enviroPost = Object.FindObjectOfType<EnviroPostProcessing>();
                if (enviroPost != null)
                {
                    Object.DestroyImmediate(enviroPost);
                }
#endif
            }
        }

        #endregion

        #region Time Of Day Setup

        /// <summary>
        /// Adds time of day
        /// </summary>
        /// <param name="profile"></param>
        public static void AddTimeOfDay(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles)
        {
            if (skyProfiles.m_showFunctionDebugsOnly)
            {
                Debug.Log("Updating AddTimeOfDay()");
            }

            if (directionalLight == null)
            {
                directionalLight = GetMainDirectionalLight();
            }

            if (sunLight == null)
            {
                sunLight = directionalLight.GetComponent<Light>();
                sunLight.enabled = false;
            }
            else
            {
                sunLight.enabled = false;
            }

            if (skyProfiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                HDRISky(profile, skyProfiles);
            }

            if (timeOfDaySystem == null)
            {
                timeOfDaySystem = GameObject.Find("AS Time Of Day");
            }

            if (newParentObject == null)
            {
                newParentObject = GetOrCreateParentObject("Ambient Skies Environment", false);
            }

            if (timeOfDaySystem == null)
            {
                timeOfDaySystem = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetPath("AS Time Of Day")));

                GameObject tod = timeOfDaySystem as GameObject;
                if (tod != null)
                {
                    tod.transform.SetParent(newParentObject.transform);
                }
            }
        }

        /// <summary>
        /// Sets the time of day settings
        /// </summary>
        /// <param name="profile"></param>
        public static void SetTimeOfDaySettings(AmbientProceduralSkyboxProfile profile, AmbientSkyProfiles skyProfiles)
        {
            if (timeOfDay == null)
            {
                timeOfDay = Object.FindObjectOfType<AmbientSkiesTimeOfDay>();

                if (!skyProfiles.m_pauseTime && Application.isPlaying)
                {
                    //profile.currentTimeOfDay = timeOfDay.m_currentTime;
                }

                //Apply all main script objects
                timeOfDay.m_ambientSkiesProfileVol1 = skyProfiles;
                timeOfDay.m_timeOfDayProfile = skyProfiles.m_timeOfDayProfile;
                timeOfDay.m_ambientProceduralSkybox = profile;

                timeOfDay.m_timeOfDayProfile.m_renderPipeline = skyProfiles.m_selectedRenderPipeline;

                timeOfDay.m_timeOfDayProfile.m_daySunIntensity = skyProfiles.m_daySunIntensity;
                timeOfDay.m_timeOfDayProfile.m_daySunGradientColor = skyProfiles.m_daySunGradientColor;
                timeOfDay.m_timeOfDayProfile.m_nightSunIntensity = skyProfiles.m_nightSunIntensity;
                timeOfDay.m_timeOfDayProfile.m_nightSunGradientColor = skyProfiles.m_nightSunGradientColor;
                timeOfDay.m_timeOfDayProfile.m_startFogDistance = skyProfiles.m_startFogDistance;
                timeOfDay.m_timeOfDayProfile.m_dayFogDensity = skyProfiles.m_dayFogDensity;
                timeOfDay.m_timeOfDayProfile.m_dayFogDistance = skyProfiles.m_dayFogDistance;
                timeOfDay.m_timeOfDayProfile.m_nightFogDensity = skyProfiles.m_nightFogDensity;
                timeOfDay.m_timeOfDayProfile.m_nightFogDistance = skyProfiles.m_nightFogDistance;
                timeOfDay.m_timeOfDayProfile.m_dayFogColor = skyProfiles.m_dayFogColor;
                timeOfDay.m_timeOfDayProfile.m_nightFogColor = skyProfiles.m_nightFogColor;
                timeOfDay.m_timeOfDayProfile.m_dayColor = skyProfiles.m_dayPostFXColor;
                timeOfDay.m_timeOfDayProfile.m_nightColor = skyProfiles.m_nightPostFXColor;
                timeOfDay.m_timeOfDayProfile.m_dayTempature = skyProfiles.m_dayTempature;
                timeOfDay.m_timeOfDayProfile.m_nightTempature = skyProfiles.m_nightTempature;
                timeOfDay.m_timeOfDayProfile.m_lightAnisotropy = skyProfiles.m_lightAnisotropy;
                timeOfDay.m_timeOfDayProfile.m_lightProbeDimmer = skyProfiles.m_lightProbeDimmer;
                timeOfDay.m_timeOfDayProfile.m_lightDepthExtent = skyProfiles.m_lightDepthExtent;
                timeOfDay.m_timeOfDayProfile.m_sunSize = skyProfiles.m_sunSize;
                timeOfDay.m_timeOfDayProfile.m_skyExposure = skyProfiles.m_skyExposure;
                timeOfDay.m_timeOfDayProfile.m_realtimeGIUpdate = skyProfiles.m_realtimeGIUpdate;
                timeOfDay.m_timeOfDayProfile.m_gIUpdateIntervalInSeconds = skyProfiles.m_gIUpdateInterval;
                timeOfDay.m_timeOfDayProfile.m_pauseTime = skyProfiles.m_pauseTime;
                timeOfDay.m_timeOfDayProfile.m_syncPostFXToTimeOfDay = skyProfiles.m_syncPostProcessing;
                timeOfDay.m_timeOfDayProfile.m_sunRotation = skyProfiles.m_skyboxRotation;
                timeOfDay.m_timeOfDayProfile.m_dayLengthInSeconds = skyProfiles.m_dayLengthInSeconds;
                timeOfDay.m_timeOfDayProfile.m_environmentSeason = skyProfiles.m_currentSeason;
                timeOfDay.m_timeOfDayProfile.m_hemisphereOrigin = skyProfiles.m_hemisphereOrigin;
                timeOfDay.m_timeOfDayProfile.m_day = skyProfiles.m_dayDate;
                timeOfDay.m_timeOfDayProfile.m_month = skyProfiles.m_monthDate;
                timeOfDay.m_timeOfDayProfile.m_year = skyProfiles.m_yearDate;
                timeOfDay.m_timeOfDayProfile.m_nightLengthInSeconds = skyProfiles.m_nightLengthInSeconds;

                timeOfDay.m_timeOfDayProfile.m_currentTime = skyProfiles.m_currentTimeOfDay;
                timeOfDay.m_timeOfDayProfile.m_pauseTimeKey = skyProfiles.m_pauseTimeKey;
                timeOfDay.m_timeOfDayProfile.m_incrementUpKey = skyProfiles.m_incrementUpKey;
                timeOfDay.m_timeOfDayProfile.m_incrementDownKey = skyProfiles.m_incrementDownKey;
                timeOfDay.m_timeOfDayProfile.m_timeToAddOrRemove = skyProfiles.m_timeToAddOrRemove;
                timeOfDay.m_timeOfDayProfile.m_rotateSunLeftKey = skyProfiles.m_rotateSunLeftKey;
                timeOfDay.m_timeOfDayProfile.m_rotateSunRightKey = skyProfiles.m_rotateSunRightKey;
                timeOfDay.m_timeOfDayProfile.m_sunRotationAmount = skyProfiles.m_sunRotationAmount;
                if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Exponential)
                {
                    RenderSettings.fog = true;
                    timeOfDay.m_timeOfDayProfile.m_fogMode = FogMode.Exponential;
                }
                else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                {
                    RenderSettings.fog = true;
                    timeOfDay.m_timeOfDayProfile.m_fogMode = FogMode.Linear;
                }
                else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.None)
                {
                    RenderSettings.fog = false;
                }
            }
            else
            {
                if (!skyProfiles.m_pauseTime && Application.isPlaying)
                {
                    //profile.currentTimeOfDay = timeOfDay.m_currentTime;
                }

                //Apply all main script objects
                timeOfDay.m_ambientSkiesProfileVol1 = skyProfiles;
                timeOfDay.m_timeOfDayProfile = skyProfiles.m_timeOfDayProfile;
                timeOfDay.m_ambientProceduralSkybox = profile;

                timeOfDay.m_timeOfDayProfile.m_renderPipeline = skyProfiles.m_selectedRenderPipeline;

                timeOfDay.m_timeOfDayProfile.m_daySunIntensity = skyProfiles.m_daySunIntensity;
                timeOfDay.m_timeOfDayProfile.m_daySunGradientColor = skyProfiles.m_daySunGradientColor;
                timeOfDay.m_timeOfDayProfile.m_nightSunIntensity = skyProfiles.m_nightSunIntensity;
                timeOfDay.m_timeOfDayProfile.m_nightSunGradientColor = skyProfiles.m_nightSunGradientColor;
                timeOfDay.m_timeOfDayProfile.m_startFogDistance = skyProfiles.m_startFogDistance;
                timeOfDay.m_timeOfDayProfile.m_dayFogDensity = skyProfiles.m_dayFogDensity;
                timeOfDay.m_timeOfDayProfile.m_dayFogDistance = skyProfiles.m_dayFogDistance;
                timeOfDay.m_timeOfDayProfile.m_nightFogDensity = skyProfiles.m_nightFogDensity;
                timeOfDay.m_timeOfDayProfile.m_nightFogDistance = skyProfiles.m_nightFogDistance;
                timeOfDay.m_timeOfDayProfile.m_dayFogColor = skyProfiles.m_dayFogColor;
                timeOfDay.m_timeOfDayProfile.m_nightFogColor = skyProfiles.m_nightFogColor;
                timeOfDay.m_timeOfDayProfile.m_dayColor = skyProfiles.m_dayPostFXColor;
                timeOfDay.m_timeOfDayProfile.m_nightColor = skyProfiles.m_nightPostFXColor;
                timeOfDay.m_timeOfDayProfile.m_dayTempature = skyProfiles.m_dayTempature;
                timeOfDay.m_timeOfDayProfile.m_nightTempature = skyProfiles.m_nightTempature;
                timeOfDay.m_timeOfDayProfile.m_lightAnisotropy = skyProfiles.m_lightAnisotropy;
                timeOfDay.m_timeOfDayProfile.m_lightProbeDimmer = skyProfiles.m_lightProbeDimmer;
                timeOfDay.m_timeOfDayProfile.m_lightDepthExtent = skyProfiles.m_lightDepthExtent;
                timeOfDay.m_timeOfDayProfile.m_sunSize = skyProfiles.m_sunSize;
                timeOfDay.m_timeOfDayProfile.m_skyExposure = skyProfiles.m_skyExposure;
                timeOfDay.m_timeOfDayProfile.m_realtimeGIUpdate = skyProfiles.m_realtimeGIUpdate;
                timeOfDay.m_timeOfDayProfile.m_gIUpdateIntervalInSeconds = skyProfiles.m_gIUpdateInterval;
                timeOfDay.m_timeOfDayProfile.m_pauseTime = skyProfiles.m_pauseTime;
                timeOfDay.m_timeOfDayProfile.m_syncPostFXToTimeOfDay = skyProfiles.m_syncPostProcessing;
                timeOfDay.m_timeOfDayProfile.m_sunRotation = skyProfiles.m_skyboxRotation;
                timeOfDay.m_timeOfDayProfile.m_dayLengthInSeconds = skyProfiles.m_dayLengthInSeconds;
                timeOfDay.m_timeOfDayProfile.m_environmentSeason = skyProfiles.m_currentSeason;
                timeOfDay.m_timeOfDayProfile.m_hemisphereOrigin = skyProfiles.m_hemisphereOrigin;
                timeOfDay.m_timeOfDayProfile.m_day = skyProfiles.m_dayDate;
                timeOfDay.m_timeOfDayProfile.m_month = skyProfiles.m_monthDate;
                timeOfDay.m_timeOfDayProfile.m_year = skyProfiles.m_yearDate;
                timeOfDay.m_timeOfDayProfile.m_nightLengthInSeconds = skyProfiles.m_nightLengthInSeconds;

                timeOfDay.m_timeOfDayProfile.m_currentTime = skyProfiles.m_currentTimeOfDay;
                timeOfDay.m_timeOfDayProfile.m_pauseTimeKey = skyProfiles.m_pauseTimeKey;
                timeOfDay.m_timeOfDayProfile.m_incrementUpKey = skyProfiles.m_incrementUpKey;
                timeOfDay.m_timeOfDayProfile.m_incrementDownKey = skyProfiles.m_incrementDownKey;
                timeOfDay.m_timeOfDayProfile.m_timeToAddOrRemove = skyProfiles.m_timeToAddOrRemove;
                timeOfDay.m_timeOfDayProfile.m_rotateSunLeftKey = skyProfiles.m_rotateSunLeftKey;
                timeOfDay.m_timeOfDayProfile.m_rotateSunRightKey = skyProfiles.m_rotateSunRightKey;
                timeOfDay.m_timeOfDayProfile.m_sunRotationAmount = skyProfiles.m_sunRotationAmount;
                if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Exponential)
                {
                    RenderSettings.fog = true;
                    timeOfDay.m_timeOfDayProfile.m_fogMode = FogMode.Exponential;
                }
                else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                {
                    RenderSettings.fog = true;
                    timeOfDay.m_timeOfDayProfile.m_fogMode = FogMode.Linear;
                }
                else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.None)
                {
                    RenderSettings.fog = false;
                }
            }

            if(skyProfiles.m_realtimeEmission)
            {
                if (emissionSetup == null)
                {
                    emissionSetup = GameObject.Find("AS Time Of Day").GetComponent<RealtimeEmissionSetup>();
                    emissionSetup = GameObject.Find("AS Time Of Day").AddComponent<RealtimeEmissionSetup>();
                    emissionSetup.m_syncToASTimeOfDay = skyProfiles.m_syncRealtimeEmissionToTimeOfDay;
                }
                else
                {
                    emissionSetup.m_syncToASTimeOfDay = skyProfiles.m_syncRealtimeEmissionToTimeOfDay;
                }
            }
            else
            {
                RealtimeEmissionSetup emissionSetup = GameObject.Find("AS Time Of Day").GetComponent<RealtimeEmissionSetup>();
                if (emissionSetup != null)
                {
                    Object.DestroyImmediate(emissionSetup);
                }
            }
        }

        /// <summary>
        /// Removes time of day system
        /// </summary>
        public static void RemoveTimeOfDay()
        {
            GameObject timeOfDay = GameObject.Find("AS Time Of Day");
            if (timeOfDay != null)
            {
                Object.DestroyImmediate(timeOfDay);
            }

            GameObject mainLight = GetMainDirectionalLight();
            if (mainLight != null)
            {
                Light light = mainLight.GetComponent<Light>();
                if (light != null)
                {
                    light.enabled = true;
                    RenderSettings.sun = light;
                }
            }

            DestroyParent("Ambient Skies Environment");
        }

        #endregion

        #region Horizon Setup

        /// <summary>
        /// Removes the Horizon Sky
        /// </summary>
        public static void RemoveHorizonSky()
        {
            GameObject horizonSky = GameObject.Find("Ambient Skies Horizon");

            if (horizonSky != null)
            {
                Object.DestroyImmediate(horizonSky);
            }
        }

        #endregion

        #region Sun Setup

        /// <summary>
        /// Get the intensition of the sun
        /// </summary>
        /// <returns>Intensity of the sun</returns>
        public static float GetSunIntensity()
        {
            GameObject directionalLightObj = GetMainDirectionalLight();
            if (directionalLightObj != null && directionalLightObj.GetComponent<Light>() != null)
            {
                return directionalLightObj.GetComponent<Light>().intensity;
            }
            return 1f;
        }

        /// <summary>
        /// Set the intensity of the sun
        /// </summary>
        /// <param name="intensity">New sun intensity</param>
        public static void SetSunIntensity(float intensity)
        {
            GameObject directionalLightObj = GetMainDirectionalLight();
            if (directionalLightObj != null && directionalLightObj.GetComponent<Light>() != null)
            {
                directionalLightObj.GetComponent<Light>().intensity = intensity;
            }
        }

        /// <summary>
        /// Set the color of the sun
        /// </summary>
        /// <param name="sunColor">New sun color</param>
        public static void SetSunColor(Color sunColor)
        {
            GameObject directionalLightObj = GetMainDirectionalLight();
            if (directionalLightObj != null && directionalLightObj.GetComponent<Light>() != null)
            {
                directionalLightObj.GetComponent<Light>().color = sunColor;
            }
        }

        #endregion

        #region Fog Setup
        
        /// <summary>
        /// Set the color of the fog
        /// </summary>
        /// <param name="fogColor">New fog color</param>
        public static void SetFogColor(Color fogColor)
        {
            RenderSettings.fogColor = fogColor;
        }

        #endregion

        #region Quality Settings Setup

        /// <summary>
        /// Sets the vsync count for your project in the quality settings
        /// </summary>
        /// <param name="skyProfiles"></param>
        public static void VSyncSettings(AmbientSkyProfiles skyProfiles)
        {
            //Set vsync mode
            switch (skyProfiles.m_vSyncMode)
            {
                case AmbientSkiesConsts.VSyncMode.DontSync:
                    QualitySettings.vSyncCount = 0;
                    break;
                case AmbientSkiesConsts.VSyncMode.EveryVBlank:
                    QualitySettings.vSyncCount = 1;
                    break;
                case AmbientSkiesConsts.VSyncMode.EverySecondVBlank:
                    QualitySettings.vSyncCount = 2;
                    break;
            }
        }

        #endregion

        #region Remove HDRP

        /// <summary>
        /// Removes HDRP objects from the scene
        /// </summary>
        public static void RemoveHDRPObjects()
        {
            GameObject hdrpWind = GameObject.Find("Ambient Skies HDRP Wind");
            if (hdrpWind != null)
            {
                Object.DestroyImmediate(hdrpWind);
            }

            GameObject hdrpPostProcess = GameObject.Find("Post Processing HDRP Volume");
            if (hdrpPostProcess != null)
            {
                Object.DestroyImmediate(hdrpPostProcess);
            }

            DestroyParent("Ambient Skies Environment");
        }

        #endregion

        #endregion

        #region Custom Utils

        /// <summary>
        /// Clear the console
        /// </summary>
        public static void ClearLog()
        {
            var logConsole = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");

            var clearConsole = logConsole.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            clearConsole.Invoke(null, null);
        }

        /// <summary>
        /// Gets and returns the lowest point of the terrain
        /// "rowsAndColumns" Lower the number the more accurate the search
        /// </summary>
        /// <param name="skyProfiles"></param>
        /// <param name="rowsAndColumns"></param>
        /// <returns></returns>
        public static void SampleTerrainHeights(AmbientSkyProfiles skyProfiles, int rowsAndColumns, out float lowestPoint, out float highestPoint)
        {
            //Initialize
            lowestPoint = float.MaxValue;
            highestPoint = float.MinValue;

#if GAIA_PRESENT
            //Get session manager
            GaiaSessionManager gaiaSession = Object.FindObjectOfType<GaiaSessionManager>();

            //Check if it's present
            if (gaiaSession != null)
            {
                lowestPoint = gaiaSession.GetSeaLevel();
                highestPoint = lowestPoint + 50f;
            }
#endif
            //Get the active terrain
            Terrain terrain = Terrain.activeTerrain;

            //Process the terrain
            if (terrain != null)
            {
                //Sanitize rows and columns
                if (rowsAndColumns < 1)
                {
                    rowsAndColumns = 1;
                }

                //Terrain size data
                Vector3 terrainSize = terrain.terrainData.size;

                float sampledHeight = 0;
                float stepSizeX = terrainSize.x / rowsAndColumns;
                float stepSizeZ = terrainSize.z / rowsAndColumns;
                float minX = terrain.transform.position.x;
                float minZ = terrain.transform.position.z;
                float maxX = minX + terrainSize.x;
                float maxZ = minZ + terrainSize.z;
                Vector3 samplePos = Vector3.zero;

                for (float x = minX; x < maxX; x+= stepSizeX)
                {
                    for (float z = minZ; z < maxZ; z += stepSizeZ)
                    {
                        samplePos.x = x;
                        samplePos.z = z;

                        //The sampled height
                        sampledHeight = terrain.SampleHeight(samplePos);

                        //Check to see if sampled height is less than lowest point
                        if (sampledHeight < lowestPoint)
                        {
                            lowestPoint = sampledHeight;
                        }

                        if (sampledHeight > highestPoint)
                        {
                            highestPoint = sampledHeight;
                        }
                    }
                }
            }

            highestPoint += 15f;

            //Fix up any potential issues with the data
            if (lowestPoint == float.MaxValue)
            {
                lowestPoint = 0f;
                highestPoint = 50f;
            }

            //Round Numbers to highest 
            lowestPoint = Mathf.Round(lowestPoint);
            highestPoint = Mathf.Round(highestPoint);

#if GAIA_PRESENT
            //Update min level back to sea level
            if (gaiaSession != null)
            {
                lowestPoint = gaiaSession.GetSeaLevel();
            }
#endif

            if (skyProfiles.m_showDebug)
            {
                Debug.Log("Lowest point of the terrain is " + lowestPoint + ", hightest is " + highestPoint);
            }
        }

        /// <summary>
        /// Gets and returns the highest point of the terrain
        /// "rowsAndColumns" Higher the number the more accurate the search
        /// </summary>
        /// <param name="skyProfiles"></param>
        /// <param name="rowsAndColumns"></param>
        /// <returns></returns>
        public static float GetHighestTerrainPoint(AmbientSkyProfiles skyProfiles, int rowsAndColumns)
        {
            //Default lowest point
            float lowestPoint = 20f;

            //Sanitize rows and columns
            if (rowsAndColumns < 1)
            {
                rowsAndColumns = 1;
            }

            //Get the active terrain
            Terrain terrain = Terrain.activeTerrain;

            //Process the terrain
            if (terrain != null)
            {
                lowestPoint = float.MinValue;

                //Terrain size data
                Vector3 terrainSize = terrain.terrainData.size;

                float sampledHeight = 0;
                float stepSizeX = terrainSize.x / rowsAndColumns;
                float stepSizeZ = terrainSize.z / rowsAndColumns;
                float minX = terrain.transform.position.x;
                float minZ = terrain.transform.position.z;
                float maxX = minX + terrainSize.x;
                float maxZ = minZ + terrainSize.z;
                Vector3 samplePos = Vector3.zero;

                for (float x = minX; x < maxX; x += stepSizeX)
                {
                    for (float z = minZ; z < maxZ; z += stepSizeZ)
                    {
                        samplePos.x = x;
                        samplePos.z = z;

                        //The sampled height
                        sampledHeight = terrain.SampleHeight(samplePos);

                        //Check to see if sampled height is less than lowest point
                        if (sampledHeight > lowestPoint)
                        {
                            lowestPoint = sampledHeight;
                        }
                    }
                }

                if (skyProfiles.m_showDebug)
                {
                    Debug.Log("Lowest point of the terrain is " + lowestPoint);
                }

                //Return the set value
                return lowestPoint;
            }

            //If nothing else return the default value
            return lowestPoint;
        }

        /// <summary>
        /// This removes the gameobject that is created on a new scene
        /// </summary>
        /// <param name="skyProfiles"></param>
        public static void RemoveNewSceneObject(AmbientSkyProfiles skyProfiles)
        {
            if (skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.DefaultProcedural || skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                GameObject newSceneObject = GameObject.Find("Ambient Skies New Scene Object (Don't Delete Me)");
                if (newSceneObject != null)
                {
                    Object.DestroyImmediate(newSceneObject);
                }
            }
        }

        /// <summary>
        /// Setup the main camera for use with HDR skyboxes
        /// </summary>
        /// <param name="mainCameraObj"></param>
        public static void SetupMainCamera(GameObject mainCameraObj)
        {
            if (mainCameraObj.GetComponent<FlareLayer>() == null)
            {
                mainCameraObj.AddComponent<FlareLayer>();
            }

            if (mainCameraObj.GetComponent<AudioListener>() == null)
            {
                mainCameraObj.AddComponent<AudioListener>();
            }

            Camera mainCamera = mainCameraObj.GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = mainCameraObj.AddComponent<Camera>();
            }

#if UNITY_5_6_OR_NEWER
            mainCamera.allowHDR = true;
#else
                mainCamera.hdr = true;
#endif

#if UNITY_2017_0_OR_NEWER
                mainCamera.allowMSAA = false;
#endif
        }

        /// <summary>
        /// Get a color from a html string
        /// </summary>
        /// <param name="htmlString">Color in RRGGBB or RRGGBBBAA or #RRGGBB or #RRGGBBAA format.</param>
        /// <returns>Color or white if unable to parse it.</returns>
        public static Color GetColorFromHTML(string htmlString)
        {
            Color color = Color.white;
            if (!htmlString.StartsWith("#"))
            {
                htmlString = "#" + htmlString;
            }
            if (!ColorUtility.TryParseHtmlString(htmlString, out color))
            {
                color = Color.white;
            }
            return color;
        }

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>The path or null</returns>
        public static string GetAssetPath(string name)
        {
            string[] assets = AssetDatabase.FindAssets(name, null);
            if (assets.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assets[0]);
            }
            return null;
        }

        /// <summary>
        /// Get or create a parent object
        /// </summary>
        /// <param name="parentGameObject">Name of the parent object to get or create</param>
        /// <returns>Parent objet</returns>
        public static GameObject GetOrCreateParentObject(string parentGameObject, bool parentToGaia)
        {
            //Get the parent object
            GameObject theParentGo = GameObject.Find(parentGameObject);

            if (theParentGo == null)
            {
                theParentGo = GameObject.Find("Ambient Skies Environment");

                if (theParentGo == null)
                {
                    theParentGo = new GameObject("Ambient Skies Environment");
                }
            }

            if (parentToGaia)
            {
                GameObject gaiaParent = GameObject.Find("Gaia Environment");
                if (gaiaParent != null)
                {
                    theParentGo.transform.SetParent(gaiaParent.transform);
                }
            }

            return theParentGo;
        }

        /// <summary>
        /// Get or create the main camera in the scene
        /// </summary>
        /// <returns>Existing or new main camera</returns>
        public static GameObject GetOrCreateMainCamera()
        {
            //Get or create the main camera
            GameObject mainCameraObj = null;

            if (Camera.main != null)
            {
                mainCameraObj = Camera.main.gameObject;
            }

            if (mainCameraObj == null)
            {
                mainCameraObj = GameObject.Find("Main Camera");
            }

            if (mainCameraObj == null)
            {
                mainCameraObj = GameObject.Find("Camera");
            }

            if (mainCameraObj == null)
            {
                Camera[] cameras = Object.FindObjectsOfType<Camera>();
                foreach (var camera in cameras)
                {
                    mainCameraObj = camera.gameObject;
                    break;
                }
            }

            if (mainCameraObj == null)
            {
                mainCameraObj = new GameObject("Main Camera");
                mainCameraObj.tag = "MainCamera";
                SetupMainCamera(mainCameraObj);
            }

            return mainCameraObj;
        }

        /// <summary>
        /// Get the main directional light in the scene
        /// </summary>
        /// <returns>Main light or null</returns>
        public static GameObject GetMainDirectionalLight()
        {
            GameObject lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                //Grab the first directional light we can find
                Light[] lights = Object.FindObjectsOfType<Light>();
                foreach (var light in lights)
                {
                    if (light.type == LightType.Directional)
                    {
                        lightObj = light.gameObject;
                    }
                }

                if (lightObj == null)
                {
                    lightObj = new GameObject("Directional Light");
                    lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                    Light lightSettings = lightObj.AddComponent<Light>();
                    lightSettings.type = LightType.Directional;
                }
            }
            return lightObj;
        }

        /// <summary>
        /// Find parent object and destroys it if it's empty
        /// </summary>
        /// <param name="parentGameObject"></param>
        public static void DestroyParent(string parentGameObject)
        {
            //If string isn't empty
            if(!string.IsNullOrEmpty(parentGameObject))
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