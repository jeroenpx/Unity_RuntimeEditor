using System;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainPaintTextureEditor : MonoBehaviour
    {
        [SerializeField]
        private TerrainLayerEditor m_terrainLayerEditor = null;

        [SerializeField]
        private TerrainBrushEditor m_terrainBrushEditor = null;

        private TerrainEditor m_terrainEditor;

        private void Awake()
        {
            m_terrainEditor = GetComponentInParent<TerrainEditor>();
            m_terrainEditor.TerrainChanged += OnTerrainChanged;
            
            if(m_terrainBrushEditor != null)
            {
                m_terrainBrushEditor.SelectedBrushChanged += OnSelectedBrushChanged;
                m_terrainBrushEditor.BrushParamsChanged += OnBrushParamsChanged;
            }
        }

        private void OnDestroy()
        {
            if(m_terrainEditor != null)
            {
                m_terrainEditor.TerrainChanged -= OnTerrainChanged;
            }


            if (m_terrainBrushEditor != null)
            {
                m_terrainBrushEditor.SelectedBrushChanged -= OnSelectedBrushChanged;
                m_terrainBrushEditor.BrushParamsChanged -= OnBrushParamsChanged;
            }
        }

        private void OnEnable()
        {
            if (m_terrainLayerEditor != null)
            {
                if (m_terrainEditor.Terrain != null)
                {
                    m_terrainLayerEditor.Terrain = m_terrainEditor.Terrain;
                }
                m_terrainLayerEditor.SelectedLayerChanged += OnSelectedLayerChanged;
            }

            if (m_terrainEditor.Projector != null)
            {
                m_terrainEditor.Projector.gameObject.SetActive(GetTerrainLayerIndex() >= 0);
            }
            
            if (m_terrainBrushEditor.SelectedBrush != null)
            {
                OnSelectedBrushChanged(this, EventArgs.Empty);
            }
            OnBrushParamsChanged(this, EventArgs.Empty);
        }

        private void OnDisable()
        {
            if (m_terrainLayerEditor != null)
            {
                m_terrainLayerEditor.SelectedLayerChanged -= OnSelectedLayerChanged;
            }

            //m_terrainEditor.Projector.gameObject.SetActive(false);
        }

        private void OnSelectedLayerChanged(object sender, EventArgs e)
        {
            InitializeTerrainTextureBrush();
            TerrainTextureBrush brush = (TerrainTextureBrush)m_terrainEditor.Projector.TerrainBrush;
            brush.TerrainLayerIndex = GetTerrainLayerIndex();
            m_terrainEditor.Projector.gameObject.SetActive(brush.TerrainLayerIndex >= 0);
        }

        private void OnSelectedBrushChanged(object sender, EventArgs e)
        {
            InitializeTerrainTextureBrush();
            m_terrainEditor.Projector.Brush = m_terrainBrushEditor.SelectedBrush.texture;
        }

        private void OnBrushParamsChanged(object sender, EventArgs e)
        {
            InitializeTerrainTextureBrush();
            m_terrainEditor.Projector.Size = m_terrainBrushEditor.BrushSize;
            m_terrainEditor.Projector.Opacity = m_terrainBrushEditor.BrushOpacity;
        }

        private void InitializeTerrainTextureBrush()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (m_terrainEditor.Projector.TerrainBrush is TerrainTextureBrush)
            {
                return;
            }

            m_terrainEditor.Projector.TerrainBrush = new TerrainTextureBrush()
            {
                TerrainLayerIndex = GetTerrainLayerIndex()
            };
            m_terrainEditor.Projector.gameObject.SetActive(GetTerrainLayerIndex() >= 0);
        }

        private int GetTerrainLayerIndex()
        {
            return m_terrainLayerEditor.SelectedLayer != null ? Array.IndexOf(m_terrainLayerEditor.Terrain.terrainData.terrainLayers, m_terrainLayerEditor.SelectedLayer) : -1;
        }

        private void OnTerrainChanged()
        {
            if (m_terrainLayerEditor != null)
            {
                if (m_terrainEditor.Terrain == null || m_terrainEditor.Terrain.terrainData == null)
                {
                    m_terrainLayerEditor.Terrain = null;
                }
                else
                {
                    m_terrainLayerEditor.Terrain = m_terrainEditor.Terrain;
                }
            }
        }
    }
}
