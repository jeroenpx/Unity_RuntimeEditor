using UnityEngine;

namespace Battlehub.RTEditor
{
    public class AnimationTimelineView : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_timeline = null;

        [SerializeField]
        private TimelineControl m_dopesheet = null;

        [SerializeField]
        private TimelineControl m_curves = null;

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

