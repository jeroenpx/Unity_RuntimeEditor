using Battlehub.RTCommon;
using Battlehub.UIControls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AnimationPropertiesView : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_previewToggle;

        [SerializeField]
        private Toggle m_recordToggle;

        [SerializeField]
        private Button m_firstFrameButton;

        [SerializeField]
        private Button m_prevFrameButton;

        [SerializeField]
        private Toggle m_playToggle;

        [SerializeField]
        private Button m_nextFrameButton;

        [SerializeField]
        private Button m_lastFrameButton;

        [SerializeField]
        private TMP_InputField m_frameInput;

        [SerializeField]
        private TMP_Dropdown m_animationsDropDown;

        [SerializeField]
        private TMP_InputField m_samplesInput;

        [SerializeField]
        private Button m_addKeyframeButton;

        [SerializeField]
        private Button m_addEventButton;

        [SerializeField]
        private Toggle m_dopesheetToggle;

        [SerializeField]
        private VirtualizingTreeView m_propertiesTreeView;
    }
}
