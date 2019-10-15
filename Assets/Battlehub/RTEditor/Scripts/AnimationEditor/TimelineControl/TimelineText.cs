using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class TimelineText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_text;

        private RectTransform m_rt;
        
        public string Text
        {
            set { m_text.text = value; }
        }

        private bool m_isSecondary;
        public bool IsSecondary
        {
            set { m_isSecondary = value; }
        }

        private bool m_isPrimary;
        public bool IsPrimary
        {
            set { m_isPrimary = value; }
        }

        public bool IsVisible
        {
            set
            {
                m_text.alpha = value ? 1 : 0;
            }
        }

        private void Awake()
        {
            if(m_text == null)
            {
                m_text = GetComponent<TextMeshProUGUI>();
            }

            m_rt = GetComponent<RectTransform>();
        }
            
        public void Refresh(float primarySpace, float secondarySpace)
        {
            m_text.ForceMeshUpdate();

            if (m_isSecondary)
            {
                if (secondarySpace < m_text.bounds.size.x + 10)
                {
                    m_text.alpha = 0;
                }
                else
                {
                    m_text.alpha = 1;
                }
            }
            else if(m_isPrimary)
            {
                if (primarySpace < m_text.bounds.size.x + 10)
                {
                    m_text.alpha = 0;
                }
                else
                {
                    m_text.alpha = 1;
                }
            }
        }
    }

}
