using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.UIControls;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTTerrain
{
    public class TerrainEditor : MonoBehaviour
    {
        public event Action TerrainChanged;

        public enum EditorType
        {
            Empty = 0,
            Raise_Or_Lower_Terrain = 1,
            Paint_Texture = 2,
            Stamp_Terrain = 3,
            Set_Height = 4,
            Smooth_Height = 5,
            Selection_Handles = 6,
            Settings = 7,
        }

        [SerializeField]
        private Toggle[] m_toggles = null;
        
        [SerializeField]
        private GameObject[] m_editors = null;
        [SerializeField]
        private CanvasGroup m_canvasGroup = null;
        [SerializeField]
        private TerrainProjector m_terrainProjectorPrefab = null;
        
        private IRTE m_editor;
        private IWindowManager m_wm;
        private bool m_wasEnabled;

        public TerrainProjector Projector
        {
            get;
            private set;
        }

        private Terrain m_terrain;
        public Terrain Terrain
        {
            get { return m_terrain; }
            set
            {
                if(m_terrain != value)
                {
                    m_terrain = value;
                    if(TerrainChanged != null)
                    {
                        TerrainChanged();
                    }

                    if(m_canvasGroup != null)
                    {
                        m_canvasGroup.interactable = m_terrain != null;
                    }
                }
            }
        }

   
        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Tools.ToolChanged += OnEditorToolChanged;
            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.WindowCreated += OnWindowCreated;
            m_wm.AfterLayout += OnAfterLayout;

            Projector = Instantiate(m_terrainProjectorPrefab, m_editor.Root);
            Projector.gameObject.SetActive(false);

            if(m_canvasGroup != null)
            {
                m_canvasGroup.interactable = false;
            }

            for(int i = 0; i < m_toggles.Length; ++i)
            {
                Toggle toggle = m_toggles[i];
                if(toggle != null)
                {
                    EditorType editorType = ToEditorType(i);
                    UnityEventHelper.AddListener(toggle, tog => tog.onValueChanged, v => OnToggleValueChanged(editorType, v));
                }
            }

            EditorType toolType = (m_editor.Tools.Custom is EditorType) ? (EditorType)m_editor.Tools.Custom : EditorType.Empty;
            Toggle selectedToggle = m_toggles[(int)toolType];
            if(selectedToggle != null)
            {
                selectedToggle.isOn = true;
            }
            else
            {
                GameObject emptyEditor = m_editors[(int)EditorType.Empty];
                if (emptyEditor)
                {
                    emptyEditor.gameObject.SetActive(true);
                }
            }

            SubscribeSelectionChangingEvent(true);
        }

   
        private void OnDestroy()
        {
            if(m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
                m_wm.AfterLayout -= OnAfterLayout;
            }

            if (m_editor != null)
            {
                m_editor.Tools.ToolChanged -= OnEditorToolChanged;
            }

            if(Projector != null)
            {
                Destroy(Projector.gameObject);
            }

            for (int i = 0; i < m_toggles.Length; ++i)
            {
                Toggle toggle = m_toggles[i];
                UnityEventHelper.RemoveAllListeners(toggle, tog => tog.onValueChanged);
            }

            SubscribeSelectionChangingEvent(false);
        }

        private void OnToggleValueChanged(EditorType editorType,  bool value)
        {
            GameObject emptyEditor = m_editors[(int)EditorType.Empty];
            if (emptyEditor)
            {
                emptyEditor.gameObject.SetActive(!value);
            }

            GameObject editor = m_editors[(int)editorType];
            if(editor)
            {
                editor.SetActive(value); 
                if(value)
                {
                    m_editor.Tools.Custom = editorType;
                }
            }

            if(Projector != null)
            {
                if (!value || (editorType == EditorType.Empty || editorType == EditorType.Settings || editorType == EditorType.Selection_Handles))
                {
                    Projector.gameObject.SetActive(false);
                }
                else
                {
                    Projector.gameObject.SetActive(true);
                }
            }
        }

        private static EditorType ToEditorType(int value)
        {
            if (!Enum.IsDefined(typeof(EditorType), value))
            {
                return EditorType.Empty;
            }
            return (EditorType)value;
        }

        private void SubscribeSelectionChangingEvent(bool subscribe)
        {
            if (m_editor != null)
            {
                foreach (RuntimeWindow window in m_editor.Windows)
                {
                    SubscribeSelectionChangingEvent(subscribe, window);
                }
            }
        }

        private void SubscribeSelectionChangingEvent(bool subscribe, RuntimeWindow window)
        {
            if (window != null && window.WindowType == RuntimeWindowType.Scene)
            {
                IRuntimeSelectionComponent selectionComponent = window.IOCContainer.Resolve<IRuntimeSelectionComponent>();

                if (selectionComponent != null)
                {
                    if (subscribe)
                    {
                        selectionComponent.SelectionChanging += OnSelectionChanging;
                    }
                    else
                    {
                        selectionComponent.SelectionChanging -= OnSelectionChanging;
                    }
                }
            }
        }

        private void OnEditorToolChanged()
        {
            if (!(m_editor.Tools.Custom is EditorType))
            {
                foreach (Toggle toggle in m_toggles)
                {
                    if (toggle != null)
                    {
                        toggle.isOn = false;
                    }
                }
            }
        }

        private void OnSelectionChanging(object sender, RuntimeSelectionChangingArgs e)
        {
            if(m_editor.Tools.Custom is EditorType)
            {
                EditorType editorType = (EditorType)m_editor.Tools.Custom;
                if(editorType != EditorType.Empty)
                {
                    IRuntimeSelectionComponent component = (IRuntimeSelectionComponent)sender;
                    RaycastHit[] hits = Physics.RaycastAll(component.Window.Pointer);
                    
                    if(Terrain != null && hits.Any(hit => hit.collider.gameObject == Terrain.gameObject))
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void OnAfterLayout(IWindowManager wm)
        {
            SubscribeSelectionChangingEvent(false);
            SubscribeSelectionChangingEvent(true);
        }

        private void OnWindowCreated(Transform windowTransform)
        {
            RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
            if(window != null && window.WindowType == RuntimeWindowType.Scene)
            {
                SubscribeSelectionChangingEvent(false, window);
                SubscribeSelectionChangingEvent(true, window);
            }
        }

    }
}
