using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
    public struct ComponentEditorSettings
    {
        public readonly bool ShowResetButton;
        public readonly bool ShowExpander;
        public readonly bool ShowEnableButton;
      
        public ComponentEditorSettings(bool showExpander, bool showResetButton, bool showEnableButton)
        {
            ShowResetButton = showResetButton;
            ShowExpander = showExpander;
            ShowEnableButton = showEnableButton;
        }
    }

    public struct CameraLayerSettings
    {
        public int RuntimeGraphicsLayer;
        public int MaxGraphicsLayers;
        
        public CameraLayerSettings(int runtimeGraphicsLayer, int maxLayers)
        {
            RuntimeGraphicsLayer = runtimeGraphicsLayer;
            MaxGraphicsLayers = maxLayers;
        }
    }

    public interface IRTE
    {
        event RuntimeEditorEvent PlaymodeStateChanging;
        event RuntimeEditorEvent PlaymodeStateChanged;
        event RuntimeEditorEvent ActiveWindowChanged;
        event RuntimeEditorEvent<RuntimeWindow> WindowRegistered;
        event RuntimeEditorEvent<RuntimeWindow> WindowUnregistered;
        event RuntimeEditorEvent IsOpenedChanged;
        event RuntimeEditorEvent IsDirtyChanged;

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

        ExposeToEditorEvents Object
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
    }

    public delegate void RuntimeEditorEvent();
    public delegate void RuntimeEditorEvent<T>(T arg);

    [DefaultExecutionOrder(-90)]
    public class RTE : MonoBehaviour, IRTE
    {
        [SerializeField]
        private ComponentEditorSettings m_componentEditorSettings = new ComponentEditorSettings(true, true, true);
        [SerializeField]
        private CameraLayerSettings m_cameraLayerSettings = new CameraLayerSettings(20, 4);
        [SerializeField]
        private bool m_useBuiltinUndo = true;

        [SerializeField]
        private GraphicRaycaster m_raycaster;
        [SerializeField]
        private EventSystem m_eventSystem;
                
        public event RuntimeEditorEvent PlaymodeStateChanging;
        public event RuntimeEditorEvent PlaymodeStateChanged;
        public event RuntimeEditorEvent ActiveWindowChanged;
        public event RuntimeEditorEvent<RuntimeWindow> WindowRegistered;
        public event RuntimeEditorEvent<RuntimeWindow> WindowUnregistered;
        public event RuntimeEditorEvent IsOpenedChanged;
        public event RuntimeEditorEvent IsDirtyChanged;

        private InputLow m_input;
        private RuntimeSelection m_selection;
        private RuntimeTools m_tools = new RuntimeTools();
        private CursorHelper m_cursorHelper = new CursorHelper();
        private IRuntimeUndo m_undo;
        private DragDrop m_dragDrop;
        private ExposeToEditorEvents m_object;

        private readonly HashSet<GameObject> m_windows = new HashSet<GameObject>();

        private RuntimeWindow m_activeWindow;
        public RuntimeWindow ActiveWindow
        {
            get { return m_activeWindow; }
        }

        public RuntimeWindow[] Windows
        {
            get { return m_windows.Where(go => go != null).Select(go => go.GetComponent<RuntimeWindow>()).Where(w => w != null).ToArray(); }
        }

        public ComponentEditorSettings ComponentEditorSettings
        {
            get { return m_componentEditorSettings; }
        }

        public CameraLayerSettings CameraLayerSettings
        {
            get { return m_cameraLayerSettings; }
        }

        [SerializeField]
        private bool m_enableVR = true;

        public bool IsVR
        {
            get;
            private set;
        }

        public IInput Input
        {
            get { return m_input; }
        }

        public IRuntimeSelectionInternal Selection
        {
            get { return m_selection; }
        }

        public IRuntimeUndo Undo
        {
            get { return m_undo; }
        }

        public RuntimeTools Tools
        {
            get { return m_tools; }
        }

        public CursorHelper CursorHelper
        {
            get { return m_cursorHelper; }
        }

        public ExposeToEditorEvents Object
        {
            get { return m_object; }
        }

        public IDragDrop DragDrop
        {
            get { return m_dragDrop; }
        }

        private bool m_isDirty;
        public bool IsDirty
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
      
        private bool m_isOpened;
        public bool IsOpened
        {
            get { return m_isOpened; }
            set
            {
                if (m_isOpened != value)
                {
                    m_isOpened = value;
                    if (!m_isOpened)
                    {
                        ActivateWindow(GetWindow(RuntimeWindowType.GameView));
                    }
                    if (IsOpenedChanged != null)
                    {
                        IsOpenedChanged();
                    }

                }
            }
        }

        private bool m_isPlayModeStateChanging;
        public bool IsPlaymodeStateChanging
        {
            get { return m_isPlayModeStateChanging; }
        }

        private bool m_isPlaying;
        public bool IsPlaying
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

        private static RTE m_instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Debug.Log("RTE Initialized");
            IOC.RegisterFallback<IRTE>(() =>
            {
                if (m_instance == null)
                {
                    GameObject editor = new GameObject("RTE");
                    editor.AddComponent<RTE>();
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
            sceneView.WindowType = RuntimeWindowType.SceneView;
            sceneView.Camera = Camera.main;

            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = editor.AddComponent<EventSystem>();
                if (m_instance.IsVR)
                {
                    RTCVRInputModule inputModule = editor.AddComponent<RTCVRInputModule>();
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
                RTCVRGraphicsRaycaster raycaster = ui.AddComponent<RTCVRGraphicsRaycaster>();
                raycaster.SceneWindow = sceneView;
                m_instance.m_raycaster = raycaster;
            }
            else
            {
                m_instance.m_raycaster = ui.AddComponent<GraphicRaycaster>();
            }
            m_instance.m_eventSystem = eventSystem;
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

            IsVR = UnityEngine.XR.XRDevice.isPresent && m_enableVR;
            m_selection = new RuntimeSelection(this);
            m_dragDrop = new DragDrop(this);
            m_object = new ExposeToEditorEvents(this);

            if(IsVR)
            {
                m_input = new InputLowVR();
            }
            else
            {
                m_input = new InputLow();
            }

            m_instance = this;
            Reset();
        }

        protected virtual void OnDestroy()
        {
            if(m_object != null)
            {
                m_object.Reset();
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

        private void Reset()
        {
            m_windows.Clear();
            m_activeWindow = null;
            m_isOpened = false;
            m_isPlaying = false;

            if(m_selection != null)
            {
                m_selection.objects = null;
            }

            if(m_tools != null)
            {
                m_tools.Reset();
            }
            
            if(m_cursorHelper != null)
            {
                m_cursorHelper.Reset();
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
                    
                ActivateWindow(activeWindow);
            }
        }

        private void Update()
        {
            if(m_input.GetPointerDown(0))
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
                    if(m_windows.Contains(result.gameObject))
                    {
                        RuntimeWindow editorWindow = result.gameObject.GetComponent<RuntimeWindow>();
                        ActivateWindow(editorWindow);
                        break;
                    }
                }
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

        public void ActivateWindow(RuntimeWindowType windowType)
        {
            RuntimeWindow window = GetWindow(windowType);
            if(window != null)
            {
                ActivateWindow(window);
            }
        }

        public void ActivateWindow(RuntimeWindow window)
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
    }

    public delegate void ObjectEvent(ExposeToEditor obj);
    public delegate void ObjectParentChangedEvent(ExposeToEditor obj, ExposeToEditor oldValue, ExposeToEditor newValue);

    #region ExposeToEditorEvents
    public class ExposeToEditorEvents
    {
        public event ObjectEvent Awaked;
        public event ObjectEvent Started;
        public event ObjectEvent Enabled;
        public event ObjectEvent Disabled;
        public event ObjectEvent Destroying;
        public event ObjectEvent Destroyed;
        public event ObjectEvent MarkAsDestroyedChanged;
        public event ObjectEvent TransformChanged;
        public event ObjectEvent NameChanged;
        public event ObjectParentChangedEvent ParentChanged;

        private IRTE m_rte;

        public ExposeToEditorEvents(RTE rte)
        {
            m_rte = rte;

            ExposeToEditor._Awaked += OnAwaked;
            ExposeToEditor._Enabled += OnEnabled;
            ExposeToEditor._Started += OnStarted;
            ExposeToEditor._Disabled += OnDisabled;
            ExposeToEditor._Destroying += OnDestroying;
            ExposeToEditor._Destroyed += OnDestroyed;
            ExposeToEditor._MarkAsDestroyedChanged += OnMarkAsDestroyedChanged;

            ExposeToEditor._TransformChanged += OnTransformChanged;
            ExposeToEditor._NameChanged += OnNameChanged;
            ExposeToEditor._ParentChanged += OnParentChanged;
        }

        public void Reset()
        {
            ExposeToEditor._Awaked -= OnAwaked;
            ExposeToEditor._Enabled -= OnEnabled;
            ExposeToEditor._Started -= OnStarted;
            ExposeToEditor._Disabled -= OnDisabled;
            ExposeToEditor._Destroying -= OnDestroying;
            ExposeToEditor._Destroyed -= OnDestroyed;
            ExposeToEditor._MarkAsDestroyedChanged -= OnMarkAsDestroyedChanged;

            ExposeToEditor._TransformChanged -= OnTransformChanged;
            ExposeToEditor._NameChanged -= OnNameChanged;
            ExposeToEditor._ParentChanged -= OnParentChanged;
        }

        private void OnAwaked(IRTE editor, ExposeToEditor obj)
        {
            if (m_rte != editor)
            {
                return;
            }

            if (Awaked != null)
            {
                Awaked(obj);
            }
        }

        private void OnEnabled(IRTE editor, ExposeToEditor obj)
        {
            if (m_rte != editor)
            {
                return;
            }

            if (Enabled != null)
            {
                Enabled(obj);
            }
        }

        private void OnStarted(IRTE editor, ExposeToEditor obj)
        {
            if (m_rte != editor)
            {
                return;
            }

            if (Started != null)
            {
                Started(obj);
            }
        }

        private void OnDisabled(IRTE editor, ExposeToEditor obj)
        {
            if (m_rte != editor)
            {
                return;
            }

            if (Disabled != null)
            {
                Disabled(obj);
            }
        }

        private void OnDestroying(IRTE editor, ExposeToEditor obj)
        {
            if (m_rte != editor)
            {
                return;
            }

            if (Destroying != null)
            {
                Destroying(obj);
            }
        }

        private void OnDestroyed(IRTE editor, ExposeToEditor obj)
        {
            if (m_rte != editor)
            {
                return;
            }

            if (Destroyed != null)
            {
                Destroyed(obj);
            }
        }

        private void OnMarkAsDestroyedChanged(IRTE editor, ExposeToEditor obj)
        {
            if (m_rte != editor)
            {
                return;
            }

            if (MarkAsDestroyedChanged != null)
            {
                MarkAsDestroyedChanged(obj);
            }
        }

        private void OnTransformChanged(IRTE editor, ExposeToEditor obj)
        {
            if (m_rte != editor)
            {
                return;
            }

            if (TransformChanged != null)
            {
                TransformChanged(obj);
            }
        }

        private void OnNameChanged(IRTE editor, ExposeToEditor obj)
        {
            if (m_rte != editor)
            {
                return;
            }

            if (NameChanged != null)
            {
                NameChanged(obj);
            }
        }

        private void OnParentChanged(IRTE editor, ExposeToEditor obj, ExposeToEditor oldValue, ExposeToEditor newValue)
        {
            if (m_rte != editor)
            {
                return;
            }

            if (ParentChanged != null)
            {
                ParentChanged(obj, oldValue, newValue);
            }
        }
    }
    #endregion
}
