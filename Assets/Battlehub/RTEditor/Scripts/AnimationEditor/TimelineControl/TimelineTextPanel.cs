using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class TimelineTextPanel : MonoBehaviour
    {
        [SerializeField]
        public RectTransform m_textRoot;

        [SerializeField]
        private TimelineText m_textPrefab;

        private List<TimelineText> m_textList = new List<TimelineText>();

        private int m_linesCount;

        public void SetParameters(int linesCount, int secondaryLinesCount)
        {
            int totalLinesCount = linesCount * secondaryLinesCount;
            int delta = totalLinesCount - m_textRoot.childCount;
            if(delta > 0)
            {
                for(int i = 0; i < delta; ++i)
                {
                    TimelineText text = Instantiate(m_textPrefab, m_textRoot);
                    m_textList.Add(text);
                }
            }
            else
            {
                int lastChildIndex = m_textRoot.childCount - 1;
                for (int i = lastChildIndex; i >= lastChildIndex - delta; i--)
                {
                    Transform child = m_textRoot.GetChild(i);
                    m_textList.Remove(child.GetComponent<TimelineText>());
                    Destroy(child.gameObject);   
                }
            }

            for (int i = 0; i < m_textList.Count; ++i)
            {
                TimelineText text = m_textList[i];
                text.IsSecondary = (i % secondaryLinesCount) != 0;
                text.Refresh();
            }
        }

        public void UpdateGraphics(float viewportSize, float contentSize, float scrollOffset, float scrollSize, float interval)
        {
            
           
            contentSize /= interval;
            viewportSize /= interval;

            m_textRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentSize);

            Vector2 position = m_textRoot.anchoredPosition;
            position.x = -(contentSize - viewportSize) * scrollOffset;
            m_textRoot.anchoredPosition = position;

            for (int i = 0; i < m_textList.Count; ++i)
            {
                TimelineText text = m_textList[i];
                text.Refresh();
            }
        }
    }

}
