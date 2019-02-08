using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
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

        bool IsDialogOpened
        {
            get;
        }

        Transform GetWindow(string windowTypeName);
        bool Exists(string windowTypeName);
        bool IsActive(string windowType);
        bool IsActive(Transform content);
        bool ActivateWindow(string windowTypeName);
        bool ActivateWindow(Transform content);
        Transform CreateWindow(string windowTypeName);
        Transform CreateDialog(string windowTypeName, string header, DialogAction<DialogCancelArgs> okAction, DialogAction<DialogCancelArgs> cancelAction = null,
             float minWidth = 250,
             float minHeight = 250,
             float preferredWidth = 700,
             float preferredHeight = 400,
             bool canResize = true);

        void MessageBox(string header, string text, DialogAction<DialogCancelArgs> ok = null);
        void MessageBox(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok = null);
        void Confirmation(string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel");
        void Confirmation(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel");
        
        void Dialog(string header, Transform content, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel",
             float minWidth = 150,
             float minHeight = 150,
             float preferredWidth = 700,
             float preferredHeight = 400);

        void Dialog(Sprite icon, string header, Transform content, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel",
            float minWidth = 150,
             float minHeight = 150,
             float preferredWidth = 700,
             float preferredHeight = 400);
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

    [Serializable]
    public class CustomWindowDescriptor
    {
        public string TypeName;
        public bool IsDialog;
        public WindowDescriptor Descriptor;
    }

    public class WindowManager : MonoBehaviour, IWindowManager
    {
        [SerializeField]
        private DialogManager m_dialogManager = null;

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
        private WindowDescriptor m_saveSceneDialog = null;

        [SerializeField]
        private WindowDescriptor m_openProjectDialog = null;

        [SerializeField]
        private WindowDescriptor m_selectAssetLibraryDialog = null;

        [SerializeField]
        private WindowDescriptor m_importAssetsDialog = null;

        [SerializeField]
        private WindowDescriptor m_aboutDialog = null;

        [SerializeField]
        private WindowDescriptor m_selectObjectDialog = null;

        [SerializeField]
        private WindowDescriptor m_selectColorDialog = null;

        [SerializeField]
        private CustomWindowDescriptor[] m_customWindows = null;

        [SerializeField]
        private DockPanel m_dockPanels = null;

        [SerializeField]
        private Transform m_componentsRoot = null;

        private IRTE m_editor;

        private readonly Dictionary<string, CustomWindowDescriptor> m_typeToCustomWindow = new Dictionary<string, CustomWindowDescriptor>();
        private readonly Dictionary<string, HashSet<Transform>> m_windows = new Dictionary<string, HashSet<Transform>>();
        private readonly Dictionary<Transform, List<Transform>> m_extraComponents = new Dictionary<Transform, List<Transform>>();        

        public bool IsDialogOpened
        {
            get { return m_dialogManager.IsDialogOpened; }
        }

        private void Start()
        {
            if (m_dockPanels == null)
            {
                m_dockPanels = FindObjectOfType<DockPanel>();
            }

            if(m_dialogManager == null)
            {
                m_dialogManager = FindObjectOfType<DialogManager>();
            }

            
            for(int i = 0; i < m_customWindows.Length; ++i)
            {
                CustomWindowDescriptor customWindow = m_customWindows[i];
                if(customWindow != null && customWindow.Descriptor != null && !m_typeToCustomWindow.ContainsKey(customWindow.TypeName))
                {
                    m_typeToCustomWindow.Add(customWindow.TypeName, customWindow);
                }
            }
            
            m_dockPanels.TabActivated += OnTabActivated;
            m_dockPanels.TabDeactivated += OnTabDeactivated;
            m_dockPanels.TabClosed += OnTabClosed;
            m_dockPanels.RegionDepthChanged += OnRegionDepthChanged;
            m_dockPanels.RegionSelected += OnRegionSelected;
            m_dockPanels.RegionUnselected += OnRegionUnselected;
            m_dockPanels.RegionEnabled += OnRegionEnabled;
            m_dockPanels.RegionDisabled += OnRegionDisabled;
            m_dockPanels.RegionMaximized += OnRegionMaximized;

            m_dialogManager.DialogDestroyed += OnDialogDestroyed;

            if (m_componentsRoot == null)
            {
                m_componentsRoot = transform;
            }

            m_editor = IOC.Resolve<IRTE>();
            m_sceneWindow.MaxWindows = m_editor.CameraLayerSettings.MaxGraphicsLayers;
            SetDefaultLayout();

            m_dockPanels.CursorHelper = m_editor.CursorHelper;
        }

        private void Update()
        {
            if(!m_editor.IsInputFieldActive)
            {
                if(m_dialogManager.IsDialogOpened)
                {
                    if(m_editor.Input.GetKeyDown(KeyCode.Escape))
                    {
                        m_dialogManager.CloseDialog();
                    }
                }
            }
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
                m_dockPanels.RegionEnabled -= OnRegionEnabled;
                m_dockPanels.RegionDisabled -= OnRegionDisabled;
                m_dockPanels.RegionMaximized -= OnRegionMaximized;
            }

            if(m_dialogManager != null)
            {
                m_dialogManager.DialogDestroyed -= OnDialogDestroyed;
            }
        }


        private void OnDialogDestroyed(Dialog dialog)
        {
            OnContentDestroyed(dialog.Content);
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


        private void OnRegionDisabled(Region region)
        {
            if(region.ActiveContent != null)
            {
                List<Transform> extraComponents;
                if (m_extraComponents.TryGetValue(region.ActiveContent, out extraComponents))
                {
                    for (int i = 0; i < extraComponents.Count; ++i)
                    {
                        Transform extraComponent = extraComponents[i];
                        if (extraComponent)
                        {
                            extraComponent.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        private void OnRegionEnabled(Region region)
        {
            if(region.ActiveContent != null)
            {
                List<Transform> extraComponents;
                if (m_extraComponents.TryGetValue(region.ActiveContent, out extraComponents))
                {
                    for (int i = 0; i < extraComponents.Count; ++i)
                    {
                        Transform extraComponent = extraComponents[i];
                        extraComponent.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void OnRegionMaximized(Region region, bool maximized)
        {
            if(!maximized)
            {
                RuntimeWindow[] windows = m_dockPanels.RootRegion.GetComponentsInChildren<RuntimeWindow>();
                for(int i = 0; i < windows.Length; ++i)
                {
                    windows[i].HandleResize();
                }
            }
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
                else if(windowTypeName == RuntimeWindowType.SaveScene.ToString().ToLower())
                {
                    wd = m_saveSceneDialog;
                }
                else if(windowTypeName == RuntimeWindowType.OpenProject.ToString().ToLower())
                {
                    wd = m_openProjectDialog;
                }
                else if(windowTypeName == RuntimeWindowType.SelectAssetLibrary.ToString().ToLower())
                {
                    wd = m_selectAssetLibraryDialog;
                }
                else if(windowTypeName == RuntimeWindowType.ImportAssets.ToString().ToLower())
                {
                    wd = m_importAssetsDialog;
                }
                else if(windowTypeName == RuntimeWindowType.About.ToString().ToLower())
                {
                    wd = m_aboutDialog;
                }
                else if(windowTypeName == RuntimeWindowType.SelectObject.ToString().ToLower())
                {
                    wd = m_selectObjectDialog;
                }
                else if(windowTypeName == RuntimeWindowType.SelectColor.ToString().ToLower())
                {
                    wd = m_selectColorDialog;
                }
                else
                {
                    CustomWindowDescriptor cwd;
                    if(m_typeToCustomWindow.TryGetValue(windowTypeName, out cwd))
                    {
                        wd = cwd.Descriptor;
                    }
                }

                if(wd != null)
                {
                    wd.Created--;
                    Debug.Assert(wd.Created >= 0);
                }

                
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
            bool isDialog;
            CreateWindow(RuntimeWindowType.Scene.ToString(), out sceneWd, out sceneContent, out isDialog);

            WindowDescriptor gameWd;
            GameObject gameContent;
            CreateWindow(RuntimeWindowType.Game.ToString(), out gameWd, out gameContent, out isDialog);

            WindowDescriptor inspectorWd;
            GameObject inspectorContent;
            CreateWindow(RuntimeWindowType.Inspector.ToString(), out inspectorWd, out inspectorContent, out isDialog);

            WindowDescriptor consoleWd;
            GameObject consoleContent;
            CreateWindow(RuntimeWindowType.Console.ToString(), out consoleWd, out consoleContent, out isDialog);

            WindowDescriptor hierarchyWd;
            GameObject hierarchyContent;
            CreateWindow(RuntimeWindowType.Hierarchy.ToString(), out hierarchyWd, out hierarchyContent, out isDialog);

            WindowDescriptor projectWd;
            GameObject projectContent;
            CreateWindow(RuntimeWindowType.Project.ToString(), out projectWd, out projectContent, out isDialog);

            LayoutInfo layout = new LayoutInfo(false,
                new LayoutInfo(false,
                    new LayoutInfo(true,
                        new LayoutInfo(inspectorContent.transform, inspectorWd.Header, inspectorWd.Icon),
                        new LayoutInfo(consoleContent.transform, consoleWd.Header, consoleWd.Icon),
                        0.5f),
                    new LayoutInfo(true,
                        new LayoutInfo(sceneContent.transform, sceneWd.Header, sceneWd.Icon),
                        new LayoutInfo(gameContent.transform, gameWd.Header, gameWd.Icon),
                        0.75f),
                    0.25f),
                new LayoutInfo(true,
                    new LayoutInfo(hierarchyContent.transform, hierarchyWd.Header, hierarchyWd.Icon),
                    new LayoutInfo(projectContent.transform, projectWd.Header, projectWd.Icon),
                    0.5f),
                0.75f);

            m_dockPanels.RootRegion.Build(layout);

            ActivateContent(sceneWd, sceneContent);
            ActivateContent(consoleWd, consoleContent);
            ActivateContent(gameWd, gameContent);
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

        public Transform CreateWindow(string windowTypeName)
        {
            WindowDescriptor wd;
            GameObject content;
            bool isDialog;

            Transform window = CreateWindow(windowTypeName, out wd, out content, out isDialog);
            if (!window)
            {
                return window;
            }

            if(isDialog)
            {
                Dialog dialog = m_dialogManager.ShowDialog(wd.Icon, wd.Header, content.transform);
                dialog.IsCancelVisible = false;
                dialog.IsOkVisible = false;
            }
            else
            {
                m_dockPanels.RootRegion.Add(wd.Icon, wd.Header, content.transform, true);
            }
            
            ActivateContent(wd, content);

            return window;
        }

        public Transform CreateDialog(string windowTypeName, string header, DialogAction<DialogCancelArgs> okAction, DialogAction<DialogCancelArgs> cancelAction,
             float minWidth,
             float minHeight,
             float preferredWidth,
             float preferredHeight,
             bool canResize = true)
        {
            WindowDescriptor wd;
            GameObject content;
            bool isDialog;

            Transform window = CreateWindow(windowTypeName, out wd, out content, out isDialog);
            if (!window)
            {
                return window;
            }

            if (isDialog)
            {
                if(header == null)
                {
                    header = wd.Header;
                }
                Dialog dialog = m_dialogManager.ShowDialog(wd.Icon, header, content.transform, okAction, "OK", cancelAction, "Cancel", minWidth, minHeight, preferredWidth, preferredHeight, canResize);
                dialog.IsCancelVisible = false;
                dialog.IsOkVisible = false;
            }
            else
            {
                throw new ArgumentException(windowTypeName + " is not a dialog");
            }

            ActivateContent(wd, content);

            return window;
        }

        private Transform CreateWindow(string windowTypeName, out WindowDescriptor wd, out GameObject content, out bool isDialog)
        {
            if (m_dockPanels == null)
            {
                Debug.LogError("Unable to create window. m_dockPanels == null. Set DockPanels field");
            }

            windowTypeName = windowTypeName.ToLower();
            wd = null;
            content = null;
            isDialog = false;

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
            else if (windowTypeName == RuntimeWindowType.SaveScene.ToString().ToLower())
            {
                wd = m_saveSceneDialog;
                isDialog = true;
            }
            else if (windowTypeName == RuntimeWindowType.OpenProject.ToString().ToLower())
            {
                wd = m_openProjectDialog;
                isDialog = true;
            }
            else if(windowTypeName == RuntimeWindowType.SelectAssetLibrary.ToString().ToLower())
            {
                wd = m_selectAssetLibraryDialog;
                isDialog = true;
            }
            else if(windowTypeName == RuntimeWindowType.ImportAssets.ToString().ToLower())
            {
                wd = m_importAssetsDialog;
                isDialog = true;
            }
            else if(windowTypeName == RuntimeWindowType.About.ToString().ToLower())
            {
                wd = m_aboutDialog;
                isDialog = true;
            }
            else if(windowTypeName == RuntimeWindowType.SelectObject.ToString().ToLower())
            {
                wd = m_selectObjectDialog;
                isDialog = true;
            }
            else if(windowTypeName == RuntimeWindowType.SelectColor.ToString().ToLower())
            {
                wd = m_selectColorDialog;
                isDialog = true;
            }
            else
            {
                CustomWindowDescriptor cwd;
                if(m_typeToCustomWindow.TryGetValue(windowTypeName, out cwd))
                {
                    wd = cwd.Descriptor;
                    isDialog = cwd.IsDialog;
                }
            }

            if (wd == null)
            {
                Debug.LogWarningFormat("{0} window was not found", windowTypeName);
                return null;
            }

            if (wd.Created >= wd.MaxWindows)
            {
                return null;
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
            return content.transform;
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
                window.SetCameraDepth(10 + depth * 5);
            }
        }

        public void MessageBox(string header, string text, DialogAction<DialogCancelArgs> ok = null)
        {
            m_dialogManager.ShowDialog(null, header, text, ok);
        }

        public void MessageBox(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok = null)
        {
            m_dialogManager.ShowDialog(icon, header, text, ok);
        }

        public void Confirmation(string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel")
        {
            m_dialogManager.ShowDialog(null, header, text, ok, okText, cancel, cancelText);

        }
        public void Confirmation(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel")
        {
            m_dialogManager.ShowDialog(icon, header, text, ok, okText, cancel, cancelText);
        }

        public void Dialog(string header, Transform content, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel", 
             float minWidth = 150,
             float minHeight = 150,
             float preferredWidth = 700,
             float preferredHeight = 400)
        {
            m_dialogManager.ShowDialog(null, header, content, ok, okText, cancel, cancelText, minWidth, minHeight, preferredWidth, preferredHeight);
        }

        public void Dialog(Sprite icon, string header, Transform content, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel",
            float minWidth = 150,
             float minHeight = 150,
             float preferredWidth = 700,
             float preferredHeight = 400)
        {
            m_dialogManager.ShowDialog(icon, header, content, ok, okText, cancel, cancelText, minWidth, minHeight, preferredWidth, preferredHeight); 
        }
    }
}

