using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public enum ProBuilderToolMode
    {
        Object = 0,
        Vertex = 1,
        Edge = 2,
        Face = 3,

    }

    public interface IProBuilderTool
    {
        event Action<ProBuilderToolMode> ModeChanged;
        ProBuilderToolMode Mode
        {
            get;
            set;
        }

        bool HasSelection
        {
            get;
        }

        IMeshEditor GetEditor();
    }

    public static class IProBuilderToolExt
    {
        public static void Extrude(this IProBuilderTool tool, float distance)
        {
            IMeshEditor meshEditor = tool.GetEditor();
            MeshEditorState oldState = meshEditor.GetState();
            meshEditor.Extrude(distance);
            tool.RecordState(meshEditor, oldState, record => meshEditor.Extrude());
        }

        public static void RecordState(this IProBuilderTool tool, IMeshEditor meshEditor, MeshEditorState oldState, Action<Record> newState)
        {
            UndoRedoCallback redo = record =>
            {
                newState(record);
                return true;
            };

            UndoRedoCallback undo = record =>
            {
                meshEditor.SetState(oldState);
                return true;
            };

            IOC.Resolve<IRTE>().Undo.CreateRecord(redo, undo);
        }

        public static void RecordState(this IProBuilderTool tool, IMeshEditor meshEditor, MeshEditorState oldState, MeshEditorState newState)
        {
            UndoRedoCallback redo = record =>
            {
                meshEditor.SetState(newState);
                return true;
            };

            UndoRedoCallback undo = record =>
            {
                meshEditor.SetState(oldState);
                return true;
            };

            IOC.Resolve<IRTE>().Undo.CreateRecord(redo, undo);
        }
    }

    [DefaultExecutionOrder(-90)]
    public class ProBuilderTool : MonoBehaviour, IProBuilderTool
    {
        public event Action<ProBuilderToolMode> ModeChanged;

        private ProBuilderToolMode m_mode = ProBuilderToolMode.Object;
        public ProBuilderToolMode Mode
        {
            get { return ModeInternal; }
            set
            {
                if(ModeInternal != value)
                {
                    var propertyInfo = Strong.PropertyInfo((ProBuilderTool x) => x.ModeInternal);
                    m_rte.Undo.BeginRecord();
                    m_rte.Undo.BeginRecordValue(this, propertyInfo);
                    ModeInternal = value;
                    m_rte.Undo.EndRecordValue(this, propertyInfo);
                    m_rte.Undo.EndRecord();
                }
            }
        }

        private ProBuilderToolMode ModeInternal
        {
            get { return m_mode; }
            set
            {
                ProBuilderToolMode oldMode = m_mode;
                m_mode = value;
                OnCurrentModeChanged(oldMode);
                if (ModeChanged != null)
                {
                    ModeChanged(oldMode);
                }
            }
        }

        public bool HasSelection
        {
            get
            {
                IMeshEditor editor = GetEditor();
                return editor != null && editor.HasSelection;
            }
        }


        private IMeshEditor[] m_meshEditors;
        private IWindowManager m_wm;
        private IRTE m_rte;
        private IBoxSelection m_boxSelection;
        private Transform m_pivot;
        private IRuntimeSelectionComponent m_selectionComponent;
               
        private void Awake()
        {
            IOC.RegisterFallback<IProBuilderTool>(this);

            m_rte = IOC.Resolve<IRTE>();
            
            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.WindowCreated += OnWindowCreated;

            m_meshEditors = new IMeshEditor[4];
            m_meshEditors[(int)ProBuilderToolMode.Face] = gameObject.AddComponent<PBFaceEditor>();

            m_pivot = new GameObject("Pivot").transform;
            m_pivot.SetParent(transform, false);

            ExposeToEditor exposed = m_pivot.gameObject.AddComponent<ExposeToEditor>();
            exposed.CanDelete = false;
            exposed.CanDuplicate = false;
            exposed.CanInspect = false;
        }

        private void Start()
        {
            SetCanSelect(Mode == ProBuilderToolMode.Object);
            if (m_rte.ActiveWindow != null && m_rte.ActiveWindow.WindowType == RuntimeWindowType.Scene)
            {
                m_selectionComponent = m_rte.ActiveWindow.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                m_boxSelection = m_rte.ActiveWindow.IOCContainer.Resolve<IBoxSelection>();

                SubscribeToEvents();
            }

            if (m_rte != null)
            {
                m_rte.ActiveWindowChanged += OnActiveWindowChanged;
                m_rte.Tools.PivotModeChanged += OnPivotModeChanged;
            }
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IProBuilderTool>(this);

            if(m_rte != null)
            {
                m_rte.ActiveWindowChanged -= OnActiveWindowChanged;
                m_rte.Tools.PivotModeChanged -= OnPivotModeChanged;
            }

            UnsubscribeFromEvents();

            if (m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
            }

            for(int i = 0; i < m_meshEditors.Length; ++i)
            {
                MonoBehaviour meshEditor = m_meshEditors[i] as MonoBehaviour;
                if(meshEditor != null)
                {
                    Destroy(meshEditor);
                }
            }
        }

        private void OnCurrentModeChanged(ProBuilderToolMode oldMode)
        {
            IMeshEditor meshEditor = m_meshEditors[(int)oldMode];
            if(meshEditor != null)
            {
                MeshSelection selection = meshEditor.ClearSelection();
                if(selection != null)
                {
                    RecordSelection(meshEditor, selection);
                }
            }

            if (oldMode == ProBuilderToolMode.Object)
            {
                SetCanSelect(false);
                if(meshEditor != null)
                {
                    if (meshEditor.HasSelection)
                    {
                        // m_rte.Selection.Select(m_pivot.gameObject, new[] { m_pivot.gameObject });
                        m_rte.Selection.activeObject = m_pivot.gameObject;
                    }

                    meshEditor.CenterMode = m_rte.Tools.PivotMode == RuntimePivotMode.Center;
                }
            }
            else if(Mode == ProBuilderToolMode.Object)
            {
                SetCanSelect(true);
                if(m_rte.Selection.activeGameObject == m_pivot.gameObject)
                {
                    if(meshEditor != null)
                    {
                        // m_rte.Selection.Select(meshEditor.Target, new[] { meshEditor.Target });
                        m_rte.Selection.activeObject = meshEditor.Target;
                    }
                    else
                    {
                        m_rte.Selection.activeObject = null;
                    }
                    
                }
            }
        }

        private void SetCanSelect(bool value)
        {
            Transform[] windows = m_wm.GetWindows(RuntimeWindowType.Scene.ToString());
            for (int i = 0; i < windows.Length; ++i)
            {
                RuntimeWindow window = windows[i].GetComponent<RuntimeWindow>();
                IRuntimeSelectionComponent selectionComponent = window.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                if (selectionComponent != null)
                {
                    selectionComponent.CanSelect = value;
                    selectionComponent.CanSelectAll = value;
                }
            }
        }

        private void OnPivotModeChanged()
        {
            IMeshEditor meshEditor = m_meshEditors[(int)m_mode];
            if(meshEditor != null)
            {
                meshEditor.CenterMode = m_rte.Tools.PivotMode == RuntimePivotMode.Center;
                m_pivot.position = meshEditor.Position;
                m_pivot.rotation = Quaternion.LookRotation(meshEditor.Normal);
            }
        }

        private void OnActiveWindowChanged(RuntimeWindow window)
        {
            UnsubscribeFromEvents();

            if (m_rte.ActiveWindow != null && m_rte.ActiveWindow.WindowType == RuntimeWindowType.Scene)
            {
                m_selectionComponent = m_rte.ActiveWindow.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                m_boxSelection = m_rte.ActiveWindow.IOCContainer.Resolve<IBoxSelection>();
            }
            else
            {
                m_selectionComponent = null;
                m_boxSelection = null;
            }

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            if (m_boxSelection != null)
            {
                m_boxSelection.Selection += OnBoxSelection;
            }

            if (m_selectionComponent != null)
            {
                if (m_selectionComponent.PositionHandle != null)
                {
                    m_selectionComponent.PositionHandle.BeforeDrag.AddListener(OnBeginMove);
                    m_selectionComponent.PositionHandle.Drop.AddListener(OnEndMove);
                }

                if (m_selectionComponent.RotationHandle != null)
                {
                    m_selectionComponent.RotationHandle.BeforeDrag.AddListener(OnBeginRotate);
                    m_selectionComponent.RotationHandle.Drop.AddListener(OnEndRotate);
                }

                if (m_selectionComponent.ScaleHandle != null)
                {
                    m_selectionComponent.ScaleHandle.BeforeDrag.AddListener(OnBeginScale);
                    m_selectionComponent.ScaleHandle.Drop.AddListener(OnEndScale);
                }
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (m_boxSelection != null)
            {
                m_boxSelection.Selection -= OnBoxSelection;
            }

            if (m_selectionComponent != null)
            {
                if (m_selectionComponent.PositionHandle != null)
                {
                    m_selectionComponent.PositionHandle.BeforeDrag.RemoveListener(OnBeginMove);
                    m_selectionComponent.PositionHandle.Drop.RemoveListener(OnEndMove);
                }

                if (m_selectionComponent.RotationHandle != null)
                {
                    m_selectionComponent.RotationHandle.BeforeDrag.RemoveListener(OnBeginRotate);
                    m_selectionComponent.RotationHandle.Drop.RemoveListener(OnEndRotate);
                }

                if (m_selectionComponent.ScaleHandle != null)
                {
                    m_selectionComponent.ScaleHandle.BeforeDrag.RemoveListener(OnBeginScale);
                    m_selectionComponent.ScaleHandle.Drop.RemoveListener(OnEndScale);
                }
            }
        }

        private void OnWindowCreated(Transform windowTransform)
        {
            if(m_mode == ProBuilderToolMode.Object)
            {
                return;
            }

            RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
            if(window.WindowType != RuntimeWindowType.Scene)
            {
                return;
            }

            IRuntimeSelectionComponent selectionComponent = window.IOCContainer.Resolve<IRuntimeSelectionComponent>();
            if(selectionComponent != null)
            {
                selectionComponent.CanSelect = false;
                selectionComponent.CanSelectAll = false;
            }
        }

        private void OnBoxSelection(object sender, BoxSelectionArgs e)
        {
            IMeshEditor meshEditor = m_meshEditors[(int)m_mode];
            if (meshEditor == null)
            {
                return;
            }

            RuntimeWindow window = m_rte.ActiveWindow;
            Vector2 min = m_boxSelection.SelectionBounds.min;
            Vector2 max = m_boxSelection.SelectionBounds.max;

            RectTransform rt = window.GetComponent<RectTransform>();
            Rect rect = new Rect(new Vector2(min.x, rt.rect.height - max.y), m_boxSelection.SelectionBounds.size);

            MeshSelection selection = meshEditor.Select(window.Camera, rect, e.GameObjects.Where(g => g.GetComponent<ExposeToEditor>() != null).ToArray(), MeshEditorSelectionMode.Add);
            m_pivot.position = meshEditor.Position;
            m_pivot.rotation = Quaternion.LookRotation(meshEditor.Normal);

            TrySelectPivot(meshEditor);

            RecordSelection(meshEditor, selection);
        }

        private void TrySelectPivot(IMeshEditor meshEditor)
        {
            if (meshEditor.HasSelection)
            {
                m_rte.Selection.activeObject = m_pivot.gameObject;
            }
            else
            {
                m_rte.Selection.activeObject = null;
            }
        }

        public IMeshEditor GetEditor()
        {
            if (m_rte.ActiveWindow == null)
            {
                return null;
            }

            if (m_rte.ActiveWindow.WindowType != RuntimeWindowType.Scene)
            {
                return null;
            }

            if (!m_rte.ActiveWindow.Camera)
            {
                return null;
            }
            return m_meshEditors[(int)m_mode]; 
        }

    
        private void LateUpdate()
        {
            IMeshEditor meshEditor = GetEditor();
            if(meshEditor == null)
            {
                return;
            }

            if (m_rte.Tools.ActiveTool != null)
            {
                if(m_selectionComponent.PositionHandle != null && m_selectionComponent.PositionHandle.IsDragging)
                {
                    meshEditor.Position = m_pivot.position;
                }
                else if(m_selectionComponent.RotationHandle != null && m_selectionComponent.RotationHandle.IsDragging)
                {
                    //meshEditor.Rotate(m_pivot.rotation);
                }
            }
            else
            {
                if (!m_rte.ActiveWindow.IsPointerOver)
                {
                    return;
                }

                if (m_rte.Input.GetPointerDown(0))
                {
                    bool shift = m_rte.Input.GetKey(KeyCode.LeftShift);

                    RuntimeWindow window = m_rte.ActiveWindow;
                    MeshSelection selection = meshEditor.Select(window.Camera, m_rte.Input.GetPointerXY(0), shift);

                    m_rte.Undo.BeginRecord();

                    if(selection != null)
                    {
                        RecordSelection(meshEditor, selection);
                    }

                    m_pivot.position = meshEditor.Position;
                    m_pivot.rotation = Quaternion.LookRotation(meshEditor.Normal);
                    TrySelectPivot(meshEditor);

                    m_rte.Undo.EndRecord();
                }
            }
        }

        

        private void RecordSelection(IMeshEditor meshEditor, MeshSelection selection)
        {
            UndoRedoCallback redo = record =>
            {
                meshEditor.RedoSelection(selection);
                m_pivot.position = meshEditor.Position;
                m_pivot.rotation = Quaternion.LookRotation(meshEditor.Normal);
                return true;
            };

            UndoRedoCallback undo = record =>
            {
                meshEditor.UndoSelection(selection);
                m_pivot.position = meshEditor.Position;
                m_pivot.rotation = Quaternion.LookRotation(meshEditor.Normal);
                return true;
            };

            m_rte.Undo.CreateRecord(redo, undo);
        }

        private void OnBeginMove(BaseHandle positionHandle)
        {
            positionHandle.EnableUndo = false;

            m_rte.Undo.BeginRecord();
            m_rte.Undo.BeginRecordTransform(m_pivot);

            IMeshEditor meshEditor = GetEditor();
            if(meshEditor != null)
            {
                m_rte.Undo.BeginRecordValue(meshEditor, Strong.PropertyInfo((IMeshEditor x) => x.Position));
                bool shift = m_rte.Input.GetKey(KeyCode.LeftShift);
                if (shift)
                {
                    MeshEditorState oldState = meshEditor.GetState();
                    meshEditor.Extrude();
                    this.RecordState(meshEditor, oldState, record => meshEditor.Extrude());
                }
            }
        }

        private void OnEndMove(BaseHandle positionHandle)
        {
            positionHandle.EnableUndo = true;

            IMeshEditor meshEditor = GetEditor();
            if (meshEditor != null)
            {
                m_rte.Undo.EndRecordValue(meshEditor, Strong.PropertyInfo((IMeshEditor x) => x.Position));
            }

            m_rte.Undo.EndRecordTransform(m_pivot);
            m_rte.Undo.EndRecord();
        }

        private void OnBeginRotate(BaseHandle rotationHandle)
        {
            Debug.Log("BeginRotate");
        }

        private void OnEndRotate(BaseHandle rotationHandle)
        {
            Debug.Log("EndRotate");
        }

        private void OnBeginScale(BaseHandle scaleHandle)
        {
            Debug.Log("BeginScale");
        }

        private void OnEndScale(BaseHandle scaleHandle)
        {
            Debug.Log("EndScale");
        }
    }
}


