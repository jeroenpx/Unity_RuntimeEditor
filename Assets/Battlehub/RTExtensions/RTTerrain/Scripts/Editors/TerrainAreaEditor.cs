using Battlehub.RTCommon;
using System;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaEditor : MonoBehaviour
    {
        private ITerrainAreaTool m_tool;

        [SerializeField]
        private TerrainBrushEditor m_terrainBrushEditor;

        private void Awake()
        {
            m_terrainBrushEditor.SelectedBrushChanged += OnSelectedBrushChanged;
            m_tool = IOC.Resolve<ITerrainAreaTool>();
            //m_tool.Brush = m_terrainBrushEditor.SelectedBrush.texture;
        }

        private void OnDestroy()
        {
            m_tool = null;

            if(m_terrainBrushEditor != null)
            {
                m_terrainBrushEditor.SelectedBrushChanged += OnSelectedBrushChanged;
            }
        }

        private void OnEnable()
        {
            m_tool.IsActive = true;   
        }

        private void OnDisable()
        {
            m_tool.IsActive = false;
        }

        private void OnSelectedBrushChanged(object sender, EventArgs e)
        {
            m_tool.Brush = m_terrainBrushEditor.SelectedBrush.texture;
        }


    }
}
