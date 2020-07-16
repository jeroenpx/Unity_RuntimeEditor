using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaHandle : BaseHandle
    {
        private float[,] m_heights;
        private Terrain m_terrain;
        private Vector3 m_prevPoint;
        private ITerrainAreaProjector m_projector;
        private Mesh m_areaResizerMesh;
        private Material m_areaResizerMaterial;
        private Vector3 AreaResizerPosition
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

                AreaResizerPosition = Position;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_areaResizerMaterial = new Material(Shader.Find("Hidden/RTHandles/PointBillboard"));
            m_areaResizerMaterial.SetFloat("_Scale", 4.5f);
            m_areaResizerMaterial.SetInt("_HandleZTest", (int)CompareFunction.Always);
            m_areaResizerMesh = new Mesh();
            BuildPointsMesh(m_areaResizerMesh);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(m_areaResizerMaterial);
            Destroy(m_areaResizerMesh);
        }

        protected override void OnEnable()
        {
            BaseHandleInput input = GetComponent<BaseHandleInput>();
            if (input == null || input.Handle != this)
            {
                input = gameObject.AddComponent<TerrainAreaHandleInput>();
                input.Handle = this;
            }
            m_projector = IOC.Resolve<ITerrainAreaProjector>();
            base.OnEnable();
        }


        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override bool OnBeginDrag()
        {
            if(!base.OnBeginDrag())
            {
                return false;
            }
            
            if(SelectedAxis == RuntimeHandleAxis.Y)
            {
                Vector3 scale = WorldScaleToHeightMap(m_projector.Scale);
                
                m_heights = new float[Mathf.FloorToInt(scale.x) + 1, Mathf.FloorToInt(scale.z) + 1];

                DragPlane = GetDragPlane(Vector3.up);   
            }
            else if(SelectedAxis == RuntimeHandleAxis.Custom)
            {
                DragPlane = GetDragPlane(Matrix4x4.identity, Vector3.up);
            }
            else
            {
                return false;
            }

            return GetPointOnDragPlane(Window.Pointer, out m_prevPoint);

        }

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
                if(SelectedAxis == RuntimeHandleAxis.Y)
                {
                    point.z = m_prevPoint.z;
                    point.x = m_prevPoint.x;

                    SetPosition(Position + point - m_prevPoint);
                    m_prevPoint = point;

                    Vector3 p = WorldToHeightMapPoint(Position);

                    int rows = m_heights.GetLength(0);
                    int cols = m_heights.GetLength(1);
                    for (int i = 0; i < rows; ++i)
                    {
                        for(int j = 0; j < cols; ++j)
                        {
                            m_heights[i, j] = p.y;
                        }
                    }

                    float res = m_terrain.terrainData.heightmapResolution;
                    m_terrain.SetHeights(
                        (int)Mathf.Clamp(Mathf.RoundToInt(p.x - rows / 2), 0, res - 1),
                        (int)Mathf.Clamp(Mathf.RoundToInt(p.z - cols / 2), 0, res - 1), m_heights);
                }
                else 
                {
                    float d = (point - Position).magnitude;

                    SetAreaResizerPosition(point);
                    m_prevPoint = point;

                    m_projector.Scale = HeightMapToWorldScale(WorldScaleToHeightMap(new Vector3(d, 1, d)));
                }
                
            }
        }

        protected override void OnDrop()
        {
            base.OnDrop();

            if(m_terrain != null)
            {
                Vector3 position = Position - m_terrain.GetPosition();
                position.y = Mathf.Clamp(position.y, 0, m_terrain.terrainData.size.y);
                SetPosition(position + m_terrain.GetPosition());

                m_projector.Position = Position;
            }
        }

        public override RuntimeHandleAxis HitTest(out float distance)
        {
            RuntimeHandleAxis axis = Appearance.HitTestPositionHandle(Window.Camera, Window.Pointer, m_drawingSettings, out distance);

            Vector2 areaResizer = Window.Camera.WorldToScreenPoint(AreaResizerPosition);
            Vector2 screenPoint = Window.Pointer.ScreenPoint;
            float toAreaResizer = (screenPoint - areaResizer).magnitude;

            if (toAreaResizer <= Appearance.SelectionMargin * Appearance.SelectionMarginPixels)
            {
                distance = toAreaResizer;
                return RuntimeHandleAxis.Custom;
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

        private RTHandles.DrawingSettings m_drawingSettings = new RTHandles.DrawingSettings
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

            m_areaResizerMaterial.SetColor("_Color", SelectedAxis == RuntimeHandleAxis.Custom ? Appearance.Colors.SelectionColor : Appearance.Colors.YColor);

            camera.CommandBuffer.DrawMesh(m_areaResizerMesh, Matrix4x4.TRS(AreaResizerPosition, Quaternion.identity, Vector3.one), m_areaResizerMaterial, 0);
        }

        private void BuildPointsMesh(Mesh target)
        {
            Vector3[] vertices = new[] { Vector3.zero };

            int[] indices = new[]
            {
                0,
            };

            Color[] colors = new[]
            {
                Color.white
            };

            target.Clear();
            target.subMeshCount = 1;
            target.name = "TerrainAreaHandleVertices";
            target.vertices = vertices;
            target.SetIndices(indices, MeshTopology.Points, 0);
            target.colors = colors;
            target.RecalculateBounds();
        }

        public virtual void ChangePosition()
        {
            Vector3 position = Position;
        
            RaycastHit hit;
            if (Physics.Raycast(Window.Pointer, out hit))
            {
                if (Editor.Selection.IsSelected(hit.collider.gameObject) && hit.collider is TerrainCollider)
                {
                    SetPosition(HeightMapPointToWorld(WorldToHeightMapPoint(hit.point)));
                    if(m_projector != null)
                    {
                        Vector3 delta = Position - m_projector.Position;
                        AreaResizerPosition += delta;
                        m_projector.Position += delta;
                    }
                }
            }

            if (position != Position)
            {
                TryRefreshCommandBuffer();
            }
        }

        private Vector3 WorldScaleToHeightMap(Vector3 scale)
        {
            TerrainData data = m_terrain.terrainData;
            Vector3 size = data.size; 
            int resolution = data.heightmapResolution - 1;
            Vector3 result = new Vector3(Mathf.RoundToInt(scale.x * resolution / size.x), 1, Mathf.RoundToInt(scale.z * resolution / size.z));
            result.x = Mathf.FloorToInt(result.x / 2) * 2;
            result.z = Mathf.FloorToInt(result.z / 2) * 2;
            return result;
        }

        private Vector3 HeightMapToWorldScale(Vector3 scale)
        {
            TerrainData data = m_terrain.terrainData;
            Vector3 size = data.size;
            int resolution = data.heightmapResolution - 1;

            return new Vector3(scale.x * size.x / resolution, 1, scale.z * size.z / resolution);
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
            float z = Mathf.RoundToInt(resolution * point.z / size.z);

            return new Vector3(x, y, z);
        }

        private Vector3 HeightMapPointToWorld(Vector3 point)
        {
            TerrainData terrainData = m_terrain.terrainData;

            Vector3 size = terrainData.size;
            int resolution = terrainData.heightmapResolution - 1;

            float x = point.x * size.x / resolution;
            float y = terrainData.GetHeight(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.z));
            float z = point.z * size.z / resolution;

            return m_terrain.GetPosition() + new Vector3(x, y, z);
        }

        private void SetPosition(Vector3 value)
        {
            foreach (BaseHandle handle in AllHandles)
            {
                if(handle is TerrainAreaHandle)
                {
                    TerrainAreaHandle terrainAreaHandle = (TerrainAreaHandle)handle;
                    terrainAreaHandle.Position = value;
                    terrainAreaHandle.TryRefreshCommandBuffer();
                }   
            }
        }

        private void SetAreaResizerPosition(Vector3 value)
        {
            foreach (BaseHandle handle in AllHandles)
            {
                if (handle is TerrainAreaHandle)
                {
                    TerrainAreaHandle terrainAreaHandle = (TerrainAreaHandle)handle;
                    terrainAreaHandle.AreaResizerPosition = value;
                    terrainAreaHandle.TryRefreshCommandBuffer();
                }
            }
        }
    }
}
