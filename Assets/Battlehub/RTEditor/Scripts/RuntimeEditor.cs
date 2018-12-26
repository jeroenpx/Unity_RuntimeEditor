using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls.DockPanels;
using Battlehub.UIControls.MenuControl;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Battlehub.RTEditor
{
    public interface IRuntimeEditor : IRTE
    {
        void CreateWindow(string window);
        void CreateOrActivateWindow(string window);

        void NewScene();
        void SaveScene();

        void CmdGameObject(string cmd);
        void RegisterCreateObject(GameObject go);
        void Close();
    }

    [RequireComponent(typeof(RTEObjects))]
    public class RuntimeEditor : RTEBase, IRuntimeEditor
    {
        private IProject m_project;
        

        [SerializeField]
        private GameObject m_progressIndicator = null;

        public override bool IsBusy
        {
            get { return base.IsBusy; }
            set
            {
                if(m_progressIndicator != null)
                {
                    m_progressIndicator.gameObject.SetActive(value);
                }

                base.IsBusy = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
      
            IOC.Resolve<IRTEAppearance>();
            m_project = IOC.Resolve<IProject>();
        }

        protected override void Start()
        {
            if (GetComponent<RuntimeEditorInput>() == null)
            {
                gameObject.AddComponent<RuntimeEditorInput>();
            }
            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

        }

        protected override void Update()
        {
            if (m_input.GetPointerDown(0))
            {
                PointerEventData pointerEventData = new PointerEventData(m_eventSystem);
                //Set the Pointer Event Position to that of the mouse position
                pointerEventData.position = m_input.GetPointerXY(0);

                //Create a list of Raycast Results
                List<RaycastResult> results = new List<RaycastResult>();

                //Raycast using the Graphics Raycaster and mouse click position
                m_raycaster.Raycast(pointerEventData, results);

                //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
                foreach (Region region in results.Select(r => r.gameObject.GetComponentInParent<Region>()))
                {
                    if(region == null)
                    {
                        continue;
                    }

                    RuntimeWindow window = region.ActiveContent != null ? region.ActiveContent.GetComponentInChildren<RuntimeWindow>() :
                        region.ContentPanel.GetComponentInChildren<RuntimeWindow>();
                    if(window != null)
                    {
                        if (m_windows.Contains(window.gameObject))
                        {
                            ActivateWindow(window);
                            break;
                        }
                    }
                }
            }
        }

        public void SetDefaultLayout()
        {
            IWindowManager windowManager = IOC.Resolve<IWindowManager>();
            windowManager.SetDefaultLayout();
        }

        public virtual void CreateWindow(string windowTypeName)
        {
            IWindowManager windowManager = IOC.Resolve<IWindowManager>();
            windowManager.CreateWindow(windowTypeName);
        }

        public virtual void ValidateProjectIsOpened(MenuItemValidationArgs validation)
        {
            validation.IsValid = m_project.IsOpened;
        }

        public virtual void CreateOrActivateWindow(string windowTypeName)
        {
            IWindowManager windowManager = IOC.Resolve<IWindowManager>();

            if(!windowManager.CreateWindow(windowTypeName))
            {
                if (windowManager.Exists(windowTypeName))
                {
                    if(!windowManager.IsActive(windowTypeName))
                    {
                        windowManager.ActivateWindow(windowTypeName);

                        Transform windowTransform = windowManager.GetWindow(windowTypeName);

                        RuntimeWindow window = windowTransform.GetComponentInChildren<RuntimeWindow>();
                        if (window != null)
                        {
                            base.ActivateWindow(window);
                        }
                    }
                }
            }
        }

        public override void ActivateWindow(RuntimeWindow window)
        {
            base.ActivateWindow(window);

            if(window != null)
            {
                IWindowManager windowManager = IOC.Resolve<IWindowManager>();
                windowManager.ActivateWindow(window.transform);
            }
        }

        public virtual void NewScene()
        {
            m_project.CreateNewScene();
        }

        public virtual void SaveScene()
        {
            IWindowManager windowManager = IOC.Resolve<IWindowManager>();
            if (m_project.LoadedScene == null)
            {
                windowManager.CreateWindow(RuntimeWindowType.SaveScene.ToString());
            }
            else
            {
                Undo.Purge();
                IsBusy = true;
                m_project.Delete(new[] { m_project.LoadedScene }, (deleteError, result) =>
                {
                    IsBusy = false;
                    if (deleteError.HasError)
                    {
                        windowManager.MessageBox("Unable to save scene", deleteError.ErrorText);
                    }
                    IsBusy = true;
                    m_project.Create(m_project.Root, new byte[0], SceneManager.GetActiveScene(), m_project.LoadedScene.Name, (error, assetItem) =>
                    {
                        m_project.LoadedScene = assetItem;

                        IsBusy = false;
                        if (error.HasError)
                        {
                            windowManager.MessageBox("Unable to save scene", error.ErrorText);
                        }
                    });
                });
            }
        }

        public void CmdGameObject(string cmd)
        {
            IGameObjectCmd goCmd = IOC.Resolve<IGameObjectCmd>();
            if(goCmd != null)
            {
                goCmd.Exec(cmd);
            }
        }

        public void RegisterCreateObject(GameObject go)
        {
            Undo.BeginRecord();
            Undo.RecordSelection();
            Undo.BeginRegisterCreateObject(go);
            Undo.EndRecord();

            bool isEnabled = Undo.Enabled;
            Undo.Enabled = false;
            Selection.activeGameObject = go;
            Undo.Enabled = isEnabled;

            Undo.BeginRecord();
            Undo.RegisterCreatedObject(go);
            Undo.RecordSelection();
            Undo.EndRecord();
        }

        public void Close()
        {
            Destroy(gameObject);
        }

        

        #region Commented Out
        /*
        public RTHandles.Grid Grid;
        public BoxSelection BoxSelect;
        public SceneGizmo SceneGizmo;
        public GameObject EditButton;
        public GameObject CloseButton;
        public GameObject EditorRoot;

        public RuntimeSceneComponent SceneView;
        public GameObject GameView;
        private ViewportFitter m_gameViewViewportFitter;

        public KeyCode RuntimeModifierKey = KeyCode.LeftControl;
        public KeyCode EditorModifierKey = KeyCode.LeftShift;
        public KeyCode ModifierKey
        {
            get
            {
                #if UNITY_EDITOR
                return EditorModifierKey;
                #else
                return RuntimeModifierKey;
                #endif
            }
        }
        public KeyCode DuplicateKey = KeyCode.D;
        public KeyCode DeleteKey = KeyCode.Delete;
        public KeyCode EnterPlayModeKey = KeyCode.P;

        public GameObject GamePrefab;
        private GameObject m_game;

        private bool m_isStarted;
        public bool IsOn
        {
            get { return RuntimeEditorApplication.IsOpened; }
            set { RuntimeEditorApplication.IsOpened = value; }
        }

        private static RuntimeEditor m_instance;
        public static RuntimeEditor Instance
        {
            get { return m_instance; }
        }

        private IProjectManager m_projectManager;
        public IProjectManager ProjectManager
        {
            get { return m_projectManager; }
        }

        private void Awake()
        {   
            if (m_instance != null)
            {
                Debug.LogWarning("Another instance of RuntimeEditor exists");
            }
            m_instance = this;

            m_projectManager = Dependencies.ProjectManager;
            if(m_projectManager == null)
            {
                Debug.LogWarning("ProjectManager not found");
            }

            if(m_projectManager != null)
            {
                m_projectManager.IgnoreTypes(
                    typeof(SpriteGizmo),
                    typeof(AudioReverbZoneGizmo),
                    typeof(AudioSourceGizmo),
                    typeof(BoxColliderGizmo),
                    typeof(CapsuleColliderGizmo),
                    typeof(SphereColliderGizmo),
                    typeof(LightGizmo),
                    typeof(DirectionalLightGizmo),
                    typeof(PointLightGizmo),
                    typeof(SpotlightGizmo),
                    typeof(SkinnedMeshRendererGizmo));

                m_projectManager.SceneLoaded += OnSceneLoaded;
                m_projectManager.SceneCreated += OnSceneCreated;
            }
         
            RuntimeEditorApplication.IsOpenedChanged += OnIsOpenedChanged;
            RuntimeEditorApplication.PlaymodeStateChanging += OnPlaymodeStateChanging;

            ExposeToEditor[] editorObjects = ExposeToEditor.FindAll(ExposeToEditorObjectType.Undefined, false).Select(go => go.GetComponent<ExposeToEditor>()).ToArray();
            for (int i = 0; i < editorObjects.Length; ++i)
            {
                editorObjects[i].ObjectType = ExposeToEditorObjectType.EditorMode;
                editorObjects[i].Init();
            }

            ExposeToEditor.Awaked += OnObjectAwaked;
            ExposeToEditor.Started += OnObjectStarted;
            RuntimeEditorApplication.SceneCameras = SceneCameras;

            PrepareCameras();
            RuntimeEditorApplication.SceneCameras[0].gameObject.SetActive(true);
            for (int i = 1; i < RuntimeEditorApplication.SceneCameras.Length; ++i)
            {
                RuntimeEditorApplication.SceneCameras[i].gameObject.SetActive(false);
            }

            EditorsMap.LoadMap();
        }

        private void OnDestroy()
        {
            if (m_instance == this)
            {
                m_instance = null;
                RuntimeEditorApplication.Reset();
                EditorsMap.Reset();
            }

            Unsubscribe();
        }

        private void OnApplicationQuit()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if(m_projectManager != null)
            {
                m_projectManager.SceneLoaded -= OnSceneLoaded;
                m_projectManager.SceneCreated -= OnSceneCreated;
            }
            
            RuntimeEditorApplication.IsOpenedChanged -= OnIsOpenedChanged;
            RuntimeEditorApplication.PlaymodeStateChanging -= OnPlaymodeStateChanging;
            BoxSelection.Filtering -= OnBoxSelectionFiltering;
            ExposeToEditor.Awaked -= OnObjectAwaked;
            ExposeToEditor.Started -= OnObjectStarted;
            ExposeToEditor.Enabled -= OnObjectEnabled;
            ExposeToEditor.Disabled -= OnObjectDisabled;
            ExposeToEditor.Destroyed -= OnObjectDestroyed;
            ExposeToEditor.MarkAsDestroyedChanged -= OnObjectMarkAsDestoryedChanged;
            GameCamera.Awaked -= OnGameCameraAwaked;
            GameCamera.Destroyed -= OnGameCameraDestroyed;
            GameCamera.Enabled -= OnGameCameraEnabled;
            GameCamera.Disabled -= OnGameCameraDisabled;
        }

        private void OnEnable()
        {
            GameCamera.Awaked += OnGameCameraAwaked;
            GameCamera.Destroyed += OnGameCameraDestroyed;
            GameCamera.Enabled += OnGameCameraEnabled;
            GameCamera.Disabled += OnGameCameraDisabled;
        }

        private void OnDisable()
        {
            GameCamera.Awaked -= OnGameCameraAwaked;
            GameCamera.Destroyed -= OnGameCameraDestroyed;
            GameCamera.Enabled -= OnGameCameraEnabled;
            GameCamera.Disabled -= OnGameCameraDisabled;
        }

        private void LateUpdate()
        {
            if (RuntimeEditorApplication.ActiveWindowType != RuntimeWindowType.Hierarchy &&
               RuntimeEditorApplication.ActiveWindowType != RuntimeWindowType.SceneView)
            {
                return;
            }

            if (InputController._GetKeyDown(SwitchSceneCameraKey))
            {
                SwitchSceneCamera();
            }

            if (InputController._GetKeyDown(EnterPlayModeKey) && InputController._GetKey(ModifierKey))
            {
                RuntimeEditorApplication.IsPlaying = !RuntimeEditorApplication.IsPlaying;
            }
            if (InputController._GetKeyDown(DuplicateKey) && InputController._GetKey(ModifierKey))
            {
                Duplicate();
            }
            else if (InputController._GetKeyDown(DeleteKey))
            {
                Delete();
            }
        }

        private void Start()
        {
            BoxSelection.Filtering += OnBoxSelectionFiltering;
            ExposeToEditor.Enabled += OnObjectEnabled;
            ExposeToEditor.Disabled += OnObjectDisabled;
            ExposeToEditor.Destroyed += OnObjectDestroyed;
            ExposeToEditor.MarkAsDestroyedChanged += OnObjectMarkAsDestoryedChanged;

            m_isStarted = true;
            if (RuntimeEditorApplication.IsOpened)
            {
                ShowEditor();
            }
            else
            {
                CloseEditor();
            }
        }


        private void PrepareCameras()
        {
            if (GameView)
            {
                m_gameViewViewportFitter = GameView.GetComponentInChildren<ViewportFitter>();
                RuntimeEditorApplication.GameCameras = FindObjectsOfType<Camera>().Where(
                        c => (Grid == null || c.gameObject != Grid.gameObject) &&
                        (RuntimeEditorApplication.ActiveSceneCamera == null || c.gameObject != RuntimeEditorApplication.ActiveSceneCamera.gameObject) &&
                        (SceneGizmo == null || c.gameObject != SceneGizmo.gameObject)).OrderBy(c => c != Camera.main).ToArray();

                for (int i = 0; i < RuntimeEditorApplication.GameCameras.Length; ++i)
                {
                    Camera camera = RuntimeEditorApplication.GameCameras[i];
                    if (!camera.GetComponent<GameCamera>())
                    {
                        camera.gameObject.AddComponent<GameCamera>();
                    }
                }
                if (RuntimeEditorApplication.GameCameras.Length == 0 && RuntimeEditorApplication.ActiveSceneCamera == null)
                {
                    Debug.LogError("No cameras found");
                    return;
                }

                if (RuntimeEditorApplication.SceneCameras == null || RuntimeEditorApplication.SceneCameras.Length == 0)
                {
                    RuntimeEditorApplication.SceneCameras = new Camera[1];
                }

                for (int i = 0; i < RuntimeEditorApplication.SceneCameras.Length; ++i)
                {
                    if (RuntimeEditorApplication.SceneCameras[i] == null)
                    {
                        GameObject editorCameraGO = new GameObject();

                        RuntimeEditorApplication.SceneCameras[i] = editorCameraGO.AddComponent<Camera>();
                        editorCameraGO.transform.SetParent(transform);
                        RuntimeEditorApplication.SceneCameras[i].transform.position = RuntimeEditorApplication.GameCameras[0].transform.position;
                        RuntimeEditorApplication.SceneCameras[i].transform.rotation = RuntimeEditorApplication.GameCameras[0].transform.rotation;
                        RuntimeEditorApplication.SceneCameras[i].transform.localScale = RuntimeEditorApplication.GameCameras[0].transform.localScale;
                        RuntimeEditorApplication.SceneCameras[i].tag = "Untagged";
                        RuntimeEditorApplication.SceneCameras[i].name = "Editor Camera";
                    }

                    if (!RuntimeEditorApplication.SceneCameras[i].GetComponent<GLCamera>())
                    {
                        RuntimeEditorApplication.SceneCameras[i].gameObject.AddComponent<GLCamera>();
                    }
                }
            }
            else
            {
                if (RuntimeEditorApplication.SceneCameras == null || RuntimeEditorApplication.SceneCameras.Length == 0)
                {
                    RuntimeEditorApplication.SceneCameras = new[] { Camera.main };
                }
            }
            

            SceneView.SceneCamera = RuntimeEditorApplication.ActiveSceneCamera;// SetSceneCamera(RuntimeEditorApplication.ActiveSceneCamera);
            ViewportFitter fitter = SceneView.GetComponent<ViewportFitter>();
            fitter.Cameras = new[] { RuntimeEditorApplication.ActiveSceneCamera };
            fitter.gameObject.SetActive(false);
            fitter.gameObject.SetActive(true);
        }

        private void InitGameView()
        {
            if(!GameView)
            {
                RuntimeEditorApplication.GameCameras = new Camera[0];
                return;
            }
            GameCamera[] cameras = FindObjectsOfType<GameCamera>();
            GameView.SetActive(cameras.Length > 0);

            RuntimeEditorApplication.GameCameras = cameras.Select(g => g.GetComponent<Camera>()).ToArray();
            if (m_gameViewViewportFitter != null)
            {
                m_gameViewViewportFitter.Cameras = RuntimeEditorApplication.GameCameras;
                m_gameViewViewportFitter.gameObject.SetActive(false);
                m_gameViewViewportFitter.gameObject.SetActive(true);
            }
        }
     
        private void OnIsOpenedChanged()
        {
            if (RuntimeEditorApplication.IsOpened)
            {
                ShowEditor();
            }
            else
            {
                CloseEditor();
            }
        }

        private void OnPlaymodeStateChanging()
        {
            if (RuntimeEditorApplication.IsPlaying)
            {
                if (IsOn)
                {
                    if (GamePrefab != null)
                    {
                        m_game = Instantiate(GamePrefab);
                    }
                    else
                    {
                        Debug.Log("GamePrefab is not set");
                    }
                    Play.Invoke();
                }   
            }
            else
            {
                if(IsOn)
                {
                    if (m_game != null)
                    {
                        DestroyImmediate(m_game);
                    }
                    Stop.Invoke();
                }
            }
        }

        private void OnGameCameraDestroyed()
        {
            InitGameView();
        }

        private void OnGameCameraAwaked()
        {
            InitGameView();
        }

        private void OnGameCameraEnabled()
        {
            InitGameView();
        }

        private void OnGameCameraDisabled()
        {
            InitGameView();
        }

        private void OnObjectAwaked(ExposeToEditor obj)
        {
            if(!m_isStarted)
            {
                if (obj.ObjectType == ExposeToEditorObjectType.Undefined)
                {
                    obj.ObjectType = ExposeToEditorObjectType.EditorMode;
                }
            }

            if (RuntimeEditorApplication.IsPlaying || !IsOn)
            {
                if (obj.ObjectType == ExposeToEditorObjectType.Undefined)
                {
                    obj.ObjectType = ExposeToEditorObjectType.PlayMode;
                }
            }
            else
            {
                if (obj.ObjectType == ExposeToEditorObjectType.Undefined)
                {
                    obj.ObjectType = ExposeToEditorObjectType.EditorMode;
                }
            }
        }

        private void OnObjectStarted(ExposeToEditor obj)
        {
            //obj.gameObject.layer = SceneView.RaycastLayer;
        }

        private void OnObjectEnabled(ExposeToEditor obj)
        {
           // obj.gameObject.layer = SceneView.RaycastLayer;
        }

        private void OnObjectDisabled(ExposeToEditor obj)
        {
        }

        private void OnObjectDestroyed(ExposeToEditor obj)
        {
            
        }

        private void OnObjectMarkAsDestoryedChanged(ExposeToEditor obj)
        {
        }

        private IEnumerator CoActivateSceneCamera()
        {
            yield return new WaitForEndOfFrame();
            RuntimeEditorApplication.ActiveSceneCamera.gameObject.SetActive(true);
            for (int i = 0; i < RuntimeEditorApplication.GameCameras.Length; ++i)
            {
                Camera cam = RuntimeEditorApplication.GameCameras[i];
                if (cam != null)
                {
                    cam.enabled = true;
                }
            }
        }

        private void ShowEditor()
        {
            if (RuntimeEditorApplication.ActiveSceneCamera != null)
            {
                StartCoroutine(CoActivateSceneCamera());
            }
            if(EditButton) EditButton.SetActive(false);
            if (EditorRoot) EditorRoot.SetActive(true);

            InitEditorComponents();
            Opened.Invoke();

            if (m_game != null)
            {
                DestroyImmediate(m_game);
            }

            for (int i = 0; i < RuntimeEditorApplication.GameCameras.Length; ++i)
            {
                Camera cam = RuntimeEditorApplication.GameCameras[i];
                if(cam != null)
                {
                    cam.enabled = false;
                }
            }
        }

        private void InitEditorComponents()
        {
            if (SceneGizmo != null)
            {
                SceneGizmo.SetSceneCamera(RuntimeEditorApplication.ActiveSceneCamera);
                SceneGizmo.gameObject.SetActive(true);
            }
            if (BoxSelect != null)
            {
                BoxSelect.SetSceneCamera(RuntimeEditorApplication.ActiveSceneCamera);
                BoxSelect.gameObject.SetActive(true);
            }
            if (Grid != null)
            {
                Grid.SceneCamera = RuntimeEditorApplication.ActiveSceneCamera;
                Grid.gameObject.SetActive(true);
            }

            BaseGizmo[] gizmos = FindObjectsOfType<BaseGizmo>();
            for(int i = 0; i < gizmos.Length; ++i)
            {
                BaseGizmo gizmo = gizmos[i];
                gizmo.SceneCamera = RuntimeEditorApplication.ActiveSceneCamera;
            }

            InitGameView();
        }

        private void CloseEditor()
        {
            if (SceneGizmo != null)
            {
                SceneGizmo.gameObject.SetActive(false);
            }

            if (BoxSelect != null)
            {
                BoxSelect.gameObject.SetActive(false);
            }

            if (Grid != null)
            {
                Grid.gameObject.SetActive(false);
            }
            if(EditButton) EditButton.SetActive(true);
            if(EditorRoot) EditorRoot.SetActive(false);

            if (RuntimeEditorApplication.ActiveSceneCamera != null)
            {
                RuntimeEditorApplication.ActiveSceneCamera.gameObject.SetActive(
                    RuntimeEditorApplication.GameCameras == null || RuntimeEditorApplication.GameCameras.Length == 0);
            }

            RuntimeEditorApplication.IsPlaying = false;
            if (m_game == null)
            {
                if (GamePrefab != null)
                {
                    m_game = Instantiate(GamePrefab);
                }
            }

            Closed.Invoke();
        }

        public void SwitchSceneCamera()
        {
            if (RuntimeEditorApplication.SceneCameras.Length <= 1)
            {
                return;
            }
            RuntimeEditorApplication.SceneCameras[RuntimeEditorApplication.ActiveSceneCameraIndex].gameObject.SetActive(false);
            RuntimeEditorApplication.ActiveSceneCameraIndex++;
            RuntimeEditorApplication.ActiveSceneCameraIndex %= RuntimeEditorApplication.SceneCameras.Length;
            RuntimeEditorApplication.SceneCameras[RuntimeEditorApplication.ActiveSceneCameraIndex].gameObject.SetActive(true);
            PrepareCameras();
            InitEditorComponents();
        }

        public void Duplicate()
        {
            Object[] selectedObjects = RuntimeSelection.objects;
            if (selectedObjects != null && selectedObjects.Length > 0)
            {
                RuntimeUndo.BeginRecord();
                Object[] duplicates = new Object[selectedObjects.Length];
                for (int i = 0; i < duplicates.Length; ++i)
                {
                    GameObject go = selectedObjects[i] as GameObject;
                    Object duplicate = Instantiate(go, go.transform.position, go.transform.rotation);
                    GameObject duplicateGo = duplicate as GameObject;
                    
                    if (go != null && duplicateGo != null)
                    {
                        duplicateGo.SetActive(true);
                        duplicateGo.SetActive(go.activeSelf);
                        if (go.transform.parent != null)
                        {
                            duplicateGo.transform.SetParent(go.transform.parent, true);
                        }
                    }

                    duplicates[i] = duplicate;
                    RuntimeUndo.BeginRegisterCreateObject(duplicateGo);
                }
                RuntimeUndo.RecordSelection();
                RuntimeUndo.EndRecord();

                bool isEnabled = RuntimeUndo.Enabled;
                RuntimeUndo.Enabled = false;
                RuntimeSelection.objects = duplicates;
                RuntimeUndo.Enabled = isEnabled;

                RuntimeUndo.BeginRecord();
                for (int i = 0; i < duplicates.Length; ++i)
                {
                    GameObject selectedObj = (GameObject)duplicates[i];
                    if (selectedObj != null)
                    {
                        RuntimeUndo.RegisterCreatedObject(selectedObj);
                    }
                }
                RuntimeUndo.RecordSelection();
                RuntimeUndo.EndRecord();
            }
        }

        public void Delete()
        {
            GameObject[] selection = RuntimeSelection.gameObjects;
            if (selection == null || selection.Length == 0)
            {
                return;
            }

            RuntimeUndo.BeginRecord();
            for (int i = 0; i < selection.Length; ++i)
            {
                GameObject selectedObj = selection[i];
                if (selectedObj != null)
                {
                    RuntimeUndo.BeginDestroyObject(selectedObj);
                }
            }
            RuntimeUndo.RecordSelection();
            RuntimeUndo.EndRecord();

            bool isEnabled = RuntimeUndo.Enabled;
            RuntimeUndo.Enabled = false;
            RuntimeSelection.objects = null;
            RuntimeUndo.Enabled = isEnabled;

            RuntimeUndo.BeginRecord();

            for (int i = 0; i < selection.Length; ++i)
            {
                GameObject selectedObj = selection[i];
                if (selectedObj != null)
                {
                    RuntimeUndo.DestroyObject(selectedObj);
                }
            }
            RuntimeUndo.RecordSelection();
            RuntimeUndo.EndRecord();
        }

 
        private void OnSceneLoaded(object sender, ProjectManagerEventArgs args)
        {
            OnNewScene();
        }

        private void OnSceneCreated(object sender, ProjectManagerEventArgs e)
        {
            OnNewScene();
        }

        private void OnNewScene()
        {
            RuntimeEditorApplication.ActiveSceneCameraIndex = 0;
            PrepareCameras();
            InitEditorComponents();
            RuntimeEditorApplication.SceneCameras[0].gameObject.SetActive(true);
            for (int i = 1; i < RuntimeEditorApplication.SceneCameras.Length; ++i)
            {
                RuntimeEditorApplication.SceneCameras[i].gameObject.SetActive(false);
            }
        }

        private void OnBoxSelectionFiltering(object sender, FilteringArgs e)
        {
            if (e.Object == null)
            {
                e.Cancel = true;
            }

            ExposeToEditor exposeToEditor = e.Object.GetComponent<ExposeToEditor>();
            if (!exposeToEditor || !exposeToEditor.CanSelect)
            {
                e.Cancel = true;
            }
        }
        */
        #endregion
    }
}
