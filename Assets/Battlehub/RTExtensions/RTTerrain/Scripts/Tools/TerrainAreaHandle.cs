using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaHandle : BaseHandle
    {
        private Terrain m_terrain;
        private Vector3 m_prevPoint;

        public override Transform[] Targets 
        {
            get { return base.Targets; }
            set
            {
                if(value != null)
                {
                    foreach(Transform transform in value)
                    {
                        m_terrain = transform.GetComponent<Terrain>();
                        if(m_terrain != null)
                        {
                            base.Targets = new[] { m_terrain.transform };
                            break;
                        }
                    }
                }
                else
                {
                    base.Targets = value;
                }
            }
        }

        protected override bool OnBeginDrag()
        {
            if(!base.OnBeginDrag())
            {
                return false;
            }
            
            if(SelectedAxis != RuntimeHandleAxis.Y)
            {
                return false;
            }

            DragPlane = GetDragPlane(Vector3.up);

            return GetPointOnDragPlane(Window.Pointer, out m_prevPoint);
        }

        float[,] heights = new float[10, 10];

        protected override void OnDrag()
        {
            if (!Window.IsPointerOver)
            {
                return;
            }

            Vector3 point;
            if(!GetPointOnDragPlane(Window.Pointer, out point))
            {
                return;
            }

            if(point != m_prevPoint)
            {
                point.z = m_prevPoint.z;
                point.x = m_prevPoint.x;

                Position += point - m_prevPoint;
                m_prevPoint = point;

                for (int i = 0; i < 10; ++i)
                {
                    for (int j = 0; j < 10; ++j)
                    {
                        heights[i, j] = Position.y;
                    }
                }

                m_terrain.SetHeights(0, 0, heights);

                RTECamera.RefreshCommandBuffer();
            }
        }

        protected override void OnDrop()
        {
            base.OnDrop();
            Debug.Log("OnDrop");
        }

        public override RuntimeHandleAxis HitTest(out float distance)
        {            
            RuntimeHandleAxis axis = Appearance.HitTestPositionHandle(Window.Camera, Window.Pointer, m_drawingSettings, out distance);

            if(axis != RuntimeHandleAxis.Y)
            {
             
            }

            return axis;
        }

        protected override void UpdateOverride()
        {
            Vector3 position = Position;
            RuntimeHandleAxis selectedAxis = SelectedAxis;

            if (Editor.Tools.IsViewing)
            {
                SelectedAxis = RuntimeHandleAxis.None;
                return;
            }

            if (!IsWindowActive || !Window.IsPointerOver)
            {
                return;
            }

            if (!IsDragging && SelectedAxis == RuntimeHandleAxis.None)
            {
                if (ChangePositionAction())
                {
                    RaycastHit hit;
                    if (Physics.Raycast(Window.Pointer, out hit))
                    {
                        if (Editor.Selection.IsSelected(hit.collider.gameObject) && hit.collider is TerrainCollider)
                        {
                            Position = hit.point;
                        }
                    }
                }
            }

            if (HightlightOnHover && !IsDragging)
            {
                SelectedAxis = HitTester.GetSelectedAxis(this);
            }

            if(position != Position || selectedAxis != SelectedAxis)
            {
                if (Model == null && RTECamera != null)
                {
                    RTECamera.RefreshCommandBuffer();
                }
            }
        }


        private DrawingSettings m_drawingSettings = new DrawingSettings
        {
            LockObject = new LockObject { PositionX = true, PositionZ = true },
            DrawLocked = false
        };
        protected override void RefreshCommandBuffer(IRTECamera camera)
        {
            base.RefreshCommandBuffer(camera);
            m_drawingSettings.Position = transform.position;
            m_drawingSettings.Rotation = transform.rotation;
            m_drawingSettings.SelectedAxis = SelectedAxis;
            
            Appearance.DoPositionHandle(camera.CommandBuffer, camera.Camera, m_drawingSettings);
        }

        public virtual bool ChangePositionAction()
        {
            return Editor.Input.GetPointerUp(0);
        }
    }
}
