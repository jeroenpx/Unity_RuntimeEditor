using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.MenuControl
{
    public class MenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private Image m_icon;

        [SerializeField]
        private Text m_text;

        [SerializeField]
        private GameObject m_expander;

        [SerializeField]
        private GameObject m_selection;

        private MenuItemInfo m_item;
        public MenuItemInfo Item
        {
            get { return m_item; }
            set
            {
                if(m_item != value)
                {
                    m_item = value;
                    DataBind();
                }
            }
        }

        private void DataBind()
        {
            if(m_item != null)
            {
                m_icon.sprite = m_item.Icon;
                m_icon.gameObject.SetActive(m_icon.sprite != null);
                m_text.text = m_item.Text;
                m_expander.SetActive(m_item.Children != null && m_item.Children.Length > 0);
            }
            else
            {
                m_icon.sprite = null;
                m_icon.gameObject.SetActive(false);
                m_text.text = string.Empty;
                m_expander.SetActive(false);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(m_item.Action != null)
            {
                if(m_item.Validate != null)
                {
                    MenuItemValidationArgs args = new MenuItemValidationArgs();
                    m_item.Validate.Invoke(args);
                    if (args.IsValid)
                    {
                        m_item.Action.Invoke();
                    }
                }
                else
                {
                    m_item.Action.Invoke();
                }
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            m_selection.SetActive(true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            m_selection.SetActive(false);
        }
    }
}

