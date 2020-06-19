//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmbientSkies
{
    public static class LightProbeUtils
    {
        #region Variables

        //Probe locaions 
        private static Quadtree<LightProbeGroup> m_probeLocations = new Quadtree<LightProbeGroup>(new Rect(0, 0, 10f, 10f));
        //Stored list of reflection probes
        public static int m_storedProbes;
        //Current created count
        public static int m_currentProbeCount;

        #endregion

        #region Light Probe Creation Button Functions

        /// <summary>
        /// Load the probes in from the scene
        /// </summary>
        public static void LoadProbesFromScene()
        {
            //Start time
            //DateTime startTime = DateTime.Now;

            //Destroy previous contents
            m_probeLocations = null;

            //Work out the bounds of the environment
            float minY = float.NaN;
            float minX = float.NaN;
            float maxX = float.NaN;
            float minZ = float.NaN;
            float maxZ = float.NaN;
            Terrain sampleTerrain = null;
            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                if (float.IsNaN(minY))
                {
                    sampleTerrain = terrain;
                    minY = terrain.transform.position.y;
                    minX = terrain.transform.position.x;
                    minZ = terrain.transform.position.z;
                    maxX = minX + terrain.terrainData.size.x;
                    maxZ = minZ + terrain.terrainData.size.z;
                }
                else
                {
                    if (terrain.transform.position.x < minX)
                    {
                        minX = terrain.transform.position.x;
                    }
                    if (terrain.transform.position.z < minZ)
                    {
                        minZ = terrain.transform.position.z;
                    }
                    if ((terrain.transform.position.x + terrain.terrainData.size.x) > maxX)
                    {
                        maxX = terrain.transform.position.x + terrain.terrainData.size.x;
                    }
                    if ((terrain.transform.position.z + terrain.terrainData.size.z) > maxZ)
                    {
                        maxZ = terrain.transform.position.z + terrain.terrainData.size.z;
                    }
                }
            }

            if (sampleTerrain != null)
            {
                Rect terrainBounds = new Rect(minX, minZ, maxX - minX, maxZ - minZ);
                m_probeLocations = new Quadtree<LightProbeGroup>(terrainBounds, 32);
            }
            else
            {
                Rect bigSpace = new Rect(-10000f, -10000f, 20000f, 20000f);
                m_probeLocations = new Quadtree<LightProbeGroup>(bigSpace, 32);
            }

            //Now grab all the light probes in the scene
            LightProbeGroup probeGroup;
            LightProbeGroup[] probeGroups = UnityEngine.Object.FindObjectsOfType<LightProbeGroup>();

            for (int probeGroupIdx = 0; probeGroupIdx < probeGroups.Length; probeGroupIdx++)
            {
                probeGroup = probeGroups[probeGroupIdx];
                for (int probePosition = 0; probePosition < probeGroup.probePositions.Length; probePosition++)
                {
                    m_probeLocations.Insert(probeGroup.transform.position.x + probeGroup.probePositions[probePosition].x, probeGroup.transform.position.z + probeGroup.probePositions[probePosition].z, probeGroup);
                }
            }
            //Debug.Log(string.Format("Loaded {0} probe positions in {1:0.000} ms", m_probeLocations.Count, (DateTime.Now - startTime).TotalMilliseconds));
        }

        /// <summary>
        /// Creates a reflection probe at current scene view camera location
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="renderPipelineSettings"></param>
        public static void CreateLightProbeAtLocation(AmbientLightingProfile profile)
        {
            GameObject lightProbeObject = GameObject.Find("Light Probes Group Data");
            if (lightProbeObject == null)
            {
                lightProbeObject = new GameObject("Light Probes Group Data");
            }

            LightProbeGroup lightProbeData = lightProbeObject.GetComponent<LightProbeGroup>();
            if (lightProbeData == null)
            {
                lightProbeData = lightProbeObject.AddComponent<LightProbeGroup>();
                lightProbeData.probePositions = new Vector3[0];
            }

            //Assign scene view camera
            Vector3 scenePosition = SceneView.lastActiveSceneView.camera.transform.position;
            if (SceneView.lastActiveSceneView.pivot != scenePosition)
            {
                LoadProbesFromScene();

                m_probeLocations = null;

                Terrain terrain = Terrain.activeTerrain;
                for (int row = 0; row < profile.lightProbesPerRow; ++row)
                {
                    for (int columns = 0; columns < profile.lightProbesPerRow; ++columns)
                    {
                        //scenePosition = lightProbeObject.transform.position - lightProbeData.transform.position;
                        scenePosition.x = ((columns + 1) * profile.lightProbeSpawnRadius / profile.lightProbesPerRow) - profile.lightProbeSpawnRadius / profile.lightProbesPerRow / 2f;
                        scenePosition.z = ((row + 1) * profile.lightProbeSpawnRadius / profile.lightProbesPerRow) - profile.lightProbeSpawnRadius / profile.lightProbesPerRow / 2f;
                        float sampledHeight = terrain.SampleHeight(scenePosition);
                        scenePosition.y = sampledHeight + 0.5f;

                        List<Vector3> probePositions = new List<Vector3>(lightProbeData.probePositions);
                        Vector3 position = lightProbeObject.transform.position - lightProbeData.transform.position; //Translate to local space relative to lpg
                        probePositions.Add(scenePosition);
                        position += new Vector3(0f, 2.5f, 0f);
                        probePositions.Add(scenePosition);
                        position += new Vector3(0f, 10f, 0f);
                        probePositions.Add(scenePosition);
                        lightProbeData.probePositions = probePositions.ToArray();
                        AddProbe(lightProbeObject.transform.position, lightProbeData);

                        m_currentProbeCount++;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Scene View camera not found, probe spawned at position 0, 0, 0");
            }

            //Parent the probe object 
            GameObject lightProbeParent = LightProbeParenting(true);

            lightProbeObject.transform.SetParent(lightProbeParent.transform);

            m_currentProbeCount++;

            SceneView.lastActiveSceneView.Repaint();
        }

        /// <summary>
        /// Setup for the automatically probe spawning
        /// </summary>
        /// <param name="profile"></param>
        public static void CreateAutomaticProbes(AmbientLightingProfile profile)
        {
            GameObject lightParentObject = LightProbeParenting(true);
            int numberTerrains = Terrain.activeTerrains.Length;

            if (numberTerrains == 0)
            {
                Debug.LogError("Unable to initiate probe spawning systen. No terrain found");
                return;
            }
            else
            {
                if (profile.lightProbesPerRow < 2)
                {
                    Debug.LogError("Please set Light Probes Per Row to a value of 2 or higher");
                    return;
                }
                else
                {
                    LoadProbesFromScene();

                    GameObject lightProbeObject = GameObject.Find("Light Probes Group Data");
                    if (lightProbeObject == null)
                    {
                        lightProbeObject = new GameObject("Light Probes Group Data");
                    }

                    LightProbeGroup lightProbeData = lightProbeObject.GetComponent<LightProbeGroup>();
                    if (lightProbeData == null)
                    {
                        lightProbeData = lightProbeObject.AddComponent<LightProbeGroup>();
                        lightProbeData.probePositions = new Vector3[0];
                    }

                    m_probeLocations = null;

                    float seaLevel = 0f;

#if GAIA_PRESENT

                    Gaia.GaiaSessionManager gaiaSession = Object.FindObjectOfType<Gaia.GaiaSessionManager>();
                    if (gaiaSession != null)
                    {
                        seaLevel = profile.seaLevel;
                    }
                    else
                    {
                        seaLevel = profile.seaLevel;
                    }
#else

                    seaLevel = profile.seaLevel;

#endif

                    for (int terrainIdx = 0; terrainIdx < numberTerrains; terrainIdx++)
                    {
                        Terrain terrain = Terrain.activeTerrains[terrainIdx];
                        Vector3 terrainSize = terrain.terrainData.size;

                        m_storedProbes = profile.lightProbesPerRow * profile.lightProbesPerRow;

                        for (int row = 0; row < profile.lightProbesPerRow; ++row)
                        {
                            for (int columns = 0; columns < profile.lightProbesPerRow; ++columns)
                            {
                                EditorUtility.DisplayProgressBar("Adding Probes", "Adding probes to terrain :" + terrain.name, (float)m_currentProbeCount / (float)m_storedProbes);

                                if (profile.lightProbeSpawnType == AmbientSkiesConsts.LightProbeSpawnType.AutomaticallyGenerated)
                                {
                                    Vector3 newPosition = lightProbeObject.transform.position - lightProbeData.transform.position;
                                    newPosition.x = ((columns + 1) * terrainSize.x / profile.lightProbesPerRow) - terrainSize.x / profile.lightProbesPerRow / 2f + terrain.transform.position.x;
                                    newPosition.z = ((row + 1) * terrainSize.z / profile.lightProbesPerRow) - terrainSize.z / profile.lightProbesPerRow / 2f + terrain.transform.position.z;
                                    float sampledHeight = terrain.SampleHeight(newPosition);
                                    newPosition.y = sampledHeight + 2.5f;

                                    List<Vector3> probePositions = new List<Vector3>(lightProbeData.probePositions);
                                    Vector3 position = lightProbeObject.transform.position - lightProbeData.transform.position; //Translate to local space relative to lpg
                                    probePositions.Add(newPosition);
                                    position += new Vector3(0f, 2.5f, 0f);
                                    probePositions.Add(newPosition);
                                    position += new Vector3(0f, 10f, 0f);
                                    probePositions.Add(newPosition);
                                    lightProbeData.probePositions = probePositions.ToArray();
                                    AddProbe(lightProbeObject.transform.position, lightProbeData);

                                    m_currentProbeCount++;
                                }
                                else
                                {
                                    Vector3 newPosition = lightProbeObject.transform.position - lightProbeData.transform.position;
                                    newPosition.x = ((columns + 1) * terrainSize.x / profile.lightProbesPerRow) - terrainSize.x / profile.lightProbesPerRow / 2f + terrain.transform.position.x;
                                    newPosition.z = ((row + 1) * terrainSize.z / profile.lightProbesPerRow) - terrainSize.z / profile.lightProbesPerRow / 2f + terrain.transform.position.z;
                                    float sampledHeight = terrain.SampleHeight(newPosition);
                                    newPosition.y = sampledHeight + 2.5f;

                                    List<Vector3> probePositions = new List<Vector3>(lightProbeData.probePositions);
                                    Vector3 position = lightProbeObject.transform.position - lightProbeData.transform.position; //Translate to local space relative to lpg

                                    if (sampledHeight > seaLevel)
                                    {
                                        probePositions.Add(newPosition);
                                        position += new Vector3(0f, 2.5f, 0f);
                                        probePositions.Add(newPosition);
                                        position += new Vector3(0f, 10f, 0f);
                                        probePositions.Add(newPosition);
                                        lightProbeData.probePositions = probePositions.ToArray();
                                        AddProbe(lightProbeObject.transform.position, lightProbeData);

                                        m_currentProbeCount++;
                                    }
                                }
                            }
                        }

                        EditorUtility.ClearProgressBar();
                    }

                    lightProbeObject.transform.SetParent(lightParentObject.transform);
                }
            }
        }

        #endregion

        #region Light Probe Utils

        /// <summary>
        /// Creates the reflection prrobe parent object
        /// </summary>
        /// <param name="parentWithAmbientSkies"></param>
        /// <returns></returns>
        private static GameObject LightProbeParenting(bool parentWithAmbientSkies)
        {
            GameObject reflectionProbeParent = GameObject.Find("Scene Light Probes");
            GameObject ambientSkiesParent = GameObject.Find("Ambient Skies Environment");
            if (reflectionProbeParent == null)
            {
                reflectionProbeParent = new GameObject("Scene Light Probes");

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
        /// Add a probe instance into storage - must be called after the initial load call
        /// </summary>
        /// <param name="position">Position it is being located at</param>
        /// <param name="probeGroup">Probe group being managed</param>
        public static void AddProbe(Vector3 position, LightProbeGroup probeGroup)
        {
            if (m_probeLocations == null)
            {
                return;
            }
            m_probeLocations.Insert(position.x, position.z, probeGroup);
        }

        /// <summary>
        /// Add a probe instance into storage - must be called after the initial load call
        /// </summary>
        /// <param name="position">Position it is being located at</param>
        /// <param name="probeGroup">Probe group being managed</param>
        public static void RemoveProbe(Vector3 position, LightProbeGroup probeGroup)
        {
            if (m_probeLocations == null)
            {
                return;
            }
            m_probeLocations.Remove(position.x, position.z, probeGroup);
        }

        /// <summary>
        /// Clears reflection probes created
        /// </summary>
        public static void ClearCreatedLightProbes()
        {
            GameObject reflectionProbeParent = GameObject.Find("Scene Light Probes");
            if (reflectionProbeParent == null)
            {
                Debug.Log("No Light Probes have been created");
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