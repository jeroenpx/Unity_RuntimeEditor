
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class PositionHandleInput : BaseHandleInput
    {
        public KeyCode VertexSnappingKey = KeyCode.V;
        public KeyCode VertexSnappingToggleKey = KeyCode.LeftShift;
        public KeyCode SnapToGroundKey = KeyCode.G;

        protected PositionHandle Handle
        {
            get { return (PositionHandle)m_handle; }
        }

        protected override void Update()
        {
            base.Update();

            if (m_editor.Tools.IsViewing)
            {
                return;
            }

            if(!m_handle.IsWindowActive || !m_handle.Window.IsPointerOver)
            {
                return;
            }

            Handle.SnapToGround = SnapToGroundAction();

            if (BeginVertexSnappingAction())
            {
                Handle.IsInVertexSnappingMode = true;
                if(VertexSnappingToggleAction())
                {
                    m_editor.Tools.IsSnapping = !m_editor.Tools.IsSnapping;
                }
            }
            else if (EndVertexSnappingAction())
            {
                Handle.IsInVertexSnappingMode = false;
            }
        }

        protected virtual bool BeginVertexSnappingAction()
        {
            return m_editor.Input.GetKeyDown(VertexSnappingKey);
        }

        protected virtual bool EndVertexSnappingAction()
        {
            return m_editor.Input.GetKeyUp(VertexSnappingKey);
        }

        protected virtual bool VertexSnappingToggleAction()
        {
            return m_editor.Input.GetKey(VertexSnappingToggleKey);
        }

        protected virtual bool SnapToGroundAction()
        {
            return m_editor.Input.GetKey(SnapToGroundKey);
        }
    }

}

