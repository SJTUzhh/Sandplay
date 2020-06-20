//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;

namespace AmbientSkies
{
    [AddComponentMenu("Procedural Worlds/Ambient Skies/System/Horizon Sky")]
    [RequireComponent(typeof(Transform))]
    public class HorizonSky : MonoBehaviour
    {
        #region Public Variables
        [Header("Horizon Sky Configuration")]
        [Tooltip("Update interval to move it's position to the camera y transform")]
        public float m_positionUpdate = 10f;
        [Tooltip("If enabled the Horizon Sky will update xyz transform positions instead of just y transform")]
        public bool m_followsCameraPosition = true;
        #endregion

        #region Private Variables
        private GameObject m_mainCamera;
        private GameObject m_HorizonSky;
        private float m_storedUpdate;
        private bool m_isUpdatingPosition;
        #endregion

        #region Start Function
        private void Start()
        {
            m_mainCamera = GetOrCreateMainCamera();
            m_HorizonSky = GameObject.Find("Ambient Skies Horizon");
            m_storedUpdate = m_positionUpdate;
            m_isUpdatingPosition = false;
            UpdatePosition();

            if (m_mainCamera == null)
            {
                m_mainCamera = GameObject.Find("Main Camera");

                if (m_mainCamera == null)
                {
                    m_mainCamera = GameObject.Find("Camera");
                }
            }
        }
        #endregion

        #region Update Function
        private void Update()
        {
            if (m_isUpdatingPosition)
            {
                return;
            }

            //Moves position
            m_storedUpdate -= Time.deltaTime;
            if (m_storedUpdate < 0)
            {
                UpdatePosition();
            }
        }
        #endregion

        #region Update Position
        public void UpdatePosition()
        {
            if (m_mainCamera == null)
            {
                m_mainCamera = GetOrCreateMainCamera();
            }

            if (m_HorizonSky == null)
            {
                m_HorizonSky = GameObject.Find("Ambient Skies Horizon");
            }

            if (!m_followsCameraPosition)
            {
                m_isUpdatingPosition = true;
                m_HorizonSky.transform.localPosition = new Vector3(0f, m_mainCamera.transform.position.y - 45f, 0f);
                m_storedUpdate = m_positionUpdate;
                m_isUpdatingPosition = false;
            }

            if (m_followsCameraPosition)
            {
                m_isUpdatingPosition = true;
                m_HorizonSky.transform.localPosition = new Vector3(m_mainCamera.transform.localPosition.x, m_mainCamera.transform.localPosition.y - 45f, m_mainCamera.transform.localPosition.z);
                m_storedUpdate = m_positionUpdate;
                m_isUpdatingPosition = false;
            }

            return;
        }
        #endregion

        #region Utils
        /// <summary>
        /// Get or create the main camera in the scene
        /// </summary>
        /// <returns>Existing or new main camera</returns>
        public static GameObject GetOrCreateMainCamera()
        {
            //Get or create the main camera
            GameObject mainCameraObj = null;

            if (Camera.main != null)
            {
                mainCameraObj = Camera.main.gameObject;
            }

            if (mainCameraObj == null)
            {
                mainCameraObj = GameObject.Find("Main Camera");
            }

            if (mainCameraObj == null)
            {
                mainCameraObj = GameObject.Find("Camera");
            }

            if (mainCameraObj == null)
            {
                Camera[] cameras = Object.FindObjectsOfType<Camera>();
                foreach (var camera in cameras)
                {
                    mainCameraObj = camera.gameObject;
                    break;
                }
            }

            if (mainCameraObj == null)
            {
                mainCameraObj = new GameObject("Main Camera");
                mainCameraObj.tag = "MainCamera";
            }

            return mainCameraObj;
        }
        #endregion
    }
}