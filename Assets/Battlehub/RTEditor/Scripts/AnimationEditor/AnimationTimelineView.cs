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
                m_target = value;
                if (m_timeline != null)
                {
                    m_timeline.SetActive(m_target != null);
                }
            }
        }

        private void Start()
        {
            m_dopesheet.RowsCount = 1;
        }

        public void AddProperites(AnimationPropertyItem[] properties)
        {
            m_dopesheet.RowsCount += properties.Length;
        }

        public void RemoveProperties(AnimationPropertyItem[] properties)
        {
            m_dopesheet.RowsCount -= properties.Length;
        }

    }

}

