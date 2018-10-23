using UnityEngine;

namespace Battlehub.RTCommon
{
    [DefaultExecutionOrder(-75)]
    public class RTEBehaviour : MonoBehaviour
    {
        [SerializeField]
        private RuntimeWindowType m_windowMask = RuntimeWindowType.SceneView;
        public RuntimeWindowType WindowMask
        {
            get { return m_windowMask; }
        }

        protected bool IsSupported(RuntimeWindow window)
        {
            if(window == null)
            {
                return false;
            }

            if(m_requiresCamera && window.Camera == null)
            {
                return false;
            }

            int mask = (int)m_windowMask;
            int windowType = (int)window.WindowType;
            return (windowType & mask) != 0;
        }

        [SerializeField]
        private bool m_requiresCamera = true;

        private RuntimeWindow m_activeWindow;
        public RuntimeWindow ActiveWindow
        {
            get { return m_activeWindow; }
        }

        public bool IsInActiveWindow
        {
            get { return ActiveWindow != null; }
        }

        private IRTE m_editor;
        public IRTE Editor
        {
            get { return m_editor; }
        }

        private void Awake()
        {
            m_editor = RTE.Get;

            AwakeOverride();

            RuntimeWindow[] windows = m_editor.Windows;
            for (int i = 0; i < windows.Length; ++i)
            {
                OnWindowRegistered(windows[i]);
            }

            m_activeWindow = m_editor.ActiveWindow;
            if (m_activeWindow != null)
            {
                OnWindowActivated();
            }
            m_editor.WindowRegistered += OnWindowRegistered;
            m_editor.WindowUnregistered += OnWindowUnregistered;
            m_editor.ActiveWindowChanged += OnActiveWindowChanged;
        }

        protected virtual void AwakeOverride()
        {

        }

        private void OnDestroy()
        {
            if(m_editor != null)
            {
                m_editor.WindowRegistered -= OnWindowRegistered;
                m_editor.WindowUnregistered -= OnWindowUnregistered;
                m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
            }
            OnDestroyOverride();
            m_editor = null;
        }

        protected virtual void OnDestroyOverride()
        {

        }

        protected virtual void OnActiveWindowChanged()
        {
            RuntimeWindow oldActiveWindow = m_activeWindow;

            if (IsSupported(m_editor.ActiveWindow))
            {
                m_activeWindow = m_editor.ActiveWindow;
            }
            else
            {
                m_activeWindow = null;
            }

            if(oldActiveWindow != m_activeWindow)
            {
                if(m_activeWindow != null)
                {
                    OnWindowActivated();
                }
                else
                {
                    OnWindowDeactivated();
                }
            }
        }

        protected virtual void OnWindowActivated()
        {

        }
        
        protected virtual void OnWindowDeactivated()
        {

        }

        protected virtual void OnWindowUnregistered(RuntimeWindow window)
        {
            
        }

        protected virtual void OnWindowRegistered(RuntimeWindow window)
        {
            
        }       
    }
}

