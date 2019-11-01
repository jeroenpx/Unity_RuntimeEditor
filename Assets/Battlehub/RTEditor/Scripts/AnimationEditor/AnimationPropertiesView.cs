using Battlehub.RTCommon;
using Battlehub.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class AnimationPropertiesView : MonoBehaviour
    {
        public delegate void EventHandler<T>(T args);

        public class ItemsArg
        {
            public int[] Rows;
            public RuntimeAnimationProperty[] Items;
        }

        public class ItemArg
        {
            public int Row;
            public RuntimeAnimationProperty Item;
        }

        private ItemArg m_propertyValueChangedArg = new ItemArg();
        public event EventHandler<ItemArg> PropertyValueChanged;
        public event EventHandler<ItemsArg> PropertiesAdded;
        public event EventHandler<ItemsArg> PropertiesRemoved;
        public event EventHandler<ItemArg> PropertyExpanded;
        public event EventHandler<ItemArg> PropertyCollapsed;

        [SerializeField]
        private VirtualizingTreeView m_propertiesTreeView = null;
        private List<RuntimeAnimationProperty> m_props = new List<RuntimeAnimationProperty>();
        
        private readonly RuntimeAnimationProperty m_emptyTop = new RuntimeAnimationProperty { ComponentTypeName = RuntimeAnimationProperty.k_SpecialEmptySpace };
        private readonly RuntimeAnimationProperty m_emptyBottom = new RuntimeAnimationProperty { ComponentTypeName = RuntimeAnimationProperty.k_SpecialAddButton };

        private bool m_isStarted;

        public GameObject Target
        {
            get;
            set;
        }

        private RuntimeAnimationClip m_clip;
        public RuntimeAnimationClip Clip
        {
            get { return m_clip; }
            set
            {
                if(m_clip != null)
                {
                    foreach(RuntimeAnimationProperty property in m_clip.Properties)
                    {
                        property.ValueChanged -= OnPropertyValueChanged;
                        if(property.Children != null)
                        {
                            foreach(RuntimeAnimationProperty childProperty in property.Children)
                            {
                                childProperty.ValueChanged -= OnPropertyValueChanged;
                            }
                        }
                    }
                }

                m_clip = value;
                DataBind();
            }
        }

        public RuntimeAnimationProperty[] Props
        {
            get { return m_props.ToArray(); }
        }

        public int IndexOf(RuntimeAnimationProperty item)
        {
            return m_props.IndexOf(item);
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

            //List<RuntimeAnimationProperty> addedProperties = new List<RuntimeAnimationProperty>();
            //List<int> addedIndexes = new List<int>();

            m_props =  new List<RuntimeAnimationProperty>();
            if(Clip != null)
            {
                if(Clip.Properties.Count > 0)
                {
                    m_props.Add(m_emptyTop);
                    //addedProperties.Add(m_emptyTop);
                    //addedIndexes.Add(0);
                }

                foreach (RuntimeAnimationProperty property in Clip.Properties)
                {
                    m_props.Add(property);
                    //addedProperties.Add(property);
                    //addedIndexes.Add(m_props.Count - 1);

                    if (property.Children != null)
                    {
                        for (int i = 0; i < property.Children.Count; i++)
                        {
                            m_props.Add(property.Children[i]);
                            //addedProperties.Add(property.Children[i]);
                            //addedIndexes.Add(m_props.Count - 1);
                            property.Children[i].ValueChanged += OnPropertyValueChanged;   
                        }
                    }
                    else
                    {
                        property.ValueChanged += OnPropertyValueChanged;
                    }
                }
            }
            m_props.Add(m_emptyBottom);
            m_propertiesTreeView.Items = m_props.Where(p => p.Children != null || p == m_emptyTop || p == m_emptyBottom);

            //if (PropertiesAdded != null)
            //{
            //    PropertiesAdded(new ItemsArg { Items = addedProperties.ToArray(), Rows = addedIndexes.ToArray() });
            //}
        }

        public void AddProperty(RuntimeAnimationProperty property)
        {
            if (property.ComponentTypeName == RuntimeAnimationProperty.k_SpecialAddButton)
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
                List<RuntimeAnimationProperty> addedProperties = new List<RuntimeAnimationProperty>();
                List<int> addedIndexes = new List<int>();

                if (m_propertiesTreeView.ItemsCount == 1)
                {
                    m_propertiesTreeView.Insert(0, m_emptyTop);
                    m_props.Insert(0, m_emptyTop);
                    addedProperties.Add(m_emptyTop);
                    addedIndexes.Add(0);
                }

                property = new RuntimeAnimationProperty(property);
                property.Parent = null;
                property.Children = null;
                property.TryToCreateChildren();

                Clip.Add(property);

                m_propertiesTreeView.Insert(m_propertiesTreeView.ItemsCount - 1, property);
                
                addedProperties.Add(property);
                addedIndexes.Add(m_props.Count - 1);
                m_props.Insert(m_props.Count - 1, property);
                if (property.Children != null)
                {
                    for(int i = 0; i < property.Children.Count; i++)
                    {
                        addedProperties.Add(property.Children[i]);
                        addedIndexes.Add(m_props.Count - 1);
                        m_props.Insert(m_props.Count - 1, property.Children[i]);
                        property.Children[i].ValueChanged += OnPropertyValueChanged;
                    }
                }
                else
                {
                    property.ValueChanged += OnPropertyValueChanged;
                }

                if(PropertiesAdded != null)
                {
                    PropertiesAdded(new ItemsArg { Items = addedProperties.ToArray(), Rows = addedIndexes.ToArray() });
                }
            }
        }

        private void OnPropertyValueChanged(RuntimeAnimationProperty property, object oldValue, object newValue)
        {
            if(PropertyValueChanged != null)
            {
                int rowIndex = m_props.IndexOf(property);

                m_propertyValueChangedArg.Row = rowIndex;
                m_propertyValueChangedArg.Item = property;

                PropertyValueChanged(m_propertyValueChangedArg);
            }
        }

        private void OnPropertiesSelectionChanged(object sender, SelectionChangedArgs e)
        {
           
        }

        private void OnPropertiesItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            RuntimeAnimationProperty item = (RuntimeAnimationProperty)e.Item;
            e.Children = item.Children;
        }

        private void OnPropertyExpanded(object sender, VirtualizingItemExpandingArgs e)
        {
            if(PropertyExpanded != null)
            {
                RuntimeAnimationProperty item = (RuntimeAnimationProperty)e.Item;
                int index = IndexOf(item);
                PropertyExpanded(new ItemArg { Item = (RuntimeAnimationProperty)e.Item, Row = index });
            }
        }

        private void OnPropertyCollapsed(object sender, VirtualizingItemCollapsedArgs e)
        {
            if(PropertyCollapsed != null)
            {
                RuntimeAnimationProperty item = (RuntimeAnimationProperty)e.Item;
                int index = IndexOf(item);
                PropertyCollapsed(new ItemArg { Item = (RuntimeAnimationProperty)e.Item, Row = index });
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

            List<Tuple<int, RuntimeAnimationProperty>> removedProperties = new List<Tuple<int, RuntimeAnimationProperty>>();
            
            HashSet<int> removedHs = new HashSet<int>();

            foreach(RuntimeAnimationProperty item in e.Items)
            {
                if(item.Parent != null)
                {
                    int row = IndexOf(item.Parent);
                    if(!removedHs.Contains(row))
                    {
                        removedHs.Add(row);
                        removedProperties.Add(new Tuple<int, RuntimeAnimationProperty>(row, item.Parent));

                        m_propertiesTreeView.RemoveChild(null, item.Parent);
                        
                        for (int i = 0; i < item.Parent.Children.Count; ++i)
                        {
                            row = IndexOf(item.Parent.Children[i]);
                            if(!removedHs.Contains(row))
                            {
                                removedHs.Add(row);
                                removedProperties.Add(new Tuple<int, RuntimeAnimationProperty>(row, item.Parent.Children[i]));
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
                        removedProperties.Add(new Tuple<int, RuntimeAnimationProperty>(row, item));

                        if (item.Children != null)
                        {
                            for (int i = 0; i < item.Children.Count; ++i)
                            {
                                row = IndexOf(item.Children[i]);
                                if (!removedHs.Contains(row))
                                {
                                    removedHs.Add(row);
                                    removedProperties.Add(new Tuple<int, RuntimeAnimationProperty>(row, item.Children[i]));
                                }
                            }
                        }
                    }
                }
            }

            for(int i = 0; i < removedProperties.Count; ++i)
            {
                RuntimeAnimationProperty property = removedProperties[i].Item2;
                property.ValueChanged -= OnPropertyValueChanged;
                m_props.Remove(property);
                Clip.Remove(property);
            }

            if(m_propertiesTreeView.ItemsCount == 2)
            {
                m_props.Remove(m_emptyTop);
                m_propertiesTreeView.RemoveChild(null, m_emptyTop);
                removedProperties.Insert(0, new Tuple<int, RuntimeAnimationProperty>(0, m_emptyTop));
            }

            IEnumerable<Tuple<int, RuntimeAnimationProperty>> orderedItems = removedProperties.OrderBy(t => t.Item1);

            if (PropertiesRemoved != null)
            {
                PropertiesRemoved(new ItemsArg {  Items = orderedItems.Select(t => t.Item2).ToArray(), Rows = orderedItems.Select(t => t.Item1).ToArray() });
            }

            m_propertiesTreeView.ItemsRemoved += OnPropertiesRemoved;
        }

        private void OnPropertiesItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            AnimationPropertyView ui = e.ItemPresenter.GetComponent<AnimationPropertyView>();
            RuntimeAnimationProperty item = (RuntimeAnimationProperty)e.Item;

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
