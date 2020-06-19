//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;

namespace AmbientSkies
{
    /// <summary>
    /// Horizon utility class
    /// </summary>
    public static class HorizonUtils
    {
        #region Variables 

        //Horizon Object
        public static GameObject horizonSkyObject;
        //The Horizon Material
        public static Material horizonMaterial;
        //Horizon Setting Components
        public static HorizonSky horizonSettings;

        #endregion

        #region Utils

        /// <summary>
        /// Removes the Horizon Sky
        /// </summary>
        public static void RemoveHorizonSky()
        {
            GameObject horizonSky = GameObject.Find("Ambient Skies Horizon");
            if (horizonSky != null)
            {
                Object.DestroyImmediate(horizonSky);
                SkyboxUtils.DestroyParent("Ambient Skies Environment");
            }
        }

        /// <summary>
        /// Adds Horizon Sky to the scene
        /// </summary>
        /// <param name="horizonSkyPrefab"></param>
        public static void AddHorizonSky(string horizonSkyPrefab)
        {
            string skySphere = GetAssetPath(horizonSkyPrefab);
            GameObject horizonSky = AssetDatabase.LoadAssetAtPath<GameObject>(skySphere);
            if (horizonSky.GetComponent<HorizonSky>() == null)
            {
                horizonSky.AddComponent<HorizonSky>();
            }

            //Finds and assigns the gameobject if string isn't null
            if (!string.IsNullOrEmpty(skySphere) && horizonSky != null)
            {
                GameObject theParentGo = GetOrCreateParentObject("Ambient Skies FX");
                Terrain activeTerrainInScene = Terrain.activeTerrain;
                float xFloat = 3000f;
                float yFloat = 3000f;
                float zFloat = 3000f;

                if (activeTerrainInScene != null)
                {
                    xFloat = activeTerrainInScene.terrainData.size.x;
                    yFloat = activeTerrainInScene.terrainData.size.y;
                    zFloat = activeTerrainInScene.terrainData.size.z;
                }

                //Spawn into your scene
                if (horizonSky != null && GameObject.Find("Ambient Skies Horizon") == null)
                {
                    horizonSky = Object.Instantiate(horizonSky);
                    horizonSky.name = "Ambient Skies Horizon";

                    if (activeTerrainInScene != null)
                    {
                        horizonSky.transform.localScale = new Vector3(xFloat, yFloat * 2.5f, zFloat);
                    }

                    Vector3 horizonPosition = horizonSky.transform.localPosition;
                    horizonPosition.y = -45f;
                    horizonSky.transform.localPosition = horizonPosition;

                    horizonSky.transform.SetParent(theParentGo.transform);
                }
            }
        }

        /// <summary>
        /// Sets the horizon shader material pramaters
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="enableSystem"></param>
        public static void SetHorizonShaderSettings(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, bool hasChanged)
        {
            if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                if (skyProfiles.m_useSkies)
                {
                    if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        if (profile.horizonSkyEnabled)
                        {
                            AddHorizonSky("Ambient Skies Horizon");

                            if (horizonMaterial == null)
                            {
                                horizonMaterial = GetHorizonMaterial();
                            }
                            else
                            {
                                if (hasChanged)
                                {
                                    ApplyHorizonSettings(skyProfiles, profile, proceduralProfile, horizonMaterial);
                                }
                            }
                        }
                        else
                        {
                            RemoveHorizonSky();
                        }
                    }
                    else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        if (proceduralProfile.horizonSkyEnabled)
                        {
                            AddHorizonSky("Ambient Skies Horizon");

                            if (horizonMaterial == null)
                            {
                                horizonMaterial = GetHorizonMaterial();
                            }
                            else
                            {
                                if (hasChanged)
                                {
                                    ApplyHorizonSettings(skyProfiles, profile, proceduralProfile, horizonMaterial);
                                }
                            }
                        }
                        else
                        {
                            RemoveHorizonSky();
                        }
                    }
                    else
                    {
                        if (gradientProfile.horizonSkyEnabled)
                        {
                            AddHorizonSky("Ambient Skies Horizon");

                            if (horizonMaterial == null)
                            {
                                horizonMaterial = GetHorizonMaterial();
                            }
                            else
                            {
                                ApplyHorizonSettings(skyProfiles, profile, proceduralProfile, horizonMaterial);
                            }
                        }
                        else
                        {
                            RemoveHorizonSky();
                        }
                    }
                }
                else
                {
                    RemoveHorizonSky();
                }
            }
            else
            {
                RemoveHorizonSky();
            }
        }

        /// <summary>
        /// Gets our own horizon shader if in the scene
        /// </summary>
        /// <returns>The water material if there is one</returns>
        public static Material GetHorizonMaterial()
        {
            //Grabs water material and returns
            string horizonObject = SkyboxUtils.GetAssetPath("Horizon Sky Material");
            Material horizonMaterial;
            if (!string.IsNullOrEmpty(horizonObject))
            {
                horizonMaterial = AssetDatabase.LoadAssetAtPath<Material>(horizonObject);
                if (horizonMaterial != null)
                {
                    //returns the material
                    return horizonMaterial;
                }
            }
            return null;
        }

        /// <summary>
        /// Applies the settings to the horizons material and gameobject
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="horizonMaterial"></param>
        public static void ApplyHorizonSettings(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, Material horizonMaterial)
        {
            if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                horizonMaterial.SetFloat("_Scattering", profile.horizonScattering);
                horizonMaterial.SetFloat("_FogDensity", profile.horizonFogDensity);
                horizonMaterial.SetFloat("_HorizonFalloff", profile.horizonFalloff);
                horizonMaterial.SetFloat("_HorizonBlend", profile.horizonBlend);

                if (horizonSkyObject == null)
                {
                    horizonSkyObject = GameObject.Find("Ambient Skies Horizon");

                    if (profile.scaleHorizonObjectWithFog && profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        horizonSkyObject.transform.localScale = new Vector3(profile.fogDistance, profile.fogDistance * 1.2f, profile.fogDistance);
                    }
                    else
                    {
                        horizonSkyObject.transform.localScale = profile.horizonSize;
                    }

                    if (!profile.followPlayer)
                    {
                        horizonSkyObject.transform.position = profile.horizonPosition;
                    }
                }
                else
                {
                    if (profile.scaleHorizonObjectWithFog && profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        horizonSkyObject.transform.localScale = new Vector3(profile.fogDistance, profile.fogDistance * 1.2f, profile.fogDistance);
                    }
                    else
                    {
                        horizonSkyObject.transform.localScale = profile.horizonSize;
                    }

                    if (!profile.followPlayer)
                    {
                        horizonSkyObject.transform.position = profile.horizonPosition;
                    }
                }

                if (horizonSettings == null)
                {
                    horizonSettings = Object.FindObjectOfType<HorizonSky>();
                    horizonSettings.m_followsCameraPosition = profile.followPlayer;
                    horizonSettings.m_positionUpdate = profile.horizonUpdateTime;
                    horizonSettings.UpdatePosition();
                }
                else
                {
                    horizonSettings.m_followsCameraPosition = profile.followPlayer;
                    horizonSettings.m_positionUpdate = profile.horizonUpdateTime;
                    horizonSettings.UpdatePosition();
                }
            }
            else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                horizonMaterial.SetFloat("_Scattering", proceduralProfile.horizonScattering);
                horizonMaterial.SetFloat("_FogDensity", proceduralProfile.horizonFogDensity);
                horizonMaterial.SetFloat("_HorizonFalloff", proceduralProfile.horizonFalloff);
                horizonMaterial.SetFloat("_HorizonBlend", proceduralProfile.horizonBlend);

                if (horizonSkyObject == null)
                {
                    horizonSkyObject = GameObject.Find("Ambient Skies Horizon");

                    if (proceduralProfile.scaleHorizonObjectWithFog && proceduralProfile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        horizonSkyObject.transform.localScale = new Vector3(proceduralProfile.proceduralFogDistance, proceduralProfile.proceduralFogDistance * 1.2f, proceduralProfile.proceduralFogDistance);
                    }
                    else
                    {
                        horizonSkyObject.transform.localScale = proceduralProfile.horizonSize;
                    }

                    if (!proceduralProfile.followPlayer)
                    {
                        horizonSkyObject.transform.position = proceduralProfile.horizonPosition;
                    }
                }
                else
                {
                    if (proceduralProfile.scaleHorizonObjectWithFog && proceduralProfile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        horizonSkyObject.transform.localScale = new Vector3(proceduralProfile.proceduralFogDistance, proceduralProfile.proceduralFogDistance * 1.2f, proceduralProfile.proceduralFogDistance);
                    }
                    else
                    {
                        horizonSkyObject.transform.localScale = proceduralProfile.horizonSize;
                    }

                    if (!proceduralProfile.followPlayer)
                    {
                        horizonSkyObject.transform.position = proceduralProfile.horizonPosition;
                    }
                }

                if (horizonSettings == null)
                {
                    horizonSettings = Object.FindObjectOfType<HorizonSky>();
                    horizonSettings.m_followsCameraPosition = proceduralProfile.followPlayer;
                    horizonSettings.m_positionUpdate = proceduralProfile.horizonUpdateTime;
                    horizonSettings.UpdatePosition();
                }
                else
                {
                    horizonSettings.m_followsCameraPosition = proceduralProfile.followPlayer;
                    horizonSettings.m_positionUpdate = proceduralProfile.horizonUpdateTime;
                    horizonSettings.UpdatePosition();
                }
            }
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
        /// Get or create a parent object
        /// </summary>
        /// <param name="parentGameObject">Name of the parent object to get or create</param>
        /// <returns>Parent objet</returns>
        public static GameObject GetOrCreateParentObject(string parentGameObject)
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

            return theParentGo;
        }

        /// <summary>
        /// Setup the main camera for use with HDR skyboxes
        /// </summary>
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

        #endregion
    }
}