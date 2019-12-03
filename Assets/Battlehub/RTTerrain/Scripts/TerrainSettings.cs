using Battlehub.RTCommon;
using Battlehub.RTEditor;
using System;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public interface ITerrainSettings
    {
        Vector3 Position
        {
            get;
            set;
        }
        float Width
        {
            get;
            set;
        }

        float Length
        {
            get;
            set;
        }

        float Resolution
        {
            get;
            set;
        }
        void Refresh();
        void InitEditor(PropertyEditor editor, PropertyInfo property, string label);
    }

    public class TerrainSettings : EditorOverride, ITerrainSettings
    {
        private Terrain m_terrain;

        public Vector3 Position
        {
            get { return GetValueSafe(() => m_terrain.transform.localPosition); }
            set
            {
                SetValueSafe(value, v =>
                {
                    m_terrain.transform.localPosition = v;
                });
            }
        }

        public float Width
        {
            get;
            set;
        }

        public float Length
        {
            get;
            set;
        }

        public float Resolution
        {
            get;
            set;
        }

        private T GetValueSafe<T>(Func<T> func)
        {
            if (m_terrain == null || m_terrain.terrainData == null)
            {
                return default;
            }

            return func();
        }

        private void SetValueSafe<T>(T value, Action<T> action)
        {
            if (m_terrain == null || m_terrain.terrainData == null)
            {
                return;
            }

            action(value);
        }

        private float[,] GetHeightmap()
        {
            int w = m_terrain.terrainData.heightmapWidth;
            int h = m_terrain.terrainData.heightmapHeight;
            return m_terrain.terrainData.GetHeights(0, 0, w, h);
        }

        private IRTE m_editor;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            m_editor = IOC.Resolve<IRTE>();
            OnSelectionChanged(null);
            m_editor.Selection.SelectionChanged += OnSelectionChanged;
            IOC.RegisterFallback<ITerrainSettings>(this);
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }
            IOC.UnregisterFallback<ITerrainSettings>(this);
        }

        private void OnSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            if (m_editor.Selection.activeGameObject == null)
            {
                m_terrain = null;
            }
            else
            {
                m_terrain = m_editor.Selection.activeGameObject.GetComponent<Terrain>();
            }

            if(m_terrain != null)
            {
                Width = GetValueSafe(() => m_terrain.terrainData.size.x);
                Length = GetValueSafe(() => m_terrain.terrainData.size.z);
                Resolution = GetValueSafe(() => m_terrain.terrainData.heightmapResolution);
            }
        }

        public void Refresh()
        {
            if (m_terrain == null || m_terrain.terrainData == null)
            {
                return;
            }

            Vector3 oldSize = m_terrain.terrainData.size;
            Vector3 newSize = m_terrain.terrainData.size;
            newSize.x = Mathf.Clamp(Width, 10, 500);
            newSize.z = Mathf.Clamp(Length, 10, 500);

            int oldHeightmapResolution = m_terrain.terrainData.heightmapResolution;
            int newHeightmapResolution = Mathf.Clamp(Mathf.RoundToInt(Resolution), 17, 2049);

            float[,] oldHeightMap = null;
            float[,] newHeightMap = null;
            if (oldHeightmapResolution != newHeightmapResolution)
            {
                oldHeightMap = GetHeightmap();
                m_terrain.terrainData.heightmapResolution = newHeightmapResolution;
                newHeightMap = GetHeightmap();
            }

            Terrain terrain = m_terrain;
            Action undo = () =>
            {
                if (terrain == null || terrain.terrainData == null)
                {
                    return;
                }

                if (terrain.terrainData.heightmapResolution != oldHeightmapResolution)
                {
                    terrain.terrainData.heightmapResolution = oldHeightmapResolution;
                    terrain.terrainData.SetHeights(0, 0, oldHeightMap);
                }
                terrain.terrainData.size = oldSize;

                Width = terrain.terrainData.size.x;
                Length = terrain.terrainData.size.z;
                Resolution = terrain.terrainData.heightmapResolution;
            };

            Action redo = () =>
            {
                if (terrain == null || terrain.terrainData == null)
                {
                    return;
                }

                if (terrain.terrainData.heightmapResolution != newHeightmapResolution)
                {
                    terrain.terrainData.heightmapResolution = newHeightmapResolution;
                    terrain.terrainData.SetHeights(0, 0, newHeightMap);
                }
                terrain.terrainData.size = newSize;

                Width = terrain.terrainData.size.x;
                Length = terrain.terrainData.size.z;
                Resolution = terrain.terrainData.heightmapResolution;
            };

            redo();

            ITerrainSelectionHandlesTool tool = IOC.Resolve<ITerrainSelectionHandlesTool>();
            if (tool == null || !tool.Refresh(redo, undo))
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.Undo.CreateRecord(record => { redo(); return true; }, record => { undo(); return true; });
            }
        }

        public void InitEditor(PropertyEditor editor, PropertyInfo property, string label)
        {
            editor.Init(m_terrain, this, property, null, label, null, null, Refresh, false);
        }
    }
}

