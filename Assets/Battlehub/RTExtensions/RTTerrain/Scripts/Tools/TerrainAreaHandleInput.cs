using Battlehub.RTEditor;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaHandleInput : BaseHandleInput
    {
        [SerializeField]
        private KeyCode m_focusKey = KeyCode.F;
        private TerrainAreaHandle TerrainAreaHandle
        {
            get { return (TerrainAreaHandle)m_handle; }
        }

        protected override void Update()
        {
            base.Update();

            if (m_editor.Tools.IsViewing)
            {
                return;
            }

            if (!m_handle.IsWindowActive || !m_handle.Window.IsPointerOver)
            {
                return;
            }

            if (ChangePositionAction() && !m_handle.IsDragging && m_handle.SelectedAxis == RuntimeHandleAxis.None)
            {
                TerrainAreaHandle.ChangePosition();
            }

            if (FocusAction() && m_handle != null && m_handle.IsWindowActive)
            {
                IScenePivot pivot = m_editor.GetScenePivot();
                pivot.Focus(m_handle.Position, m_handle.Appearance.HandleScale);
            }
        }

        protected virtual bool ChangePositionAction()
        {
            return m_editor.Input.GetPointerDown(0);
        }

        protected virtual bool FocusAction()
        {
            return m_editor.Input.GetKeyDown(m_focusKey);
        }
    }
}

