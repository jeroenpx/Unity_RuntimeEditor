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
        void UpdatePivot();

        void ApplyMaterial(Material material);
        void ApplyMaterial(Material material, Camera camera, Vector3 mousePosition);
        void SelectFaces(Material material);
        void UnselectFaces(Material material);
        void DeleteFaces();
        void SelectHoles();
        void FillHoles();
        void Subdivide();
    }

    public static class IProBuilderToolExt
    {
        public static void Extrude(this IProBuilderTool tool, float distance)
        {
            IMeshEditor meshEditor = tool.GetEditor();
            MeshEditorState oldState = meshEditor.GetState();
            meshEditor.Extrude(distance);
            MeshEditorState newState = meshEditor.GetState();
            tool.UpdatePivot();
            tool.RecordState(meshEditor, oldState, newState);
        }

        public static void RecordStateWithAction(this IProBuilderTool tool, IMeshEditor meshEditor, MeshEditorState oldState, Action<Record> newState, bool changed = true)
        {
            UndoRedoCallback redo = record =>
            {
                newState(record);
                return changed;
            };

            UndoRedoCallback undo = record =>
            {
                meshEditor.SetState(oldState);
                return true;
            };

            IOC.Resolve<IRTE>().Undo.CreateRecord(redo, undo);
        }

        public static void RecordState(this IProBuilderTool tool, IMeshEditor meshEditor, MeshEditorState oldState, MeshEditorState newState, 
            bool oldStateChanged = true, bool newStateChanged = true)
        {
            UndoRedoCallback redo = record =>
            {
                if(newState != null)
                {
                    meshEditor.SetState(newState);
                    return newStateChanged;
                }
                return false;
            };

            UndoRedoCallback undo = record =>
            {
                if(oldState != null)
                {
                    meshEditor.SetState(oldState);
                    return oldStateChanged;
                }
                return false;
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
        private IMaterialEditor m_materialEditor;
        private IWindowManager m_wm;
        private IRuntimeEditor m_rte;
        private IBoxSelection m_boxSelection;
        private Transform m_pivot;
        private IRuntimeSelectionComponent m_selectionComponent;
               
        private void Awake()
        {
            IOC.RegisterFallback<IProBuilderTool>(this);

            m_rte = IOC.Resolve<IRuntimeEditor>();

            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.WindowCreated += OnWindowCreated;

            m_materialEditor = gameObject.AddComponent<PBMaterialEditor>();

            PBVertexEditor vertexEditor = gameObject.AddComponent<PBVertexEditor>();
            PBEdgeEditor edgeEditor = gameObject.AddComponent<PBEdgeEditor>();
            PBFaceEditor faceEditor = gameObject.AddComponent<PBFaceEditor>();

            m_meshEditors = new IMeshEditor[4];
            m_meshEditors[(int)ProBuilderToolMode.Vertex] = vertexEditor;
            m_meshEditors[(int)ProBuilderToolMode.Edge] = edgeEditor;
            m_meshEditors[(int)ProBuilderToolMode.Face] = faceEditor;

            foreach (IMeshEditor editor in m_meshEditors)
            {
                if (editor == null)
                {
                    continue;
                }
                editor.CenterMode = m_rte.Tools.PivotMode == RuntimePivotMode.Center;
            }
            UpdateGlobalMode();

            m_pivot = new GameObject("Pivot").transform;
            LockAxes lockAxes = m_pivot.gameObject.AddComponent<LockAxes>();
            lockAxes.RotationFree = true;

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
                m_rte.Tools.PivotModeChanging += OnPivotModeChanging;
                m_rte.Tools.PivotModeChanged += OnPivotModeChanged;
                m_rte.Tools.PivotRotationChanging += OnPivotRotationChanging;
                m_rte.Tools.PivotRotationChanged += OnPivotRotationChanged;
                m_rte.SceneLoading += OnSceneLoading;
            }
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IProBuilderTool>(this);

            if(m_rte != null)
            {
                m_rte.ActiveWindowChanged -= OnActiveWindowChanged;
                m_rte.Tools.PivotModeChanging -= OnPivotModeChanging;
                m_rte.Tools.PivotModeChanged -= OnPivotModeChanged;
                m_rte.Tools.PivotRotationChanging -= OnPivotRotationChanging;
                m_rte.Tools.PivotRotationChanged -= OnPivotRotationChanged;
                m_rte.SceneLoading -= OnSceneLoading;
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
            if(m_materialEditor != null)
            {
                Destroy(m_materialEditor as MonoBehaviour);
            }
        }

        private void OnCurrentModeChanged(ProBuilderToolMode oldMode)
        {
            IMeshEditor disabledEditor = m_meshEditors[(int)oldMode];
            IMeshEditor enabledEditor = m_meshEditors[(int)m_mode];

            GameObject target = null;
            if(disabledEditor != null)
            {
                target = disabledEditor.Target;

                MeshSelection disabledSelection = disabledEditor.ClearSelection();
                MeshSelection selection = null;
                if(disabledSelection != null)
                {
                    selection = new MeshSelection(disabledSelection);
                }
                
                if(selection != null)
                {
                    if (oldMode == ProBuilderToolMode.Face)
                    {
                        if (m_mode == ProBuilderToolMode.Vertex)
                        {
                            selection.FacesToVertices(true);
                            enabledEditor.ApplySelection(selection);
                        }
                        else if(m_mode == ProBuilderToolMode.Edge)
                        {
                            selection.FacesToEdges(true);
                            enabledEditor.ApplySelection(selection);
                        }
                    }
                    else if(oldMode == ProBuilderToolMode.Edge)
                    {
                        if(m_mode == ProBuilderToolMode.Vertex)
                        {
                            selection.EdgesToVertices(true);
                            enabledEditor.ApplySelection(selection);
                        }
                        else if(m_mode == ProBuilderToolMode.Face)
                        {
                            selection.EdgesToFaces(true);
                            enabledEditor.ApplySelection(selection);

                            if (!selection.HasFaces)
                            {
                                m_rte.Selection.activeObject = target;
                            }
                        }
                    }
                    else if (oldMode == ProBuilderToolMode.Vertex)
                    {
                        if (m_mode == ProBuilderToolMode.Face)
                        {
                            selection.VerticesToFaces(true);
                            enabledEditor.ApplySelection(selection);
                            if (!selection.HasFaces)
                            {
                                m_rte.Selection.activeObject = target;
                            }
                        }
                        else if(m_mode == ProBuilderToolMode.Edge)
                        {
                            selection.VerticesToEdges(true);
                            enabledEditor.ApplySelection(selection);

                            if (!selection.HasEdges)
                            {
                                m_rte.Selection.activeObject = target;
                            }
                        }
                    }

                    if(enabledEditor != null && m_rte.Selection.activeObject != m_pivot)
                    {
                        GameObject[] gameObjects = m_rte.Selection.gameObjects;
                        if (gameObjects != null)
                        {
                            enabledEditor.ApplySelection(new MeshSelection(gameObjects));
                        }
                    }

                    
                }
                else
                {
                    m_rte.Selection.activeObject = target;
                }

                if(selection != null)
                {
                    UndoRedoCallback redo = record =>
                    {
                        if (enabledEditor != null)
                        {
                            enabledEditor.ApplySelection(selection);
                            m_pivot.position = enabledEditor.Position;
                            m_pivot.rotation = GetPivotRotation(enabledEditor);
                        }

                        return true;
                    };

                    UndoRedoCallback undo = record =>
                    {
                        if (disabledEditor != null)
                        {
                            disabledEditor.RollbackSelection(disabledSelection);
                            m_pivot.position = disabledEditor.Position;
                            m_pivot.rotation = GetPivotRotation(disabledEditor);
                        }

                        return true;
                    };
                    m_rte.Undo.CreateRecord(redo, undo);
                }
            }
            else
            {
                if (enabledEditor != null)
                {
                    GameObject[] gameObjects = m_rte.Selection.gameObjects;
                    if (gameObjects != null)
                    {
                        enabledEditor.ApplySelection(new MeshSelection(gameObjects));
                    }
                }
            }

            if (oldMode == ProBuilderToolMode.Object)
            {
                SetCanSelect(false);
                if(disabledEditor != null)
                {
                    if (disabledEditor.HasSelection)
                    {
                        m_rte.Selection.activeObject = m_pivot.gameObject;
                    }

                    disabledEditor.CenterMode = m_rte.Tools.PivotMode == RuntimePivotMode.Center;
                }
            }
            else if(Mode == ProBuilderToolMode.Object)
            {
                SetCanSelect(true);
                if(m_rte.Selection.activeGameObject == m_pivot.gameObject)
                {
                    if(disabledEditor != null)
                    {
                        m_rte.Selection.activeObject = target;
                    }
                    else
                    {
                        m_rte.Selection.activeObject = null;
                    }       
                }
            }

            if(enabledEditor != null)
            {
                m_pivot.position = enabledEditor.Position;
                m_pivot.rotation = GetPivotRotation(enabledEditor); 
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

        private void OnPivotModeChanging()
        {
            m_rte.Undo.BeginRecordValue(m_rte.Tools, Strong.PropertyInfo((RuntimeTools x) => x.PivotMode));
        }

        private void OnPivotModeChanged()
        {
            m_rte.Undo.EndRecordValue(m_rte.Tools, Strong.PropertyInfo((RuntimeTools x) => x.PivotMode));
            UpdateCenterMode();
        }

        private void OnPivotRotationChanging()
        {
            m_rte.Undo.BeginRecordValue(m_rte.Tools, Strong.PropertyInfo((RuntimeTools x) => x.PivotRotation));
        }

        private void OnPivotRotationChanged()
        {
            m_rte.Undo.EndRecordValue(m_rte.Tools, Strong.PropertyInfo((RuntimeTools x) => x.PivotRotation));
            UpdatePivot();
        }

        public void UpdatePivot()
        {
            UpdateCenterMode();
            UpdateGlobalMode();
        }

        private void UpdateCenterMode()
        {
            foreach(IMeshEditor editor in m_meshEditors)
            {
                if(editor == null)
                {
                    continue;
                }
                editor.CenterMode = m_rte.Tools.PivotMode == RuntimePivotMode.Center;
            }

            IMeshEditor meshEditor = m_meshEditors[(int)m_mode];
            if (meshEditor != null)
            {
                m_pivot.position = meshEditor.Position;
                m_pivot.rotation = GetPivotRotation(meshEditor);
            }
        }

        private void UpdateGlobalMode()
        {
            foreach (IMeshEditor editor in m_meshEditors)
            {
                if (editor == null)
                {
                    continue;
                }
                editor.GlobalMode = m_rte.Tools.PivotRotation == RuntimePivotRotation.Global;
            }

            IMeshEditor currentEditor = GetEditor();
            if(currentEditor != null)
            {
                m_pivot.rotation = GetPivotRotation(currentEditor);
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

            RuntimeWindow window = windowTransform.GetComponentInChildren<RuntimeWindow>(true);
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

        private void OnSceneLoading()
        {
            IMeshEditor meshEditor = GetEditor();
            if (meshEditor != null)
            {
                meshEditor.ClearSelection();
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
          
            m_rte.Undo.BeginRecord();

            if (selection != null)
            {
                RecordSelection(meshEditor, selection);
            }

            m_pivot.position = meshEditor.Position;
            m_pivot.rotation = GetPivotRotation(meshEditor);
            
            TrySelectPivot(meshEditor);

            m_rte.Undo.EndRecord();
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
            return m_meshEditors[(int)m_mode]; 
        }

        private void LateUpdate()
        {
            if (m_rte.ActiveWindow == null)
            {
                return;
            }

            if (m_rte.ActiveWindow.WindowType != RuntimeWindowType.Scene)
            {
                return;
            }

            if (!m_rte.ActiveWindow.Camera)
            {
                return;
            }

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
                    meshEditor.Rotate(m_pivot.rotation);
                }
                else if(m_selectionComponent.ScaleHandle != null && m_selectionComponent.ScaleHandle.IsDragging)
                {
                    Vector3 localScale = m_pivot.localScale;
                    if(Mathf.Approximately(localScale.x, 0))
                    {
                        localScale.x = 0.00001f;
                    }
                    if(Mathf.Approximately(localScale.y, 0))
                    {
                        localScale.y = 0.000001f;
                    }
                    if(Mathf.Approximately(localScale.z, 0))
                    {
                        localScale.z = 0.000001f;
                    }
                    m_pivot.localScale = localScale;
                    meshEditor.Scale(m_pivot.localScale, m_pivot.rotation);
                }
            }
            else
            {
                if (!m_rte.ActiveWindow.IsPointerOver)
                {
                    return;
                }

                RuntimeWindow window = m_rte.ActiveWindow;
                meshEditor.Hover(window.Camera, m_rte.Input.GetPointerXY(0));
                
                if (m_rte.Input.GetPointerDown(0))
                {
                    bool shift = m_rte.Input.GetKey(KeyCode.LeftShift);
                    MeshSelection selection = meshEditor.Select(window.Camera, m_rte.Input.GetPointerXY(0), shift);
                    m_rte.Undo.BeginRecord();

                    if(selection != null)
                    {
                        RecordSelection(meshEditor, selection);
                    }

                    m_pivot.position = meshEditor.Position;
                    m_pivot.rotation = GetPivotRotation(meshEditor);
                    TrySelectPivot(meshEditor);

                    m_rte.Undo.EndRecord();
                }
            }
        }

        private void RecordSelection(IMeshEditor meshEditor, MeshSelection selection)
        {
            UndoRedoCallback redo = record =>
            {
                meshEditor.ApplySelection(selection);
                m_pivot.position = meshEditor.Position;
                m_pivot.rotation = GetPivotRotation(meshEditor);// Quaternion.LookRotation(meshEditor.Normal);
                return true;
            };

            UndoRedoCallback undo = record =>
            {
                meshEditor.RollbackSelection(selection);
                m_pivot.position = meshEditor.Position;
                m_pivot.rotation = GetPivotRotation(meshEditor);// Quaternion.LookRotation(meshEditor.Normal);
                return true;
            };

            m_rte.Undo.CreateRecord(redo, undo);
        }

        private bool m_shift;
        private void OnBeginMove(BaseHandle positionHandle)
        {
            IMeshEditor meshEditor = GetEditor();
            if(meshEditor != null)
            {
                positionHandle.EnableUndo = false;

                m_rte.Undo.BeginRecord();
                m_rte.Undo.BeginRecordTransform(m_pivot);
                m_rte.Undo.RecordValue(meshEditor, Strong.PropertyInfo((IMeshEditor x) => x.Position));
                m_shift = m_rte.Input.GetKey(KeyCode.LeftShift);
                if (m_shift)
                {
                    MeshEditorState oldState = meshEditor.GetState();
                    meshEditor.Extrude();
                    this.RecordState(meshEditor, oldState, null, false);
                }

                m_rte.Undo.EndRecord();
            }
        }

        private void OnEndMove(BaseHandle positionHandle)
        {
            IMeshEditor meshEditor = GetEditor();
            if (meshEditor != null)
            {
                positionHandle.EnableUndo = true;

                m_rte.Undo.BeginRecord();

                MeshEditorState newState = meshEditor.GetState();
                this.RecordState(meshEditor, null, newState, false, false);

                m_rte.Undo.EndRecordTransform(m_pivot);
                m_rte.Undo.RecordValue(meshEditor, Strong.PropertyInfo((IMeshEditor x) => x.Position));
                m_rte.Undo.EndRecord();
            }
        }

        private Quaternion m_initialRotation;
        private void OnBeginRotate(BaseHandle rotationHandle)
        {
            IMeshEditor meshEditor = GetEditor();
            if(meshEditor != null)
            {
                rotationHandle.EnableUndo = false;
                m_initialRotation = GetPivotRotation(meshEditor);
                m_pivot.rotation = m_initialRotation;
                meshEditor.BeginRotate(m_initialRotation);
            }
        }

        private void OnEndRotate(BaseHandle rotationHandle)
        {
            IMeshEditor meshEditor = GetEditor();
            if(meshEditor != null)
            {
                rotationHandle.EnableUndo = true;

                Quaternion initialRotation = m_initialRotation;
                Quaternion endRotation = m_pivot.rotation;
                meshEditor.EndRotate();

                Quaternion newStartRotation = GetPivotRotation(meshEditor);
                m_pivot.rotation = newStartRotation;

                m_rte.Undo.CreateRecord(record =>
                {
                    meshEditor.BeginRotate(initialRotation);
                    meshEditor.Rotate(endRotation);
                    meshEditor.EndRotate();

                    m_pivot.transform.rotation = newStartRotation;
                    return true;
                },
                record =>
                {
                    meshEditor.BeginRotate(endRotation);
                    meshEditor.Rotate(initialRotation);
                    meshEditor.EndRotate();

                    m_pivot.transform.rotation = initialRotation;
                    return true;
                });   
            }
        }

        private void OnBeginScale(BaseHandle scaleHandle)
        {
            IMeshEditor meshEditor = GetEditor();
            if (meshEditor != null)
            {
                scaleHandle.EnableUndo = false;
                m_pivot.localScale = Vector3.one;
                meshEditor.BeginScale();
            }
        }

        private void OnEndScale(BaseHandle scaleHandle)
        {
            IMeshEditor meshEditor = GetEditor();
            if (meshEditor != null)
            {
                scaleHandle.EnableUndo = true;
                meshEditor.EndScale();

                Vector3 newScale = m_pivot.localScale;
                Quaternion rotation = m_pivot.rotation;
                m_pivot.localScale = Vector3.one;

                m_rte.Undo.CreateRecord(record =>
                {
                    meshEditor.BeginScale();
                    meshEditor.Scale(newScale, rotation);
                    meshEditor.EndScale();
                    return true;
                },
               record =>
               {
                   meshEditor.BeginScale();
                   meshEditor.Scale(new Vector3(1.0f / newScale.x, 1.0f / newScale.y, 1.0f / newScale.z), rotation);
                   meshEditor.EndRotate();
                   return true;
               });
            }
        }

        private Quaternion GetPivotRotation(IMeshEditor meshEditor)
        {
            return m_rte.Tools.PivotRotation == RuntimePivotRotation.Global ? Quaternion.identity : Quaternion.LookRotation(meshEditor.Normal);
        }

        public void ApplyMaterial(Material material, Camera camera, Vector3 mousePosition)
        {
            IMeshEditor editor = GetEditor();
            if (editor != null)
            {
                MeshSelection selection = editor.GetSelection();
                ApplyMaterialResult result = m_materialEditor.ApplyMaterial(material, selection, camera, mousePosition);
                RecordApplyMaterialResult(result);
            }
            else
            {
                GameObject gameObject = PBUtility.PickObject(camera, mousePosition);
                if(gameObject != null)
                {
                    if (m_rte.Selection.IsSelected(gameObject))
                    {
                        ApplyMaterialToSelectedGameObjects(material);
                    }
                    else
                    {
                        ApplyMaterialResult result = m_materialEditor.ApplyMaterial(material, gameObject);
                        RecordApplyMaterialResult(result);
                    }
                }
                
            }
        }

        public void ApplyMaterial(Material material)
        {
            IMeshEditor editor = GetEditor();
            if(editor != null)
            {
                MeshSelection selection = editor.GetSelection();
                m_rte.Undo.BeginRecord();

                ApplyMaterialResult result = m_materialEditor.ApplyMaterial(material, selection);
                RecordApplyMaterialResult(result);
            }
            else
            {
                ApplyMaterialToSelectedGameObjects(material);
            }
        }

        private void ApplyMaterialToSelectedGameObjects(Material material)
        {
            GameObject[] gameObjects = m_rte.Selection.gameObjects;
            if (gameObjects != null)
            {
                m_rte.Undo.BeginRecord();
                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    if (gameObjects[i] == null)
                    {
                        continue;
                    }
                    
                    ApplyMaterialResult result = m_materialEditor.ApplyMaterial(material, gameObjects[i]);
                    RecordApplyMaterialResult(result);
                }
                m_rte.Undo.EndRecord();
            }
        }

        private void RecordApplyMaterialResult(ApplyMaterialResult result)
        {
            m_rte.Undo.CreateRecord(record =>
            {
                m_materialEditor.ApplyMaterials(result.NewState);
                return true;
            },
            record =>
            {
                m_materialEditor.ApplyMaterials(result.OldState);
                return true;
            },
            record => { },
            (record, oldReference, newReference) =>
            {
                result.OldState.Erase(oldReference, newReference);
                result.NewState.Erase(oldReference, newReference);
                return false;
            });
        }

        public void SelectFaces(Material material)
        {
            IMeshEditor meshEditor = GetEditor();
            if(meshEditor != null)
            {
                MeshSelection selection = meshEditor.Select(material);
                m_rte.Undo.BeginRecord();

                if (selection != null)
                {
                    RecordSelection(meshEditor, selection);
                }

                m_pivot.position = meshEditor.Position;
                m_pivot.rotation = GetPivotRotation(meshEditor);

                TrySelectPivot(meshEditor);

                m_rte.Undo.EndRecord();
            }
        }

        public void UnselectFaces(Material material)
        {
            IMeshEditor meshEditor = GetEditor();
            if (meshEditor != null)
            {
                MeshSelection selection = meshEditor.Unselect(material);
                m_rte.Undo.BeginRecord();

                if (selection != null)
                {
                    RecordSelection(meshEditor, selection);
                }

                m_pivot.position = meshEditor.Position;
                m_pivot.rotation = GetPivotRotation(meshEditor);

                TrySelectPivot(meshEditor);

                m_rte.Undo.EndRecord();
            }
        }

        public void Subdivide()
        {
            if(m_rte.Selection.activeGameObject != null)
            {
                foreach(GameObject go in m_rte.Selection.gameObjects)
                {
                    PBMesh pbMesh = go.GetComponent<PBMesh>();
                    if(pbMesh != null)
                    {
                        pbMesh.Subdivide();
                    }
                }
            }
        }

        public void DeleteFaces()
        {
            RunStateChangeAction(meshEditor => meshEditor.Delete(), true);
        }

        public void SelectHoles()
        {
            IMeshEditor meshEditor = GetEditor();
            if (meshEditor != null)
            {
                MeshSelection selection = meshEditor.SelectHoles();
                if(selection != null)
                {
                    RecordSelection(meshEditor, selection);
                }
            }
        }

        public void FillHoles()
        {
            RunStateChangeAction(meshEditor => meshEditor.FillHoles(), false);
        }

        private void RunStateChangeAction(Action<IMeshEditor> action, bool clearSelection)
        {
            IMeshEditor meshEditor = GetEditor();
            if (meshEditor != null)
            {
                m_rte.Undo.BeginRecord();
                m_rte.Undo.BeginRecordTransform(m_pivot);
                m_rte.Undo.RecordValue(meshEditor, Strong.PropertyInfo((IMeshEditor x) => x.Position));

                MeshEditorState oldState = meshEditor.GetState();
                action(meshEditor);
                MeshEditorState newState = meshEditor.GetState();
                MeshSelection selection = null;
                if (clearSelection)
                {
                     selection = meshEditor.ClearSelection();
                }
                this.RecordState(meshEditor, oldState, newState);
                if (selection != null)
                {
                    RecordSelection(meshEditor, selection);
                }
                TrySelectPivot(meshEditor);
                m_rte.Undo.EndRecord();
            }
        }
    }
}


