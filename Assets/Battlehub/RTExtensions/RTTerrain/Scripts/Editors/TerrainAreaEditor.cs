using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaEditor : MonoBehaviour
    {
        private ITerrainAreaTool m_tool;

        private void Awake()
        {
            m_tool = IOC.Resolve<ITerrainAreaTool>();
        }

        private void OnDestroy()
        {
            m_tool = null;
        }

        private void OnEnable()
        {
            m_tool.IsActive = true;   
        }

        private void OnDisable()
        {
            m_tool.IsActive = false;
        }

    }
}
