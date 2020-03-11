using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.Spline3;
using System;
using UnityEngine;

namespace Battlehub.MeshDeformer3
{
    public enum MeshDeformerToolMode
    {
        Object = 0,
        ControlPoint = 1,
    }

    public interface IMeshDeformerTool
    {
        event Action SelectionChanged;
        event Action ModeChanged;

        MeshDeformerToolMode Mode
        {
            get;
            set;
        }

        void SelectControlPoint(Camera camera, Vector2 point);
        bool DragControlPoint(bool extend);
        bool CanDeform();
        void DeformAxis(Axis axis);
        bool CanAppend();
        void Append();
        bool CanRemove();
        void Remove();
    }

    [DefaultExecutionOrder(-90)]
    public class MeshDeformerTool : MonoBehaviour, IMeshDeformerTool
    {
        public event Action SelectionChanged;
        public event Action ModeChanged;

        private ISelectionComponentState m_selectionComponentState;
        private ControlPointPicker m_controlPointPicker;
        private IRTE m_editor;
        
        private MeshDeformerToolMode m_mode = MeshDeformerToolMode.Object;
        public MeshDeformerToolMode Mode
        {
            get { return m_mode; }
            set
            {
                if(m_mode != value)
                {
                    m_mode = value;
                    UnsubscribeEvents();
                    if (m_mode != MeshDeformerToolMode.Object)
                    {
                        //RuntimeTool current = m_editor.Tools.Current;
                        //m_editor.Tools.ToolChanged -= OnEditorToolChanged;
                        //m_editor.Tools.Current = RuntimeTool.None;
                        //m_editor.Tools.Current = current;
                        //m_editor.Tools.ToolChanged += OnEditorToolChanged;

                        SetupSelectionComponentAndSubscribeEvents();
                        m_editor.ActiveWindowChanged += OnActiveWindowChanged;
                        EnableSplineRenderers(true);
                    }
                    else
                    {
                        SetCanSelect(true);
                        m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
                        EnableSplineRenderers(false);
                    }
                    if (ModeChanged != null)
                    {
                        ModeChanged();
                    }
                }
            }
        }

        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Tools.ToolChanged += OnEditorToolChanged;

            IOC.RegisterFallback<IMeshDeformerTool>(this);

            GameObject controlPointPicker = new GameObject("ControlPointPicker");

            controlPointPicker.transform.SetParent(transform, false);
            controlPointPicker.gameObject.SetActive(false);
            controlPointPicker.hideFlags = HideFlags.HideInHierarchy;
            ExposeToEditor exposeToEditor = controlPointPicker.AddComponent<ExposeToEditor>();
            exposeToEditor.CanInspect = false;
            controlPointPicker.gameObject.SetActive(true);

            m_controlPointPicker = controlPointPicker.AddComponent<ControlPointPicker>();
            m_controlPointPicker.SelectionChanged += OnPickerSelectionChanged;
        }
    
        private void OnActiveWindowChanged(RuntimeWindow arg)
        {
            SetupSelectionComponentAndSubscribeEvents();
        }

        private void OnDestroy()
        {
            if(m_editor != null)
            {
                m_editor.Tools.ToolChanged -= OnEditorToolChanged;
                m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
            }

            if(m_controlPointPicker != null)
            {
                m_controlPointPicker.SelectionChanged -= OnPickerSelectionChanged;
            }

            UnsubscribeEvents();

            if (m_selectionComponentState != null)
            {
                m_selectionComponentState.CanSelect(this, true);
                m_selectionComponentState.IsBoxSelectionEnabled(this, true);
                m_selectionComponentState.CanSelectAll(this, true);
            }

            m_selectionComponentState = null;

            EnableSplineRenderers(false);

            IOC.UnregisterFallback<IMeshDeformerTool>(this);
        }

        private void OnPickerSelectionChanged()
        {
            if(SelectionChanged != null)
            {
                SelectionChanged();
            }
        }

        private void OnEditorToolChanged()
        {
            if(m_editor.Tools.Current == RuntimeTool.None || m_editor.Tools.Current == RuntimeTool.Custom)
            {
                Mode = MeshDeformerToolMode.Object;
            }
        }

        private void SetupSelectionComponentAndSubscribeEvents()
        {
            UnsubscribeEvents();

            if (m_editor.ActiveWindow != null)
            {
                SetCanSelect(true);
                m_selectionComponentState = m_editor.ActiveWindow.IOCContainer.Resolve<ISelectionComponentState>();
                SetCanSelect(false);
                SubscribeEvents();
            }
            else
            {
                SetCanSelect(true);
            }
        }

        private void SetCanSelect(bool value)
        {
            if (m_selectionComponentState != null)
            {
                m_selectionComponentState.CanSelect(this, value);
                m_selectionComponentState.IsBoxSelectionEnabled(this, value);
                m_selectionComponentState.CanSelectAll(this, value);
            }
        }

        private void UnsubscribeEvents()
        {
            if (m_selectionComponentState == null || m_selectionComponentState.Component.PositionHandle == null)
            {
                return;
            }

            m_selectionComponentState.Component.PositionHandle.BeforeDrag.RemoveListener(OnPositionHandleBeforeDrag);
            m_selectionComponentState.Component.PositionHandle.Drop.RemoveListener(OnPositionHandleDrop);
        }

        private void SubscribeEvents()
        {
            if (m_selectionComponentState == null || m_selectionComponentState.Component.PositionHandle == null)
            {
                return;
            }

            m_selectionComponentState.Component.PositionHandle.BeforeDrag.AddListener(OnPositionHandleBeforeDrag);
            m_selectionComponentState.Component.PositionHandle.Drop.AddListener(OnPositionHandleDrop);
        }

        public void EnableSplineRenderers(bool enable)
        {
            SplineRenderer[] splineRenderers = FindObjectsOfType<SplineRenderer>();
            for (int i = 0; i < splineRenderers.Length; ++i)
            {
                SplineRenderer splineRenderer = splineRenderers[i];
                if(splineRenderer.GetComponent<Deformer>() != null)
                {
                    splineRenderer.Layer = m_editor.CameraLayerSettings.AllScenesLayer;
                    splineRenderer.enabled = enable;
                }
            }
        }

        public void SelectControlPoint(Camera camera, Vector2 point)
        {
            if(m_selectionComponentState == null)
            {
                return;
            }

            if (m_selectionComponentState.Component.IsPositionHandleEnabled)
            {
                BaseSpline spline = m_controlPointPicker.Selection != null ? m_controlPointPicker.Selection.GetSpline() : null;
                if (spline != null && !(spline is Deformer))
                {
                    return;
                }

                m_editor.Undo.BeginRecord();
                PickResult oldSelection = m_controlPointPicker.Selection != null ? new PickResult(m_controlPointPicker.Selection) : null;
                PickResult newSelection = m_controlPointPicker.Pick(camera, point);
                if (newSelection != null && newSelection.Spline != null && newSelection.Spline.GetComponent<Deformer>() == null)
                {
                    newSelection = null;
                }

                GameObject deformerGo = null;
                if(newSelection == null)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(camera.ScreenPointToRay(point), out hit))
                    {
                        Segment segment = hit.collider.GetComponent<Segment>();
                        if (segment != null)
                        {
                            Deformer deformer = segment.GetComponentInParent<Deformer>();
                            if(deformer != null)
                            {
                                deformerGo = deformer.gameObject;
                            }
                        }
                    }
                }

                if(deformerGo != null)
                {
                    m_editor.Selection.Select(deformerGo, new[] { deformerGo });
                }

                m_controlPointPicker.ApplySelection(newSelection, true);
                newSelection = newSelection != null ? new PickResult(newSelection) : null;
                m_editor.Undo.CreateRecord(record =>
                {
                    m_controlPointPicker.Selection = newSelection;
                    if (deformerGo != null)
                    {
                        m_editor.Selection.Select(deformerGo, new[] { deformerGo });
                    }
                    return true;
                },
                record =>
                {
                    m_controlPointPicker.Selection = oldSelection;
                    if (deformerGo != null)
                    {
                        m_editor.Selection.Select(deformerGo, new[] { deformerGo });
                    }
                    return true;
                });
                m_editor.Undo.EndRecord();
            }
        }

    

        public bool DragControlPoint(bool extend)
        {
            PositionHandle positionHandle = m_editor.Tools.ActiveTool as PositionHandle;
            if (m_controlPointPicker.IsPointSelected && positionHandle != null && positionHandle.IsDragging)
            {
                if (extend)
                {
                    ControlPointPicker picker = m_editor.Selection.activeGameObject.GetComponent<ControlPointPicker>();
                    BaseSpline spline = picker.Selection.GetSpline();
                    if(!(spline is Deformer))
                    {
                        return false;
                    }

                    BaseSplineState oldState = spline.GetState();
                    PickResult oldSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;
                    m_controlPointPicker.Drag(true);
                    spline = picker.Selection.GetSpline();
                    BaseSplineState newState = spline.GetState();
                    PickResult newSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;
                    RecordState(spline.gameObject, oldState, newState, picker, oldSelection, newSelection);
                }
                else
                {
                    m_controlPointPicker.Drag(false);
                }
                return true;
            }
            return false;
        }

        public bool CanDeform()
        {
            return
                m_editor.Selection.activeGameObject != null &&
                m_editor.Selection.activeGameObject.GetComponent<MeshFilter>() &&
                m_editor.Selection.activeGameObject.GetComponent<MeshCollider>();
        }

        public void DeformAxis(Axis axis)
        {
            Deformer deformer = m_editor.Selection.activeGameObject.GetComponent<Deformer>();
            if (deformer == null)
            {
                m_editor.Undo.BeginRecord();
                m_editor.Undo.AddComponent(m_editor.Selection.activeGameObject.GetComponent<ExposeToEditor>(), typeof(Deformer));
                m_editor.Undo.CreateRecord(redo =>
                {
                    EnableSplineRenderers(Mode == MeshDeformerToolMode.ControlPoint);
                    return false;
                }, undo => false);
                m_editor.Undo.EndRecord();
            }
            else
            {
                BaseSplineState oldState = deformer.GetState();
                PickResult oldSelection = m_controlPointPicker.Selection != null ? new PickResult(m_controlPointPicker.Selection) : null;
                deformer.Axis = axis;
                BaseSplineState newState = deformer.GetState();
                PickResult newSelection = m_controlPointPicker.Selection != null ? new PickResult(m_controlPointPicker.Selection) : null;
                RecordState(deformer.gameObject, oldState, newState, m_controlPointPicker, oldSelection, newSelection);
            }

            EnableSplineRenderers(false);
        }

        public bool CanAppend()
        {
            ControlPointPicker picker = m_editor.Selection.activeGameObject.GetComponent<ControlPointPicker>();
            BaseSpline spline = picker.Selection.GetSpline();
            return picker.Selection.Index == spline.SegmentsCount + 1 ||
                   picker.Selection.Index == spline.SegmentsCount + 2 ||
                   picker.Selection.Index == 0 || picker.Selection.Index == 1;
        }

        public void Append()
        {
            ControlPointPicker picker = m_editor.Selection.activeGameObject.GetComponent<ControlPointPicker>();
            BaseSpline spline = picker.Selection.GetSpline();
            BaseSplineState oldState = spline.GetState();
            PickResult oldSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;
            if (picker.Selection.Index == 0 || picker.Selection.Index == 1)
            {
                picker.Prepend();
            }
            else
            {
                picker.Append();
            }

            spline = picker.Selection.GetSpline();
            BaseSplineState newState = spline.GetState();
            PickResult newSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;
            RecordState(spline.gameObject, oldState, newState, picker, oldSelection, newSelection);
        }

        public bool CanRemove()
        {
            return m_controlPointPicker != null && m_controlPointPicker.Selection != null && m_controlPointPicker.Selection.GetSpline().SegmentsCount > 1;
        }

        public void Remove()
        {
            ControlPointPicker picker = m_editor.Selection.activeGameObject.GetComponent<ControlPointPicker>();
            BaseSpline spline = picker.Selection.GetSpline();
            BaseSplineState oldState = spline.GetState();
            PickResult oldSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;
            picker.Remove();
            PickResult newSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;

            spline = picker.Selection.GetSpline();
            BaseSplineState newState = spline.GetState();
            RecordState(spline.gameObject, oldState, newState, picker, oldSelection, newSelection);
        }

        private void RecordState(GameObject spline,
            BaseSplineState oldState,
            BaseSplineState newState,
            ControlPointPicker picker,
            PickResult oldSelection,
            PickResult newSelection)
        {
            m_editor.Undo.CreateRecord(record =>
            {
                Deformer deformer = spline.GetComponent<Deformer>();
                deformer.SetState(newState);
                picker.Selection = newSelection;
                return true;
            },
            record =>
            {
                Deformer deformer = spline.GetComponent<Deformer>();
                deformer.SetState(oldState);
                picker.Selection = oldSelection;
                return true;
            });
        }

        private void OnPositionHandleBeforeDrag(BaseHandle handle)
        {
           // handle.EnableUndo = false;
        }

        private void OnPositionHandleDrop(BaseHandle handle)
        {
            //handle.EnableUndo = true;
        }
    }
}
