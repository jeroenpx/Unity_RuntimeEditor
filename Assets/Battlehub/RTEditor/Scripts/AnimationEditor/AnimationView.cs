using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class AnimationView : RuntimeWindow
    {
        private AnimationPropertiesView m_propertiesView;
        private AnimationTimelineView m_timelineView;
        private AnimationCreateView m_animationCreateView;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Animation;
            base.AwakeOverride();

            m_propertiesView = GetComponentInChildren<AnimationPropertiesView>(true);
            m_propertiesView.PropertiesAdded += OnPropertiesAdded;
            m_propertiesView.PropertiesRemoved += OnPropertiesRemoved;

            m_timelineView = GetComponentInChildren<AnimationTimelineView>(true);
            m_animationCreateView = GetComponentInChildren<AnimationCreateView>(true);
            m_animationCreateView.Click += OnCreateClick;

            OnSelectionChanged(null);

            Editor.Selection.SelectionChanged += OnSelectionChanged;
        }


        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(Editor != null)
            {
                Editor.Selection.SelectionChanged -= OnSelectionChanged;
            }

            if(m_propertiesView != null)
            {
                m_propertiesView.PropertiesAdded -= OnPropertiesAdded;
                m_propertiesView.PropertiesRemoved -= OnPropertiesRemoved;
            }

            if(m_animationCreateView != null)
            {
                m_animationCreateView.Click -= OnCreateClick;
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            if (Editor.Selection.activeGameObject != null)
            {
                RuntimeAnimation animation = Editor.Selection.activeGameObject.GetComponent<RuntimeAnimation>();
                m_propertiesView.Target = animation;
                m_timelineView.Target = animation;
            }
            else
            {
                m_propertiesView.Target = null;
                m_timelineView.Target = null;
            }

            UpdateVisualState();
        }

        private void OnCreateClick()
        {
            GameObject go = Editor.Selection.activeGameObject;
            RuntimeAnimation animation = go.GetComponent<RuntimeAnimation>();

            if(animation == null)
            {
                animation = go.AddComponent<RuntimeAnimation>();
            }

            RuntimeAnimationClip clip = ScriptableObject.CreateInstance<RuntimeAnimationClip>();
            clip.name = "New Animation Clip";

            if (animation.Clips == null || animation.Clips.Length == 0)
            {
                animation.Clips = new[] { clip };
                m_propertiesView.Target = animation;
                m_timelineView.Target = animation;
            }
            else
            {
                Debug.Assert(m_propertiesView.Target != null);
                m_propertiesView.SelectedClip = clip;
            }

            UpdateVisualState();
        }

        private void OnPropertiesAdded(AnimationPropertiesView.ItemsArgs args)
        {
            m_timelineView.AddProperites(args.Items);
        }

        private void OnPropertiesRemoved(AnimationPropertiesView.ItemsArgs args)
        {
            m_timelineView.RemoveProperties(args.Items);
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

                if (m_propertiesView.SelectedClip == null)
                {
                    m_animationCreateView.Text = string.Format("To begin animating {0}, create a RuntimeAnimation Clip", Editor.Selection.activeGameObject.name);
                }

                if (animation != null && m_propertiesView.SelectedClip != null)
                {
                    m_animationCreateView.gameObject.SetActive(false);
                }
                else
                {
                    m_animationCreateView.gameObject.SetActive(true);
                    m_propertiesView.Target = null;
                }
            }
        }


    }
}

