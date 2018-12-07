using System;
using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.UIControls.MenuControl
{
    public class MenuItemValidationArgs
    {
        public bool IsValid
        {
            get;
            set;
        }

        public MenuItemValidationArgs()
        {
            IsValid = true;
        }
    }

    [Serializable]
    public class MenuItemValidationEvent : UnityEvent<MenuItemValidationArgs>
    {
    }


    public class MenuItemInfo
    {
        public Sprite Icon;
        public string Text;
        public MenuItemValidationEvent Validate;
        public UnityEvent Action;

        public MenuItemInfo[] Children;
    }

    public class Menu : MonoBehaviour
    {
        [SerializeField]
        private MenuItem m_menuItemPrefab;

        [SerializeField]
        private Transform m_panel;

        private MenuItemInfo[] m_items;
        public MenuItemInfo[] Items
        {
            get { return m_items; }
            set
            {
                m_items = value;
                DataBind();
            }
        }

        private void Awake()
        {
            if(m_panel == null)
            {
                m_panel = transform;
            }

            DataBind();
        }

        private void DataBind()
        {
            foreach(Transform child in m_panel)
            {
                Destroy(child.gameObject);
            }

            if(m_items != null)
            {
                for(int i = 0; i < m_items.Length; ++i)
                {
                    MenuItem menuItem = Instantiate(m_menuItemPrefab, m_panel, false);
                    menuItem.Item = m_items[i];
                }
            }
        }

        public void Open(Transform anchor, bool alignVertically)
        {

            DataBind();
        }

    }
}
