using UnityEngine;

namespace Battlehub.RTEditor
{
    public class AnimationTimelineView : MonoBehaviour
    {
        [SerializeField]
        private TimelineControl m_dopesheet;

        [SerializeField]
        private TimelineControl m_curves;

        public bool IsDopesheet
        {
            get;
            set;
        }

    }

}

