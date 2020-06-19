//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.Rendering;
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif
#if LWPipeline && UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering.LWRP;
#elif LWPipeline && UNITY_2018_3_OR_NEWER
using UnityEngine.Experimental.Rendering.LightweightPipeline;
#endif

#if CTS_PRESENT
using CTS;
#endif
#if GAIA_PRESENT
using Gaia;
#endif

namespace AmbientSkies
{
    public static class AmbientSkiesPipelineUtils
    {
        #region Utils

        #region Variables

        //Get parent object
        public static GameObject parentObject;
        //Get volume object
        public static GameObject volumeObject;
        //Active terrain
        public static Terrain terrain;
        //Density volume object
        public static GameObject densityFogVolume;
        //The main light object
        public static GameObject directionalLight;
        //The sun light
        public static Light sunLight;
        //Camera object
        public static GameObject mainCamera;

#if HDPipeline && UNITY_2018_3_OR_NEWER
        //The volume object
        public static Volume volumeSettings;
        //The volume profile that contains the settings
        public static VolumeProfile volumeProfile;
        //The density volume settings
        public static DensityVolume density;  
        //HDRP Light data
        public static HDAdditionalLightData hDAdditionalLightData;
        //HDRP Shadow data
        public static AdditionalShadowData shadowData;    
        //HDRP Camera data
        public static HDAdditionalCameraData hdCamData;        
#endif

#if HDPipeline && UNITY_2019_1_OR_NEWER
        //Ambient lighting settings
        public static StaticLightingSky bakingSkySettings;
#elif HDPipeline && !UNITY_2019_1_OR_NEWER
        //Ambient lighting settings
        public static BakingSky bakingSkySettings;
#endif

        #endregion

        #region Volume and Profile Updates

        /// <summary>
        /// Applies and creates the volume settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="renderPipelineSettings"></param>
        /// <param name="volumeName"></param>
        public static void SetupHDEnvironmentalVolume(AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, AmbientSkyProfiles skyProfiles, int profileIdx, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings, string volumeName, string hdVolumeProfile, bool updateVisualEnvironment, bool updateFog, bool updateShadows, bool updateAmbientLight, bool updateScreenReflections, bool updateScreenRefractions, bool updateSun)
        {
            //Get parent object
            if (parentObject == null)
            {
                parentObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);
            }

            //Get volume object
            if (volumeObject == null)
            {
                volumeObject = GameObject.Find(volumeName);
            }

            bool useTimeOfDay = false;
            if (skyProfiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
            {
                useTimeOfDay = true;
                if (useTimeOfDay)
                {
                    //removes warning
                }
            }

            //Apply only if system type is Ambient Skies
            if (skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.DefaultProcedural || skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                //High Definition
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    RemoveOldHDRPObjects();

                    AddRequiredComponents();

                    if (skyProfiles.m_useSkies)
                    {
#if HDPipeline && UNITY_2018_3_OR_NEWER

                        SetHDCamera();

                        //Set Sun rotation
                        SkyboxUtils.SetSkyboxRotation(skyProfiles, profile, proceduralProfile, gradientProfile, renderPipelineSettings);

                        if (volumeObject == null)
                        {
                            volumeObject = SetupEnvironmentHDRPVolumeObject(volumeName, parentObject, volumeObject);
                        }

                        if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies || skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies || skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                        {
                            if (volumeSettings == null)
                            {
                                volumeSettings = SetVolumeSetup(volumeName);
                            }

                            if (skyProfiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                            {
                                if (!string.IsNullOrEmpty(hdVolumeProfile))
                                {
                                    if (volumeProfile == null)
                                    {
                                        volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                        volumeProfile = volumeSettings.sharedProfile;
                                    }
                                    else
                                    {
                                        if (volumeSettings.sharedProfile.name != hdVolumeProfile)
                                        {
                                            volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                            volumeProfile = volumeSettings.sharedProfile;
                                        }
                                    }

                                    if (volumeProfile != null)
                                    {
                                        EditorUtility.SetDirty(volumeProfile);

                                        //Visual Enviro
                                        if (updateVisualEnvironment)
                                        {
                                            ApplyVisualEnvironment(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile, parentObject, useTimeOfDay);
                                        }

                                        //Shadows
                                        if (updateShadows)
                                        {
                                            //HD Shadows
                                            ApplyHDShadowSettings(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

                                            //Contact Shadows
                                            ApplyContactShadows(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

                                            //Micro Shadows
                                            ApplyMicroShadowing(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }

                                        //Volumetric Light
                                        if (updateFog)
                                        {
                                            ApplyVolumetricLightingController(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }

                                        //Screen Space Reflection
                                        if (updateScreenReflections)
                                        {
                                            ApplyScreenSpaceReflection(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }

                                        //Screen Space Refraction
                                        if (updateScreenRefractions)
                                        {
                                            ApplyScreenSpaceRefraction(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(hdVolumeProfile))
                                {
                                    if (volumeProfile == null)
                                    {
                                        volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                        volumeProfile = volumeSettings.sharedProfile;
                                    }
                                    else
                                    {
                                        if (volumeSettings.sharedProfile != null)
                                        {
                                            if (volumeSettings.sharedProfile.name != hdVolumeProfile)
                                            {
                                                volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                                volumeProfile = volumeSettings.sharedProfile;
                                            }
                                        }
                                        else
                                        {
                                            volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                            volumeProfile = volumeSettings.sharedProfile;
                                        }
                                    }

                                    if (volumeProfile != null)
                                    {
                                        EditorUtility.SetDirty(volumeProfile);

                                        //Visual Enviro
                                        if (updateVisualEnvironment)
                                        {
                                            if (skyProfiles.m_showFunctionDebugsOnly)
                                            {
                                                Debug.Log("Updating ApplyVisualEnvironment()");
                                            }

                                            ApplyVisualEnvironment(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile, parentObject, useTimeOfDay);
                                        }

                                        //Shadows
                                        if (updateShadows)
                                        {
                                            if (skyProfiles.m_showFunctionDebugsOnly)
                                            {
                                                Debug.Log("Updating ApplyHDShadowSettings() + ApplyContactShadows() + ApplyMicroShadowing()");
                                            }

                                            //HD Shadows
                                            ApplyHDShadowSettings(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

                                            //Contact Shadows
                                            ApplyContactShadows(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

                                            //Micro Shadows
                                            ApplyMicroShadowing(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }

                                        //Fog
                                        if (updateFog)
                                        {
                                            if (skyProfiles.m_showFunctionDebugsOnly)
                                            {
                                                Debug.Log("Updating ApplyFogSettings() + ApplyVolumetricLightingController()");
                                            }

                                            //Fog
                                            ApplyFogSettings(parentObject, volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                            //Volumetric Light
                                            ApplyVolumetricLightingController(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                            updateSun = true;
                                        }

                                        //Screen Space Reflection
                                        if (updateScreenReflections)
                                        {
                                            if (skyProfiles.m_showFunctionDebugsOnly)
                                            {
                                                Debug.Log("Updating ApplyScreenSpaceReflection()");
                                            }

                                            ApplyScreenSpaceReflection(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }

                                        //Screen Space Refraction
                                        if (updateScreenRefractions)
                                        {
                                            if (skyProfiles.m_showFunctionDebugsOnly)
                                            {
                                                Debug.Log("Updating ApplyScreenSpaceRefraction()");
                                            }

                                            ApplyScreenSpaceRefraction(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }
                                    }
                                }
                            }

                            if (updateSun)
                            {
                                if (skyProfiles.m_showFunctionDebugsOnly)
                                {
                                    Debug.Log("Updating ApplyDirectionalLightSettings()");
                                }

                                ApplyDirectionalLightSettings(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                            }
                        }
                        else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            Volume volumeSettings = volumeObject.GetComponent<Volume>();
                            if (volumeSettings != null)
                            {
                                Object.DestroyImmediate(volumeSettings);
                            }
                        }
                        else
                        {
                            if (volumeSettings == null)
                            {
                                volumeSettings = SetVolumeSetup(volumeName);
                            }

                            if (volumeSettings.sharedProfile.name != hdVolumeProfile)
                            {
                                volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                volumeProfile = volumeSettings.sharedProfile;
                            }

                            if (volumeProfile != null)
                            {
                                EditorUtility.SetDirty(volumeProfile);

                                DefaultProceduralSkySetup(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile, parentObject, volumeName, hdVolumeProfile);
                            }
                        }

                        if (updateAmbientLight)
                        {
                            if (skyProfiles.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating ApplyHDRPStaticLighting() + ApplyIndirectLightingController()");
                            }

                            ApplyHDRPStaticLighting(volumeName, hdVolumeProfile, profile, proceduralProfile, gradientProfile, skyProfiles);
                            ApplyIndirectLightingController(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                        }
#endif
                    }
                }
                else
                {
                    GameObject environmentVolume = GameObject.Find(volumeName);
                    if (environmentVolume != null)
                    {
                        Object.DestroyImmediate(environmentVolume);
                    }

                    SkyboxUtils.DestroyParent("Ambient Skies Environment");
                }
            }
            else
            {
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    //Hd Pipeline Volume Setup
                    volumeObject = GameObject.Find(volumeName);
                    if (volumeObject == null)
                    {
                        volumeObject = new GameObject(volumeName);
                        volumeObject.layer = LayerMask.NameToLayer("TransparentFX");
                        volumeObject.transform.SetParent(parentObject.transform);
                    }
                    else
                    {
                        volumeObject.layer = LayerMask.NameToLayer("TransparentFX");
                        volumeObject.transform.SetParent(parentObject.transform);
                    }
                }
                else
                {
                    //Removes the object
                    volumeObject = GameObject.Find(volumeName);
                    if (volumeObject != null)
                    {
                        Object.DestroyImmediate(volumeObject);
                    }

                    SkyboxUtils.DestroyParent("Ambient Skies Environment");
                }
            }
        }

        #region Apply Settings

        #region Default Procedural Setup

        /// <summary>
        /// Apply default sky system type
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        /// <param name="parentObject"></param>
#if HDPipeline && UNITY_2018_3_OR_NEWER
        public static void DefaultProceduralSkySetup(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, GameObject parentObject, string volumeName, string hdVolumeProfile)
        {
            //Visual Enviro
            ProceduralSkyVisualEnvironment(volumeProfile, proceduralProfile, parentObject);

            //HD Shadows
            ApplyHDShadowSettings(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Contact Shadows
            ApplyContactShadows(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Micro Shadows
            ApplyMicroShadowing(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Volumetric Light
            ApplyVolumetricLightingController(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Indirect Lighting
            ApplyIndirectLightingController(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Screen Space Reflection
            ApplyScreenSpaceReflection(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Screen Space Refraction
            ApplyScreenSpaceRefraction(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Apply Static Lighting
            ApplyHDRPStaticLighting(volumeName, hdVolumeProfile, profile, proceduralProfile, gradientProfile, skyProfiles);
        }

        /// <summary>
        /// Sets up the procedural sky for default sky type
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        /// <param name="parentObject"></param>
        public static void ProceduralSkyVisualEnvironment(VolumeProfile volumeProfile, AmbientProceduralSkyboxProfile profile, GameObject parentObject)
        {
#if HDPipeline && UNITY_2018_3_OR_NEWER
            //Visual Enviro
            VisualEnvironment visualEnvironmentSettings;
            if (volumeProfile.TryGet(out visualEnvironmentSettings))
            {
                visualEnvironmentSettings.fogType.value = FogType.Volumetric;

                ProceduralSky proceduralSky;
                if (volumeProfile.TryGet(out proceduralSky))
                {
                    proceduralSky.active = true;
                    proceduralSky.enableSunDisk.value = true;
                    proceduralSky.includeSunInBaking.value = true;
                    proceduralSky.sunSize.value = 0.015f;
                    proceduralSky.sunSizeConvergence.value = 10f;
                    proceduralSky.atmosphereThickness.value = 0.85f;
                    proceduralSky.skyTint.value = SkyboxUtils.GetColorFromHTML("B4B4B4");
                    proceduralSky.groundColor.value = SkyboxUtils.GetColorFromHTML("B5C2C7");
                    proceduralSky.exposure.value = 1f;
                    proceduralSky.multiplier.value = 1.3f;
                }

                ExponentialFog exponentialFog;
                if (volumeProfile.TryGet(out exponentialFog))
                {
                    exponentialFog.active = false;
                }

                LinearFog linearFog;
                if (volumeProfile.TryGet(out linearFog))
                {
                    linearFog.active = false;
                }

                VolumetricFog volumetricFog;
                if (volumeProfile.TryGet(out volumetricFog))
                {
                    volumetricFog.active = true;
                    volumetricFog.albedo.value = SkyboxUtils.GetColorFromHTML("D2DED9");
                    volumetricFog.color.value = profile.proceduralFogColor;
                    volumetricFog.colorMode.value = FogColorMode.SkyColor;
                    volumetricFog.meanFreePath.value = 1000f;
                    volumetricFog.baseHeight.value = 5f;
                    volumetricFog.meanHeight.value = 125f;
                    volumetricFog.anisotropy.value = 0.7f;
                    volumetricFog.globalLightProbeDimmer.value = 0.5f;
                    volumetricFog.maxFogDistance.value = 10000f;
                    volumetricFog.enableDistantFog.value = true;
                    volumetricFog.mipFogNear.value = 0f;
                    volumetricFog.mipFogFar.value = 1000f;
                    volumetricFog.mipFogMaxMip.value = 0.5f;
                }

                if (terrain == null)
                {
                    terrain = Terrain.activeTerrain;
                }

                if (densityFogVolume == null)
                {
                    densityFogVolume = GameObject.Find("Density Volume");

                    if (densityFogVolume == null)
                    {
                        densityFogVolume = new GameObject("Density Volume");
                        densityFogVolume.transform.SetParent(parentObject.transform);

                        density = densityFogVolume.AddComponent<DensityVolume>();
                        density.parameters.albedo = profile.singleScatteringAlbedo;
                        density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                        density.parameters.volumeMask = profile.fogDensityMaskTexture;
                        density.parameters.textureTiling = profile.densityMaskTiling;

                        density.parameters.size = new Vector3(10000f, 10000f, 10000f);
                    }
                }
                else
                {
                    density = densityFogVolume.GetComponent<DensityVolume>();
                    if (density == null)
                    {
                        density = densityFogVolume.AddComponent<DensityVolume>();
                    }
                    else
                    {
                        density.parameters.albedo = profile.singleScatteringAlbedo;
                        density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                        density.parameters.volumeMask = profile.fogDensityMaskTexture;
                        density.parameters.textureTiling = profile.densityMaskTiling;
                    }

                    density.parameters.size = new Vector3(10000f, 10000f, 10000f);
                }
            }
#endif
        }
#endif

        #endregion

        #region Apply Volume Settings

#if HDPipeline && UNITY_2018_3_OR_NEWER
        /// <summary>
        /// Applies the visual environent settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        /// <param name="parentObject"></param>
        public static void ApplyVisualEnvironment(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, GameObject parentObject, bool timeOfDayEnabled)
        {
            //Visual Enviro
            VisualEnvironment visualEnvironmentSettings;
            if (timeOfDayEnabled)
            {
                if (volumeProfile.TryGet(out visualEnvironmentSettings))
                {
                    visualEnvironmentSettings.skyType.value = 2;

                    ProceduralSky proceduralSky;
                    if (volumeProfile.TryGet(out proceduralSky))
                    {
                        proceduralSky.active = true;
                        proceduralSky.enableSunDisk.value = proceduralProfile.enableSunDisk;
                        proceduralSky.sunSize.value = proceduralProfile.proceduralSunSize;
                        proceduralSky.sunSizeConvergence.value = proceduralProfile.proceduralSunSizeConvergence;
                        proceduralSky.atmosphereThickness.value = proceduralProfile.proceduralAtmosphereThickness;
                        proceduralSky.skyTint.value = proceduralProfile.proceduralSkyTint;
                        proceduralSky.groundColor.value = proceduralProfile.proceduralGroundColor;
                        proceduralSky.multiplier.value = proceduralProfile.proceduralSkyMultiplier;
                    }

                    HDRISky hDRISky;
                    if (volumeProfile.TryGet(out hDRISky))
                    {
                        hDRISky.active = false;
                    }

                    GradientSky gradientSkyV;
                    if (volumeProfile.TryGet(out gradientSkyV))
                    {
                        gradientSkyV.active = false;
                    }

                    if (profile.fogType == AmbientSkiesConsts.VolumeFogType.None)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.None;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                    else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Exponential || profile.fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Exponential;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = true;
                            exponentialFog.density.value = profile.exponentialFogDensity;
                            exponentialFog.fogDistance.value = profile.fogDistance;
                            exponentialFog.fogBaseHeight.value = profile.exponentialBaseHeight;
                            exponentialFog.fogHeightAttenuation.value = profile.exponentialHeightAttenuation;
                            exponentialFog.maxFogDistance.value = profile.exponentialMaxFogDistance;
                            exponentialFog.mipFogNear.value = profile.exponentialMipFogNear;
                            exponentialFog.mipFogFar.value = profile.exponentialMipFogFar;
                            exponentialFog.mipFogMaxMip.value = profile.exponentialMipFogMaxMip;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                    else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Linear;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = true;
                            linearFog.density.value = profile.linearFogDensity;
                            linearFog.fogStart.value = profile.nearFogDistance;
                            linearFog.fogEnd.value = profile.fogDistance;
                            linearFog.fogHeightStart.value = profile.linearHeightStart;
                            linearFog.fogHeightEnd.value = profile.linearHeightEnd;
                            linearFog.maxFogDistance.value = profile.linearMaxFogDistance;
                            linearFog.mipFogNear.value = profile.linearMipFogNear;
                            linearFog.mipFogFar.value = profile.linearMipFogFar;
                            linearFog.mipFogMaxMip.value = profile.linearMipFogMaxMip;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                    else
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Volumetric;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = true;
                            volumetricFog.albedo.value = profile.fogColor;
                            volumetricFog.color.value = profile.fogColor;
                            volumetricFog.meanFreePath.value = profile.volumetricBaseFogDistance;
                            volumetricFog.baseHeight.value = profile.volumetricBaseFogHeight;
                            volumetricFog.meanHeight.value = profile.volumetricMeanHeight;
                            volumetricFog.anisotropy.value = profile.volumetricGlobalAnisotropy;
                            volumetricFog.globalLightProbeDimmer.value = profile.volumetricGlobalLightProbeDimmer;
                            volumetricFog.maxFogDistance.value = profile.fogDistance;
                            volumetricFog.enableDistantFog.value = profile.volumetricEnableDistanceFog;
                            volumetricFog.colorMode.value = profile.volumetricFogColorMode;
                            volumetricFog.mipFogNear.value = profile.nearFogDistance;
                            volumetricFog.mipFogFar.value = profile.fogDistance;
                            volumetricFog.mipFogMaxMip.value = profile.volumetricMipFogMaxMip;
                        }

                        if (profile.useFogDensityVolume)
                        {
                            Terrain terrain = Terrain.activeTerrain;
                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                            if (densityFogVolume == null)
                            {
                                densityFogVolume = new GameObject("Density Volume");
                                densityFogVolume.transform.SetParent(parentObject.transform);

                                DensityVolume density = densityFogVolume.AddComponent<DensityVolume>();
                                density.parameters.albedo = profile.singleScatteringAlbedo;
                                density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                density.parameters.textureTiling = profile.densityMaskTiling;

                                if (terrain != null)
                                {
                                    density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                }
                                else
                                {
                                    density.parameters.size = new Vector3(2000f, 400f, 2000f);
                                }
                            }
                            else
                            {
                                DensityVolume density = densityFogVolume.GetComponent<DensityVolume>();
                                if (density != null)
                                {
                                    density.parameters.albedo = profile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = profile.densityMaskTiling;
                                }

                                if (terrain != null)
                                {
                                    density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                }
                                else
                                {
                                    density.parameters.size = new Vector3(2000f, 400f, 2000f);
                                }
                            }
                        }
                        else
                        {
                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                            if (densityFogVolume != null)
                            {
                                Object.DestroyImmediate(densityFogVolume);
                            }
                        }
                    }
                }
            }
            else
            {
                if (volumeProfile.TryGet(out visualEnvironmentSettings))
                {
                    if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        visualEnvironmentSettings.skyType.value = 1;

                        HDRISky hDRISky;
                        if (volumeProfile.TryGet(out hDRISky))
                        {
                            hDRISky.active = true;
                            if (!profile.isPWProfile)
                            {
                                hDRISky.hdriSky.value = profile.customSkybox;
                            }
                            else
                            {
                                hDRISky.hdriSky.value = profile.hDRISkybox;
                            }
                            hDRISky.skyIntensityMode.value = profile.hDRISkyIntensityMode;
                            hDRISky.exposure.value = profile.skyboxExposure;
                            hDRISky.multiplier.value = profile.skyMultiplier;
                            hDRISky.rotation.value = profile.skyboxRotation;

                            if (profile.hDRIUpdateMode == AmbientSkiesConsts.EnvironementSkyUpdateMode.OnChanged)
                            {
#if !UNITY_2019_2_OR_NEWER
                                hDRISky.updateMode.value = EnvironementUpdateMode.OnChanged;
#else
                                hDRISky.updateMode.value = EnvironmentUpdateMode.OnChanged;
#endif
                            }
                            else if (profile.hDRIUpdateMode == AmbientSkiesConsts.EnvironementSkyUpdateMode.OnDemand)
                            {
#if !UNITY_2019_2_OR_NEWER
                                hDRISky.updateMode.value = EnvironementUpdateMode.OnDemand;
#else
                                hDRISky.updateMode.value = EnvironmentUpdateMode.OnDemand;
#endif
                            }
                            else
                            {
#if !UNITY_2019_2_OR_NEWER
                                hDRISky.updateMode.value = EnvironementUpdateMode.Realtime;
#else
                                hDRISky.updateMode.value = EnvironmentUpdateMode.Realtime;
#endif
                            }

                        }

                        GradientSky gradientSkyV;
                        if (volumeProfile.TryGet(out gradientSkyV))
                        {
                            gradientSkyV.active = false;
                        }

                        ProceduralSky proceduralSkyV;
                        if (volumeProfile.TryGet(out proceduralSkyV))
                        {
                            proceduralSkyV.active = false;
                        }
                    }
                    else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        visualEnvironmentSettings.skyType.value = 2;

                        HDRISky hDRISky;
                        if (volumeProfile.TryGet(out hDRISky))
                        {
                            hDRISky.active = false;
                        }

                        GradientSky gradientSkyV;
                        if (volumeProfile.TryGet(out gradientSkyV))
                        {
                            gradientSkyV.active = false;
                        }

                        ProceduralSky proceduralSky;
                        if (volumeProfile.TryGet(out proceduralSky))
                        {
                            proceduralSky.active = true;
                            if (proceduralProfile.enableSunDisk)
                            {
                                proceduralSky.enableSunDisk.value = true;
                                proceduralSky.includeSunInBaking.value = proceduralProfile.includeSunInBaking;
                            }
                            else
                            {
                                proceduralSky.enableSunDisk.value = false;
                                proceduralSky.includeSunInBaking.value = false;
                            }

                            proceduralSky.sunSize.value = proceduralProfile.proceduralSunSize;
                            proceduralSky.sunSizeConvergence.value = proceduralProfile.proceduralSunSizeConvergence;
                            proceduralSky.atmosphereThickness.value = proceduralProfile.proceduralAtmosphereThickness;
                            proceduralSky.skyTint.value = proceduralProfile.proceduralSkyTint;
                            proceduralSky.groundColor.value = proceduralProfile.proceduralGroundColor;
                            proceduralSky.exposure.value = proceduralProfile.proceduralSkyExposure;
                            proceduralSky.multiplier.value = proceduralProfile.proceduralSkyMultiplier;
                        }
                    }
                    else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                    {
                        visualEnvironmentSettings.skyType.value = 3;

                        HDRISky hDRISky;
                        if (volumeProfile.TryGet(out hDRISky))
                        {
                            hDRISky.active = false;
                        }

                        GradientSky gradientSky;
                        if (volumeProfile.TryGet(out gradientSky))
                        {
                            gradientSky.active = true;
                            gradientSky.top.value = gradientProfile.topColor;
                            gradientSky.middle.value = gradientProfile.middleColor;
                            gradientSky.bottom.value = gradientProfile.bottomColor;
                            gradientSky.gradientDiffusion.value = gradientProfile.gradientDiffusion;
                            gradientSky.exposure.value = gradientProfile.hDRIExposure;
                            gradientSky.multiplier.value = gradientProfile.skyMultiplier;
                        }

                        ProceduralSky proceduralSkyV;
                        if (volumeProfile.TryGet(out proceduralSkyV))
                        {
                            proceduralSkyV.active = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies the fog settings
        /// </summary>
        /// <param name="parentObject"></param>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        /// <param name="proceduralProfile"></param>
        /// <param name="none"></param>
        /// <param name="hdri"></param>
        /// <param name="procedural"></param>
        /// <param name="gradient"></param>
        public static void ApplyFogSettings(GameObject parentObject, VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Visual Enviro
            VisualEnvironment visualEnvironmentSettings;
            if (volumeProfile.TryGet(out visualEnvironmentSettings))
            {
                //Fog
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Exponential || profile.fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Exponential;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = true;
                            exponentialFog.colorMode.value = profile.fogColorMode;
                            exponentialFog.color.value = profile.fogColor;
                            exponentialFog.density.value = profile.exponentialFogDensity;
                            exponentialFog.fogDistance.value = profile.hDRPFogDistance;
                            exponentialFog.fogBaseHeight.value = profile.exponentialBaseHeight;
                            exponentialFog.fogHeightAttenuation.value = profile.exponentialHeightAttenuation;
                            exponentialFog.maxFogDistance.value = profile.exponentialMaxFogDistance;
                            exponentialFog.mipFogNear.value = profile.exponentialMipFogNear;
                            exponentialFog.mipFogFar.value = profile.exponentialMipFogFar;
                            exponentialFog.mipFogMaxMip.value = profile.exponentialMipFogMaxMip;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                    else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Linear;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = true;
                            linearFog.colorMode.value = profile.fogColorMode;
                            linearFog.color.value = profile.fogColor;
                            linearFog.density.value = profile.linearFogDensity;
                            linearFog.fogStart.value = profile.nearFogDistance;
                            linearFog.fogEnd.value = profile.hDRPFogDistance;
                            linearFog.fogHeightStart.value = profile.linearHeightStart;
                            linearFog.fogHeightEnd.value = profile.linearHeightEnd;
                            linearFog.maxFogDistance.value = profile.linearMaxFogDistance;
                            linearFog.mipFogNear.value = profile.linearMipFogNear;
                            linearFog.mipFogFar.value = profile.linearMipFogFar;
                            linearFog.mipFogMaxMip.value = profile.linearMipFogMaxMip;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                    else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Volumetric;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = true;
                            volumetricFog.albedo.value = profile.fogColor;
                            volumetricFog.color.value = profile.fogColor;
                            volumetricFog.colorMode.value = profile.fogColorMode;
                            volumetricFog.meanFreePath.value = profile.hDRPFogDistance;
                            volumetricFog.baseHeight.value = profile.volumetricBaseFogHeight;
                            volumetricFog.meanHeight.value = profile.volumetricMeanHeight;
                            volumetricFog.anisotropy.value = profile.volumetricGlobalAnisotropy;
                            volumetricFog.globalLightProbeDimmer.value = profile.volumetricGlobalLightProbeDimmer;
                            volumetricFog.maxFogDistance.value = profile.volumetricBaseFogDistance;
                            volumetricFog.enableDistantFog.value = profile.volumetricEnableDistanceFog;
                            volumetricFog.mipFogNear.value = profile.nearFogDistance;
                            volumetricFog.mipFogFar.value = profile.hDRPFogDistance;
                            volumetricFog.mipFogMaxMip.value = profile.volumetricMipFogMaxMip;
                        }

                        if (profile.useFogDensityVolume)
                        {
                            if (terrain == null)
                            {
                                terrain = Terrain.activeTerrain;
                            }

                            if (densityFogVolume == null)
                            {
                                densityFogVolume = GameObject.Find("Density Volume");
                                if (densityFogVolume == null)
                                {
                                    densityFogVolume = new GameObject("Density Volume");
                                    densityFogVolume.transform.SetParent(parentObject.transform);

                                    density = densityFogVolume.AddComponent<DensityVolume>();
                                    density.parameters.albedo = profile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = profile.densityMaskTiling;

                                    if (terrain != null)
                                    {
                                        density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                    }
                                    else
                                    {
                                        density.parameters.size = profile.densityScale;
                                    }
                                }
                            }


                            else
                            {
                                density = densityFogVolume.GetComponent<DensityVolume>();
                                if (density == null)
                                {
                                    density = densityFogVolume.AddComponent<DensityVolume>();
                                }
                                else
                                {
                                    density.parameters.albedo = profile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = profile.densityMaskTiling;
                                    density.parameters.size = profile.densityScale;
                                }
                            }
                        }
                        else
                        {
                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                            if (densityFogVolume != null)
                            {
                                Object.DestroyImmediate(densityFogVolume);
                            }
                        }
                    }
                    else
                    {
                        visualEnvironmentSettings.fogType.value = FogType.None;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    if (proceduralProfile.fogType == AmbientSkiesConsts.VolumeFogType.Exponential || proceduralProfile.fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Exponential;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = true;
                            exponentialFog.colorMode.value = proceduralProfile.fogColorMode;
                            exponentialFog.density.value = proceduralProfile.exponentialFogDensity;
                            exponentialFog.color.value = proceduralProfile.proceduralFogColor;
                            exponentialFog.fogDistance.value = proceduralProfile.hDRPFogDistance;
                            exponentialFog.fogBaseHeight.value = proceduralProfile.exponentialBaseHeight;
                            exponentialFog.fogHeightAttenuation.value = proceduralProfile.exponentialHeightAttenuation;
                            exponentialFog.maxFogDistance.value = proceduralProfile.exponentialMaxFogDistance;
                            exponentialFog.mipFogNear.value = proceduralProfile.exponentialMipFogNear;
                            exponentialFog.mipFogFar.value = proceduralProfile.exponentialMipFogFar;
                            exponentialFog.mipFogMaxMip.value = proceduralProfile.exponentialMipFogMaxMip;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                    else if (proceduralProfile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Linear;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = true;
                            linearFog.colorMode.value = proceduralProfile.fogColorMode;
                            linearFog.density.value = proceduralProfile.linearFogDensity;
                            linearFog.color.value = proceduralProfile.proceduralFogColor;
                            linearFog.fogStart.value = proceduralProfile.proceduralNearFogDistance;
                            linearFog.fogEnd.value = proceduralProfile.hDRPFogDistance;
                            linearFog.fogHeightStart.value = proceduralProfile.linearHeightStart;
                            linearFog.fogHeightEnd.value = proceduralProfile.linearHeightEnd;
                            linearFog.maxFogDistance.value = proceduralProfile.linearMaxFogDistance;
                            linearFog.mipFogNear.value = proceduralProfile.linearMipFogNear;
                            linearFog.mipFogFar.value = proceduralProfile.linearMipFogFar;
                            linearFog.mipFogMaxMip.value = proceduralProfile.linearMipFogMaxMip;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                    else if (proceduralProfile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Volumetric;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = true;
                            volumetricFog.colorMode.value = proceduralProfile.fogColorMode;
                            volumetricFog.albedo.value = proceduralProfile.proceduralFogColor;
                            volumetricFog.color.value = proceduralProfile.proceduralFogColor;
                            volumetricFog.meanFreePath.value = proceduralProfile.hDRPFogDistance;
                            volumetricFog.baseHeight.value = proceduralProfile.volumetricBaseFogHeight;
                            volumetricFog.meanHeight.value = proceduralProfile.volumetricMeanHeight;
                            volumetricFog.anisotropy.value = proceduralProfile.volumetricGlobalAnisotropy;
                            volumetricFog.globalLightProbeDimmer.value = proceduralProfile.volumetricGlobalLightProbeDimmer;
                            volumetricFog.maxFogDistance.value = proceduralProfile.volumetricBaseFogDistance;
                            volumetricFog.enableDistantFog.value = proceduralProfile.volumetricEnableDistanceFog;
                            volumetricFog.mipFogNear.value = proceduralProfile.proceduralNearFogDistance;
                            volumetricFog.mipFogFar.value = proceduralProfile.proceduralFogDistance;
                            volumetricFog.mipFogMaxMip.value = proceduralProfile.volumetricMipFogMaxMip;
                        }

                        if (proceduralProfile.useFogDensityVolume)
                        {
                            if (terrain != null)
                            {
                                terrain = Terrain.activeTerrain;
                            }

                            if (densityFogVolume == null)
                            {
                                densityFogVolume = GameObject.Find("Density Volume");
                                if (densityFogVolume == null)
                                {
                                    densityFogVolume = new GameObject("Density Volume");
                                    densityFogVolume.transform.SetParent(parentObject.transform);

                                    density = densityFogVolume.AddComponent<DensityVolume>();
                                    density.parameters.albedo = proceduralProfile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = proceduralProfile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = proceduralProfile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = proceduralProfile.densityMaskTiling;

                                    if (terrain != null)
                                    {
                                        density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                    }
                                    else
                                    {
                                        density.parameters.size = proceduralProfile.densityScale;
                                    }
                                }
                            }
                            else
                            {
                                if (density == null)
                                {
                                    density = densityFogVolume.GetComponent<DensityVolume>();
                                }
                                else
                                {
                                    density.parameters.albedo = proceduralProfile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = proceduralProfile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = proceduralProfile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = proceduralProfile.densityMaskTiling;
                                    density.parameters.size = proceduralProfile.densityScale;
                                }
                            }
                        }
                        else
                        {
                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                            if (densityFogVolume != null)
                            {
                                Object.DestroyImmediate(densityFogVolume);
                            }
                        }
                    }
                    else
                    {
                        visualEnvironmentSettings.fogType.value = FogType.None;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    if (gradientProfile.fogType == AmbientSkiesConsts.VolumeFogType.Exponential || gradientProfile.fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Exponential;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = true;
                            exponentialFog.colorMode.value = gradientProfile.fogColorMode;
                            exponentialFog.color.value = gradientProfile.fogColor;
                            exponentialFog.density.value = gradientProfile.exponentialFogDensity;
                            exponentialFog.fogDistance.value = gradientProfile.hDRPFogDistance;
                            exponentialFog.fogBaseHeight.value = gradientProfile.exponentialBaseHeight;
                            exponentialFog.fogHeightAttenuation.value = gradientProfile.exponentialHeightAttenuation;
                            exponentialFog.maxFogDistance.value = gradientProfile.exponentialMaxFogDistance;
                            exponentialFog.mipFogNear.value = gradientProfile.exponentialMipFogNear;
                            exponentialFog.mipFogFar.value = gradientProfile.exponentialMipFogFar;
                            exponentialFog.mipFogMaxMip.value = gradientProfile.exponentialMipFogMaxMip;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                    else if (gradientProfile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Linear;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = true;
                            linearFog.colorMode.value = gradientProfile.fogColorMode;
                            linearFog.color.value = gradientProfile.fogColor;
                            linearFog.density.value = gradientProfile.linearFogDensity;
                            linearFog.fogStart.value = gradientProfile.nearFogDistance;
                            linearFog.fogEnd.value = gradientProfile.hDRPFogDistance;
                            linearFog.fogHeightStart.value = gradientProfile.linearHeightStart;
                            linearFog.fogHeightEnd.value = gradientProfile.linearHeightEnd;
                            linearFog.maxFogDistance.value = gradientProfile.linearMaxFogDistance;
                            linearFog.mipFogNear.value = gradientProfile.linearMipFogNear;
                            linearFog.mipFogFar.value = gradientProfile.linearMipFogFar;
                            linearFog.mipFogMaxMip.value = gradientProfile.linearMipFogMaxMip;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                    else if (gradientProfile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                    {
                        visualEnvironmentSettings.fogType.value = FogType.Volumetric;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = true;
                            volumetricFog.colorMode.value = gradientProfile.fogColorMode;
                            volumetricFog.albedo.value = gradientProfile.fogColor;
                            volumetricFog.color.value = gradientProfile.fogColor;
                            volumetricFog.meanFreePath.value = gradientProfile.hDRPFogDistance;
                            volumetricFog.baseHeight.value = gradientProfile.volumetricBaseFogHeight;
                            volumetricFog.meanHeight.value = gradientProfile.volumetricMeanHeight;
                            volumetricFog.anisotropy.value = gradientProfile.volumetricGlobalAnisotropy;
                            volumetricFog.globalLightProbeDimmer.value = gradientProfile.volumetricGlobalLightProbeDimmer;
                            volumetricFog.maxFogDistance.value = gradientProfile.volumetricBaseFogDistance;
                            volumetricFog.enableDistantFog.value = gradientProfile.volumetricEnableDistanceFog;
                            volumetricFog.mipFogNear.value = gradientProfile.nearFogDistance;
                            volumetricFog.mipFogFar.value = gradientProfile.fogDistance;
                            volumetricFog.mipFogMaxMip.value = gradientProfile.volumetricMipFogMaxMip;
                        }

                        if (gradientProfile.useFogDensityVolume)
                        {
                            if (terrain != null)
                            {
                                terrain = Terrain.activeTerrain;
                            }

                            if (densityFogVolume == null)
                            {
                                densityFogVolume = GameObject.Find("Density Volume");
                            }

                            if (densityFogVolume == null)
                            {
                                densityFogVolume = new GameObject("Density Volume");
                                densityFogVolume.transform.SetParent(parentObject.transform);

                                density = densityFogVolume.AddComponent<DensityVolume>();
                                density.parameters.albedo = gradientProfile.singleScatteringAlbedo;
                                density.parameters.meanFreePath = gradientProfile.densityVolumeFogDistance;
                                density.parameters.volumeMask = gradientProfile.fogDensityMaskTexture;
                                density.parameters.textureTiling = gradientProfile.densityMaskTiling;

                                if (terrain != null)
                                {
                                    density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                }
                                else
                                {
                                    density.parameters.size = gradientProfile.densityScale;
                                }
                            }
                            else
                            {
                                if (density == null)
                                {
                                    density = densityFogVolume.GetComponent<DensityVolume>();
                                }

                                else
                                {
                                    density.parameters.albedo = gradientProfile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = gradientProfile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = gradientProfile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = gradientProfile.densityMaskTiling;
                                    density.parameters.size = gradientProfile.densityScale;
                                }
                            }
                        }
                        else
                        {
                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                            if (densityFogVolume != null)
                            {
                                Object.DestroyImmediate(densityFogVolume);
                            }
                        }
                    }
                    else
                    {
                        visualEnvironmentSettings.fogType.value = FogType.None;

                        ExponentialFog exponentialFog;
                        if (volumeProfile.TryGet(out exponentialFog))
                        {
                            exponentialFog.active = false;
                        }

                        LinearFog linearFog;
                        if (volumeProfile.TryGet(out linearFog))
                        {
                            linearFog.active = false;
                        }

                        VolumetricFog volumetricFog;
                        if (volumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                }               
            }
        }

        /// <summary>
        /// Applies the hd shadow settings settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyHDShadowSettings(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //HD Shadows
            HDShadowSettings hDShadowSettings;
            if (volumeProfile.TryGet(out hDShadowSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    hDShadowSettings.maxShadowDistance.value = profile.shadowDistance;
                    if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 1;
                    }
                    else if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 2;
                    }
                    else if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 3;
                    }
                    else
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 4;
                    }

                    hDShadowSettings.cascadeShadowSplit0.value = profile.cascadeSplit1;
                    hDShadowSettings.cascadeShadowSplit1.value = profile.cascadeSplit2;
                    hDShadowSettings.cascadeShadowSplit2.value = profile.cascadeSplit3;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    hDShadowSettings.maxShadowDistance.value = proceduralProfile.shadowDistance;
                    if (proceduralProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 1;
                    }
                    else if (proceduralProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 2;
                    }
                    else if (proceduralProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 3;
                    }
                    else
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 4;
                    }

                    hDShadowSettings.cascadeShadowSplit0.value = proceduralProfile.cascadeSplit1;
                    hDShadowSettings.cascadeShadowSplit1.value = proceduralProfile.cascadeSplit2;
                    hDShadowSettings.cascadeShadowSplit2.value = proceduralProfile.cascadeSplit3;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    hDShadowSettings.maxShadowDistance.value = gradientProfile.shadowDistance;
                    if (gradientProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 1;
                    }
                    else if (gradientProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 2;
                    }
                    else if (gradientProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 3;
                    }
                    else
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 4;
                    }

                    hDShadowSettings.cascadeShadowSplit0.value = gradientProfile.cascadeSplit1;
                    hDShadowSettings.cascadeShadowSplit1.value = gradientProfile.cascadeSplit2;
                    hDShadowSettings.cascadeShadowSplit2.value = gradientProfile.cascadeSplit3;
                }
            }
        }

        /// <summary>
        /// Applies the contact shadows settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyContactShadows(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Contact Shadows
            ContactShadows contactShadowsSettings;
            if (volumeProfile.TryGet(out contactShadowsSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    contactShadowsSettings.active = profile.useContactShadows;
                    contactShadowsSettings.length.value = profile.contactShadowsLength;
                    contactShadowsSettings.distanceScaleFactor.value = profile.contactShadowsDistanceScaleFactor;
                    contactShadowsSettings.maxDistance.value = profile.contactShadowsMaxDistance;
                    contactShadowsSettings.fadeDistance.value = profile.contactShadowsFadeDistance;
                    contactShadowsSettings.sampleCount.value = profile.contactShadowsSampleCount;
                    contactShadowsSettings.opacity.value = profile.contactShadowsOpacity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    contactShadowsSettings.active = proceduralProfile.useContactShadows;
                    contactShadowsSettings.length.value = proceduralProfile.contactShadowsLength;
                    contactShadowsSettings.distanceScaleFactor.value = proceduralProfile.contactShadowsDistanceScaleFactor;
                    contactShadowsSettings.maxDistance.value = proceduralProfile.contactShadowsMaxDistance;
                    contactShadowsSettings.fadeDistance.value = proceduralProfile.contactShadowsFadeDistance;
                    contactShadowsSettings.sampleCount.value = proceduralProfile.contactShadowsSampleCount;
                    contactShadowsSettings.opacity.value = proceduralProfile.contactShadowsOpacity;
                }
                else if(skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    contactShadowsSettings.active = gradientProfile.useContactShadows;
                    contactShadowsSettings.length.value = gradientProfile.contactShadowsLength;
                    contactShadowsSettings.distanceScaleFactor.value = gradientProfile.contactShadowsDistanceScaleFactor;
                    contactShadowsSettings.maxDistance.value = gradientProfile.contactShadowsMaxDistance;
                    contactShadowsSettings.fadeDistance.value = gradientProfile.contactShadowsFadeDistance;
                    contactShadowsSettings.sampleCount.value = gradientProfile.contactShadowsSampleCount;
                    contactShadowsSettings.opacity.value = gradientProfile.contactShadowsOpacity;
                }
            }
        }

        /// <summary>
        /// Applies the micro shadows settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyMicroShadowing(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
#if UNITY_2018_3_OR_NEWER
            //Micro Shadows
            MicroShadowing microShadowingSettings;
            if (volumeProfile.TryGet(out microShadowingSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    microShadowingSettings.active = profile.useMicroShadowing;
                    microShadowingSettings.opacity.value = profile.microShadowOpacity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    microShadowingSettings.active = proceduralProfile.useMicroShadowing;
                    microShadowingSettings.opacity.value = proceduralProfile.microShadowOpacity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    microShadowingSettings.active = gradientProfile.useMicroShadowing;
                    microShadowingSettings.opacity.value = gradientProfile.microShadowOpacity;
                }
            }
#endif
        }

        /// <summary>
        /// Applies the volumetric light controller settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyVolumetricLightingController(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Volumetric Light
            VolumetricLightingController volumetricLightingControllerSettings;
            if (volumeProfile.TryGet(out volumetricLightingControllerSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    volumetricLightingControllerSettings.depthExtent.value = profile.volumetricDistanceRange;
                    volumetricLightingControllerSettings.sliceDistributionUniformity.value = profile.volumetricSliceDistributionUniformity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    volumetricLightingControllerSettings.depthExtent.value = proceduralProfile.volumetricDistanceRange;
                    volumetricLightingControllerSettings.sliceDistributionUniformity.value = proceduralProfile.volumetricSliceDistributionUniformity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    volumetricLightingControllerSettings.depthExtent.value = gradientProfile.volumetricDistanceRange;
                    volumetricLightingControllerSettings.sliceDistributionUniformity.value = gradientProfile.volumetricSliceDistributionUniformity;
                }
            }
        }

        /// <summary>
        /// Applies the indirect light controller settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyIndirectLightingController(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Indirect Lighting
            IndirectLightingController indirectLightingControllerSettings;
            if (volumeProfile.TryGet(out indirectLightingControllerSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    indirectLightingControllerSettings.indirectDiffuseIntensity.value = profile.indirectDiffuseIntensity;
                    indirectLightingControllerSettings.indirectSpecularIntensity.value = profile.indirectSpecularIntensity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    indirectLightingControllerSettings.indirectDiffuseIntensity.value = proceduralProfile.indirectDiffuseIntensity;
                    indirectLightingControllerSettings.indirectSpecularIntensity.value = proceduralProfile.indirectSpecularIntensity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    indirectLightingControllerSettings.indirectDiffuseIntensity.value = gradientProfile.indirectDiffuseIntensity;
                    indirectLightingControllerSettings.indirectSpecularIntensity.value = gradientProfile.indirectSpecularIntensity;
                }
            }
        }

        /// <summary>
        /// Applies the screen space reflections settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyScreenSpaceReflection(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Screen Space Reflection
            ScreenSpaceReflection screenSpaceReflectionSettings;
            if (volumeProfile.TryGet(out screenSpaceReflectionSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    screenSpaceReflectionSettings.active = profile.enableScreenSpaceReflections;
                    screenSpaceReflectionSettings.screenFadeDistance.value = profile.screenEdgeFadeDistance;
                    screenSpaceReflectionSettings.rayMaxIterations.value = profile.maxNumberOfRaySteps;
                    screenSpaceReflectionSettings.depthBufferThickness.value = profile.objectThickness;
                    screenSpaceReflectionSettings.minSmoothness.value = profile.minSmoothness;
                    screenSpaceReflectionSettings.smoothnessFadeStart.value = profile.smoothnessFadeStart;
                    screenSpaceReflectionSettings.reflectSky.value = profile.reflectSky;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    screenSpaceReflectionSettings.active = proceduralProfile.enableScreenSpaceReflections;
                    screenSpaceReflectionSettings.screenFadeDistance.value = proceduralProfile.screenEdgeFadeDistance;
                    screenSpaceReflectionSettings.rayMaxIterations.value = proceduralProfile.maxNumberOfRaySteps;
                    screenSpaceReflectionSettings.depthBufferThickness.value = proceduralProfile.objectThickness;
                    screenSpaceReflectionSettings.minSmoothness.value = proceduralProfile.minSmoothness;
                    screenSpaceReflectionSettings.smoothnessFadeStart.value = proceduralProfile.smoothnessFadeStart;
                    screenSpaceReflectionSettings.reflectSky.value = proceduralProfile.reflectSky;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    screenSpaceReflectionSettings.active = gradientProfile.enableScreenSpaceReflections;
                    screenSpaceReflectionSettings.screenFadeDistance.value = gradientProfile.screenEdgeFadeDistance;
                    screenSpaceReflectionSettings.rayMaxIterations.value = gradientProfile.maxNumberOfRaySteps;
                    screenSpaceReflectionSettings.depthBufferThickness.value = gradientProfile.objectThickness;
                    screenSpaceReflectionSettings.minSmoothness.value = gradientProfile.minSmoothness;
                    screenSpaceReflectionSettings.smoothnessFadeStart.value = gradientProfile.smoothnessFadeStart;
                    screenSpaceReflectionSettings.reflectSky.value = gradientProfile.reflectSky;
                }
            }
        }

        /// <summary>
        /// Applies the screen space refractions settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyScreenSpaceRefraction(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Screen Space Refraction
            ScreenSpaceRefraction screenSpaceRefractionSettings;
            if (volumeProfile.TryGet(out screenSpaceRefractionSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    screenSpaceRefractionSettings.active = profile.enableScreenSpaceRefractions;
                    screenSpaceRefractionSettings.screenFadeDistance.value = profile.screenWeightDistance;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    screenSpaceRefractionSettings.active = proceduralProfile.enableScreenSpaceRefractions;
                    screenSpaceRefractionSettings.screenFadeDistance.value = proceduralProfile.screenWeightDistance;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    screenSpaceRefractionSettings.active = gradientProfile.enableScreenSpaceRefractions;
                    screenSpaceRefractionSettings.screenFadeDistance.value = gradientProfile.screenWeightDistance;
                }
            }
        }

        /// <summary>
        /// Sets up the Static lighting
        /// </summary>
        /// <param name="volumeObject"></param>
        /// <param name="hdVolumeProfile"></param>
        /// <param name="profile"></param>
        /// <param name="skyProfiles"></param>
        public static void ApplyHDRPStaticLighting(string volumeName, string hdVolumeProfile, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, AmbientSkyProfiles skyProfiles)
        {
            //Baking Sky Setup
#if UNITY_2019_1_OR_NEWER
            if (volumeObject == null)
            {
                volumeObject = GameObject.Find(volumeName);
            }

            if (bakingSkySettings == null)
            {
                bakingSkySettings = volumeObject.GetComponent<StaticLightingSky>();
            }

            if (profile.useBakingSky && skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies || proceduralProfile.useBakingSky && skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies || gradientProfile.useBakingSky && skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies || skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.DefaultProcedural)
            {
                if (skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                {
                    if (bakingSkySettings == null)
                    {
                        bakingSkySettings = volumeObject.AddComponent<StaticLightingSky>();
                    }

                    bakingSkySettings.profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));

                    if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        bakingSkySettings.staticLightingSkyUniqueID = 1;
                    }
                    else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies || skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.DefaultProcedural)
                    {
                        bakingSkySettings.staticLightingSkyUniqueID = 2;
                    }
                    else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                    {
                        bakingSkySettings.staticLightingSkyUniqueID = 3;
                    }
                }
                else
                {
                    if (bakingSkySettings != null)
                    {
                        Object.DestroyImmediate(bakingSkySettings);
                    }
                }
            }
            else
            {
                if (bakingSkySettings != null)
                {
                    Object.DestroyImmediate(bakingSkySettings);
                }
            }
#elif UNITY_2018_3_OR_NEWER
            if (volumeObject == null)
            {
                volumeObject = GameObject.Find(volumeName);
            }

            if (bakingSkySettings == null)
            {
            bakingSkySettings = volumeObject.GetComponent<BakingSky>();
            }
            if (profile.useBakingSky)
            {
                if (skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                {
                    if (bakingSkySettings == null)
                    {
                        bakingSkySettings = volumeObject.AddComponent<BakingSky>();
                    }

                    bakingSkySettings.profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));

                    if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        bakingSkySettings.bakingSkyUniqueID = 1;
                    }
                    else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        bakingSkySettings.bakingSkyUniqueID = 2;
                    }
                    else
                    {
                        bakingSkySettings.bakingSkyUniqueID = 3;
                    }
                }
                else
                {
                    if (bakingSkySettings != null)
                    {
                        Object.DestroyImmediate(bakingSkySettings);
                    }
                }
            }
            else
            {
                if (bakingSkySettings != null)
                {
                    Object.DestroyImmediate(bakingSkySettings);
                }
            }
#endif
        }
#endif
        #endregion

        #region Volume Setup

        /// <summary>
        /// Sets up the volume to be disabled 
        /// </summary>
        /// <param name="volumeName"></param>
        /// <param name="parentObject"></param>
        public static void DisabledHDRPSky(string volumeName, GameObject parentObject)
        {
#if HDPipeline && UNITY_2018_3_OR_NEWER
            if (volumeSettings == null)
            {
                volumeSettings = SetVolumeSetup(volumeName);
            }

            if (volumeObject == null)
            {
                volumeObject = GameObject.Find(volumeName);
                if (volumeObject == null)
                {
                    volumeObject = new GameObject(volumeName);
                    volumeObject.AddComponent<Volume>();
                    volumeObject.transform.SetParent(parentObject.transform);

                    volumeSettings.isGlobal = true;
                    volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath("Ambient Skies HD Volume Profile"));

                    volumeProfile = volumeSettings.sharedProfile;
                }
            }           
            else
            {
                if (volumeProfile == null)
                {
                    volumeProfile = volumeObject.GetComponent<Volume>().sharedProfile;
                }
            }

            if (volumeSettings != null)
            {
                EditorUtility.SetDirty(volumeSettings);

                LinearFog linearFog;
                if (volumeProfile.TryGet(out linearFog))
                {
                    linearFog.active = true;
                    linearFog.density.value = 1f;
                    linearFog.fogStart.value = 150f;
                    linearFog.fogEnd.value = 850f;
                    linearFog.fogHeightStart.value = -700f;
                    linearFog.fogHeightEnd.value = 400f;
                    linearFog.maxFogDistance.value = 5000f;
                    linearFog.mipFogNear.value = 0f;
                    linearFog.mipFogFar.value = 1000f;
                    linearFog.mipFogMaxMip.value = 0.5f;
                }

                VolumetricFog volumetricFog;
                if (volumeProfile.TryGet(out volumetricFog))
                {
                    volumetricFog.active = false;
                }

                ExponentialFog exponentialFog;
                if (volumeProfile.TryGet(out exponentialFog))
                {
                    exponentialFog.active = false;
                }

                VisualEnvironment visualEnvironment;
                if (volumeProfile.TryGet(out visualEnvironment))
                {
                    visualEnvironment.fogType.value = FogType.Linear;
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
                    proceduralSky.sunSize.value = 0.03f;
                    proceduralSky.sunSizeConvergence.value = 10f;
                }

                GradientSky gradientSky;
                if (volumeProfile.TryGet(out gradientSky))
                {
                    gradientSky.active = false;
                }
            }

            if (directionalLight == null)
            {
                directionalLight = SkyboxUtils.GetMainDirectionalLight();
            }
            else
            {
                if (sunLight == null)
                {
                    sunLight = directionalLight.GetComponent<Light>();
                    if (sunLight != null)
                    {
                        sunLight.color = SkyboxUtils.GetColorFromHTML("FFDCC5");
                        sunLight.intensity = 3.14f;
                    }

                }
                else
                {
                    sunLight.color = SkyboxUtils.GetColorFromHTML("FFDCC5");
                    sunLight.intensity = 3.14f;
                }

                if (hDAdditionalLightData == null)
                {
                    hDAdditionalLightData = directionalLight.GetComponent<HDAdditionalLightData>();
                    if (hDAdditionalLightData == null)
                    {
                        hDAdditionalLightData = directionalLight.AddComponent<HDAdditionalLightData>();
                        hDAdditionalLightData.intensity = 3.14f;
                    }
                    else
                    {
                        hDAdditionalLightData.intensity = 3.14f;
                    }
                }
                else
                {
                    hDAdditionalLightData.intensity = 3.14f;
                }
            }
#endif
        }

        /// <summary>
        /// Sets the HDRP environment volume object
        /// </summary>
        /// <param name="volumeName"></param>
        /// <param name="parentObject"></param>
        /// <param name="volumeObject"></param>
        public static GameObject SetupEnvironmentHDRPVolumeObject(string volumeName, GameObject parentObject, GameObject volumeObject)
        {
            GameObject newGameobject = volumeObject;
            //Hd Pipeline Volume Setup           
            if (newGameobject == null)
            {
                newGameobject = new GameObject(volumeName);
                newGameobject.layer = LayerMask.NameToLayer("TransparentFX");
                newGameobject.transform.SetParent(parentObject.transform);
            }

            return newGameobject;
        }

        /// <summary>
        /// Sets the volume object setup
        /// </summary>
        /// <param name="volumeSettings"></param>
        /// <param name="volumeObject"></param>
        /// <param name="profile"></param>
#if HDPipeline
        public static Volume SetVolumeSetup(string volumeName)
        {
            //Get volume object
            GameObject volumeObject = GameObject.Find(volumeName);
            Volume volumeSettings = volumeObject.GetComponent<Volume>();
            if (volumeSettings == null)
            {
                volumeSettings = volumeObject.AddComponent<Volume>();
                volumeSettings.isGlobal = true;
                volumeSettings.blendDistance = 5f;
                volumeSettings.weight = 1f;
                volumeSettings.priority = 0f;
            }
            else
            {
                volumeSettings.isGlobal = true;
                volumeSettings.blendDistance = 5f;
                volumeSettings.weight = 1f;
                volumeSettings.priority = 0f;
            }

            return volumeSettings;
        }


        /// <summary>
        /// Applies directional light settings
        /// </summary>
        /// <param name="skyProfiles"></param>
        /// <param name="profile"></param>
        public static void ApplyDirectionalLightSettings(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientPorfile)
        {
#if HDPipeline && UNITY_2018_3_OR_NEWER
            //Get the main directional light
            if (directionalLight == null)
            {
                directionalLight = SkyboxUtils.GetMainDirectionalLight();
            }

            if (directionalLight != null)
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    if (hDAdditionalLightData == null)
                    {
                        hDAdditionalLightData = directionalLight.GetComponent<HDAdditionalLightData>();
                    }

                    if (hDAdditionalLightData != null)
                    {
                        if (sunLight == null)
                        {
                            sunLight = directionalLight.GetComponent<Light>();
                            sunLight.color = profile.sunColor;
                        }
                        else
                        {
                            sunLight.color = profile.sunColor;
                        }

                        if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                        {
                            hDAdditionalLightData.useVolumetric = true;
                        }
                        else
                        {
                            hDAdditionalLightData.useVolumetric = false;
                        }
                        hDAdditionalLightData.lightUnit = LightUnit.Lux;
                        hDAdditionalLightData.intensity = profile.HDRPSunIntensity * 3.14f;
                    }

                    if (shadowData == null)
                    {
                        shadowData = Object.FindObjectOfType<AdditionalShadowData>();
                    }

                    if (shadowData != null)
                    {
                        shadowData.contactShadows = profile.useContactShadows;

                        switch (profile.shadowQuality)
                        {
                            case AmbientSkiesConsts.HDShadowQuality.Resolution64:
                                shadowData.shadowResolution = 64;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution128:
                                shadowData.shadowResolution = 128;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution256:
                                shadowData.shadowResolution = 256;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution512:
                                shadowData.shadowResolution = 512;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution1024:
                                shadowData.shadowResolution = 1024;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution2048:
                                shadowData.shadowResolution = 2048;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution4096:
                                shadowData.shadowResolution = 4096;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution8192:
                                shadowData.shadowResolution = 8192;
                                break;
                        }
                    }
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    ProceduralSky proceduralSky;
                    if (volumeProfile.TryGet(out proceduralSky))
                    {
                        proceduralSky.enableSunDisk.value = proceduralProfile.enableSunDisk;
                        proceduralSky.includeSunInBaking.value = proceduralProfile.includeSunInBaking;
                        proceduralSky.sunSize.value = proceduralProfile.hdrpProceduralSunSize;
                        proceduralSky.sunSizeConvergence.value = proceduralProfile.hdrpProceduralSunSizeConvergence;
                    }
                    if (hDAdditionalLightData == null)
                    {
                        hDAdditionalLightData = directionalLight.GetComponent<HDAdditionalLightData>();
                    }

                    if (hDAdditionalLightData != null)
                    {
                        if (sunLight == null)
                        {
                            sunLight = directionalLight.GetComponent<Light>();
                            sunLight.color = proceduralProfile.proceduralSunColor;
                        }
                        else
                        {
                            sunLight.color = proceduralProfile.proceduralSunColor;
                        }

                        if (proceduralProfile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                        {
                            hDAdditionalLightData.useVolumetric = true;
                        }
                        else
                        {
                            hDAdditionalLightData.useVolumetric = false;
                        }
                        hDAdditionalLightData.lightUnit = LightUnit.Lux;
                        hDAdditionalLightData.intensity = proceduralProfile.proceduralHDRPSunIntensity * 3.14f;
                    }

                    if (shadowData == null)
                    {
                        shadowData = Object.FindObjectOfType<AdditionalShadowData>();
                    }

                    if (shadowData != null)
                    {
                        shadowData.contactShadows = proceduralProfile.useContactShadows;

                        switch (proceduralProfile.shadowQuality)
                        {
                            case AmbientSkiesConsts.HDShadowQuality.Resolution64:
                                shadowData.shadowResolution = 64;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution128:
                                shadowData.shadowResolution = 128;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution256:
                                shadowData.shadowResolution = 256;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution512:
                                shadowData.shadowResolution = 512;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution1024:
                                shadowData.shadowResolution = 1024;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution2048:
                                shadowData.shadowResolution = 2048;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution4096:
                                shadowData.shadowResolution = 4096;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution8192:
                                shadowData.shadowResolution = 8192;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution16384:
                                shadowData.shadowResolution = 16384;
                                break;
                        }
                    }
                }
                else
                {
                    if (hDAdditionalLightData == null)
                    {
                        hDAdditionalLightData = directionalLight.GetComponent<HDAdditionalLightData>();
                    }

                    if (hDAdditionalLightData != null)
                    {
                        if (sunLight == null)
                        {
                            sunLight = directionalLight.GetComponent<Light>();
                        }

                        if (gradientPorfile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                        {
                            hDAdditionalLightData.useVolumetric = true;
                        }
                        else
                        {
                            hDAdditionalLightData.useVolumetric = false;
                        }
                        hDAdditionalLightData.lightUnit = LightUnit.Lux;
                        hDAdditionalLightData.intensity = gradientPorfile.HDRPSunIntensity * 3.14f;
                        sunLight.color = gradientPorfile.sunColor;
                    }

                    if (shadowData == null)
                    {
                        shadowData = Object.FindObjectOfType<AdditionalShadowData>();
                    }

                    if (shadowData != null)
                    {
                        shadowData.contactShadows = gradientPorfile.useContactShadows;
                        switch (gradientPorfile.shadowQuality)
                        {
                            case AmbientSkiesConsts.HDShadowQuality.Resolution64:
                                shadowData.shadowResolution = 64;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution128:
                                shadowData.shadowResolution = 128;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution256:
                                shadowData.shadowResolution = 256;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution512:
                                shadowData.shadowResolution = 512;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution1024:
                                shadowData.shadowResolution = 1024;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution2048:
                                shadowData.shadowResolution = 2048;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution4096:
                                shadowData.shadowResolution = 4096;
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution8192:
                                shadowData.shadowResolution = 8192;
                                break;
                        }
                    }
                }
            }
            #endif
        }

#endif
        #endregion

        #region HDRP Utils

        /// <summary>
        /// Adds HDRP components to light and camera if missing
        /// </summary>
        public static void AddRequiredComponents()
        {
#if HDPipeline
            if (directionalLight == null)
            {
                directionalLight = SkyboxUtils.GetMainDirectionalLight();
            }

            if (mainCamera == null)
            {
                mainCamera = SkyboxUtils.GetOrCreateMainCamera();
            }

            if (directionalLight.GetComponent<HDAdditionalLightData>() == null)
            {
                hDAdditionalLightData = directionalLight.AddComponent<HDAdditionalLightData>();
            }

            if (directionalLight.GetComponent<AdditionalShadowData>() == null)
            {
                shadowData = directionalLight.AddComponent<AdditionalShadowData>();
            }

            if (mainCamera.GetComponent<HDAdditionalCameraData>() == null)
            {
                hdCamData = mainCamera.AddComponent<HDAdditionalCameraData>();
            }
#endif
        }

        /// <summary>
        /// Fixes realtime reflection probes in hd to set the main one to baked
        /// </summary>
        public static void FixHDReflectionProbes()
        {
            ReflectionProbe[] reflectionProbes = Object.FindObjectsOfType<ReflectionProbe>();
            if (reflectionProbes != null)
            {
                foreach (ReflectionProbe probe in reflectionProbes)
                {
                    if (probe.name == "Global Reflection Probe")
                    {
                        probe.mode = ReflectionProbeMode.Baked;
                    }
                }
#if HDPipeline
                HDAdditionalReflectionData[] reflectionData = Object.FindObjectsOfType<HDAdditionalReflectionData>();
                if (reflectionData != null)
                {
                    foreach (HDAdditionalReflectionData data in reflectionData)
                    {
                        if (data.gameObject.name == "Global Reflection Probe")
                        {
#if UNITY_2019_1_OR_NEWER
                            data.mode = ProbeSettings.Mode.Baked;
#elif UNITY_2018_3_OR_NEWER
                            data.mode = ReflectionProbeMode.Baked;

#endif
                        }
                    }
                }
#endif
            }
        }

#if HDPipeline
        /// <summary>
        /// Sets up the HD Camera Data
        /// </summary>
        public static void SetHDCamera()
        {
            if (hdCamData == null)
            {
                hdCamData = Object.FindObjectOfType<HDAdditionalCameraData>();
            }
            else
            {
#if UNITY_2019_1_OR_NEWER
                hdCamData.volumeLayerMask = 2;
#endif
            }
        }
#endif

        /// <summary>
        /// Removes the old HDRP objects from the scene
        /// </summary>
        public static void RemoveOldHDRPObjects()
        {
            //Locates the old volume profine that unity creates
            GameObject oldVolumeObject = GameObject.Find("Volume Settings");
            if (oldVolumeObject != null)
            {
                //Destroys the object
                Object.DestroyImmediate(oldVolumeObject);
            }

            //Find the old post processing object that unity creates
            GameObject oldPostProcessing = GameObject.Find("Post-process Volume");
            if (oldPostProcessing != null)
            {
                //Destoys the old post processing object
                Object.DestroyImmediate(oldPostProcessing);
            }

            //Locates the old volume profine that unity creates
            GameObject old2019VolumeObject = GameObject.Find("Render Settings");
            if (old2019VolumeObject != null)
            {
                //Destroys the object
                Object.DestroyImmediate(old2019VolumeObject);
            }

            //Locates the old volume profine that unity creates
            GameObject old2019VolumeObject2 = GameObject.Find("Rendering Settings");
            if (old2019VolumeObject2 != null)
            {
                //Destroys the object
                Object.DestroyImmediate(old2019VolumeObject2);
            }

            //Find the old post processing object that unity creates
            GameObject old2019PostProcessing = GameObject.Find("Post Processing Settings");
            if (old2019PostProcessing != null)
            {
                //Destroys the object
                Object.DestroyImmediate(old2019PostProcessing);
            }

            //Find the old post processing object that unity creates
            GameObject old2019PostProcessing2 = GameObject.Find("Default Post-process");
            if (old2019PostProcessing2 != null)
            {
                //Destroys the object
                Object.DestroyImmediate(old2019PostProcessing2);
            }

            //Find the old post processing object that unity creates
            GameObject old2019PostProcessing3 = GameObject.Find("Scene Post-process");
            if (old2019PostProcessing2 != null)
            {
                //Destroys the object
                Object.DestroyImmediate(old2019PostProcessing3);
            }
        }

        #endregion

        #endregion

        #endregion

        #region Set Scripting Defines

        /// <summary>
        /// Set up the High Definition defines
        /// </summary>
        public static void SetHighDefinitionDefinesStatic()
        {
#if UNITY_2018_3_OR_NEWER
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject HDPipeline
            if (!currBuildSettings.Contains("HDPipeline"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";HDPipeline");
            }
#endif
        }

        /// <summary>
        /// Set up the Lightweight defines
        /// </summary>
        public static void SetLightweightDefinesStatic()
        {
#if UNITY_2018_3_OR_NEWER
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject LWPipeline
            if (!currBuildSettings.Contains("LWPipeline"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";LWPipeline");
            }
#endif
        }

        /// <summary>
        /// Set up the Lightweight defines
        /// </summary>
        public static void SetAmbientSkiesDefinesStatic()
        {
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject LWPipeline
            if (!currBuildSettings.Contains("AMBIENT_SKIES"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";AMBIENT_SKIES");
            }
        }

        #endregion

        #region Helper Methos

        /// <summary>
        /// Set linear deffered lighting (the best for outdoor scenes)
        /// </summary>
        public static void SetLinearDeferredLighting()
        {
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
        /// Get the currently active terrain - or any terrain
        /// </summary>
        /// <returns>A terrain if there is one</returns>
        public static Terrain[] GetActiveTerrain()
        {
            //Grab active terrain if we can
            Terrain[] terrain = Terrain.activeTerrains;
            if (terrain != null)
            {
                return terrain;
            }

            return null;
        }

        #endregion

        #endregion
    }
}