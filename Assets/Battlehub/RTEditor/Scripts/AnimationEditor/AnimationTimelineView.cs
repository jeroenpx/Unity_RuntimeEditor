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

                Dopesheet.Clip = new Dopesheet.DsAnimationClip();
                Dopesheet.VisibleRowsCount = 1;

                if(m_clip != null)
                {
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

                    AddRows(addedIndexes.ToArray(), addedProperties.ToArray());
                }
                
                if (m_timeline != null)
                {
                    m_timeline.SetActive(m_clip != null);
                }
            }
        }

        public bool IsPlaying
        {
            get { return m_dopesheet.IsPlaying; }
            set { m_dopesheet.IsPlaying = value; }
        }

        private void Start()
        {
            Dopesheet.VisibleRowsCount = 1;
            Dopesheet.ClipModified += OnClipModified;
        }

        private void OnDestroy()
        {
            Dopesheet.ClipModified -= OnClipModified;
        }

        public void SetKeyframeValue(int row, RuntimeAnimationProperty property)
        {
            Dopesheet.SetKeyframeValue(row, Convert.ToSingle(property.Value));
        }

        public void AddRows(int[] rows, RuntimeAnimationProperty[] properties)
        {
            int parentIndex = -1;
            for(int i = 0; i < properties.Length; ++i)
            {
                RuntimeAnimationProperty property = properties[i];

                if(property.ComponentTypeName == RuntimeAnimationProperty.k_SpecialEmptySpace)
                {
                    Dopesheet.AddRow(true, -1, 0, null);
                }
                else
                {
                    bool isParent = property.Children != null && property.Children.Count > 0;
                    if(isParent)
                    {
                        parentIndex = rows[i];
                        Dopesheet.AddRow(true, 0, 0, null);
                    }
                    else
                    {
                        Dopesheet.AddRow(false, parentIndex, Convert.ToSingle(property.Value), property.Curve);
                    }
                }
            }
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
    }

}

