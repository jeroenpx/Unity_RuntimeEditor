using UnityEngine;
using Battlehub.RTCommon;
namespace Battlehub.RTGizmos
{
    [DefaultExecutionOrder(-61)]
    public class BaseGizmoInput : MonoBehaviour
    {
        [SerializeField]
        protected BaseGizmo m_gizmo;
        protected IRTE m_editor;

        private void Awake()
        {
            if (m_gizmo == null)
            {
                m_gizmo = GetComponent<BaseGizmo>();
            }
            m_editor = m_gizmo.Editor;
        }

        private void OnEnable()
        {
            if (BeginDragAction())
            {
                m_gizmo.BeginDrag();
            }
        }

        protected virtual void Update()
        {
            if (BeginDragAction())
            {
                m_gizmo.BeginDrag();
            }
            else if (EndDragAction())
            {
                m_gizmo.EndDrag();
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
    }
}