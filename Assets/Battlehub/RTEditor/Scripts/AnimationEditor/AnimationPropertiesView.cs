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
        private Toggle m_previewToggle = null;

        [SerializeField]
        private Toggle m_recordToggle = null;

        [SerializeField]
        private Button m_firstFrameButton = null;

        [SerializeField]
        private Button m_prevFrameButton = null;

        [SerializeField]
        private Toggle m_playToggle = null;

        [SerializeField]
        private Button m_nextFrameButton = null;

        [SerializeField]
        private Button m_lastFrameButton = null;

        [SerializeField]
        private TMP_InputField m_frameInput = null;

        [SerializeField]
        private TMP_Dropdown m_animationsDropDown = null;

        [SerializeField]
        private TMP_InputField m_samplesInput = null;

        [SerializeField]
        private Button m_addKeyframeButton = null;

        [SerializeField]
        private Button m_addEventButton = null;

        [SerializeField]
        private Toggle m_dopesheetToggle = null;

        [SerializeField]
        private VirtualizingTreeView m_propertiesTreeView = null;
        private List<AnimationPropertyItem> m_properties = new List<AnimationPropertyItem>();

        private readonly AnimationPropertyItem m_emptyTop = new AnimationPropertyItem { ComponentType = AnimationPropertyItem.k_SpecialEmptySpace };
        private readonly AnimationPropertyItem m_emptyBottom = new AnimationPropertyItem { ComponentType = AnimationPropertyItem.k_SpecialAddButton };

        [SerializeField]
        private CanvasGroup m_canvasGroup = null;

        [SerializeField]
        private GameObject m_blockUI = null;

        private bool m_isStarted;

        public int m_selectedClipIndex;
        public RuntimeAnimationClip SelectedClip
        {
            get
            {
                if(m_target == null || m_target.Clips == null || m_selectedClipIndex < 0 || m_selectedClipIndex >= m_target.Clips.Length)
                {
                    return null;
                }

                return m_target.Clips[m_selectedClipIndex];
            }
            set
            {
                if (m_target == null || m_target.Clips == null || m_selectedClipIndex < 0 || m_selectedClipIndex >= m_target.Clips.Length)
                {
                    return;
                }

                m_target.Clips[m_selectedClipIndex] = value;
            }
        }

        private RuntimeAnimation m_target;
        public RuntimeAnimation Target
        {
            get { return m_target; }
            set
            {
                if(value == null)
                {
                    if (m_previewToggle != null)
                    {
                        m_previewToggle.isOn = false;
                    }

                    if (m_playToggle != null)
                    {
                        m_playToggle.isOn = false;
                    }

                    if (m_frameInput != null)
                    {
                        m_frameInput.text = "0";
                    }

                    if(m_samplesInput != null)
                    {
                        m_samplesInput.text = "60";
                    }

                    if(m_dopesheetToggle != null)
                    {
                        m_dopesheetToggle.isOn = true;
                    }

                    if(m_animationsDropDown != null)
                    {
                        m_animationsDropDown.ClearOptions();
                    }
                }

                m_target = value;
                if (m_target == null || m_target.Clips == null || m_target.Clips.Length == 0)
                {
                    m_selectedClipIndex = -1;
                }
                else
                {
                    m_selectedClipIndex = -1;
                    for(int i = 0; i < m_target.Clips.Length; i++)
                    {
                        if(m_target.Clips[i] != null)
                        {
                            m_selectedClipIndex = i;
                            break;
                        }
                    }

                    if (m_animationsDropDown != null)
                    {
                        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
                        for (int i = 0; i < m_target.Clips.Length; ++i)
                        {
                            if (m_target.Clips[i] == null)
                            {
                                options.Add(new TMP_Dropdown.OptionData("(Missing)"));
                            }
                            else
                            {
                                options.Add(new TMP_Dropdown.OptionData(m_target.Clips[i].name));
                            }
                        }
                        m_animationsDropDown.options = options;
                        m_animationsDropDown.value = m_selectedClipIndex;
                    }
                }

                if(m_canvasGroup != null)
                {
                    m_canvasGroup.alpha = m_target != null ? 1 : 0.5f;
                }

                if(m_blockUI != null)
                {
                    m_blockUI.SetActive(m_target == null);

                    if(m_target == null)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                    }
                }

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

            UnityEventHelper.AddListener(m_previewToggle, toggle => toggle.onValueChanged, OnPreviewToggleValueChanged);
            UnityEventHelper.AddListener(m_recordToggle, toggle => toggle.onValueChanged, OnRecordToggleValueChanged);
            UnityEventHelper.AddListener(m_firstFrameButton, button => button.onClick, OnFirstFrameButtonClick);
            UnityEventHelper.AddListener(m_prevFrameButton, button => button.onClick, OnPrevFrameButtonClick);
            UnityEventHelper.AddListener(m_playToggle, toggle => toggle.onValueChanged, OnPlayToggleValueChanged);
            UnityEventHelper.AddListener(m_nextFrameButton, button => button.onClick, OnNextFrameButtonClick);
            UnityEventHelper.AddListener(m_lastFrameButton, button => button.onClick, OnLastFrameButtonClick);
            UnityEventHelper.AddListener(m_frameInput, input => input.onEndEdit, OnFrameInputEndEdit);
            UnityEventHelper.AddListener(m_animationsDropDown, dropdown => dropdown.onValueChanged, OnAnimationsDropdownValueChanged);
            UnityEventHelper.AddListener(m_samplesInput, input => input.onEndEdit, OnSamplesInputEndEdit);
            UnityEventHelper.AddListener(m_addKeyframeButton, button => button.onClick, OnAddKeyframeButtonClick);
            UnityEventHelper.AddListener(m_addEventButton, button => button.onClick, OnAddEventButtonClick);
            UnityEventHelper.AddListener(m_dopesheetToggle, toggle => toggle.onValueChanged, OnDopesheetToggleValueChanged);
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

            UnityEventHelper.RemoveListener(m_previewToggle, toggle => toggle.onValueChanged, OnPreviewToggleValueChanged);
            UnityEventHelper.RemoveListener(m_recordToggle, toggle => toggle.onValueChanged, OnRecordToggleValueChanged);
            UnityEventHelper.RemoveListener(m_firstFrameButton, button => button.onClick, OnFirstFrameButtonClick);
            UnityEventHelper.RemoveListener(m_prevFrameButton, button => button.onClick, OnPrevFrameButtonClick);
            UnityEventHelper.RemoveListener(m_playToggle, toggle => toggle.onValueChanged, OnPlayToggleValueChanged);
            UnityEventHelper.RemoveListener(m_nextFrameButton, button => button.onClick, OnNextFrameButtonClick);
            UnityEventHelper.RemoveListener(m_lastFrameButton, button => button.onClick, OnLastFrameButtonClick);
            UnityEventHelper.RemoveListener(m_frameInput, input => input.onEndEdit, OnFrameInputEndEdit);
            UnityEventHelper.RemoveListener(m_animationsDropDown, dropdown => dropdown.onValueChanged, OnAnimationsDropdownValueChanged);
            UnityEventHelper.RemoveListener(m_samplesInput, input => input.onEndEdit, OnSamplesInputEndEdit);
            UnityEventHelper.RemoveListener(m_addKeyframeButton, button => button.onClick, OnAddKeyframeButtonClick);
            UnityEventHelper.RemoveListener(m_addEventButton, button => button.onClick, OnAddEventButtonClick);
            UnityEventHelper.RemoveListener(m_dopesheetToggle, toggle => toggle.onValueChanged, OnDopesheetToggleValueChanged);
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
                m_properties.Insert(m_properties.Count - 1, propertyItem);
                addedProperties.Add(propertyItem);
                addedIndexes.Add(m_properties.Count - 1);
                if (propertyItem.Children != null)
                {
                    for(int i = 0; i < propertyItem.Children.Count; i++)
                    {
                        m_properties.Insert(m_properties.Count - 1, propertyItem.Children[i]);
                        addedProperties.Add(propertyItem.Children[i]);
                        addedIndexes.Add(m_properties.Count - 1);
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

        private void OnPreviewToggleValueChanged(bool value)
        {

        }

        private void OnRecordToggleValueChanged(bool value)
        {

        }

        private void OnFirstFrameButtonClick()
        {

        }

        private void OnPrevFrameButtonClick()
        {

        }

        private void OnPlayToggleValueChanged(bool value)
        {

        }

        private void OnNextFrameButtonClick()
        {

        }

        private void OnLastFrameButtonClick()
        {

        }

        private void OnFrameInputEndEdit(string value)
        {

        }

        private void OnAnimationsDropdownValueChanged(int value)
        {

        }

        private void OnSamplesInputEndEdit(string value)
        {

        }

        private void OnAddKeyframeButtonClick()
        {

        }

        private void OnAddEventButtonClick()
        {

        }

        private void OnDopesheetToggleValueChanged(bool value)
        {

        }
    }
}
