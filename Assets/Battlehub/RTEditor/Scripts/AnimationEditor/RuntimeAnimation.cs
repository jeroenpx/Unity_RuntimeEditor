using UnityEngine;

namespace Battlehub.RTEditor
{
    public class RuntimeAnimationClip : ScriptableObject
    {
        
    }

    public class RuntimeAnimation : MonoBehaviour
    {
        private Animation m_animation;
        public RuntimeAnimationClip[] Clips
        {
            get;
            set;
        }
    }
}

