//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
#endif
#if Mewlist_Clouds
using Mewlist;
#endif

namespace AmbientSkies
{
    public static class MassiveCloudsUtils
    {
        //Bool to enable debugging
        private static bool m_debugMode = false;

        /// <summary>
        /// Checks to see if the system is in the project
        /// </summary>
        /// <param name="fileDirectory"></param>
        public static void DefineMassiveCouds(bool fileDirectory)
        {
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (fileDirectory)
            {
                if (m_debugMode)
                {
                    Debug.Log("Massive Clouds System Found, Adding scripting define to enable it in Ambient Skies");
                }
                if (!currBuildSettings.Contains("Mewlist_Clouds"))
                {
                    if (string.IsNullOrEmpty(currBuildSettings))
                    {
                        if (m_debugMode)
                        {
                            Debug.Log("Adding Clouds System");
                        }
                        currBuildSettings = "Mewlist_Clouds";
                    }
                    else
                    {
                        if (m_debugMode)
                        {
                            Debug.Log("Adding Clouds System");
                        }
                        currBuildSettings += ";Mewlist_Clouds";
                    }
                }
            }
            else
            {
                if (currBuildSettings.Contains("Mewlist_Clouds"))
                {
                    if (m_debugMode)
                    {
                        Debug.Log("Removing Clouds System");
                    }
                    currBuildSettings = currBuildSettings.Replace("Mewlist_Clouds;", "");
                    currBuildSettings = currBuildSettings.Replace("Mewlist_Clouds", "");
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
        }

        /// <summary>
        /// Applies the settings to the system
        /// </summary>
        /// <param name="profile"></param>
        public static void SetupMassiveCloudsSystem(AmbientPostProcessingProfile profile)
        {
#if Mewlist_Clouds && UNITY_POST_PROCESSING_STACK_V2
            GameObject postProcessVolumeObj = GameObject.Find("Global Post Processing");
            PostProcessVolume postProcessVolume;
            PostProcessProfile processProfile;
            if (postProcessVolumeObj != null)
            {
                postProcessVolume = postProcessVolumeObj.GetComponent<PostProcessVolume>();
                if (postProcessVolume != null)
                {
                    processProfile = postProcessVolume.sharedProfile;
                    if (processProfile != null)
                    {
                        EditorUtility.SetDirty(processProfile);

                        if (profile.massiveCloudsEnabled)
                        {
                            MassiveCloudsEffectSettings massiveClouds;
                            if (!processProfile.TryGetSettings(out massiveClouds))
                            {
                                if (Application.isPlaying)
                                {
                                    Debug.LogWarning("Trying to add 'MassiveCloudsEffectSettings' to " + processProfile + " Unable to add the component in play mode. Please stop the application enable the cloud system then play the application");
                                    profile.massiveCloudsEnabled = false;
                                }
                                else
                                {
                                    massiveClouds = processProfile.AddSettings<MassiveCloudsEffectSettings>();

                                    if (processProfile.TryGetSettings(out massiveClouds))
                                    {
                                        if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
                                        {
                                            massiveClouds.IsHDRP.overrideState = true;
                                            massiveClouds.IsHDRP.value = true;
                                        }
                                        else
                                        {
                                            massiveClouds.IsHDRP.value = false;
                                        }

                                        massiveClouds.active = true;
                                        massiveClouds.BaseColor.overrideState = true;
                                        massiveClouds.FogColor.overrideState = true;
                                        massiveClouds.Profile.overrideState = true;
                                        massiveClouds.SynchronizeBaseColorWithFogColor.overrideState = true;
                                        massiveClouds.SynchronizeGlobalFogColor.overrideState = true;

                                        massiveClouds.SynchronizeBaseColorWithFogColor.value = profile.syncBaseFogColor;
                                        massiveClouds.SynchronizeGlobalFogColor.value = profile.syncGlobalFogColor;
                                        massiveClouds.FogColor.value = profile.cloudsFogColor;
                                        massiveClouds.BaseColor.value = profile.cloudsBaseFogColor;
                                        if (massiveClouds.Profile.value == null)
                                        {
                                            massiveClouds.Profile.value = AssetDatabase.LoadAssetAtPath<MassiveCloudsProfile>(SkyboxUtils.GetAssetPath("SolidRich"));
                                            profile.cloudProfile = massiveClouds.Profile.value;
                                        }
                                        else
                                        {
                                            massiveClouds.Profile.value = profile.cloudProfile;
                                        }                        
                                    }                     
                                }
                            }
                            else
                            {
                                if (processProfile.TryGetSettings(out massiveClouds))
                                {
                                    massiveClouds.active = true;
                                    massiveClouds.BaseColor.overrideState = true;
                                    massiveClouds.FogColor.overrideState = true;
                                    massiveClouds.Profile.overrideState = true;
                                    massiveClouds.SynchronizeBaseColorWithFogColor.overrideState = true;
                                    massiveClouds.SynchronizeGlobalFogColor.overrideState = true;

                                    massiveClouds.SynchronizeBaseColorWithFogColor.value = profile.syncBaseFogColor;
                                    massiveClouds.SynchronizeGlobalFogColor.value = profile.syncGlobalFogColor;
                                    massiveClouds.FogColor.value = profile.cloudsFogColor;
                                    massiveClouds.BaseColor.value = profile.cloudsBaseFogColor;
                                    massiveClouds.Profile.value = profile.cloudProfile;
                                }
                            }
                        }
                        else
                        {
                            MassiveCloudsEffectSettings massiveClouds;
                            if (Application.isPlaying)
                            {
                                if (processProfile.TryGetSettings(out massiveClouds))
                                {
                                    massiveClouds.active = false;
                                }
                            }
                            else
                            {
                                if (processProfile.TryGetSettings(out massiveClouds))
                                {
                                    processProfile.RemoveSettings<MassiveCloudsEffectSettings>();
                                }
                            }
                        }
                    }
                }
            }
#endif
        }
    }
}