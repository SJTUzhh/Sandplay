//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif
using System.Collections.Generic;

namespace AmbientSkies
{
    public static class ReflectionProbeUtils
    {
        #region Variables

        //Stored list of reflection probes
        public static List<ReflectionProbe> m_storedProbes = new List<ReflectionProbe>();
        //Current created count
        public static int m_currentProbeCount;
        //If the probe system is active to begin baking the probes
        public static bool m_probeRenderActive = false;

        #endregion

        #region Reflection Probe Creation Button Functions

        /// <summary>
        /// Creates a reflection probe at current scene view camera location
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="renderPipelineSettings"></param>
        public static void CreateProbeAtLocation(AmbientLightingProfile profile, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
            GameObject oldReflectionProbe = GameObject.Find("Global Reflection Probe");
            if (oldReflectionProbe != null)
            {
                Object.DestroyImmediate(oldReflectionProbe);
                Debug.Log("Old Reflection Probe Destroyed");
            }

            //Creates the probe
            GameObject createdProbeObject = new GameObject(profile.reflectionProbeName);

            //Assign scene view camera
            Vector3 position = SceneView.lastActiveSceneView.camera.transform.position;
            if (SceneView.lastActiveSceneView.pivot != position)
            {
                //Set position based on scene view camera position
                createdProbeObject.transform.localPosition = position;
            }
            else
            {
                Debug.LogWarning("Scene View camera not found, probe spawned at position 0, 0, 0");
            }

            //Parent the probe object 
            GameObject reflectionProbeParent = ReflectionProbeParenting(true);

            createdProbeObject.transform.SetParent(reflectionProbeParent.transform);

            //Add probe data
            ReflectionProbe probeData = createdProbeObject.AddComponent<ReflectionProbe>();

            //Configure Probe settings
            probeData.blendDistance = profile.reflectionProbeBlendDistance;
            probeData.cullingMask = profile.reflectionprobeCullingMask;
            probeData.farClipPlane = profile.reflectionProbeClipPlaneDistance;
            probeData.mode = profile.reflectionProbeMode;
            probeData.refreshMode = profile.reflectionProbeRefresh;
            probeData.blendDistance = 5f;

            switch (profile.reflectionProbeResolution)
            {
                case AmbientSkiesConsts.ReflectionProbeResolution.Resolution16:
                    probeData.resolution = 16;
                    break;
                case AmbientSkiesConsts.ReflectionProbeResolution.Resolution32:
                    probeData.resolution = 32;
                    break;
                case AmbientSkiesConsts.ReflectionProbeResolution.Resolution64:
                    probeData.resolution = 64;
                    break;
                case AmbientSkiesConsts.ReflectionProbeResolution.Resolution128:
                    probeData.resolution = 128;
                    break;
                case AmbientSkiesConsts.ReflectionProbeResolution.Resolution256:
                    probeData.resolution = 256;
                    break;
                case AmbientSkiesConsts.ReflectionProbeResolution.Resolution512:
                    probeData.resolution = 512;
                    break;
                case AmbientSkiesConsts.ReflectionProbeResolution.Resolution1024:
                    probeData.resolution = 1024;
                    break;
                case AmbientSkiesConsts.ReflectionProbeResolution.Resolution2048:
                    probeData.resolution = 2048;
                    break;
            }

            probeData.shadowDistance = profile.reflectionProbeShadowDistance;
            probeData.size = profile.reflectionProbeScale;
            probeData.timeSlicingMode = profile.reflectionProbeTimeSlicingMode;
            probeData.hdr = true;
            probeData.shadowDistance = profile.reflectionProbeShadowDistance;
            
            //If HDRP
            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline && UNITY_2018_3_OR_NEWER
                HDAdditionalReflectionData reflectionData = createdProbeObject.GetComponent<HDAdditionalReflectionData>();
                if (reflectionData == null)
                {
                    reflectionData = createdProbeObject.AddComponent<HDAdditionalReflectionData>();
                    reflectionData.multiplier = 1f;
                }
                else
                {
                    reflectionData.multiplier = 1f;
                }
#endif
            }

            m_currentProbeCount++;

            SceneView.lastActiveSceneView.Repaint();
            EditorGUIUtility.PingObject(createdProbeObject);
        }

        /// <summary>
        /// Setup for the automatically probe spawning
        /// </summary>
        /// <param name="profile"></param>
        public static void CreateAutomaticProbes(AmbientLightingProfile profile, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
            GameObject oldReflectionProbe = GameObject.Find("Global Reflection Probe");
            if (oldReflectionProbe != null)
            {
                Object.DestroyImmediate(oldReflectionProbe);
                Debug.Log("Old Reflection Probe Destroyed");
            }

            GameObject relfectionParentObject = ReflectionProbeParenting(true);
            int numberTerrains = Terrain.activeTerrains.Length;

            if (numberTerrains == 0)
            {
                Debug.LogError("Unable to initiate probe spawning systen. No terrain found");
                return;
            }
            else
            {
                if (profile.reflectionProbesPerRow < 2)
                {
                    Debug.LogError("Please set Probes Per Row to a value of 2 or higher");
                    return;
                }
                else
                {
                    m_currentProbeCount = 0;

                    float seaLevel = 0f;
                    bool seaLevelActive = false;

#if GAIA_PRESENT

                    Gaia.GaiaSessionManager gaiaSession = Object.FindObjectOfType<Gaia.GaiaSessionManager>();
                    if (gaiaSession != null)
                    {
                        seaLevel = profile.seaLevel;
                        seaLevelActive = true;
                    }
#else

                    seaLevel = profile.seaLevel;

#endif

                    for (int terrainIdx = 0; terrainIdx < numberTerrains; terrainIdx++)
                    {
                        Terrain terrain = Terrain.activeTerrains[terrainIdx];
                        Vector3 terrainSize = terrain.terrainData.size;

                        for (int row = 0; row < profile.reflectionProbesPerRow; ++row)
                        {
                            for (int columns = 0; columns < profile.reflectionProbesPerRow; ++columns)
                            {
                                GameObject probeObject = new GameObject("Global Generated Reflection Probe");
                                Vector3 newPosition = probeObject.transform.position;
                                newPosition.x = ((columns + 1) * terrainSize.x / profile.reflectionProbesPerRow) - terrainSize.x / profile.reflectionProbesPerRow / 2f + terrain.transform.position.x;
                                newPosition.z = ((row + 1) * terrainSize.z / profile.reflectionProbesPerRow) - terrainSize.z / profile.reflectionProbesPerRow / 2f + terrain.transform.position.z;
                                float sampledHeight = terrain.SampleHeight(newPosition);

                                ReflectionProbe probeData = probeObject.AddComponent<ReflectionProbe>();
                                probeData.enabled = false;
                                probeData.blendDistance = 0f;
                                probeData.cullingMask = profile.reflectionprobeCullingMask;
                                probeData.farClipPlane = profile.reflectionProbeClipPlaneDistance;
                                probeData.mode = profile.reflectionProbeMode;
                                probeData.refreshMode = profile.reflectionProbeRefresh;

                                if (seaLevelActive)
                                {
                                    newPosition.y = 500f + seaLevel + 0.2f;
                                }
                                else
                                {
                                    newPosition.y = sampledHeight + profile.reflectionProbeOffset;
                                    probeData.center = new Vector3(0f, 0f - profile.reflectionProbeOffset - sampledHeight, 0f);
                                }
                                

                                probeObject.transform.position = newPosition;
                                probeObject.transform.SetParent(relfectionParentObject.transform);

                                switch (profile.reflectionProbeResolution)
                                {
                                    case AmbientSkiesConsts.ReflectionProbeResolution.Resolution16:
                                        probeData.resolution = 16;
                                        break;
                                    case AmbientSkiesConsts.ReflectionProbeResolution.Resolution32:
                                        probeData.resolution = 32;
                                        break;
                                    case AmbientSkiesConsts.ReflectionProbeResolution.Resolution64:
                                        probeData.resolution = 64;
                                        break;
                                    case AmbientSkiesConsts.ReflectionProbeResolution.Resolution128:
                                        probeData.resolution = 128;
                                        break;
                                    case AmbientSkiesConsts.ReflectionProbeResolution.Resolution256:
                                        probeData.resolution = 256;
                                        break;
                                    case AmbientSkiesConsts.ReflectionProbeResolution.Resolution512:
                                        probeData.resolution = 512;
                                        break;
                                    case AmbientSkiesConsts.ReflectionProbeResolution.Resolution1024:
                                        probeData.resolution = 1024;
                                        break;
                                    case AmbientSkiesConsts.ReflectionProbeResolution.Resolution2048:
                                        probeData.resolution = 2048;
                                        break;
                                }

                                probeData.shadowDistance = 80f;
                                probeData.size = new Vector3(terrainSize.x / profile.reflectionProbesPerRow, terrainSize.y, terrainSize.z / profile.reflectionProbesPerRow);
                                probeData.timeSlicingMode = profile.reflectionProbeTimeSlicingMode;
                                probeData.hdr = true;
                                probeData.shadowDistance = profile.reflectionProbeShadowDistance;

                                //If HDRP
                                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                                {
#if HDPipeline && UNITY_2018_3_OR_NEWER
                                    HDAdditionalReflectionData reflectionData = probeObject.GetComponent<HDAdditionalReflectionData>();
                                    if (reflectionData == null)
                                    {
                                        reflectionData = probeObject.AddComponent<HDAdditionalReflectionData>();
                                        reflectionData.multiplier = 1f;
                                    }
                                    else
                                    {
                                        reflectionData.multiplier = 1f;
                                    }
#endif
                                }

                                m_storedProbes.Add(probeData);
                                m_currentProbeCount++;
                            }
                        }
                    }

#if HDPipeline && GAIA_PRESENT
                    if (seaLevelActive)
                    {
                        GameObject planarObject = GameObject.Find("Gaia Water Planar Reflections Object");
                        if (planarObject == null)
                        {
                            planarObject = new GameObject("Gaia Water Planar Reflections Object");
                            PlanarReflectionProbe planar = planarObject.AddComponent<PlanarReflectionProbe>();
                            planar.mode = ProbeSettings.Mode.Realtime;
                            planar.realtimeMode = ProbeSettings.RealtimeMode.OnEnable;
                            planar.influenceVolume.shape = InfluenceShape.Box;
                            planar.influenceVolume.boxSize = new Vector3(10000f, 5f, 10000f);

                            planarObject.transform.position = new Vector3(0f, profile.seaLevel + 0.2f, 0f);
                            planarObject.transform.SetParent(relfectionParentObject.transform);
                        }
                    }
#endif

                    m_probeRenderActive = true;
                }
            }
        }

        #endregion

        #region Reflection Probe Utils

        /// <summary>
        /// Creates the reflection prrobe parent object
        /// </summary>
        /// <param name="parentWithAmbientSkies"></param>
        /// <returns></returns>
        private static GameObject ReflectionProbeParenting(bool parentWithAmbientSkies)
        {
            GameObject reflectionProbeParent = GameObject.Find("Scene Reflection Probes");
            GameObject ambientSkiesParent = GameObject.Find("Ambient Skies Environment");
            if (reflectionProbeParent == null)
            {
                reflectionProbeParent = new GameObject("Scene Reflection Probes");

                if (parentWithAmbientSkies)
                {
                    if (ambientSkiesParent == null)
                    {
                        ambientSkiesParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);
                        reflectionProbeParent.transform.SetParent(ambientSkiesParent.transform);
                    }
                    else
                    {
                        reflectionProbeParent.transform.SetParent(ambientSkiesParent.transform);
                    }
                }

                return reflectionProbeParent;
            }
            else
            {
                return reflectionProbeParent;
            }
        }

        /// <summary>
        /// Clears reflection probes created
        /// </summary>
        public static void ClearCreatedReflectionProbes()
        {
            GameObject oldReflectionProbe = GameObject.Find("Global Reflection Probe");
            if (oldReflectionProbe != null)
            {
                Object.DestroyImmediate(oldReflectionProbe);
                Debug.Log("Old Reflection Probe Destroyed");
            }

            GameObject reflectionProbeParent = GameObject.Find("Scene Reflection Probes");
            if (reflectionProbeParent == null)
            {
                Debug.Log("No Reflection Probes have been created");
            }
            else
            {
                Object.DestroyImmediate(reflectionProbeParent);
                m_currentProbeCount = 0;
            }
        }

        #endregion
    }
}