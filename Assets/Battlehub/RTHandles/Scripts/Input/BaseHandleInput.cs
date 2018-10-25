using UnityEngine;
using Battlehub.RTCommon;
namespace Battlehub.RTHandles
{
    [DefaultExecutionOrder(-60)]
    public class BaseHandleInput : MonoBehaviour
    {
        [SerializeField]
        protected BaseHandle m_handle;
        protected IRTE m_editor;

        private void Awake()
        {
            if (m_handle == null)
            {
                m_handle = GetComponent<BaseHandle>();
            }
            m_editor = m_handle.Editor;
        }

        private void OnEnable()
        {
            if (BeginDragAction())
            {
                m_handle.BeginDrag();
            }
        }


        protected virtual void Update()
        {
            if (BeginDragAction())
            {
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

