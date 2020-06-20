//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
using System;
#if WORLDAPI_PRESENT
using WAPI;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Experimental.Rendering;
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace AmbientSkies
{
    //#if !WORLDAPI_PRESENT
    /// <summary>
    /// Ambient Skies Time Of Day.
    /// This script controls time of day for Ambient Skies.
    /// We would like to thank Glen Rhodes for the inspiration behind this script. For more information refer to: 
    /// https://unity3d.com/learn/tutorials/topics/graphics/realtime-global-illumination-daynight-cycle
    /// http://www.glenrhodes.com/
    /// </summary>
    [AddComponentMenu("Procedural Worlds/Ambient Skies/System/Time Of Day")]
    [ExecuteInEditMode]
    public class AmbientSkiesTimeOfDay : MonoBehaviour
    {
        #region Variables

        [Header("Global Settings")]
        //Time of day profile that holds all the settings
        public AmbientSkiesTimeOfDayProfile m_timeOfDayProfile;
        //Main scriptable object for ambient skies
        [HideInInspector]
        public AmbientSkyProfiles m_ambientSkiesProfileVol1;
        //Current skybox settings
        [HideInInspector]
        public AmbientProceduralSkyboxProfile m_ambientProceduralSkybox;

        [HideInInspector]
        //The light rotation of the system
        public float lightRotationDot;
        //Bool to check if sun or the moon settings should be active
        [HideInInspector]
        public bool m_sunActive;

        private float minPoint;

        //Enables if it is night time
        private bool m_nightIsActive = false;
        //Main directional light object
        private GameObject m_mainLight;
        //Main directional sun light component
        private Light m_directionalSunLight;
        //Main directional moon light component
        private Light m_directionalMoonLight;
        //The Stored update timer for the GI Update
        private float updateGI;
#if HDPipeline
        //Sun hd light data
        private HDAdditionalLightData sunLightData;
        //Moon hd light data
        private HDAdditionalLightData nightLightData;
#endif

        #endregion

        #region Setup

        /// <summary>
        /// Setup before scene loads up
        /// </summary>
        private void Awake()
        {
            //Array list of components in the scene
            Component[] components = FindObjectsOfType<Component>();
            //Foreach loop through the components in the array
            foreach(Component component in components)
            {
                //If the component is empty
                if (component == null)
                {
                    //Destroy that components
                    DestroyImmediate(component);
                    Debug.Log(component + " Has been destroyed");
                }
            }
        }

        /// <summary>
        /// Start system
        /// </summary>
        private void Start()
        {
            //If profile exists
            if (m_timeOfDayProfile != null)
            {
                //If option is WAPI but WAPI is not in the project
                if (m_timeOfDayProfile.m_timeOfDayController == AmbientSkiesConsts.TimeOfDayController.WorldManagerAPI)
                {
                    if (m_timeOfDayProfile.m_debugMode)
                    {
                        Debug.LogWarning("Selected World Manager API for time of day controller but missing World Manager API from your project. Download World Manager API from here " + "http://www.procedural-worlds.com/blog/wapi/");
                    }
                    return;
                }
            }

            //If graphics settings is null pipeline is set to Built-In
            if (GraphicsSettings.renderPipelineAsset == null)
            {
                m_timeOfDayProfile.m_renderPipeline = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;
            }
            //If graphics settings is HDRenderPipelineAsset pipeline is set to High Definition
            else if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
            {
                m_timeOfDayProfile.m_renderPipeline = AmbientSkiesConsts.RenderPipelineSettings.HighDefinition;
            }
            //If graphics settings is not null or HDRenderPipelineAsset pipeline is set to Lightweight
            else
            {
                m_timeOfDayProfile.m_renderPipeline = AmbientSkiesConsts.RenderPipelineSettings.Lightweight;
            }

            //Main light object
            m_mainLight = GameObject.Find("TOD Light Handler");

            //Set the update gi value to the update interval
            updateGI = m_timeOfDayProfile.m_gIUpdateIntervalInSeconds;

            if (Application.isPlaying)
            {
                StartCoroutine(UpdateGiTimer());
            }
            else
            {
                StopAllCoroutines();
            }

            //If main light is null
            if (m_mainLight == null)
            {
                if (m_timeOfDayProfile.m_debugMode)
                {
                    Debug.LogError("Unable to find main direcional light");
                }
            }
            else
            {
                //Assign sun light component
                m_directionalSunLight = GameObject.Find("Directional Light Sun").GetComponent<Light>();

                //Assign moon light component
                m_directionalMoonLight = GameObject.Find("Directional Light Moon").GetComponent<Light>();
            }

            //If directional sun is there
            if (m_directionalSunLight != null)
            {
#if HDPipeline
                //Assigns the HD light data
                sunLightData = m_directionalSunLight.GetComponent<HDAdditionalLightData>();           
#endif
                //Sets indirect light bounce intensity. This works with GI to add more light into the scene as ambiance from the light
                m_directionalSunLight.bounceIntensity = 2.5f;
            }
            //If directional moon is there
            if (m_directionalMoonLight != null)
            {
#if HDPipeline
                //Assigns the HD light data
                nightLightData = m_directionalMoonLight.GetComponent<HDAdditionalLightData>();
#endif
                //Sets indirect light bounce intensity. This works with GI to add more light into the scene as ambiance from the light
                m_directionalMoonLight.bounceIntensity = 4f;
            }

            //Sets variables for built in/Lightweight
            switch (m_timeOfDayProfile.m_renderPipeline)
            {
                case AmbientSkiesConsts.RenderPipelineSettings.BuiltIn:
                    m_timeOfDayProfile.m_skyboxMaterial = GetAndSetSkyboxMaterial();
                    break;
                case AmbientSkiesConsts.RenderPipelineSettings.Lightweight:
                    m_timeOfDayProfile.m_skyboxMaterial = GetAndSetSkyboxMaterial();
                    break;
            }
        }

        /// <summary>
        /// Sets on enable
        /// </summary>
        private void OnEnable()
        {
            //Sets correct values of TOD on enabled
            if (m_ambientSkiesProfileVol1 != null)
            {
                m_timeOfDayProfile.m_sunRotationAmount = m_ambientSkiesProfileVol1.m_sunRotationAmount;
                m_timeOfDayProfile.m_timeToAddOrRemove = m_ambientSkiesProfileVol1.m_timeToAddOrRemove;
                m_timeOfDayProfile.m_pauseTime = m_ambientSkiesProfileVol1.m_pauseTime;
                m_timeOfDayProfile.m_currentTime = m_ambientSkiesProfileVol1.m_currentTimeOfDay;
            }

            //Assign sun light component
            m_directionalSunLight = GameObject.Find("Directional Light Sun").GetComponent<Light>();

            //Assign moon light component
            m_directionalMoonLight = GameObject.Find("Directional Light Moon").GetComponent<Light>();
        }

        /// <summary>
        /// Sets on disable
        /// </summary>
        private void OnDisable()
        {
            //Sets correct values of TOD on enabled
            if (m_ambientSkiesProfileVol1 != null)
            {
                m_ambientSkiesProfileVol1.m_sunRotationAmount = m_timeOfDayProfile.m_sunRotationAmount;
                m_ambientSkiesProfileVol1.m_timeToAddOrRemove = m_timeOfDayProfile.m_timeToAddOrRemove;
                m_ambientSkiesProfileVol1.m_pauseTime = m_timeOfDayProfile.m_pauseTime;
                m_ambientSkiesProfileVol1.m_currentTimeOfDay = m_timeOfDayProfile.m_currentTime;
            }
        }

        #endregion

        #region Realtime Update

        /// <summary>
        /// Update system
        /// </summary>
        private void Update()
        {
            if (m_timeOfDayProfile == null)
            {
                Debug.LogError("No Profile Selected for time of day... EXITING!!");
                return;
            }

            //Disable all coroutines to keep system clean in editor
            if (!Application.isPlaying)
            {
                StopAllCoroutines();
            }
            else
            {
                //If pause time key is pressed
                if (Input.GetKeyDown(m_timeOfDayProfile.m_pauseTimeKey))
                {
                    PauseTime(m_timeOfDayProfile.m_pauseTime);
                }

                //If increment up key is pressed
                if (Input.GetKeyDown(m_timeOfDayProfile.m_incrementUpKey))
                {
                    AddTime(m_timeOfDayProfile.m_timeToAddOrRemove);
                }

                //If increment down key is pressed
                if (Input.GetKeyDown(m_timeOfDayProfile.m_incrementDownKey))
                {
                    RemoveTime(m_timeOfDayProfile.m_timeToAddOrRemove);
                }

                //If rotate sun left key is pressed
                if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(m_timeOfDayProfile.m_rotateSunLeftKey) || Input.GetKeyDown(KeyCode.RightControl) && Input.GetKeyDown(m_timeOfDayProfile.m_rotateSunLeftKey))
                {
                    RotateSunLeft(m_timeOfDayProfile.m_sunRotationAmount);
                }

                //If rotate sun right key is pressed
                if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(m_timeOfDayProfile.m_rotateSunRightKey) || Input.GetKeyDown(KeyCode.RightControl) && Input.GetKeyDown(m_timeOfDayProfile.m_rotateSunRightKey))
                {
                    RotateSunRight(m_timeOfDayProfile.m_sunRotationAmount);
                }

                if (m_ambientProceduralSkybox != null)
                {
                    m_ambientProceduralSkybox.skyboxRotation = m_timeOfDayProfile.m_sunRotation;
                }
            }

            //If synchronization mode is WAPI return
            if (m_timeOfDayProfile.m_timeOfDayController == AmbientSkiesConsts.TimeOfDayController.WorldManagerAPI)
            {
                if (m_timeOfDayProfile.m_debugMode)
                {
                    Debug.LogWarning("Selected World Manager API for time of day controller but missing World Manager API from your project. Download World Manager API from here " + "http://www.procedural-worlds.com/blog/wapi/");
                }
                return;
            }
            else
            {               
                //If main light is null
                if (m_mainLight == null)
                {
                    m_mainLight = GameObject.Find("TOD Light Handler");
                    if (m_mainLight == null)
                    {
                        if (m_timeOfDayProfile.m_debugMode)
                        {
                            Debug.LogError("Unable to find object light handler");
                        }
                    }
                }
                else
                {
                    if (m_mainLight.name != "TOD Light Handler")
                    {
                        //Main light object
                        m_mainLight = GameObject.Find("TOD Light Handler");
                    }

                    if (m_directionalSunLight.name != "Directional Light Sun")
                    {
                        //Assign sun light component
                        m_directionalSunLight = GameObject.Find("Directional Light Sun").GetComponent<Light>();
                    }
                    if (m_directionalMoonLight.name != "Directional Light Moon")
                    {
                        //Assign moon light component
                        m_directionalMoonLight = GameObject.Find("Directional Light Moon").GetComponent<Light>();
                    }
                }

                //If directional sun is there
                if (m_directionalSunLight != null)
                {
#if HDPipeline
                    //Assigns the HD light data
                    sunLightData = m_directionalSunLight.GetComponent<HDAdditionalLightData>();
                    if (sunLightData == null)
                    {
                        //Adds the data to the light
                        sunLightData = m_directionalSunLight.gameObject.AddComponent<HDAdditionalLightData>();
                    }

                    //Checks to see if hd shadow data is there
                    if (m_directionalSunLight.GetComponent<AdditionalShadowData>() == null)
                    {
                        //Adds the data to the light
                        m_directionalSunLight.gameObject.AddComponent<AdditionalShadowData>();
                    }
#endif
                    //Sets indirect light bounce intensity. This works with GI to add more light into the scene as ambiance from the light
                    m_directionalSunLight.bounceIntensity = 2.5f;
                }
                //If directional moon is there
                if (m_directionalMoonLight != null)
                {
#if HDPipeline
                    //Assigns the HD light data
                    nightLightData = m_directionalMoonLight.GetComponent<HDAdditionalLightData>();
                    if (nightLightData == null)
                    {
                        //Adds the data to the light
                        nightLightData = m_directionalMoonLight.gameObject.AddComponent<HDAdditionalLightData>();
                    }

                    //Checks to see if hd shadow data is there
                    if (m_directionalMoonLight.GetComponent<AdditionalShadowData>() == null)
                    {
                        //Adds the data to the light
                        m_directionalMoonLight.gameObject.AddComponent<AdditionalShadowData>();
                    }
#endif
                    //Sets indirect light bounce intensity. This works with GI to add more light into the scene as ambiance from the light
                    m_directionalMoonLight.bounceIntensity = 4f;
                }

                //Switch check to check which time of day synchronization mode is active
                switch (m_timeOfDayProfile.m_timeOfDayController)
                {
                    case AmbientSkiesConsts.TimeOfDayController.Realtime:
                        //Updates Realtime mode
                        RealtimeSynchronization();
                        break;
                    case AmbientSkiesConsts.TimeOfDayController.AmbientSkies:
                        //Updates Ambient Skies mode
                        AmbientSkiesSynchronization();
                        break;
                    case AmbientSkiesConsts.TimeOfDayController.ThirdParty:
                        //Updates Third Party mode
                        ThirdPartySynchronization();
                        break;
                }
            }
        }

        #endregion

        #region Synchronization methods

        /// <summary>
        /// This handles all the Realtime updates to the time of day system
        /// </summary>
        public void RealtimeSynchronization()
        {
            //Gets the current date/time
            DateTime m_currentDateAndTime = DateTime.Now;

            //Sets the time of day hour based on your systems time
            m_timeOfDayProfile.m_timeOfDayHour = m_currentDateAndTime.TimeOfDay.Hours;
            //Sets the time of day minute based on your systems time
            m_timeOfDayProfile.m_timeOfDayMinutes = m_currentDateAndTime.TimeOfDay.Minutes;
            //Sets the time of day second based on your systems time
            m_timeOfDayProfile.m_timeOfDaySeconds = m_currentDateAndTime.TimeOfDay.Seconds;
            //Sets the day based on your systems date
            m_timeOfDayProfile.m_day = m_currentDateAndTime.Date.Day;
            //Sets the month based on your systems date
            m_timeOfDayProfile.m_month = m_currentDateAndTime.Date.Month;
            //Sets the year based on your systems date
            m_timeOfDayProfile.m_year = m_currentDateAndTime.Date.Year;
        }

        /// <summary>
        /// This handles all the Ambient Skies updates to the time of day system
        /// </summary>
        public void AmbientSkiesSynchronization()
        {
            //Only check if the application is playing
            if (Application.isPlaying)
            {
                //If pause time is true
                if (!m_timeOfDayProfile.m_pauseTime)
                {
                    //Game time amount
                    float gameTime;
                    if (m_nightIsActive)
                    {
                        //Updated Game Time
                        gameTime = +Time.deltaTime / m_timeOfDayProfile.m_nightLengthInSeconds;
                    }
                    else
                    {
                        //Updated Game Time
                        gameTime = +Time.deltaTime / m_timeOfDayProfile.m_dayLengthInSeconds;
                    }

                    //Update current time and setup, this can be used by users to manually change the time of day
                    m_timeOfDayProfile.m_currentTime += gameTime;
                    m_ambientSkiesProfileVol1.m_currentTimeOfDay = m_timeOfDayProfile.m_currentTime;
                    //If time reaches 1
                    if (m_timeOfDayProfile.m_currentTime >= 0.99f)
                    {
                        //Set time to 0 to allow it to proceed
                        m_timeOfDayProfile.m_currentTime = 0f;
                        //Add a day
                        m_timeOfDayProfile.m_day += 1;
                    }
                }  
                
                //If sun rotation is greater than 360 set it to 0
                if (m_timeOfDayProfile.m_sunRotation > 360f)
                {
                    m_timeOfDayProfile.m_sunRotation = 0f;
                }
                //If sun rotation is less than than 0 set it to 360
                if (m_timeOfDayProfile.m_sunRotation < 0f)
                {
                    m_timeOfDayProfile.m_sunRotation = 360f;
                }

                #region Time and Date updates

                //m_timeOfDaySeconds += Mathf.FloorToInt(m_currentTime * 86400f);
                //Add 1 minute when 60 seconds have passed
                if (m_timeOfDayProfile.m_timeOfDaySeconds > 59)
                {
                    m_timeOfDayProfile.m_timeOfDayMinutes += 1;
                    m_timeOfDayProfile.m_timeOfDaySeconds = 0;
                }
                //Add 1 hour when 60 minutes have passed
                if (m_timeOfDayProfile.m_timeOfDayMinutes > 59)
                {
                    m_timeOfDayProfile.m_timeOfDayHour += 1;
                    m_timeOfDayProfile.m_timeOfDayMinutes = 0;
                }
                //Add 1 day when 24 hours have passed
                if (m_timeOfDayProfile.m_timeOfDayHour > 24)
                {
                    m_timeOfDayProfile.m_day += 1;
                    m_timeOfDayProfile.m_timeOfDayHour = 1;
                }
                //Add 1 month when 31 days have passed
                if (m_timeOfDayProfile.m_day > 31)
                {
                    m_timeOfDayProfile.m_month += 1;
                    m_timeOfDayProfile.m_day = 1;
                }
                //Add 1 year when 12 months have passed
                if (m_timeOfDayProfile.m_month > 12)
                {
                    m_timeOfDayProfile.m_year += 1;
                    m_timeOfDayProfile.m_month = 1;
                }

                #endregion
            }

            #region Seasons Update
            if (m_timeOfDayProfile.m_hemisphereOrigin == AmbientSkiesConsts.HemisphereOrigin.Northern)
            {                
                //Change season to Spring if date is greater and month matches
                if (m_timeOfDayProfile.m_day >= 1 && m_timeOfDayProfile.m_month == 3)
                {
                    m_timeOfDayProfile.m_environmentSeason = AmbientSkiesConsts.CurrentSeason.Spring;
                    m_ambientSkiesProfileVol1.m_currentSeason = AmbientSkiesConsts.CurrentSeason.Spring;
                }
                //Change season to Summer if date is greater and month matches
                else if (m_timeOfDayProfile.m_day >= 1 && m_timeOfDayProfile.m_month == 6)
                {
                    m_timeOfDayProfile.m_environmentSeason = AmbientSkiesConsts.CurrentSeason.Summer;
                    m_ambientSkiesProfileVol1.m_currentSeason = AmbientSkiesConsts.CurrentSeason.Summer;
                }
                //Change season to Autumn Fall if date is greater and month matches
                else if (m_timeOfDayProfile.m_day >= 1 && m_timeOfDayProfile.m_month == 9)
                {
                    m_timeOfDayProfile.m_environmentSeason = AmbientSkiesConsts.CurrentSeason.AutumnFall;
                    m_ambientSkiesProfileVol1.m_currentSeason = AmbientSkiesConsts.CurrentSeason.AutumnFall;
                }
                //Change season to Winter if date is greater and month matches
                else if (m_timeOfDayProfile.m_day >= 1 && m_timeOfDayProfile.m_month == 12)
                {
                    m_timeOfDayProfile.m_environmentSeason = AmbientSkiesConsts.CurrentSeason.Winter;
                    m_ambientSkiesProfileVol1.m_currentSeason = AmbientSkiesConsts.CurrentSeason.Winter;
                }
            }
            else
            {
                //Change season to Spring if date is greater and month matches
                if (m_timeOfDayProfile.m_day >= 1 && m_timeOfDayProfile.m_month == 9)
                {
                    m_timeOfDayProfile.m_environmentSeason = AmbientSkiesConsts.CurrentSeason.Spring;
                    m_ambientSkiesProfileVol1.m_currentSeason = AmbientSkiesConsts.CurrentSeason.Spring;
                }
                //Change season to Summer if date is greater and month matches
                else if (m_timeOfDayProfile.m_day >= 1 && m_timeOfDayProfile.m_month == 12)
                {
                    m_timeOfDayProfile.m_environmentSeason = AmbientSkiesConsts.CurrentSeason.Summer;
                    m_ambientSkiesProfileVol1.m_currentSeason = AmbientSkiesConsts.CurrentSeason.Summer;
                }
                //Change season to Autumn Fall if date is greater and month matches
                else if (m_timeOfDayProfile.m_day >= 1 && m_timeOfDayProfile.m_month == 3)
                {
                    m_timeOfDayProfile.m_environmentSeason = AmbientSkiesConsts.CurrentSeason.AutumnFall;
                    m_ambientSkiesProfileVol1.m_currentSeason = AmbientSkiesConsts.CurrentSeason.AutumnFall;
                }
                //Change season to Winter if date is greater and month matches
                else if (m_timeOfDayProfile.m_day >= 1 && m_timeOfDayProfile.m_month == 6)
                {
                    m_timeOfDayProfile.m_environmentSeason = AmbientSkiesConsts.CurrentSeason.Winter;
                    m_ambientSkiesProfileVol1.m_currentSeason = AmbientSkiesConsts.CurrentSeason.Winter;
                }
            }


            #endregion

            //If render pipeline is High Definition
            if (m_timeOfDayProfile.m_renderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                //Update time of day system for HD
                UpdateHighDefinition(m_timeOfDayProfile.m_currentTime);
                //Sync post processing
                PostProcessingSync();
            }
            else
            {
                //Update time of day system for Built-In/LW
                UpdateBuiltInAndLightweight(m_timeOfDayProfile.m_currentTime);
                //Sync post processing
                PostProcessingSync();
            }
        }

        /// <summary>
        /// This handles all the Third Party updates to the time of day system
        /// </summary>
        public void ThirdPartySynchronization()
        {

        }

        /// <summary>
        /// Used to count down the update timer for GI Update
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateGiTimer()
        {
            //While active always execute this
            while (true)
            {
                //Wait 1 second before moving on
                yield return new WaitForSeconds(1);
                //Take 1 second away
                updateGI--;
                //If timer is less than or equal to 0
                if (updateGI <= 0)
                {
                    //Execue the GI update
                    UpdateDynamicGI(m_timeOfDayProfile.m_realtimeGIUpdate);
                }
                else
                {
                    //Return empty
                    yield return null;
                }
            }
        }

        #endregion

        #region Environmental Updates

        /// <summary>
        /// Function call to update the lighting/environment in built-in and lightweight
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateBuiltInAndLightweight(float gameTime)
        {
            //If main light object is empty exit the system
            if (m_mainLight == null)
            {
                if (m_timeOfDayProfile.m_debugMode)
                {
                    Debug.Log("HD mode enabled, sun is empty exiting");
                }
                return;
            }
            else
            {
                //If each of sun or moon lights are null exit the system
                if (m_directionalSunLight == null || m_directionalMoonLight == null)
                {
                    if (m_timeOfDayProfile.m_debugMode)
                    {
                        Debug.Log("Main light component is null exiting");
                        return;
                    }
                }

                //Ambient horizon when sun sets/moon sets
                float minAmbientPoint = -0.1f;
                //Min ambients
                float minAmbient = 0.7f;
                //Max ambients
                float maxAmbient = 1.2f;
                //Day time atmoshpere thickness used for procedural skybox
                float dayAtmosphereThickness = 1.2f;
                //Night time atmoshpere thickness used for procedural skybox
                float nightAtmosphereThickness = 0.8f;

                //Calculate the horizonRange
                float horizonRange = 1 - minPoint;
                //Light rotation calculation
                lightRotationDot = Mathf.Clamp01((Vector3.Dot(m_mainLight.transform.forward, Vector3.down) - minPoint) / horizonRange);
                //Ambiance calculation
                float ambience = ((maxAmbient - minAmbient) * lightRotationDot) + minAmbient;

                //Calculate the horizonRange
                horizonRange = 1 - minAmbientPoint;
                //Light rotation calculation
                lightRotationDot = Mathf.Clamp01((Vector3.Dot(m_mainLight.transform.forward, Vector3.down) - minAmbientPoint) / horizonRange);

                //Ambiance thickness calculation
                ambience = ((dayAtmosphereThickness - nightAtmosphereThickness) * lightRotationDot) + nightAtmosphereThickness;

                //Update rotation of the light handle object
                m_mainLight.transform.localRotation = Quaternion.Euler((gameTime * 360f) - 90f, m_timeOfDayProfile.m_sunRotation, 0f);

                //Object rotation is + Daytime
                if (lightRotationDot > 0)
                {
                    if (m_timeOfDayProfile.m_debugMode)
                    {
                        Debug.Log("Sun active as " + m_mainLight + " dot > 0");
                    }

                    //Set sun active as true
                    m_sunActive = true;
                    m_nightIsActive = false;
                    minPoint = -0.03f;
                }
                //Object rotation is - Night time
                else
                {
                    if (m_timeOfDayProfile.m_debugMode)
                    {
                        Debug.Log("Moon active as " + m_mainLight + " dot < 0");
                    }

                    //Set sun active as false
                    m_sunActive = false;
                    m_nightIsActive = true;
                    minPoint = 0.03f;
                }

                //Sun bool is active
                if (m_sunActive)
                {
                    //Sets the sunlight so the skybox can react correctly
                    RenderSettings.sun = m_directionalSunLight;

                    //Set moon light intensity to 0 (In HD this has to be done to remove console errors)
                    m_directionalMoonLight.intensity = 0f;

                    //Update sun color based of a gradient color chart
                    if (m_timeOfDayProfile.m_daySunGradientColor != null)
                    {
                        m_directionalSunLight.color = m_timeOfDayProfile.m_daySunGradientColor.Evaluate(lightRotationDot);
                    }

                    //Update the sun intensity based of an animation curve
                    if (m_timeOfDayProfile.m_daySunIntensity != null)
                    {
                        m_directionalSunLight.intensity = m_timeOfDayProfile.m_daySunIntensity.Evaluate(lightRotationDot) / 2f;
                    }

                    //Sets the skybox material to the scene
                    if (RenderSettings.skybox != m_timeOfDayProfile.m_skyboxMaterial)
                    {
                        //Assigns the material to the render settings
                        RenderSettings.skybox = m_timeOfDayProfile.m_skyboxMaterial;

                        m_timeOfDayProfile.m_skyboxMaterial.SetColor("_SkyTint", new Color32(110, 110, 110, 255));
                    }

                    //Sets the atmosphere thickness shader value
                    m_timeOfDayProfile.m_skyboxMaterial.SetFloat("_AtmosphereThickness", ambience);
                    //Sets the ground color shader value
                    if (m_timeOfDayProfile.m_dayFogColor != null)
                    {
                        m_timeOfDayProfile.m_skyboxMaterial.SetColor("_GroundColor", m_timeOfDayProfile.m_dayFogColor.Evaluate(lightRotationDot));
                    }   

                    //Sets the fog mode to user selected
                    RenderSettings.fogMode = m_timeOfDayProfile.m_fogMode;
                    //Sets the fog start distance
                    RenderSettings.fogStartDistance = m_timeOfDayProfile.m_startFogDistance;
                    //Sets the fog end distance
                    if (m_timeOfDayProfile.m_dayFogDistance != null)
                    {
                        RenderSettings.fogEndDistance = m_timeOfDayProfile.m_dayFogDistance.Evaluate(lightRotationDot);
                    }

                    //Sets fog color
                    if (m_timeOfDayProfile.m_dayFogColor != null)
                    {
                        RenderSettings.fogColor = m_timeOfDayProfile.m_dayFogColor.Evaluate(lightRotationDot);
                    }

                    //Sets the fog density
                    if (m_timeOfDayProfile.m_dayFogDensity != null)
                    {
                        RenderSettings.fogDensity = m_timeOfDayProfile.m_dayFogDensity.Evaluate(lightRotationDot);
                    }

                    //Ambiance calculation
                    ambience = ((maxAmbient - minAmbient) * lightRotationDot) + minAmbient;
                    //Sets the ambient intensity
                    RenderSettings.ambientIntensity = ambience;
                }
                //Sun bool is not active
                else
                {
                    //Sets the sunlight so the skybox can react correctly
                    RenderSettings.sun = m_directionalMoonLight;

                    //Set sun light intensity to 0 (In HD this has to be done to remove console errors)
                    m_directionalSunLight.intensity = 0f;

                    //Update night color based of a gradient color chart
                    if (m_timeOfDayProfile.m_nightSunGradientColor != null)
                    {
                        m_directionalMoonLight.color = m_timeOfDayProfile.m_nightSunGradientColor.Evaluate(lightRotationDot);
                    }

                    //Update the moon intensity based of an animation curve
                    if (m_timeOfDayProfile.m_nightSunIntensity != null)
                    {
                        m_directionalMoonLight.intensity = m_timeOfDayProfile.m_nightSunIntensity.Evaluate(lightRotationDot) / 2;
                    }

                    //Sets the skybox material to the scene
                    if (RenderSettings.skybox != m_timeOfDayProfile.m_skyboxMaterial)
                    {
                        //Assigns the material to the render settings
                        RenderSettings.skybox = m_timeOfDayProfile.m_skyboxMaterial;
                    }

                    //Sets the atmosphere thickness shader value
                    m_timeOfDayProfile.m_skyboxMaterial.SetFloat("_AtmosphereThickness", ambience / 3f);
                    //Sets the ground color shader value
                    if (m_timeOfDayProfile.m_nightFogColor != null)
                    {
                        m_timeOfDayProfile.m_skyboxMaterial.SetColor("_GroundColor", m_timeOfDayProfile.m_nightFogColor.Evaluate(lightRotationDot));
                    }

                    //Sets the fog mode to user selected
                    RenderSettings.fogMode = m_timeOfDayProfile.m_fogMode;
                    //Sets the fog start distance
                    RenderSettings.fogStartDistance = m_timeOfDayProfile.m_startFogDistance;
                    //Sets the fog end distance
                    if (m_timeOfDayProfile.m_nightFogDistance != null)
                    {
                        RenderSettings.fogEndDistance = m_timeOfDayProfile.m_nightFogDistance.Evaluate(lightRotationDot);
                    }

                    //Sets fog color
                    if (m_timeOfDayProfile.m_nightFogColor != null)
                    {
                        RenderSettings.fogColor = m_timeOfDayProfile.m_nightFogColor.Evaluate(lightRotationDot);
                    }

                    //Ambiance calculation
                    ambience = ((maxAmbient - minAmbient) * lightRotationDot) + minAmbient;
                    //Sets the ambient intensity
                    RenderSettings.ambientIntensity = ambience;
                    //Sets the fog density
                    if (m_timeOfDayProfile.m_nightFogDensity != null)
                    {
                        RenderSettings.fogDensity = m_timeOfDayProfile.m_nightFogDensity.Evaluate(lightRotationDot);
                    }
                }                
            }
        }

        /// <summary>
        /// Function call to update the lighting/environment in high definition
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateHighDefinition(float gameTime)
        {
#if HDPipeline && UNITY_2018_3_OR_NEWER
            //Finds all HD Light datas
            HDAdditionalLightData[] lightDatas = FindObjectsOfType<HDAdditionalLightData>();
            //Foreach loop of all light datas
            foreach (HDAdditionalLightData lightData in lightDatas)
            {
                //If the light mode doesn't equal Lux
                if (lightData.lightUnit != LightUnit.Lux)
                {
                    //Set light mode to Lux
                    lightData.lightUnit = LightUnit.Lux;
                }
            }

            //If main light object is empty exit the system
            if (m_mainLight == null)
            {
                if (m_timeOfDayProfile.m_debugMode)
                {
                    Debug.Log("HD mode enabled, sun is empty exiting");
                }
                return;
            }
            else
            {
                //If each of sun or moon lights are null exit the system
                if (m_directionalSunLight == null || m_directionalMoonLight == null)
                {
                    if (m_timeOfDayProfile.m_debugMode)
                    {
                        Debug.Log("Main light component is null exiting");
                        return;
                    }
                }

                //Bool to check if sun or the moon settings should be active
                m_sunActive = false;

                //Ambient horizon when sun sets/moon sets
                float minAmbientPoint = -0.03f;
                //Min ambients
                float minAmbient = 1f;
                //Max ambients
                float maxAmbient = 1.45f;
                //Day time atmoshpere thickness used for procedural skybox
                float dayAtmosphereThickness = 1.2f;
                //Night time atmoshpere thickness used for procedural skybox
                float nightAtmosphereThickness = 0.8f;

                //Calculate the horizonRange
                float horizonRange = 1 - minPoint;
                //Light rotation calculation
                lightRotationDot = Mathf.Clamp01((Vector3.Dot(m_mainLight.transform.forward, Vector3.down) - minPoint) / horizonRange);
                //Ambiance calculation
                float ambience = ((maxAmbient - minAmbient) * lightRotationDot) + minAmbient;

                //Calculate the horizonRange
                horizonRange = 1 - minAmbientPoint;
                //Light rotation calculation
                lightRotationDot = Mathf.Clamp01((Vector3.Dot(m_mainLight.transform.forward, Vector3.down) - minAmbientPoint) / horizonRange);
                //Ambiance calculation
                ambience = ((maxAmbient - minAmbient) * lightRotationDot) + minAmbient;
                //Ambiance thickness calculation
                ambience = ((dayAtmosphereThickness - nightAtmosphereThickness) * lightRotationDot) + nightAtmosphereThickness;

                //Update rotation of the light handle object
                m_mainLight.transform.localRotation = Quaternion.Euler((gameTime * 360f) - 90f, m_timeOfDayProfile.m_sunRotation, 0f);

                //Object rotation is + 
                if (lightRotationDot > 0)
                {
                    if (m_timeOfDayProfile.m_debugMode)
                    {
                        Debug.Log("Sun active as " + m_mainLight + " dot > 0");
                    }

                    //Set sun active as true
                    m_sunActive = true;
                    minPoint = -0.03f;
                }
                //Object rotation is -
                else
                {
                    if (m_timeOfDayProfile.m_debugMode)
                    {
                        Debug.Log("Moon active as " + m_mainLight + " dot < 0");
                    }

                    //Set sun active as false
                    m_sunActive = false;
                    minPoint = 0.03f;
                }

                //Sun bool is active
                if (m_sunActive)
                {
                    m_directionalSunLight.enabled = true;
                    //Check if not null
                    if (sunLightData != null)
                    {
                        //Update sun intensity based of a animated curve
                        if (m_timeOfDayProfile.m_daySunIntensity != null)
                        {
                            sunLightData.intensity = m_timeOfDayProfile.m_daySunIntensity.Evaluate(lightRotationDot) * 3.14f;
                        }
                    }
                    //Check if not null
                    if (nightLightData != null)
                    {
                        //Set night light intensity to 0 (In HD this has to be done to remove console errors)
                        nightLightData.intensity = 0f;
                    }

                    //Update sun color based of a gradient color chart
                    if (m_timeOfDayProfile.m_daySunGradientColor != null)
                    {
                        m_directionalSunLight.color = m_timeOfDayProfile.m_daySunGradientColor.Evaluate(lightRotationDot);
                    }
                }
                //Sun bool is not active
                else
                {
                    m_directionalMoonLight.enabled = true;
                    //Check if not null
                    if (sunLightData != null)
                    {
                        //Set night light intensity to 0 (In HD this has to be done to remove console errors)
                        sunLightData.intensity = 0f;
                    }
                    //Check if not null
                    if (nightLightData != null)
                    {
                        //Update night intensity based of a animated curve
                        if (m_timeOfDayProfile.m_nightSunIntensity != null)
                        {
                            nightLightData.intensity = m_timeOfDayProfile.m_nightSunIntensity.Evaluate(lightRotationDot) * 3.14f;
                        }
                    }

                    //Update night color based of a gradient color chart
                    if (m_timeOfDayProfile.m_nightSunGradientColor != null)
                    {
                        m_directionalMoonLight.color = m_timeOfDayProfile.m_nightSunGradientColor.Evaluate(lightRotationDot);
                    }
                }

                //Assing the volume profile to allow us to modify settings
                VolumeProfile volumeProfile = m_timeOfDayProfile.m_hDRPVolumeProfile;
                if (volumeProfile == null)
                {
                    volumeProfile = GetVolumeProfile();
                    m_timeOfDayProfile.m_hDRPVolumeProfile = volumeProfile;
                }
                if (volumeProfile != null)
                {
                    //Visual environment, settings that determine which type of sky is being used
                    VisualEnvironment visualEnvironment;
                    if (volumeProfile.TryGet(out visualEnvironment))
                    {
                        //Change it to Procedural
                        visualEnvironment.skyType.value = 2;
                        visualEnvironment.fogType.value = FogType.Volumetric;
                    }

                    //HDRI sky (cubemap based skyboxes)
                    HDRISky hDRISky;
                    if (volumeProfile.TryGet(out hDRISky))
                    {
                        //Disable it
                        hDRISky.active = false;
                    }

                    //Procedural skybox settings
                    ProceduralSky proceduralSky;
                    if (volumeProfile.TryGet(out proceduralSky))
                    {
                        //Enable it
                        proceduralSky.active = true;
                        proceduralSky.skyTint.value = new Color32(110, 110, 110, 255);
                        //Sets the sun size based on the light rotation
                        if (m_timeOfDayProfile.m_sunSize != null)
                        {
                            proceduralSky.sunSize.value = m_timeOfDayProfile.m_sunSize.Evaluate(lightRotationDot);
                        }

                        //Sets the skybox exposure based on the light rotation
                        if (m_timeOfDayProfile.m_skyExposure != null)
                        {
                            proceduralSky.exposure.value = m_timeOfDayProfile.m_skyExposure.Evaluate(lightRotationDot);
                        }
                        //Set the atmosphere thickness to ambienance value
                        proceduralSky.atmosphereThickness.value = ambience;
                        //If sun active is true
                        if (m_sunActive)
                        {
                            //Set ground color based on a gradient color chart day
                            if (m_timeOfDayProfile.m_dayFogColor != null)
                            {
                                proceduralSky.groundColor.value = m_timeOfDayProfile.m_dayFogColor.Evaluate(lightRotationDot);
                            }
                        }
                        //If sun active is false
                        else
                        {
                            //Set ground color based on a gradient color chart night
                            if (m_timeOfDayProfile.m_nightFogColor != null)
                            {
                                proceduralSky.groundColor.value = m_timeOfDayProfile.m_nightFogColor.Evaluate(lightRotationDot);
                            }
                        }

                        proceduralSky.includeSunInBaking.overrideState = true;
                        proceduralSky.includeSunInBaking.value = true;
                    }

                    //Volumetric fog settings
                    VolumetricFog volumetricFog;
                    if (volumeProfile.TryGet(out volumetricFog))
                    {
                        //Sets the fog height to be correct and seen by the player
                        volumetricFog.active = true;
                        volumetricFog.baseHeight.value = 50f;
                        volumetricFog.meanHeight.value = 120f;

                        //Set fog settings based on a gradient color chart day
                        if (m_sunActive)
                        {
                            if (m_timeOfDayProfile.m_dayFogColor != null)
                            {
                                volumetricFog.albedo.value = m_timeOfDayProfile.m_dayFogColor.Evaluate(lightRotationDot);
                            }

                            if (m_timeOfDayProfile.m_dayFogDistance != null)
                            {
                                volumetricFog.meanFreePath.value = m_timeOfDayProfile.m_dayFogDistance.Evaluate(lightRotationDot);
                            }

                            if (m_timeOfDayProfile.m_lightAnisotropy != null)
                            {
                                volumetricFog.anisotropy.value = m_timeOfDayProfile.m_lightAnisotropy.Evaluate(lightRotationDot);        
                            }

                            if (m_timeOfDayProfile.m_lightProbeDimmer != null)
                            {
                                volumetricFog.globalLightProbeDimmer.value = m_timeOfDayProfile.m_lightProbeDimmer.Evaluate(lightRotationDot);
                            }
                        }
                        //Set fog settings based on a gradient color chart night
                        else
                        {
                            if (m_timeOfDayProfile.m_nightFogColor != null)
                            {
                                volumetricFog.albedo.value = m_timeOfDayProfile.m_nightFogColor.Evaluate(lightRotationDot);
                            }

                            if (m_timeOfDayProfile.m_nightFogDistance != null)
                            {
                                volumetricFog.meanFreePath.value = m_timeOfDayProfile.m_nightFogDistance.Evaluate(lightRotationDot);
                            }

                            if (m_timeOfDayProfile.m_lightAnisotropy != null)
                            {
                                volumetricFog.anisotropy.value = m_timeOfDayProfile.m_lightAnisotropy.Evaluate(lightRotationDot);               
                            }

                            if (m_timeOfDayProfile.m_lightProbeDimmer != null)
                            {
                                volumetricFog.globalLightProbeDimmer.value = m_timeOfDayProfile.m_lightProbeDimmer.Evaluate(lightRotationDot);
                            }
                        }
                    }

                    //Volumetric Lighting Controller, used to enhance fog and light contribution settings to the scene
                    VolumetricLightingController volumetricLightingController;
                    if (volumeProfile.TryGet(out volumetricLightingController))
                    {
                        if (m_timeOfDayProfile.m_lightDepthExtent != null)
                        {
                            volumetricLightingController.depthExtent.value = m_timeOfDayProfile.m_lightDepthExtent.Evaluate(lightRotationDot);
                        }
                    }

                    //Ambient light intensity settings
                    IndirectLightingController lightingController;
                    if (volumeProfile.TryGet(out lightingController))
                    {
                        //Sets diffused ambient light intensity to ambience
                        lightingController.indirectDiffuseIntensity.value = ambience * 1.25f;
                    }
                }            
            }   
#endif
        }

        /// <summary>
        /// Used to control and sync the post processing to the system
        /// </summary>
        public void PostProcessingSync()
        {
#if UNITY_POST_PROCESSING_STACK_V2

#if UNITY_2019_1_OR_NEWER && HDPipeline
            //Find HDRP post process object
            GameObject hDRPPostProcessObject = GameObject.Find("Post Processing HDRP Volume");
            if (hDRPPostProcessObject == null)
            {
                return;
            }
            else
            {
                //Get the volume from the HDRP object
                VolumeProfile profile = hDRPPostProcessObject.GetComponent<Volume>().sharedProfile;
                if (profile != null)
                {
                    //Color adjustments value
                    ColorAdjustments colorAdjustments;
                    //White balance value
                    WhiteBalance whiteBalance;

                    //If it's day time
                    if (m_sunActive)
                    {
                        //Get color adjustments from profile
                        if (profile.TryGet(out colorAdjustments))
                        {
                            //If it's not enabled
                            if (!colorAdjustments.colorFilter.overrideState)
                            {
                                //Enable effect
                                colorAdjustments.colorFilter.overrideState = true;
                            }

                            //Set the color filter based on the time of day
                            colorAdjustments.colorFilter.value = m_timeOfDayProfile.m_dayColor.Evaluate(lightRotationDot);
                        }

                        //Get white balance from profile
                        if (profile.TryGet(out whiteBalance))
                        {
                            //If it's not enabled
                            if (!whiteBalance.temperature.overrideState)
                            {
                                //Enable effect
                                whiteBalance.temperature.overrideState = true;
                            }

                            //Set the tempature based on the time of day
                            whiteBalance.temperature.value = m_timeOfDayProfile.m_dayTempature.Evaluate(lightRotationDot);
                        }
                    }
                    //If it's night time
                    else
                    {
                        //Get color adjustments from profile
                        if (profile.TryGet(out colorAdjustments))
                        {
                            //If it's not enabled
                            if (!colorAdjustments.colorFilter.overrideState)
                            {
                                //Enable effect
                                colorAdjustments.colorFilter.overrideState = true;
                            }

                            //Set the color filter based on the time of day
                            colorAdjustments.colorFilter.value = m_timeOfDayProfile.m_nightColor.Evaluate(lightRotationDot);
                        }

                        //Get white balance from profile
                        if (profile.TryGet(out whiteBalance))
                        {
                            //If it's not enabled
                            if (!whiteBalance.temperature.overrideState)
                            {
                                //Enable effect
                                whiteBalance.temperature.overrideState = true;
                            }

                            //Set the tempature based on the time of day
                            whiteBalance.temperature.value = m_timeOfDayProfile.m_nightTempature.Evaluate(lightRotationDot);
                        }
                    }
                }
            }
#else
            //If unable to find the gameobject exit the function
            if (GameObject.Find("Global Post Processing") == null)
            {
                return;
            }
            //Find the global post processing volume
            PostProcessVolume processVolume = GameObject.Find("Global Post Processing").GetComponent<PostProcessVolume>();
            if (processVolume != null)
            {
                //Get the profile attached to the volume
                PostProcessProfile profile = processVolume.sharedProfile;
                if (profile != null)
                {
                    //Color grading value
                    ColorGrading colorGrading;
                    //Get color grading value
                    if (profile.TryGetSettings(out colorGrading))
                    {
                        //If using sync system
                        if (m_timeOfDayProfile.m_syncPostFXToTimeOfDay)
                        {
                            //Enable settings
                            colorGrading.colorFilter.overrideState = true;
                            colorGrading.temperature.overrideState = true;
                        }
                        //If not using sync system revert and exit
                        else
                        {
                            //Disable setting
                            colorGrading.colorFilter.overrideState = false;
                            return;
                        }

                        //If day is active
                        if (m_sunActive)
                        {
                            //Adjust color folter based of the gradient
                            if (m_timeOfDayProfile.m_dayColor != null)
                            {
                                colorGrading.colorFilter.value = m_timeOfDayProfile.m_dayColor.Evaluate(lightRotationDot);
                            }

                            //Adjust tempature base on animation curve
                            if (m_timeOfDayProfile.m_dayTempature != null)
                            {
                                colorGrading.temperature.value = m_timeOfDayProfile.m_dayTempature.Evaluate(lightRotationDot);
                            }
                        }
                        //If night is active
                        else
                        {
                            //Adjust color folter based of the gradient
                            if (m_timeOfDayProfile.m_nightColor != null)
                            {
                                colorGrading.colorFilter.value = m_timeOfDayProfile.m_nightColor.Evaluate(lightRotationDot);
                            }

                            //Adjust tempature base on animation curve
                            if (m_timeOfDayProfile.m_nightTempature != null)
                            {
                                colorGrading.temperature.value = m_timeOfDayProfile.m_nightTempature.Evaluate(lightRotationDot);
                            }
                        }
                    }
                }
            }

#endif
#endif
        }

        /// <summary>
        /// Sets pause time status
        /// </summary>
        /// <param name="isTimePaused"></param>
        public void PauseTime(bool isTimePaused)
        {
            //If time is already paused
            if (m_timeOfDayProfile.m_pauseTime)
            {
                //Unpause time
                m_timeOfDayProfile.m_pauseTime = false;
                m_ambientSkiesProfileVol1.m_pauseTime = false;
            }
            else
            {
                //Pause time
                m_timeOfDayProfile.m_pauseTime = true;
                m_ambientSkiesProfileVol1.m_pauseTime = true;
            }
        }

        /// <summary>
        /// Adds time from current time of day
        /// </summary>
        /// <param name="timeToAdd"></param>
        public void AddTime(float timeToAdd)
        {
            //Add time
            m_ambientSkiesProfileVol1.m_currentTimeOfDay += timeToAdd;
            m_timeOfDayProfile.m_currentTime = m_ambientSkiesProfileVol1.m_currentTimeOfDay;
        }

        /// <summary>
        /// Removes time from current time of day
        /// </summary>
        /// <param name="timeToRemove"></param>
        public void RemoveTime(float timeToRemove)
        {
            //Remove time
            m_ambientSkiesProfileVol1.m_currentTimeOfDay -= timeToRemove;
            m_timeOfDayProfile.m_currentTime = m_ambientSkiesProfileVol1.m_currentTimeOfDay;
        }

        /// <summary>
        /// Used to rotate the sun left
        /// </summary>
        /// <param name="rotationValue"></param>
        public void RotateSunLeft(float rotationValue)
        {
            if (m_mainLight != null)
            {
                m_ambientSkiesProfileVol1.m_skyboxRotation -= rotationValue;
                m_timeOfDayProfile.m_sunRotation = m_ambientSkiesProfileVol1.m_skyboxRotation;
            }
        }

        /// <summary>
        /// Used to rotate the sun Right
        /// </summary>
        /// <param name="rotationValue"></param>
        public void RotateSunRight(float rotationValue)
        {
            if (m_mainLight != null)
            {
                m_ambientSkiesProfileVol1.m_skyboxRotation += rotationValue;
                m_timeOfDayProfile.m_sunRotation = m_ambientSkiesProfileVol1.m_skyboxRotation;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets and returns the volume profile
        /// </summary>
        /// <returns>Volume Profile</returns>
#if HDPipeline
        public static VolumeProfile GetVolumeProfile()
        {
            //Finds the volume in the scene
            Volume volume = GameObject.Find("High Definition Environment Volume").GetComponent<Volume>();
            if (volume != null)
            {
                //Checks to see if the profile is assigned in the volume
                if (volume.sharedProfile == null)
                {
#if UNITY_EDITOR
                    //If Missing it'll add it to the volume
                    volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(GetAssetPath("Ambient Skies HD Volume Profile"));
#endif
                    //Gets the profile
                    VolumeProfile volumeProfile = volume.sharedProfile;
                    //Returns the profile
                    return volumeProfile;
                }
                else
                {
                    //Gets the profile
                    VolumeProfile volumeProfile = volume.sharedProfile;
                    //Returns the profile
                    return volumeProfile;
                }
            }

            //Else return null
            return null;
        }
#endif

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>The path or null</returns>
#if UNITY_EDITOR
        public static string GetAssetPath(string name)
        {
            string[] assets = AssetDatabase.FindAssets(name, null);
            if (assets.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assets[0]);
            }
            return null;
        }
#endif

        /// <summary>
        /// Get the main directional light in the scene
        /// </summary>
        /// <returns>Main light or null</returns>
        public GameObject GetMainDirectionalLight()
        {
            //Find "Directional Light" GameObject
            GameObject lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                //Grab the first directional light we can find
                Light[] lights = FindObjectsOfType<Light>();
                foreach (var light in lights)
                {
                    //Filter light mode to Directional
                    if (light.type == LightType.Directional)
                    {
                        lightObj = light.gameObject;
                        break;
                    }
                }
            }
            //Return object
            return lightObj;
        }

        /// <summary>
        /// Gets and returns the skybox material
        /// </summary>
        /// <returns>Skybox Material</returns>
        public Material GetAndSetSkyboxMaterial()
        {
            //Gets material from skybox in settings
            Material skyMat = RenderSettings.skybox;
            //If render pipeline is HD
            if (m_timeOfDayProfile.m_renderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                //Set it to null as not needed
                skyMat = null;
                //Return material
                return skyMat;
            }
            else
            {
                //If it's null load it up from the project
                if (skyMat == null)
                {
#if UNITY_EDITOR
                    //Load "Ambient Skies Skybox" material from project
                    skyMat = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Ambient Skies Skybox"));
#endif
                }
            }

            //Return material
            return skyMat;
        }

        /// <summary>
        /// Updates Dynamic GI
        /// </summary>
        /// <param name="updateEveryFrame"></param>
        /// <param name="gameTime"></param>
        public void UpdateDynamicGI(bool updateEveryFrame)
        {
            //If realtime GI is enabled
            if (m_timeOfDayProfile.m_realtimeGIUpdate)
            {
                //Checks if the application is playing
                if (Application.isPlaying)
                {
                    //If want to use update every frame
                    if (updateEveryFrame)
                    {
                        if (m_timeOfDayProfile.m_debugMode)
                        {
                            Debug.Log("Updating GI Realtime Per Frame");
                        }

                        //Sets GI to update every frame
                        DynamicGI.synchronousMode = true;
                        return;
                    }
                    //If not want to use update every frame
                    else
                    {
                        //Resets the timer
                        updateGI = m_timeOfDayProfile.m_gIUpdateIntervalInSeconds;
                        //Sets GI to not update every frame
                        DynamicGI.synchronousMode = false;

                        //Gets all reflection probes in the scene
                        ReflectionProbe[] reflectionProbes = FindObjectsOfType<ReflectionProbe>();
                        foreach (ReflectionProbe probe in reflectionProbes)
                        {
                            //Rebakes the texture
                            probe.RenderProbe();
                        }

                        if (m_timeOfDayProfile.m_debugMode)
                        {
                            Debug.Log("Updating GI Realtime Once");
                        }

                        //Manual update of the GI
                        DynamicGI.UpdateEnvironment();

                        if (m_timeOfDayProfile.m_debugMode)
                        {
                            Debug.Log("Updating GI");
                        }
                        return;
                    }
                }
            }
        }

        #endregion

    }
}
//#else

///
// Add WAPI version here
///

//#endif