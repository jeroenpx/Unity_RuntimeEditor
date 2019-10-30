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
        private ITimelineControl Dopesheet
        {
            get { return m_dopesheet; }
        }


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

                Dopesheet.VisibleRowsCount = 1;
                m_target = value;
                if (m_timeline != null)
                {
                    m_timeline.SetActive(m_target != null);
                }
            }
        }

        public bool IsPlaying
        {
            get { return m_dopesheet.IsPlaying; }
            set { m_dopesheet.IsPlaying = value; }
        }

        private void Start()
        {
            Dopesheet.VisibleRowsCount = 1;
        }

        public void AddProperties(int[] rows, AnimationPropertyItem[] properties)
        {
            int parentIndex = -1;
            for(int i = 0; i < properties.Length; ++i)
            {
                AnimationPropertyItem property = properties[i];

                if(property.ComponentType == AnimationPropertyItem.k_SpecialEmptySpace)
                {
                    Dopesheet.AddRow(true, -1);
                }
                else
                {
                    bool isParent = property.Children != null && property.Children.Count > 0;
                    if(isParent)
                    {
                        parentIndex = rows[i];
                        Dopesheet.AddRow(true, 0);
                    }
                    else
                    {
                        Dopesheet.AddRow(false, parentIndex);
                    }
                }
            }
        }

        public void RemoveProperties(int[] rows, AnimationPropertyItem[] properties)
        {
            for (int i = properties.Length - 1; i >= 0; --i)
            {
                Dopesheet.RemoveKeyframes(rows[i]);
            }

            for (int i = properties.Length - 1; i >= 0; --i)
            {
                Dopesheet.RemoveRow(rows[i]);
            }

            Dopesheet.Refresh();
        }
              
        public void ExpandProperty(int row, AnimationPropertyItem property)
        {
            Dopesheet.Expand(row, property.Children.Count);
        }

        public void CollapseProperty(int row, AnimationPropertyItem property)
        {
            Dopesheet.Collapse(row, property.Children.Count);
        }

        public void NextSample()
        {
            Dopesheet.NextSample();
        }

        public void PrevSample()
        {
            Dopesheet.PrevSample();
        }

        public void LastSample()
        {
            Dopesheet.LastSample();
        }

        public void FirstSample()
        {
            Dopesheet.FirstSample();
        }
    }

}

