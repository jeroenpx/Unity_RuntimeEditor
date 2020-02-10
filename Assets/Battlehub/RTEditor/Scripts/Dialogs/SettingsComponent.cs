using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [Serializable]
    public struct InspectorSettings
    {
        [Serializable]
        public struct ComponentEditorSettings
        {
            public bool ShowResetButton;
            public bool ShowExpander;
            public bool ShowEnableButton;
            public bool ShowRemoveButton;

            public ComponentEditorSettings(bool showExpander, bool showResetButton, bool showEnableButton, bool showRemoveButton)
            {
                ShowResetButton = showResetButton;
                ShowExpander = showExpander;
                ShowEnableButton = showEnableButton;
                ShowRemoveButton = showRemoveButton;
            }
        }

        public ComponentEditorSettings ComponentEditor;
        public bool ShowAddComponentButton;

        public InspectorSettings(ComponentEditorSettings componentEditorSettings, bool showAddComponentButton)
        {
            ComponentEditor = componentEditorSettings;
            ShowAddComponentButton = showAddComponentButton;
        }

        public InspectorSettings(bool showAddComponentButton)
        {
            ComponentEditor = new ComponentEditorSettings(true, true, true, true);
            ShowAddComponentButton = showAddComponentButton;
        }
    }

    [Serializable]
    public class BuiltInWindowsSettings
    {
        public static readonly BuiltInWindowsSettings Default = new BuiltInWindowsSettings();

        public InspectorSettings Inspector;
        public BuiltInWindowsSettings()
        {
            Inspector = new InspectorSettings(true);
        }
    }


    public interface ISettingsComponent
    {
        event EventHandler ResetToDefaultsEvent;

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

        float FreeRotationSmoothSpeed
        {
            get;
            set;
        }

        bool RotationInvertX
        {
            get;
            set;
        }

        bool RotationInvertY
        {
            get;
            set;
        }

        float FreeMovementSmoothSpeed
        {
            get;
            set;
        }
        
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

        BuiltInWindowsSettings BuiltInWindowsSettings
        {
            get;
        }

        IList<GameObject> CustomSettings
        {
            get;
        }

        void ResetToDefaults();
        void RegisterCustomSettings(GameObject prefab);
        void UnregsiterCustomSettings(GameObject prefab);
    }

    [DefaultExecutionOrder(-1)]
    public class SettingsComponent : MonoBehaviour, ISettingsComponent
    {
        public event EventHandler ResetToDefaultsEvent;

        private IWindowManager m_wm;

        private Dictionary<Transform, IRuntimeSceneComponent> m_sceneComponents = new Dictionary<Transform, IRuntimeSceneComponent>();

        [SerializeField]
        private BuiltInWindowsSettings m_windowSettingsDefault = new BuiltInWindowsSettings();
        public BuiltInWindowsSettings BuiltInWindowsSettings
        {
            get { return m_windowSettingsDefault; }
        }

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
        private float m_freeMovementSmoothSpeed = 10.0f;
        public float FreeMovementSmoothSpeed
        {
            get { return GetFloat("FreeMovementSmoothSpeed", m_freeMovementSmoothSpeed); }
            set
            {
                SetFloat("FreeMovementSmoothSpeed", value);
                foreach (IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.FreeMovementSmoothSpeed = value;
                }
            }
        }

        [SerializeField]
        private float m_freeRotationSmoothSpeed = 10.0f;
        public float FreeRotationSmoothSpeed
        {
            get { return GetFloat("FreeRotationSmoothSpeed", m_freeRotationSmoothSpeed); }
            set
            {
                SetFloat("FreeRotationSmoothSpeed", value);
                foreach (IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.FreeRotationSmoothSpeed = value;
                }
            }
        }

        [SerializeField]
        private bool m_rotationInvertX = false;
        public bool RotationInvertX
        {
            get { return GetBool("RotationInvertX", m_rotationInvertX); }
            set
            {
                SetBool("RotationInvertX", value);
                foreach (IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.RotationInvertX = value;
                }
            }
        }

        [SerializeField]
        private bool m_rotationInvertY = false;
        public bool RotationInvertY
        {
            get { return GetBool("RotationInvertY", m_rotationInvertY); }
            set
            {
                SetBool("RotationInvertY", value);
                foreach (IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.RotationInvertY = value;
                }
            }
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

        private List<GameObject> m_customSettings = new List<GameObject>();
        public IList<GameObject> CustomSettings
        {
            get { return m_customSettings; }
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
            sceneComponent.FreeRotationSmoothSpeed = FreeRotationSmoothSpeed;
            sceneComponent.RotationInvertX = RotationInvertX;
            sceneComponent.RotationInvertY = RotationInvertY;
            sceneComponent.FreeMovementSmoothSpeed = FreeMovementSmoothSpeed;
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
            DeleteKey("FreeRotationSmoothSpeed");
            DeleteKey("RotationInvertX");
            DeleteKey("RotationInvertY");
            DeleteKey("FreeMovementSmoothSpeed");
            DeleteKey("ZoomSpeed");
            DeleteKey("ConstantZoomSpeed");

            if(ResetToDefaultsEvent != null)
            {
                ResetToDefaultsEvent(this, EventArgs.Empty);
            }
            
            ApplySettings();
        }

        public void RegisterCustomSettings(GameObject prefab)
        {
            m_customSettings.Add(prefab);
        }

        public void UnregsiterCustomSettings(GameObject prefab)
        {
            m_customSettings.Remove(prefab);
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

