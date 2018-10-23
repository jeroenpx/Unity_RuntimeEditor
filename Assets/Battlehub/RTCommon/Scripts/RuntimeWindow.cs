using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
    [System.Flags]
    public enum RuntimeWindowType
    {
        None = 0,
        GameView = 1,
        SceneView = 2,
        Hierarchy = 4,
        ProjectTree = 8,
        Resources = 16,
        Inspector = 32,
        Other = 64
    }

    public class RuntimeWindow : DragDropTarget
    {
        public RuntimeWindowType WindowType;

        private bool m_isActivated;

        [SerializeField]
        private Image m_rootGraphics;

        [SerializeField]
        private RectTransform m_rectTransform;
        private Canvas m_canvas;

        [SerializeField]
        private Camera m_camera;
        public Camera Camera
        {
            get { return m_camera; }
            set
            {
                if(m_camera != null)
                {
                    m_camera.cullingMask |= (1 << Editor.CameraLayerSettings.RuntimeHandlesLayer);

                    GLCamera glCamera = m_camera.GetComponent<GLCamera>();
                    if(glCamera != null)
                    {
                        Destroy(glCamera);
                    }
                }

                m_camera = value;
                if(m_camera != null)
                {
                    m_camera.cullingMask &= ~(1 << Editor.CameraLayerSettings.RuntimeHandlesLayer);
                }

                if(WindowType == RuntimeWindowType.SceneView)
                {
                    if(!m_camera.GetComponent<GLCamera>())
                    {
                        m_camera.gameObject.AddComponent<GLCamera>();
                    }
                }
            }
        }

        [SerializeField]
        private Pointer m_pointer;
        public Pointer Pointer
        {
            get { return m_pointer; }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            if(m_rootGraphics == null)
            {
                if(!Editor.IsVR)
                {
                    m_rootGraphics = gameObject.AddComponent<Image>();
                    m_rootGraphics.color = new Color(0, 0, 0, 0);
                    m_rootGraphics.raycastTarget = true;
                }
            }

            if (m_pointer == null)
            {
                if(Editor.IsVR)
                {
                    m_pointer = gameObject.AddComponent<VRPointer>();
                }
                else
                {
                    m_pointer = gameObject.AddComponent<Pointer>();
                }
            }

            if (m_camera != null)
            {
                m_camera.cullingMask &= ~(1 << Editor.CameraLayerSettings.RuntimeHandlesLayer);
            }

            if (WindowType == RuntimeWindowType.SceneView)
            {
                if (!m_camera.GetComponent<GLCamera>())
                {
                    m_camera.gameObject.AddComponent<GLCamera>();
                }
            }

            m_rectTransform = GetComponent<RectTransform>();
            m_canvas = GetComponentInParent<Canvas>();

            OnRectTransformDimensionsChange();

            Editor.ActiveWindowChanged += OnActiveWindowChanged;
            Editor.RegisterWindow(this);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(Editor != null)
            {
                Editor.ActiveWindowChanged -= OnActiveWindowChanged;
                Editor.UnregisterWindow(this);
            }

            if (m_camera != null)
            {
                m_camera.cullingMask |= (1 << Editor.CameraLayerSettings.RuntimeHandlesLayer);
            }
        }

        private void Update()
        {
            UpdateOverride();
        }

        protected virtual void UpdateOverride()
        {
            
        }

        private void OnActiveWindowChanged()
        {
            if (Editor.ActiveWindow == this)
            {
                if (!m_isActivated)
                {
                    m_isActivated = true;
                    OnActivated();
                }
            }
            else
            {
                if (m_isActivated)
                {
                    m_isActivated = false;
                    OnDeactivated();
                }
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (m_camera != null && m_rectTransform != null && m_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector3[] corners = new Vector3[4];
                m_rectTransform.GetWorldCorners(corners);
                m_camera.pixelRect = new Rect(corners[0], new Vector2(corners[2].x - corners[0].x, corners[1].y - corners[0].y));
            }
        }

        protected virtual void OnActivated()
        {
            Debug.Log("Activated");
        }

        protected virtual void OnDeactivated()
        {
            Debug.Log("Deactivated");
        }      
    }
}
