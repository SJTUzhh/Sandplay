//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif
using UnityEngine.Rendering;

namespace AmbientSkies
{
    /// <summary>
    /// A script to handle auto focus for Ambient Skies
    /// </summary
    [AddComponentMenu("Procedural Worlds/Ambient Skies/Rendering/Auto Depth Of Field")]
    public class AutoDepthOfField : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// Render pipeline active
        /// </summary>
        public AmbientSkiesConsts.RenderPipelineSettings m_renderPipeLine = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;

        /// <summary>
        /// Type of tracking to use.
        /// </summary>
        public AmbientSkiesConsts.DOFTrackingType m_trackingType = AmbientSkiesConsts.DOFTrackingType.FollowScreen;

        /// <summary>
        /// An additional manual offset to be added to the focus - will also be used when fixed offset is selected
        /// </summary>
        public float m_focusOffset = 0f;

        /// <summary>
        /// Source camera object - where the ray shoots from
        /// </summary>
        public Camera m_sourceCamera;

        /// <summary>
        /// Target object for dof - set this to a physical object at start to track that object - leave it null
        /// to have it updated automatically based on raycast
        /// </summary>
        public GameObject m_targetObject;

        /// <summary>
        /// Which layers we will hit
        /// </summary>
        public LayerMask m_targetLayer;

        /// <summary>
        /// Maximum focus distance when follow mouse and follow screen selected
        /// </summary>
        public float m_maxFocusDistance = 100f;

        /// <summary>
        /// The actual focus distance value
        /// </summary>
        public float m_actualFocusDistance = 1f;

        /// <summary>
        /// Processing profile that is current selected
        /// </summary>
        [HideInInspector]
        public AmbientPostProcessingProfile m_processingProfile;

        /// <summary>
        /// Interpolate focus over time
        /// </summary>
        //public bool m_interpolateFocus = true;

        /// <summary>
        /// Time to interpolate
        /// </summary>
        //public float m_interpolationTime = 0.75f;

        /// <summary>
        /// Determine if we got a hit within the max distance
        /// </summary>
        private bool m_maxDistanceExceeded = false;

        /// <summary>
        /// Our DOF object
        /// </summary>
        private UnityEngine.Rendering.PostProcessing.DepthOfField m_dof;

#if HDPipeline && UNITY_2019_1_OR_NEWER
        /// <summary>
        /// HDRP Dof object
        /// </summary>
        private UnityEngine.Experimental.Rendering.HDPipeline.DepthOfField m_hdDof;
#endif

        /// <summary>
        /// Our last hit point
        /// </summary>
        private Vector3 m_dofTrackingPoint = Vector3.negativeInfinity;

        #endregion

        #region Start and Update

        /// <summary>
        /// Get the main camera if it doesnt exist
        /// </summary>
        void Start()
        {
            SetupAutoFocus();
        }

        /// <summary>
        /// Apply on disable
        /// </summary>
        private void OnDisable()
        {
            if (m_processingProfile != null)
            {
                m_processingProfile.depthOfFieldFocusDistance = m_actualFocusDistance;
#if HDPipeline && UNITY_2019_1_OR_NEWER
                if (m_hdDof != null)
                {
                    m_hdDof.focusDistance.value = m_actualFocusDistance;
                }
    #endif
            }
        }

        /// <summary>
        /// Process DOF update
        /// </summary>
        void LateUpdate()
        {
            if (m_renderPipeLine != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                if (m_sourceCamera == null || m_dof == null)
                {
                    return;
                }
            }
#if HDPipeline && UNITY_2019_1_OR_NEWER
            else
            {               
                if (m_sourceCamera == null || m_hdDof == null)
                {
                    return;
                }
            }
#else
            else
            {
                if (m_sourceCamera == null || m_dof == null)
                {
                    return;
                }
            }
#endif

            //Setup 
            SetupAutoFocus();

            //Update the focus target
            UpdateDofTrackingPoint();

            //Set focus distance
            if (m_trackingType == AmbientSkiesConsts.DOFTrackingType.FixedOffset || m_maxDistanceExceeded)
            {
                if (m_renderPipeLine != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    m_dof.focusDistance.value = m_maxFocusDistance + m_focusOffset;
                }
#if !UNITY_2019_1_OR_NEWER
                else
                {
                    m_dof.focusDistance.value = m_maxFocusDistance + m_focusOffset;
                }
#else
                else
                {
#if HDPipeline
                    m_hdDof.focusDistance.value = m_maxFocusDistance + m_focusOffset;
#endif
                }
#endif
            }

            if (m_renderPipeLine != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                m_actualFocusDistance = m_dof.focusDistance.value;
            }
#if !UNITY_2019_1_OR_NEWER
            else
            {
                m_actualFocusDistance = m_dof.focusDistance.value;
            }
#else
            else
            {
#if HDPipeline
                m_actualFocusDistance = m_hdDof.focusDistance.value;
#endif
            }
#endif
            

            if (m_processingProfile != null)
            {
                m_processingProfile.depthOfFieldFocusDistance = m_actualFocusDistance;
            }
        }

#endregion

        #region Depth Of Field Functions

        /// <summary>
        /// Do a raycast to update the focus target
        /// </summary>
        void UpdateDofTrackingPoint()
        {
            switch (m_trackingType)
            {
                case AmbientSkiesConsts.DOFTrackingType.LeftMouseClick:
                {
                    if (Input.GetMouseButton(0))
                    {
                        RaycastHit hit;
                        Ray ray = m_sourceCamera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out hit, m_maxFocusDistance, m_targetLayer))
                        {
                            m_maxDistanceExceeded = false;
                            m_dofTrackingPoint = hit.point;

                                if (m_renderPipeLine != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                                {
                                    m_dof.focusDistance.value = (m_sourceCamera.transform.position - m_dofTrackingPoint).magnitude + m_focusOffset;
                                }
#if HDPipeline && UNITY_2019_1_OR_NEWER
                                else
                                {                                   
                                    m_hdDof.focusDistance.value = (m_sourceCamera.transform.position - m_dofTrackingPoint).magnitude + m_focusOffset;
                                }
#else
                                else
                                {
                                    m_dof.focusDistance.value = (m_sourceCamera.transform.position - m_dofTrackingPoint).magnitude + m_focusOffset;
                                }
#endif
                            }
                        else
                        {
                            m_maxDistanceExceeded = true;
                        }
                    }
                    break;
                }
                case AmbientSkiesConsts.DOFTrackingType.RightMouseClick:
                {
                    if (Input.GetMouseButton(1))
                    {
                        RaycastHit hit;
                        Ray ray = m_sourceCamera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out hit, m_maxFocusDistance, m_targetLayer))
                        {
                            m_maxDistanceExceeded = false;
                            m_dofTrackingPoint = hit.point;

                                if (m_renderPipeLine != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                                {
                                    m_dof.focusDistance.value = (m_sourceCamera.transform.position - m_dofTrackingPoint).magnitude + m_focusOffset;
                                }
#if HDPipeline && UNITY_2019_1_OR_NEWER
                                else
                                {                                    
                                    m_hdDof.focusDistance.value = (m_sourceCamera.transform.position - m_dofTrackingPoint).magnitude + m_focusOffset;
                                }
#else
                                else
                                {
                                    m_dof.focusDistance.value = (m_sourceCamera.transform.position - m_dofTrackingPoint).magnitude + m_focusOffset;
                                }
#endif
                            }
                            else
                        {
                            m_maxDistanceExceeded = true;
                        }
                    }
                    break;
                }
                case AmbientSkiesConsts.DOFTrackingType.FollowScreen:
                {
                    RaycastHit hit;
                    Ray ray = new Ray(m_sourceCamera.transform.position, m_sourceCamera.transform.forward);
                    //Debug.DrawRay(ray.origin, ray.direction * m_maxFocusDistance, Color.red);
                    if (Physics.Raycast(ray, out hit, m_maxFocusDistance, m_targetLayer))
                    {
                        m_maxDistanceExceeded = false;
                        m_dofTrackingPoint = hit.point;
#if HDPipeline && UNITY_2019_1_OR_NEWER
                            m_hdDof.focusDistance.value = (m_sourceCamera.transform.position - m_dofTrackingPoint).magnitude / 4f + m_focusOffset;
#else
                            m_dof.focusDistance.value = (m_sourceCamera.transform.position - m_dofTrackingPoint).magnitude + m_focusOffset;
#endif
                        }
                    else
                    {
                        m_maxDistanceExceeded = true;
                    }
                    break;
                }
                case AmbientSkiesConsts.DOFTrackingType.FollowTarget:
                {
                    m_dofTrackingPoint = m_targetObject.transform.position;
                    break;
                }
            }
        }

        /// <summary>
        /// Setup autofocus object
        /// </summary>
        void SetupAutoFocus()
        {
            SetPipelineRenderer();

            //Set up main camera
            if (m_sourceCamera == null)
            {
                m_sourceCamera = GetMainCamera();
                if (m_sourceCamera == null)
                {
                    Debug.Log("DOF Autofocus exiting, unable to find main camera!");
                    enabled = false;
                    return;
                }
            }

            //Determine tracking type
            if (m_trackingType == AmbientSkiesConsts.DOFTrackingType.FollowTarget && m_targetObject == null)
            {
                Debug.Log("Tracking target is missing, falling back to follow screen!");
                m_trackingType = AmbientSkiesConsts.DOFTrackingType.FollowScreen;
            }

            if (m_renderPipeLine != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                //Find our DOF component
                if (m_dof == null)
                {
                    GameObject ppObj = GameObject.Find("Global Post Processing");
                    if (ppObj == null)
                    {
                        Debug.Log("DOF Autofocus exiting, unable to global post processing object!");
                        enabled = false;
                        return;
                    }

                    PostProcessVolume ppVolume = ppObj.GetComponent<PostProcessVolume>();
                    {
                        if (ppVolume == null)
                        {
                            Debug.Log("DOF Autofocus exiting, unable to global post processing volume!");
                            enabled = false;
                            return;
                        }
                    }

                    PostProcessProfile ppProfile = ppVolume.sharedProfile;
                    if (ppProfile == null)
                    {
                        Debug.Log("DOF Autofocus exiting, unable to global post processing profile!");
                        enabled = false;
                        return;
                    }

                    if (!ppProfile.HasSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>())
                    {
                        Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                        enabled = false;
                        return;
                    }

                    if (!ppProfile.TryGetSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>(out m_dof))
                    {
                        Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                        m_dof = null;
                        enabled = false;
                        return;
                    }
                    else
                    {
                        m_dof.focusDistance.overrideState = true;
                        m_actualFocusDistance = m_dof.focusDistance.value;
                    }
                }
                else
                {
                    m_dof.focusDistance.overrideState = true;
                    m_actualFocusDistance = m_dof.focusDistance.value;
                }
            }
#if HDPipeline && UNITY_2019_1_OR_NEWER
            else
            {
                if (m_hdDof == null)
                {
                    VolumeProfile volumeProfile = FindObjectOfType<Volume>().sharedProfile;

                    UnityEngine.Experimental.Rendering.HDPipeline.DepthOfField dof;
                    if (volumeProfile.TryGet(out dof))
                    {
                        m_hdDof = dof;
                        m_hdDof.focusMode.value = DepthOfFieldMode.UsePhysicalCamera;
                        m_hdDof.focusDistance.overrideState = true;
                        m_actualFocusDistance = m_hdDof.focusDistance.value;
                    }
                }
                else
                {
                    m_hdDof.focusMode.value = DepthOfFieldMode.UsePhysicalCamera;
                    m_hdDof.focusDistance.overrideState = true;
                    m_actualFocusDistance = m_hdDof.focusDistance.value;
                }
            }
#else
            else
            {
                //Find our DOF component
                if (m_dof == null)
                {
                    GameObject ppObj = GameObject.Find("Global Post Processing");
                    if (m_renderPipeLine != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
                        if (ppObj == null)
                        {
                            Debug.Log("DOF Autofocus exiting, unable to global post processing object!");
                            enabled = false;
                            return;
                        }

                        PostProcessVolume ppVolume = ppObj.GetComponent<PostProcessVolume>();
                        {
                            if (ppVolume == null && m_renderPipeLine == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                            {
                                Debug.Log("DOF Autofocus exiting, unable to global post processing volume!");
                                enabled = false;
                                return;
                            }
                        }

                        PostProcessProfile ppProfile = ppVolume.sharedProfile;
                        if (ppProfile == null && m_renderPipeLine != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                        {
                            Debug.Log("DOF Autofocus exiting, unable to global post processing profile!");
                            enabled = false;
                            return;
                        }

                        if (!ppProfile.HasSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>())
                        {
                            Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                            enabled = false;
                            return;
                        }

                        if (!ppProfile.TryGetSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>(out m_dof))
                        {
                            Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                            m_dof = null;
                            enabled = false;
                            return;
                        }
                        else
                        {
                            m_dof.focusDistance.overrideState = true;
                            m_actualFocusDistance = m_dof.focusDistance.value;
                        }
                    }
                }
                else
                {
                    m_dof.focusDistance.overrideState = true;
                    m_actualFocusDistance = m_dof.focusDistance.value;
                }
            }
#endif
        }

        #endregion

        #region Utils

        /// <summary>
        /// Get the main camera or null if none available
        /// </summary>
        /// <returns>Main camera or null</returns>
        Camera GetMainCamera()
        {
            GameObject mainCameraObject = GameObject.Find("Main Camera");
            if (mainCameraObject != null)
            {
                return mainCameraObject.GetComponent<Camera>();
            }

            mainCameraObject = GameObject.Find("Camera");
            if (mainCameraObject != null)
            {
                return mainCameraObject.GetComponent<Camera>();
            }

            mainCameraObject = GameObject.Find("FirstPersonCharacter");
            if (mainCameraObject != null)
            {
                return mainCameraObject.GetComponentInChildren<Camera>();
            }

            mainCameraObject = GameObject.Find("FlyCam");
            if (mainCameraObject != null)
            {
                return mainCameraObject.GetComponent<Camera>();
            }

            if (Camera.main != null)
            {
                return Camera.main;
            }

            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            foreach (var camera in cameras)
            {
                return camera;
            }

            return null;
        }

        /// <summary>
        /// Sets the render pipeline within the project
        /// </summary>
        void SetPipelineRenderer()
        {
            if (GraphicsSettings.renderPipelineAsset == null)
            {
                m_renderPipeLine = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;
            }
            else if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
            {
                m_renderPipeLine = AmbientSkiesConsts.RenderPipelineSettings.HighDefinition;
            }
            else
            {
                m_renderPipeLine = AmbientSkiesConsts.RenderPipelineSettings.Lightweight;
            }
        }

        #endregion
    }
}
#endif