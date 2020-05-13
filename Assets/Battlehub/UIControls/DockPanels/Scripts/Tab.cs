using TMPro;
using System;
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

    public interface IPanelDelegate { 

        // Called when the given tab (and corresponding panel) receives an event where the panel should attempt to close and be destroyed.  
        // The handler of this method promises to eventually call tab.Close() if the closing should proceed.
        void OnPanelAttemptClose(Tab tab); 

        // When received, the host region is about to close and destory this tab and corresponding panel.   
        // A receiver typically uses this callback to release/recycle allocated resources.
        // When a receiver gets this callback, either OnAttemptClose() called proceedToClose(true), or this tab panel is being closed forcibly.
        void OnPanelClosing(Tab tab); 

        // When received, this tab and corresponding panel has been brought to the foreground and will be visible.
        void OnPanelVisible(Tab tab, bool isVisible); 

    }

    public delegate void TabEventArgs(Tab sender);
    public delegate void TabEventArgs<T>(Tab sender, T args);

    public class Tab : MonoBehaviour, IBeginDragHandler, IDragHandler, IInitializePotentialDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, 
        IPointerEnterHandler, IPointerExitHandler
    {
        public event TabEventArgs<bool> Toggle;
        public event TabEventArgs<PointerEventData> PointerDown;
        public event TabEventArgs<PointerEventData> PointerUp;
        public event TabEventArgs<PointerEventData> InitializePotentialDrag;
        public event TabEventArgs<PointerEventData> BeginDrag;
        public event TabEventArgs<PointerEventData> Drag;
        public event TabEventArgs<PointerEventData> EndDrag;
        public event TabEventArgs Closed;

        // Optional -- set in order to receive additional callbacks.
        public IPanelDelegate PanelDelegate;

        private bool m_closing = false;
        public bool Closing
        {
            get { return m_closing; }
        }

        private DockPanel m_root;

        private Tab m_tabPreview;

        [SerializeField]
        private CanvasGroup m_canvasGroup = null;

        [SerializeField]
        private Image m_img = null;

        [SerializeField]
        private TextMeshProUGUI m_text = null;

        [SerializeField]
        private Toggle m_toggle = null;

        [SerializeField]
        private Button m_closeButton = null;

        [SerializeField]
        private RectTransform m_contentPart = null;

        private RectTransform m_rt;

        public Sprite Icon
        {
            get { return m_img.sprite; }
            set
            {
                m_img.sprite = value;
                m_img.gameObject.SetActive(value != null);
            }
        }

        public string Text
        {
            get { return m_text.text; }
            set { m_text.text = value; }
        }

        public ToggleGroup ToggleGroup
        {
            get { return m_toggle.group; }
            set
            {
                if(m_toggle.group)
                {
                    m_toggle.group.UnregisterToggle(m_toggle);
                }

                m_toggle.group = value;
            }
        }

        public Region ParentRegion
        {
            get { return GetComponentInParent<Region>(); }
        }
     
        public bool IsOn
        {
            get { return m_toggle.isOn; }
            set { m_toggle.isOn = value; }
        }

        public int Index
        {
            get { return transform.GetSiblingIndex(); }
            set { transform.SetSiblingIndex(value); }
        }

        [SerializeField]
        private bool m_showOnPointerOver = false;
        private bool m_isPointerOver;
        [SerializeField]
        private bool m_canClose = true;
        public bool CanClose
        {
            get { return m_canClose; }
            set
            {
                m_canClose = value;
                if(m_canClose)
                {
                    IsCloseButtonVisible = m_canClose && (!m_showOnPointerOver || m_isPointerOver);
                }
                else
                {
                    IsCloseButtonVisible = false;
                }
            }
        }

        public bool IsCloseButtonVisible
        {
            get
            {
                if(m_closeButton != null)
                {
                    return m_closeButton.transform.parent.gameObject.activeSelf;
                }
                return false;
            }
            set
            {
                if(m_closeButton != null)
                {
                    m_closeButton.transform.parent.gameObject.SetActive(value && CanClose);
                }
            }
        }

        private bool m_canDrag = true;
        public bool CanDrag
        {
            get { return m_canDrag; }
            set { m_canDrag = value; }
        }

        public Vector3 PreviewPosition
        {
            get { return m_tabPreview.transform.position; }
            set
            {
                m_tabPreview.transform.position = value;
                Vector3 localPosition = m_tabPreview.transform.localPosition;
                localPosition.z = 0;
                m_tabPreview.transform.localPosition = localPosition;
            }
        }

        public bool IsContentActive
        {
            get { return m_contentPart.gameObject.activeSelf; }
            set
            {
                m_contentPart.gameObject.SetActive(value);
                RectTransform foreground = (RectTransform)m_contentPart.Find("Foreground");
                LayoutElement layoutElement = GetComponent<LayoutElement>();
                if (foreground != null && layoutElement != null)
                {
                    foreground.offsetMax = new Vector2(foreground.offsetMax.x, -Mathf.Max(layoutElement.minHeight, layoutElement.preferredHeight));
                }
            }
        }

        public bool IsPreviewContentActive
        {
            get { return m_tabPreview.IsContentActive; }
            set { m_tabPreview.IsContentActive = value; }
        }

        private float m_maxWidth;
        public float MaxWidth
        {
            set { m_maxWidth = value; }
        }

        public Vector2 RectSize
        {
            get {  return m_rt.rect.size; }
        }

        public Vector2 PreviewContentSize
        {
            set
            {
               m_tabPreview.MaxWidth = m_rt.rect.width; 
               m_tabPreview.Size = value;
            }
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
            m_rt = (RectTransform)transform;
            if(m_toggle != null)
            {
                m_toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }
            
            if(m_closeButton != null)
            {
                m_closeButton.onClick.AddListener(AttemptClose);
            }

            IsCloseButtonVisible = m_canClose && (!m_showOnPointerOver || m_isPointerOver);
            IsContentActive = false;
        }

        private void Start()
        {
            m_root = GetComponentInParent<DockPanel>();
        }

        private void OnDestroy()
        {
            if(m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }

            if (m_closeButton != null)
            {
                m_closeButton.onClick.RemoveListener(AttemptClose);
            }
        }

        private void OnToggleValueChanged(bool value)
        {
            PanelDelegate?.OnPanelVisible(this, value);

            if(Toggle != null)
            {
                Toggle(this, value);
            }
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            if(!m_canDrag)
            {
                return;
            }

            if(InitializePotentialDrag != null)
            {
                InitializePotentialDrag(this, eventData);
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!m_canDrag)
            {
                return;
            }

            gameObject.SetActive(false);
            m_tabPreview = Instantiate(this, m_root.Preview);
            m_tabPreview.ToggleGroup = null;
            m_tabPreview.gameObject.SetActive(true);
            gameObject.SetActive(true);
            m_tabPreview.m_toggle.isOn = true;

            RectTransform previewTransform = (RectTransform)m_tabPreview.transform;
            RectTransform rt = (RectTransform)transform;
            PreviewPosition = rt.position;
            previewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.rect.width);
            previewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rt.rect.height);

            m_tabPreview.Icon = Icon;
            m_tabPreview.Text = Text;
            m_tabPreview.IsCloseButtonVisible = IsCloseButtonVisible;

            m_canvasGroup.alpha = 0;

            if (BeginDrag != null)
            {
                BeginDrag(this, eventData);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!m_canDrag)
            {
                return;
            }
            if (Drag != null)
            {
                Drag(this, eventData);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!m_canDrag)
            {
                return;
            }
            m_canvasGroup.alpha = 1;

            if (EndDrag != null)
            {
                EndDrag(this, eventData);
            }

            Destroy(m_tabPreview.gameObject);
            m_tabPreview = null;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {            
            m_toggle.isOn = true;

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

        public void OnClosing() 
        {
            PanelDelegate?.OnPanelClosing(this);
        }

        public void Close()
        {
            if(Closed != null && m_closing == false)
            {
                m_closing = true;
                Closed(this);
            }
        }

        public void AttemptClose()
        {
            if (PanelDelegate != null)
            {
                if (m_closing == false)
                {
                    PanelDelegate.OnPanelAttemptClose(this);
                }
            }
            else
            {
                Close();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_isPointerOver = true;
            if(m_showOnPointerOver && m_canClose)
            {
                IsCloseButtonVisible = true;
            }
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_isPointerOver = false;
            if (m_showOnPointerOver)
            {
                IsCloseButtonVisible = false;
            }
        }
    }
}
