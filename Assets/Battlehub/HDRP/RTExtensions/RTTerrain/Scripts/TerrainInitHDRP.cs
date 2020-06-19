using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine;

namespace Battlehub.RTTerrain.HDRP
{
    public class TerrainInitHDRP : EditorExtension
    {
        [SerializeField]
        private TerrainProjectorBase m_projector = null;

        private TerrainProjectorBase InstantiateProjector()
        {
            return Instantiate(m_projector);
        }

        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            IOC.Register(InstantiateProjector);
        }

        protected override void OnEditorOpened()
        {
            base.OnEditorOpened();
            IOC.Unregister(InstantiateProjector);
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            IOC.Unregister(InstantiateProjector);
        }
    }
}

