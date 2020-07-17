using Battlehub.RTEditor;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaHandleInput : BaseHandleInput
    {
        [SerializeField]
        private KeyCode m_focusKey = KeyCode.F;

        [SerializeField]
        private KeyCode m_absoluteHeightKey = KeyCode.LeftShift;

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

            TerrainAreaHandle.AbsoluteHeightMode = AbsoluteHeightAction();

            if (ChangePositionAction() && !m_handle.IsDragging && m_handle.SelectedAxis == RuntimeHandleAxis.None)
            {
                TerrainAreaHandle.ChangePosition();
            }

            if (FocusAction() && m_handle != null && m_handle.IsWindowActive)
            {
                IScenePivot pivot = m_editor.GetScenePivot();
                Vector3[] areaResizerPositions = TerrainAreaHandle.AreaResizerPositions;
                pivot.Focus(m_handle.Position, (areaResizerPositions[1] - areaResizerPositions[0]).magnitude);
            }

        }

        protected virtual bool ChangePositionAction()
        {
            return m_editor.Input.GetPointerDown(0);
        }

        protected virtual bool AbsoluteHeightAction()
        {
            return m_editor.Input.GetKey(m_absoluteHeightKey);
        }

        protected virtual bool FocusAction()
        {
            return m_editor.Input.GetKeyDown(m_focusKey);
        }
    }
}

