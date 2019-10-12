using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class TimelineText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_text;
        
        private int m_samplesCount;
        public int SamplesCount
        {
            set { m_samplesCount = value; }
        }

        private bool m_isSecondary;
        public bool IsSecondary
        {
            set { m_isSecondary = value; }
        }

        private void Awake()
        {
            if(m_text == null)
            {
                m_text = GetComponent<TextMeshProUGUI>();
            }
        }
            
        public void Refresh()
        {
            RectTransform rt = (RectTransform)transform;
            if(m_isSecondary)
            {
                if (rt.rect.width < m_text.bounds.size.x + 10)
                {
                    m_text.alpha = 0;
                }
                else
                {
                    m_text.alpha = 1;
                }
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            
        }

    }

}
