﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
    public enum RuntimeWindowType
    {
        None = 0,
        Game = 1,
        Scene = 2,
        Hierarchy = 3,
        Project = 4,
        ProjectTree = 5,
        ProjectFolder = 6,
        Inspector = 7,
        Console = 8,

        ToolsPanel = 21,

        OpenProject = 51,
        SelectAssetLibrary = 52,
        ImportAssets = 53,
        SaveScene = 54,
        About = 55,

        SelectObject = 101,
        SelectColor = 102,
    }

    public class RuntimeWindow : DragDropTarget
    {
        [SerializeField]
        private bool m_activateOnAnyKey = false;
        public bool ActivateOnAnyKey
        {
            get { return m_activateOnAnyKey; }
            set { m_activateOnAnyKey = true; }
        }

        private bool m_isActivated;
        private IOCContainer m_container = new IOCContainer();
        public IOCContainer IOCContainer
        {
            get { return m_container; }
        }

        [SerializeField]
        private RuntimeWindowType m_windowType = RuntimeWindowType.Scene;
        public virtual RuntimeWindowType WindowType
        {
            get { return m_windowType; }
            set
            {
                if(m_windowType != value)
                {
                    m_index = Editor.GetIndex(value);
                    m_windowType = value;
                }
            }
        }

        private int m_index;
        public virtual int Index
        {
            get { return m_index; }
        }

        private Vector3 m_position;
        private Rect m_rect;
        [SerializeField]
        private RectTransform m_rectTransform;
        private Canvas m_canvas;
        [SerializeField]
        private Image m_background;
        public Image Background
        {
            get { return m_background; }
        }

        [SerializeField]
        protected Camera m_camera;
        public virtual Camera Camera
        {
            get { return m_camera; }
            set
            {
                if(m_camera == value)
                {
                    return;
                }

                if(m_camera != null)
                {
                    ResetCullingMask();

                    GLCamera glCamera = m_camera.GetComponent<GLCamera>();
                    if(glCamera != null)
                    {
                        Destroy(glCamera);
                    }
                }

                m_camera = value;
                if(m_camera != null)
                {
                    SetCullingMask();
                    if (WindowType == RuntimeWindowType.Scene)
                    {
                        GLCamera glCamera = m_camera.GetComponent<GLCamera>();
                        if (!glCamera)
                        {
                            glCamera = m_camera.gameObject.AddComponent<GLCamera>();
                        }
                        glCamera.CullingMask = 1 << (Editor.CameraLayerSettings.RuntimeGraphicsLayer + m_index);
                    }
                }
            }
        }

        private int m_cameraDepth;
        public int CameraDepth
        {
            get { return m_cameraDepth; }
        }

        public virtual void SetCameraDepth(int depth)
        {
            m_cameraDepth = depth;
            if (m_camera != null)
            {
                m_camera.depth = m_cameraDepth;
            }
        }


        [SerializeField]
        private Pointer m_pointer;
        public virtual Pointer Pointer
        {
            get { return m_pointer; }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            if(m_background == null)
            {
                if(!Editor.IsVR)
                {
                    m_background = GetComponent<Image>();
                    if(m_background == null)
                    {
                        m_background = gameObject.AddComponent<Image>();
                        m_background.color = new Color(0, 0, 0, 0);
                        m_background.raycastTarget = true;
                    }
                    else
                    {
                        m_background.raycastTarget = true;
                    }
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

            m_rectTransform = GetComponent<RectTransform>();
            m_canvas = GetComponentInParent<Canvas>();

            Editor.ActiveWindowChanged += OnActiveWindowChanged;

            m_index = Editor.GetIndex(WindowType);

            if (m_camera != null)
            {
                SetCullingMask();
                if (WindowType == RuntimeWindowType.Scene)
                {
                    GLCamera glCamera = m_camera.GetComponent<GLCamera>();
                    if (!glCamera)
                    {
                        glCamera = m_camera.gameObject.AddComponent<GLCamera>();
                    }
                    glCamera.CullingMask = 1 << (Editor.CameraLayerSettings.RuntimeGraphicsLayer + m_index);
                }
            }

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
                ResetCullingMask();
            }
        }

        private void Update()
        {
            UpdateOverride();
        }

        protected virtual void UpdateOverride()
        {
            if(m_camera != null)
            {
                if(m_rectTransform.rect != m_rect || m_rectTransform.position != m_position)
                {
                    OnRectTransformDimensionsChange();
                    m_rect = m_rectTransform.rect;
                    m_position = m_rectTransform.position;
                }
            }
        }

       

        protected virtual void OnActiveWindowChanged()
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

       
        protected virtual void OnRectTransformDimensionsChange()
        {
            HandleResize();
        }

        public void HandleResize()
        {
            if (m_camera != null && m_rectTransform != null)
            {
                if (m_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Vector3[] corners = new Vector3[4];
                    m_rectTransform.GetWorldCorners(corners);
                    ResizeCamera(new Rect(corners[0], new Vector2(corners[2].x - corners[0].x, corners[1].y - corners[0].y)));
                }
                else if (m_canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    if(m_canvas.worldCamera != Camera)
                    {
                        Vector3[] corners = new Vector3[4];
                        m_rectTransform.GetWorldCorners(corners);

                        corners[0] = RectTransformUtility.WorldToScreenPoint(m_canvas.worldCamera, corners[0]);
                        corners[1] = RectTransformUtility.WorldToScreenPoint(m_canvas.worldCamera, corners[1]);
                        corners[2] = RectTransformUtility.WorldToScreenPoint(m_canvas.worldCamera, corners[2]);
                        corners[3] = RectTransformUtility.WorldToScreenPoint(m_canvas.worldCamera, corners[3]);

                        Vector2 size = new Vector2(corners[2].x - corners[0].x, corners[1].y - corners[0].y);
                        ResizeCamera(new Rect(corners[0], size));
                    }
                }
            }
        }

        protected virtual void ResizeCamera(Rect pixelRect)
        {
            m_camera.pixelRect = pixelRect;
        }

        protected virtual void OnActivated()
        {
            //Debug.Log("Activated");
        }

        protected virtual void OnDeactivated()
        {
            //Debug.Log("Deactivated");
        }

        protected virtual void SetCullingMask()
        {
            SetCullingMask(m_camera);
        }

        protected virtual void SetCullingMask(Camera camera)
        {
            CameraLayerSettings settings = Editor.CameraLayerSettings;
            camera.cullingMask &= ~(((1 << settings.MaxGraphicsLayers) - 1) << settings.RuntimeGraphicsLayer);
        }

        protected virtual void ResetCullingMask()
        {
            ResetCullingMask(m_camera);
        }

        protected virtual void ResetCullingMask(Camera camera)
        {
            CameraLayerSettings settings = Editor.CameraLayerSettings;
            camera.cullingMask |= (((1 << settings.MaxGraphicsLayers) - 1) << settings.RuntimeGraphicsLayer);
        }

    }
}
