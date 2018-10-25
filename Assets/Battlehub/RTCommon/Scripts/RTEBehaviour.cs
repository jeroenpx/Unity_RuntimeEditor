using UnityEngine;

namespace Battlehub.RTCommon
{
    public class RTEBehaviour : MonoBehaviour
    {
        private IRTE m_editor;
        public IRTE Editor
        {
            get { return m_editor; }
        }

        [SerializeField]
        private RuntimeWindow m_window;

        public virtual RuntimeWindow Window
        {
            get { return m_window; }
            set
            {
                if(m_awaked)
                {
                    throw new System.NotSupportedException("window change is not supported");
                }
                m_editor = RTE.Get;
                m_window = value;
            }
        }

        public bool IsWindowActive
        {
            get { return Window == m_editor.ActiveWindow; }
        }

        private bool m_awaked;
        private void Awake()
        {
            
            m_editor = RTE.Get;

            if(Window == null)
            {
                Window = m_editor.GetWindow(RuntimeWindowType.SceneView);
                if(Window == null)
                {
                    Debug.LogError("m_window == null");
                    enabled = false;
                    return;
                }
            }

            m_awaked = true;

            AwakeOverride();

            if(IsWindowActive)
            {
                OnWindowActivated();
            }
            m_editor.ActiveWindowChanged += OnActiveWindowChanged;
        }

        protected virtual void AwakeOverride()
        {

        }

        private void OnDestroy()
        {
            if(m_editor != null)
            {
                m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
            }
            OnDestroyOverride();
            m_editor = null;
        }

        protected virtual void OnDestroyOverride()
        {

        }

        private void OnActiveWindowChanged()
        {
            if (m_editor.ActiveWindow == Window)
            {
                OnWindowActivated();
            }
            else
            {
                OnWindowDeactivated();
            }
        }

        protected virtual void OnWindowActivated()
        {
            
        }

        protected virtual void OnWindowDeactivated()
        {

        }
    }
}

