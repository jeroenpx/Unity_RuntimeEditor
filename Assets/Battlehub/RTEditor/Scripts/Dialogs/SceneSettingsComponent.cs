using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.Utils;
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

        void ResetToDefaults();
    }

    public class SettingsComponent : MonoBehaviour, ISettingsComponent
    {
        private IWindowManager m_wm;

        private Dictionary<Transform, IRuntimeSceneComponent> m_sceneComponents = new Dictionary<Transform, IRuntimeSceneComponent>();

        public bool IsGridVisible
        {
            get { return GetBool("IsGridVisible", true); }
            set
            {
                SetBool("IsGridVisible", value);
                foreach(IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.IsGridVisible = value;
                }
            }
        }

        public bool IsGridEnabled
        {
            get { return GetBool("IsGridEnabled", false); }
            set
            {
                SetBool("IsGridEnabled", value);
                foreach (IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.IsGridEnabled = value;
                }
            }
        }

        public float GridSize
        {
            get { return GetFloat("GridSize", 0.5f); }
            set
            {
                SetFloat("GridSize", value);
                foreach (IRuntimeSceneComponent sceneComponent in m_sceneComponents.Values)
                {
                    sceneComponent.SizeOfGrid = value;
                }
            }
        }

        public bool UIAutoScale
        {
            get { return GetBool("UIAutoScale", true); }
            set
            {
                SetBool("UIAutoScale", value);
                EndEditUIScale();
            }
        }

        public float UIScale
        {
            get { return GetFloat("UIScale", 1); }
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
            appearance.UIScaler.scaleFactor = scale;

            IRuntimeHandlesComponent handles = IOC.Resolve<IRuntimeHandlesComponent>();
            handles.HandleScale = scale;
            handles.SceneGizmoScale = scale;
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
            DeleteKey("UIAutoScale");
            DeleteKey("UIScale");

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

