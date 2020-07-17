using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaHandle : BaseHandle
    {
        private Terrain m_terrain;
        private float[,] m_beginDragHeights;
        private float[,] m_heights;
        private Vector3 m_beginDragPoint;
        private Vector3 m_prevPoint;
        private ITerrainAreaProjector m_projector;
        private Mesh m_areaResizerMesh;
        private Material m_areaResizerMaterial;
        private int m_areaResizerIndex;
        private HashSet<Transform> m_objects = new HashSet<Transform>();

        public bool AbsoluteHeightMode
        {
            get;
            set;
        }
        
        public Vector3[] AreaResizerPositions
        {
            get { return m_areaResizerMesh.vertices; }
            set 
            {
                m_areaResizerMesh.vertices = value;
                m_areaResizerMesh.RecalculateBounds();
            }
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

                if(m_areaResizerMesh != null)
                {
                    Destroy(m_areaResizerMesh);
                }

                m_areaResizerMesh = new Mesh();
                BuildPointsMesh(m_areaResizerMesh);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_areaResizerMaterial = new Material(Shader.Find("Hidden/RTHandles/PointBillboard"));
            m_areaResizerMaterial.SetFloat("_Scale", 4.5f);
            m_areaResizerMaterial.SetInt("_HandleZTest", (int)CompareFunction.Always);
            if (m_areaResizerMesh != null)
            {
                Destroy(m_areaResizerMesh);
            }
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

                Vector3 p = WorldToHeightMapPoint(Position);
                int res = m_terrain.terrainData.heightmapResolution;
                int rows = Mathf.FloorToInt(scale.z) + 1;
                int cols = Mathf.FloorToInt(scale.x) + 1;
                rows += Mathf.Clamp(Mathf.RoundToInt(p.z - rows / 2), int.MinValue, 0);
                cols += Mathf.Clamp(Mathf.RoundToInt(p.x - cols / 2), int.MinValue, 0);
                rows += Mathf.Clamp(-(Mathf.RoundToInt(p.z + rows / 2) - res + 1), int.MinValue, 0);
                cols += Mathf.Clamp(-(Mathf.RoundToInt(p.x + cols / 2) - res + 1), int.MinValue, 0);
                
                if (rows < 0 || cols < 0)
                {
                    return false;
                }

                m_beginDragHeights = m_terrain.GetHeights(
                       Mathf.Clamp(Mathf.RoundToInt(p.x - cols / 2), 0, res - (cols + 1)),
                       Mathf.Clamp(Mathf.RoundToInt(p.z - rows / 2), 0, res - (rows + 1)), cols, rows);
                m_heights = new float[rows, cols];

                m_beginDragPoint = WorldToHeightMapPoint(Position);
                DragPlane = GetDragPlane(Vector3.up);

                m_objects.Clear();
                RaycastHit[] hits = Physics.BoxCastAll(m_projector.Position + Vector3.up * m_projector.Scale.y, m_projector.Scale * 0.5f, Vector3.down);
                foreach (RaycastHit hit in hits)
                {
                    if(!m_objects.Contains(hit.transform) && !(hit.collider is TerrainCollider))
                    {
                        m_objects.Add(hit.transform);
                    }
                }
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

            if(SelectedAxis == RuntimeHandleAxis.Y)
            {
                Vector3 pointOnDragPlane;
                if (!GetPointOnDragPlane(Window.Pointer, out pointOnDragPlane))
                {
                    return;
                }

                if (pointOnDragPlane != m_prevPoint)
                {
                    pointOnDragPlane.z = m_prevPoint.z;
                    pointOnDragPlane.x = m_prevPoint.x;

                    SetPosition(Position + pointOnDragPlane - m_prevPoint);
                    m_prevPoint = pointOnDragPlane;

                    int rows = m_heights.GetLength(0);
                    int cols = m_heights.GetLength(1);
                    Vector3 point = WorldToHeightMapPoint(Position);

                    if (AbsoluteHeightMode)
                    {
                        for (int i = 0; i < rows; ++i)
                        {
                            float v = i / (float)(rows - 1);
                            for (int j = 0; j < cols; ++j)
                            {
                                float u = j / (float)(cols - 1);
                                Color c = m_projector.Brush.GetPixelBilinear(u, v);
                                m_heights[i, j] = point.y * c.a;
                            }
                        }
                    }
                    else
                    { 
                        float delta = (point - m_beginDragPoint).y;
                    
                        for (int i = 0; i < rows; ++i)
                        {
                            float v = i / (float)(rows - 1);
                            for (int j = 0; j < cols; ++j)
                            {
                                float u = j / (float)(cols - 1);
                                Color c = m_projector.Brush.GetPixelBilinear(u, v);
                                m_heights[i, j] = m_beginDragHeights[i, j] + delta * c.a;
                            }
                        }

                        foreach(Transform obj in m_objects)
                        {
                            Vector3 position = obj.position;
                            position.y = HeightMapPointToWorld(WorldToHeightMapPoint(position)).y;
                            obj.position = position;
                        }
                    }
                   
                    float res = m_terrain.terrainData.heightmapResolution;
                    m_terrain.SetHeights(
                        (int)Mathf.Clamp(Mathf.RoundToInt(point.x - cols / 2), 0, res - (cols + 1)),
                        (int)Mathf.Clamp(Mathf.RoundToInt(point.z - rows / 2), 0, res - (rows + 1)), m_heights);
                }
            }
            else if (SelectedAxis == RuntimeHandleAxis.Custom) 
            {
                Vector3 point = m_prevPoint;
                RaycastHit hit;
                if (Physics.Raycast(Window.Pointer, out hit))
                {
                    if (Editor.Selection.IsSelected(hit.collider.gameObject) && hit.collider is TerrainCollider)
                    {
                        point = hit.point;
                    }
                }

                if(m_prevPoint != point && m_areaResizerIndex >= 0)
                {
                    SetAreaResizerPosition(point, m_areaResizerIndex);
                    m_prevPoint = point;

                    Vector3[] areaResizerPositions = AreaResizerPositions;
                    Vector3 areaSize = HeightMapToWorldScale(WorldScaleToHeightMap((areaResizerPositions[1] - areaResizerPositions[0]) * 0.5f));
                    Vector3 areaCenter = m_areaResizerIndex > 0 ? areaResizerPositions[0] + areaSize : areaResizerPositions[1] - areaSize;

                    Position = HeightMapPointToWorld(WorldToHeightMapPoint(areaCenter));
                    m_projector.Position = HeightMapPointToWorld(WorldToHeightMapPoint(areaCenter));

                    float scaleX = Mathf.Abs(areaSize.x * 2);
                    float scaleZ = Mathf.Abs(areaSize.z * 2);

                    Vector3 projectorScale = new Vector3(scaleX, 1, scaleZ);
                    projectorScale.x = Mathf.Max(projectorScale.x, 0.1f);
                    projectorScale.z = Mathf.Max(projectorScale.z, 0.1f);
                    m_projector.Scale = projectorScale;
                }
            }
        }

        protected override void OnDrop()
        {
            base.OnDrop();
            m_objects.Clear();

            if (m_terrain != null)
            {
                Vector3 position = Position - m_terrain.GetPosition();
                position.y = Mathf.Clamp(position.y, 0, m_terrain.terrainData.size.y);

                position = HeightMapPointToWorld(WorldToHeightMapPoint(position));
                SetPosition(position + m_terrain.GetPosition());

                Vector3 projectorScale = m_projector.Scale;
                projectorScale.y = 0;

                SetAreaResizerPosition(Position - projectorScale * 0.5f, 0);
                SetAreaResizerPosition(Position + projectorScale * 0.5f, 1);

                UpdateAreaResizersHight();
            }
        }

        private void UpdateAreaResizersHight()
        {
            SetAreaResizerPosition(HeightMapPointToWorld(WorldToHeightMapPoint(AreaResizerPositions[0])), 0);
            SetAreaResizerPosition(HeightMapPointToWorld(WorldToHeightMapPoint(AreaResizerPositions[1])), 1);
        }

        public override RuntimeHandleAxis HitTest(out float distance)
        {
            if(IsDragging)
            {
                distance = 0;
                return SelectedAxis;
            }

            RuntimeHandleAxis axis = Appearance.HitTestPositionHandle(Window.Camera, Window.Pointer, m_drawingSettings, out distance);

            Vector3[] areaResizerPositions = AreaResizerPositions;

            m_areaResizerIndex = -1;
            for(int i = 0; i < areaResizerPositions.Length; ++i)
            {
                Vector2 areaResizer = Window.Camera.WorldToScreenPoint(areaResizerPositions[i]);
                Vector2 screenPoint = Window.Pointer.ScreenPoint;
                float toAreaResizer = (screenPoint - areaResizer).magnitude;

                if (toAreaResizer <= Appearance.SelectionMargin * Appearance.SelectionMarginPixels)
                {
                    distance = toAreaResizer;
                    axis = RuntimeHandleAxis.Custom;
                    m_areaResizerIndex = i;
                }
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

            Color[] colors = m_areaResizerMesh.colors;
            for(int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Appearance.Colors.YColor;
            }

            if(SelectedAxis == RuntimeHandleAxis.Custom && m_areaResizerIndex >= 0)
            {
                colors[m_areaResizerIndex] = Appearance.Colors.SelectionColor;
            }

            m_areaResizerMesh.colors = colors;
            camera.CommandBuffer.DrawMesh(m_areaResizerMesh, Matrix4x4.identity, m_areaResizerMaterial, 0);
        }

        private void BuildPointsMesh(Mesh target)
        {
            Vector3[] vertices = new[] { Vector3.zero, Vector3.zero };

            int[] indices = new[]
            {
                0, 1
            };

            Color[] colors = new Color[]
            {
                Color.white, Color.white
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
                        Vector3[] resizerPositions = AreaResizerPositions;
                        for(int i = 0; i < resizerPositions.Length; ++i)
                        {
                            resizerPositions[i] += delta;
                        }
                        AreaResizerPositions = resizerPositions;
                        m_projector.Position += delta;
                    }
                    UpdateAreaResizersHight();
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

        private void SetAreaResizerPosition(Vector3 value, int index)
        {
            foreach (BaseHandle handle in AllHandles)
            {
                if (handle is TerrainAreaHandle)
                {
                    TerrainAreaHandle terrainAreaHandle = (TerrainAreaHandle)handle;
                    Vector3[] vertices = terrainAreaHandle.AreaResizerPositions;
                    vertices[index] = value;
                    terrainAreaHandle.AreaResizerPositions = vertices;
                    terrainAreaHandle.TryRefreshCommandBuffer();
                }
            }
        }
    }
}
