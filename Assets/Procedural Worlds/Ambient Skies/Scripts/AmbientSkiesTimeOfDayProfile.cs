//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
#endif

namespace AmbientSkies
{
    public class AmbientSkiesTimeOfDayProfile : ScriptableObject
    {
        #region Variables

        [Header("Global Settings")]
        //Enables the codes debugging to check for errors or check tasks log
        [HideInInspector]
        public bool m_debugMode = true;
        //Global setting to set the system behaviour
        [HideInInspector]
        public AmbientSkiesConsts.TimeOfDayController m_timeOfDayController = AmbientSkiesConsts.TimeOfDayController.AmbientSkies;
        [Header("Realtime GI Settings")]
        //This cool controls the realtime GI update
        public bool m_realtimeGIUpdate = true;
        //Time to wait before we update the realtime GI
        public int m_gIUpdateIntervalInSeconds = 15;
        [Header("Season Settings")]
        //Sets the Hemisphere Origin
        public AmbientSkiesConsts.HemisphereOrigin m_hemisphereOrigin = AmbientSkiesConsts.HemisphereOrigin.Northern;
        //Sets the current season
        public AmbientSkiesConsts.CurrentSeason m_environmentSeason = AmbientSkiesConsts.CurrentSeason.Winter;
        //Render Pipeline, auto assings on start
        [HideInInspector]
        public AmbientSkiesConsts.RenderPipelineSettings m_renderPipeline;
        [Header("Keycode Commands & Increment Settings")]
        //Key button used to pause time
        public KeyCode m_pauseTimeKey = KeyCode.P;
        //Key button used to add time to current time
        public KeyCode m_incrementUpKey = KeyCode.Q;
        //Key button used to remove time to current time
        public KeyCode m_incrementDownKey = KeyCode.E;
        //Value to add or remove from current time of day
        [Range(0.001f, 1f)]
        public float m_timeToAddOrRemove = 0.025f;
        //Key button used to rotate the sun left
        public KeyCode m_rotateSunLeftKey = KeyCode.I;
        //Key button used to rotate the sun right
        public KeyCode m_rotateSunRightKey = KeyCode.O;
        //Amount that the sun is rotated
        [Range(0f, 359f)]
        public float m_sunRotationAmount = 15f;
        [Header("Time Of Day Settings")]
        //Used to pause the time of day
        public bool m_pauseTime = false;
        [Range(0f, 1f)]
        public float m_currentTime = 0.5f;
        //Day length
        public float m_dayLengthInSeconds = 300f;
        //Night length
        public float m_nightLengthInSeconds = 150f;
        //Time of day Hour
        [HideInInspector]
        [Range(0, 23)]
        public int m_timeOfDayHour = 11;
        //Time of day Minutes
        [HideInInspector]
        [Range(0, 59)]
        public int m_timeOfDayMinutes = 20;
        //Time of day Seconds
        [HideInInspector]
        [Range(0, 59)]
        public int m_timeOfDaySeconds = 15;
        [Header("Date Settings")]
        //Day
        [Range(1, 31)]
        public int m_day = 4;
        //Month
        [Range(1, 12)]
        public int m_month = 1;
        //Year
        public int m_year = 2013;
        [Header("Light Settings")]
        //Rotates on the 360f
        [Range(0f, 360f)]
        public float m_sunRotation;
        //Day sun intensity during the time of day 0 = midnight  0.5 = midday 1 = start midnight
        public AnimationCurve m_daySunIntensity;
        //Day sun color gradient setting controls the color of the sun during the time of day
        public Gradient m_daySunGradientColor;
        //Night sun intensity during the time of day 0 = midnight  0.5 = midday 1 = start midnight
        public AnimationCurve m_nightSunIntensity;
        //Night sun color gradient setting controls the color of the sun during the time of day
        public Gradient m_nightSunGradientColor;
        [Header("Fog Settings")]
        //Starting fog settings (Linear fog mode)
        public float m_startFogDistance = 20f;
        //Fog day density for exponential and exponential squared fog modes
        public AnimationCurve m_dayFogDensity;
        //Fog night density for exponential and exponential squared fog modes
        public AnimationCurve m_nightFogDensity;
        //Day end fog settings (Linear fog mode) 0 = midnight  0.5 = midday 1 = start midnight
        public AnimationCurve m_dayFogDistance;
        //Day fog color during the time of day 0 = midnight  0.5 = midday 1 = start midnight
        public Gradient m_dayFogColor;
        //Night end fog settings (Linear fog mode) 0 = midnight  0.5 = midday 1 = start midnight
        public AnimationCurve m_nightFogDistance;
        //Night fog color during the time of day 0 = midnight  0.5 = midday 1 = start midnight
        public Gradient m_nightFogColor;
        [Header("Post Processing Settings")]
        //Syncs the tempature of the current post processing profile to time of day
        public bool m_syncPostFXToTimeOfDay = true;
        //Color curve for day
        public Gradient m_dayColor;
        //Color curve for night
        public Gradient m_nightColor;
        //Tempature curve for day
        public AnimationCurve m_dayTempature;
        //Tempature curve for night
        public AnimationCurve m_nightTempature;
        [Header("[Built-In/Lightweight Only] Settings")]
        //Skybox material
        public Material m_skyboxMaterial;
        //Fog Mode To be rendered
        public FogMode m_fogMode = FogMode.Linear;
        [Header("[High Definition Only] Settings")]
#if HDPipeline
        //HDRP volume used to read and apply sky settings
        public VolumeProfile m_hDRPVolumeProfile;
#endif
        //Volumetric sun sizing and blend fade off
        public AnimationCurve m_lightAnisotropy;
        //Volumetric light contribution to the fog
        public AnimationCurve m_lightProbeDimmer;
        //How much fog is multiplied by the and main active light
        public AnimationCurve m_lightDepthExtent;
        //How big the sun size is during day/night
        public AnimationCurve m_sunSize;
        //Sets the skybox exposure based on the time of day
        public AnimationCurve m_skyExposure;

        #endregion

        #region Profile Creating Menu

        /// <summary>
        /// Create sky profile asset
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Ambient Skies/Time Of Day Profile")]
        public static void CreateSkyProfiles()
        {
            AmbientSkiesTimeOfDayProfile asset = ScriptableObject.CreateInstance<AmbientSkiesTimeOfDayProfile>();
            AssetDatabase.CreateAsset(asset, "Assets/Time Of Day Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
#endif

        #endregion
    }
}