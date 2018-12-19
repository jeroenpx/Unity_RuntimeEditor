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
        void SetDefaultLayout();

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
            m_dockPanels.RegionSelected += OnRegionSelected;
            m_dockPanels.RegionUnselected += OnRegionUnselected;

            if (m_componentsRoot == null)
            {
                m_componentsRoot = transform;
            }

            m_editor = IOC.Resolve<IRTE>();
            m_sceneWindow.MaxWindows = m_editor.CameraLayerSettings.MaxGraphicsLayers;
            SetDefaultLayout();
        }

        private void OnDestroy()
        {
            if(m_dockPanels != null)
            {
                m_dockPanels.TabActivated -= OnTabActivated;
                m_dockPanels.TabDeactivated -= OnTabDeactivated;
                m_dockPanels.TabClosed -= OnTabClosed;
                m_dockPanels.RegionDepthChanged -= OnRegionDepthChanged;
                m_dockPanels.RegionSelected -= OnRegionSelected;
                m_dockPanels.RegionUnselected -= OnRegionUnselected;
            }
        }

        private void OnRegionSelected(Region region)
        {
            RuntimeWindow window = region.ContentPanel.GetComponentInChildren<RuntimeWindow>();
            if (window != null)
            {
                window.Editor.ActivateWindow(window);
            }
        }

        private void OnRegionUnselected(Region region)
        {

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

            RuntimeWindow window = region.ContentPanel.GetComponentInChildren<RuntimeWindow>();
            if(window != null)
            {
                window.Editor.ActivateWindow(window);
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
            OnContentDestroyed(content);
        }

        private void OnContentDestroyed(Transform content)
        {
            string windowTypeName = m_windows.Where(kvp => kvp.Value.Contains(content)).Select(kvp => kvp.Key).FirstOrDefault();
            if (!string.IsNullOrEmpty(windowTypeName))
            {
                HashSet<Transform> windowsOfType = m_windows[windowTypeName];
                windowsOfType.Remove(content);

                if (windowsOfType.Count == 0)
                {
                    m_windows.Remove(windowTypeName);
                }

                List<Transform> extraComponents = new List<Transform>();
                if (m_extraComponents.TryGetValue(content, out extraComponents))
                {
                    for (int i = 0; i < extraComponents.Count; ++i)
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

                wd.Created--;
                Debug.Assert(wd.Created >= 0);
            }
        }

        public void SetDefaultLayout()
        {
            Region rootRegion = m_dockPanels.RootRegion;
            ClearRegion(rootRegion);
            foreach(Transform child in m_dockPanels.Free)
            {
                Region region = child.GetComponent<Region>();
                ClearRegion(region);
            }

            WindowDescriptor sceneWd;
            GameObject sceneContent;
            CreateWindow(RuntimeWindowType.Scene.ToString(), out sceneWd, out sceneContent);

            WindowDescriptor gameWd;
            GameObject gameContent;
            CreateWindow(RuntimeWindowType.Game.ToString(), out gameWd, out gameContent);

            WindowDescriptor inspectorWd;
            GameObject inspectorContent;
            CreateWindow(RuntimeWindowType.Inspector.ToString(), out inspectorWd, out inspectorContent);

            WindowDescriptor consoleWd;
            GameObject consoleContent;
            CreateWindow(RuntimeWindowType.Console.ToString(), out consoleWd, out consoleContent);

            WindowDescriptor hierarchyWd;
            GameObject hierarchyContent;
            CreateWindow(RuntimeWindowType.Hierarchy.ToString(), out hierarchyWd, out hierarchyContent);

            WindowDescriptor projectWd;
            GameObject projectContent;
            CreateWindow(RuntimeWindowType.Project.ToString(), out projectWd, out projectContent);

            LayoutInfo layout = new LayoutInfo(false,
                new LayoutInfo(false,
                    new LayoutInfo(true,
                        new LayoutInfo(inspectorContent.transform, inspectorWd.Header, inspectorWd.Icon),
                        new LayoutInfo(consoleContent.transform, consoleWd.Header, consoleWd.Icon),
                        0.7f),
                    new LayoutInfo(true,
                        new LayoutInfo(sceneContent.transform, sceneWd.Header, sceneWd.Icon),
                        new LayoutInfo(gameContent.transform, gameWd.Header, gameWd.Icon),
                        0.5f),
                    0.25f),
                new LayoutInfo(true,
                    new LayoutInfo(hierarchyContent.transform, hierarchyWd.Header, hierarchyWd.Icon),
                    new LayoutInfo(projectContent.transform, projectWd.Header, projectWd.Icon),
                    0.5f),
                1.0f);

            m_dockPanels.RootRegion.Build(layout);

            ActivateContent(sceneWd, sceneContent);
            ActivateContent(projectWd, projectContent);
            ActivateContent(hierarchyWd, hierarchyContent);
            ActivateContent(inspectorWd, inspectorContent);
        }

        private void ClearRegion(Region rootRegion)
        {
            Region[] regions = rootRegion.GetComponentsInChildren<Region>();
            for (int i = 0; i < regions.Length; ++i)
            {
                Region region = regions[i];
                foreach (Transform content in region.ContentPanel)
                {
                    OnContentDestroyed(content);
                }
            }
            rootRegion.Clear();
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
            WindowDescriptor wd;
            GameObject content;
            if (!CreateWindow(windowTypeName, out wd, out content))
            {
                return false;
            }

            m_dockPanels.RootRegion.Add(wd.Icon, wd.Header, content.transform, true);

            ActivateContent(wd, content);

            return true;
        }

        private bool CreateWindow(string windowTypeName, out WindowDescriptor wd, out GameObject content)
        {
            if (m_dockPanels == null)
            {
                Debug.LogError("Unable to create window. m_dockPanels == null. Set DockPanels field");
            }

            windowTypeName = windowTypeName.ToLower();
            wd = null;
            content = null;
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

            if (wd.ContentPrefab != null)
            {
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


                List<Transform> extraComponents = new List<Transform>();
                for (int i = 0; i < children.Length; ++i)
                {
                    if (children[i].parent == m_componentsRoot)
                    {
                        extraComponents.Add(children[i]);
                    }
                }

                m_extraComponents.Add(content.transform, extraComponents);
            }
            else
            {
                Debug.LogWarningFormat("{0} WindowDescriptor.ContentPrefab is null", windowTypeName);

                content = new GameObject();
                content.AddComponent<RectTransform>();
                content.name = "Empty Content";

                m_extraComponents.Add(content.transform, new List<Transform>());
            }

            HashSet<Transform> windows;
            if (!m_windows.TryGetValue(windowTypeName, out windows))
            {
                windows = new HashSet<Transform>();
                m_windows.Add(windowTypeName, windows);
            }

            windows.Add(content.transform);
            return true;
        }

        private void ActivateContent(WindowDescriptor wd, GameObject content)
        {
            List<Transform> extraComponentsList = new List<Transform>();
            m_extraComponents.TryGetValue(content.transform, out extraComponentsList);
            for (int i = 0; i < extraComponentsList.Count; ++i)
            {
                extraComponentsList[i].gameObject.SetActive(true);
            }

            wd.ContentPrefab.SetActive(true);
            content.SetActive(true);
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

