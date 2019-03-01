
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTHandles.Demo
{
    public class DemoEditor : SimpleEditor
    {
        [SerializeField]
        private Button m_focusButton;

        [SerializeField]
        private Button m_play;

        [SerializeField]
        private Button m_stop;

        [SerializeField]
        private GameObject m_components;

        [SerializeField]
        private GameObject m_ui;

        [SerializeField]
        private GameObject m_editorCamera;

        [SerializeField]
        private GameObject m_gameCamera;

        protected override void Start()
        {
            base.Start();
            Editor.IsOpened = true;
            Editor.IsPlaying = false;
            OnPlaymodeStateChanged();
            Editor.PlaymodeStateChanged += OnPlaymodeStateChanged;
            Editor.Selection.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(Editor != null)
            {
                Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;
                Editor.Selection.SelectionChanged -= OnSelectionChanged;
            }
        }

        protected override void SubscribeUIEvents()
        {
            base.SubscribeUIEvents();

            if (m_play != null)
            {
                m_play.onClick.AddListener(OnPlayClick);
            }
            if(m_stop != null)
            {
                m_stop.onClick.AddListener(OnStopClick);
            }
            if(m_focusButton != null)
            {
                m_focusButton.onClick.AddListener(OnFocusClick);
            }  
        }

        protected override void UnsubscribeUIEvents()
        {
            base.UnsubscribeUIEvents();

            if (m_play != null)
            {
                m_play.onClick.RemoveListener(OnPlayClick);
            }
            if (m_stop != null)
            {
                m_stop.onClick.RemoveListener(OnStopClick);
            }
            if (m_focusButton != null)
            {
                m_focusButton.onClick.RemoveListener(OnFocusClick);
            }
        }

        private void OnPlaymodeStateChanged()
        {
            if (m_components != null)
            {
                m_components.SetActive(!Editor.IsPlaying);
            }
            if (m_ui != null)
            {
                m_ui.SetActive(!Editor.IsPlaying);
            }
            if(m_stop != null)
            {
                m_stop.gameObject.SetActive(Editor.IsPlaying);
            }
            if(m_editorCamera != null)
            {
                m_editorCamera.SetActive(!Editor.IsPlaying);
            }
            if(m_gameCamera != null)
            {
                m_gameCamera.SetActive(Editor.IsPlaying);
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            if(m_focusButton != null)
            {
                m_focusButton.interactable = Editor.Selection.Length > 0;
            }
        }

        private void OnFocusClick()
        {
            IScenePivot scenePivot = Editor.ActiveWindow.IOCContainer.Resolve<IScenePivot>();
            scenePivot.Focus();
        }

        private void OnPlayClick()
        {
            Editor.IsPlaying = true;
        }

        private void OnStopClick()
        {
            Editor.IsPlaying = false;
        }
    }

}

