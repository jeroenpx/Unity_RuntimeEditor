using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class BaseViewInput<T> : MonoBehaviour where T : RuntimeWindow
    {
        public KeyCode RuntimeModifierKey = KeyCode.LeftControl;
        public KeyCode EditorModifierKey = KeyCode.LeftShift;
        public KeyCode ModifierKey
        {
            get
            {
                #if UNITY_EDITOR
                return EditorModifierKey;
                #else
                return RuntimeModifierKey;
                #endif
            }
        }

        private T m_window;
        protected T View
        {
            get { return m_window; }
        }

        private IRTE m_editor;
        protected IRTE Editor
        {
            get { return m_editor; }
        }

        private IInput m_input;
        protected IInput Input
        {
            get { return m_input; }
        }

        private void Start()
        {
            m_window = GetComponent<T>();
            m_editor = m_window.Editor;
            m_input = m_editor.Input;
            StartOverride();
        }

        protected virtual void StartOverride()
        {

        }

        private void Update()
        {
            if (m_window.Editor.ActiveWindow != m_window)
            {
                return;
            }
            UpdateOverride();
        }

        protected virtual void UpdateOverride()
        {

        }
    }
}
