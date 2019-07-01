using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class UVEditorView : RuntimeWindow
    {
        private IProBuilderTool m_tool;
        
        [SerializeField]
        private GameObject m_uvAutoEditorPanel = null;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Custom;
            base.AwakeOverride();

            m_tool = IOC.Resolve<IProBuilderTool>();
            
            OnToolSelectionChanged();
            m_tool.SelectionChanged += OnToolSelectionChanged;
            Editor.Selection.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(m_tool != null)
            {
                m_tool.SelectionChanged -= OnToolSelectionChanged;
            }

            if(Editor != null)
            {
                Editor.Selection.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void OnToolSelectionChanged()
        {
            UpdateVisualState();
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (!m_tool.HasSelectedFaces)
            {
                if (m_uvAutoEditorPanel != null)
                {
                    m_uvAutoEditorPanel.gameObject.SetActive(false);
                }
            }
            else
            {
                if (m_uvAutoEditorPanel != null)
                {
                    m_uvAutoEditorPanel.gameObject.SetActive(true);
                }
            }
        }
    }
}

