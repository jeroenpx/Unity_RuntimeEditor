using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.Utils;
using System;
using System.Reflection;
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

        private ITerrainSettings m_terrainSettings;

        protected virtual void Awake()
        {
            m_terrainSettings = IOC.Resolve<ITerrainSettings>();

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
                    m_terrainSettings.InitEditor(m_widthEditor, Strong.PropertyInfo((ITerrainSettings x) => x.Width), "Width");
                }

                if (m_lengthEditor != null)
                {
                    m_terrainSettings.InitEditor(m_lengthEditor, Strong.PropertyInfo((ITerrainSettings x) => x.Length), "Length");
                }

                if(m_heightmapResolutionEditor != null)
                {
                    m_terrainSettings.InitEditor(m_heightmapResolutionEditor, Strong.PropertyInfo((ITerrainSettings x) => x.Resolution), "Resolution");
                }

                if(m_positionEditor != null)
                {
                    m_terrainSettings.InitEditor(m_positionEditor, Strong.PropertyInfo((ITerrainSettings x) => x.Position), "Position");
                }
            }
        }

        

    }
}

