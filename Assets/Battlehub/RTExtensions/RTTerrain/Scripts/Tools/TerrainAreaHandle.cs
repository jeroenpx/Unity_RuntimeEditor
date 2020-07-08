using Battlehub.RTCommon;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaHandle : BaseHandle
    {
        protected override bool OnBeginDrag()
        {
            Debug.Log("OnBeginDrag");
            return true;
        }

        protected override void OnDrag()
        {
            if (!Window.IsPointerOver)
            {
                return;
            }

            Debug.Log("OnDrag");
        }

        protected override void OnDrop()
        {
            base.OnDrop();
            Debug.Log("OnDrop");
        }
    }

}
