using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTEditor.Demo;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public interface ITerrainAreaTool
    {
        bool IsActive
        {
            get;
            set;
        }
        
    }

    public class TerrainAreaTool : CustomHandleExtension<TerrainAreaHandle>, ITerrainAreaTool
    {
        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<ITerrainAreaTool>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            IOC.UnregisterFallback<ITerrainAreaTool>(this);
        }

        protected override TerrainAreaHandle CreateHandle(SceneView scene)
        {
            TerrainAreaHandle handle = base.CreateHandle(scene);
            //handle.Projector = m_terrainEditor.Projector;
            return handle;
        }

        protected override void SetCurrentTool(RuntimeTool tool)
        {
            
        }
    }
}
