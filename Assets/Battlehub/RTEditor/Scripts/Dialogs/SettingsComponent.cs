using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface ISettingsComponent
    {
        bool IsGridVisible
        {
            get;
            set;
        }

        bool IsGridEnabled
        {
            get;
            set;
        }

        bool GridZTest
        {
            get;
            set;
        }

        float GridSize
        {
            get;
            set;
        }

        bool UIAutoScale
        {
            get;
            set;
        }

        float UIScale
        {
            get;
            set;
        }

        void EndEditUIScale();

        float ZoomSpeed
        {
            get;
            set;
        }

        bool ConstantZoomSpeed
        {
            get;
            set;
        }

        void ResetToDefaults();
    }


    public class SettingsComponent : MonoBehaviour, ISettingsComponent
    {
        private IWindowManager m_wm;

        private Dictionary<Transform, IRuntimeSceneComponent> m_sceneComponents = new Dictionary<Transform, IRuntimeSceneComponent>();

        [SerializeField]
        private bool m_isGridVisibleDefault = true;
        public bool IsGridVisible
        {
            get { return GetBool("IsGridVisible", m_isGridVisibleDefault); }
            set
            {
                SetBool("IsGridVisible", value);
                foreach(IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.IsGridVisible = value;
                }
            }
        }

        [SerializeField]
        private bool m_isGridEnabledDefault = false;
        public bool IsGridEnabled
        {
            get { return GetBool("IsGridEnabled", m_isGridEnabledDefault); }
            set
            {
                SetBool("IsGridEnabled", value);
                foreach (IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.IsGridEnabled = value;
                }
            }
        }

        [SerializeField]
        private bool m_gridZTest = true;
        public bool GridZTest
        {
            get { return GetBool("GridZTest", m_gridZTest); }
            set
            {
                SetBool("GridZTest", value);
                foreach(IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.GridZTest = value;
                }
            }
        }

        [SerializeField]
        private float m_gridSizeDefault = 0.5f;
        public float GridSize
        {
            get { return GetFloat("GridSize", m_gridSizeDefault); }
            set
            {
                SetFloat("GridSize", value);
                foreach (IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.SizeOfGrid = value;
                }
            }
        }

        [SerializeField]
        private bool m_uiAutoScaleDefault = true;
        public bool UIAutoScale
        {
            get { return GetBool("UIAutoScale", m_uiAutoScaleDefault); }
            set
            {
                SetBool("UIAutoScale", value);
                EndEditUIScale();
            }
        }

        [SerializeField]
        private float m_uiScaleDefault = 1.0f;
        public float UIScale
        {
            get { return GetFloat("UIScale", m_uiScaleDefault); }
            set
            {
                SetFloat("UIScale", value);
            }
        }

        public void EndEditUIScale()
        {
            float scale = 1.0f;
            if (UIAutoScale)
            {
                if (!Application.isEditor)
                {
                    scale = Mathf.Clamp((float)System.Math.Round(Display.main.systemWidth / 1920.0f, 1), 0.5f, 4);
                }
            }
            else
            {
                scale = UIScale;
            }

            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            appearance.UIScale = scale;

            IRuntimeHandlesComponent handles = IOC.Resolve<IRuntimeHandlesComponent>();
            handles.HandleScale = scale;
            handles.SceneGizmoScale = scale;
        }

        [SerializeField]
        private float m_zoomSpeedDefault = 5.0f;
        public float ZoomSpeed
        {
            get { return GetFloat("ZoomSpeed", m_zoomSpeedDefault); }
            set
            {
                SetFloat("ZoomSpeed", value);
                foreach (IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.ZoomSpeed = value;
                }
            }
        }

        [SerializeField]
        private bool m_constantZoomSpeedDefault = false;
        public bool ConstantZoomSpeed
        {
            get { return GetBool("ConstantZoomSpeed", m_constantZoomSpeedDefault); }
            set
            {
                SetBool("ConstantZoomSpeed", value);
                foreach (IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.ConstantZoomSpeed = value;
                }
            }
        }

        private void Awake()
        {
            IOC.RegisterFallback<ISettingsComponent>(this);
            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.AfterLayout += OnAfterLayout;
            m_wm.WindowCreated += OnWindowCreated;
            m_wm.WindowDestroyed += OnWindowDestoryed;
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<ISettingsComponent>(this);

            if(m_wm != null)
            {
                m_wm.AfterLayout -= OnAfterLayout;
                m_wm.WindowCreated -= OnWindowCreated;
                m_wm.WindowDestroyed -= OnWindowDestoryed;
            }
        }

        private void OnWindowCreated(Transform windowTransform)
        {
            RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
            if(window != null && window.WindowType == RuntimeWindowType.Scene)
            {
                IRuntimeSceneComponent sceneComponent = window.IOCContainer.Resolve<IRuntimeSceneComponent>();
                if(sceneComponent != null)
                {
                    m_sceneComponents.Add(windowTransform, sceneComponent);
                    ApplySettings(sceneComponent);
                }
            }
        }

        private void OnWindowDestoryed(Transform windowTransform)
        {
            RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
            if(window != null && window.WindowType == RuntimeWindowType.Scene)
            {
                m_sceneComponents.Remove(windowTransform);
            }
        }

        private void OnAfterLayout(IWindowManager obj)
        {
            Transform[] sceneWindows = m_wm.GetWindows(RuntimeWindowType.Scene.ToString());
            for (int i = 0; i < sceneWindows.Length; ++i)
            {
                Transform windowTransform = sceneWindows[i];
                RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
                if (window != null)
                {
                    IRuntimeSceneComponent sceneComponent = window.IOCContainer.Resolve<IRuntimeSceneComponent>();
                    if (sceneComponent != null)
                    {
                        m_sceneComponents.Add(windowTransform, sceneComponent);
                    }
                }
            }

            ApplySettings();
        }

        private void ApplySettings(IRuntimeSceneComponent sceneComponent)
        {
            sceneComponent.IsGridVisible = IsGridVisible;
            sceneComponent.IsGridEnabled = IsGridEnabled;
            sceneComponent.SizeOfGrid = GridSize;
            sceneComponent.GridZTest = GridZTest;
            sceneComponent.ZoomSpeed = ZoomSpeed;
            sceneComponent.ConstantZoomSpeed = ConstantZoomSpeed;
        }

        private void ApplySettings()
        {
            foreach(IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
            {
                ApplySettings(sceneComponent);
            }

            EndEditUIScale();
        }

        public void ResetToDefaults()
        {
            DeleteKey("IsGridVisible");
            DeleteKey("IsGridEnabled");
            DeleteKey("GridSize");
            DeleteKey("GridZTest");
            DeleteKey("UIAutoScale");
            DeleteKey("UIScale");
            DeleteKey("ZoomSpeed");
            DeleteKey("ConstantZoomSpeed");
            
            ApplySettings();
        }

        private const string KeyPrefix = "Battlehub.RTEditor.Settings.";

        private void DeleteKey(string propertyName)
        {
            PlayerPrefs.DeleteKey(KeyPrefix + propertyName);
        }

        private void SetString(string propertyName, string value)
        {
            PlayerPrefs.SetString(KeyPrefix + propertyName, value);
        }

        private string GetString(string propertyName, string defaultValue)
        {
            return PlayerPrefs.GetString(KeyPrefix + propertyName, defaultValue);
        }

        private void SetFloat(string propertyName, float value)
        {
            PlayerPrefs.SetFloat(KeyPrefix + propertyName, value);
        }

        private float GetFloat(string propertyName, float defaultValue)
        {
            return PlayerPrefs.GetFloat(KeyPrefix + propertyName, defaultValue);
        }

        private void SetInt(string propertyName, int value)
        {
            PlayerPrefs.SetInt(KeyPrefix + propertyName, value);
        }

        private int GetInt(string propertyName, int defaultValue)
        {
            return PlayerPrefs.GetInt(KeyPrefix + propertyName, defaultValue);
        }

        private void SetBool(string propertyName, bool value)
        {
            PlayerPrefs.SetInt(KeyPrefix + propertyName, value ? 1 : 0);
        }

        private bool GetBool(string propertyName, bool defaultValue)
        {
            return PlayerPrefs.GetInt(KeyPrefix + propertyName, defaultValue ? 1 : 0) != 0;
        }
    }
}

