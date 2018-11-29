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

    public delegate void TabEventArgs(Tab sender);
    public delegate void TabEventArgs<T>(Tab sender, T args);

    public class Tab : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event TabEventArgs<bool> Toggle;
        public event TabEventArgs<PointerEventData> PointerDown;
        public event TabEventArgs<PointerEventData> PointerUp;
        public event TabEventArgs<PointerEventData> BeginDrag;
        public event TabEventArgs<PointerEventData> Drag;
        public event TabEventArgs<PointerEventData> EndDrag;
        public event TabEventArgs Close;

        private Region m_parentRegion;

        [SerializeField]
        private TabPreview m_tabPreviewPrefab;
        private TabPreview m_tabPreview;

        [SerializeField]
        private CanvasGroup m_canvasGroup;
        
        [SerializeField]
        private Image m_img;

        [SerializeField]
        private Text m_text;

        [SerializeField]
        private Toggle m_toggle;

        [SerializeField]
        private Button m_closeButton;

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

        public Vector3 Position
        {
            get { return m_tabPreview.transform.position; }
            set { m_tabPreview.transform.position = value; }
        }
        
        private void Awake()
        {
            m_toggle.onValueChanged.AddListener(OnToggleValueChanged);
            if(m_closeButton != null)
            {
                m_closeButton.onClick.AddListener(OnCloseButtonClick);
            }
        }

        private void Start()
        {
            m_parentRegion = GetComponentInParent<Region>();
        }

        private void OnDestroy()
        {
            if(m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }

            if (m_closeButton != null)
            {
                m_closeButton.onClick.RemoveListener(OnCloseButtonClick);
            }
        }

        private void OnToggleValueChanged(bool value)
        {
            if(Toggle != null)
            {
                Toggle(this, value);
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            m_tabPreview = Instantiate(m_tabPreviewPrefab, m_parentRegion.PreviewPanel);

            RectTransform previewTransform = (RectTransform)m_tabPreview.transform;
            RectTransform rt = (RectTransform)transform;
            previewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.rect.width);
            previewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rt.rect.height);
            previewTransform.position = Position;

            m_tabPreview.Icon = Icon;
            m_tabPreview.Text = Text;

            m_canvasGroup.alpha = 0;

            if (BeginDrag != null)
            {
                BeginDrag(this, eventData);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if(Drag != null)
            {
                Drag(this, eventData);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            Destroy(m_tabPreview.gameObject);
            m_tabPreview = null;

            m_canvasGroup.alpha = 1;

            if (EndDrag != null)
            {
                EndDrag(this, eventData);
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if(PointerDown != null)
            {
                PointerDown(this, eventData);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if(PointerUp != null)
            {
                PointerUp(this, eventData);
            }
        }

        private void OnCloseButtonClick()
        {
            if(Close != null)
            {
                Close(this);
            }
        }
    }
}
