using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [DisallowMultipleComponent]
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
                m_clip.SetCurve("", componentType, property.AnimationPropertyPath, property.Curve);
            }
        }

        private void ClearCurve(RuntimeAnimationProperty property)
        {
            Type componentType = property.ComponentType;
            if (componentType != null && property.Parent == null)
            {
                m_clip.SetCurve("", componentType, property.AnimationPropertyPath, null);
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
            for (int i = 0; i < m_properties.Count; ++i)
            {
                RuntimeAnimationProperty property = m_properties[i];
                ClearCurve(property);

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

        private int m_clipIndex = -1;
        private List<RuntimeAnimationClip> m_rtClips = new List<RuntimeAnimationClip>();

        public int ClipIndex
        {
            get { return m_clipIndex; }
            set
            {
                if(m_clipIndex != value)
                {
                    IsPlaying = false;
                }
                m_clipIndex = value;
                Refresh();
            }
        }

        public int ClipsCount
        {
            get { return m_rtClips.Count; }
        }

        public List<RuntimeAnimationClip> Clips
        {
            get { return m_rtClips.ToList();  }
            set
            {
                if(value == null)
                {
                    if(m_rtClips != null)
                    {
                        for(int i = 0; i < m_rtClips.Count; ++i)
                        {
                            RuntimeAnimationClip clip = m_rtClips[i];
                            if(clip != null)
                            {
                                RemoveClip(clip);
                            }
                        }
                    }
                    m_rtClips = new List<RuntimeAnimationClip>();
                }
                else
                {
                    for (int i = 0; i < m_rtClips.Count; ++i)
                    {
                        RuntimeAnimationClip rtClip = m_rtClips[i];
                        if(rtClip != null)
                        {
                            if(m_animation.GetClip(rtClip.name) != null)
                            {
                                m_animation.RemoveClip(rtClip.name);
                            }
                        }
                    }

                    for(int i = 0; i < value.Count; ++i)
                    {
                        RuntimeAnimationClip rtClip = value[i];
                        if(rtClip != null)
                        {
                            m_animation.AddClip(rtClip.Clip, rtClip.name);
                        }
                    }

                    if(ClipIndex >= m_rtClips.Count)
                    {
                        ClipIndex = m_rtClips.Count - 1;
                    }

                    m_rtClips = value.ToList();
                }
            }
        }

        private AnimationState State
        {
            get
            {
                if(ClipIndex < 0)
                {
                    Debug.LogWarning("ClipIndex < 0");
                    return null;
                }

                RuntimeAnimationClip clip = m_rtClips[ClipIndex];
                if(clip == null)
                {
                    Debug.LogWarning("Clip == null");
                    return null;
                }

                if(!m_animation.isPlaying)
                {
                   // Debug.LogWarning("!m_animation.IsPlaying");
                    return null;
                }

                AnimationState state = m_animation[clip.name];
                if(state == null)
                {
                    Debug.LogWarning("state == null");
                    return null;
                }

                return state;
            }
        }
        
        public bool IsInPreviewMode
        {
            get { return m_animation.isPlaying; }
            set
            {
                if (value)
                {
                    if(!m_animation.isPlaying)
                    {
                        Refresh();
                    }
                }
                else
                {
                    m_animation.Stop();
                }
            }
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
                    if(m_isPlaying)
                    {
                        if(!m_animation.isPlaying)
                        {
                            Refresh();
                            if(!m_animation.isPlaying)
                            {
                                m_isPlaying = false;
                                return;
                            }
                            else
                            {
                                m_isPlaying = true;
                            }
                        }

                        AnimationState state = State;
                        if(state != null)
                        {
                            state.speed = 1;
                        }
                    }
                    else
                    {
                        AnimationState state = State;
                        if (state != null)
                        {
                            state.speed = 0;
                        }
                    }
                }
            }
        }

        public float NormalizedTime
        {
            get
            {
                AnimationState state = State;
                if(state != null)
                {
                    return state.normalizedTime;
                }
                return 0.0f;
            }
            set
            {
                if (!m_animation.isPlaying)
                {
                    Refresh();
                }

                AnimationState state = State;
                if (state != null)
                {
                    state.normalizedTime = value;
                }

            }
        }
        
        public void AddClip(RuntimeAnimationClip rtClip)
        {
            m_rtClips.Add(rtClip);
            m_animation.AddClip(rtClip.Clip, rtClip.name);

            if (m_rtClips.Count == 1)
            {
                ClipIndex = 0;
            }
        }
   
        public void RemoveClip(RuntimeAnimationClip rtClip)
        {
            int index = m_rtClips.IndexOf(rtClip);
            if (index == ClipIndex)
            {
                m_animation.Stop();
                m_isPlaying = false;
            }

            if (index >= 0)
            {
                m_rtClips.RemoveAt(index);

                if (m_animation.GetClip(rtClip.name) != null)
                {
                    m_animation.RemoveClip(rtClip.name);
                }
            }

            if (ClipIndex >= m_rtClips.Count)
            {
                ClipIndex = m_rtClips.Count - 1;
            }
        }


        private void Awake()
        {
            m_animation = GetComponent<Animation>();
            if (m_animation == null)
            {
                m_animation = gameObject.AddComponent<Animation>();
            }
        }

        public void Refresh()
        {
            if (ClipIndex >= 0)
            {
                RuntimeAnimationClip clip = m_rtClips[ClipIndex];
                if(clip == null)
                {
                    return;
                }
                clip.Refresh();
                clip.Clip.wrapMode = WrapMode.ClampForever;

                float speed = 0;
                float normalizedTime = 0;
                AnimationState animationState;
                if (m_animation.isPlaying)
                {
                    animationState = m_animation[clip.name];
                    if (animationState != null)
                    {
                        speed = animationState.speed;
                        normalizedTime = animationState.normalizedTime;
                    }
                }

                m_animation.Stop();
                m_animation.RemoveClip(clip.name);
                m_animation.AddClip(clip.Clip, clip.name);

                m_animation.Play(clip.name);

                if(m_animation.isPlaying)
                {
                    m_isPlaying = !clip.Clip.empty && speed > 0;
                    animationState = m_animation[clip.name];
                    animationState.speed = speed;
                    animationState.normalizedTime = normalizedTime;
                }
                else
                {
                    m_isPlaying = false;
                }
            }
        }

        public void SetClips(IList<RuntimeAnimationClip> clips, int currentClipIndex)
        {
            m_isPlaying = false;

            if (m_animation == null)
            {
                m_animation = gameObject.GetComponent<Animation>();
                m_animation.Stop();

                foreach (RuntimeAnimationClip clip in m_rtClips)
                {
                    m_animation.RemoveClip(clip.name);
                }
            }
            if(m_animation == null)
            {
                m_animation = gameObject.AddComponent<Animation>();
            }

            m_rtClips.Clear();

            if(clips != null)
            {
                foreach (RuntimeAnimationClip clip in clips)
                {
                    m_rtClips.Add(clip);
                }
                m_clipIndex = currentClipIndex;
            }
            else
            {
                m_clipIndex = -1;
            }
        }
    }
}

