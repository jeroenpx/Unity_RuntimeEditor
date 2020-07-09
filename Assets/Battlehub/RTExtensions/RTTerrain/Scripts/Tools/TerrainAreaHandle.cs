using Battlehub.RTCommon;
using Battlehub.RTEditor;
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


        private DrawingSettings m_drawingSettings = new DrawingSettings
        {
            LockObject = new LockObject { PositionX = true, PositionZ = true },
            DrawLocked = false
        };
        protected override void RefreshCommandBuffer(IRTECamera camera)
        {
            base.RefreshCommandBuffer(camera);
            m_drawingSettings.Position = transform.position;
            m_drawingSettings.Rotation = transform.rotation;
            Appearance.DoPositionHandle(camera.CommandBuffer, camera.Camera, m_drawingSettings);
        }
    }
}
