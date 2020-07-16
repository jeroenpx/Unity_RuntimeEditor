using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTEditor.Demo;
using System.Linq;
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

    [DefaultExecutionOrder(1)]
    public class TerrainAreaTool : CustomHandleExtension<TerrainAreaHandle>, ITerrainAreaTool
    {
        private ITerrainAreaProjector m_projector;

        protected override void Activate()
        {
            base.Activate();
            m_projector = IOC.Resolve<ITerrainAreaProjector>();
        }

        protected override void Deactivate()
        {
            base.Deactivate();
            if(m_projector != null)
            {
                m_projector.Destroy();
            }
        }

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
            TerrainAreaHandle exisitingHandle = GetComponentInChildren<TerrainAreaHandle>();
            TerrainAreaHandle handle = base.CreateHandle(scene);

            IRTE rte = IOC.Resolve<IRTE>();
            if(rte.Selection.gameObjects != null)
            {
                handle.Targets = rte.Selection.gameObjects.Select(go => go.transform).ToArray();
            }

            if (exisitingHandle != null)
            {
                handle.Position = exisitingHandle.Position;
            }

            return handle;
        }
        

        protected override void SetCurrentTool(RuntimeTool tool)
        {
            
        }
    }
}
