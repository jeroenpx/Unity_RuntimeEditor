using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.MenuControl
{
    public class MenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private Menu m_menuPrefab;

        [SerializeField]
        private Image m_icon;

        [SerializeField]
        private Text m_text;

        [SerializeField]
        private GameObject m_expander;

        [SerializeField]
        private GameObject m_selection;

        private Transform m_root;
        public Transform Root
        {
            get { return m_root; }
            set { m_root = value; }
        }

        private int m_depth;
        public int Depth
        {
            get { return m_depth; }
            set { m_depth = value; }
        }

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

        private MenuItemInfo[] m_children;
        public MenuItemInfo[] Children
        {
            get { return m_children; }
            set { m_children = value; }
        }

        public bool HasChildren
        {
            get { return m_children != null && m_children.Length > 0; }
        }

        private Menu m_submenu;

        private void OnDestroy()
        {
            if(m_submenu != null)
            {
                Destroy(m_submenu.gameObject);
            }
        }

        private void DataBind()
        {
            if(m_item != null)
            {
                m_icon.sprite = m_item.Icon;
                m_icon.gameObject.SetActive(m_icon.sprite != null);
                m_text.text = m_item.Text;
                m_expander.SetActive(HasChildren);
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
            if (HasChildren)
            {
                return;
            }
            if (m_item.Action != null)
            {
                if (m_item.Validate != null)
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
            if(HasChildren)
            {
                m_submenu = Instantiate(m_menuPrefab, m_root, false);
                m_submenu.Depth = m_depth;
                m_submenu.Items = Children;
                m_submenu.transform.position = FindPosition();
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            m_selection.SetActive(false);
          //  Destroy(m_submenu.gameObject);
        }

        private Vector3 FindPosition()
        {
            RectTransform rootRT = (RectTransform)m_root;
            RectTransform rt = (RectTransform)transform;

            Vector2 size = new Vector2(rt.rect.width, rt.rect.height * Children.Length);

            Vector3 position = -Vector2.Scale(rt.rect.size, rt.pivot);
            position.y = -position.y;
            position = rt.TransformPoint(position);
            position = rootRT.InverseTransformPoint(position);

            Vector2 topLeft = -Vector2.Scale(rootRT.rect.size, rootRT.pivot);
            
            if (position.x + size.x + size.x > topLeft.x + rootRT.rect.width)
            {
                position.x = position.x - size.x;
            }
            else
            {
                position.x = position.x + size.x;
            }            
            
            if (position.y - size.y < topLeft.y)
            {
                position.y -= (position.y - size.y) - topLeft.y;
            }

            return rootRT.TransformPoint(position);
        }
    }
}

