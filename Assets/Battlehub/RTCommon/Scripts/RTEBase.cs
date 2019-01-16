using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
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

    [System.Serializable]
    public struct CameraLayerSettings
    {
        public int ResourcePreviewLayer;
        public int RuntimeGraphicsLayer;
        public int MaxGraphicsLayers;

        public int RaycastMask
        {
            get
            {
                return ~(((1 << MaxGraphicsLayers) - 1) << RuntimeGraphicsLayer);
            }
        }
        
        public CameraLayerSettings(int resourcePreviewLayer, int runtimeGraphicsLayer, int maxLayers)
        {
            ResourcePreviewLayer = resourcePreviewLayer;
            RuntimeGraphicsLayer = runtimeGraphicsLayer;
            MaxGraphicsLayers = maxLayers;
        }
    }

    public interface IRTE
    {
        event RTEEvent PlaymodeStateChanging;
        event RTEEvent PlaymodeStateChanged;
        event RTEEvent ActiveWindowChanged;
        event RTEEvent<RuntimeWindow> WindowRegistered;
        event RTEEvent<RuntimeWindow> WindowUnregistered;
        event RTEEvent IsOpenedChanged;
        event RTEEvent IsDirtyChanged;

        ComponentEditorSettings ComponentEditorSettings
        {
            get;
        }

        CameraLayerSettings CameraLayerSettings
        {
            get;
        }

        bool IsVR
        {
            get;
        }

        IInput Input
        {
            get;
        }

        IRuntimeSelectionInternal Selection
        {
            get;
        }

        IRuntimeUndo Undo
        {
            get;
        }

        RuntimeTools Tools
        {
            get;
        }

        CursorHelper CursorHelper
        {
            get;
        }

        IRTEObjects Object
        {
            get;
        }

        IDragDrop DragDrop
        {
            get;
        }

        bool IsDirty
        {
            get;
            set;
        }

        bool IsOpened
        {
            get;
            set;
        }

        bool IsBusy
        {
            get;
            set;
        }

        bool IsPlaymodeStateChanging
        {
            get;
        }

        bool IsPlaying
        {
            get;
            set;
        }

        Transform Root
        {
            get;
        }

        bool IsInputFieldActive
        {
            get;
        }

        RuntimeWindow ActiveWindow
        {
            get;
        }

        RuntimeWindow[] Windows
        {
            get;
        }

        int GetIndex(RuntimeWindowType windowType);
        RuntimeWindow GetWindow(RuntimeWindowType windowType);
        void ActivateWindow(RuntimeWindowType window);
        void ActivateWindow(RuntimeWindow window);
        void RegisterWindow(RuntimeWindow window);
        void UnregisterWindow(RuntimeWindow window);

        void RegisterCreatedObject(GameObject go);
        void Duplicate(GameObject[] go);
        void Delete(GameObject[] go);
        void Close();
    }

    public delegate void RTEEvent();
    public delegate void RTEEvent<T>(T arg);
    
    [DefaultExecutionOrder(-90)]
    public class RTEBase : MonoBehaviour, IRTE
    {
        [SerializeField]
        protected GraphicRaycaster m_raycaster;
        [SerializeField]
        protected EventSystem m_eventSystem;

        [SerializeField]
        private ComponentEditorSettings m_componentEditorSettings = new ComponentEditorSettings(true, true, true, true);
        [SerializeField]
        private CameraLayerSettings m_cameraLayerSettings = new CameraLayerSettings(20, 21, 4);
        [SerializeField]
        private bool m_useBuiltinUndo = true;
        
        [SerializeField]
        private bool m_enableVRIfAvailable = false;

        [SerializeField]
        private bool m_isOpened = true;
        [SerializeField]
        private UnityEvent IsOpenedEvent = null;
        [SerializeField]
        private UnityEvent IsClosedEvent = null;

        public event RTEEvent PlaymodeStateChanging;
        public event RTEEvent PlaymodeStateChanged;
        public event RTEEvent ActiveWindowChanged;
        public event RTEEvent<RuntimeWindow> WindowRegistered;
        public event RTEEvent<RuntimeWindow> WindowUnregistered;
        public event RTEEvent IsOpenedChanged;
        public event RTEEvent IsDirtyChanged;
        public event RTEEvent IsBusyChanged;

        protected InputLow m_input;
        private RuntimeSelection m_selection;
        private RuntimeTools m_tools = new RuntimeTools();
        private CursorHelper m_cursorHelper = new CursorHelper();
        private IRuntimeUndo m_undo;
        private DragDrop m_dragDrop;
        private IRTEObjects m_object;

        protected GameObject m_currentSelectedGameObject;
        protected InputField m_currentInputField;
        protected float m_zAxis;

        protected readonly HashSet<GameObject> m_windows = new HashSet<GameObject>();
        public bool IsInputFieldActive
        {
            get { return m_currentInputField != null; }
        }

        private RuntimeWindow m_activeWindow;
        public virtual RuntimeWindow ActiveWindow
        {
            get { return m_activeWindow; }
        }

        public virtual RuntimeWindow[] Windows
        {
            get { return m_windows.Where(go => go != null).Select(go => go.GetComponent<RuntimeWindow>()).Where(w => w != null).ToArray(); }
        }

        public virtual ComponentEditorSettings ComponentEditorSettings
        {
            get { return m_componentEditorSettings; }
        }

        public virtual CameraLayerSettings CameraLayerSettings
        {
            get { return m_cameraLayerSettings; }
        }


        public virtual bool IsVR
        {
            get;
            private set;
        }

        public virtual IInput Input
        {
            get { return m_input; }
        }

        public virtual IRuntimeSelectionInternal Selection
        {
            get { return m_selection; }
        }

        public virtual IRuntimeUndo Undo
        {
            get { return m_undo; }
        }

        public virtual RuntimeTools Tools
        {
            get { return m_tools; }
        }

        public virtual CursorHelper CursorHelper
        {
            get { return m_cursorHelper; }
        }

        public virtual IRTEObjects Object
        {
            get { return m_object; }
        }

        public virtual IDragDrop DragDrop
        {
            get { return m_dragDrop; }
        }

        private bool m_isDirty;
        public virtual bool IsDirty
        {
            get { return m_isDirty; }
            set
            {
                if(m_isDirty != value)
                {
                    m_isDirty = value;
                    if(IsDirtyChanged != null)
                    {
                        IsDirtyChanged();
                    }
                }
            }
        }
      
        public virtual bool IsOpened
        {
            get { return m_isOpened; }
            set
            {
                if (m_isOpened != value)
                {
                    m_isOpened = value;
                    if(!m_isOpened)
                    {
                        IsPlaying = false;
                    }

                    if (!m_isOpened)
                    {
                        ActivateWindow(GetWindow(RuntimeWindowType.Game));
                    }
                    if (IsOpenedChanged != null)
                    {
                        IsOpenedChanged();
                    }
                    if (m_isOpened)
                    {
                        if(IsOpenedEvent != null)
                        {
                            IsOpenedEvent.Invoke();
                        }
                    }
                    else
                    {
                        if (IsClosedEvent != null)
                        {
                            IsClosedEvent.Invoke();
                        }
                    }
                }
            }
        }

        private bool m_isBusy;
        public virtual bool IsBusy
        {
            get { return m_isBusy; }
            set
            {
                if(m_isBusy != value)
                {
                    m_isBusy = value;
                    if (IsBusyChanged != null)
                    {
                        IsBusyChanged();
                    }           
                }
            }
        }

        private bool m_isPlayModeStateChanging;
        public virtual bool IsPlaymodeStateChanging
        {
            get { return m_isPlayModeStateChanging; }
        }

        private bool m_isPlaying;
        public virtual bool IsPlaying
        {
            get
            {
                return m_isPlaying;
            }
            set
            {
                if (m_isPlaying != value)
                {
                    m_isPlaying = value;

                    m_isPlayModeStateChanging = true;
                    if (PlaymodeStateChanging != null)
                    {
                        PlaymodeStateChanging();
                    }
                    if (PlaymodeStateChanged != null)
                    {
                        PlaymodeStateChanged();
                    }
                    m_isPlayModeStateChanging = false;
                }
            }
        }

        public virtual Transform Root
        {
            get { return transform; }
        }

        private static RTEBase m_instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Debug.Log("RTE Initialized");
            IOC.RegisterFallback<IRTE>(() =>
            {
                if (m_instance == null)
                {
                    GameObject editor = new GameObject("RTE");
                    editor.AddComponent<RTEBase>();
                    m_instance.BuildUp(editor);
                }
                return m_instance;
            });
        }

        protected virtual void BuildUp(GameObject editor)
        {
            editor.AddComponent<GLRenderer>();

            GameObject ui = new GameObject("UI");
            ui.transform.SetParent(editor.transform);

            Canvas canvas = ui.AddComponent<Canvas>();
            if (m_instance.IsVR)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
            }

            canvas.sortingOrder = short.MinValue;

            GameObject scene = new GameObject("SceneWindow");
            scene.transform.SetParent(ui.transform, false);

            RuntimeWindow sceneView = scene.AddComponent<RuntimeWindow>();
            sceneView.WindowType = RuntimeWindowType.Scene;
            sceneView.Camera = Camera.main;

            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = editor.AddComponent<EventSystem>();
                if (m_instance.IsVR)
                {
                    RTEVRInputModule inputModule = editor.AddComponent<RTEVRInputModule>();
                    inputModule.rayTransform = sceneView.Camera.transform;
                    inputModule.Editor = this;
                }
                else
                {
                    editor.AddComponent<StandaloneInputModule>();
                }
            }

            RectTransform rectTransform = sceneView.GetComponent<RectTransform>();
            if (rectTransform != null)
            {

                RectTransform parentTransform = rectTransform.parent as RectTransform;
                if (parentTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.offsetMax = new Vector2(0, 0);
                    rectTransform.offsetMin = new Vector2(0, 0);
                }
            }

            if (m_instance.IsVR)
            {
                RTEVRGraphicsRaycaster raycaster = ui.AddComponent<RTEVRGraphicsRaycaster>();
                raycaster.SceneWindow = sceneView;
                m_instance.m_raycaster = raycaster;
            }
            else
            {
                m_instance.m_raycaster = ui.AddComponent<GraphicRaycaster>();
            }
            m_instance.m_eventSystem = eventSystem;
        }

        private bool m_isPaused;
        public bool IsApplicationPaused
        {
            get { return m_isPaused; }
        }

        private void OnApplicationQuit()
        {
            m_isPaused = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if(Application.isEditor)
            {
                return;
            }
            m_isPaused = !hasFocus;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            m_isPaused = pauseStatus;
        }

        protected virtual void Awake()
        {
            if (m_instance != null)
            {
                Debug.LogWarning("Another instance of RTE exists");
                return;
            }
            if(m_useBuiltinUndo)
            {
                m_undo = new RuntimeUndo(this);
            }
            else
            {
                m_undo = new DisabledUndo();
            }

            if(m_raycaster == null)
            {
                m_raycaster = GetComponent<GraphicRaycaster>();
            }



            IsVR = UnityEngine.XR.XRDevice.isPresent && m_enableVRIfAvailable;
            m_selection = new RuntimeSelection(this);
            m_dragDrop = new DragDrop(this);
            m_object = GetComponent<RTEObjects>();

            if(IsVR)
            {
                m_input = new InputLowVR();
            }
            else
            {
                m_input = new InputLow();
            }

            m_instance = this;

            bool isOpened = m_isOpened;
            m_isOpened = !isOpened;
            IsOpened = isOpened;
        }

        protected virtual void Start()
        {
            if(GetComponent<RTEBaseInput>() == null)
            {
                gameObject.AddComponent<RTEBaseInput>();
            }
        }

        protected virtual void OnDestroy()
        {
            IsOpened = false;

            if(m_object != null)
            {
                m_object = null;
            }
        
            if(m_dragDrop != null)
            {
                m_dragDrop.Reset();
            }
            if(m_instance == this)
            {
                m_instance = null;
            }
        }

 
        public void RegisterWindow(RuntimeWindow window)
        {
            if(!m_windows.Contains(window.gameObject))
            {
                m_windows.Add(window.gameObject);
            }

            if(WindowRegistered != null)
            {
                WindowRegistered(window);
            }

            if(m_windows.Count == 1)
            {
                ActivateWindow(window);
            }
        }

        public void UnregisterWindow(RuntimeWindow window)
        {
            m_windows.Remove(window.gameObject);

            if(IsApplicationPaused)
            {
                return;
            }

            if(WindowUnregistered != null)
            {
                WindowUnregistered(window);
            }

            if(m_activeWindow == window)
            {
                RuntimeWindow activeWindow = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).Where(w => w.WindowType == window.WindowType).FirstOrDefault();
                if(activeWindow == null)
                {
                    activeWindow = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).FirstOrDefault();
                }
                    
                if(IsOpened)
                {
                    ActivateWindow(activeWindow);
                }
            }
        }


        protected virtual void Update()
        {
            UpdateCurrentInputField();

            bool mwheel = false;
            if (m_zAxis != Mathf.CeilToInt(Mathf.Abs(m_input.GetAxis(InputAxis.Z))))
            {
                mwheel = m_zAxis == 0;
                m_zAxis = Mathf.CeilToInt(Mathf.Abs(m_input.GetAxis(InputAxis.Z)));

            }

            bool pointerDownOrUp = m_input.GetPointerDown(0) ||
                m_input.GetPointerDown(1) ||
                m_input.GetPointerDown(2) ||
                
                m_input.GetPointerUp(0);

            if (pointerDownOrUp ||
                mwheel ||
                m_input.IsAnyKeyDown() && (m_currentInputField == null || !m_currentInputField.isFocused))
            {
                PointerEventData pointerEventData = new PointerEventData(m_eventSystem);
                //Set the Pointer Event Position to that of the mouse position
                pointerEventData.position = m_input.GetPointerXY(0);

                //Create a list of Raycast Results
                List<RaycastResult> results = new List<RaycastResult>();

                //Raycast using the Graphics Raycaster and mouse click position
                m_raycaster.Raycast(pointerEventData, results);

                //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
                foreach (RaycastResult result in results)
                {
                    if (m_windows.Contains(result.gameObject))
                    {
                        RuntimeWindow editorWindow = result.gameObject.GetComponent<RuntimeWindow>();
                        if(pointerDownOrUp || editorWindow.ActivateOnAnyKey)
                        {
                            ActivateWindow(editorWindow);
                            break;
                        }
                    }
                }
            }
        }

        protected void UpdateCurrentInputField()
        {
            if (m_eventSystem.currentSelectedGameObject != null && m_eventSystem.currentSelectedGameObject.activeInHierarchy)
            {
                if (m_eventSystem.currentSelectedGameObject != m_currentSelectedGameObject)
                {
                    m_currentSelectedGameObject = m_eventSystem.currentSelectedGameObject;
                    if (m_currentSelectedGameObject != null)
                    {
                        m_currentInputField = m_currentSelectedGameObject.GetComponent<InputField>();
                    }
                    else
                    {
                        if(m_currentInputField != null)
                        {
                            m_currentInputField.DeactivateInputField();
                        }
                        m_currentInputField = null;
                    }
                }
            }
            else
            {
                m_currentSelectedGameObject = null;
                if(m_currentInputField != null)
                {
                    m_currentInputField.DeactivateInputField();
                }
                m_currentInputField = null;
            }
        }

        public int GetIndex(RuntimeWindowType windowType)
        {
            IEnumerable<RuntimeWindow> windows = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).Where(w => w.WindowType == windowType).OrderBy(w => w.Index);
            int freeIndex = 0;
            foreach(RuntimeWindow window in windows)
            {
                if(window.Index != freeIndex)
                {
                    return freeIndex;
                }
                freeIndex++;
            }
            return freeIndex;
        }

        public RuntimeWindow GetWindow(RuntimeWindowType window)
        {
            return m_windows.Select(w => w.GetComponent<RuntimeWindow>()).FirstOrDefault(w => w.WindowType == window);
        }

        public virtual void ActivateWindow(RuntimeWindowType windowType)
        {
            RuntimeWindow window = GetWindow(windowType);
            if(window != null)
            {
                ActivateWindow(window);
            }
        }

        public virtual void ActivateWindow(RuntimeWindow window)
        {
            if (m_activeWindow != window)
            {
                m_activeWindow = window;
                if (ActiveWindowChanged != null)
                {
                    ActiveWindowChanged();
                }
            }
        }

        public void RegisterCreatedObject(GameObject go)
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

        public void Duplicate(GameObject[] gameObjects)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                return;
            }

            if (!Undo.Enabled)
            {
                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    GameObject go = gameObjects[i];
                    if (go != null)
                    {
                        Instantiate(go, go.transform.position, go.transform.rotation);
                    }
                }
                return;
            }

            Undo.BeginRecord();
            GameObject[] duplicates = new GameObject[gameObjects.Length];
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                if (go == null)
                {
                    continue;
                }
                GameObject duplicate = Instantiate(go, go.transform.position, go.transform.rotation);

                duplicate.SetActive(true);
                duplicate.SetActive(go.activeSelf);
                if (go.transform.parent != null)
                {
                    duplicate.transform.SetParent(go.transform.parent, true);
                }

                duplicates[i] = duplicate;
                Undo.BeginRegisterCreateObject(duplicate);
            }
            Undo.RecordSelection();
            Undo.EndRecord();

            bool isEnabled = Undo.Enabled;
            Undo.Enabled = false;
            Selection.objects = duplicates;
            Undo.Enabled = isEnabled;

            Undo.BeginRecord();
            for (int i = 0; i < duplicates.Length; ++i)
            {
                GameObject selectedObj = duplicates[i];
                if (selectedObj != null)
                {
                    Undo.RegisterCreatedObject(selectedObj);
                }
            }
            Undo.RecordSelection();
            Undo.EndRecord();
        }

        public void Delete(GameObject[] gameObjects)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                return;
            }

            if (!Undo.Enabled)
            {
                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    GameObject go = gameObjects[i];
                    if (go != null)
                    {
                        Destroy(go);
                    }
                }
                return;
            }

            Undo.BeginRecord();
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                if (go != null)
                {
                    Undo.BeginDestroyObject(go);
                }
            }
            Undo.RecordSelection();
            Undo.EndRecord();

            bool isEnabled = Undo.Enabled;
            Undo.Enabled = false;
            Selection.objects = null;
            Undo.Enabled = isEnabled;

            Undo.BeginRecord();

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                if (go != null)
                {
                    Undo.DestroyObject(go);
                }
            }
            Undo.RecordSelection();
            Undo.EndRecord();
        }


        public void Close()
        {
            IsOpened = false;
            Destroy(gameObject);
        }
    }
}
