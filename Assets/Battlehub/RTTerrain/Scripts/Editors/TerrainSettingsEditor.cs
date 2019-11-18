using Battlehub.RTEditor;
using Battlehub.Utils;
using System;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainSettingsEditor : MonoBehaviour
    {
        [SerializeField]
        private FloatEditor m_widthEditor = null;
        [SerializeField]
        private FloatEditor m_lengthEditor = null;
        [SerializeField]
        private FloatEditor m_heightmapResolutionEditor = null;
        [SerializeField]
        private Vector3Editor m_positionEditor = null;

        private TerrainEditor m_terrainEditor;
        protected TerrainEditor TerrainEditor
        {
            get { return m_terrainEditor; }
        }

        private TerrainBrush m_terrainBrush;

        private class TerrainSettingsAccessor
        {
            private Terrain m_terrain;

            public float Width
            {
                get { return GetValueSafe(() => m_terrain.terrainData.size.x); }
                set
                {
                    SetValueSafe(value, v =>
                    {
                        Vector3 size = m_terrain.terrainData.size;
                        size.x = Mathf.Clamp(v, 10, 500);
                        m_terrain.terrainData.size = size;
                    });
                }
            }

            public float Length
            {
                get { return GetValueSafe(() => m_terrain.terrainData.size.z); }
                set
                {
                    SetValueSafe(value, v =>
                    {
                        Vector3 size = m_terrain.terrainData.size;
                        size.z = Mathf.Clamp(v, 10, 500);
                        m_terrain.terrainData.size = size;
                    });       
                }
            }

            public float Resolution
            {
                get { return GetValueSafe(() => m_terrain.terrainData.heightmapResolution); }
                set
                {
                    SetValueSafe(value, v =>
                    {
                        //int heightMapRes = (1 << Mathf.CeilToInt(Mathf.Log(v, 2))) + 1;
                        m_terrain.terrainData.heightmapResolution = Mathf.Clamp(Mathf.RoundToInt(v), 17, 2049);
                    });
                }
            }

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

            public TerrainSettingsAccessor(Terrain terrain)
            {
                m_terrain = terrain;
            }
        }

        protected virtual void Awake()
        {
            m_terrainEditor = GetComponentInParent<TerrainEditor>();
            m_terrainEditor.TerrainChanged += OnTerrainChanged;
        }
        
        protected virtual void OnDestroy()
        {
            if(m_terrainEditor != null)
            {
                m_terrainEditor.TerrainChanged -= OnTerrainChanged;
            }
        }

        protected virtual void OnEnable()
        {
            InitEditors();
        }

        protected virtual void OnDisable()
        {
            
        }

        private void OnTerrainChanged()
        {
            InitEditors();
        }

        private void InitEditors()
        {
            Terrain terrain = TerrainEditor.Terrain;
            if (terrain != null)
            {
                if (m_widthEditor != null)
                {
                    m_widthEditor.Init(terrain, new TerrainSettingsAccessor(terrain), Strong.PropertyInfo((TerrainSettingsAccessor x) => x.Width), null, "Width");
                }

                if (m_lengthEditor != null)
                {
                    m_lengthEditor.Init(terrain, new TerrainSettingsAccessor(terrain), Strong.PropertyInfo((TerrainSettingsAccessor x) => x.Length), null, "Length");
                }

                if(m_heightmapResolutionEditor != null)
                {
                    m_heightmapResolutionEditor.Init(terrain, new TerrainSettingsAccessor(terrain), Strong.PropertyInfo((TerrainSettingsAccessor x) => x.Resolution), null, "Resolution");
                }

                if(m_positionEditor != null)
                {
                    m_positionEditor.Init(terrain, new TerrainSettingsAccessor(terrain), Strong.PropertyInfo((TerrainSettingsAccessor x) => x.Position), null, "Position");
                }
            }
        }

    }
}

