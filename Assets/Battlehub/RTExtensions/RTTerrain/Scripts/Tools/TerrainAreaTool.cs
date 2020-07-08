using Battlehub.RTCommon;
using Battlehub.RTEditor.Demo;

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

        protected override void SetCurrentTool(RuntimeTool tool)
        {
            
        }
    }
}
