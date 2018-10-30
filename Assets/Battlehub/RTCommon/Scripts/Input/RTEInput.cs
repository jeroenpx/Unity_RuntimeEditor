using UnityEngine;

namespace Battlehub.RTCommon
{
    public class RTEInput : MonoBehaviour
    {
        [SerializeField]
        protected KeyCode OpenEditorKey = KeyCode.F12;
        [SerializeField]
        protected KeyCode PlayKey = KeyCode.F5;

        protected IRTE m_editor;

        protected bool OpenEditorAction()
        {
            return m_editor.Input.GetKeyDown(OpenEditorKey);
        }

        protected bool PlayAction()
        {
            return m_editor.Input.GetKeyDown(PlayKey);
        }
       
        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
        }

        private void Update()
        {
            if(OpenEditorAction())
            {
                m_editor.IsOpened = !m_editor.IsOpened;
            }

            if(m_editor.IsOpened)
            {
                if(PlayAction())
                {
                    m_editor.IsPlaying = !m_editor.IsPlaying;
                }
            }
        }
    }
}
