using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Battlehub.RTHandles;
using Battlehub.RTCommon;
using System.Linq;
using System;

using UnityObject = UnityEngine.Object;
using Battlehub.RTEditor;

namespace Battlehub.RTTerrain
{
    public interface ITerrainTool
    {
        Terrain ActiveTerrain
        {
            get; set;
        }

        float ZSpacing
        {
            get;
            set;
        }

        float XSpacing
        {
            get;
            set;
        }

        bool EnableZTest
        {
            get;
            set;
        }

        void ResetPosition();
        void CutHoles();
        void ClearHoles();
    }

    public class TerrainTool : EditorOverride, ITerrainTool
    {
        public enum Interpolation
        {
            Bilinear,
            Bicubic
        }

        private Interpolation m_prevInterpolation;

        [SerializeField]
        private bool m_enableZTest = true;

        public bool EnableZTest
        {
            get { return m_enableZTest; }
            set
            {
                m_enableZTest = value;
                if (ActiveTerrain == null)
                {
                    return;
                }

                foreach (GameObject go in m_handles.Keys)
                {
                    TerrainToolHandle handle = go.GetComponent<TerrainToolHandle>();
                    handle.ZTest = value;
                }
            }
        }

        public float ZSpacing
        {
            get
            {
                if (ActiveTerrain == null || m_state == null)
                {
                    return 0;
                }
                return m_state.ZSpacing;
            }
            set
            {
                if (ActiveTerrain == null || m_state == null)
                {
                    return;
                }

                if (m_state.ZSpacing != value)
                {
                    m_state.ZSpacing = value;
                    m_zCount = Mathf.FloorToInt(m_state.ZSize / m_state.ZSpacing) + 1;
                    Refresh();
                }
            }
        }

        public float XSpacing
        {
            get
            {
                if (ActiveTerrain == null || m_state == null)
                {
                    return 0;
                }
                return m_state.XSpacing;
            }
            set
            {
                if (ActiveTerrain == null || m_state == null)
                {
                    return;
                }

                if (m_state.XSpacing != value)
                {
                    m_state.XSpacing = value;
                    m_xCount = Mathf.FloorToInt(m_state.XSize / m_state.XSpacing) + 1;
                    Refresh();
                }
            }
        }

        private Terrain m_activeTerrain;
        public Terrain ActiveTerrain
        {
            get { return m_activeTerrain; }
            set
            {
                gameObject.SetActive(m_activeTerrain != null);
                if (m_activeTerrain != value)
                {
                    m_activeTerrain = value;
                    Disable();
                    if(m_activeTerrain != null)
                    {
                        Enable();
                    }
                }
            }
        }

        public TerrainData ActiveTerrainData
        {
            get
            {
                Terrain activeTerrain = ActiveTerrain;
                if (activeTerrain == null)
                {
                    return null;
                }

                return activeTerrain.terrainData;
            }
        }


        [SerializeField]
        public TerrainToolHandle m_handlePrefab;
        private TerrainToolState m_state;

        private IRTE m_editor;
        private int m_zCount;
        private int m_xCount;
        private float[,] m_lerpGrid;
        private Dictionary<GameObject, int> m_handles;
        private GameObject[] m_targetHandles;
        private bool m_isDragging;
        private IRuntimeSceneComponent m_sceneComponent;
        private TerrainToolHandle m_pointerOverHandle;
        private CachedBicubicInterpolator m_interpolator;
        private ITerrainCutoutMaskRenderer m_cutoutMaskRenderer;

        private float[,] m_additiveHeights;
        private float[,] m_interpolatedHeights;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            m_editor = IOC.Resolve<IRTE>();
            IOC.RegisterFallback<ITerrainTool>(this);
            m_cutoutMaskRenderer = IOC.Resolve<ITerrainCutoutMaskRenderer>();
            m_cutoutMaskRenderer.ObjectImageLayer = m_editor.CameraLayerSettings.ResourcePreviewLayer;
            ActiveTerrain = null;
        }

        private void Enable()
        {
            m_editor.ActiveWindowChanged += OnActiveWindowChanged;
            m_editor.Selection.SelectionChanged += OnSelectionChanged;
            m_interpolator = new CachedBicubicInterpolator();

            TerrainData data = ActiveTerrainData;
            m_additiveHeights = data.GetHeights(0, 0, data.heightmapWidth, data.heightmapHeight);
            m_interpolatedHeights = new float[data.heightmapHeight, data.heightmapWidth];

            const float preferredSpace = 10;

            m_state = ActiveTerrain.GetComponent<TerrainToolState>();
            if (m_state == null || m_state.HeightMap.Length != data.heightmapWidth * data.heightmapHeight)
            {
                m_state = ActiveTerrain.gameObject.GetComponent<TerrainToolState>();
                if(m_state == null)
                {
                    m_state = ActiveTerrain.gameObject.AddComponent<TerrainToolState>();
                }
                
                m_state.ZSize = ActiveTerrainData.size.z;
                m_state.XSize = ActiveTerrainData.size.x;

                m_zCount = Mathf.Max(2, Mathf.FloorToInt(ActiveTerrainData.size.z / preferredSpace)) + 1;
                m_xCount = Mathf.Max(2, Mathf.FloorToInt(ActiveTerrainData.size.x / preferredSpace)) + 1;

                m_state.ZSpacing = m_state.ZSize / (m_zCount - 1);
                m_state.XSpacing = m_state.XSize / (m_xCount - 1);

                m_state.Grid = new float[m_zCount * m_xCount];
                m_state.HeightMap = new float[data.heightmapWidth * data.heightmapHeight];
                m_state.CutoutTexture = m_cutoutMaskRenderer.CreateMask(ActiveTerrainData, null);
            }
            else
            {
                m_state.ZSize = ActiveTerrainData.size.z;
                m_state.XSize = ActiveTerrainData.size.x;

                m_state.ZSpacing = m_state.ZSize / (m_zCount - 1);
                m_state.XSpacing = m_state.XSize / (m_xCount - 1);

                TryRefreshGrid();
            }

            InitHandles();
            OnActiveWindowChanged(m_editor.ActiveWindow);
            EnableZTest = EnableZTest;
        }

        private void Disable()
        {
            if (m_sceneComponent != null)
            {
                m_sceneComponent.PositionHandle.BeforeDrag.RemoveListener(OnBeforeDrag);
                m_sceneComponent.PositionHandle.Drop.RemoveListener(OnDrop);
            }

            m_sceneComponent = null;

            if (m_editor != null)
            {
                m_editor.Selection.activeGameObject = null;
                m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void DisableHandles()
        {
            if (m_handles != null)
            {
                foreach (KeyValuePair<GameObject, int> kvp in m_handles)
                {
                    GameObject handle = kvp.Key;
                    handle.SetActive(false);

                   // LockAxes lockAxes = handle.gameObject.GetComponent<LockAxes>();
                   // lockAxes.PositionX = lockAxes.PositionY = lockAxes.PositionZ = true;
                   // lockAxes.ScaleX = lockAxes.ScaleY = lockAxes.ScaleZ = true;
                   // lockAxes.RotationX = lockAxes.RotationY = lockAxes.RotationZ = lockAxes.RotationScreen = lockAxes.RotationFree = true;
                }   
            }
        }

        private void DestroyHandles()
        {
            if (m_handles != null)
            {
                foreach (KeyValuePair<GameObject, int> kvp in m_handles)
                {
                    GameObject handle = kvp.Key;
                    Destroy(handle);
                }
                m_handles = null;
            }
            m_targetHandles = null;
            m_editor.Selection.activeGameObject = null;
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
        
            IOC.UnregisterFallback<ITerrainTool>(this);
            if(m_state != null)
            {
                if (m_state.CutoutTexture != null)
                {
                    Destroy(m_state.CutoutTexture);
                }
            }
            DestroyHandles();
        }

        private void InitAdditiveHeights(int hmWidth, int hmHeight)
        {
            for (int i = 0; i < hmHeight; ++i)
            {
                for (int j = 0; j < hmWidth; ++j)
                {
                    if (!IsCutout(j, i))
                    {
                        m_additiveHeights[i, j] -= m_interpolatedHeights[i, j];
                    }
                }
            }
        }

        private bool IsCutout(int x, int y)
        {
            int width = ActiveTerrainData.heightmapWidth;
            int height = ActiveTerrainData.heightmapHeight;

            float u = (float)(x) / width;
            float v = (float)(y) / height;
            if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
            {
                Color color = m_state.CutoutTexture.GetPixelBilinear(u, v);
                if (Mathf.Approximately(color.a, 1))
                {
                    return true;
                }
            }
            return false;
        }

        private void SetActiveTerrainHeights(int x, int y, float[,] heights)
        {
            int h = heights.GetLength(0);
            int w = heights.GetLength(1);

            float[,] currentHeights = ActiveTerrainData.GetHeights(x, y, w, h);
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if(IsCutout(j, i))
                    {
                        currentHeights[i, j] = m_additiveHeights[y + i, x + j];
                    }
                    else
                    {
                        currentHeights[i, j] = heights[i, j] + m_additiveHeights[y + i, x + j];
                    }
                }
            }

            ActiveTerrainData.SetHeights(x, y, currentHeights);
        }

        private void SetInterpolatedHeights(int x, int y, float[,] heights)
        {
            int h = heights.GetLength(0);
            int w = heights.GetLength(1);
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    m_interpolatedHeights[y + i, x + j] = heights[i, j];
                }
            }
        }

        private float[,] GetInterpolatedHeights(int x, int y, int w, int h)
        {
            float[,] result = new float[h, w];
            for(int i = 0; i < h; i++)
            {
                for(int j = 0; j < w; j++)
                {
                    result[i, j] = m_interpolatedHeights[y + i, x + j];
                }
            }
            return result;
        }


        public void ResetPosition()
        {
            if (m_targetHandles != null)
            {
                foreach (GameObject handle in m_targetHandles)
                {
                    Vector3 pos = handle.transform.localPosition;
                    pos.y = 0;
                    handle.transform.localPosition = pos;
                    UpdateTerrain(handle, GetInterpolatedHeights, SetActiveTerrainHeights);
                }
            }
        }

        public void ClearHoles()
        {
            CreateAndApplyCutoutTexture(new GameObject[0]);
        }

        public void CutHoles()
        {
            GameObject[] objects = m_editor.Selection.gameObjects;
            objects = CreateAndApplyCutoutTexture(objects);
        }

        private GameObject[] CreateAndApplyCutoutTexture(GameObject[] objects)
        {
            if (objects != null)
            {
                objects = objects.Where(o => !m_handles.ContainsKey(o) && !o.GetComponent<Terrain>()).ToArray();
            }

            if (m_state.CutoutTexture != null)
            {
                Destroy(m_state.CutoutTexture);
            }

            m_state.CutoutTexture = m_cutoutMaskRenderer.CreateMask(ActiveTerrainData, objects);

            float[,] hmap = m_interpolatedHeights; 
            SetActiveTerrainHeights(0, 0, hmap);
            return objects;
        }

        private void Refresh()
        {
            DisableHandles();
            TryRefreshGrid();
            InitHandles();
        }

        private void TryRefreshGrid()
        {
            TerrainData data = ActiveTerrainData;
            m_additiveHeights = data.GetHeights(0, 0, data.heightmapWidth, data.heightmapHeight);

            if (m_zCount * m_xCount == m_state.Grid.Length)
            {
                for (int i = 0; i < m_interpolatedHeights.GetLength(0); ++i)
                {
                    for (int j = 0; j < m_interpolatedHeights.GetLength(1); ++j)
                    {
                        m_interpolatedHeights[i, j] = m_state.HeightMap[i * m_interpolatedHeights.GetLength(1) + j];
                    }
                }
                
                InitAdditiveHeights(data.heightmapWidth, data.heightmapHeight);
                return;
            }

            m_zCount = Mathf.FloorToInt(m_state.ZSize / m_state.ZSpacing) + 1;
            m_xCount = Mathf.FloorToInt(m_state.XSize / m_state.XSpacing) + 1;
            m_state.Grid = new float[m_zCount * m_xCount];

            for (int i = 0; i < m_interpolatedHeights.GetLength(0); ++i)
            {
                for (int j = 0; j < m_interpolatedHeights.GetLength(1); ++j)
                {
                    m_interpolatedHeights[i, j] = 0;
                    m_state.HeightMap[i * m_interpolatedHeights.GetLength(1) + j] = 0;
                }
            }

            SetActiveTerrainHeights(0, 0, m_interpolatedHeights);
        }

        private void InitHandles()
        {
            m_prevInterpolation = m_state.Interpolation;
            InitLerpGrid();

            DisableHandles();
            GameObject[] cache = m_handles != null ? m_handles.Keys.ToArray() : new GameObject[0];
            m_handles = new Dictionary<GameObject, int>(m_state.Grid.Length);

            int handleNumber = 0;
            for (int x = 0; x < m_xCount; ++x)
            {
                for (int z = 0; z < m_zCount; ++z)
                {
                    m_handlePrefab.gameObject.SetActive(false);

                    TerrainToolHandle handle;
                    LockAxes lockAxes;
                    if (handleNumber < cache.Length)
                    {
                        handle = cache[handleNumber].GetComponent<TerrainToolHandle>();
                        lockAxes = handle.gameObject.GetComponent<LockAxes>();
                    }
                    else
                    {
                        handle = Instantiate(m_handlePrefab, transform);
                        lockAxes = handle.gameObject.AddComponent<LockAxes>();

                    }

                    lockAxes.PositionZ = false;
                    lockAxes.PositionX = true;
                    lockAxes.PositionZ = true;
                    lockAxes.ScaleX = lockAxes.ScaleY = lockAxes.ScaleZ = true;
                    lockAxes.RotationX = lockAxes.RotationY = lockAxes.RotationZ = lockAxes.RotationScreen = lockAxes.RotationFree = true;

                    handle.ZTest = EnableZTest;
                    handle.gameObject.hideFlags = HideFlags.HideInHierarchy;

                    float y = m_state.Grid[z * m_xCount + x] * ActiveTerrainData.heightmapScale.y;
                    handle.transform.localPosition = new Vector3(x * m_state.XSpacing, y, z * m_state.ZSpacing);
                    handle.name = "h " + x + "," + z;
                    handle.gameObject.SetActive(true);

                    m_handles.Add(handle.gameObject, z * m_xCount + x);
                    handleNumber++;
                }
            }
        }

        private void OnActiveWindowChanged(RuntimeWindow window)
        {
            if (m_editor.ActiveWindow == null)
            {
                return;
            }

            if (m_editor.ActiveWindow.WindowType == RuntimeWindowType.Scene)
            {
                if (m_sceneComponent != null)
                {
                    m_sceneComponent.PositionHandle.BeforeDrag.RemoveListener(OnBeforeDrag);
                    m_sceneComponent.PositionHandle.Drop.RemoveListener(OnDrop);
                }

                m_sceneComponent = m_editor.ActiveWindow.IOCContainer.Resolve<IRuntimeSceneComponent>();
                if (m_sceneComponent != null)
                {
                    m_sceneComponent.PositionHandle.BeforeDrag.AddListener(OnBeforeDrag);
                    m_sceneComponent.PositionHandle.Drop.AddListener(OnDrop);
                }
            }
        }

        private void OnSelectionChanged(UnityObject[] unselectedObjects)
        {
            if(unselectedObjects != null)
            {
                foreach (UnityObject obj in unselectedObjects)
                {
                    GameObject go = obj as GameObject;
                    if (go != null)
                    {
                        TerrainToolHandle handle = go.GetComponent<TerrainToolHandle>();
                        if (handle != null)
                        {
                            handle.IsSelected = false;
                        }
                    }
                }
            }
            
            if (m_editor.Selection.gameObjects == null || m_editor.Selection.gameObjects.Length == 0)
            {
                m_targetHandles = null;
            }
            else
            {
                m_targetHandles = m_editor.Selection.gameObjects.Where(go => go != null && m_handles.ContainsKey(go)).ToArray();
                foreach(GameObject go in m_targetHandles)
                {
                    TerrainToolHandle handle = go.GetComponent<TerrainToolHandle>();
                    handle.IsSelected = true;
                }
                if(m_targetHandles.Length == 0)
                {
                    m_targetHandles = null;
                }
            }
        }

        private Vector3[] m_handlePositions;
        
        private void OnBeforeDrag(BaseHandle handle)
        {
            m_isDragging = m_targetHandles != null;
            if (m_isDragging)
            {
                handle.EnableUndo = false;
                if (m_targetHandles != null)
                {
                    m_handlePositions = new Vector3[m_targetHandles.Length];
                    for (int i = 0; i < m_targetHandles.Length; ++i)
                    {
                        m_handlePositions[i] = m_targetHandles[i].transform.position;
                    }
                }
            }
        }

        private void OnDrop(BaseHandle handle)
        {
            if(m_isDragging)
            {
                m_isDragging = false;
                if (m_targetHandles != null)
                {
                    Vector3[] oldHandlePositions = m_handlePositions.ToArray();
                    Vector3[] handlePositions = new Vector3[m_targetHandles.Length];

                    for (int i = 0; i < m_targetHandles.Length; ++i)
                    {
                        handlePositions[i] = m_targetHandles[i].transform.position;
                        UpdateTerrain(m_targetHandles[i], GetInterpolatedHeights, SetActiveTerrainHeights);
                    }

                    m_editor.Undo.CreateRecord(redoRecord =>
                    {
                        m_targetHandles = m_editor.Selection.gameObjects.Where(go => go != null && m_handles.ContainsKey(go)).ToArray();
                        for (int i = 0; i < m_targetHandles.Length; ++i)
                        {
                            m_targetHandles[i].transform.position = handlePositions[i];
                            UpdateTerrain(m_targetHandles[i], GetInterpolatedHeights, SetActiveTerrainHeights);
                        }
                        return true;
                    },
                    undoRecord =>
                    {
                        m_targetHandles = m_editor.Selection.gameObjects.Where(go => go != null && m_handles.ContainsKey(go)).ToArray();
                        for (int i = 0; i < m_targetHandles.Length; ++i)
                        {
                            m_targetHandles[i].transform.position = oldHandlePositions[i];
                            UpdateTerrain(m_targetHandles[i], GetInterpolatedHeights, SetActiveTerrainHeights);
                        }
                        return true;
                    });

                    m_handlePositions = null;
                }
                handle.EnableUndo = true;
            }
        }

        private void LateUpdate()
        {
            if(ActiveTerrain == null)
            {
                gameObject.SetActive(false);
                return;
            }
                        
            Transform terrainTransform = ActiveTerrain.transform;
            if(terrainTransform.position != gameObject.transform.position ||
               terrainTransform.rotation != gameObject.transform.rotation ||
               terrainTransform.localScale != gameObject.transform.localScale)
            {
                gameObject.transform.position = terrainTransform.position;
                gameObject.transform.rotation = terrainTransform.rotation;
                gameObject.transform.localScale = terrainTransform.localScale;
            }

            if(m_editor.ActiveWindow != null)
            {
                RuntimeWindow window = m_editor.ActiveWindow;
                if(window.WindowType == RuntimeWindowType.Scene)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(window.Pointer, out hit))
                    {
                        TryHitTerrainHandle(hit);
                    }
                }
            }

            if(m_state.Interpolation != m_prevInterpolation)
            {
                m_prevInterpolation = m_state.Interpolation;
                InitLerpGrid();
            }

            if(m_isDragging)
            {
                if (m_targetHandles != null && m_targetHandles.Length == 1)
                {
                    for(int i = 0; i < m_targetHandles.Length; ++i)
                    {
                        UpdateTerrain(m_targetHandles[i], GetInterpolatedHeights, SetActiveTerrainHeights);
                    }
                }
            }
        }

        private void TryHitTerrainHandle(RaycastHit hit)
        {
            TerrainToolHandle handle = hit.collider.GetComponent<TerrainToolHandle>();
            if (m_pointerOverHandle != handle)
            {
                if (m_pointerOverHandle != null)
                {
                    m_pointerOverHandle.IsPointerOver = false;
                }

                m_pointerOverHandle = handle;

                if (m_pointerOverHandle != null)
                {
                    m_pointerOverHandle.IsPointerOver = true;
                }
            }
        }

        private void UpdateTerrain(GameObject handle, Func<int, int, int, int, float[,]> getValues, Action<int, int, float[,]> setValues)
        {
            switch (m_state.Interpolation)
            {
                case Interpolation.Bilinear: UpdateTerrainBilinear(handle, getValues, setValues); break;
                case Interpolation.Bicubic: UpdateTerrainBicubic(handle, getValues, setValues); break;
            }
        }

        private void UpdateTerrainBilinear(GameObject handle, Func<int, int, int, int, float[,]> getValues, Action<int, int, float[,]> setValues)
        {
            int hid;
            if (!m_handles.TryGetValue(handle, out hid))
            {
                Debug.LogError("Handle is not found!");
            }
            
            Vector3 pos = handle.transform.localPosition;
            UpdateTerrainBilinear(hid, pos, getValues, setValues);
        }

        private void UpdateTerrainBilinear(int hid, Vector3 position, Func<int, int, int, int, float[,]> getValues, Action<int, int, float[,]> setValues)
        {
            TerrainData data = ActiveTerrainData;
            float[] grid = m_state.Grid;
            grid[hid] = position.y / data.heightmapScale.y;

            m_lerpGrid[0, 0] = grid[hid - m_zCount - 1];
            m_lerpGrid[0, 1] = grid[hid - m_zCount];
            m_lerpGrid[0, 2] = grid[hid - m_zCount + 1];
            m_lerpGrid[1, 0] = grid[hid - 1];
            m_lerpGrid[1, 1] = grid[hid];
            m_lerpGrid[1, 2] = grid[hid + 1];
            m_lerpGrid[2, 0] = grid[hid + m_zCount - 1];
            m_lerpGrid[2, 1] = grid[hid + m_zCount];
            m_lerpGrid[2, 2] = grid[hid + m_zCount + 1];

            Vector2Int blockSize = new Vector2Int(
                (int)(m_state.XSpacing / data.heightmapScale.x),
                (int)(m_state.ZSpacing / data.heightmapScale.z));

            Vector2Int hPos = new Vector2Int(
                (int)(position.x / data.heightmapScale.x),
                (int)(position.z / data.heightmapScale.z));

            hPos -= blockSize;

            float[,] heightsvalues = getValues(
                hPos.x, hPos.y,
                blockSize.x * 2 + 1, blockSize.y * 2 + 1);

            for (int gy = 0; gy < 2; gy++)
            {
                int baseY = gy * blockSize.y;

                for (int gx = 0; gx < 2; gx++)
                {
                    int baseX = gx * blockSize.x;

                    for (int y = 0; y < blockSize.y; y++)
                    {
                        float ty = (float)y / blockSize.y;
                        for (int x = 0; x < blockSize.x; x++)
                        {
                            float tx = (float)x / blockSize.x;
                            heightsvalues[baseY + y, baseX + x] =
                                Mathf.Lerp(
                                    Mathf.Lerp(m_lerpGrid[gy, gx], m_lerpGrid[gy, gx + 1], tx),
                                    Mathf.Lerp(m_lerpGrid[gy + 1, gx], m_lerpGrid[gy + 1, gx + 1], tx),
                                    ty);
                        }
                    }
                }
            }

            setValues(hPos.x, hPos.y, heightsvalues);
        }

        private void UpdateTerrainBicubic(GameObject handle, Func<int, int, int, int, float[,]> getValues, Action<int, int, float[,]> setValues)
        {
            Vector3 position = handle.transform.localPosition;// Position;
            int hid = -1;
            m_handles.TryGetValue(handle, out hid);

            UpdateTerrainBicubic(hid, position, getValues, setValues);
        }

        private void UpdateTerrainBicubic(int hid, Vector3 position, Func<int, int, int, int, float[,]> getValues, Action<int, int, float[,]> setValues)
        {
            var data = ActiveTerrainData;
            if (hid >= 0)
            {
                m_state.Grid[hid] = position.y / data.heightmapScale.y;
            }
            else
            {
                Debug.LogError("Gizmo is not found!");
            }

            int2 iidx = new int2(hid % m_xCount, hid / m_xCount);

            for (int y = 0; y < 7; y++)
            {
                int _y = math.clamp(iidx.y - 3 + y, 0, m_zCount - 1);

                for (int x = 0; x < 7; x++)
                {
                    int _x = math.clamp(iidx.x - 3 + x, 0, m_xCount - 1);
                    m_lerpGrid[y, x] = m_state.Grid[m_xCount * _y + _x];
                }
            }

            float2 heightmapScale = ((float3)data.heightmapScale).xz;
            float2 pos = ((float3)position).xz;
            int2 block_size = (int2)(new float2(m_state.XSpacing, m_state.ZSpacing) / heightmapScale);

            int2 hPos = (int2)(pos / heightmapScale);
            hPos -= block_size * 2;

            int2 max_block = new int2(block_size.x * 4, block_size.y * 4);
            int res = data.heightmapResolution;
            RectInt r = new RectInt(hPos.x, hPos.y, max_block.x, max_block.y);
            r.xMin = math.clamp(r.xMin, 0, res);
            r.xMax = math.clamp(r.xMax, 0, res);
            r.yMin = math.clamp(r.yMin, 0, res);
            r.yMax = math.clamp(r.yMax, 0, res);

            float[,] hmap = getValues(r.x, r.y, r.width, r.height);

            for (int gy = 0; gy < 4; gy++)
            {
                int base_y = gy * block_size.y;

                for (int gx = 0; gx < 4; gx++)
                {
                    int base_x = gx * block_size.x;

                    m_interpolator.UpdateCoefficients(new float4x4(
                        m_lerpGrid[gy,     gx], m_lerpGrid[gy,     gx + 1], m_lerpGrid[gy,     gx + 2], m_lerpGrid[gy, gx + 3],
                        m_lerpGrid[gy + 1, gx], m_lerpGrid[gy + 1, gx + 1], m_lerpGrid[gy + 1, gx + 2], m_lerpGrid[gy + 1, gx + 3],
                        m_lerpGrid[gy + 2, gx], m_lerpGrid[gy + 2, gx + 1], m_lerpGrid[gy + 2, gx + 2], m_lerpGrid[gy + 2, gx + 3],
                        m_lerpGrid[gy + 3, gx], m_lerpGrid[gy + 3, gx + 1], m_lerpGrid[gy + 3, gx + 2], m_lerpGrid[gy + 3, gx + 3]
                    ));

                    for (int y = 0; y < block_size.y; y++)
                    {
                        int _y = hPos.y + base_y + y;
                        if (_y >= r.yMin && _y < r.yMax)
                        {
                            float ty = (float)y / block_size.y;

                            for (int x = 0; x < block_size.x; x++)
                            {
                                int _x = hPos.x + base_x + x;
                                if (_x >= r.xMin && _x < r.xMax)
                                {
                                    float tx = (float)x / block_size.x;

                                    try
                                    {
                                        float height = m_interpolator.GetValue(tx, ty);
                                        float u = (float)(r.x + (_x - r.xMin)) / data.heightmapWidth;
                                        float v = (float)(r.y + (_y - r.yMin)) / data.heightmapHeight;
                                        if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
                                        {
                                            Color color = m_state.CutoutTexture.GetPixelBilinear(u, v);
                                            if (Mathf.Approximately(color.a, 1))
                                            {
                                                hmap[_y - r.yMin, _x - r.xMin] = 0;
                                            }
                                            else
                                            {
                                                hmap[_y - r.yMin, _x - r.xMin] = height;
                                            }
                                        }
                                        else
                                        {
                                            hmap[_y - r.yMin, _x - r.xMin] = height;
                                        }
                                        
                                        m_state.HeightMap[(r.y + (_y - r.yMin)) * data.heightmapWidth + r.x + (_x - r.xMin)] = height;
                                        m_interpolatedHeights[(r.y + (_y - r.yMin)), r.x + (_x - r.xMin)] = height;
                                    }
                                    catch
                                    {
                                        Debug.LogError("!!!!!!!!!!!!!!!!!!!");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            setValues(r.x, r.y, hmap);
        }

        private void InitLerpGrid()
        {
            if (m_state.Interpolation == Interpolation.Bilinear)
            {
                m_lerpGrid = new float[3, 3];
            }
            else
            {
                m_lerpGrid = new float[7, 7];
            }
        }
    }
}
