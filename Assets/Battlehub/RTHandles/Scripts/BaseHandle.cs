using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

using Battlehub.RTCommon;
using Battlehub.Utils;

namespace Battlehub.RTHandles
{
    /// <summary>
    /// Base class for all handles (Position, Rotation and Scale)
    /// </summary>
    public abstract class BaseHandle : RTEBehaviour, IGL
    {
        private const float SelectionMarginPixels = 10;

        /// <summary>
        /// current size of grid 
        /// </summary>
        protected float EffectiveGridUnitSize
        {
            get;
            private set;
        }

        /// <summary>
        /// HighlightOnHover
        /// </summary>
        public bool HightlightOnHover = true;

        public RuntimeHandlesComponent Appearance;
        /// <summary>
        /// Configurable model
        /// </summary>
        public BaseHandleModel Model;
        
        protected LockObject LockObject
        {
            get { return Editor.Tools.LockAxes; }
            set { Editor.Tools.LockAxes = value; }
        }

        protected virtual Vector3 HandlePosition
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        /// <summary>
        /// Target objects which will be affected by handle (for example if m_targets array containes O1 and O2 objects, and O1 is parent of O2 then m_activeTargets array will contain only O1 object)
        /// </summary>
        private Transform[] m_activeTargets;
        protected Transform[] ActiveTargets
        {
            get { return m_activeTargets; }
        }


        private Transform[] m_activeRealTargets;
        private Transform[] m_realTargets;
        protected Transform[] RealTargets
        {
            get
            {
                if(m_realTargets == null)
                {
                    return Targets;
                }
                return m_realTargets;
            }
        }

        private Transform[] m_commonCenter;
        private Transform[] m_commonCenterTarget;

        private void GetActiveRealTargets()
        {
            if(m_realTargets == null)
            {
                m_activeRealTargets = null;
                return;
            }

            m_realTargets = m_realTargets.Where(t => t != null && t.hideFlags == HideFlags.None).ToArray();
            HashSet<Transform> targetsHS = new HashSet<Transform>();
            for (int i = 0; i < m_realTargets.Length; ++i)
            {
                if (m_realTargets[i] != null && !targetsHS.Contains(m_realTargets[i]))
                {
                    targetsHS.Add(m_realTargets[i]);
                }
            }
            m_realTargets = targetsHS.ToArray();
            if (m_realTargets.Length == 0)
            {
                m_activeRealTargets = new Transform[0];
                return;
            }
            else if (m_realTargets.Length == 1)
            {
                m_activeRealTargets = new[] { m_realTargets[0] };
            }

            for (int i = 0; i < m_realTargets.Length; ++i)
            {
                Transform target = m_realTargets[i];
                Transform p = target.parent;
                while (p != null)
                {
                    if (targetsHS.Contains(p))
                    {
                        targetsHS.Remove(target);
                        break;
                    }

                    p = p.parent;
                }
            }

            m_activeRealTargets = targetsHS.ToArray();
        }
        /// <summary>
        /// All Target objects
        /// </summary>
        [SerializeField]
        private Transform[] m_targets;
        public Transform[] Targets
        {
            get
            {
                return Targets_Internal;
            }
            set
            {
                DestroyCommonCenter();
                m_realTargets = value;
                GetActiveRealTargets();
                Targets_Internal = value;
                if (Targets_Internal == null || Targets_Internal.Length == 0)
                {
                    return;
                }

                if (Editor.Tools.PivotMode == RuntimePivotMode.Center && ActiveTargets.Length > 1)
                {
                    Vector3 centerPosition = Targets_Internal[0].position;
                    for (int i = 1; i < Targets_Internal.Length; ++i)
                    {
                        centerPosition += Targets_Internal[i].position;
                    }

                    centerPosition = centerPosition / Targets_Internal.Length;
                    m_commonCenter = new Transform[1];
                    m_commonCenter[0] = new GameObject { name = "CommonCenter" }.transform;
                    m_commonCenter[0].SetParent(transform.parent, true);
                    m_commonCenter[0].position = centerPosition;
                    m_commonCenterTarget = new Transform[m_realTargets.Length];
                    for (int i = 0; i < m_commonCenterTarget.Length; ++i)
                    {
                        GameObject target = new GameObject { name = "ActiveTarget " + m_realTargets[i].name };
                        target.transform.SetParent(m_commonCenter[0]);

                        target.transform.position = m_realTargets[i].position;
                        target.transform.rotation = m_realTargets[i].rotation;
                        target.transform.localScale = m_realTargets[i].localScale;

                        m_commonCenterTarget[i] = target.transform;
                    }
                    LockObject lockObject = LockObject;
                    Targets_Internal = m_commonCenter;
                    LockObject = lockObject;
                }
            }
        }

        private Transform[] Targets_Internal
        {
            get { return m_targets; }
            set
            {
             
                m_targets = value;
                if(m_targets == null)
                {
                    LockObject = LockAxes.Eval(null);
                    m_activeTargets = null;
                    return;
                }

                m_targets = m_targets.Where(t => t != null && t.hideFlags == HideFlags.None).ToArray();
                HashSet<Transform> targetsHS = new HashSet<Transform>();
                for (int i = 0; i < m_targets.Length; ++i)
                {
                    if (m_targets[i] != null && !targetsHS.Contains(m_targets[i]))
                    {
                        targetsHS.Add(m_targets[i]);
                    }
                }
                m_targets = targetsHS.ToArray();
                if (m_targets.Length == 0)
                {
                    LockObject = LockAxes.Eval(new LockAxes[0]);
                    m_activeTargets = new Transform[0];
                    return;
                }
                else if(m_targets.Length == 1)
                {
                    m_activeTargets = new [] { m_targets[0] };
                }

                for(int i = 0; i < m_targets.Length; ++i)
                {
                    Transform target = m_targets[i];
                    Transform p = target.parent;
                    while(p != null)
                    {
                        if(targetsHS.Contains(p))
                        {
                            targetsHS.Remove(target);
                            break;
                        }

                        p = p.parent;
                    }
                }

                m_activeTargets = targetsHS.ToArray();
                LockObject = LockAxes.Eval(m_activeTargets.Where(t => t.GetComponent<LockAxes>() != null).Select(t => t.GetComponent<LockAxes>()).ToArray());
                if(m_activeTargets.Any(target => target.gameObject.isStatic))
                {
                    LockObject = new LockObject();
                    LockObject.PositionX = LockObject.PositionY = LockObject.PositionZ = true;
                    LockObject.RotationX = LockObject.RotationY = LockObject.RotationZ = true;
                    LockObject.ScaleX = LockObject.ScaleY = LockObject.ScaleZ = true;
                    LockObject.RotationScreen = true;
                }

                if(m_activeTargets != null && m_activeTargets.Length > 0)
                {
                    transform.position = m_activeTargets[0].position;
                }
            }
        }

        public Transform Target
        {
            get
            {
                if(Targets == null || Targets.Length == 0)
                {
                    return null;
                }
                return Targets[0];
            }
        }

        /// <summary>
        /// Selected axis
        /// </summary>
        private RuntimeHandleAxis m_selectedAxis;

        /// <summary>
        /// Whether drag operation in progress
        /// </summary>
        private bool m_isDragging;
        
        /// <summary>
        /// Drag plane
        /// </summary>
        private Plane m_dragPlane;

        public bool IsDragging
        {
            get { return m_isDragging; }
        }

        /// <summary>
        /// Tool type
        /// </summary>
        public abstract RuntimeTool Tool
        {
            get;
        }

        /// <summary>
        /// Quaternion Rotation based on selected coordinate system (local or global)
        /// </summary>
        protected Quaternion Rotation
        {
            get
            {
                if(Targets == null || Targets.Length <= 0 || Target == null)
                {
                    return Quaternion.identity;
                }

                return Editor.Tools.PivotRotation == RuntimePivotRotation.Local ? Target.rotation : Quaternion.identity;
            }
        }

        protected virtual RuntimeHandleAxis SelectedAxis
        {
            get { return m_selectedAxis; }
            set
            {
                m_selectedAxis = value;
                if (Model != null)
                {
                    Model.Select(SelectedAxis);
                }
            }
        }

        protected Plane DragPlane
        {
            get { return m_dragPlane; }
            set { m_dragPlane = value; }
        }

        protected abstract float CurrentGridUnitSize
        {
            get;
        }

        private bool m_unitSnapping;
        public bool UnitSnapping
        {
            get { return m_unitSnapping; }
            set { m_unitSnapping = value; }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();

            RuntimeHandlesComponent.InitializeIfRequired(ref Appearance);
            if (m_targets != null && m_targets.Length > 0)
            {
                Targets = m_targets;
            }

            if (Targets == null || Targets.Length == 0)
            {
                Targets = new[] { transform };
            }

            if (GLRenderer.Instance == null)
            {
                GameObject glRenderer = new GameObject();
                glRenderer.name = "GLRenderer";
                glRenderer.AddComponent<GLRenderer>();
            }

            if (GLRenderer.Instance != null)
            {
                GLRenderer.Instance.Add(this);
            }

            if (Targets[0].position != transform.position)
            {
                transform.position = Targets[0].position;
            }

            if (Model != null)
            {
                bool activeSelf = Model.gameObject.activeSelf;
                Model.gameObject.SetActive(false);
                BaseHandleModel model = Instantiate(Model, transform.parent);

                model.name = Model.name;
                model.Appearance = Appearance;
                
                Model.gameObject.SetActive(activeSelf);
                model.gameObject.SetActive(true);

                Model = model;
                Model.SetLock(LockObject);
            }
        }

        private void Start()
        {
            if(GetComponent<BaseHandleInput>() == null)
            {
                gameObject.AddComponent<BaseHandleInput>();
            }
            OnStartOverride();
        }

        protected virtual void OnStartOverride()
        {

        }

        private void OnEnable()
        {
            Editor.Tools.PivotModeChanged += OnPivotModeChanged;
            Editor.Tools.ToolChanged += OnRuntimeToolChanged;
            Editor.Tools.LockAxesChanged += OnLockAxesChanged;
            Editor.Undo.UndoCompleted += OnUndoCompleted;
            Editor.Undo.RedoCompleted += OnRedoCompleted;

            OnEnableOverride();

            if (Model != null)
            {
                if (ActiveWindow != null)
                {
                    SyncModelTransform();
                }
                
                Model.gameObject.SetActive(true);
            }
            else
            {
                if (GLRenderer.Instance != null)
                {
                    GLRenderer.Instance.Add(this);
                }
            }
        }

        protected virtual void OnEnableOverride()
        {

        }

        private void OnDisable()
        {
            if (GLRenderer.Instance != null)
            {
                GLRenderer.Instance.Remove(this);
            }


            if (Editor != null)
            {
                Editor.Tools.PivotModeChanged -= OnPivotModeChanged;
                Editor.Tools.ToolChanged -= OnRuntimeToolChanged;
                Editor.Tools.LockAxesChanged -= OnLockAxesChanged;
                Editor.Undo.UndoCompleted -= OnUndoCompleted;
                Editor.Undo.RedoCompleted -= OnRedoCompleted;
            }

            DestroyCommonCenter();

            if (Model != null)
            {
                Model.gameObject.SetActive(false);
            }

            OnDisableOverride();
        }

        protected virtual void OnDisableOverride()
        {

        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if (GLRenderer.Instance != null)
            {
                GLRenderer.Instance.Remove(this);
            }

            DestroyCommonCenter();

            if (Model != null && Model.gameObject != null)
            {
                if (!Model.gameObject.IsPrefab())
                {
                    Destroy(Model);
                }
            }
        }

        protected override void OnWindowDeactivated()
        {
            base.OnWindowDeactivated();
            if (Editor != null)
            {
                if (Editor.Tools.ActiveTool == this)
                {
                    Editor.Tools.ActiveTool = null;
                }
            }
        }

        private void DestroyCommonCenter()
        {
            if (m_commonCenter != null)
            {
                for (int i = 0; i < m_commonCenter.Length; ++i)
                {
                    Destroy(m_commonCenter[i].gameObject);
                }
            }

            if (m_commonCenterTarget != null)
            {
                for (int i = 0; i < m_commonCenterTarget.Length; ++i)
                {
                    Destroy(m_commonCenterTarget[i].gameObject);
                }
            }

            m_commonCenter = null;
            m_commonCenterTarget = null;
        }

        private void Update()
        {
            if(Model != null)
            {
                Model.ModelScale = Appearance.HandleScale;
                Model.SelectionMargin = Appearance.SelectionMargin;
            }

            if (m_isDragging)
            {
                if (Editor.Tools.IsViewing)
                {
                    m_isDragging = false;
                }
                else
                {
                    if (m_unitSnapping)
                    {
                        EffectiveGridUnitSize = CurrentGridUnitSize;
                    }
                    else
                    {
                        EffectiveGridUnitSize = 0;
                    }

                    OnDrag();
                }
            }
            else
            {
                if (!Editor.ActiveWindow.IsPointerOver)
                {
                    SelectedAxis = RuntimeHandleAxis.None;
                }
            }
            
            UpdateOverride();

            if (m_isDragging)
            {
                if (Editor.Tools.PivotMode == RuntimePivotMode.Center && m_commonCenterTarget != null && m_realTargets != null && m_realTargets.Length > 1)
                {
                    for (int i = 0; i < m_commonCenterTarget.Length; ++i)
                    {
                        Transform commonCenterTarget = m_commonCenterTarget[i];
                        Transform target = m_realTargets[i];

                        target.transform.position = commonCenterTarget.position;
                        target.transform.rotation = commonCenterTarget.rotation;
                        target.transform.localScale = commonCenterTarget.localScale;
                    }
                }
            }
        }

        protected virtual void UpdateOverride()
        {
            if (Targets != null && Targets.Length > 0 && Targets[0] != null && Targets[0].position != transform.position)
            {
                if (IsDragging)
                {
                    Vector3 offset = transform.position - Targets[0].position;
                    for (int i = 0; i < ActiveTargets.Length; ++i)
                    {
                        if (ActiveTargets[i] != null)
                        {
                            ActiveTargets[i].position += offset;
                        }
                    }

                }
                else
                {
                    transform.position = Targets[0].position;
                    transform.rotation = Targets[0].rotation;
                }
            }
        }

        private void LateUpdate()
        {
            if (Model != null && ActiveWindow != null)
            {
                SyncModelTransform();
            }
        }

        protected virtual void SyncModelTransform()
        {
            Vector3 position = HandlePosition;
            Model.transform.position = position;
            Model.transform.rotation = Rotation;
            float screenScale = RuntimeHandlesComponent.GetScreenScale(transform.position, ActiveWindow.Camera);
            Model.transform.localScale = Appearance.InvertZAxis ? new Vector3(1, 1, -1) * screenScale : Vector3.one * screenScale;
        }

        public void BeginDrag()
        {
            m_isDragging = OnBeginDrag();
            if (m_isDragging)
            {
                Editor.Tools.ActiveTool = this;
                RecordTransform();
            }
            else
            {
                Editor.Tools.ActiveTool = null;
            }
        }

        public void EndDrag()
        {
            if (m_isDragging)
            {
                OnDrop();
                RecordTransform();
                m_isDragging = false;
                Editor.Tools.ActiveTool = null;
            }
        }

        /// Drag And Drop virtual methods
        protected virtual bool OnBeginDrag()
        {
            if(ActiveWindow == null)
            {
                return false;
            }

            return true;
        }

        protected virtual void OnDrag()
        {

        }

        protected virtual void OnDrop()
        {

        }

        protected virtual void OnRuntimeToolChanged()
        {
            EndDrag();
        }

        protected virtual void OnPivotModeChanged()
        {
            if (RealTargets != null)
            {
                Targets = RealTargets;
            }

            if (Editor.Tools.PivotMode != RuntimePivotMode.Center)
            {
                m_realTargets = null;   
            }
            
            if(Target != null)
            {
                transform.position = Target.position;
            }            
        }

        private void OnLockAxesChanged()
        {
            if(Model != null)
            {
                if (!Model.gameObject.IsPrefab())
                {
                    Model.SetLock(LockObject);
                }
            }
        }

        protected virtual void RecordTransform()
        {
            Editor.Undo.BeginRecord();
            for (int i = 0; i < m_activeRealTargets.Length; ++i)
            {
                Editor.Undo.RecordTransform(m_activeRealTargets[i]);
            }
            Editor.Undo.EndRecord();
        }

        private void OnRedoCompleted()
        {
            if (Editor.Tools.PivotMode == RuntimePivotMode.Center)
            {
                if(m_realTargets != null)
                {
                    Targets = m_realTargets;
                }
             
            }
        }

        private void OnUndoCompleted()
        {
            if (Editor.Tools.PivotMode == RuntimePivotMode.Center)
            {
                if (m_realTargets != null)
                {
                    Targets = m_realTargets;
                }
              
            }
        }

        /// Hit testing methods      
        protected virtual bool HitCenter()
        {
            Vector2 screenCenter = ActiveWindow.Camera.WorldToScreenPoint(transform.position);
            Vector2 mouse = ActiveWindow.Pointer.ScreenPoint;

            return (mouse - screenCenter).magnitude <= Appearance.SelectionMargin * SelectionMarginPixels;
        }

        protected virtual bool HitAxis(Vector3 axis, Matrix4x4 matrix, out float distanceToAxis)
        {
            axis = matrix.MultiplyVector(axis);
            Vector2 screenVectorBegin = ActiveWindow.Camera.WorldToScreenPoint(transform.position);
            Vector2 screenVectorEnd = ActiveWindow.Camera.WorldToScreenPoint(axis + transform.position);

            Vector3 screenVector = screenVectorEnd - screenVectorBegin;
            float screenVectorMag = screenVector.magnitude;
            screenVector.Normalize();
            if (screenVector != Vector3.zero)
            {
                return HitScreenAxis(out distanceToAxis, screenVectorBegin, screenVector, screenVectorMag);
            }
            else
            {
                Vector2 mousePosition = ActiveWindow.Pointer.ScreenPoint;

                distanceToAxis = (screenVectorBegin - mousePosition).magnitude;
                bool result = distanceToAxis <= Appearance.SelectionMargin * SelectionMarginPixels;
                if (!result)
                {
                    distanceToAxis = float.PositiveInfinity;
                }
                else
                {
                    distanceToAxis = 0.0f;
                }
                return result;
            }
        }

        protected virtual bool HitScreenAxis(out float distanceToAxis, Vector2 screenVectorBegin, Vector3 screenVector, float screenVectorMag)
        {
            Vector2 perp = PerpendicularClockwise(screenVector).normalized;
            Vector2 mousePosition = Editor.Input.GetPointerXY(0);
            Vector2 relMousePositon = mousePosition - screenVectorBegin;

            distanceToAxis = Mathf.Abs(Vector2.Dot(perp, relMousePositon));
            Vector2 hitPoint = (relMousePositon - perp * distanceToAxis);
            float vectorSpaceCoord = Vector2.Dot(screenVector, hitPoint);

            float selectionMargin = Appearance.SelectionMargin * SelectionMarginPixels;
            bool result = vectorSpaceCoord <= screenVectorMag + selectionMargin && vectorSpaceCoord >= -selectionMargin && distanceToAxis <= selectionMargin;
            if (!result)
            {
                distanceToAxis = float.PositiveInfinity;
            }
            else
            {
                if (screenVectorMag < selectionMargin)
                {
                    distanceToAxis = 0.0f;
                }
            }
            return result;
        }

        protected virtual Plane GetDragPlane(Matrix4x4 matrix, Vector3 axis)
        {
            Plane plane = new Plane(matrix.MultiplyVector(axis).normalized, matrix.MultiplyPoint(Vector3.zero));
            return plane;
        }

        protected virtual Plane GetDragPlane()
        {
            Vector3 toCam = ActiveWindow.Camera.cameraToWorldMatrix.MultiplyVector(Vector3.forward); //Camera.transform.position - transform.position;
            Plane dragPlane = new Plane(toCam.normalized, transform.position);
            return dragPlane;
        }

        protected virtual bool GetPointOnDragPlane(Vector3 screenPos, out Vector3 point)
        {
            return GetPointOnDragPlane(m_dragPlane, screenPos, out point);
        }

        protected virtual bool GetPointOnDragPlane(Plane dragPlane, Vector3 screenPos, out Vector3 point)
        {
            Ray ray = ActiveWindow.Camera.ScreenPointToRay(screenPos);
            return GetPointOnDragPlane(dragPlane, ray, out point);
        }

        protected virtual bool GetPointOnDragPlane(Ray ray, out Vector3 point)
        {
            return GetPointOnDragPlane(m_dragPlane, ray, out point);
        }

        protected virtual bool GetPointOnDragPlane(Plane dragPlane, Ray ray, out Vector3 point)
        {
            float distance;
            if (dragPlane.Raycast(ray, out distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }

            point = Vector3.zero;
            return false;
        }


        private static Vector2 PerpendicularClockwise(Vector2 vector2)
        {
            return new Vector2(-vector2.y, vector2.x);
        }

        void IGL.Draw(int cullingMask)
        {
            RTLayer layer = RTLayer.SceneView;
            if((cullingMask & (int)layer) == 0)
            {
                return;
            }

            if(Model == null)
            {
                DrawOverride();
            }
        }

        protected virtual void DrawOverride()
        {

        }
    }
}
