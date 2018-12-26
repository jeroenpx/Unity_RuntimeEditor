using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class OpenEditor : MonoBehaviour
    {
        [SerializeField]
        private Button m_openEditorButton = null;

        [SerializeField]
        private RuntimeEditor m_editorPrefab = null;

        private RuntimeEditor m_editor;

        private void Awake()
        {
            m_editor = FindObjectOfType<RuntimeEditor>();
            if(m_editor != null)
            {
                if(m_editor.IsOpened)
                {
                    m_editor.IsOpenedChanged += OnIsOpenedChanged;
                    gameObject.SetActive(false);
                }
                
            }

            m_openEditorButton.onClick.AddListener(OnOpen);
        }

        private void OnOpen()
        {
            m_editor = Instantiate(m_editorPrefab);
            m_editor.name = "RuntimeEditor";
            m_editor.IsOpenedChanged += OnIsOpenedChanged;
            gameObject.SetActive(false);
        }

        private void OnIsOpenedChanged()
        {
            if(m_editor != null)
            {
                m_editor.IsOpenedChanged -= OnIsOpenedChanged;
            }
            
            if(this != null)
            {
                gameObject.SetActive(true);
            }
        }
    }
}

