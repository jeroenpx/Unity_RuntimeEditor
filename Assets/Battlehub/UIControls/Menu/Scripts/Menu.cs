using System;
using System.Collections.Generic;
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

    [Serializable]
    public class MenuItemInfo
    {
        public string Path;
        public string Text;
        public Sprite Icon;
        
        public MenuItemValidationEvent Validate;
        public UnityEvent Action;
    }

    public class Menu : MonoBehaviour
    {
        [SerializeField]
        private MenuItem m_menuItemPrefab;

        [SerializeField]
        private Transform m_panel;

        private Transform m_root;

        private int m_depth;
        public int Depth
        {
            get { return m_depth; }
            set { m_depth = value; }
        }

        [SerializeField]
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

            m_root = transform.parent;

            DataBind();
        }

        private void DataBind()
        {
            foreach(Transform child in m_panel)
            {
                Destroy(child.gameObject);
            }

            Dictionary<string, MenuItemInfo> pathToItem = new Dictionary<string, MenuItemInfo>();
            Dictionary<string, List<MenuItemInfo>> pathToChildren = new Dictionary<string, List<MenuItemInfo>>();
            if(m_items != null)
            {
                for(int i = 0; i < m_items.Length; ++i)
                {
                    MenuItemInfo menuItemInfo = m_items[i];
                    if(string.IsNullOrEmpty(menuItemInfo.Path))
                    {
                        continue;
                    }

                    menuItemInfo.Path = menuItemInfo.Path.Replace("\\", "/");
                    string[] pathParts = menuItemInfo.Path.Split('/');
                    if(pathParts.Length == m_depth + 1)
                    {
                        if (string.IsNullOrEmpty(menuItemInfo.Text))
                        {
                            menuItemInfo.Text = pathParts[m_depth];
                        }
                        pathToItem[pathParts[m_depth]] = menuItemInfo;
                    }
                    else
                    {
                        string path = string.Join("/", pathParts, 0, m_depth + 1);
                        List<MenuItemInfo> childrenList;
                        if(!pathToChildren.TryGetValue(path, out childrenList))
                        {
                            childrenList = new List<MenuItemInfo>();
                            pathToChildren.Add(path, childrenList);
                        }

                        if(!pathToItem.ContainsKey(pathParts[m_depth]))
                        {
                            pathToItem[pathParts[m_depth]] = new MenuItemInfo
                            {
                                Text = pathParts[m_depth],
                                Path = path
                            };
                        }

                        if(string.IsNullOrEmpty(menuItemInfo.Text))
                        {
                            menuItemInfo.Text = pathParts[m_depth + 1];
                        }
                        childrenList.Add(menuItemInfo);
                    }
                }
            }

            foreach(MenuItemInfo menuItemInfo in pathToItem.Values)
            {
                MenuItem menuItem = Instantiate(m_menuItemPrefab, m_panel, false);
                menuItem.Depth = Depth + 1;
                menuItem.Root = m_root;

                List<MenuItemInfo> childrenList;
                if (pathToChildren.TryGetValue(menuItemInfo.Path, out childrenList))
                {
                    menuItem.Children = childrenList.ToArray();
                }

                menuItem.Item = menuItemInfo;
            }
        }

        public void Open(Transform anchor, bool alignVertically)
        {
            DataBind();
        }

    }
}
