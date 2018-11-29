using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class TabPreview : MonoBehaviour
    {
        [SerializeField]
        private Image m_img;

        [SerializeField]
        private Text m_text;

        [SerializeField]
        private RectTransform m_contentPart;


        public Sprite Icon
        {
            get { return m_img.sprite; }
            set { m_img.sprite = value; }
        }

        public string Text
        {
            get { return m_text.text; }
            set { m_text.text = value; }
        }


        private void Awake()
        {
            
        }

    }
}

