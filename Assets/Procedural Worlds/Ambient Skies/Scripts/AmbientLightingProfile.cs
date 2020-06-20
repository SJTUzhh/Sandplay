//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AmbientSkies
{
    /// <summary>
    /// Settings for an ambient lightmapping settings
    /// </summary>
    [System.Serializable]
    public class AmbientLightingProfile
    {
        #region Lightmaps Defaults Variables

        //Lightmapping settings
        public string name;
        public int profileIndex;
        public string assetName;

        //Default lightmapping settings
        public bool deaultShowLightmapSettings;
        public bool defaultRealtimeGlobalIllumination;
        public bool defaultBakedGlobalIllumination;
        public AmbientSkiesConsts.LightmapperMode defaultLightmappingMode = AmbientSkiesConsts.LightmapperMode.Enlighten;
        public float defaultIndirectRelolution;
        public float defaultLightmapResolution;
        public int defaultLightmapPadding;
        public bool defaultUseHighResolutionLightmapSize;
        public bool defaultCompressLightmaps;
        public bool defaultAmbientOcclusion;
        public float defaultMaxDistance;
        public float defaultIndirectContribution;
        public float defaultDirectContribution;
        public bool defaultUseDirectionalMode;
        public float defaultLightIndirectIntensity = 2f;
        public float defaultLightBoostIntensity = 6f;
        public bool defaultFinalGather = false;
        public int defaultFinalGatherRayCount = 256;
        public bool defaultFinalGatherDenoising = true;
        public bool defaultAutoLightmapGeneration = false;
        [Header("Reflection Probe Settings")]
        public string defaultReflectionProbeName = "New Reflection probe";
        public AmbientSkiesConsts.ReflectionProbeSpawnType defaultReflectionProbeSpawnType = AmbientSkiesConsts.ReflectionProbeSpawnType.AutomaticallyGenerated;
        public ReflectionProbeMode defaultReflectionProbeMode = ReflectionProbeMode.Realtime;
        public ReflectionProbeRefreshMode defaultReflectionProbeRefresh = ReflectionProbeRefreshMode.OnAwake;
        public ReflectionCubemapCompression defaultReflectionCubemapCompression = ReflectionCubemapCompression.Auto;
        public ReflectionProbeTimeSlicingMode defaultReflectionProbeTimeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
        public Vector3 defaultReflectionProbeScale = new Vector3(100f, 100f, 100f);
        public int defaultReflectionProbesPerRow = 5;
        public float defaultReflectionProbeOffset = 10f;
        public float defaultReflectionProbeClipPlaneDistance = 1000f;
        public float defaultReflectionProbeBlendDistance = 5f;
        public LayerMask defaultReflectionprobeCullingMask = 1;
        public float defaultReflectionProbeShadowDistance = 100f;
        public AmbientSkiesConsts.ReflectionProbeResolution defaultReflectionProbeResolution = AmbientSkiesConsts.ReflectionProbeResolution.Resolution128;
        public AmbientSkiesConsts.LightProbeSpawnType defaultLightProbeSpawnType = AmbientSkiesConsts.LightProbeSpawnType.AutomaticallyGenerated;
        public int defaultLightProbesPerRow = 50;
        public int defaultLightProbeSpawnRadius = 25;
        public float defaultSeaLevel = 50f;
#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
        public LightmapEditorSettings.FilterMode defaultFilterMode = LightmapEditorSettings.FilterMode.Auto;
#endif

        #endregion

        #region Lightmaps Current Variables

        //Current lightmapping settings
        public bool showLightmapSettings;
        public bool realtimeGlobalIllumination;
        public bool bakedGlobalIllumination;
        public AmbientSkiesConsts.LightmapperMode lightmappingMode = AmbientSkiesConsts.LightmapperMode.Enlighten;
        public float indirectRelolution;
        public float lightmapResolution;
        public int lightmapPadding;
        public bool useHighResolutionLightmapSize;
        public bool compressLightmaps;
        public bool ambientOcclusion;
        public float maxDistance;
        public float indirectContribution;
        public float directContribution;
        public bool useDirectionalMode;
        public float lightIndirectIntensity = 2f;
        public float lightBoostIntensity = 6f;
        public bool finalGather = false;
        public int finalGatherRayCount = 256;
        public bool finalGatherDenoising = true;
        public bool autoLightmapGeneration = false;
        [Header("Reflection Probe Settings")]
        public string reflectionProbeName = "New Reflection probe";
        public AmbientSkiesConsts.ReflectionProbeSpawnType reflectionProbeSpawnType = AmbientSkiesConsts.ReflectionProbeSpawnType.AutomaticallyGenerated;
        public ReflectionProbeMode reflectionProbeMode = ReflectionProbeMode.Realtime;
        public ReflectionProbeRefreshMode reflectionProbeRefresh = ReflectionProbeRefreshMode.OnAwake;
        public ReflectionCubemapCompression reflectionCubemapCompression = ReflectionCubemapCompression.Auto;
        public ReflectionProbeTimeSlicingMode reflectionProbeTimeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
        public Vector3 reflectionProbeScale = new Vector3(100f, 100f, 100f);
        public int reflectionProbesPerRow = 5;
        public float reflectionProbeOffset = 10f;
        public float reflectionProbeClipPlaneDistance = 1000f;
        public float reflectionProbeBlendDistance = 5f;
        public LayerMask reflectionprobeCullingMask = 1;
        public float reflectionProbeShadowDistance = 100f;
        public AmbientSkiesConsts.ReflectionProbeResolution reflectionProbeResolution = AmbientSkiesConsts.ReflectionProbeResolution.Resolution128;
        public AmbientSkiesConsts.LightProbeSpawnType lightProbeSpawnType = AmbientSkiesConsts.LightProbeSpawnType.AutomaticallyGenerated;
        public int lightProbesPerRow = 50;
        public int lightProbeSpawnRadius = 25;
        public float seaLevel = 50f;
#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
        public LightmapEditorSettings.FilterMode filterMode = LightmapEditorSettings.FilterMode.Auto;
#endif

        #endregion

        #region Defaults Setup

        public void RevertToDefault()
        {
            showLightmapSettings = deaultShowLightmapSettings;
            realtimeGlobalIllumination = defaultRealtimeGlobalIllumination;
            bakedGlobalIllumination = defaultBakedGlobalIllumination;
            lightmappingMode = defaultLightmappingMode;
            indirectRelolution = defaultIndirectRelolution;
            lightmapResolution = defaultLightmapResolution;
            lightmapPadding = defaultLightmapPadding;
            useHighResolutionLightmapSize = defaultUseHighResolutionLightmapSize;
            compressLightmaps = defaultCompressLightmaps;
            ambientOcclusion = defaultAmbientOcclusion;
            maxDistance = defaultMaxDistance;
            indirectContribution = defaultIndirectContribution;
            directContribution = defaultDirectContribution;
            useDirectionalMode = defaultUseDirectionalMode;
            lightIndirectIntensity = defaultLightIndirectIntensity;
            lightBoostIntensity = defaultLightBoostIntensity;
            finalGather = defaultFinalGather;
            finalGatherRayCount = defaultFinalGatherRayCount;
            finalGatherDenoising = defaultFinalGatherDenoising;
            autoLightmapGeneration = defaultAutoLightmapGeneration;
            reflectionProbeName = defaultReflectionProbeName;
            reflectionProbeSpawnType = defaultReflectionProbeSpawnType;
            reflectionProbeMode = defaultReflectionProbeMode;
            reflectionProbeRefresh = defaultReflectionProbeRefresh;
            reflectionCubemapCompression = defaultReflectionCubemapCompression;
            reflectionProbeTimeSlicingMode = defaultReflectionProbeTimeSlicingMode;
            reflectionProbeScale = defaultReflectionProbeScale;
            reflectionProbeOffset = defaultReflectionProbeOffset;
            reflectionProbesPerRow = defaultReflectionProbesPerRow;
            reflectionProbeClipPlaneDistance = defaultReflectionProbeClipPlaneDistance;
            reflectionProbeBlendDistance = defaultReflectionProbeBlendDistance;
            reflectionprobeCullingMask = defaultReflectionprobeCullingMask;
            reflectionProbeShadowDistance = defaultReflectionProbeShadowDistance;
            reflectionProbeResolution = defaultReflectionProbeResolution;
            lightProbesPerRow = defaultLightProbesPerRow;
            lightProbeSpawnType = defaultLightProbeSpawnType;
            lightProbeSpawnRadius = defaultLightProbeSpawnRadius;
            seaLevel = defaultSeaLevel;
#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
            filterMode = defaultFilterMode;
#endif
        }

        public void SaveCurrentToDefault()
        {
            deaultShowLightmapSettings = showLightmapSettings;
            defaultRealtimeGlobalIllumination = realtimeGlobalIllumination;
            defaultBakedGlobalIllumination = bakedGlobalIllumination;
            defaultLightmappingMode = lightmappingMode;
            defaultIndirectRelolution = indirectRelolution;
            defaultLightmapResolution = lightmapResolution;
            defaultLightmapPadding = lightmapPadding;
            defaultUseHighResolutionLightmapSize = useHighResolutionLightmapSize;
            defaultCompressLightmaps = compressLightmaps;
            defaultAmbientOcclusion = ambientOcclusion;
            defaultMaxDistance = maxDistance;
            defaultIndirectContribution = indirectContribution;
            defaultDirectContribution = directContribution;
            defaultUseDirectionalMode = useDirectionalMode;
            defaultLightIndirectIntensity = lightIndirectIntensity;
            defaultLightBoostIntensity = lightBoostIntensity;
            defaultFinalGather = finalGather;
            defaultFinalGatherRayCount = finalGatherRayCount;
            defaultFinalGatherDenoising = finalGatherDenoising;
            defaultAutoLightmapGeneration = autoLightmapGeneration;
            defaultReflectionProbeName = reflectionProbeName;
            defaultReflectionProbeSpawnType = reflectionProbeSpawnType;
            defaultReflectionProbeMode = reflectionProbeMode;
            defaultReflectionProbeRefresh = reflectionProbeRefresh;
            defaultReflectionCubemapCompression = reflectionCubemapCompression;
            defaultReflectionProbeTimeSlicingMode = reflectionProbeTimeSlicingMode;
            defaultReflectionProbeScale = reflectionProbeScale;
            defaultReflectionProbeOffset = reflectionProbeOffset;
            defaultReflectionProbesPerRow = reflectionProbesPerRow;
            defaultReflectionProbeClipPlaneDistance = reflectionProbeClipPlaneDistance;
            defaultReflectionProbeBlendDistance = reflectionProbeBlendDistance;
            defaultReflectionprobeCullingMask = reflectionprobeCullingMask;
            defaultReflectionProbeShadowDistance = reflectionProbeShadowDistance;
            defaultReflectionProbeResolution = reflectionProbeResolution;
            defaultLightProbesPerRow = lightProbesPerRow;
            defaultLightProbeSpawnType = lightProbeSpawnType;
            defaultLightProbeSpawnRadius = lightProbeSpawnRadius;
            defaultSeaLevel = seaLevel;
#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
            defaultFilterMode = filterMode;
#endif
        }

        #endregion
    }
}