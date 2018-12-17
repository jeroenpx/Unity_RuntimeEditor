using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public interface IWindowManager
    {
        Transform GetWindow(string windowTypeName);
        bool Exists(string windowTypeName);
        bool IsActive(string windowType);
        bool IsActive(Transform content);
        bool ActivateWindow(string windowTypeName);
        bool ActivateWindow(Transform content);
        bool CreateWindow(string windowTypeName);
    }

    [Serializable]
    public class WindowDescriptor
    {
        public Sprite Icon;
        public string Header;
        public GameObject ContentPrefab;
        public GameObject[] ComponentPrefabs;
        [ReadOnly]
        public int MaxWindows = 1;
        [ReadOnly]
        public int Created = 0;
    }

    public class WindowManager : MonoBehaviour, IWindowManager
    {
        [SerializeField]
        private WindowDescriptor m_sceneWindow = null;

        [SerializeField]
        private WindowDescriptor m_gameWindow = null;

        [SerializeField]
        private WindowDescriptor m_hierarchyWindow = null;

        [SerializeField]
        private WindowDescriptor m_inspectorWindow = null;

        [SerializeField]
        private WindowDescriptor m_projectWindow = null;

        [SerializeField]
        private WindowDescriptor m_consoleWindow = null;

        [SerializeField]
        private DockPanelsRoot m_dockPanels = null;

        [SerializeField]
        private Transform m_componentsRoot = null;

        private IRTE m_editor;

        private readonly Dictionary<string, HashSet<Transform>> m_windows = new Dictionary<string, HashSet<Transform>>();
        private readonly Dictionary<Transform, List<Transform>> m_extraComponents = new Dictionary<Transform, List<Transform>>();

        private void Start()
        {

            if (m_dockPanels == null)
            {
                m_dockPanels = FindObjectOfType<DockPanelsRoot>();
            }

            m_dockPanels.TabActivated += OnTabActivated;
            m_dockPanels.TabDeactivated += OnTabDeactivated;
            m_dockPanels.TabClosed += OnTabClosed;
            m_dockPanels.RegionDepthChanged += OnRegionDepthChanged;

            if (m_componentsRoot == null)
            {
                m_componentsRoot = transform;
            }

            m_editor = IOC.Resolve<IRTE>();
            m_sceneWindow.MaxWindows = m_editor.CameraLayerSettings.MaxGraphicsLayers;
        }



        private void OnDestroy()
        {
            if(m_dockPanels != null)
            {
                m_dockPanels.TabActivated -= OnTabActivated;
                m_dockPanels.TabDeactivated -= OnTabDeactivated;
                m_dockPanels.TabClosed -= OnTabClosed;
                m_dockPanels.RegionDepthChanged -= OnRegionDepthChanged;
            }
        }

        private void OnTabActivated(Region region, Transform content)
        {
            List<Transform> extraComponents;
            if(m_extraComponents.TryGetValue(content, out extraComponents))
            {
                for(int i = 0; i < extraComponents.Count; ++i)
                {
                    Transform extraComponent = extraComponents[i];
                    extraComponent.gameObject.SetActive(true);
                }
            }
        }

        private void OnTabDeactivated(Region region, Transform content)
        {
            List<Transform> extraComponents;
            if (m_extraComponents.TryGetValue(content, out extraComponents))
            {
                for (int i = 0; i < extraComponents.Count; ++i)
                {
                    Transform extraComponent = extraComponents[i];
                    if(extraComponent)
                    {
                        extraComponent.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnTabClosed(Region region, Transform content)
        {
            string windowTypeName = m_windows.Where(kvp => kvp.Value.Contains(content)).Select(kvp => kvp.Key).FirstOrDefault();
            if(!string.IsNullOrEmpty(windowTypeName))
            {
                HashSet<Transform> windowsOfType = m_windows[windowTypeName];
                windowsOfType.Remove(content);

                if(windowsOfType.Count == 0)
                {
                    m_windows.Remove(windowTypeName);
                }

                List<Transform> extraComponents = new List<Transform>();
                if(m_extraComponents.TryGetValue(content, out extraComponents))
                {
                    for(int i = 0; i < extraComponents.Count; ++i)
                    {
                        Destroy(extraComponents[i].gameObject);
                    }
                }

                WindowDescriptor wd = null;
                if (windowTypeName == RuntimeWindowType.Scene.ToString().ToLower())
                {
                    wd = m_sceneWindow;
                }
                else if (windowTypeName == RuntimeWindowType.Game.ToString().ToLower())
                {
                    wd = m_gameWindow;
                }
                else if (windowTypeName == RuntimeWindowType.Hierarchy.ToString().ToLower())
                {
                    wd = m_hierarchyWindow;
                }
                else if(windowTypeName == RuntimeWindowType.Inspector.ToString().ToLower())
                {
                    wd = m_inspectorWindow;
                }
                else if (windowTypeName == RuntimeWindowType.Project.ToString().ToLower())
                {
                    wd = m_projectWindow;
                }
                else if (windowTypeName == RuntimeWindowType.Console.ToString().ToLower())
                {
                    wd = m_consoleWindow;
                }

                wd.Created--;
                Debug.Assert(wd.Created >= 0);
            }
        }

        public bool Exists(string windowTypeName)
        {
            return GetWindow(windowTypeName) != null;
        }

        public Transform GetWindow(string windowTypeName)
        {
            HashSet<Transform> hs;
            if(m_windows.TryGetValue(windowTypeName.ToLower(), out hs))
            {
                return hs.FirstOrDefault();
            }
            return null;
        }

        public bool IsActive(string windowTypeName)
        {
            HashSet<Transform> hs;
            if (m_windows.TryGetValue(windowTypeName.ToLower(), out hs))
            {
                foreach(Transform content in hs)
                {
                    Tab tab = Region.FindTab(content);
                    if(tab != null && tab.IsOn)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsActive(Transform content)
        {
            Tab tab = Region.FindTab(content);
            return tab != null && tab.IsOn;
        }
    
        public bool ActivateWindow(string windowTypeName)
        {
            Transform content = GetWindow(windowTypeName);
            if(content == null)
            {
                return false;
            }
            return ActivateWindow(content);
        }

        public bool ActivateWindow(Transform content)
        {
            Tab tab = Region.FindTab(content);
            if (tab == null)
            {
                return false;
            }

            tab.IsOn = true;
            return true;
        }

        public bool CreateWindow(string windowTypeName)
        {
            if (m_dockPanels == null)
            {
                Debug.LogError("Unable to create window. m_dockPanels == null. Set DockPanels field");
            }

            windowTypeName = windowTypeName.ToLower();

            WindowDescriptor wd = null;
            if (windowTypeName == RuntimeWindowType.Scene.ToString().ToLower())
            {
                wd = m_sceneWindow;
            }
            else if (windowTypeName == RuntimeWindowType.Game.ToString().ToLower())
            {
                wd = m_gameWindow;
            }
            else if (windowTypeName == RuntimeWindowType.Hierarchy.ToString().ToLower())
            {
                wd = m_hierarchyWindow;
            }
            else if (windowTypeName == RuntimeWindowType.Inspector.ToString().ToLower())
            {
                wd = m_inspectorWindow;
            }
            else if (windowTypeName == RuntimeWindowType.Project.ToString().ToLower())
            {
                wd = m_projectWindow;
            }
            else if (windowTypeName == RuntimeWindowType.Console.ToString().ToLower())
            {
                wd = m_consoleWindow;
            }

            if (wd == null)
            {
                Debug.LogWarningFormat("{0} window was not found", windowTypeName);
                return false;
            }

            if (wd.Created >= wd.MaxWindows)
            {
                return false;
            }
            wd.Created++;

            GameObject content;
            if (wd.ContentPrefab != null)
            {
                bool wasActive = wd.ContentPrefab.activeSelf;
                wd.ContentPrefab.SetActive(false);
                content = Instantiate(wd.ContentPrefab);
                content.name = windowTypeName;

                Transform[] children = content.transform.OfType<Transform>().ToArray();
                for (int i = 0; i < wd.ComponentPrefabs.Length; ++i)
                {
                    GameObject componentPrefab = wd.ComponentPrefabs[i];
                    if (componentPrefab != null)
                    {
                        Transform component = children[componentPrefab.transform.GetSiblingIndex()];
                        component.gameObject.SetActive(false);
                        component.transform.SetParent(m_componentsRoot, false);
                    }
                }

                m_dockPanels.RootRegion.Add(wd.Icon, wd.Header, content.transform, true);

                List<Transform> extraComponents = new List<Transform>();

                for (int i = 0; i < children.Length; ++i)
                {
                    if (children[i].parent == m_componentsRoot)
                    {
                        children[i].gameObject.SetActive(true);
                        extraComponents.Add(children[i]);
                    }
                }

                m_extraComponents.Add(content.transform, extraComponents);

                content.SetActive(true);
                wd.ContentPrefab.SetActive(wasActive);
            }
            else
            {
                Debug.LogWarningFormat("{0} WindowDescriptor.ContentPrefab is null", windowTypeName);

                content = new GameObject();
                content.AddComponent<RectTransform>();
                content.name = "Empty Content";
                m_dockPanels.RootRegion.Add(wd.Icon, wd.Header, content.transform, true);

                m_extraComponents.Add(content.transform, new List<Transform>());
            }

            HashSet<Transform> windows;
            if(!m_windows.TryGetValue(windowTypeName, out windows))
            {
                windows = new HashSet<Transform>();
                m_windows.Add(windowTypeName, windows);
            }

            windows.Add(content.transform);
            
            return true;
        }

        private void OnRegionDepthChanged(Region region, int depth)
        {
            RuntimeWindow[] windows = region.GetComponentsInChildren<RuntimeWindow>();
            for(int i = 0; i < windows.Length; ++i)
            {
                RuntimeWindow window = windows[i];

                if (window.Camera != null)
                {
                    window.Camera.depth = depth * 2;
                }
            }
        }
    }
}

