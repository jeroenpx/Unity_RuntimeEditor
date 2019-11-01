using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class RuntimeAnimationClip : ScriptableObject
    {
        private readonly List<RuntimeAnimationProperty> m_properties;
        private AnimationClip m_clip;

        public AnimationClip Clip
        {
            get { return m_clip; }
        }

        public ICollection<RuntimeAnimationProperty> Properties
        {
            get { return m_properties; }
        }

        public void Add(RuntimeAnimationProperty property)
        {
            m_properties.Add(property);

            if(property.Children != null)
            {
                for(int i = 0; i < property.Children.Count; ++i)
                {
                    SetCurve(property.Children[i]);
                }
            }
            else
            {
                SetCurve(property);
            }
        }

        public void Remove(RuntimeAnimationProperty property)
        {
            m_properties.Remove(property);
            ClearCurve(property);
        }

        private void SetCurve(RuntimeAnimationProperty property)
        {
            Type componentType = property.ComponentType;
            if (componentType != null && property.Children == null)
            {
                m_clip.SetCurve("", componentType, property.PropertyPath, property.Curve);
            }
        }

        private void ClearCurve(RuntimeAnimationProperty property)
        {
            Type componentType = property.ComponentType;
            //if (componentType != null && property.Children == null)
            if (componentType != null && property.Parent == null)
            {
                m_clip.SetCurve("", componentType, property.PropertyPath, null);
            }
        }

        public RuntimeAnimationClip()
        {
            m_properties = new List<RuntimeAnimationProperty>();
        }

        private void OnEnable()
        {
            m_clip = new AnimationClip();
            m_clip.legacy = true;
        }

        public void Refresh()
        {
            for(int i = 0; i < m_properties.Count; ++i)
            {
                RuntimeAnimationProperty property = m_properties[i];
                if (property.Children != null)
                {
                    for (int j = 0; j < property.Children.Count; ++j)
                    {
                        SetCurve(property.Children[j]);
                    }
                }
                else
                {
                    SetCurve(property);
                }
            }
        }
    }

    public class RuntimeAnimation : MonoBehaviour
    {
        private Animation m_animation;

        public int ClipIndex = -1;

        private readonly List<RuntimeAnimationClip> m_rtClips = new List<RuntimeAnimationClip>();
        public IList<RuntimeAnimationClip> Clips
        {
            get { return m_rtClips; }
        }

        private bool m_isPlaying;
        public bool IsPlaying
        {
            get { return m_isPlaying; }
            set
            {
                if(m_isPlaying != value)
                {
                    m_isPlaying = value;
                    if (m_isPlaying)
                    {
                        if (ClipIndex >= 0 && ClipIndex < m_rtClips.Count)
                        {
                            RuntimeAnimationClip clip = m_rtClips[ClipIndex];
                            m_animation.Play(clip.name);
                        }
                    }
                    else
                    {
                        m_animation.Stop();
                    }
                }
            }
        }
        
        public void AddClip(RuntimeAnimationClip rtClip)
        {
            if(m_rtClips.Count == 0)
            {
                ClipIndex = 0;
            }

            m_rtClips.Add(rtClip);
            m_animation.AddClip(rtClip.Clip, rtClip.name);
        }

        public void RemoveClip(RuntimeAnimationClip rtClip)
        {
            int index = m_rtClips.IndexOf(rtClip);
            if(index == ClipIndex)
            {
                m_animation.Stop();
            }

            if(index >= 0)
            {
                RemoveClipAt(index);
            }

            if(ClipIndex >= m_rtClips.Count)
            {
                ClipIndex = m_rtClips.Count - 1;
            }
        }

        public void RemoveClipAt(int index)
        {
            RuntimeAnimationClip rtClip = m_rtClips[index];
            m_rtClips.RemoveAt(index);
            m_animation.RemoveClip(rtClip.Clip);
        }

        private void Awake()
        {
            m_animation = GetComponent<Animation>();
            if (m_animation == null)
            {
                m_animation = gameObject.AddComponent<Animation>();
            }

            //IEnumerator en = m_animation.GetEnumerator();
            //en.MoveNext();
            //m_animationState = (AnimationState)en.Current;
        }


        public void Refresh()
        {
            if(ClipIndex >= 0)
            {
                RuntimeAnimationClip clip = m_rtClips[ClipIndex];
                clip.Refresh();
                clip.Clip.wrapMode = WrapMode.Loop;

                m_animation.RemoveClip(clip.name);
                m_animation.AddClip(clip.Clip, clip.name);
            }
        }
    }
}

