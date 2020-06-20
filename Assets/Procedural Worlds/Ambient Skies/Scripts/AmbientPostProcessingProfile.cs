//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#if HDPipeline
using UnityEngine.Experimental.Rendering;
#endif
#endif
#if Mewlist_Clouds
using Mewlist;
#endif
using UnityEngine.Rendering;

namespace AmbientSkies
{
    /// <summary>
    /// Post processing profile for ambient skies
    /// </summary>
    [System.Serializable]
    public class AmbientPostProcessingProfile
    {
        #region Post Processing Default Variables

        //Skybox settings
        public string name;
        public string assetName;
        public int profileIndex;
#if UNITY_POST_PROCESSING_STACK_V2
        public PostProcessProfile creationPostProcessProfile;
#endif

        //Default settings
        public bool defaultEnableEditMode = false;
        public bool defaultHideGizmos = true;
        public bool defaultAutoMatchProfile = true;

        public bool defaultAoEnabled = true;
        public float defaultAoAmount = 1f;
        public Color32 defaultAoColor = new Color32(0, 0, 0, 0);

        public bool defaultAutoExposureEnabled = true;
        public float defaultExposureAmount = 0.85f;
        public float defaultExposureMin = -0.5f;
        public float defaultExposureMax = 0f;

        public bool defaultBloomEnabled = true;
        public float defaultBloomAmount = 2f;
        public float defaultBloomThreshold = 1f;
        public Texture2D defaultLensTexture;
        public float defaultLensIntensity = 1f;

        public bool defaultChromaticAberrationEnabled = true;
        public float defaultChromaticAberrationIntensity = 0.07f;

        public bool defaultColorGradingEnabled = true;
        public Texture2D defaultColorGradingLut;
        public float defaultColorGradingPostExposure = 0.2f;
        public Color32 defaultColorGradingColorFilter = new Color32(255, 255, 255, 255);
        public int defaultColorGradingTempature = 0;
        public int defaultColorGradingTint = 0;
        public float defaultColorGradingSaturation = 5f;
        public float defaultColorGradingContrast = 15f;

        //Channel Mixer
        public int defaultChannelMixerRed = 100;
        public int defaultChannelMixerGreen = 100;
        public int defaultChannelMixerBlue = 100;

        public bool defaultDepthOfFieldEnabled = true;
        public float defaultDepthOfFieldFocusDistance;
        public float defaultDepthOfFieldAperture = 7.5f;
        public float defaultDepthOfFieldFocalLength = 30f;
        public AmbientSkiesConsts.DOFTrackingType defaultDepthOfFieldTrackingType = AmbientSkiesConsts.DOFTrackingType.FollowScreen;
        public float defaultFocusOffset = 0f;
        public LayerMask defaultTargetLayer;
        public float defaultMaxFocusDistance = 1000f;

        public bool defaultDistortionEnabled = true;
        public float defaultDistortionIntensity = 19f;
        public float defaultDistortionScale = 1.02f;

        public bool defaultGrainEnabled = true;
        public float defaultGrainIntensity = 0.1f;
        public float defaultGrainSize = 0.5f;

        public bool defaultScreenSpaceReflectionsEnabled = true;
        public int defaultMaximumIterationCount = 16;
        public float defaultThickness = 8f;

#if UNITY_POST_PROCESSING_STACK_V2
        public PostProcessProfile defaultCustomPostProcessingProfile;
        public GradingMode defaultColorGradingMode = GradingMode.HighDefinitionRange;
        public KernelSize defaultMaxBlurSize = KernelSize.Medium;
        public ScreenSpaceReflectionResolution defaultSpaceReflectionResolution = ScreenSpaceReflectionResolution.Downsampled;
        public ScreenSpaceReflectionPreset defaultScreenSpaceReflectionPreset = ScreenSpaceReflectionPreset.Low;
        public AmbientOcclusionMode defaultAmbientOcclusionMode = AmbientOcclusionMode.MultiScaleVolumetricObscurance;
#endif
        public float defaultMaximumMarchDistance = 250f;
        public float defaultDistanceFade = 0.5f;
        public float defaultScreenSpaceVignette = 0.5f;

        public bool defaultVignetteEnabled = true;
        public float defaultVignetteIntensity = 0.2f;
        public float defaultVignetteSmoothness = 0.7f;

        public bool defaultMotionBlurEnabled = true;
        public int defaultShutterAngle = 142;
        public int defaultSampleCount = 13;

        public AmbientSkiesConsts.AntiAliasingMode defaultAntiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.SMAA;
        public AmbientSkiesConsts.HDRMode defaultHDRMode = AmbientSkiesConsts.HDRMode.On;
        public AmbientSkiesConsts.DepthOfFieldMode defaultDepthOfFieldMode = AmbientSkiesConsts.DepthOfFieldMode.Manual;

#if Mewlist_Clouds
        //Massive Clouds System Variables
        public bool defaultMassiveCloudsEnabled = false;
        public MassiveCloudsProfile defaultCloudProfile;
        public bool defaultSyncGlobalFogColor = true;
        public Color32 defaultCloudsFogColor = new Color32(200, 200, 230, 255);
        public bool defaultSyncBaseFogColor = true;
        public Color32 defaultCloudsBaseFogColor = new Color32(200, 200, 230, 255);
        public bool defaultCloudIsHDRP = false;
#endif
        #endregion

        #region Post Processing Default Variables HDRP

        //Main Settings
        public bool defaultDithering = true;

        //AO
        public float defaultHDRPAOIntensity = 1.2f;
        public float defaultHDRPAOThicknessModifier = 1.7f;
        public float defaultHDRPAODirectLightingStrength = 1f;

        //Bloom
        public float defaultHDRPBloomIntensity = 0.25f;
        public float defaultHDRPBloomScatter = 0.733f;
        public Color32 defaultHDRPBloomTint = new Color32(255, 255, 255, 255);
        public Texture2D defaultHDRPBloomDirtLensTexture;
        public float defaultHDRPBloomDirtLensIntensity = 1f;
        public bool defaultHDRPBloomHighQualityFiltering = true;
        public bool defaultHDRPBloomPrefiler = false;
        public bool defaultHDRPBloomAnamorphic = true;

        //Channel Mixer
        public int defaultHDRPChannelMixerRed = 100;
        public int defaultHDRPChannelMixerGreen = 100;
        public int defaultHDRPChannelMixerBlue = 100;

        //Chromatic Aberration
        public Texture2D defaultHDRPChromaticAberrationSpectralLut;
        public float defaultHDRPChromaticAberrationIntensity = 0.06f;
        public int defaultHDRPChromaticAberrationMaxSamples = 8;

        //Color Adjustments
        public float defaultHDRPColorAdjustmentPostExposure = 1.4f;
        public float defaultHDRPColorAdjustmentContrast = 5f;
        public Color32 defaultHDRPColorAdjustmentColorFilter = new Color32(255, 255, 255, 255);
        public int defaultHDRPColorAdjustmentHueShift = 0;
        public float defaultHDRPColorAdjustmentSaturation = 25f;

        //Color Lookup
        public Texture defaultHDRPColorLookupTexture;
        public float defaultHDRPColorLookupContribution = 1f;

        //Depth Of Field
        public float defaultHDRPDepthOfFieldFocusDistance = 10f;
        public float defaultHDRPDepthOfFieldNearBlurStart = 0f;
        public float defaultHDRPDepthOfFieldNearBlurEnd = 1.2f;
        public int defaultHDRPDepthOfFieldNearBlurSampleCount = 5;
        public float defaultHDRPDepthOfFieldNearBlurMaxRadius = 4f;
        public float defaultHDRPDepthOfFieldFarBlurStart = 0f;
        public float defaultHDRPDepthOfFieldFarBlurEnd = 1.2f;
        public int defaultHDRPDepthOfFieldFarBlurSampleCount = 5;
        public float defaultHDRPDepthOfFieldFarBlurMaxRadius = 4f;
        public bool defaultHDRPDepthOfFieldHighQualityFiltering = true;

        //Film Grain
        public float defualtHDRPFilmGrainIntensity = 0.1f;
        public float defualtHDRPFilmGrainResponse = 0.8f;

        //Lens Distortion
        public float defaultHDRPLensDistortionIntensity = 0f;
        public float defaultHDRPLensDistortionXMultiplier = 1f;
        public float defaultHDRPLensDistortionYMultiplier = 1f;
        public Vector2 defaultHDRPLensDistortionCenter = new Vector2(0.5f, 0.5f);
        public float defaultHDRPLensDistortionScale = 1f;

        //Motion Blur
        public float defaultHDRPMotionBlurIntensity = 0.5f;
        public int defaultHDRPMotionBlurSampleCount = 8;
        public int defaultHDRPMotionBlurMaxVelocity = 250;
        public float defaultHDRPMotionBlurMinVelocity = 2f;
        public float defaultHDRPMotionBlurCameraRotationVelocityClamp = 0.03f;

        //Panini Projection
        public bool defaultHDRPPaniniProjectionEnabled = true;
        public float defaultHDRPPaniniProjectionDistance = 0.02f;
        public float defaultHDRPPaniniProjectionCropToFit = 1f;

        //Shadow Midtones Hightlights
        public float defualtHDRPShadowsMidtonesHighlightsShadowLimitStart = 0f;
        public float defualtHDRPShadowsMidtonesHighlightsShadowLimitEnd = 0.3f;
        public float defualtHDRPShadowsMidtonesHighlightsHightlightLimitStart = 0.55f;
        public float defualtHDRPShadowsMidtonesHighlightsHighlightLimitEnd = 1f;

        //Split Toning
        public Color32 defaultHDRPSplitToningShadows = new Color32(84, 84, 84, 255);
        public Color32 defaultHDRPSplitToningHighlights = new Color32(171, 171, 171, 255);
        public int defaultHDRPSplitToningBalance = -40;

        //Tonemapping
        public float defaultHDRPTonemappingToeStrength = 0f;
        public float defaultHDRPTonemappingToeLength = 0.5f;
        public float defaultHDRPTonemappingShoulderStrength = 0f;
        public float defaultHDRPTonemappingShoulderLength = 0.5f;
        public float defaultHDRPTonemappingShoulderAngle = 0f;
        public float defaultHDRPTonemappingGamma = 1f;

        //Vignette 
        public Texture2D defaultHDRPVignetteMask;
        public float defaultHDRPVignetteMaskOpacity = 1f;
        public Color32 defaultHDRPVignetteColor = new Color32(0, 0, 0, 0);
        public Vector2 defaultHDRPVignetteCenter = new Vector2(0.5f, 0.5f);
        public float defaultHDRPVignetteIntensity = 0.25f;
        public float defaultHDRPVignetteSmoothness = 0.5f;
        public float defaultHDRPVignetteRoundness = 1f;
        public bool defaultHDRPVignetteRounded = false;

        //White Balance
        public int defaultHDRPWhiteBalanceTempature = 3;
        public int defaultHDRPWhiteBalanceTint = 9;

        //Exposure
        public float defaultHDRPExposureCompensation = 15.2f;
        public float defaultHDRPExposureFixedExposure = -0.2f;
        public AnimationCurve defaultHDRPExposureCurveMap;
        public float defaultHDRPExposureLimitMin = -10f;
        public float defaultHDRPExposureLimitMax = 20f;      
        public float defaultHDRPExposureAdaptionSpeedDarkToLight = 3f;
        public float defaultHDRPExposureAdaptionSpeedLightToDark = 1f;

#if HDPipeline && UNITY_2019_1_OR_NEWER
        //HDRP Specific
        public VolumeProfile defaultCustomHDRPPostProcessingprofile;
        public UnityEngine.Experimental.Rendering.HDPipeline.BloomResolution defaultHDRPBloomResolution = UnityEngine.Experimental.Rendering.HDPipeline.BloomResolution.Half;
        public UnityEngine.Experimental.Rendering.HDPipeline.ColorCurves defaultHDRPColorCurves;
        public UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldMode defaultHDRPDepthOfFieldFocusMode = UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldMode.Manual;
        public UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldResolution defaultHDRPDepthOfFieldResolution = UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldResolution.Full;
        public UnityEngine.Experimental.Rendering.HDPipeline.FilmGrainLookup defualtHDRPFilmGrainType = UnityEngine.Experimental.Rendering.HDPipeline.FilmGrainLookup.Thin1;
        public UnityEngine.Experimental.Rendering.HDPipeline.LiftGammaGain defualtHDRPLiftGammaGain;
        public UnityEngine.Experimental.Rendering.HDPipeline.ShadowsMidtonesHighlights defualtHDRPShadowsMidtonesHighlights;
        public UnityEngine.Experimental.Rendering.HDPipeline.TonemappingMode defaultHDRPTonemappingMode = UnityEngine.Experimental.Rendering.HDPipeline.TonemappingMode.ACES;
        public UnityEngine.Experimental.Rendering.HDPipeline.VignetteMode defaultHDRPVignetteMode = UnityEngine.Experimental.Rendering.HDPipeline.VignetteMode.Procedural;
        public UnityEngine.Experimental.Rendering.HDPipeline.MeteringMode defaultHDRPExposureMeteringMode = UnityEngine.Experimental.Rendering.HDPipeline.MeteringMode.CenterWeighted;
        public UnityEngine.Experimental.Rendering.HDPipeline.LuminanceSource defaultHDRPExposureLuminationSource = UnityEngine.Experimental.Rendering.HDPipeline.LuminanceSource.ColorBuffer;
        public UnityEngine.Experimental.Rendering.HDPipeline.AdaptationMode defaultHDRPExposureAdaptionMode = UnityEngine.Experimental.Rendering.HDPipeline.AdaptationMode.Progressive;
        public UnityEngine.Experimental.Rendering.HDPipeline.ExposureMode defaultHDRPExposureMode = UnityEngine.Experimental.Rendering.HDPipeline.ExposureMode.UsePhysicalCamera;
#endif

        #endregion

        #region Post Processing Current Variables
        //Current settings
        public bool enableEditMode = false;
        public bool hideGizmos = true;
        public bool autoMatchProfile = true;

        public bool aoEnabled = true;
        public float aoAmount = 1f;
        public Color32 aoColor = new Color32(0, 0, 0, 0);

        public bool autoExposureEnabled = true;
        public float exposureAmount = 0.85f;
        public float exposureMin = -0.5f;
        public float exposureMax = 0f;

        public bool bloomEnabled = true;
        public float bloomAmount = 2f;
        public float bloomThreshold = 1f;
        public Texture2D lensTexture;
        public float lensIntensity = 1f;

        public bool chromaticAberrationEnabled = true;
        public float chromaticAberrationIntensity = 0.07f;

        public bool colorGradingEnabled = true;
        public Texture2D colorGradingLut;
        public float colorGradingPostExposure = 0.2f;
        public Color32 colorGradingColorFilter = new Color32(255, 255, 255, 255);   
        public int colorGradingTempature = 0;
        public int colorGradingTint = 0;
        public float colorGradingSaturation = 5f;
        public float colorGradingContrast = 15f;

        //Channel Mixer
        public int channelMixerRed = 100;
        public int channelMixerGreen = 100;
        public int channelMixerBlue = 100;

        public bool depthOfFieldEnabled = true;
        public float depthOfFieldFocusDistance = 1f;
        public float depthOfFieldAperture = 7.5f;
        public float depthOfFieldFocalLength = 30f;
        public AmbientSkiesConsts.DOFTrackingType depthOfFieldTrackingType = AmbientSkiesConsts.DOFTrackingType.FollowScreen;
        public float focusOffset = 0f;
        public LayerMask targetLayer = 1;
        public float maxFocusDistance = 1000f;

        public bool distortionEnabled = true;
        public float distortionIntensity = 19f;
        public float distortionScale = 1.02f;

        public bool grainEnabled = true;
        public float grainIntensity = 0.1f;
        public float grainSize = 0.5f;

        public bool screenSpaceReflectionsEnabled = true;
        public int maximumIterationCount = 16;
        public float thickness = 8f;

#if UNITY_POST_PROCESSING_STACK_V2
        public GradingMode colorGradingMode = GradingMode.HighDefinitionRange;
        public PostProcessProfile customPostProcessingProfile;
        public KernelSize maxBlurSize = KernelSize.Medium;
        public ScreenSpaceReflectionResolution spaceReflectionResolution = ScreenSpaceReflectionResolution.Downsampled;
        public ScreenSpaceReflectionPreset screenSpaceReflectionPreset = ScreenSpaceReflectionPreset.Low;
        public AmbientOcclusionMode ambientOcclusionMode = AmbientOcclusionMode.MultiScaleVolumetricObscurance;
#endif
        public float maximumMarchDistance = 250f;
        public float distanceFade = 0.5f;
        public float screenSpaceVignette = 0.5f;

        public bool vignetteEnabled = true;
        public float vignetteIntensity = 0.2f;
        public float vignetteSmoothness = 0.7f;

        public bool motionBlurEnabled = true;
        public int shutterAngle = 142;
        public int sampleCount = 13;

        public AmbientSkiesConsts.AntiAliasingMode antiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.SMAA;
        public AmbientSkiesConsts.HDRMode hDRMode = AmbientSkiesConsts.HDRMode.On;
        public AmbientSkiesConsts.DepthOfFieldMode depthOfFieldMode = AmbientSkiesConsts.DepthOfFieldMode.Manual;

#if Mewlist_Clouds
        //Massive Clouds System Variables
        public bool massiveCloudsEnabled = false;
        public MassiveCloudsProfile cloudProfile;
        public bool syncGlobalFogColor = true;
        public Color32 cloudsFogColor = new Color32(200, 200, 230, 255);
        public bool syncBaseFogColor = true;
        public Color32 cloudsBaseFogColor = new Color32(200, 200, 230, 255);
        public bool cloudIsHDRP = false;
#endif
        #endregion

        #region Post Processing Current Variables HDRP

        //Main Settings
        public bool dithering = true;

        //AO
        public float hDRPAOIntensity = 1.2f;
        public float hDRPAOThicknessModifier = 1.7f;
        public float hDRPAODirectLightingStrength = 1f;

        //Bloom
        public float hDRPBloomIntensity = 0.25f;
        public float hDRPBloomScatter = 0.733f;
        public Color32 hDRPBloomTint = new Color32(255, 255, 255, 255);
        public Texture2D hDRPBloomDirtLensTexture;
        public float hDRPBloomDirtLensIntensity = 1f;
        public bool hDRPBloomHighQualityFiltering = true;
        public bool hDRPBloomPrefiler = false;
        public bool hDRPBloomAnamorphic = true;

        //Channel Mixer
        public int hDRPChannelMixerRed = 100;
        public int hDRPChannelMixerGreen = 100;
        public int hDRPChannelMixerBlue = 100;

        //Chromatic Aberration
        public Texture2D hDRPChromaticAberrationSpectralLut;
        public float hDRPChromaticAberrationIntensity = 0.06f;
        public int hDRPChromaticAberrationMaxSamples = 8;

        //Color Adjustments
        public float hDRPColorAdjustmentPostExposure = 1.4f;
        public float hDRPColorAdjustmentContrast = 5f;
        public Color32 hDRPColorAdjustmentColorFilter = new Color32(255, 255, 255, 255);
        public int hDRPColorAdjustmentHueShift = 0;
        public float hDRPColorAdjustmentSaturation = 25f;


        //Color Lookup
        public Texture hDRPColorLookupTexture;
        public float hDRPColorLookupContribution = 1f;

        //Depth Of Field
        public float hDRPDepthOfFieldFocusDistance = 10f;
        public float hDRPDepthOfFieldNearBlurStart = 0f;
        public float hDRPDepthOfFieldNearBlurEnd = 1.2f;
        public int hDRPDepthOfFieldNearBlurSampleCount = 5;
        public float hDRPDepthOfFieldNearBlurMaxRadius = 4f;
        public float hDRPDepthOfFieldFarBlurStart = 0f;
        public float hDRPDepthOfFieldFarBlurEnd = 1.2f;
        public int hDRPDepthOfFieldFarBlurSampleCount = 5;
        public float hDRPDepthOfFieldFarBlurMaxRadius = 4f;
        public bool hDRPDepthOfFieldHighQualityFiltering = true;

        //Film Grain
        public float hDRPFilmGrainIntensity = 0.1f;
        public float hDRPFilmGrainResponse = 0.8f;

        //Lens Distortion
        public float hDRPLensDistortionIntensity = 0f;
        public float hDRPLensDistortionXMultiplier = 1f;
        public float hDRPLensDistortionYMultiplier = 1f;
        public Vector2 hDRPLensDistortionCenter = new Vector2(0.5f, 0.5f);
        public float hDRPLensDistortionScale = 1f;

        //Motion Blur
        public float hDRPMotionBlurIntensity = 0.5f;
        public int hDRPMotionBlurSampleCount = 8;
        public int hDRPMotionBlurMaxVelocity = 250;
        public float hDRPMotionBlurMinVelocity = 2f;
        public float hDRPMotionBlurCameraRotationVelocityClamp = 0.03f;

        //Panini Projection
        public bool hDRPPaniniProjectionEnabled = true;
        public float hDRPPaniniProjectionDistance = 0.02f;
        public float hDRPPaniniProjectionCropToFit = 1f;

        //Shadow Midtones Hightlights
        public float hDRPShadowsMidtonesHighlightsShadowLimitStart = 0f;
        public float hDRPShadowsMidtonesHighlightsShadowLimitEnd = 0.3f;
        public float hDRPShadowsMidtonesHighlightsHightlightLimitStart = 0.55f;
        public float hDRPShadowsMidtonesHighlightsHighlightLimitEnd = 1f;

        //Split Toning
        public Color32 hDRPSplitToningShadows = new Color32(84, 84, 84, 255);
        public Color32 hDRPSplitToningHighlights = new Color32(171, 171, 171, 255);
        public int hDRPSplitToningBalance = -40;

        //Tonemapping
        public float hDRPTonemappingToeStrength = 0f;
        public float hDRPTonemappingToeLength = 0.5f;
        public float hDRPTonemappingShoulderStrength = 0f;
        public float hDRPTonemappingShoulderLength = 0.5f;
        public float hDRPTonemappingShoulderAngle = 0f;
        public float hDRPTonemappingGamma = 1f;

        //Vignette 
        public Texture2D hDRPVignetteMask;
        public float hDRPVignetteMaskOpacity = 1f;
        public Color32 hDRPVignetteColor = new Color32(0, 0, 0, 0);
        public Vector2 hDRPVignetteCenter = new Vector2(0.5f, 0.5f);
        public float hDRPVignetteIntensity = 0.25f;
        public float hDRPVignetteSmoothness = 0.5f;
        public float hDRPVignetteRoundness = 1f;
        public bool hDRPVignetteRounded = false;

        //White Balance
        public int hDRPWhiteBalanceTempature = 3;
        public int hDRPWhiteBalanceTint = 9;

        //Exposure
        public float hDRPExposureCompensation = 15.2f;
        public float hDRPExposureFixedExposure = -0.2f;
        public float hDRPExposureLimitMin = -10f;
        public float hDRPExposureLimitMax = 20f;
        public float hDRPExposureAdaptionSpeedDarkToLight = 3f;
        public float hDRPExposureAdaptionSpeedLightToDark = 1f;

#if HDPipeline && UNITY_2019_1_OR_NEWER
        //HDRP Specific
        public VolumeProfile customHDRPPostProcessingprofile;
        public UnityEngine.Experimental.Rendering.HDPipeline.BloomResolution hDRPBloomResolution = UnityEngine.Experimental.Rendering.HDPipeline.BloomResolution.Half;
        public UnityEngine.Experimental.Rendering.HDPipeline.ColorCurves hDRPColorCurves;
        public UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldMode hDRPDepthOfFieldFocusMode = UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldMode.Manual;
        public UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldResolution hDRPDepthOfFieldResolution = UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldResolution.Full;
        public UnityEngine.Experimental.Rendering.HDPipeline.FilmGrainLookup hDRPFilmGrainType = UnityEngine.Experimental.Rendering.HDPipeline.FilmGrainLookup.Thin1;
        public UnityEngine.Experimental.Rendering.HDPipeline.LiftGammaGain hDRPLiftGammaGain;
        public UnityEngine.Experimental.Rendering.HDPipeline.ShadowsMidtonesHighlights hDRPShadowsMidtonesHighlights;
        public UnityEngine.Experimental.Rendering.HDPipeline.TonemappingMode hDRPTonemappingMode = UnityEngine.Experimental.Rendering.HDPipeline.TonemappingMode.ACES;
        public UnityEngine.Experimental.Rendering.HDPipeline.VignetteMode hDRPVignetteMode = UnityEngine.Experimental.Rendering.HDPipeline.VignetteMode.Procedural;
        public UnityEngine.Experimental.Rendering.HDPipeline.ExposureMode hDRPExposureMode = UnityEngine.Experimental.Rendering.HDPipeline.ExposureMode.UsePhysicalCamera;
        public UnityEngine.Experimental.Rendering.HDPipeline.MeteringMode hDRPExposureMeteringMode = UnityEngine.Experimental.Rendering.HDPipeline.MeteringMode.CenterWeighted;
        public UnityEngine.Experimental.Rendering.HDPipeline.LuminanceSource hDRPExposureLuminationSource = UnityEngine.Experimental.Rendering.HDPipeline.LuminanceSource.ColorBuffer;
        public UnityEngine.Experimental.Rendering.HDPipeline.AdaptationMode hDRPExposureAdaptionMode = UnityEngine.Experimental.Rendering.HDPipeline.AdaptationMode.Progressive;
        public AnimationCurve hDRPExposureCurveMap;
#endif

        #endregion

        #region Default Setups
        /// <summary>
        /// Revert current settings back to default settings
        /// </summary>
        public void RevertToDefault(AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
            if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                #region Built-In/LWRP

                enableEditMode = defaultEnableEditMode;
                hideGizmos = defaultHideGizmos;
                hDRMode = defaultHDRMode;
                depthOfFieldMode = defaultDepthOfFieldMode;
                autoMatchProfile = defaultAutoMatchProfile;

                aoEnabled = defaultAoEnabled;
                aoAmount = defaultAoAmount;
                aoColor = defaultAoColor;

                autoExposureEnabled = defaultAutoExposureEnabled;
                exposureAmount = defaultExposureAmount;
                exposureMin = defaultExposureMin;
                exposureMax = defaultExposureMax;

                bloomEnabled = defaultBloomEnabled;
                bloomAmount = defaultBloomAmount;
                bloomThreshold = defaultBloomThreshold;
                lensTexture = defaultLensTexture;
                lensIntensity = defaultLensIntensity;

                chromaticAberrationEnabled = defaultChromaticAberrationEnabled;
                chromaticAberrationIntensity = defaultChromaticAberrationIntensity;

                colorGradingEnabled = defaultColorGradingEnabled;
                colorGradingLut = defaultColorGradingLut;
                colorGradingColorFilter = defaultColorGradingColorFilter;
                colorGradingPostExposure = defaultColorGradingPostExposure;
                colorGradingTempature = defaultColorGradingTempature;
                colorGradingTint = defaultColorGradingTint;
                colorGradingSaturation = defaultColorGradingSaturation;
                colorGradingContrast = defaultColorGradingContrast;
                channelMixerRed = defaultChannelMixerRed;
                channelMixerBlue = defaultChannelMixerBlue;
                channelMixerGreen = defaultChannelMixerGreen;

                depthOfFieldEnabled = defaultDepthOfFieldEnabled;
                depthOfFieldFocusDistance = defaultDepthOfFieldFocusDistance;
                depthOfFieldAperture = defaultDepthOfFieldAperture;
                depthOfFieldFocalLength = defaultDepthOfFieldFocalLength;
                depthOfFieldTrackingType = defaultDepthOfFieldTrackingType;
                focusOffset = defaultFocusOffset;
                targetLayer = defaultTargetLayer;
                maxFocusDistance = defaultMaxFocusDistance;

                distortionEnabled = defaultDistortionEnabled;
                distortionIntensity = defaultDistortionIntensity;
                distortionScale = defaultDistortionScale;

                grainEnabled = defaultGrainEnabled;
                grainIntensity = defaultGrainIntensity;
                grainSize = defaultGrainSize;

                screenSpaceReflectionsEnabled = defaultScreenSpaceReflectionsEnabled;
                maximumIterationCount = defaultMaximumIterationCount;
                thickness = defaultThickness;
#if UNITY_POST_PROCESSING_STACK_V2
                colorGradingMode = defaultColorGradingMode;
                customPostProcessingProfile = defaultCustomPostProcessingProfile;
                maxBlurSize = defaultMaxBlurSize;
                spaceReflectionResolution = defaultSpaceReflectionResolution;
                screenSpaceReflectionPreset = defaultScreenSpaceReflectionPreset;
                ambientOcclusionMode = defaultAmbientOcclusionMode;
#endif
                maximumMarchDistance = defaultMaximumMarchDistance;
                distanceFade = defaultDistanceFade;
                screenSpaceVignette = defaultScreenSpaceVignette;

                vignetteEnabled = defaultVignetteEnabled;
                vignetteIntensity = defaultVignetteIntensity;
                vignetteSmoothness = defaultVignetteSmoothness;

                motionBlurEnabled = defaultMotionBlurEnabled;
                shutterAngle = defaultShutterAngle;
                sampleCount = defaultSampleCount;

                antiAliasingMode = defaultAntiAliasingMode;

#if Mewlist_Clouds
                //Massive Clouds System Variables
                massiveCloudsEnabled = defaultMassiveCloudsEnabled;
                cloudProfile = defaultCloudProfile;
                syncGlobalFogColor = defaultSyncGlobalFogColor;
                cloudsFogColor = defaultCloudsFogColor;
                syncBaseFogColor = defaultSyncBaseFogColor;
                cloudsBaseFogColor = defaultCloudsBaseFogColor;
#endif

                #endregion
            }
            else
            {
                #region HDRP Post Processing Values

#if HDPipeline && UNITY_2019_1_OR_NEWER

                hideGizmos = defaultHideGizmos;
                antiAliasingMode = defaultAntiAliasingMode;
                dithering = defaultDithering;
                hDRMode = defaultHDRMode;
                customHDRPPostProcessingprofile = defaultCustomHDRPPostProcessingprofile;
                autoMatchProfile = defaultAutoMatchProfile;

                aoEnabled = defaultAoEnabled;
                hDRPAOIntensity = defaultHDRPAOIntensity;
                hDRPAOThicknessModifier = defaultHDRPAOThicknessModifier;
                hDRPAODirectLightingStrength = defaultHDRPAODirectLightingStrength;

                autoExposureEnabled = defaultAutoExposureEnabled;
                hDRPExposureMode = defaultHDRPExposureMode;
                hDRPExposureMeteringMode = defaultHDRPExposureMeteringMode;
                hDRPExposureLuminationSource = defaultHDRPExposureLuminationSource;
                hDRPExposureFixedExposure = defaultHDRPExposureFixedExposure;
                hDRPExposureCurveMap = defaultHDRPExposureCurveMap;
                hDRPExposureCompensation = defaultHDRPExposureCompensation;
                hDRPExposureLimitMin = defaultHDRPExposureLimitMin;
                hDRPExposureLimitMax = defaultHDRPExposureLimitMax;
                hDRPExposureAdaptionMode = defaultHDRPExposureAdaptionMode;
                hDRPExposureAdaptionSpeedDarkToLight = defaultHDRPExposureAdaptionSpeedDarkToLight;
                hDRPExposureAdaptionSpeedLightToDark = defaultHDRPExposureAdaptionSpeedLightToDark;

                bloomEnabled = defaultBloomEnabled;
                hDRPBloomIntensity = defaultHDRPBloomIntensity;
                hDRPBloomScatter = defaultHDRPBloomScatter;
                hDRPBloomTint = defaultHDRPBloomTint;
                hDRPBloomDirtLensTexture = defaultHDRPBloomDirtLensTexture;
                hDRPBloomDirtLensIntensity = defaultHDRPBloomDirtLensIntensity;
                hDRPBloomResolution = defaultHDRPBloomResolution;
                hDRPBloomHighQualityFiltering = defaultHDRPBloomHighQualityFiltering;
                hDRPBloomPrefiler = defaultHDRPBloomPrefiler;
                hDRPBloomAnamorphic = defaultHDRPBloomAnamorphic;

                chromaticAberrationEnabled = defaultChromaticAberrationEnabled;
                hDRPChromaticAberrationSpectralLut = defaultHDRPChromaticAberrationSpectralLut;
                hDRPChromaticAberrationIntensity = defaultHDRPChromaticAberrationIntensity;
                hDRPChromaticAberrationMaxSamples = defaultHDRPChromaticAberrationMaxSamples;

                colorGradingEnabled = defaultColorGradingEnabled;
                hDRPColorLookupTexture = defaultHDRPColorLookupTexture;
                hDRPColorAdjustmentColorFilter = defaultHDRPColorAdjustmentColorFilter;
                hDRPColorAdjustmentPostExposure = defaultHDRPColorAdjustmentPostExposure;
                hDRPWhiteBalanceTempature = defaultHDRPWhiteBalanceTempature;
                hDRPColorLookupContribution = defaultHDRPColorLookupContribution;
                hDRPWhiteBalanceTint = defaultHDRPWhiteBalanceTint;
                hDRPColorAdjustmentSaturation = defaultHDRPColorAdjustmentSaturation;
                hDRPColorAdjustmentContrast = defaultHDRPColorAdjustmentContrast;
                hDRPChannelMixerRed = defaultHDRPChannelMixerRed;
                hDRPChannelMixerGreen = defaultHDRPChannelMixerGreen;
                hDRPChannelMixerBlue = defaultHDRPChannelMixerBlue;
                hDRPTonemappingMode = defaultHDRPTonemappingMode;
                hDRPTonemappingToeStrength = defaultHDRPTonemappingToeStrength;
                hDRPTonemappingToeLength = defaultHDRPTonemappingToeLength;
                hDRPTonemappingShoulderStrength = defaultHDRPTonemappingShoulderStrength;
                hDRPTonemappingShoulderLength = defaultHDRPTonemappingShoulderLength;
                hDRPTonemappingShoulderAngle = defaultHDRPTonemappingShoulderAngle;
                hDRPTonemappingGamma = defaultHDRPTonemappingGamma;
                hDRPSplitToningShadows = defaultHDRPSplitToningShadows;
                hDRPSplitToningHighlights = defaultHDRPSplitToningHighlights;
                hDRPSplitToningBalance = defaultHDRPSplitToningBalance;

                depthOfFieldEnabled = defaultDepthOfFieldEnabled;
                hDRPDepthOfFieldFocusMode = defaultHDRPDepthOfFieldFocusMode;
                hDRPDepthOfFieldNearBlurStart = defaultHDRPDepthOfFieldNearBlurStart;
                hDRPDepthOfFieldNearBlurEnd = defaultHDRPDepthOfFieldNearBlurEnd;
                hDRPDepthOfFieldNearBlurSampleCount = defaultHDRPDepthOfFieldNearBlurSampleCount;
                hDRPDepthOfFieldNearBlurMaxRadius = defaultHDRPDepthOfFieldNearBlurMaxRadius;
                hDRPDepthOfFieldFarBlurStart = defaultHDRPDepthOfFieldFarBlurStart;
                hDRPDepthOfFieldFarBlurEnd = defaultHDRPDepthOfFieldFarBlurEnd;
                hDRPDepthOfFieldFarBlurSampleCount = defaultHDRPDepthOfFieldFarBlurSampleCount;
                hDRPDepthOfFieldFarBlurMaxRadius = defaultHDRPDepthOfFieldFarBlurMaxRadius;
                hDRPDepthOfFieldResolution = defaultHDRPDepthOfFieldResolution;
                hDRPDepthOfFieldHighQualityFiltering = defaultHDRPDepthOfFieldHighQualityFiltering;

                grainEnabled = defaultGrainEnabled;
                hDRPFilmGrainType = defualtHDRPFilmGrainType;
                hDRPFilmGrainIntensity = defualtHDRPFilmGrainIntensity;
                hDRPFilmGrainResponse = defualtHDRPFilmGrainResponse;

                distortionEnabled = defaultDistortionEnabled;
                hDRPLensDistortionIntensity = defaultHDRPLensDistortionIntensity;
                hDRPLensDistortionXMultiplier = defaultHDRPLensDistortionXMultiplier;
                hDRPLensDistortionYMultiplier = defaultHDRPLensDistortionYMultiplier;
                hDRPLensDistortionCenter = defaultHDRPLensDistortionCenter;
                hDRPLensDistortionScale = defaultHDRPLensDistortionScale;

                vignetteEnabled = defaultVignetteEnabled;
                hDRPVignetteMode = defaultHDRPVignetteMode;
                hDRPVignetteColor = defaultHDRPVignetteColor;
                hDRPVignetteCenter = defaultHDRPVignetteCenter;
                hDRPVignetteIntensity = defaultHDRPVignetteIntensity;
                hDRPVignetteSmoothness = defaultHDRPVignetteSmoothness;
                hDRPVignetteRoundness = defaultHDRPVignetteRoundness;
                hDRPVignetteRounded = defaultHDRPVignetteRounded;
                hDRPVignetteMask = defaultHDRPVignetteMask;
                hDRPVignetteMaskOpacity = defaultHDRPVignetteMaskOpacity;

                motionBlurEnabled = defaultMotionBlurEnabled;
                hDRPMotionBlurIntensity = defaultHDRPMotionBlurIntensity;
                hDRPMotionBlurSampleCount = defaultHDRPMotionBlurSampleCount;
                hDRPMotionBlurMaxVelocity = defaultHDRPMotionBlurMaxVelocity;
                hDRPMotionBlurMinVelocity = defaultHDRPMotionBlurMinVelocity;
                hDRPMotionBlurCameraRotationVelocityClamp = defaultHDRPMotionBlurCameraRotationVelocityClamp;

                hDRPPaniniProjectionEnabled = defaultHDRPPaniniProjectionEnabled;
                hDRPPaniniProjectionDistance = defaultHDRPPaniniProjectionDistance;
                hDRPPaniniProjectionCropToFit = defaultHDRPPaniniProjectionCropToFit;

#endif

                #endregion
            }
        }

        /// <summary>
        /// Save current settings to default settings
        /// </summary>
        public void SaveCurrentToDefault(AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
            if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                #region Built-In/LWRP

                defaultEnableEditMode = enableEditMode;
                defaultHideGizmos = hideGizmos;
                defaultAutoMatchProfile = autoMatchProfile;

                defaultHDRMode = hDRMode;
                defaultDepthOfFieldMode = depthOfFieldMode;

                defaultAoEnabled = aoEnabled;
                defaultAoAmount = aoAmount;
                defaultAoColor = aoColor;

                defaultAutoExposureEnabled = autoExposureEnabled;
                defaultExposureAmount = exposureAmount;

                defaultBloomEnabled = bloomEnabled;
                defaultBloomAmount = bloomAmount;
                defaultBloomThreshold = bloomThreshold;
                defaultLensTexture = lensTexture;
                defaultLensIntensity = lensIntensity;

                defaultChromaticAberrationEnabled = chromaticAberrationEnabled;
                defaultChromaticAberrationIntensity = chromaticAberrationIntensity;

                defaultColorGradingEnabled = colorGradingEnabled;
                defaultColorGradingLut = colorGradingLut;
                defaultColorGradingColorFilter = colorGradingColorFilter;
                defaultColorGradingPostExposure = colorGradingPostExposure;
                defaultColorGradingTempature = colorGradingTempature;
                defaultColorGradingTint = colorGradingTint;
                defaultColorGradingSaturation = colorGradingSaturation;
                defaultColorGradingContrast = colorGradingContrast;
                defaultChannelMixerRed = channelMixerRed;
                defaultChannelMixerBlue = channelMixerBlue;
                defaultChannelMixerGreen = channelMixerGreen;

                defaultDepthOfFieldEnabled = depthOfFieldEnabled;
                defaultDepthOfFieldFocusDistance = depthOfFieldFocusDistance;
                defaultDepthOfFieldAperture = depthOfFieldAperture;
                defaultDepthOfFieldFocalLength = depthOfFieldFocalLength;
                defaultDepthOfFieldTrackingType = depthOfFieldTrackingType;

                defaultDistortionEnabled = distortionEnabled;
                defaultDistortionIntensity = distortionIntensity;
                defaultDistortionScale = distortionScale;

                defaultGrainEnabled = grainEnabled;
                defaultGrainIntensity = grainIntensity;
                defaultGrainSize = grainSize;

                defaultScreenSpaceReflectionsEnabled = screenSpaceReflectionsEnabled;
                defaultMaximumIterationCount = maximumIterationCount;
                defaultThickness = thickness;
#if UNITY_POST_PROCESSING_STACK_V2
                defaultColorGradingMode = colorGradingMode;
                defaultCustomPostProcessingProfile = customPostProcessingProfile;
                defaultMaxBlurSize = maxBlurSize;
                defaultSpaceReflectionResolution = spaceReflectionResolution;
                defaultScreenSpaceReflectionPreset = screenSpaceReflectionPreset;
                defaultAmbientOcclusionMode = ambientOcclusionMode;
#endif
                defaultMaximumMarchDistance = maximumMarchDistance;
                defaultDistanceFade = distanceFade;
                defaultScreenSpaceVignette = screenSpaceVignette;

                defaultVignetteEnabled = vignetteEnabled;
                defaultVignetteIntensity = vignetteIntensity;
                defaultVignetteSmoothness = vignetteSmoothness;

                defaultMotionBlurEnabled = motionBlurEnabled;
                defaultShutterAngle = shutterAngle;
                defaultSampleCount = sampleCount;

                defaultAntiAliasingMode = antiAliasingMode;

#if Mewlist_Clouds
                //Massive Clouds System Variables
                defaultMassiveCloudsEnabled = massiveCloudsEnabled;
                defaultCloudProfile = cloudProfile;
                defaultSyncGlobalFogColor = syncGlobalFogColor;
                defaultCloudsFogColor = cloudsFogColor;
                defaultSyncBaseFogColor = syncBaseFogColor;
                defaultCloudsBaseFogColor = cloudsBaseFogColor;
#endif

                #endregion
            }
            else
            {
                #region HDRP Post Processing Values

#if HDPipeline && UNITY_2019_1_OR_NEWER

                defaultEnableEditMode = enableEditMode;
                defaultHideGizmos = hideGizmos;
                defaultAutoMatchProfile = autoMatchProfile;

                defaultAntiAliasingMode = antiAliasingMode;
                defaultDithering = dithering;
                defaultHDRMode = hDRMode;
                defaultCustomHDRPPostProcessingprofile = customHDRPPostProcessingprofile;

                defaultAoEnabled = aoEnabled;
                defaultHDRPAOIntensity = hDRPAOIntensity;
                defaultHDRPAOThicknessModifier = hDRPAOThicknessModifier;
                defaultHDRPAODirectLightingStrength = hDRPAODirectLightingStrength;

                defaultAutoExposureEnabled = autoExposureEnabled;
                defaultHDRPExposureMode = hDRPExposureMode;
                defaultHDRPExposureMeteringMode = hDRPExposureMeteringMode;
                defaultHDRPExposureLuminationSource = hDRPExposureLuminationSource;
                defaultHDRPExposureFixedExposure = hDRPExposureFixedExposure;
                defaultHDRPExposureCurveMap = hDRPExposureCurveMap;
                defaultHDRPExposureCompensation = hDRPExposureCompensation;
                defaultHDRPExposureLimitMin = hDRPExposureLimitMin;
                defaultHDRPExposureLimitMax = hDRPExposureLimitMax;
                defaultHDRPExposureAdaptionMode = hDRPExposureAdaptionMode;
                defaultHDRPExposureAdaptionSpeedDarkToLight = hDRPExposureAdaptionSpeedDarkToLight;
                defaultHDRPExposureAdaptionSpeedLightToDark = hDRPExposureAdaptionSpeedLightToDark;

                defaultBloomEnabled = bloomEnabled;
                defaultHDRPBloomIntensity = hDRPBloomIntensity;
                defaultHDRPBloomScatter = hDRPBloomScatter;
                defaultHDRPBloomTint = hDRPBloomTint;
                defaultHDRPBloomDirtLensTexture = hDRPBloomDirtLensTexture;
                defaultHDRPBloomDirtLensIntensity = hDRPBloomDirtLensIntensity;
                defaultHDRPBloomResolution = hDRPBloomResolution;
                defaultHDRPBloomHighQualityFiltering = hDRPBloomHighQualityFiltering;
                defaultHDRPBloomPrefiler = hDRPBloomPrefiler;
                defaultHDRPBloomAnamorphic = hDRPBloomAnamorphic;

                defaultChromaticAberrationEnabled = chromaticAberrationEnabled;
                defaultHDRPChromaticAberrationSpectralLut = hDRPChromaticAberrationSpectralLut;
                defaultHDRPChromaticAberrationIntensity = hDRPChromaticAberrationIntensity;
                defaultHDRPChromaticAberrationMaxSamples = hDRPChromaticAberrationMaxSamples;

                defaultColorGradingEnabled = colorGradingEnabled;
                defaultHDRPColorLookupTexture = hDRPColorLookupTexture;
                defaultHDRPColorAdjustmentColorFilter = hDRPColorAdjustmentColorFilter;
                defaultHDRPColorAdjustmentPostExposure = hDRPColorAdjustmentPostExposure;
                defaultHDRPWhiteBalanceTempature = hDRPWhiteBalanceTempature;
                defaultHDRPColorLookupContribution = hDRPColorLookupContribution;
                defaultHDRPWhiteBalanceTint = hDRPWhiteBalanceTint;
                defaultHDRPColorAdjustmentSaturation = hDRPColorAdjustmentSaturation;
                defaultHDRPColorAdjustmentContrast = hDRPColorAdjustmentContrast;
                defaultHDRPChannelMixerRed = hDRPChannelMixerRed;
                defaultHDRPChannelMixerGreen = hDRPChannelMixerGreen;
                defaultHDRPChannelMixerBlue = hDRPChannelMixerBlue;
                defaultHDRPTonemappingMode = hDRPTonemappingMode;
                defaultHDRPTonemappingToeStrength = hDRPTonemappingToeStrength;
                defaultHDRPTonemappingToeLength = hDRPTonemappingToeLength;
                defaultHDRPTonemappingShoulderStrength = hDRPTonemappingShoulderStrength;
                defaultHDRPTonemappingShoulderLength = hDRPTonemappingShoulderLength;
                defaultHDRPTonemappingShoulderAngle = hDRPTonemappingShoulderAngle;
                defaultHDRPTonemappingGamma = hDRPTonemappingGamma;
                defaultHDRPSplitToningShadows = hDRPSplitToningShadows;
                defaultHDRPSplitToningHighlights = hDRPSplitToningHighlights;
                defaultHDRPSplitToningBalance = hDRPSplitToningBalance;

                defaultDepthOfFieldEnabled = depthOfFieldEnabled;
                defaultHDRPDepthOfFieldFocusMode = hDRPDepthOfFieldFocusMode;
                defaultHDRPDepthOfFieldNearBlurStart = hDRPDepthOfFieldNearBlurStart;
                defaultHDRPDepthOfFieldNearBlurEnd = hDRPDepthOfFieldNearBlurEnd;
                defaultHDRPDepthOfFieldNearBlurSampleCount = hDRPDepthOfFieldNearBlurSampleCount;
                defaultHDRPDepthOfFieldNearBlurMaxRadius = hDRPDepthOfFieldNearBlurMaxRadius;
                defaultHDRPDepthOfFieldFarBlurStart = hDRPDepthOfFieldFarBlurStart;
                defaultHDRPDepthOfFieldFarBlurEnd = hDRPDepthOfFieldFarBlurEnd;
                defaultHDRPDepthOfFieldFarBlurSampleCount = hDRPDepthOfFieldFarBlurSampleCount;
                defaultHDRPDepthOfFieldFarBlurMaxRadius = hDRPDepthOfFieldFarBlurMaxRadius;
                defaultHDRPDepthOfFieldResolution = hDRPDepthOfFieldResolution;
                defaultHDRPDepthOfFieldHighQualityFiltering = hDRPDepthOfFieldHighQualityFiltering;

                defaultGrainEnabled = grainEnabled;
                defualtHDRPFilmGrainType = hDRPFilmGrainType;
                defualtHDRPFilmGrainIntensity = hDRPFilmGrainIntensity;
                defualtHDRPFilmGrainResponse = hDRPFilmGrainResponse;

                defaultDistortionEnabled = distortionEnabled;
                defaultHDRPLensDistortionIntensity = hDRPLensDistortionIntensity;
                defaultHDRPLensDistortionXMultiplier = hDRPLensDistortionXMultiplier;
                defaultHDRPLensDistortionYMultiplier = hDRPLensDistortionYMultiplier;
                defaultHDRPLensDistortionCenter = hDRPLensDistortionCenter;
                defaultHDRPLensDistortionScale = hDRPLensDistortionScale;

                defaultVignetteEnabled = vignetteEnabled;
                defaultHDRPVignetteMode = hDRPVignetteMode;
                defaultHDRPVignetteColor = hDRPVignetteColor;
                defaultHDRPVignetteCenter = hDRPVignetteCenter;
                defaultHDRPVignetteIntensity = hDRPVignetteIntensity;
                defaultHDRPVignetteSmoothness = hDRPVignetteSmoothness;
                defaultHDRPVignetteRoundness = hDRPVignetteRoundness;
                defaultHDRPVignetteRounded = hDRPVignetteRounded;
                defaultHDRPVignetteMask = hDRPVignetteMask;
                defaultHDRPVignetteMaskOpacity = hDRPVignetteMaskOpacity;

                defaultMotionBlurEnabled = motionBlurEnabled;
                hDRPMotionBlurIntensity = defaultHDRPMotionBlurIntensity;
                defaultHDRPMotionBlurSampleCount = hDRPMotionBlurSampleCount;
                defaultHDRPMotionBlurMaxVelocity = hDRPMotionBlurMaxVelocity;
                defaultHDRPMotionBlurMinVelocity = hDRPMotionBlurMinVelocity;
                defaultHDRPMotionBlurCameraRotationVelocityClamp = hDRPMotionBlurCameraRotationVelocityClamp;

                defaultHDRPPaniniProjectionEnabled = hDRPPaniniProjectionEnabled;
                defaultHDRPPaniniProjectionDistance = hDRPPaniniProjectionDistance;
                defaultHDRPPaniniProjectionCropToFit = hDRPPaniniProjectionCropToFit;

#endif

                #endregion
            }
        }
        #endregion
    }
}