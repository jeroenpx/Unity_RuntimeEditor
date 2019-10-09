using UnityEngine;

namespace Battlehub.RTEditor
{
    public class AnimationTimelineView : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_timeline;

        [SerializeField]
        private TimelineControl m_dopesheet;

        [SerializeField]
        private TimelineControl m_curves;

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

    }

}

