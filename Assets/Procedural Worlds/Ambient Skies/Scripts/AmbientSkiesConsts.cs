//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;

namespace AmbientSkies
{
    public class AmbientSkiesConsts : MonoBehaviour
    {
        public enum RenderPipelineSettings { BuiltIn, Lightweight, HighDefinition }

        public enum HighDefinitionQualitySetting { Custom, MobileAndAndriod, VR, Desktop, HighEndDesktop, Cinematic }

        public enum EnvironementSkyUpdateMode { OnChanged, OnDemand, Realtime }

        public enum VolumeSkyType { Gradient, HDRISky, ProceduralSky }

        public enum VolumeFogType { None, Exponential, ExponentialSquared, Linear, Volumetric }
        
        public enum ShadowCascade { CascadeCount1, CascadeCount2, CascadeCount3, CascadeCount4 }

        public enum HDShadowQuality { Resolution64, Resolution128, Resolution256, Resolution512, Resolution1024, Resolution2048, Resolution4096, Resolution8192, Resolution16384 }

        public enum AmbientMode { Color, Gradient, Skybox }

        public enum VSyncMode { DontSync, EveryVBlank, EverySecondVBlank }

        public enum AntiAliasingMode { None, FXAA, SMAA, TAA, MSAA }

        public enum HDRMode { On, Off }

        public enum SkyType { HDRISky, ProceduralSky }

        public enum PlatformTarget { DesktopAndConsole, MobileAndVR }

        public enum DepthOfFieldMode { AutoFocus, Manual }

        public enum DOFQuality { NORMAL, HIGH }

        public enum DOFTrackingType { FixedOffset, LeftMouseClick, RightMouseClick, FollowScreen, FollowTarget }

        public enum SystemTypes { AmbientHDRISkies, AmbientProceduralSkies, AmbientGradientSkies, DefaultProcedural, ThirdParty }

        public enum TimeOfDayController { Realtime, AmbientSkies, WorldManagerAPI, ThirdParty }

        public enum CurrentSeason { Spring, Summer, AutumnFall, Winter }

        public enum HemisphereOrigin { Northern, Southern }

        public enum DisableAndEnable { Enable, Disable }

        public enum LightmapperMode { Enlighten, ProgressiveCPU, ProgressiveGPU }

        public enum AutoConfigureType { Terrain, Camera, Manual }

        public enum ReflectionProbeSpawnType { AutomaticallyGenerated, ManualPlacement }

        public enum LightProbeSpawnType { AutomaticallyGenerated, MinDefaultHeight, ManualPlacement }

        public enum ReflectionProbeResolution { Resolution16, Resolution32, Resolution64, Resolution128, Resolution256, Resolution512, Resolution1024, Resolution2048 }

        public enum EnvironmentLightingEditMode { Exterior, Interior }
    }
}