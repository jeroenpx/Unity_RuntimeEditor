using UnityEngine;

namespace Battlehub.RTEditor
{
    public class AnimationTimelineView : MonoBehaviour
    {
#pragma warning disable 0414
        [SerializeField]
        private GameObject m_timeline = null;

        [SerializeField]
        private TimelineControl m_dopesheet = null;

        [SerializeField]
        private TimelineControl m_curves = null;
#pragma warning restore 0219
        public bool IsDopesheet
        {
            get;
            set;
        }


        private RuntimeAnimation m_target;
        public RuntimeAnimation Target
        {
            get { return m_target; }
            set
            {
                m_dopesheet.VisibleRowsCount = 1;
                m_target = value;
                if (m_timeline != null)
                {
                    m_timeline.SetActive(m_target != null);
                }
            }
        }

        private void Start()
        {
            m_dopesheet.VisibleRowsCount = 1;
        }

        public void AddProperties(AnimationPropertyItem[] properties)
        {
            for(int i = 0; i < properties.Length; ++i)
            {
                AnimationPropertyItem property = properties[i];

                bool isVisible = property.ComponentType == AnimationPropertyItem.k_SpecialEmptySpace || property.Children != null && property.Children.Count > 0;
                m_dopesheet.Clip.AddRow(isVisible);
                if (isVisible)
                {
                    m_dopesheet.VisibleRowsCount++;
                }
            }
            
        }

        public void RemoveProperties(int[] rows, AnimationPropertyItem[] properties)
        {
            for (int i = properties.Length - 1; i >= 0; --i)
            {
                int rowIndex = rows[i];
                Dopesheet.DopesheetRow row = m_dopesheet.Clip.Rows[rowIndex];
                m_dopesheet.Clip.RemoveKeyframes(true, row.Keyframes.ToArray());
                m_dopesheet.Clip.RemoveKeyframes(true, row.SelectedKeyframes.ToArray());
            }

            for (int i = properties.Length - 1; i >= 0; --i)
            {
                int rowIndex = rows[i];
                bool isVisible = m_dopesheet.Clip.RemoveRow(rowIndex);
                if(isVisible)
                {
                    m_dopesheet.VisibleRowsCount--;
                }
            }

            m_dopesheet.Clip.UpdateDictionaries();
        }
              

        public void ExpandProperty(int row, AnimationPropertyItem property)
        {
            m_dopesheet.VisibleRowsCount += property.Children.Count;
            m_dopesheet.Clip.Expand(row, property.Children.Count);
        }

        public void CollapseProperty(int row, AnimationPropertyItem property)
        {
            m_dopesheet.VisibleRowsCount -= property.Children.Count;
            m_dopesheet.Clip.Collapse(row, property.Children.Count);
        }

    }

}

