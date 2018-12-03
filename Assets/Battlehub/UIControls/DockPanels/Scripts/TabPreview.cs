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

        [SerializeField]
        private RectTransform m_rt;

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

        public bool IsContentActive
        {
            get { return m_contentPart.gameObject.activeSelf; }
            set { m_contentPart.gameObject.SetActive(value); }
        }

        private float m_maxWidth;
        public float MaxWidth
        {
            set { m_maxWidth = value; }
        }

        public Vector2 Size
        {
            set
            {
                m_contentPart.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                m_contentPart.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
                m_rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(value.x, m_maxWidth));
            }
        }


        private void Awake()
        {
            if(m_rt == null)
            {
                m_rt = (RectTransform)transform;
            }

            IsContentActive = false;
        }

    }
}

