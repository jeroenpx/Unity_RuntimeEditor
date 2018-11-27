using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class PointerEventArgs
    {
        public PointerEventData EventData
        {
            get;
            private set;
        }

        public PointerEventArgs(PointerEventData eventData)
        {
            EventData = eventData;
        }
    }

    public delegate void TabEventArgs<T>(Tab sender, T args);

    public class Tab : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event TabEventArgs<bool> Toggle;
        public event TabEventArgs<PointerEventData> PointerDown;
        public event TabEventArgs<PointerEventData> PointerUp;
        public event TabEventArgs<PointerEventData> BeginDrag;
        public event TabEventArgs<PointerEventData> Drag;
        public event TabEventArgs<PointerEventData> EndDrag;

        [SerializeField]
        private Image m_img;

        [SerializeField]
        private Text m_text;

        [SerializeField]
        private Toggle m_toggle;

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

        public ToggleGroup ToggleGroup
        {
            get { return m_toggle.group; }
            set { m_toggle.group = value; }
        }

        public bool IsOn
        {
            get { return m_toggle.isOn; }
            set { m_toggle.isOn = value; }
        }

        public int Index
        {
            get { return transform.GetSiblingIndex(); }
        }

        private void Awake()
        {

            m_toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDestroy()
        {
            if(m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }
        }

        private void OnToggleValueChanged(bool value)
        {
            if(Toggle != null)
            {
                Toggle(this, value);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("BeginDrag");
            if(BeginDrag != null)
            {
                BeginDrag(this, eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("Drag");
            if(Drag != null)
            {
                Drag(this, eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("EndDrag");
            if(EndDrag != null)
            {
                EndDrag(this, eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("PointerDown");
            if(PointerDown != null)
            {
                PointerDown(this, eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("PointerUp");
            if(PointerUp != null)
            {
                PointerUp(this, eventData);
            }
        }
    }

}
