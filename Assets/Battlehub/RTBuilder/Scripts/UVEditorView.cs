using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class UVEditorView : RuntimeWindow
    {
        private IProBuilderTool m_tool;
        private IWindowManager m_wm;
        
        [SerializeField]
        private GameObject m_uvAutoEditorPanel = null;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Custom;
            base.AwakeOverride();

            m_tool = IOC.Resolve<IProBuilderTool>();

            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.WindowCreated += OnWindowCreated;
            
            OnToolSelectionChanged();
            m_tool.SelectionChanged += OnToolSelectionChanged;
            Editor.Selection.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
            }

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

        private void OnWindowCreated(Transform obj)
        {
            if(obj == m_wm.GetWindow("ProBuilder"))
            {
                if (m_tool != null)
                {
                    m_tool.SelectionChanged -= OnToolSelectionChanged;
                }
                m_tool = IOC.Resolve<IProBuilderTool>();
                OnToolSelectionChanged();
                m_tool.SelectionChanged += OnToolSelectionChanged;
            }
        }
    }
}

