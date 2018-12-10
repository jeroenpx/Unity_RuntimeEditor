using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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
        private RectTransform m_anchor;

        [SerializeField]
        private Transform m_panel;

        private Transform m_root;

        private int m_depth;
        public int Depth
        {
            get { return m_depth; }
            set { m_depth = value; }
        }

        private MenuItem m_child;
        public MenuItem Child
        {
            get { return m_child; }
            set
            {
                if(m_child != null && m_child != value && m_child.Submenu != null)
                {
                    MenuItem oldChild = m_child;
                    m_child = value;
                    oldChild.Unselect();
                }
                else
                {
                    m_child = value;
                }

                
            }
        }

        private MenuItem m_parent;
        public MenuItem Parent
        {
            get { return m_parent; }
            set
            {
                m_parent = value;
            }
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
        }

        private void DataBind()
        {
            Clear();

            Dictionary<string, MenuItemInfo> pathToItem = new Dictionary<string, MenuItemInfo>();
            Dictionary<string, List<MenuItemInfo>> pathToChildren = new Dictionary<string, List<MenuItemInfo>>();
            if (m_items != null)
            {
                for (int i = 0; i < m_items.Length; ++i)
                {
                    MenuItemInfo menuItemInfo = m_items[i];
                    if (string.IsNullOrEmpty(menuItemInfo.Path))
                    {
                        continue;
                    }

                    menuItemInfo.Path = menuItemInfo.Path.Replace("\\", "/");
                    string[] pathParts = menuItemInfo.Path.Split('/');
                    if (pathParts.Length == m_depth + 1)
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
                        if (!pathToChildren.TryGetValue(path, out childrenList))
                        {
                            childrenList = new List<MenuItemInfo>();
                            pathToChildren.Add(path, childrenList);
                        }

                        if (!pathToItem.ContainsKey(pathParts[m_depth]))
                        {
                            pathToItem[pathParts[m_depth]] = new MenuItemInfo
                            {
                                Text = pathParts[m_depth],
                                Path = path
                            };
                        }

                        if (string.IsNullOrEmpty(menuItemInfo.Text))
                        {
                            menuItemInfo.Text = pathParts[m_depth + 1];
                        }
                        childrenList.Add(menuItemInfo);
                    }
                }
            }

            foreach (MenuItemInfo menuItemInfo in pathToItem.Values)
            {
                MenuItem menuItem = Instantiate(m_menuItemPrefab, m_panel, false);
                menuItem.name = "MenuItem";
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

        private void Clear()
        {
            foreach (Transform child in m_panel)
            {
                MenuItem menuItem = child.GetComponent<MenuItem>();
                Destroy(child.gameObject);
            }
        }

        public void Open()
        {
            if(m_anchor != null)
            {
                Vector3[] corners = new Vector3[4];
                m_anchor.GetWorldCorners(corners);
                transform.position = corners[0];
            }
            
            DataBind();
            gameObject.SetActive(true);

            if(m_anchor == null)
            {
                Fit();
            }   
        }

        private void Fit()
        {
            RectTransform rootRT = (RectTransform)m_root;
            Vector3 position = rootRT.InverseTransformPoint(transform.position);

            Vector2 topLeft = -Vector2.Scale(rootRT.rect.size, rootRT.pivot);
            RectTransform rt = m_menuItemPrefab.GetComponent<RectTransform>();
            Vector2 size = new Vector2(rt.rect.width, rt.rect.height * m_panel.childCount);

            if (position.x + size.x  > topLeft.x + rootRT.rect.width)
            {
                position.x = position.x - size.x - 3;
            }
            else
            {
                position.x += 3;
            }

            if (position.y - size.y < topLeft.y)
            {
                position.y -= (position.y - size.y) - topLeft.y;
            }

            transform.position = rootRT.TransformPoint(position);
        }

        public void Close()
        {
            Clear();
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                if(m_child == null)
                {
                    MenuItem parentMenuItem = m_parent;
                    while (parentMenuItem != null && !parentMenuItem.IsPointerOver)
                    {
                        Menu parentMenu = parentMenuItem.GetComponentInParent<Menu>();
                        parentMenuItem = parentMenu.m_parent;
                        if (parentMenuItem != null)
                        {
                            Destroy(parentMenu.gameObject);
                        }
                        else
                        {
                            parentMenu.Close();
                        }
                    }
                    

                    if(m_parent == null)
                    {
                        Close();
                    }
                    else
                    {
                        if (!m_parent.IsPointerOver)
                        {
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }
    }
}
