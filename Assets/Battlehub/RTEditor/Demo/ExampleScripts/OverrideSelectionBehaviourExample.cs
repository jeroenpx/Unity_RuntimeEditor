using Battlehub.RTCommon;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class OverrideSelectionBehaviourExample : EditorOverride
    {
        private IRTE m_editor;
        private IRuntimeSelectionComponent m_selectionComponent;
        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            m_editor = IOC.Resolve<IRTE>();
            OnActiveWindowChanged(null);
            m_editor.ActiveWindowChanged += OnActiveWindowChanged;
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            if (m_editor != null)
            {
                m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
            }
            if (m_selectionComponent != null)
            {
                m_selectionComponent.SelectionChanging -= OnSelectionChanging;
            }
        }

        private void OnActiveWindowChanged(RuntimeWindow deactivatedWindow)
        {
            if (m_selectionComponent != null)
            {
                m_selectionComponent.SelectionChanging -= OnSelectionChanging;
            }

            if (m_editor.ActiveWindow != null && m_editor.ActiveWindow.WindowType == RuntimeWindowType.Scene)
            {
                m_selectionComponent = m_editor.ActiveWindow.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                m_selectionComponent.SelectionChanging += OnSelectionChanging;
            }
        }

        private void OnSelectionChanging(object sender, RuntimeSelectionChangingArgs e)
        {
            var selected = e.Selected;
            for (int i = selected.Count - 1; i >= 0; i--)
            {
                GameObject go = (GameObject)selected[i];
                ExposeToEditor exposed = go.GetComponent<ExposeToEditor>();
                ExposeToEditor parent = exposed.GetParent();
                if (parent != null)
                {
                    selected.Add(parent.gameObject);
                    selected.RemoveAt(i);
                }
            }
        }
    }

}

