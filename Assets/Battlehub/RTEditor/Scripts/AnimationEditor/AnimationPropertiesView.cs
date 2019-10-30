using Battlehub.RTCommon;
using Battlehub.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AnimationPropertiesView : MonoBehaviour
    {
        public delegate void EventHandler<T>(T args);

        public class ItemsArgs
        {
            public int[] Rows;
            public AnimationPropertyItem[] Items;
        }

        public class ExpandCollapseArgs
        {
            public int Row;
            public AnimationPropertyItem Item;
        }

        public event EventHandler<ItemsArgs> PropertiesAdded;
        public event EventHandler<ItemsArgs> PropertiesRemoved;
        public event EventHandler<ExpandCollapseArgs> PropertyExpanded;
        public event EventHandler<ExpandCollapseArgs> PropertyCollapsed;

        [SerializeField]
        private VirtualizingTreeView m_propertiesTreeView = null;
        private List<AnimationPropertyItem> m_properties = new List<AnimationPropertyItem>();

        private readonly AnimationPropertyItem m_emptyTop = new AnimationPropertyItem { ComponentType = AnimationPropertyItem.k_SpecialEmptySpace };
        private readonly AnimationPropertyItem m_emptyBottom = new AnimationPropertyItem { ComponentType = AnimationPropertyItem.k_SpecialAddButton };

        private bool m_isStarted;

        private RuntimeAnimation m_target;
        public RuntimeAnimation Target
        {
            get { return m_target; }
            set
            {
                m_target = value;
                DataBind();
            }
        }

        public AnimationPropertyItem[] Properties
        {
            get
            {
                return m_properties.ToArray(); //m_propertiesTreeView.Items.OfType<AnimationPropertyItem>().ToArray();
            }
        }

        public int IndexOf(AnimationPropertyItem item)
        {
            return m_properties.IndexOf(item);
        }

        protected virtual void Awake()
        {
            Subscribe();
        }

        protected virtual void Start()
        {
            m_isStarted = true;

            if (m_propertiesTreeView != null)
            {
                m_propertiesTreeView.CanReorder = false;
                m_propertiesTreeView.CanReparent = false;
                m_propertiesTreeView.CanDrag = false;
                m_propertiesTreeView.CanSelectAll = false;
                m_propertiesTreeView.CanMultiSelect = false;
                m_propertiesTreeView.CanEdit = false;

                DataBind();
            }
        }

        protected virtual void OnDestroy()
        {
            Unsubscribe();
        }

        protected virtual void Subscribe()
        {
            if(m_propertiesTreeView != null)
            {
                m_propertiesTreeView.ItemDataBinding += OnPropertiesItemDataBinding;
                m_propertiesTreeView.ItemExpanding += OnPropertiesItemExpanding;
                m_propertiesTreeView.ItemExpanded += OnPropertyExpanded;
                m_propertiesTreeView.ItemCollapsed += OnPropertyCollapsed;
                m_propertiesTreeView.SelectionChanged += OnPropertiesSelectionChanged;
                m_propertiesTreeView.ItemsRemoving += OnPropertiesRemoving;
                m_propertiesTreeView.ItemsRemoved += OnPropertiesRemoved;
            }
        }

        protected virtual void Unsubscribe()
        {
            if (m_propertiesTreeView != null)
            {
                m_propertiesTreeView.ItemDataBinding -= OnPropertiesItemDataBinding;
                m_propertiesTreeView.ItemExpanding -= OnPropertiesItemExpanding;
                m_propertiesTreeView.ItemExpanded -= OnPropertyExpanded;
                m_propertiesTreeView.ItemCollapsed -= OnPropertyCollapsed;
                m_propertiesTreeView.SelectionChanged -= OnPropertiesSelectionChanged;
                m_propertiesTreeView.ItemsRemoving -= OnPropertiesRemoving;
                m_propertiesTreeView.ItemsRemoved -= OnPropertiesRemoved;
            }
        }

        private void DataBind()
        {
            if (!m_isStarted)
            {
                return;
            }
            m_properties = new List<AnimationPropertyItem> { m_emptyBottom };
            m_propertiesTreeView.Items = m_properties;
        }


        public void AddProperty(AnimationPropertyItem propertyItem)
        {
            if (propertyItem.ComponentType == AnimationPropertyItem.k_SpecialAddButton)
            {
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                IAnimationSelectPropertiesDialog selectPropertiesDialog = null;
                Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialogWindow(RuntimeWindowType.SelectAnimationProperties.ToString(), "Select Properties",
                     (sender, args) => { }, (sender, args) => { }, 250, 250, 400, 400);
                selectPropertiesDialog = IOC.Resolve<IAnimationSelectPropertiesDialog>();
                selectPropertiesDialog.View = this;
                selectPropertiesDialog.Target = Target;
            }
            else
            {

                List<AnimationPropertyItem> addedProperties = new List<AnimationPropertyItem>();
                List<int> addedIndexes = new List<int>();

                if (m_propertiesTreeView.ItemsCount == 1)
                {
                    m_propertiesTreeView.Insert(0, m_emptyTop);
                    m_properties.Insert(0, m_emptyTop);
                    addedProperties.Add(m_emptyTop);
                    addedIndexes.Add(0);
                }

                propertyItem = new AnimationPropertyItem(propertyItem);
                propertyItem.Parent = null;
                propertyItem.Children = null;
                propertyItem.TryToCreateChildren();
                m_propertiesTreeView.Insert(m_propertiesTreeView.ItemsCount - 1, propertyItem);
                
                addedProperties.Add(propertyItem);
                addedIndexes.Add(m_properties.Count - 1);
                m_properties.Insert(m_properties.Count - 1, propertyItem);
                if (propertyItem.Children != null)
                {
                    for(int i = 0; i < propertyItem.Children.Count; i++)
                    {
                        addedProperties.Add(propertyItem.Children[i]);
                        addedIndexes.Add(m_properties.Count - 1);
                        m_properties.Insert(m_properties.Count - 1, propertyItem.Children[i]);
                    }
                }

                if(PropertiesAdded != null)
                {
                    PropertiesAdded(new ItemsArgs { Items = addedProperties.ToArray(), Rows = addedIndexes.ToArray() });
                }
            }
        }

        private void OnPropertiesSelectionChanged(object sender, SelectionChangedArgs e)
        {
           
        }

        private void OnPropertiesItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            AnimationPropertyItem item = (AnimationPropertyItem)e.Item;
            e.Children = item.Children;
        }

        private void OnPropertyExpanded(object sender, VirtualizingItemExpandingArgs e)
        {
            if(PropertyExpanded != null)
            {
                AnimationPropertyItem item = (AnimationPropertyItem)e.Item;
                int index = IndexOf(item);
                PropertyExpanded(new ExpandCollapseArgs { Item = (AnimationPropertyItem)e.Item, Row = index });
            }
        }

        private void OnPropertyCollapsed(object sender, VirtualizingItemCollapsedArgs e)
        {
            if(PropertyCollapsed != null)
            {
                AnimationPropertyItem item = (AnimationPropertyItem)e.Item;
                int index = IndexOf(item);
                PropertyCollapsed(new ExpandCollapseArgs { Item = (AnimationPropertyItem)e.Item, Row = index });
            }
        }

        private void OnPropertiesRemoving(object sender, ItemsCancelArgs e)
        {
            if (m_propertiesTreeView.ItemsCount > 2)
            {
                e.Items.Remove(m_emptyTop);
            }

            e.Items.Remove(m_emptyBottom);   
        }

        private void OnPropertiesRemoved(object sender, ItemsRemovedArgs e)
        {
            m_propertiesTreeView.ItemsRemoved -= OnPropertiesRemoved;

            List<Tuple<int, AnimationPropertyItem>> removedProperties = new List<Tuple<int, AnimationPropertyItem>>();
            
            HashSet<int> removedHs = new HashSet<int>();

            foreach(AnimationPropertyItem item in e.Items)
            {
                if(item.Parent != null)
                {
                    int row = IndexOf(item.Parent);
                    if(!removedHs.Contains(row))
                    {
                        removedHs.Add(row);
                        removedProperties.Add(new Tuple<int, AnimationPropertyItem>(row, item.Parent));

                        m_propertiesTreeView.RemoveChild(null, item.Parent);
                        
                        for (int i = 0; i < item.Parent.Children.Count; ++i)
                        {
                            row = IndexOf(item.Parent.Children[i]);
                            if(!removedHs.Contains(row))
                            {
                                removedHs.Add(row);
                                removedProperties.Add(new Tuple<int, AnimationPropertyItem>(row, item.Parent.Children[i]));
                            }
                        }
                    }
                }
                  
                else
                {
                    int row = IndexOf(item);
                    if(!removedHs.Contains(row))
                    {
                        removedHs.Add(row);
                        removedProperties.Add(new Tuple<int, AnimationPropertyItem>(row, item));

                        if (item.Children != null)
                        {
                            for (int i = 0; i < item.Children.Count; ++i)
                            {
                                row = IndexOf(item.Children[i]);
                                if (!removedHs.Contains(row))
                                {
                                    removedHs.Add(row);
                                    removedProperties.Add(new Tuple<int, AnimationPropertyItem>(row, item.Children[i]));
                                }
                            }
                        }
                    }
                }
            }

            for(int i = 0; i < removedProperties.Count; ++i)
            {
                m_properties.Remove(removedProperties[i].Item2);
            }

            if(m_propertiesTreeView.ItemsCount == 2)
            {
                m_properties.Remove(m_emptyTop);
                m_propertiesTreeView.RemoveChild(null, m_emptyTop);
                removedProperties.Insert(0, new Tuple<int, AnimationPropertyItem>(0, m_emptyTop));
            }

            IEnumerable<Tuple<int, AnimationPropertyItem>> orderedItems = removedProperties.OrderBy(t => t.Item1);

            if (PropertiesRemoved != null)
            {
                PropertiesRemoved(new ItemsArgs {  Items = orderedItems.Select(t => t.Item2).ToArray(), Rows = orderedItems.Select(t => t.Item1).ToArray() });
            }

            m_propertiesTreeView.ItemsRemoved += OnPropertiesRemoved;
        }

        private void OnPropertiesItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            AnimationPropertyView ui = e.ItemPresenter.GetComponent<AnimationPropertyView>();
            AnimationPropertyItem item = (AnimationPropertyItem)e.Item;

            ui.View = this;
            if (m_emptyBottom != item && m_emptyTop != item)
            {
                e.CanSelect = true;
            }
            else
            {   
                e.CanSelect = false;
            }

            ui.Item = item;

            e.HasChildren = item.Children != null && item.Children.Count > 0;
        }
    }
}
