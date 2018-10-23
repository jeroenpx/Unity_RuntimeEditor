using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class RuntimeSelectionInputBase : MonoBehaviour
    {
        protected RuntimeSelectionComponent m_component;
        
        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {
            m_component = GetComponent<RuntimeSelectionComponent>();
        }

        protected virtual void OnDestroy()
        {

        }

        protected virtual void LateUpdate()
        {
            if (!m_component.IsInActiveWindow || !m_component.ActiveWindow.IsPointerOver)
            {
                return;
            }

            if (!m_component.IsUISelected && !m_component.Editor.IsVR)
            {
                return;
            }

            if (SelectAction())
            {
                SelectGO();
            }
        }

        protected virtual bool SelectAction()
        {
            return m_component.Editor.Input.GetPointerDown(0);
        }

        protected virtual void SelectGO()
        {
            RuntimeTools tools = m_component.Editor.Tools;
            IRuntimeSelection selection = m_component.Editor.Selection;
            IInput input = m_component.Editor.Input;

            if (tools.ActiveTool != null && tools.ActiveTool != m_component.BoxSelection)
            {
                return;
            }

            if (tools.IsViewing)
            {
                return;
            }

            if (!selection.Enabled)
            {
                return;
            }

            OnSelectGO();
        }

        protected virtual void OnSelectGO()
        {
            m_component.SelectGO(false, false);
        }
    }

}
