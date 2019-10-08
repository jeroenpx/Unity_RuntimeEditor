using Battlehub.RTCommon;
using Battlehub.UIControls;
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

        private readonly AnimationPropertyItem m_empty = new AnimationPropertyItem();

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
                return m_propertiesTreeView.Items.OfType<AnimationPropertyItem>().ToArray();
            }
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
                m_propertiesTreeView.SelectionChanged += OnPropertiesSelectionChanged;
                m_propertiesTreeView.ItemsRemoving += OnPropertiesItemRemoving;
                m_propertiesTreeView.ItemsRemoved += OnPropertiesItemRemoved;
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
                m_propertiesTreeView.SelectionChanged -= OnPropertiesSelectionChanged;
                m_propertiesTreeView.ItemsRemoving -= OnPropertiesItemRemoving;
                m_propertiesTreeView.ItemsRemoved -= OnPropertiesItemRemoved;
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
            m_propertiesTreeView.Items = new List<AnimationPropertyItem>
            {
                m_empty
            };
        }


        public void AddProperty(AnimationPropertyItem propertyItem)
        {
            if (propertyItem == null)
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
                propertyItem = new AnimationPropertyItem(propertyItem);

                propertyItem.Parent = null;
                propertyItem.Children = null;

                propertyItem.TryToCreateChildren();

                m_propertiesTreeView.Insert(m_propertiesTreeView.ItemsCount - 1, propertyItem);
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

        private void OnPropertiesItemRemoving(object sender, ItemsCancelArgs e)
        {
            e.Items.Remove(m_empty);   
        }

        private void OnPropertiesItemRemoved(object sender, ItemsRemovedArgs e)
        {
            foreach(AnimationPropertyItem item in e.Items)
            {
                if(item.Parent != null)
                {
                    m_propertiesTreeView.RemoveChild(null, item.Parent);
                }
            }
        }

        private void OnPropertiesItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            AnimationPropertyView ui = e.ItemPresenter.GetComponent<AnimationPropertyView>();
            AnimationPropertyItem item = (AnimationPropertyItem)e.Item;

            ui.View = this;
            if (m_empty != item)
            {
                ui.Item = item;
                e.CanSelect = true;
            }
            else
            {
                ui.Item = null;
                e.CanSelect = false;
            }

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
