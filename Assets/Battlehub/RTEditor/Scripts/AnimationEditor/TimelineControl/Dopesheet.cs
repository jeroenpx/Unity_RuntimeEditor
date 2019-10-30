using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTEditor
{
    public class Dopesheet : MonoBehaviour
    {
        public class AnimationClip
        {
            public int VisibleRowsCount
            {
                get;
                private set;
            }

            public int RowsCount
            {
                get { return m_rows.Count; }
            }
            
            public int ColsCount
            {
                get;
                private set;
            }

            public int FirstSample
            {
                get;
                private set;
            }

            public int LastSample
            {
                get;
                private set;
            }

            private readonly Dictionary<int, DopesheetRow> m_visibleIndexToRow = new Dictionary<int, DopesheetRow>();
            private readonly List<DopesheetRow> m_rows = new List<DopesheetRow>();

            private readonly List<Keyframe> m_keyframes = new List<Keyframe>();
            private readonly List<Keyframe> m_selectedKeyframes = new List<Keyframe>();

            private readonly Dictionary<int, Keyframe> m_kfDictionary = new Dictionary<int, Keyframe>();
            private readonly Dictionary<int, Keyframe> m_selectedKfDictionary = new Dictionary<int, Keyframe>();

            public IList<DopesheetRow> Rows
            {
                get { return m_rows; }
            }

            public IList<Keyframe> Keyframes
            {
                get { return m_keyframes; }
            }

            public IList<Keyframe> SelectedKeyframes
            {
                get { return m_selectedKeyframes; }
            }

            public DopesheetRow GetRowByVisibleIndex(int visibleIndex)
            {
                DopesheetRow result;
                if(!m_visibleIndexToRow.TryGetValue(visibleIndex, out result))
                {
                    return null;
                }
                return result;
            }
            
            public bool IsSelected(int row, int col)
            {
                int key = row * ColsCount + col;
                return m_selectedKfDictionary.ContainsKey(key);
            }

            public bool HasKeyframe(int row, int col)
            {
                int key = row * ColsCount + col;
                return m_selectedKfDictionary.ContainsKey(key) || m_kfDictionary.ContainsKey(key);
            }

            public Keyframe GetSelectedKeyframe(int row, int col)
            {
                int key = row * ColsCount + col;
                Keyframe result;
                if (!m_selectedKfDictionary.TryGetValue(key, out result))
                {
                    return null;
                }
                return result;
            }

            public Keyframe GetKeyframe(int row, int col)
            {
                int key = row * ColsCount + col;
                Keyframe result;
                if(!m_kfDictionary.TryGetValue(key, out result))
                {
                    return null;
                }
                return result;
            }

            public void AddKeyframes(params Keyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    Keyframe kf = keyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;
                    m_kfDictionary.Add(key, kf);
                    m_keyframes.Add(kf);
                    kf.Row.Keyframes.Add(kf);

                    if(kf.Col > LastSample)
                    {
                        LastSample = kf.Col;
                    }
                    if(kf.Col < FirstSample)
                    {
                        FirstSample = kf.Col;
                    }
                }
            }

            public void ClearKeyframes()
            {
                m_keyframes.Clear();
                m_kfDictionary.Clear();

                foreach(DopesheetRow row in m_rows)
                {
                    row.Keyframes.Clear();
                }
            }

            public void ClearSelectedKeyframes()
            {
                m_selectedKeyframes.Clear();
                m_selectedKfDictionary.Clear();

                foreach (DopesheetRow row in m_rows)
                {
                    row.SelectedKeyframes.Clear();
                }
            }

            public void RemoveKeyframes(bool all, params Keyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    Keyframe kf = keyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;
                    
                    if(all)
                    {
                        if (m_kfDictionary.TryGetValue(key, out kf))
                        {
                            m_keyframes.Remove(kf);
                            m_kfDictionary.Remove(key);
                            kf.Row.Keyframes.Remove(kf);
                        }
                        else if (m_selectedKfDictionary.TryGetValue(key, out kf))
                        {
                            m_selectedKeyframes.Remove(kf);
                            m_selectedKfDictionary.Remove(key);
                            kf.Row.SelectedKeyframes.Remove(kf);
                        }
                    }
                    else
                    {
                        if (m_kfDictionary.TryGetValue(key, out kf))
                        {
                            m_keyframes.Remove(kf);
                            m_kfDictionary.Remove(key);
                            kf.Row.Keyframes.Remove(kf);
                        }
                    }  
                }
            }

            public void SelectKeyframes(params Keyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    Keyframe kf = keyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;

                    if (m_kfDictionary.TryGetValue(key, out kf))
                    {
                        SelectKeyframe(kf, key);
                        DopesheetRow dopesheetRow = kf.Row;
                        while(dopesheetRow.Parent != null)
                        {
                            Keyframe parentKf;
                            int parentKey = dopesheetRow.Parent.Index * ColsCount + kf.Col;
                            if (m_kfDictionary.TryGetValue(parentKey, out parentKf))
                            {
                                SelectKeyframe(parentKf, parentKey);
                            }
                            dopesheetRow = dopesheetRow.Parent;
                        }

                        dopesheetRow = kf.Row;
                        if (dopesheetRow.Children != null)
                        {
                            List<Keyframe> childKeyframes = new List<Keyframe>();
                            for (int c = 0; c < dopesheetRow.Children.Count; ++c)
                            {
                                DopesheetRow childRow = dopesheetRow.Children[c];
                                Keyframe childKeyframe = GetKeyframe(childRow.Index, kf.Col);
                                if (childKeyframe != null)
                                {
                                    childKeyframes.Add(childKeyframe);
                                }
                            }

                            SelectKeyframes(childKeyframes.ToArray());
                        }
                    }
                }
            }

            private void SelectKeyframe(Keyframe kf, int key)
            {
                m_keyframes.Remove(kf);
                m_kfDictionary.Remove(key);
                kf.Row.Keyframes.Remove(kf);

                m_selectedKfDictionary.Add(key, kf);
                m_selectedKeyframes.Add(kf);

                DopesheetRow dopesheetRow = kf.Row;
                dopesheetRow.SelectedKeyframes.Add(kf);
            }

            public void UnselectKeyframes(params Keyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    Keyframe kf = keyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;

                    if (m_selectedKfDictionary.TryGetValue(key, out kf))
                    {
                        m_selectedKeyframes.Remove(kf);
                        m_selectedKfDictionary.Remove(key);
                        kf.Row.SelectedKeyframes.Remove(kf);
                    }
                    else
                    {
                        continue;
                    }

                    m_kfDictionary.Add(key, kf);
                    m_keyframes.Add(kf);
                    kf.Row.Keyframes.Add(kf);
                }
            }

            public void AddRow(bool isVisible, int parentIndex)
            {
                DopesheetRow row = new DopesheetRow();
                row.IsVisible = isVisible;
                row.Index = m_rows.Count;
                m_rows.Add(row);
                
                if(parentIndex > -1)
                {
                    row.Parent = m_rows[parentIndex];
                    if(row.Parent.Children == null)
                    {
                        row.Parent.Children = new List<DopesheetRow>();
                    }

                    row.Parent.Children.Add(row);
                }

                Keyframe kf0 = new Keyframe(row, 0);
                Keyframe kf1 = new Keyframe(row, ColsCount - 1);
                AddKeyframes(kf0, kf1);

                UpdateRowIndexes();
            }

            public bool RemoveRow(int row)
            {
                DopesheetRow dopesheetRow = m_rows[row];
                if(dopesheetRow.Parent != null)
                {
                    dopesheetRow.Parent.Children.Remove(dopesheetRow);
                    if(dopesheetRow.Parent.Children.Count == 0)
                    {
                        dopesheetRow.Parent.Children = null;
                    }
                }
                m_rows.RemoveAt(row);
                UpdateRowIndexes();
                return dopesheetRow.IsVisible;
            }

            public void Expand(int row, int count)
            {
                for(int i = row + 1; i <= row + count; ++i)
                {
                    m_rows[i].IsVisible = true;
                }
                UpdateRowIndexes();
            }

            public void Collapse(int row, int count)
            {
                for (int i = row + 1; i <= row + count; ++i)
                {
                    m_rows[i].IsVisible = false;
                }
                UpdateRowIndexes();
            }

            private void UpdateRowIndexes()
            {
                m_visibleIndexToRow.Clear();

                for (int i = 0; i < m_rows.Count; ++i)
                {
                    m_rows[i].Index = i;
                }

                VisibleRowsCount = 0;
                for (int i = 0; i < m_rows.Count; ++i)
                {
                    if (m_rows[i].IsVisible)
                    {
                        m_rows[i].VisibleIndex = VisibleRowsCount;
                        m_visibleIndexToRow.Add(VisibleRowsCount, m_rows[i]);
                        VisibleRowsCount++;
                    }
                    else
                    {
                        m_rows[i].VisibleIndex = -1;
                    }
                }
            }

            public void ResizeClip(IList<Keyframe> keyframes)
            {
                for (int i = 0; i < keyframes.Count; ++i)
                {
                    Keyframe kf = keyframes[i];
                    if (ColsCount <= kf.Col || RowsCount <= kf.Row.Index)
                    {
                        ColsCount = Mathf.Max(ColsCount, kf.Col + 1);
                        RefreshDictionaries();
                    }
                }
            }

            public void Refresh(bool dictonaries = true, bool firstAndLastSample = true)
            {
                if(dictonaries)
                {
                    RefreshDictionaries();
                }

                if(firstAndLastSample)
                {
                    RefreshFirstAndLastSample();
                }
            }

            private void RefreshDictionaries()
            {
                m_kfDictionary.Clear();
                m_selectedKfDictionary.Clear();

                for (int i = 0; i < m_keyframes.Count; ++i)
                {
                    Keyframe kf = m_keyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;

                    m_kfDictionary.Add(key, kf);
                }

                for (int i = 0; i < m_selectedKeyframes.Count; ++i)
                {
                    Keyframe kf = m_selectedKeyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;

                    m_selectedKfDictionary.Add(key, kf);
                }
            }

            private void RefreshFirstAndLastSample()
            {
                if (m_rows.Count == 0)
                {
                    FirstSample = 0;
                    LastSample = 0;
                }
                else
                {
                    int min = ColsCount;
                    int max = 0;
                    for (int i = 0; i < m_keyframes.Count; ++i)
                    {
                        Keyframe keyframe = m_keyframes[i];
                        if (keyframe.Col < min)
                        {
                            min = keyframe.Col;
                        }
                        if (keyframe.Col > max)
                        {
                            max = keyframe.Col;
                        }
                    }

                    for (int i = 0; i < m_selectedKeyframes.Count; ++i)
                    {
                        Keyframe keyframe = m_selectedKeyframes[i];
                        if (keyframe.Col < min)
                        {
                            min = keyframe.Col;
                        }
                        if (keyframe.Col > max)
                        {
                            max = keyframe.Col;
                        }
                    }
                }
            }

            public AnimationClip(int rows, int columns)
            {
               // RowsCount = rows;
                ColsCount = columns;
            }

        }

        public class DopesheetRow
        {
            public int Index;
            public int VisibleIndex;
            public readonly List<Keyframe> Keyframes = new List<Keyframe>();
            public readonly List<Keyframe> SelectedKeyframes = new List<Keyframe>();
            public bool IsVisible = true;

            public DopesheetRow Parent;
            public List<DopesheetRow> Children;
        }

        public class Keyframe
        {
            public DopesheetRow Row;
            public int Col;
            
            public Keyframe(DopesheetRow row, int col)
            {
                Row = row;
                Col = col;
            }
        }


        [SerializeField]
        private Mesh m_quad = null;

        [SerializeField]
        private Material m_material = null;

        [SerializeField]
        private Material m_selectionMaterial = null;

        private CommandBuffer m_commandBuffer;
        private Camera m_camera;

        private TimelineGridParameters m_parameters = null;

        private const int k_batchSize = 512;
        private Matrix4x4[] m_matrices = new Matrix4x4[k_batchSize];

        private AnimationClip m_clip;
        public AnimationClip Clip
        {
            get { return m_clip; }
            set { m_clip = value; }
        }

        private void OnDestroy()
        {
            if (m_camera != null)
            {
                m_camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
            }
        }

        public void Init(Camera camera)
        {
            if (m_camera != null)
            {
                m_camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
            }

            if (m_commandBuffer == null)
            {
                m_commandBuffer = new CommandBuffer();
            }

            m_camera = camera;
            m_camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
        }

        public void SetGridParameters(TimelineGridParameters parameters)
        {
            Vector2 maxSupportedViewportSize = new Vector2(4096, 4096);
            SetGridParameters(parameters, maxSupportedViewportSize);
        }

        public void SetGridParameters(TimelineGridParameters parameters, Vector2 viewportSize)
        {
            m_commandBuffer.Clear();

            m_parameters = parameters;
        }

        public void UpdateGraphics(Vector2 viewportSize, Vector2 contentSize, Vector2 normalizedOffset, Vector2 normalizedSize, Vector2 interval)
        {
            if (m_parameters == null)
            {
                throw new System.InvalidOperationException("Call SetGridParameters method first");
            }

            m_commandBuffer.Clear();

            int vLinesCount = m_parameters.VertLines;
            int hLinesCount = m_parameters.HorLines;
            Color lineColor = m_parameters.LineColor;

            int index = 0;
          //  int cols = m_clip.Cols;
            int rows = m_clip.RowsCount;

            float rowHeight = m_parameters.FixedHeight / viewportSize.y;
            float aspect = viewportSize.x / viewportSize.y;

            int vLinesSq = m_parameters.VertLinesSecondary * m_parameters.VertLinesSecondary;
            int hLinesSq = m_parameters.HorLinesSecondary * m_parameters.HorLinesSecondary;

            Vector2 contentScale = new Vector2(
                1.0f / normalizedSize.x,
                1.0f / normalizedSize.y);

            Vector3 offset = new Vector3(-0.5f, 0.5f - rowHeight * 0.5f, 1.0f);
            offset.x -= ((1 - normalizedSize.x) * normalizedOffset.x / normalizedSize.x);
            offset.x *= aspect;
            offset.y += ((1 - normalizedSize.y) * (1 - normalizedOffset.y) / normalizedSize.y);

            float px = interval.x * normalizedSize.x;
            float visibleColumns = m_parameters.VertLines * Mathf.Pow(m_parameters.VertLinesSecondary, Mathf.Log(px, m_parameters.VertLinesSecondary));
            float offsetColumns = -(1 - 1 / normalizedSize.x) * normalizedOffset.x * visibleColumns;

            //Debug.Log(offsetColumns + " " + visibleColumns);

            Vector3 keyframeScale = Vector3.one * rowHeight * 0.5f;

            UpdateKeyframes(false, m_clip.Rows, m_material, index, rowHeight, aspect, offset, visibleColumns, keyframeScale);
            UpdateKeyframes(true, m_clip.Rows, m_selectionMaterial, index, rowHeight, aspect, offset, visibleColumns, keyframeScale);
        }

        private void UpdateKeyframes(bool selected, IList<DopesheetRow> rows, Material material, int index, float rowHeight, float aspect, Vector3 offset, float visibleColumns, Vector3 keyframeScale)
        {
            int rowNumber = 0;
            for(int i = 0; i < rows.Count; ++i)
            {
                DopesheetRow row = rows[i];
                if(row.IsVisible)
                {
                    List<Keyframe> keyframes = selected ? row.SelectedKeyframes : row.Keyframes;
                    for (int j = 0; j < keyframes.Count; ++j)
                    {
                        Keyframe keyframe = keyframes[j];
                        m_matrices[index] = Matrix4x4.TRS(
                            offset + new Vector3(aspect * keyframe.Col / visibleColumns, -rowHeight * rowNumber, 1),
                            Quaternion.Euler(0, 0, 45),
                            keyframeScale);

                        index++;
                        if (index == k_batchSize)
                        {
                            index = 0;
                            m_commandBuffer.DrawMeshInstanced(m_quad, 0, material, 0, m_matrices, k_batchSize);
                        }
                    }

                    rowNumber++;
                }
            }

            if (0 < index && index < k_batchSize)
            {
                m_commandBuffer.DrawMeshInstanced(m_quad, 0, material, 0, m_matrices, index);
            }
        }
    }
}
