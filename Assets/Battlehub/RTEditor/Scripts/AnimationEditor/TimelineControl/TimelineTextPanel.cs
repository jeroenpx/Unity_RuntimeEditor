using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class TimelineTextPanel : MonoBehaviour
    {
        [SerializeField]
        public RectTransform m_textRoot = null;

        [SerializeField]
        private TimelineText m_textPrefab = null;

        private List<TimelineText> m_textList = new List<TimelineText>();

        private int m_linesCount;
        private int m_secondaryLinesCount;
        private int m_samples;

        public void SetParameters(int linesCount, int secondaryLinesCount, int samples)
        {
            m_linesCount = linesCount;
            m_secondaryLinesCount = secondaryLinesCount;
            m_samples = samples;

            int sqSecondaryLinesCount = m_secondaryLinesCount * m_secondaryLinesCount;
            int totalLinesCount = m_linesCount * m_secondaryLinesCount;
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

                bool isSecondary = (i % m_secondaryLinesCount) != 0;
                bool isPrimary = !isSecondary && (i % sqSecondaryLinesCount) != 0;

                text.IsSecondary = isSecondary;
                text.IsPrimary = isPrimary;
            }
        }

        public void UpdateGraphics(float viewportSize, float contentSize, float scrollOffset, float scrollSize, float interval)
        {
            float intervalMaxValue = m_samples;

            float size = contentSize;
            size /= interval;

            float kLines = TimelineGrid.k_Lines;
            float scaleFactor = Mathf.Pow(kLines, Mathf.Ceil(Mathf.Log(interval * scrollSize, kLines)));
            size *= scaleFactor;

            m_textRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_textRoot);

            Vector2 position = m_textRoot.anchoredPosition;
            float mod = (((contentSize / m_linesCount) / interval) * scaleFactor);
            position.x = -((contentSize - viewportSize) * scrollOffset) % mod;
            m_textRoot.anchoredPosition = position;

            float seconarySpace = size / m_textList.Count;
            float primarySpace = seconarySpace * m_secondaryLinesCount;

           


            float s = Mathf.Log(interval, kLines);
            float sc = Mathf.Ceil(s);
            float sc1 = Mathf.Pow(kLines, Mathf.Ceil(Mathf.Log(interval * scrollSize, kLines)) - 1);
            Debug.Log("SC1 : " + sc1 + " Log: " + (Mathf.Log(interval, kLines) - 1));



            //float tBegin = s * (1 - scrollSize) * scrollOffset;
            //float tEnd = tBegin + s * scrollSize;

            //Debug.LogFormat("TBegin {0}, TEnd {1}", tBegin, tEnd);

            for (int i = 0; i < m_textList.Count; ++i)
            {
                TimelineText text = m_textList[i];

                // int intervalNumber = (int)(((float)i / m_textList.Count) * Mathf.Log(interval, kLines));
                int offset = 0;// Mathf.FloorToInt((m_linesCount * scaleFactor)  * scrollOffset);
                int intervalNumber = Mathf.RoundToInt((1 / intervalMaxValue) * i * sc1 * 60) / 60;
                int timeValue = offset + Mathf.RoundToInt((1 / intervalMaxValue) * i * sc1 * 60) % 60;


                text.Text = string.Format("{0:D2}:{1:D2}", intervalNumber, timeValue);

                
                text.Refresh(primarySpace, seconarySpace);
            }
        }
    }

}
