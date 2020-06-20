//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif

namespace AmbientSkies
{
    /// <summary>
    /// A class manipulate Unity Post Processing v2.
    /// The system has a global overide capability as well;
    /// </summary>
    public static class PostProcessingUtils
    {
        #region Variables

#if UNITY_POST_PROCESSING_STACK_V2
        //Global post processing profile
        public static PostProcessProfile postProcessProfile;
        //Post processing layer component
        public static PostProcessLayer processLayer;
        //Post processing voluem component
        public static PostProcessVolume postProcessVolume;
#endif
        //The parent object
        public static GameObject theParent;
        //Camera object
        public static GameObject mainCameraObj;
        //Camera component
        public static Camera camera;
        //Post processing volume object
        public static GameObject postProcessVolumeObj;
        //HDRP Post processing volume object
        public static GameObject postVolumeObject;

#if HDPipeline && UNITY_2018_3_OR_NEWER
        //HDRP Volume settings
        public static Volume volume;
        //HDRP Volume profile settings
        public static VolumeProfile volumeProfile;
        //HDRP camera data
        public static HDAdditionalCameraData cameraData;
#endif

        #endregion

        #region Utils

        /// <summary>
        /// Returns true if post processing v2 is installed
        /// </summary>
        /// <returns>True if installed</returns>
        public static bool PostProcessingInstalled()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            return true;
#else
            return false;
#endif
        }

        #region Get/Set From Profile

        /// <summary>
        /// Get current profile index that has the profile name
        /// </summary>
        /// <param name="profiles">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromProfileName(AmbientSkyProfiles profiles, string name)
        {
            for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
            {
                if (profiles.m_ppProfiles[idx].name == name)
                {
                    return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get current profile index of currently active post processing profile
        /// </summary>
        /// <param name="profile">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromPostProcessing(AmbientSkyProfiles profiles)
        {
            #if UNITY_POST_PROCESSING_STACK_V2
            PostProcessProfile profile = GetGlobalPostProcessingProfile();
            if (profile == null)
            {
                return 0;
            }
            else
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].assetName == profile.name)
                    {
                        return idx;
                    }
                }
            }
            #endif
            return -1;
        }

        /// <summary>
        /// Get the currently active global post processing profile
        /// </summary>
        /// <returns>Currently active global post processing profile or null if there is none / its not set up properly</returns>
#if UNITY_POST_PROCESSING_STACK_V2
        public static PostProcessProfile GetGlobalPostProcessingProfile()
        {
            //Get global post processing object
            GameObject postProcessVolumeObj = GameObject.Find("Global Post Processing");
            if (postProcessVolumeObj == null)
            {
                return null;
            }

            //Get global post processing volume
            PostProcessVolume postProcessVolume = postProcessVolumeObj.GetComponent<PostProcessVolume>();
            if (postProcessVolume == null)
            {
                return null;
            }

            //Return its profile
            return postProcessVolume.sharedProfile;
    }
#endif

        /// <summary>
        /// Get a profile from the asset name
        /// </summary>
        /// <param name="profiles">List of profiles</param>
        /// <param name="profileAssetName">Asset name we are looking for</param>
        /// <returns>Profile or null if not found</returns>
        public static AmbientPostProcessingProfile GetProfileFromAssetName(AmbientSkyProfiles profiles, string profileAssetName)
        {
            if (profiles == null)
            {
                return null;
            }
            return profiles.m_ppProfiles.Find(x => x.assetName == profileAssetName);
        }

        /// <summary>
        /// Load the selected profile and apply it
        /// </summary>
        /// <param name="profile">The profiles object</param>
        /// <param name="profileName">The name of the profile to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileName(AmbientSkyProfiles profile, AmbientSkyboxProfile skyProfile, string profileName, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updateSSR, bool updateVignette, bool updateTargetPlatform)
        {
            AmbientPostProcessingProfile p = profile.m_ppProfiles.Find(x => x.name == profileName);
            if (p == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply post processing profile!");
                return;
            }
            SetPostProcessingProfile(p, profile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updateSSR, updateVignette, updateTargetPlatform);
        }

        /// <summary>
        /// Load the selected profile 
        /// </summary>
        /// <param name="profile">The profiles object</param>
        /// <param name="assetName">The name of the profile to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromAssetName(AmbientSkyProfiles profile, AmbientSkyboxProfile skyProfile, string assetName, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updateSSR, bool updateVignette, bool updateTargetPlatform)
        {
            AmbientPostProcessingProfile p = profile.m_ppProfiles.Find(x => x.assetName == assetName);
            if (p == null)
            {
                Debug.LogWarning("Invalid asset name supplied, can not apply post processing settings!");
                return;
            }
            SetPostProcessingProfile(p, profile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updateSSR, updateVignette, updateTargetPlatform);
        }

        /// <summary>
        /// Load the selected profile and apply
        /// </summary>
        /// <param name="profile">The profiles object</param>
        /// <param name="profileIndex">The zero based index to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileIndex(AmbientSkyProfiles skyProfile, AmbientPostProcessingProfile profile, int profileIndex, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updateSSR, bool updateVignette, bool updatePanini, bool updateTargetPlatform)
        {
            if (skyProfile == null)
            {
                Debug.LogError("Missing the sky profiles (m_profiles from Ambient Skies) try reopening ambient skies and try again. Exiting");
                return;
            }

            if (profile == null)
            {
                Debug.LogError("Missing the sky profiles (m_selectedPostProcessingProfile from Ambient Skies) try reopening ambient skies and try again. Exiting");
                return;
            }

            if (profileIndex < 0 || profileIndex >= skyProfile.m_ppProfiles.Count)
            {
                Debug.LogWarning("Invalid profile index selected, can not apply post processing settings!");
                return;
            }
            if (skyProfile.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                SetHDRPPostProcessingProfile(skyProfile.m_ppProfiles[profileIndex], skyProfile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updatePanini, updateVignette, updateTargetPlatform, updateSSR);
            }
            else
            {
                SetPostProcessingProfile(skyProfile.m_ppProfiles[profileIndex], skyProfile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updateSSR, updateVignette, updateTargetPlatform);
            }
        }

        #endregion

        #region Post Processing Setup

        /// <summary>
        /// Set the specified post processing profile up.
        /// </summary>
        /// <param name="profile">Profile to set up</param>
        /// <param name="useDefaults">Use defaults or current</param>
        public static void SetPostProcessingProfile(AmbientPostProcessingProfile profile, AmbientSkyProfiles skyProfile, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updateSSR, bool updateVignette, bool updateTargetPlatform)
        {
            //Clear console if required
            if (skyProfile.m_smartConsoleClean)
            {
                SkyboxUtils.ClearLog();
                Debug.Log("Console cleared successfully");
            }
#if UNITY_POST_PROCESSING_STACK_V2
            if (skyProfile.m_usePostFX)
            {
#if GAIA_PRESENT
                SkyboxUtils.SetGaiaParenting(true);
#endif
                RemoveHDRPPostProcessing(skyProfile);

                //Get the FX parent
                if (theParent == null)
                {
                    theParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);
                }

                if (mainCameraObj == null)
                {
                    mainCameraObj = SkyboxUtils.GetOrCreateMainCamera();
                }

                if (camera == null)
                {
                    camera = mainCameraObj.GetComponent<Camera>();
                }

                if (processLayer == null)
                {
                    processLayer = mainCameraObj.GetComponent<PostProcessLayer>();
                    if (processLayer == null)
                    {
                        processLayer = mainCameraObj.AddComponent<PostProcessLayer>();
                        processLayer.volumeLayer = 2;
                    }
                }
                else
                {
                    processLayer.volumeLayer = 2;
                }

                //Find or create global post processing volume object
                if (postProcessVolumeObj == null)
                {
                    postProcessVolumeObj = GameObject.Find("Global Post Processing");
                    if (postProcessVolumeObj == null)
                    {
                        postProcessVolumeObj = new GameObject("Global Post Processing");
                        postProcessVolumeObj.transform.parent = theParent.transform;
                        postProcessVolumeObj.layer = LayerMask.NameToLayer("TransparentFX");
                        postProcessVolumeObj.AddComponent<PostProcessVolume>();
                    }
                }

                //Setup the global post processing volume
                if (postProcessVolume == null)
                {
                    postProcessVolume = postProcessVolumeObj.GetComponent<PostProcessVolume>();
                    postProcessVolume.isGlobal = true;
                    postProcessVolume.priority = 0f;
                    postProcessVolume.weight = 1f;
                    postProcessVolume.blendDistance = 0f;
                }
                else
                {
                    postProcessVolume.isGlobal = true;
                    postProcessVolume.priority = 0f;
                    postProcessVolume.weight = 1f;
                    postProcessVolume.blendDistance = 0f;
                }

                if (postProcessVolume != null)
                {
                    postProcessProfile = postProcessVolume.sharedProfile;

                    bool loadProfile = false;
                    if (postProcessVolume.sharedProfile == null)
                    {
                        loadProfile = true;
                    }
                    else
                    {
                        if (postProcessVolume.sharedProfile.name != profile.assetName)
                        {
                            loadProfile = true;
                        }
                    }
                    if (loadProfile)
                    {
                        if (profile.name == "User")
                        {
                            if (profile.customPostProcessingProfile == null)
                            {
                                if (skyProfile.m_showDebug)
                                {
                                    Debug.LogError("Missing profile please add one in the post fx tab. Exiting!");
                                    return;
                                }
                            }
                            else
                            {
                                if (postProcessVolume.sharedProfile.name != profile.customPostProcessingProfile.name)
                                {
                                    postProcessVolume.sharedProfile = profile.customPostProcessingProfile;
                                    postProcessProfile = postProcessVolume.sharedProfile;
                                }
                            }
                        }
                        else
                        {
                            //Get the profile path
                            string postProcessPath = SkyboxUtils.GetAssetPath(profile.assetName);
                            if (string.IsNullOrEmpty(postProcessPath))
                            {
                                Debug.LogErrorFormat("AmbientSkies:SetPostProcessingProfile() : Unable to load '{0}' profile - Aborting!", profile.assetName);
                                return;
                            }

                            if (postProcessVolume.sharedProfile == null)
                            {
                                postProcessVolume.sharedProfile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(postProcessPath);
                                postProcessProfile = postProcessVolume.sharedProfile;
                            }
                            else
                            {
                                if (postProcessVolume.sharedProfile.name != postProcessPath)
                                {
                                    postProcessVolume.sharedProfile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(postProcessPath);
                                    postProcessProfile = postProcessVolume.sharedProfile;
                                }
                            }
                        }
                    }

                    if (postProcessProfile == null)
                    {
                        return;
                    }
                    else
                    {
                        EditorUtility.SetDirty(postProcessProfile);

                        //Get the configurable values
                        SetAntiAliasing(skyProfile, profile, mainCameraObj, camera);
#if Mewlist_Clouds
                        MassiveCloudsUtils.SetupMassiveCloudsSystem(profile);
#endif
                        if (updateAO)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetAmbientOcclusion()");
                            }

                            SetAmbientOcclusion(profile, postProcessProfile);
                        }

                        if (updateAutoExposure)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetAutoExposure()");
                            }

                            SetAutoExposure(profile, postProcessProfile);
                        }

                        if (updateBloom)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetBloom()");
                            }

                            SetBloom(profile, postProcessProfile);
                        }

                        if (updateChromatic)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetChromaticAberration()");
                            }

                            SetChromaticAberration(profile, postProcessProfile);
                        }

                        if (updateColorGrading)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetColorGrading()");
                            }

                            SetColorGrading(profile, postProcessProfile);
                        }

                        if (updateDOF)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetDepthOfField()");
                            }

                            SetDepthOfField(profile, camera, postProcessProfile);
                        }

                        if (updateGrain)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetGrain()");
                            }

                            SetGrain(profile, postProcessProfile);
                        }

                        if (updateLensDistortion)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetLensDistortion()");
                            }

                            SetLensDistortion(profile, postProcessProfile);
                        }

                        if (updateSSR)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetScreenSpaceReflections()");
                            }

                            SetScreenSpaceReflections(profile, postProcessProfile);
                        }

                        if (updateVignette)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetVignette()");
                            }

                            SetVignette(profile, postProcessProfile);
                        }

                        if (updateMotionBlur)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetMotionBlur()");
                            }

                            SetMotionBlur(profile, postProcessProfile);
                        }

                        if (updateTargetPlatform)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetTargetPlatform()");
                            }

                            SetTargetPlatform(skyProfile, postProcessProfile);
                        }

                        HidePostProcessingGizmo(profile);
                    }
                }
            }
            else
            {
                RemovePostProcessing();
                SkyboxUtils.DestroyParent("Ambient Skies Environment");
            }
#endif
        }

        /// <summary>
        /// Sets HDRP post processing 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="skyProfile"></param>
        /// <param name="useDefaults"></param>
        public static void SetHDRPPostProcessingProfile(AmbientPostProcessingProfile profile, AmbientSkyProfiles skyProfile, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updatePanini, bool updateVignette, bool updateTargetPlatform, bool updateSSR)
        {
            //Clear console if required
            if (skyProfile.m_smartConsoleClean)
            {
                SkyboxUtils.ClearLog();
                Debug.Log("Console cleared successfully");
            }
#if HDPipeline
            if (skyProfile.m_usePostFX)
            {
#if !UNITY_2019_1_OR_NEWER
                RemovePostProcessing();
                SetPostProcessingProfile(profile, skyProfile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updateSSR, updateVignette, updateTargetPlatform);
                return;
#else
                RemovePostProcessing();

                //Get the FX parent
                if (theParent == null)
                {
                    theParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);
                }

                if (mainCameraObj == null)
                {
                    mainCameraObj = SkyboxUtils.GetOrCreateMainCamera();
                }

                if (camera == null)
                {
                    camera = mainCameraObj.GetComponent<Camera>();
                }


                if (postVolumeObject == null)
                {
                    postVolumeObject = GameObject.Find("Post Processing HDRP Volume");
                    if (postVolumeObject == null)
                    {
                        postVolumeObject = new GameObject("Post Processing HDRP Volume");
                        postVolumeObject.layer = 1;
                        postVolumeObject.transform.SetParent(theParent.transform);
                    }
                }

                if (volume == null)
                {
                    volume = postVolumeObject.GetComponent<Volume>();
                    if (volume == null)
                    {
                        volume = postVolumeObject.AddComponent<Volume>();
                        volume.isGlobal = true;
                    }
                }

                bool loadProfile = false;
                if (volume.sharedProfile == null)
                {
                    loadProfile = true;
                }
                else
                {
                    if (volume.sharedProfile.name != profile.assetName + " HD")
                    {
                        loadProfile = true;
                    }
                }
                if (loadProfile)
                {
                    if (profile.name == "User")
                    {
                        if (profile.customHDRPPostProcessingprofile == null)
                        {
                            return;
                        }
                        if (volume.sharedProfile.name != profile.customHDRPPostProcessingprofile.name)
                        {
                            volume.sharedProfile = profile.customHDRPPostProcessingprofile;
                        }
                    }
                    else
                    {
                        //Get the profile path
                        string postProcessPath = profile.assetName + " HD";

                        if (string.IsNullOrEmpty(postProcessPath))
                        {
                            Debug.LogErrorFormat("AmbientSkies:SetPostProcessingProfile() : Unable to load '{0}' profile - Aborting!", profile.assetName);
                            return;
                        }

                        volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(postProcessPath));
                        volumeProfile = volume.sharedProfile;
                    }
                }

                if (volume.sharedProfile == null)
                {
                    return;
                }
                else
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    //Volume shared profile
                    if (volumeProfile == null)
                    {
                        volumeProfile = volume.sharedProfile;
                    }

                    EditorUtility.SetDirty(volumeProfile);

#if HDPipeline && UNITY_2019_1_OR_NEWER
                    //Apply settings
                    SetAntiAliasing(skyProfile, profile, mainCameraObj, camera);

                    if (profile.name != "User")
                    {
                        if (updateAO)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPAmbientOcclusion()");
                            }

                            SetHDRPAmbientOcclusion(profile, volumeProfile);
                        }

                        if (updateAutoExposure)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPAutoExposure()");
                            }

                            SetHDRPAutoExposure(profile, volumeProfile);
                        }

                        if (updateBloom)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPBloom()");
                            }

                            SetHDRPBloom(profile, volumeProfile);
                        }

                        if (updateChromatic)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPChromaticAberration()");
                            }

                            SetHDRPChromaticAberration(profile, volumeProfile);
                        }

                        if (updateColorGrading)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPColorGrading()");
                            }

                            SetHDRPColorGrading(profile, volumeProfile);
                        }

                        if (updateDOF)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPDepthOfField()");
                            }

                            SetHDRPDepthOfField(profile, volumeProfile, camera);
                        }

                        if (updateGrain)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPGrain()");
                            }

                            SetHDRPGrain(profile, volumeProfile);
                        }

                        if (updateLensDistortion)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPLensDistortion()");
                            }

                            SetHDRPLensDistortion(profile, volumeProfile);
                        }



                        if (updateVignette)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPVignette()");
                            }

                            SetHDRPVignette(profile, volumeProfile);
                        }

                        if (updateMotionBlur)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPMotionBlur()");
                            }

                            SetHDRPMotionBlur(profile, volumeProfile);
                        }

                        if (updatePanini)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPPaniniProjection()");
                            }

                            SetHDRPPaniniProjection(profile, volumeProfile);
                        }
                    }
#endif

#endif
                }
            }
            else
            {
                GameObject postVolumeObject = GameObject.Find("Post Processing HDRP Volume");
                if (postVolumeObject != null)
                {
                    Object.DestroyImmediate(postVolumeObject);
                }
#endif
                }
#if !UNITY_2019_1_OR_NEWER
            else
            {
                RemovePostProcessing();
            }
#endif

#endif
        }

        /// <summary>
        /// Remove post processing from camera and scene
        /// </summary>
        public static void RemovePostProcessing()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            //Remove from camera
            GameObject mainCameraObj = SkyboxUtils.GetOrCreateMainCamera();
            PostProcessLayer cameraProcessLayer = mainCameraObj.GetComponent<PostProcessLayer>();
            GameObject postProcessVolumeObj = GameObject.Find("Global Post Processing");

            if (cameraProcessLayer == null && postProcessVolumeObj == null)
            {
                return;
            }

            if (cameraProcessLayer != null)
            {
                Object.DestroyImmediate(cameraProcessLayer);
            }

            //Remove from scene
            if (postProcessVolumeObj != null)
            {
                Object.DestroyImmediate(postProcessVolumeObj);
            }

#if !UNITY_2019_1_OR_NEWER
            AutoDepthOfField autoFocus = Object.FindObjectOfType<AutoDepthOfField>();
            if (autoFocus != null)
            {
                Object.DestroyImmediate(autoFocus);
            }
#endif
#endif
        }

        /// <summary>
        /// Removes HDRP post processing object
        /// </summary>
        public static void RemoveHDRPPostProcessing(AmbientSkyProfiles skyProfile)
        {
            if (skyProfile.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                GameObject postProcessingHDRPObject = GameObject.Find("Post Processing HDRP Volume");
                if (postProcessingHDRPObject != null)
                {
                    Object.DestroyImmediate(postProcessingHDRPObject);
                }
            }
        }

        /// <summary>
        /// Hides the gizmo of post processing
        /// </summary>
        /// <param name="profile"></param>
        public static void HidePostProcessingGizmo(AmbientPostProcessingProfile profile)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (postProcessVolume == null)
            {
                postProcessVolume = Object.FindObjectOfType<PostProcessVolume>();
            }
            else
            {
                if (profile.hideGizmos)
                {
                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(postProcessVolume, false);
                }
                else
                {
                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(postProcessVolume, true);
                }
                
            }
#endif
        }

        /// <summary>
        /// Focuses the post processing profile
        /// </summary>
        /// <param name="profile"></param>
        public static void FocusPostProcessProfile(AmbientSkyProfiles skyProfiles, AmbientPostProcessingProfile profile)
        {
#if UNITY_2019_1_OR_NEWER
            //If HDRP
            if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline
                //Profile searched for
                VolumeProfile processProfile;

                //Get the profile path
                string postProcessPath = profile.assetName + " HD";

                string newPostProcessPath = SkyboxUtils.GetAssetPath(postProcessPath);

                if (string.IsNullOrEmpty(postProcessPath))
                {
                    Debug.LogErrorFormat("AmbientSkies:SetPostProcessingProfile() : Unable to load '{0}' profile - Aborting!", profile.assetName);
                    return;
                }
                else
                {
                    processProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(postProcessPath));
                    if (processProfile != null)
                    {
                        Selection.activeObject = processProfile;
                        ///
                        ///Caused a error using this
                        ///EditorUtility.FocusProjectWindow();
                        ///
                    }
                }
#endif
            }
            else
            {
                //Profile searched for
#if UNITY_POST_PROCESSING_STACK_V2
                PostProcessVolume volume = Object.FindObjectOfType<PostProcessVolume>();
                if (volume != null)
                {
                    PostProcessProfile processProfile;

                    processProfile = volume.sharedProfile;
                    if (processProfile != null)
                    {
                        Selection.activeObject = processProfile;
                        ///
                        ///Caused a error using this
                        ///EditorUtility.FocusProjectWindow();
                        ///
                    }
                    else
                    {
                        Debug.LogErrorFormat("Unable to focus profile, asset could not be found.");
                        return;
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Unable to find post process volume, asset can not be found as volume does not exist.");
                    return;
                }
#endif
            }

#else

            //Profile searched for

#if UNITY_POST_PROCESSING_STACK_V2
            PostProcessVolume volume = Object.FindObjectOfType<PostProcessVolume>();
            if (volume != null)
            {
                PostProcessProfile processProfile;

                processProfile = volume.sharedProfile;
                if (processProfile != null)
                {
                    Selection.activeObject = processProfile;
                    ///
                    ///Caused a error using this
                    ///EditorUtility.FocusProjectWindow();
                    ///
                }
                else
                {
                    Debug.LogErrorFormat("Unable to focus profile, asset could not be found.");
                    return;
                }
            }
            else
            {
                Debug.LogErrorFormat("Unable to find post process volume, asset can not be found as volume does not exist.");
                return;
            }
#endif

#endif
        }

#if UNITY_POST_PROCESSING_STACK_V2

        #region Apply Settings

        /// <summary>
        /// Sets camera and anti aliasing
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="mainCameraObj"></param>
        /// <param name="camera"></param>
        public static void SetAntiAliasing(AmbientSkyProfiles skyProfiles, AmbientPostProcessingProfile profile, GameObject mainCameraObj, Camera camera)
        {       
            if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline && UNITY_2019_1_OR_NEWER
                PostProcessLayer cameraProcessLayer = Object.FindObjectOfType<PostProcessLayer>();
                if (cameraProcessLayer != null)
                {
                    Object.DestroyImmediate(cameraProcessLayer);
                }

                if (cameraData == null)
                {
                    cameraData = mainCameraObj.GetComponent<HDAdditionalCameraData>();
                }
                else
                {
                    cameraData.dithering = profile.dithering;
                    switch (profile.antiAliasingMode)
                    {
                        case AmbientSkiesConsts.AntiAliasingMode.None:
                            cameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.FXAA:
                            cameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.TAA:
                            cameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.MSAA:
                            EditorUtility.DisplayDialog("Warning!", "MSAA is not supported in 2019 HDRP, switching to FXAA", "Ok");
                            profile.antiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.FXAA;
                            break;
                    }
                }
#else
                if (processLayer == null)
                {
                    processLayer = mainCameraObj.GetComponent<PostProcessLayer>();
                }
                else
                {
                    switch (profile.antiAliasingMode)
                    {
                        case AmbientSkiesConsts.AntiAliasingMode.None:
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.FXAA:
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.SMAA:
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.TAA:
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.MSAA:
                            camera.allowMSAA = true;
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                            profile.antiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.FXAA;
                            break;
                    }
                }
#endif
            }
            else
            {
                //Checks to see if the camera is there
                if (camera != null)
                {
                    if (profile.hDRMode == AmbientSkiesConsts.HDRMode.On)
                    {
                        camera.allowHDR = true;
                    }
                    else
                    {
                        camera.allowHDR = false;
                    }
                }

                //Setup the camera up to support post processing
                if (processLayer == null)
                {
                    processLayer = mainCameraObj.GetComponent<PostProcessLayer>();
                    if (processLayer == null)
                    {
                        processLayer = mainCameraObj.AddComponent<PostProcessLayer>();
                    }
                }

                processLayer.volumeTrigger = mainCameraObj.transform;
                processLayer.volumeLayer = 2;
                processLayer.fog.excludeSkybox = true;
                processLayer.fog.enabled = true;
                processLayer.stopNaNPropagation = true;

                switch (profile.antiAliasingMode)
                {
                    case AmbientSkiesConsts.AntiAliasingMode.None:
                        processLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.FXAA:
                        processLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.SMAA:
                        processLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.TAA:
                        processLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.MSAA:
                        if (camera.renderingPath == RenderingPath.DeferredShading)
                        {
                            if (EditorUtility.DisplayDialog("Warning!", "MSAA is not supported in deferred rendering path. Switching Anti Aliasing to none, please select another option that supported in deferred rendering path or set your rendering path to forward to use MSAA", "Ok"))
                            {
                                processLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                                profile.antiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.None;
                            }
                        }
                        else
                        {
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                            camera.allowMSAA = true;
                        }
                        break;
                }
            }
        }

        #region Built-In and LWRP

        /// <summary>
        /// Sets ambient occlusion settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetAmbientOcclusion(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.AmbientOcclusion ao;
            if (postProcessProfile.TryGetSettings(out ao))
            {
                ao.active = profile.aoEnabled;
                ao.intensity.value = profile.aoAmount;
                ao.color.value = profile.aoColor;
                if (!ao.color.overrideState)
                {
                    ao.color.overrideState = true;
                }
#if UNITY_POST_PROCESSING_STACK_V2
                ao.mode.value = profile.ambientOcclusionMode;
#endif
            }
        }

        /// <summary>
        /// Sets auto exposure settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetAutoExposure(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            AutoExposure autoExposure;
            if (postProcessProfile.TryGetSettings(out autoExposure))
            {
                autoExposure.active = profile.autoExposureEnabled;
                autoExposure.keyValue.value = profile.exposureAmount;
                autoExposure.minLuminance.value = profile.exposureMin;
                autoExposure.maxLuminance.value = profile.exposureMax;
            }
        }

        /// <summary>
        /// Sets bloom settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetBloom(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.Bloom bloom;
            if (postProcessProfile.TryGetSettings(out bloom))
            {
                bloom.active = profile.bloomEnabled;
                bloom.intensity.value = profile.bloomAmount;
                bloom.threshold.value = profile.bloomThreshold;
                bloom.dirtTexture.value = profile.lensTexture;
                bloom.dirtIntensity.value = profile.lensIntensity;
            }
        }

        /// <summary>
        /// Sets chromatic aberration settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetChromaticAberration(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.ChromaticAberration chromaticAberration;
            if (postProcessProfile.TryGetSettings(out chromaticAberration))
            {
                chromaticAberration.active = profile.chromaticAberrationEnabled;
                chromaticAberration.intensity.value = profile.chromaticAberrationIntensity;
            }
        }

        /// <summary>
        /// Sets color grading settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetColorGrading(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            ColorGrading colorGrading;
            if (postProcessProfile.TryGetSettings(out colorGrading))
            {
                colorGrading.active = profile.colorGradingEnabled;

                colorGrading.gradingMode.value = profile.colorGradingMode;
                colorGrading.gradingMode.overrideState = true;

                colorGrading.ldrLut.value = profile.colorGradingLut;
                colorGrading.ldrLut.overrideState = true;

                colorGrading.externalLut.value = profile.colorGradingLut;
                colorGrading.externalLut.overrideState = true;

                colorGrading.colorFilter.value = profile.colorGradingColorFilter;
                colorGrading.colorFilter.overrideState = true;
                colorGrading.postExposure.value = profile.colorGradingPostExposure;
                colorGrading.postExposure.overrideState = true;
                colorGrading.temperature.value = profile.colorGradingTempature;
                colorGrading.tint.value = profile.colorGradingTint;
                colorGrading.saturation.value = profile.colorGradingSaturation;
                colorGrading.contrast.value = profile.colorGradingContrast;

                colorGrading.mixerRedOutRedIn.overrideState = true;
                colorGrading.mixerBlueOutBlueIn.overrideState = true;
                colorGrading.mixerGreenOutGreenIn.overrideState = true;
                colorGrading.mixerRedOutRedIn.value = profile.channelMixerRed;
                colorGrading.mixerBlueOutBlueIn.value = profile.channelMixerBlue;
                colorGrading.mixerGreenOutGreenIn.value = profile.channelMixerGreen;
            }
        }

        /// <summary>
        /// Sets depth of field settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetDepthOfField(AmbientPostProcessingProfile profile, Camera camera, PostProcessProfile postProcessProfile)
        {
            if (profile.depthOfFieldEnabled)
            {
                if (profile.depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                    if (autoFocus == null)
                    {
                        autoFocus = camera.gameObject.AddComponent<AutoDepthOfField>();
                        autoFocus.m_processingProfile = profile;
                        autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                        autoFocus.m_focusOffset = profile.focusOffset;
                        autoFocus.m_targetLayer = profile.targetLayer;
                        autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                        autoFocus.m_actualFocusDistance = profile.depthOfFieldFocusDistance;
                        profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                    }
                    else
                    {
                        autoFocus.m_processingProfile = profile;
                        autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                        autoFocus.m_focusOffset = profile.focusOffset;
                        autoFocus.m_targetLayer = profile.targetLayer;
                        autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                        profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                    }
                }
                else
                {
                    AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                    if (autoFocus != null)
                    {
                        Object.DestroyImmediate(autoFocus);
                    }
                }

                UnityEngine.Rendering.PostProcessing.DepthOfField dof;
                if (postProcessProfile.TryGetSettings(out dof))
                {
                    dof.active = profile.depthOfFieldEnabled;
                    if (profile.depthOfFieldMode != AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                    {
                        dof.focusDistance.value = profile.depthOfFieldFocusDistance;
                    }
                    else
                    {
                        profile.depthOfFieldFocusDistance = dof.focusDistance.value;
                    }

                    dof.aperture.value = profile.depthOfFieldAperture;
                    dof.focalLength.value = profile.depthOfFieldFocalLength;
                    dof.kernelSize.value = profile.maxBlurSize;
                }
            }
            else
            {
                UnityEngine.Rendering.PostProcessing.DepthOfField dof;
                if (postProcessProfile.TryGetSettings(out dof))
                {
                    dof.active = profile.depthOfFieldEnabled;
                }
                AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                if (autoFocus != null)
                {
                    Object.DestroyImmediate(autoFocus);
                }
            }
        }

        /// <summary>
        /// Sets grain settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetGrain(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            Grain grain;
            if (postProcessProfile.TryGetSettings(out grain))
            {
                grain.active = profile.grainEnabled;
                grain.intensity.value = profile.grainIntensity;
                grain.size.value = profile.grainSize;
            }
        }

        /// <summary>
        /// Sets lens distortion settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetLensDistortion(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.LensDistortion lensDistortion = null;
            if (postProcessProfile.TryGetSettings(out lensDistortion))
            {
                if (profile.distortionEnabled && profile.distortionIntensity > 0f)
                {
                    lensDistortion.active = profile.distortionEnabled;
                    lensDistortion.intensity.value = profile.distortionIntensity;
                    lensDistortion.scale.value = profile.distortionScale;
                }
                else
                {
                    lensDistortion.active = false;
                }
            }
        }

        /// <summary>
        /// Sets screen space reflections settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetScreenSpaceReflections(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            ScreenSpaceReflections screenSpaceReflections = null;
            if (postProcessProfile.TryGetSettings(out screenSpaceReflections))
            {
                if (profile.screenSpaceReflectionPreset == ScreenSpaceReflectionPreset.Custom)
                {
                    screenSpaceReflections.maximumIterationCount.overrideState = true;
                    screenSpaceReflections.thickness.overrideState = true;
                    screenSpaceReflections.resolution.overrideState = true;
                }
                else
                {
                    screenSpaceReflections.maximumIterationCount.overrideState = false;
                    screenSpaceReflections.thickness.overrideState = false;
                    screenSpaceReflections.resolution.overrideState = false;
                }

                screenSpaceReflections.active = profile.screenSpaceReflectionsEnabled;
                screenSpaceReflections.maximumIterationCount.value = profile.maximumIterationCount;
                screenSpaceReflections.thickness.value = profile.thickness;
                screenSpaceReflections.resolution.value = profile.spaceReflectionResolution;
                screenSpaceReflections.preset.value = profile.screenSpaceReflectionPreset;
                screenSpaceReflections.maximumMarchDistance.value = profile.maximumMarchDistance;
                screenSpaceReflections.distanceFade.value = profile.distanceFade;
                screenSpaceReflections.vignette.value = profile.screenSpaceVignette;
            }
        }

        /// <summary>
        /// Sets vignette settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetVignette(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.Vignette vignette = null;
            if (postProcessProfile.TryGetSettings(out vignette))
            {
                if (profile.vignetteEnabled && profile.vignetteIntensity > 0f)
                {
                    vignette.active = profile.vignetteEnabled;
                    vignette.intensity.value = profile.vignetteIntensity;
                    vignette.smoothness.value = profile.vignetteSmoothness;
                }
                else
                {
                    vignette.active = false;
                }
            }
        }

        /// <summary>
        /// Sets motion blur settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetMotionBlur(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.MotionBlur motionBlur;
            if (postProcessProfile.TryGetSettings(out motionBlur))
            {
                motionBlur.active = profile.motionBlurEnabled;
                motionBlur.shutterAngle.value = profile.shutterAngle;
                motionBlur.sampleCount.value = profile.sampleCount;
            }
        }

        /// <summary>
        /// Sets target platform settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetTargetPlatform(AmbientSkyProfiles skyProfile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.AmbientOcclusion ao;
            UnityEngine.Rendering.PostProcessing.Bloom bloom;
            UnityEngine.Rendering.PostProcessing.ChromaticAberration chromaticAberration;
            if (skyProfile.m_targetPlatform == AmbientSkiesConsts.PlatformTarget.DesktopAndConsole)
            {
                if (postProcessProfile.TryGetSettings(out bloom))
                {
                    bloom.fastMode.overrideState = true;
                    bloom.fastMode.value = false;
                }

                if (postProcessProfile.TryGetSettings(out chromaticAberration))
                {
                    chromaticAberration.fastMode.overrideState = true;
                    chromaticAberration.fastMode.value = false;
                }

                if (postProcessProfile.TryGetSettings(out ao))
                {
                    ao.ambientOnly.overrideState = true;
                    ao.ambientOnly.value = true;
                }
            }
            else
            {
                if (postProcessProfile.TryGetSettings(out bloom))
                {
                    bloom.fastMode.overrideState = true;
                    bloom.fastMode.value = true;
                }

                if (postProcessProfile.TryGetSettings(out chromaticAberration))
                {
                    chromaticAberration.fastMode.overrideState = true;
                    chromaticAberration.fastMode.value = true;
                }
                if (postProcessProfile.TryGetSettings(out ao))
                {
                    ao.ambientOnly.overrideState = true;
                    ao.ambientOnly.value = false;
                }
            }
        }

        #endregion

        #region HDRP

#if HDPipeline && UNITY_2019_1_OR_NEWER

        /// <summary>
        /// Sets ambient occlusion settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetHDRPAmbientOcclusion(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.AmbientOcclusion ao;
            if (volumeProfile.TryGet(out ao))
            {
                //ao.SetAllOverridesTo(true);

                ao.active = profile.aoEnabled;
                ao.intensity.value = profile.hDRPAOIntensity;
#if !UNITY_2019_2_OR_NEWER
                ao.thicknessModifier.value = profile.hDRPAOThicknessModifier;
#else
                ao.radius.value = profile.hDRPAOThicknessModifier;
#endif
                ao.directLightingStrength.value = profile.hDRPAODirectLightingStrength;
            }
        }

        /// <summary>
        /// Sets auto exposure settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetHDRPAutoExposure(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.Exposure exposure;
            if (volumeProfile.TryGet(out exposure))
            {
                //exposure.SetAllOverridesTo(true);

                exposure.active = profile.autoExposureEnabled;
                exposure.mode.value = profile.hDRPExposureMode;
                exposure.fixedExposure.value = profile.hDRPExposureFixedExposure;
                exposure.compensation.value = profile.hDRPExposureCompensation;
                exposure.meteringMode.value = profile.hDRPExposureMeteringMode;
                exposure.luminanceSource.value = profile.hDRPExposureLuminationSource;
                exposure.curveMap.value = profile.hDRPExposureCurveMap;
                exposure.limitMin.value = profile.hDRPExposureLimitMin;
                exposure.limitMax.value = profile.hDRPExposureLimitMax;
                exposure.adaptationMode.value = profile.hDRPExposureAdaptionMode;
                exposure.adaptationSpeedDarkToLight.value = profile.hDRPExposureAdaptionSpeedDarkToLight;
                exposure.adaptationSpeedLightToDark.value = profile.hDRPExposureAdaptionSpeedLightToDark;
            }
        }

        /// <summary>
        /// Sets bloom settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetHDRPBloom(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.Bloom bloom;
            if (volumeProfile.TryGet(out bloom))
            {
                //bloom.SetAllOverridesTo(true);

                bloom.active = profile.bloomEnabled;
                bloom.intensity.value = profile.hDRPBloomIntensity;
                bloom.scatter.value = profile.hDRPBloomScatter;
                bloom.tint.value = profile.hDRPBloomTint;
                bloom.dirtTexture.value = profile.hDRPBloomDirtLensTexture;
                bloom.dirtIntensity.value = profile.hDRPBloomDirtLensIntensity;

                bloom.resolution.value = profile.hDRPBloomResolution;
                bloom.highQualityFiltering.value = profile.hDRPBloomHighQualityFiltering;
                bloom.prefilter.value = profile.hDRPBloomPrefiler;
                bloom.anamorphic.value = profile.hDRPBloomAnamorphic;
            }
        }

        /// <summary>
        /// Sets chromatic aberration settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetHDRPChromaticAberration(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.ChromaticAberration chromaticAberration;
            if (volumeProfile.TryGet(out chromaticAberration))
            {
                //chromaticAberration.SetAllOverridesTo(true);

                chromaticAberration.active = profile.chromaticAberrationEnabled;
                chromaticAberration.spectralLut.value = profile.hDRPChromaticAberrationSpectralLut;
                chromaticAberration.intensity.value = profile.hDRPChromaticAberrationIntensity;
                chromaticAberration.maxSamples.value = profile.hDRPChromaticAberrationMaxSamples;
            }
        }

        /// <summary>
        /// Sets color grading settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPColorGrading(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.ColorAdjustments colorAdjustments;
            if (volumeProfile.TryGet(out colorAdjustments))
            {
                //colorAdjustments.SetAllOverridesTo(true);

                colorAdjustments.active = profile.colorGradingEnabled;
                colorAdjustments.postExposure.value = profile.hDRPColorAdjustmentPostExposure;
                colorAdjustments.contrast.value = profile.hDRPColorAdjustmentContrast;
                colorAdjustments.colorFilter.value = profile.hDRPColorAdjustmentColorFilter;
                colorAdjustments.hueShift.value = profile.hDRPColorAdjustmentHueShift;
                colorAdjustments.saturation.value = profile.hDRPColorAdjustmentSaturation;
            }

            UnityEngine.Experimental.Rendering.HDPipeline.ChannelMixer channelMixer;
            if (volumeProfile.TryGet(out channelMixer))
            {
                //channelMixer.SetAllOverridesTo(true);

                channelMixer.active = profile.colorGradingEnabled;
                channelMixer.redOutRedIn.value = profile.hDRPChannelMixerRed;
                channelMixer.greenOutGreenIn.value = profile.hDRPChannelMixerGreen;
                channelMixer.blueOutBlueIn.value = profile.hDRPChannelMixerBlue;
            }

            UnityEngine.Experimental.Rendering.HDPipeline.ColorLookup colorLookup;
            if (volumeProfile.TryGet(out colorLookup))
            {
                //colorLookup.SetAllOverridesTo(true);

                colorLookup.active = profile.colorGradingEnabled;
                colorLookup.texture.value = profile.hDRPColorLookupTexture;
                colorLookup.contribution.value = profile.hDRPColorLookupContribution;
            }

            UnityEngine.Experimental.Rendering.HDPipeline.Tonemapping tonemapping;
            if (volumeProfile.TryGet(out tonemapping))
            {
                //tonemapping.SetAllOverridesTo(true);

                tonemapping.active = profile.colorGradingEnabled;
                tonemapping.mode.value = profile.hDRPTonemappingMode;
                tonemapping.toeStrength.value = profile.hDRPTonemappingToeStrength;
                tonemapping.toeLength.value = profile.hDRPTonemappingToeLength;
                tonemapping.shoulderStrength.value = profile.hDRPTonemappingShoulderStrength;
                tonemapping.shoulderLength.value = profile.hDRPTonemappingShoulderLength;
                tonemapping.shoulderAngle.value = profile.hDRPTonemappingShoulderAngle;
                tonemapping.gamma.value = profile.hDRPTonemappingGamma;
            }

            UnityEngine.Experimental.Rendering.HDPipeline.Tonemapping hdrpTonemapping;
            if (volumeProfile.TryGet(out hdrpTonemapping))
            {
                //hdrpTonemapping.SetAllOverridesTo(true);

                hdrpTonemapping.active = profile.colorGradingEnabled;
                hdrpTonemapping.mode.value = profile.hDRPTonemappingMode;
            }

            UnityEngine.Experimental.Rendering.HDPipeline.SplitToning splitToning;
            if (volumeProfile.TryGet(out splitToning))
            {
                //splitToning.SetAllOverridesTo(true);

                splitToning.active = profile.colorGradingEnabled;
                splitToning.shadows.value = profile.hDRPSplitToningShadows;
                splitToning.highlights.value = profile.hDRPSplitToningHighlights;
                splitToning.balance.value = profile.hDRPSplitToningBalance;
            }

            UnityEngine.Experimental.Rendering.HDPipeline.WhiteBalance whiteBalance;
            if (volumeProfile.TryGet(out whiteBalance))
            {
                //whiteBalance.SetAllOverridesTo(true);

                whiteBalance.active = profile.colorGradingEnabled;
                whiteBalance.temperature.value = profile.hDRPWhiteBalanceTempature;
                whiteBalance.tint.value = profile.hDRPWhiteBalanceTint;
            }
        }

        /// <summary>
        /// Sets depth of field settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPDepthOfField(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile, Camera camera)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.DepthOfField dof;
            if (volumeProfile.TryGet(out dof))
            {
                //dof.SetAllOverridesTo(true);

                if (profile.depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus && profile.depthOfFieldEnabled)
                {
                    AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                    if (autoFocus == null)
                    {
                        autoFocus = camera.gameObject.AddComponent<AutoDepthOfField>();
                        autoFocus.m_processingProfile = profile;
                        autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                        autoFocus.m_focusOffset = profile.focusOffset;
                        autoFocus.m_targetLayer = profile.targetLayer;
                        autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                        profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                        autoFocus.m_actualFocusDistance = profile.depthOfFieldFocusDistance;
                    }
                    else
                    {
                        autoFocus.m_processingProfile = profile;
                        autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                        autoFocus.m_focusOffset = profile.focusOffset;
                        autoFocus.m_targetLayer = profile.targetLayer;
                        profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                        autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                    }
                }
                else
                {
                    AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                    if (autoFocus != null)
                    {
                        Object.DestroyImmediate(autoFocus);
                    }
                }

                dof.active = profile.depthOfFieldEnabled;
                if (profile.depthOfFieldMode != AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    dof.focusDistance.value = profile.hDRPDepthOfFieldFocusDistance;
                    dof.focusMode.value = DepthOfFieldMode.Manual;
                }
                else
                {
                    dof.focusMode.value = DepthOfFieldMode.UsePhysicalCamera;
                }

                dof.nearFocusStart.value = profile.hDRPDepthOfFieldNearBlurStart;
                dof.nearFocusEnd.value = profile.hDRPDepthOfFieldNearBlurEnd;
                dof.nearSampleCount.value = profile.hDRPDepthOfFieldNearBlurSampleCount;
                dof.nearMaxBlur.value = profile.hDRPDepthOfFieldNearBlurMaxRadius;

                dof.farFocusStart.value = profile.hDRPDepthOfFieldFarBlurStart;
                dof.farFocusEnd.value = profile.hDRPDepthOfFieldFarBlurEnd;
                dof.farSampleCount.value = profile.hDRPDepthOfFieldFarBlurSampleCount;
                dof.farMaxBlur.value = profile.hDRPDepthOfFieldFarBlurMaxRadius;

                dof.resolution.value = profile.hDRPDepthOfFieldResolution;
                dof.highQualityFiltering.value = profile.hDRPDepthOfFieldHighQualityFiltering;
            }
        }

        /// <summary>
        /// Sets grain settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPGrain(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.FilmGrain grain;
            if (volumeProfile.TryGet(out grain))
            {
                //grain.SetAllOverridesTo(true);

                grain.active = profile.grainEnabled;
                grain.type.value = profile.hDRPFilmGrainType;
                grain.intensity.value = profile.hDRPFilmGrainIntensity;
                grain.response.value = profile.hDRPFilmGrainResponse;
            }
        }

        /// <summary>
        /// Sets lens distortion settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPLensDistortion(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.LensDistortion lensDistortion;
            if (volumeProfile.TryGet(out lensDistortion))
            {
                //lensDistortion.SetAllOverridesTo(true);

                lensDistortion.active = profile.distortionEnabled;
                lensDistortion.intensity.value = profile.hDRPLensDistortionIntensity;
                lensDistortion.xMultiplier.value = profile.hDRPLensDistortionXMultiplier;
                lensDistortion.yMultiplier.value = profile.hDRPLensDistortionYMultiplier;
                lensDistortion.center.value = profile.hDRPLensDistortionCenter;
                lensDistortion.scale.value = profile.hDRPLensDistortionScale;
            }
        }

        /// <summary>
        /// Sets motion blur settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPMotionBlur(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.MotionBlur motionBlur;
            if (volumeProfile.TryGet(out motionBlur))
            {
                //motionBlur.SetAllOverridesTo(true);

                motionBlur.active = profile.motionBlurEnabled;
                motionBlur.intensity.value = profile.hDRPMotionBlurIntensity;
                motionBlur.sampleCount.value = profile.hDRPMotionBlurSampleCount;
#if !UNITY_2019_2_OR_NEWER
                motionBlur.maxVelocity.value = profile.hDRPMotionBlurMaxVelocity;
                motionBlur.minVel.value = profile.hDRPMotionBlurMinVelocity;
#else
                motionBlur.maximumVelocity.value = profile.hDRPMotionBlurMaxVelocity;
                motionBlur.minimumVelocity.value = profile.hDRPMotionBlurMinVelocity;
#endif
                motionBlur.cameraRotationVelocityClamp.value = profile.hDRPMotionBlurCameraRotationVelocityClamp;
            }
        }

        /// <summary>
        /// Sets panini projection settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetHDRPPaniniProjection(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.PaniniProjection paniniProjection;
            if (volumeProfile.TryGet(out paniniProjection))
            {
                //paniniProjection.SetAllOverridesTo(true);

                paniniProjection.active = profile.hDRPPaniniProjectionEnabled;
                paniniProjection.distance.value = profile.hDRPPaniniProjectionDistance;
                paniniProjection.cropToFit.value = profile.hDRPPaniniProjectionCropToFit;
            }
        }

        /// <summary>
        /// Sets vignette settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPVignette(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Experimental.Rendering.HDPipeline.Vignette vignette;
            if (volumeProfile.TryGet(out vignette))
            {
                //vignette.SetAllOverridesTo(true);

                vignette.active = profile.vignetteEnabled;
                vignette.mode.value = profile.hDRPVignetteMode;
                vignette.color.value = profile.hDRPVignetteColor;
                vignette.center.value = profile.hDRPVignetteCenter;
                vignette.intensity.value = profile.hDRPVignetteIntensity;
                vignette.smoothness.value = profile.hDRPVignetteSmoothness;
                vignette.roundness.value = profile.hDRPVignetteRoundness;
                vignette.rounded.value = profile.hDRPVignetteRounded;
                vignette.mask.value = profile.hDRPVignetteMask;
                vignette.opacity.value = profile.hDRPVignetteMaskOpacity;
            }
        }

#endif

        #endregion

        #endregion

#endif

        #endregion

        #endregion
    }
}