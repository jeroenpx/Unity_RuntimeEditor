using Battlehub.RTCommon;
using Battlehub.RTHandles;
using SyntaxTree.VisualStudio.Unity.Bridge;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaHandle : BaseHandle
    {
        private Terrain m_terrain;
        private Vector3 m_prevPoint;
        private bool m_initialized = false;

        public TerrainProjectorBase Projector
        {
            get;
            set;
        }

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

        protected override void OnEnable()
        {  
            BaseHandleInput input = GetComponent<BaseHandleInput>();
            if (input == null || input.Handle != this)
            {
                input = gameObject.AddComponent<TerrainAreaHandleInput>();
                input.Handle = this;
            }
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_initialized = false;
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

        float[,] heights = new float[1, 1];

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
                
                Vector3 p = WorldToHeightMapPoint(Position);
                heights[0, 0] = p.y;
                m_terrain.SetHeights((int)p.x, (int)p.z, heights);

                RTECamera.RefreshCommandBuffer();
            }
        }

        protected override void OnDrop()
        {
            base.OnDrop();

            if(m_terrain != null)
            {
                Vector3 position = Position - m_terrain.GetPosition();
                position.y = Mathf.Clamp(position.y, 0, m_terrain.terrainData.size.y);
                //Position = position + m_terrain.GetPosition();
            }
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
            if (TrySelectAxis())
            {
                TryRefreshCommandBuffer();
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

            if(m_initialized)
            {
                Appearance.DoPositionHandle(camera.CommandBuffer, camera.Camera, m_drawingSettings);
            }
        }

        public virtual void ChangePosition()
        {
            Vector3 position = Position;
        
            RaycastHit hit;
            if (Physics.Raycast(Window.Pointer, out hit))
            {
                if (Editor.Selection.IsSelected(hit.collider.gameObject) && hit.collider is TerrainCollider)
                {
                    Position = HeightMapPointToWorld(WorldToHeightMapPoint(hit.point));
                    m_initialized = true;
                }
            }

            if (position != Position)
            {
                TryRefreshCommandBuffer();
            }
        }

        private Vector3 WorldToHeightMapPoint(Vector3 point)
        {
            point = point - m_terrain.GetPosition();

            TerrainData data = m_terrain.terrainData;

            Vector3 size = data.size;
            Vector3 scale = data.heightmapScale;
            int resolution = data.heightmapResolution - 1;

            float x = Mathf.RoundToInt(resolution * point.x / size.x);
            float y = point.y / scale.y;
            float z = Mathf.FloorToInt(resolution * point.z / size.z);

            return new Vector3(x, y, z);
        }

        private Vector3 HeightMapPointToWorld(Vector3 point)
        {
            TerrainData terrainData = m_terrain.terrainData;

            Vector3 size = terrainData.size;
            int resolution = terrainData.heightmapResolution - 1;

            float x = point.x * size.x / resolution;
            float y = terrainData.GetHeight((int)point.x, (int)point.z);
            float z = point.z * size.z / resolution;

            return m_terrain.GetPosition() + new Vector3(x, y, z);
        }

    }
}
