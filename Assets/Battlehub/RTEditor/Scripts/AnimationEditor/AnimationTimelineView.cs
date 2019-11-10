using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class AnimationTimelineView : MonoBehaviour
    {
#pragma warning disable 0414
        [SerializeField]
        private GameObject m_timeline = null;

        public int CurrentSample
        {
            get
            {
                if(IsDopesheet)
                {
                    if(m_dopesheet == null)
                    {
                        return 0;
                    }

                    return m_dopesheet.CurrentSample;
                }

                if(m_curves == null)
                {
                    return 0;
                }

                return m_curves.CurrentSample;
            }
        }

        [SerializeField]
        private TimelineControl m_dopesheet = null;
        private ITimelineControl Dopesheet
        {
            get { return m_dopesheet; }
        }


        [SerializeField]
        private TimelineControl m_curves = null;
#pragma warning restore 0219
        public bool IsDopesheet
        {
            get;
            set;
        }

        public RuntimeAnimation Animation
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
                m_clip = value;

                m_dopesheet.SetSample(0);

                if (m_clip != null)
                {                    
                    AnimationClip clip = m_clip.Clip;
                    const int frameRate = 60;
                    clip.frameRate = frameRate;
                    int samplesCount = Mathf.CeilToInt(clip.length * clip.frameRate);

                    Dopesheet.Clip = new Dopesheet.DsAnimationClip(samplesCount, frameRate);
                    Dopesheet.VisibleRowsCount = 1;

                    List<RuntimeAnimationProperty> addedProperties = new List<RuntimeAnimationProperty>();
                    List<int> addedIndexes = new List<int>();
                    if (m_clip.Properties.Count > 0)
                    {
                        addedProperties.Add(new RuntimeAnimationProperty { ComponentTypeName = RuntimeAnimationProperty.k_SpecialEmptySpace });
                        addedIndexes.Add(0);
                    }
                    int index = 1;
                    foreach (RuntimeAnimationProperty property in m_clip.Properties)
                    {
                        addedProperties.Add(property);
                        addedIndexes.Add(index);
                        index++;

                        if (property.Children != null)
                        {
                            for (int i = 0; i < property.Children.Count; i++)
                            {
                                addedProperties.Add(property.Children[i]);
                                addedIndexes.Add(index);
                                index++;
                            }
                        }
                    }

                    AddRows(addedIndexes.ToArray(), addedProperties.ToArray(), false);
                    Dopesheet.Refresh();
                }
                else
                {
                    Dopesheet.Clip = new Dopesheet.DsAnimationClip();
                    Dopesheet.VisibleRowsCount = 1;
                }

                if (m_timeline != null)
                {
                    m_timeline.SetActive(m_clip != null);
                }
            }
        }

        private void Start()
        {
            Dopesheet.VisibleRowsCount = 1;
            Dopesheet.ClipModified += OnClipModified;
            Dopesheet.SampleChanged += OnSampleChanged;
        }

        private void OnDestroy()
        {
            if((Dopesheet as Component) != null)
            {
                Dopesheet.ClipModified -= OnClipModified;
                Dopesheet.SampleChanged -= OnSampleChanged;
            }
        }

        private void Update()
        {
            if(Animation != null && Animation.IsPlaying)
            {
                Dopesheet.SetNormalizedTime(Animation.NormalizedTime % 1, false);
                if(Animation.NormalizedTime > 1)
                {
                    Animation.NormalizedTime = 0;
                }
            }
        }

        public void BeginSetKeyframeValues()
        {
            Dopesheet.BeginSetKeyframeValues();
        }

        public void EndSetKeyframeValues()
        {
            Dopesheet.EndSetKeyframeValues();
        }

        public void SetKeyframeValue(int row, RuntimeAnimationProperty property)
        {
            Dopesheet.SetKeyframeValue(Convert.ToSingle(property.Value), row);
        }

        public void AddRows(int[] rows, RuntimeAnimationProperty[] properties, bool isNew = true)
        {
            int parentIndex = 0;
            for(int i = 0; i < properties.Length; ++i)
            {
                RuntimeAnimationProperty property = properties[i];

                if(property.ComponentTypeName == RuntimeAnimationProperty.k_SpecialEmptySpace)
                {
                    Dopesheet.AddRow(true, isNew, -1, 0, null);
                }
                else
                {
                    if(property.Parent == null)
                    {
                        if (property.Curve != null)
                        {
                            Dopesheet.AddRow(true, isNew, 0, Convert.ToSingle(property.Value), property.Curve);
                        }
                        else
                        {
                            parentIndex = rows[i];
                            Dopesheet.AddRow(true, isNew, 0, 0, null);
                        }
                    }
                    else
                    {
                        Dopesheet.AddRow(false, isNew, parentIndex, Convert.ToSingle(property.Value), property.Curve);
                    }
                }
            }

            if(!isNew)
            {
                float clipLength = Clip.Clip.length;
                Dopesheet.BeginSetKeyframeValues();
                for (int i = 0; i < properties.Length; ++i)
                {
                    RuntimeAnimationProperty property = properties[i];
                    if (property.ComponentTypeName == RuntimeAnimationProperty.k_SpecialEmptySpace)
                    {
                        continue;
                    }

                    AnimationCurve curve = property.Curve;
                    if (curve != null)
                    {
                        Keyframe[] keys = curve.keys;
                        for(int k = 0; k < keys.Length; ++k)
                        {
                            Keyframe kf = keys[k];

                            int sample = Mathf.RoundToInt(kf.time * Dopesheet.SamplesCount / clipLength);
                            Dopesheet.SetKeyframeValue(kf.value, rows[i], sample);
                        }
                    }
                }
                Dopesheet.EndSetKeyframeValues(false);
            }

            OnClipModified();
        }

        public void RemoveRows(int[] rows, RuntimeAnimationProperty[] properties)
        {
            for (int i = properties.Length - 1; i >= 0; --i)
            {
                Dopesheet.RemoveKeyframes(rows[i]);
            }

            for (int i = properties.Length - 1; i >= 0; --i)
            {
                Dopesheet.RemoveRow(rows[i]);
            }

            Dopesheet.Refresh(true, true, false);
        }
              
        public void ExpandRow(int row, RuntimeAnimationProperty property)
        {
            Dopesheet.Expand(row, property.Children.Count);
        }

        public void CollapseRow(int row, RuntimeAnimationProperty property)
        {
            Dopesheet.Collapse(row, property.Children.Count);
        }

        public void SetSample(int sampleNumber)
        {
            Dopesheet.SetSample(sampleNumber);
        }

        public void NextSample()
        {
            Dopesheet.NextSample();
        }

        public void PrevSample()
        {
            Dopesheet.PrevSample();
        }

        public void LastSample()
        {
            Dopesheet.LastSample();
        }

        public void FirstSample()
        {
            Dopesheet.FirstSample();
        }

        private void OnClipModified()
        {
            if(Animation != null)
            {
                Animation.Refresh();
            }
        }

        private void OnSampleChanged()
        {
            if(Animation != null)
            {
                Animation.NormalizedTime = Dopesheet.NormalizedTime;
            }
        }
    }

}

