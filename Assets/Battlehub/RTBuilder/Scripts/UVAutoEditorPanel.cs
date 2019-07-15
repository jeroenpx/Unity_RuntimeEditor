using UnityEngine;
using Battlehub.RTEditor;
using Battlehub.Utils;
using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using UnityEngine.UI;

namespace Battlehub.RTBuilder
{
    public class UVAutoEditorPanel : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_useGizmosToggle = null;

        [SerializeField]
        private EnumEditor m_fillModeEditor = null;

        [SerializeField]
        private EnumEditor m_anchorEditor = null;

        [SerializeField]
        private Vector2Editor m_offsetEditor = null;

        [SerializeField]
        private RangeEditor m_rotationEditor = null;

        [SerializeField]
        private Vector2Editor m_tilingEditor = null;

        [SerializeField]
        private BoolEditor m_worldSpaceEditor = null;

        [SerializeField]
        private BoolEditor m_flipUEditor = null;

        [SerializeField]
        private BoolEditor m_flipVEditor = null;

        [SerializeField]
        private BoolEditor m_swapUVEditor = null;

        private IProBuilderTool m_tool;
        private IRTE m_editor;
   
        private void Awake()
        {
            m_tool = IOC.Resolve<IProBuilderTool>();
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_editor.Undo.UndoCompleted += OnUpdateVisualState;
            m_editor.Undo.RedoCompleted += OnUpdateVisualState;
            m_editor.Undo.StateChanged += OnUpdateVisualState;
            
            if(m_useGizmosToggle != null)
            {
                m_useGizmosToggle.onValueChanged.AddListener(OnUseGizmosValueChanged);
            }

            if(m_fillModeEditor != null)
            {
                m_fillModeEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.fill), null, "Fill");
            }

            if (m_anchorEditor != null)
            {
                m_anchorEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.anchor), null, "Anchor");
            }

            if (m_offsetEditor != null)
            {
                m_offsetEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.offset), null, "Offset");
            }

            if(m_rotationEditor != null)
            {
                m_rotationEditor.Min = 0;
                m_rotationEditor.Max = 360;
                m_rotationEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.rotation), null, "Rotation");
            }

            if(m_tilingEditor != null)
            {
                m_tilingEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.scale), null, "Tiling");
            }

            if(m_worldSpaceEditor != null)
            {
                m_worldSpaceEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.useWorldSpace), null, "World Space");
            }

            if(m_flipUEditor != null)
            {
                m_flipUEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.flipU), null, "Flip U");
            }

            if(m_flipVEditor != null)
            {
                m_flipVEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.flipV), null, "Flip V");
            }

            if(m_swapUVEditor != null)
            {
                m_swapUVEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.swapUV), null, "Swap UV");
            }
        }

        private void OnDestroy()
        {
            if(m_useGizmosToggle != null)
            {
                m_useGizmosToggle.isOn = false;
                m_useGizmosToggle.onValueChanged.RemoveListener(OnUseGizmosValueChanged);
            }

            if(m_editor != null)
            {
                m_editor.Undo.UndoCompleted -= OnUpdateVisualState;
                m_editor.Undo.RedoCompleted -= OnUpdateVisualState;
                m_editor.Undo.StateChanged -= OnUpdateVisualState;
            }
        }

        private void OnUseGizmosValueChanged(bool value)
        {
            m_tool.UVEditingMode = value;
        }

        private void OnUpdateVisualState()
        {
            m_useGizmosToggle.isOn = m_tool.UVEditingMode;
        }
    }
}

