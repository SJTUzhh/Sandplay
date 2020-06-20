//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using System.Collections.Generic;

namespace AmbientSkies
{
    /// <summary>
    /// This script is an example of syncing something to the ambient skies time of day system.
    /// This example will set emissive materials on during the night and off during the day.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Procedural Worlds/Ambient Skies/Rendering/Realtime Material Emission Setup")]
    public class RealtimeEmissionSetup : MonoBehaviour
    {
        #region Variables

        [Header("Global Settings")]
        //Allows syncing to the Ambient Time Of Day system
        public bool m_syncToASTimeOfDay = true;
        //If not using the sync function you can use this key to switch it on and off
        public KeyCode m_emissionOverwriteActivation = KeyCode.I;
        //Array List that is used to enable and disable emission on
        public List<Material> m_emissiveMaterials = new List<Material>();
        //Scene Renderers
        public List<Renderer> m_sceneRenderers = new List<Renderer>();

        //Stores the status wheather it's day or night
        private bool m_sunActive;
        //Time of day system in the scene
        private AmbientSkiesTimeOfDay m_timeOfDay;
        //Current render pipeline that's selected
        private AmbientSkiesConsts.RenderPipelineSettings m_renderPipelineSettings;

        //Emission on as vector 4
        private Color m_emissiveOn = new Color(255f, 255f, 255f, 1f);
        //Emission off as vector 4
        private Color m_emissiveOff = new Color(1f, 1f, 1f, 0f);
        //activation status of the emission to see what state it's in
        private bool m_activestats;
        //Shows debugging
        private bool m_showDebugging = false;

        #endregion

        #region Setup

        /// <summary>
        /// Setup function
        /// </summary>
        private void Start()
        {
            //Find time of day component in the scene
            m_timeOfDay = FindObjectOfType<AmbientSkiesTimeOfDay>();

            //If time of day component is not null
            if (m_timeOfDay != null)
            {
                //Sets the render pipeline in this system
                m_renderPipelineSettings = m_timeOfDay.m_timeOfDayProfile.m_renderPipeline;
            }
        }

        #endregion

        #region Main Function

        /// <summary>
        /// Update function of the emission setup
        /// </summary>
        private void Update()
        {
            if (m_timeOfDay == null)
            {
                //Find time of day component in the scene
                m_timeOfDay = FindObjectOfType<AmbientSkiesTimeOfDay>();
            }

            //If using sync to time of day system
            if (m_syncToASTimeOfDay)
            {
                //If time of day componenet is null exit
                if (m_timeOfDay == null)
                {
                    if (m_showDebugging)
                    {
                        Debug.Log("Missing AmbientSkiesTimeOfDay component in the scene... Exiting system");
                    }

                    return;
                }
                else
                {
                    //Gets the current stats to see if the sun is active or moon is active
                    m_sunActive = m_timeOfDay.m_sunActive;
                    //If the current time is night
                    if (!m_sunActive)
                    {
                        //Set the emissive status as on
                        m_activestats = true;

                        //Checks to see if the array list has more than one material
                        if (m_emissiveMaterials != null)
                        {
                            //Get all materials that's been assigned in the public materials list
                            foreach (Material mats in m_emissiveMaterials)
                            {
                                //If pipeline is HD
                                if (m_renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                                {
                                    //For each one assign the paramater as off
                                    mats.SetVector("_EmissiveColor", m_emissiveOn / 10f);
                                }
                                else
                                {
                                    //For each one assign the paramater as off
                                    mats.SetVector("_EmissionColor", m_emissiveOn / 150f);
                                }
                            }

                            if (m_sceneRenderers.Count > 0)
                            {
                                if (m_renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                                {
                                    //Applies the mesh render to the dynamic GI
                                    foreach (Renderer renderer in m_sceneRenderers)
                                    {
                                        //Apply GI to renderer in list
                                        DynamicGI.SetEmissive(renderer, m_emissiveOn / 45f);
                                    }
                                }
                                else
                                {
                                    //Applies the mesh render to the dynamic GI
                                    foreach (Renderer renderer in m_sceneRenderers)
                                    {
                                        //Apply GI to renderer in list
                                        DynamicGI.SetEmissive(renderer, m_emissiveOn / 80f);
                                    }
                                }
                            }
                        }
                    }
                    //If current time is day
                    else
                    {
                        //Set the emissive status as off
                        m_activestats = false;

                        //Checks to see if the array list has more than one material
                        if (m_emissiveMaterials != null)
                        {
                            //Get all materials that's been assigned in the public materials list
                            foreach (Material mats in m_emissiveMaterials)
                            {
                                //If pipeline is HD
                                if (m_renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                                {
                                    //For each one assign the paramater as off
                                    mats.SetVector("_EmissiveColor", m_emissiveOff * 0);
                                }
                                else
                                {
                                    //For each one assign the paramater as off
                                    mats.SetVector("_EmissionColor", m_emissiveOff * 0);
                                }
                            }

                            if (m_sceneRenderers.Count > 0)
                            {
                                //Applies the mesh render to the dynamic GI
                                foreach (Renderer renderer in m_sceneRenderers)
                                {
                                    //Apply GI to renderer in list
                                    DynamicGI.SetEmissive(renderer, m_emissiveOff / 100);
                                }
                            }
                        }
                    }
                }
            }
            //If not syncing use keycode to switch emission on and off
            else
            {
                //If the button is pressed and the activation status is false
                if (Input.GetKeyDown(m_emissionOverwriteActivation) && !m_activestats)
                {
                    //Set the emissive status as on
                    m_activestats = true;

                    //Checks to see if the array list has more than one material
                    if (m_emissiveMaterials != null)
                    {
                        //Get all materials that's been assigned in the public materials list
                        foreach (Material mats in m_emissiveMaterials)
                        {
                            //If pipeline is HD
                            if (m_renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                            {
                                //For each one assign the paramater as off
                                mats.SetVector("_EmissiveColor", m_emissiveOn / 4f);
                            }
                            else
                            {
                                //For each one assign the paramater as off
                                mats.SetVector("_EmissionColor", m_emissiveOn / 7f);
                            }
                        }

                        if (m_sceneRenderers.Count > 0)
                        {
                            //Applies the mesh render to the dynamic GI
                            foreach (Renderer renderer in m_sceneRenderers)
                            {
                                //Apply GI to renderer in list
                                DynamicGI.SetEmissive(renderer, m_emissiveOn);
                            }
                        }

                        //Update the GI
                        DynamicGI.UpdateEnvironment();
                    }
                }
                else if (Input.GetKeyDown(m_emissionOverwriteActivation))
                {
                    //Set the emissive status as off
                    m_activestats = false;

                    //Checks to see if the array list has more than one material
                    if (m_emissiveMaterials != null)
                    {
                        //Get all materials that's been assigned in the public materials list
                        foreach (Material mats in m_emissiveMaterials)
                        {
                            //If pipeline is HD
                            if (m_renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                            {
                                //For each one assign the paramater as off
                                mats.SetVector("_EmissiveColor", m_emissiveOff);
                                //For each one assign the paramater as off
                                mats.SetColor("_EmissiveColor", m_emissiveOff);
                            }
                            else
                            {
                                //For each one assign the paramater as off
                                mats.SetVector("_EmissionColor", m_emissiveOff);
                                //For each one assign the paramater as off
                                mats.SetColor("_EmissionColor", m_emissiveOff);
                            }
                        }

                        if (m_sceneRenderers.Count > 0)
                        {
                            //Applies the mesh render to the dynamic GI
                            foreach (Renderer renderer in m_sceneRenderers)
                            {
                                //Apply GI to renderer in list
                                DynamicGI.SetEmissive(renderer, m_emissiveOff);
                            }
                        }

                        //Update the GI
                        DynamicGI.UpdateEnvironment();
                    }
                }
            }         
        }

        #endregion
    }
}