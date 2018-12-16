using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
    /// <summary>
    /// Attach to any game object to Draw grid
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class RuntimeGrid : RTEBehaviour
    {
        private Camera m_camera;
        public RuntimeHandlesComponent Appearance;
        public float CamOffset = 0.0f;
        public bool AutoCamOffset = true;
        public Vector3 GridOffset;

        protected virtual void Start()
        {
            RuntimeHandlesComponent.InitializeIfRequired(ref Appearance);

            m_camera = GetComponent<Camera>();
            m_camera.clearFlags = CameraClearFlags.Nothing;
            m_camera.renderingPath = RenderingPath.Forward;
            m_camera.cullingMask = 0;

            if (m_camera.depth != Window.Camera.depth + 0.01f)
            {
                m_camera.depth = Window.Camera.depth + 0.01f;
            }
            if (Window.Editor.IsVR)
            {
                m_camera.transform.SetParent(Window.Camera.transform.parent, false);
            }
            SetupCamera();
        }

        private void OnPreRender()
        {
            m_camera.farClipPlane = RuntimeHandlesComponent.GetGridFarPlane();
        }

        private void OnPostRender()
        { 
            if(AutoCamOffset)
            {
                Appearance.DrawGrid(GridOffset, Camera.current.transform.position.y);
            }
            else
            {
                Appearance.DrawGrid(GridOffset, CamOffset);
            }
        }

        private void Update()
        {
            SetupCamera();
        }

        private void SetupCamera()
        {
            m_camera.transform.position = Window.Camera.transform.position;
            m_camera.transform.rotation = Window.Camera.transform.rotation;
            m_camera.transform.localScale = Window.Camera.transform.localScale;

            if (m_camera.depth != Window.Camera.depth + 0.01f)
            {
                m_camera.depth = Window.Camera.depth + 0.01f;
            }

            if (m_camera.fieldOfView != Window.Camera.fieldOfView)
            {
                m_camera.fieldOfView = Window.Camera.fieldOfView;
            }

            if(m_camera.nearClipPlane != Window.Camera.nearClipPlane)
            {
                m_camera.nearClipPlane = Window.Camera.nearClipPlane;
            }

            if (m_camera.orthographic != Window.Camera.orthographic)
            {
                m_camera.orthographic = Window.Camera.orthographic;
            }

            if (m_camera.orthographicSize != Window.Camera.orthographicSize)
            {
                m_camera.orthographicSize = Window.Camera.orthographicSize;
            }

            if (m_camera.rect != Window.Camera.rect)
            {
                m_camera.rect = Window.Camera.rect;
            }
        }
    }
}

