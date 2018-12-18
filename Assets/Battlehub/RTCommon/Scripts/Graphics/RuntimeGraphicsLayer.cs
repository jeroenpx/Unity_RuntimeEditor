using System.Linq;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace Battlehub.RTCommon
{
    [DefaultExecutionOrder(-55)]
    [RequireComponent(typeof(RuntimeWindow))]
    public class RuntimeGraphicsLayer : MonoBehaviour
    {
        [SerializeField]
        private Camera m_graphicsLayerCamera;

        private RuntimeWindow m_editorWindow;

        private TrackedPoseDriver m_trackedPoseDriver;

        public RuntimeWindow Window
        {
            get { return m_editorWindow; }
        }
        
        private void Awake()
        {
            m_editorWindow = GetComponent<RuntimeWindow>();
            PrepareGraphicsLayerCamera();
        }

        private void Start()
        {
            if (m_editorWindow.Index >= m_editorWindow.Editor.CameraLayerSettings.MaxGraphicsLayers)
            {
                Debug.LogError("m_editorWindow.Index >= m_editorWindow.Editor.CameraLayerSettings.MaxGraphicsLayers");
            }
        }

        private void OnDestroy()
        {
            if(m_graphicsLayerCamera != null)
            {
                Destroy(m_graphicsLayerCamera.gameObject);
            }
        }

        private void PrepareGraphicsLayerCamera()
        {
            m_trackedPoseDriver = m_editorWindow.Camera.GetComponent<TrackedPoseDriver>();
            if (m_editorWindow.Editor.IsVR && m_editorWindow.Camera.stereoEnabled && m_editorWindow.Camera.stereoTargetEye == StereoTargetEyeMask.Both )
            {
                bool wasActive = m_editorWindow.Camera.gameObject.activeSelf;
                m_editorWindow.Camera.gameObject.SetActive(false);
                m_graphicsLayerCamera = Instantiate(m_editorWindow.Camera, m_editorWindow.Camera.transform.parent);
                m_graphicsLayerCamera.transform.SetSiblingIndex(m_editorWindow.Camera.transform.GetSiblingIndex() + 1);
                m_editorWindow.Camera.gameObject.SetActive(wasActive);
                m_graphicsLayerCamera.gameObject.SetActive(wasActive);
            }
            else
            {
                m_graphicsLayerCamera = Instantiate(m_editorWindow.Camera, m_editorWindow.Camera.transform);
            }

            for (int i = m_graphicsLayerCamera.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(m_graphicsLayerCamera.transform.GetChild(i).gameObject);
            }

            Component[] components = m_graphicsLayerCamera.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i)
            {
                Component component = components[i];
                if (component is Transform)
                {
                    continue;
                }
                if (component is Camera)
                {
                    continue;
                }
             
                Destroy(component);
            }

            m_graphicsLayerCamera.clearFlags = CameraClearFlags.Depth;
            m_graphicsLayerCamera.transform.localPosition = Vector3.zero;
            m_graphicsLayerCamera.transform.localRotation = Quaternion.identity;
            m_graphicsLayerCamera.transform.localScale = Vector3.one;
            m_graphicsLayerCamera.name = "GraphicsLayerCamera";
            m_graphicsLayerCamera.depth = m_editorWindow.Camera.depth + 1;
            m_graphicsLayerCamera.cullingMask = 1 << (m_editorWindow.Editor.CameraLayerSettings.RuntimeGraphicsLayer + m_editorWindow.Index);

            if(m_trackedPoseDriver != null)
            {
                m_graphicsLayerCamera.projectionMatrix = m_editorWindow.Camera.projectionMatrix;
            }
        }

        private void LateUpdate()
        {
            if(m_graphicsLayerCamera.depth != m_editorWindow.Camera.depth + 1)
            {
                m_graphicsLayerCamera.depth = m_editorWindow.Camera.depth + 1;
            }

            if (m_graphicsLayerCamera.fieldOfView != m_editorWindow.Camera.fieldOfView)
            {
                m_graphicsLayerCamera.fieldOfView = m_editorWindow.Camera.fieldOfView;
            }

            if (m_graphicsLayerCamera.orthographic != m_editorWindow.Camera.orthographic)
            {
                m_graphicsLayerCamera.orthographic = m_editorWindow.Camera.orthographic;
            }

            if (m_graphicsLayerCamera.orthographicSize != m_editorWindow.Camera.orthographicSize)
            {
                m_graphicsLayerCamera.orthographicSize = m_editorWindow.Camera.orthographicSize;
            }

            if (m_graphicsLayerCamera.rect != m_editorWindow.Camera.rect)
            {
                m_graphicsLayerCamera.rect = m_editorWindow.Camera.rect;
            }

            if(m_graphicsLayerCamera.enabled != m_editorWindow.Camera.enabled)
            {
                m_graphicsLayerCamera.enabled = m_editorWindow.Camera.enabled;
            }

            if(m_graphicsLayerCamera.gameObject.activeSelf != m_editorWindow.Camera.gameObject.activeSelf)
            {
                m_graphicsLayerCamera.gameObject.SetActive(m_editorWindow.Camera.gameObject.activeSelf);
            }

            if(m_trackedPoseDriver != null)
            {
                m_graphicsLayerCamera.projectionMatrix = m_editorWindow.Camera.projectionMatrix;
            }
        }
    }
}



