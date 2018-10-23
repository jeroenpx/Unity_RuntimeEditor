using UnityEngine;
using Battlehub.RTCommon;
namespace Battlehub.RTHandles
{
    [DefaultExecutionOrder(-80)]
    public class BaseHandleInput : MonoBehaviour
    {
        [SerializeField]
        protected BaseHandle m_handle;
        protected IRTE m_editor;

        private void Start()
        {
            if (m_handle == null)
            {
                m_handle = GetComponent<BaseHandle>();
            }
            m_editor = m_handle.Editor;
        }

        protected virtual void Update()
        {
            if (BeginDragAction())
            {
                if (m_editor.Tools.Current != m_handle.Tool && m_editor.Tools.Current != RuntimeTool.None || m_editor.Tools.IsViewing)
                {
                    return;
                }

                if (!m_handle.IsInActiveWindow)
                {
                    return;
                }

                if (m_editor.Tools.ActiveTool != null)
                {
                    return;
                }

                if (m_handle.ActiveWindow.Camera != null && !m_handle.ActiveWindow.IsPointerOver)
                {
                    return;
                }

                m_handle.BeginDrag();

            }
            else if (EndDragAction())
            {
                m_handle.EndDrag();
            }

            if(m_handle.IsDragging)
            {
                m_handle.UnitSnapping = UnitSnappingAction();
            }
        }

        protected virtual bool BeginDragAction()
        {
            return m_editor.Input.GetPointerDown(0);
        }

        protected virtual bool EndDragAction()
        {
            return m_editor.Input.GetPointerUp(0);
        }

        protected virtual bool UnitSnappingAction()
        {
            return
#if UNITY_EDITOR
                m_editor.Input.GetKey(KeyCode.LeftShift)
#else
                m_editor.Input.GetKey(KeyCode.LeftControl)
#endif
                || m_editor.Tools.UnitSnapping;
        }

    }

}

