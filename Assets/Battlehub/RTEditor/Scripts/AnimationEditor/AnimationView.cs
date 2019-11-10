using Battlehub.RTCommon;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Battlehub.UIControls;
using UnityEngine.EventSystems;
using System.Linq;
using Battlehub.RTHandles;
using System.Collections;

namespace Battlehub.RTEditor
{
    public class AnimationView : RuntimeWindow
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
        private CanvasGroup m_group = null;

        [SerializeField]
        private GameObject m_blockUI = null;

        private AnimationPropertiesView m_propertiesView;
        private AnimationTimelineView m_timelineView;
        private AnimationCreateView m_animationCreateView;

        private bool m_wasInPreviewMode;
        private float m_normalizedTime;
        private bool m_isTransforming;
        private int m_currentSample;
       
        private IEnumerable<RuntimeAnimationClip> NotNullClips
        {
            get
            {
                return Target.Clips.Where(c => c != null);
            }
        }

        private RuntimeAnimation m_target;
        public RuntimeAnimation Target
        {
            get { return m_target; }
            set
            {
                if (value == null)
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
                        m_currentSample = 0;
                    }

                    if (m_samplesInput != null)
                    {
                        m_samplesInput.text = "60";
                    }

                    if (m_dopesheetToggle != null)
                    {
                        m_dopesheetToggle.isOn = true;
                    }

                    if (m_animationsDropDown != null)
                    {
                        m_animationsDropDown.ClearOptions();
                    }
                }

                m_target = value;
                if (m_target == null || m_target.Clips == null || m_target.Clips.Count == 0)
                {
                    m_propertiesView.Target = null;
                    m_propertiesView.Clip = null;
                    m_timelineView.Clip = null;
                    m_timelineView.Animation = null;
                }
                else
                {
                    IList<RuntimeAnimationClip> clips = m_target.Clips;
                    for (int i = 0; i < clips.Count; i++)
                    {
                        if (clips != null)
                        {
                            m_propertiesView.Target = m_target.gameObject;
                            m_propertiesView.Clip = clips[i];
                            m_timelineView.Animation = m_target;
                            m_timelineView.Clip = clips[i];
                            break;
                        }
                    }

                    if (m_animationsDropDown != null)
                    {
                        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
                        RuntimeAnimationClip[] notNullClips = NotNullClips.ToArray();
                        for (int i = 0; i < notNullClips.Length; ++i)
                        {
                            options.Add(new TMP_Dropdown.OptionData(notNullClips[i].name));
                        }

                        if(notNullClips.Length > 0)
                        {
                            m_animationsDropDown.options = options;
                            m_animationsDropDown.value = 0;
                            m_target.ClipIndex = clips.IndexOf(notNullClips[0]);
                        }
                        else
                        {
                            m_target.ClipIndex = -1;
                        }
                    }
                }

                if (m_group != null)
                {
                    m_group.alpha = m_propertiesView.Clip != null ? 1 : 0.5f;
                }

                if (m_blockUI != null)
                {
                    m_blockUI.SetActive(m_propertiesView.Clip == null);

                    if (m_propertiesView.Clip == null)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                    }
                }
            }
        }

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Animation;
            base.AwakeOverride();

            m_propertiesView = GetComponentInChildren<AnimationPropertiesView>(true);
            m_propertiesView.PropertiesAdded += OnPropertiesAdded;
            m_propertiesView.PropertiesRemoved += OnPropertiesRemoved;
            m_propertiesView.PropertyExpanded += OnPropertyExpanded;
            m_propertiesView.PropertyCollapsed += OnPropertyCollapsed;
            m_propertiesView.PropertyValueChanged += OnPropertyValueChanged;
            
            m_timelineView = GetComponentInChildren<AnimationTimelineView>(true);
            m_timelineView.IsDopesheet = m_dopesheetToggle.isOn;
            m_animationCreateView = GetComponentInChildren<AnimationCreateView>(true);
            m_animationCreateView.Click += OnCreateClick;

            OnSelectionChanged(null);

            Editor.Selection.SelectionChanged += OnSelectionChanged;
            Editor.Object.ComponentAdded += OnComponentAdded;

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

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(Editor != null)
            {
                Editor.Selection.SelectionChanged -= OnSelectionChanged;
                if(Editor.Object != null)
                {
                    Editor.Object.ComponentAdded -= OnComponentAdded;
                }
            }

            if(m_propertiesView != null)
            {
                m_propertiesView.PropertiesAdded -= OnPropertiesAdded;
                m_propertiesView.PropertiesRemoved -= OnPropertiesRemoved;
                m_propertiesView.PropertyExpanded -= OnPropertyExpanded;
                m_propertiesView.PropertyCollapsed -= OnPropertyCollapsed;
                m_propertiesView.PropertyValueChanged -= OnPropertyValueChanged;
            }

            if (m_animationCreateView != null)
            {
                m_animationCreateView.Click -= OnCreateClick;
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

        protected override void UpdateOverride()
        {
            base.UpdateOverride();

            Object activeTool = Editor.Tools.ActiveTool;
            if(activeTool is BaseHandle)
            {
                if(!m_isTransforming && m_target)
                {
                    m_wasInPreviewMode = m_target.IsInPreviewMode;
                    m_normalizedTime = m_target.NormalizedTime;
                    m_target.IsInPreviewMode = false;
                }

                m_isTransforming = true;
            }
            else
            {
                if(m_isTransforming && m_target)
                {
                    if(m_recordToggle.isOn)
                    {
                        RecordAll();
                    }

                    if(m_wasInPreviewMode)
                    {
                        m_target.IsInPreviewMode = true;
                        m_target.NormalizedTime = m_normalizedTime;
                    }
                }
                m_isTransforming = false;
            }

            bool isInPreviewMode = m_target != null && m_target.IsInPreviewMode;
            if (m_previewToggle.isOn != isInPreviewMode)
            {
                m_previewToggle.isOn = isInPreviewMode;
            }

            bool isPlaying = m_target != null && m_target.IsPlaying;
            if(m_playToggle.isOn != isPlaying)
            {
                m_playToggle.isOn = isPlaying;
            }

            if(m_currentSample != m_timelineView.CurrentSample)
            {
                m_currentSample = m_timelineView.CurrentSample;
                m_frameInput.text = m_currentSample.ToString();
            }

            object t = Target;
            if (t != null && Target == null)
            {
                Target = null;
                UpdateVisualState();
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            UpdateTarget();
            UpdateVisualState();
        }

        private void OnComponentAdded(ExposeToEditor obj, Component addedComponent)
        {
            if(addedComponent is RuntimeAnimation && Editor.Selection.activeGameObject == obj.gameObject)
            {
                UpdateTarget();
                UpdateVisualState();
            }
        }

        private void UpdateTarget()
        {
            if (Editor.Selection.activeGameObject != null)
            {
                RuntimeAnimation animation = Editor.Selection.activeGameObject.GetComponent<RuntimeAnimation>();
                Target = animation;
            }
            else
            {
                Target = null;
            }
        }

        private void OnCreateClick()
        {
            IOC.Resolve<IWindowManager>().CreateDialogWindow(RuntimeWindowType.SaveAsset.ToString(), "Save Animation Clip",
                (sender, args) => {});

            ISaveAssetDialog saveAssetDialog = IOC.Resolve<ISaveAssetDialog>();
            RuntimeAnimationClip clip = ScriptableObject.CreateInstance<RuntimeAnimationClip>();
            clip.name = "New Animation Clip";
            
            saveAssetDialog.Asset = clip;
            saveAssetDialog.SaveCompleted += OnSaveCompleted;
        }

        private void OnSaveCompleted(ISaveAssetDialog sender, Object asset)
        {
            sender.SaveCompleted -= OnSaveCompleted;

            if(asset == null)
            {
                return;
            }

            RuntimeAnimationClip clip = (RuntimeAnimationClip)asset;
            GameObject go = Editor.Selection.activeGameObject;
            RuntimeAnimation animation = go.GetComponent<RuntimeAnimation>();
            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();

            if (animation == null)
            {
                if (exposeToEditor != null)
                {
                    Editor.Undo.AddComponent(exposeToEditor, typeof(RuntimeAnimation));
                    animation = exposeToEditor.GetComponent<RuntimeAnimation>();
                }
                else
                {
                    animation = go.AddComponent<RuntimeAnimation>();
                }
            }

            
            animation.AddClip(clip);
            animation.ClipIndex = animation.ClipsCount - 1;
            UpdateTarget();

            m_propertiesView.Target = animation.gameObject;
            m_propertiesView.Clip = clip;
            m_timelineView.Clip = clip;

            if (exposeToEditor != null && animation != null)
            {
                exposeToEditor.ReloadComponentEditor(animation, true);
            }

            animation.Refresh();
            UpdateVisualState();
        }

        private void OnPropertiesAdded(AnimationPropertiesView.ItemsArg args)
        {
            m_timelineView.AddRows(args.Rows, args.Items);
        }

        private void OnPropertiesRemoved(AnimationPropertiesView.ItemsArg args)
        {
            m_timelineView.RemoveRows(args.Rows, args.Items);
        }

        private void OnPropertyExpanded(AnimationPropertiesView.ItemArg args)
        {
            m_timelineView.ExpandRow(args.Row, args.Item);
        }

        private void OnPropertyCollapsed(AnimationPropertiesView.ItemArg args)
        {
            m_timelineView.CollapseRow(args.Row, args.Item);
        }

        private void OnPropertyValueChanged(AnimationPropertiesView.ItemArg args)
        {
            m_timelineView.SetKeyframeValue(args.Row, args.Item);
        }

        private void OnPreviewToggleValueChanged(bool value)
        {
            if(m_target != null)
            {
                m_target.IsInPreviewMode = value;
            }
        }

        private void OnRecordToggleValueChanged(bool value)
        {
            if(value)
            {
                Debug.Log("Recording...");
            }
        }

        private void OnFirstFrameButtonClick()
        {
            m_timelineView.FirstSample();
        }

        private void OnPrevFrameButtonClick()
        {
            m_timelineView.PrevSample();
        }

        private void OnPlayToggleValueChanged(bool value)
        {
            Target.IsPlaying = value;
        }

        private void OnNextFrameButtonClick()
        {
            m_timelineView.NextSample();
        }

        private void OnLastFrameButtonClick()
        {
            m_timelineView.LastSample();
        }

        private void OnFrameInputEndEdit(string value)
        {
            m_timelineView.SetSample(int.Parse(value));
        }

        private void OnAnimationsDropdownValueChanged(int value)
        {
            if(m_target != null)
            {
                RuntimeAnimationClip clip = NotNullClips.ToArray()[value];
                m_target.ClipIndex = m_target.Clips.IndexOf(clip);
            }

            UpdateVisualState();
        }

        private void OnSamplesInputEndEdit(string value)
        {

        }

        private void OnAddKeyframeButtonClick()
        {
            RuntimeAnimationProperty[] properties = m_propertiesView.SelectedProps;
            if (properties.Length == 0)
            {
                properties = m_propertiesView.Props.Where(p => p.ComponentType != null && (p.Children == null || p.Children.Count == 0)).ToArray();
            }

            Record(properties);
        }

        private void RecordAll()
        {
            Record(m_propertiesView.Props.Where(p => p.ComponentType != null && (p.Children == null || p.Children.Count == 0)).ToArray());
        }

        private void Record(RuntimeAnimationProperty[] properties)
        {
            m_timelineView.BeginSetKeyframeValues();
            for (int i = 0; i < properties.Length; ++i)
            {
                RuntimeAnimationProperty property = properties[i];
                if (property.Children == null || property.Children.Count == 0)
                {
                    int index = m_propertiesView.IndexOf(property);
                    m_timelineView.SetKeyframeValue(index, property);
                }
                else
                {
                    foreach (RuntimeAnimationProperty childProperty in property.Children)
                    {
                        int index = m_propertiesView.IndexOf(childProperty);
                        m_timelineView.SetKeyframeValue(index, childProperty);
                    }
                }
            }
            m_timelineView.EndSetKeyframeValues();
        }

        private void OnAddEventButtonClick()
        {

        }

        private void OnDopesheetToggleValueChanged(bool value)
        {
            m_timelineView.IsDopesheet = value;
        }

        private void UpdateVisualState()
        {
            if (Editor.Selection.activeGameObject == null)
            {
                m_animationCreateView.gameObject.SetActive(false);
            }
            else
            {
                RuntimeAnimation animation = Editor.Selection.activeGameObject.GetComponent<RuntimeAnimation>();
                if (animation == null)
                {
                    m_animationCreateView.Text = string.Format("To begin animating {0}, create a RuntimeAnimation and a RuntimeAnimation Clip", Editor.Selection.activeGameObject.name);
                }

                if (m_target == null)
                {
                    m_animationCreateView.Text = string.Format("To begin animating {0}, create a RuntimeAnimation Clip", Editor.Selection.activeGameObject.name);
                }

                if (animation != null && animation.ClipIndex >= 0 && animation.ClipIndex <= animation.ClipsCount && animation.Clips[animation.ClipIndex] != null)
                {
                    m_animationCreateView.gameObject.SetActive(false);
                }
                else
                {
                    m_animationCreateView.gameObject.SetActive(true);
                    Target = null;
                }
            }
        }
    }
}

