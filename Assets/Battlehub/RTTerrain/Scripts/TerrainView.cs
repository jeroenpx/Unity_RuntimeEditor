using Battlehub.RTCommon;

using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainView : RuntimeWindow
    {
        [SerializeField]
        private TerrainEditor m_terrainEditor = null;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Custom;
            base.AwakeOverride();

            if(m_terrainEditor != null)
            {
                m_terrainEditor.Terrain = Terrain.activeTerrain;
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
        }
    }
}