//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
#if AMBIENT_SKIES
using System.Collections.Generic;
using UnityEngine;

namespace AmbientSkies
{
    [AddComponentMenu("Procedural Worlds/Ambient Skies/API Component")]
    public class AmientSkiesAPI : MonoBehaviour
    {
        #region Variables

        //The current render pipeline
        public AmbientSkiesConsts.RenderPipelineSettings m_renderPipelineSelected;

        //Main systems
        public List<AmbientSkyProfiles> m_profileList;
        public int m_profileListIndex;
        public AmbientSkyProfiles m_profiles;
        public int newProfileListIndex;
        //Skybox profiles
        public int m_selectedSkyboxProfileIndex;
        public AmbientSkyboxProfile m_selectedSkyboxProfile;
        //Procedural profiles
        public int m_selectedProceduralSkyboxProfileIndex;
        public AmbientProceduralSkyboxProfile m_selectedProceduralSkyboxProfile;
        //Gradient profiles
        public int m_selectedGradientSkyboxProfileIndex;
        public AmbientGradientSkyboxProfile m_selectedGradientSkyboxProfile;
        //Post processing profiles
        public int m_selectedPostProcessingProfileIndex;
        public AmbientPostProcessingProfile m_selectedPostProcessingProfile;
        //Lighting profiles
        public AmbientLightingProfile m_selectedLightingProfile;
        public int m_selectedLightingProfileIndex;

        //Profile choices on all 3 tabs
        public List<string> profileChoices = new List<string>();
        public List<string> skyboxChoices = new List<string>();
        public List<string> proceduralSkyboxChoices = new List<string>();
#if UNITY_2018_3_OR_NEWER
        public List<string> gradientSkyboxChoices = new List<string>();
#endif
        public List<string> ppChoices = new List<string>();
        public List<string> lightmappingChoices = new List<string>();

        //Current skybox selections
        public int newSkyboxSelection;
        public int newProceduralSkyboxSelection;
        public int newGradientSkyboxSelection;

        #endregion
    }
}
#endif