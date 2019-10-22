using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTEditor
{
    public class Dopesheet : MonoBehaviour
    {
        public class AnimationClip
        {
            public int Rows;
            public int Cols;

            private readonly List<Keyframe> m_keyframes = new List<Keyframe>();
            private readonly List<Keyframe> m_selectedKeyframes = new List<Keyframe>();

            private readonly Dictionary<int, Keyframe> m_kfDictionary = new Dictionary<int, Keyframe>();
            private readonly Dictionary<int, Keyframe> m_selectedKfDictionary = new Dictionary<int, Keyframe>();

            public IList<Keyframe> Keyframes
            {
                get { return m_keyframes; }
            }

            public IList<Keyframe> SelectedKeyframes
            {
                get { return m_selectedKeyframes; }
            }

            public bool IsSelected(int row, int col)
            {
                int key = row * Cols + col;
                return m_selectedKfDictionary.ContainsKey(key);
            }

            public bool HasKeyframe(int row, int col)
            {
                int key = row * Cols + col;
                return m_selectedKfDictionary.ContainsKey(key) || m_kfDictionary.ContainsKey(key);
            }

            public Keyframe GetSelectedKeyframe(int row, int col)
            {
                int key = row * Cols + col;
                Keyframe result;
                if (!m_selectedKfDictionary.TryGetValue(key, out result))
                {
                    return null;
                }
                return result;
            }

            public Keyframe GetKeyframe(int row, int col)
            {
                int key = row * Cols + col;
                Keyframe result;
                if(!m_kfDictionary.TryGetValue(key, out result))
                {
                    return null;
                }
                return result;
            }


            public void UpdateDictionaries()
            {
                m_kfDictionary.Clear();
                m_selectedKfDictionary.Clear();

                for(int i = 0; i < m_keyframes.Count; ++i)
                {
                    Keyframe kf = m_keyframes[i];
                    int key = kf.Row * Cols + kf.Col;

                    m_kfDictionary.Add(key, kf);
                }

                for (int i = 0; i < m_selectedKeyframes.Count; ++i)
                {
                    Keyframe kf = m_selectedKeyframes[i];
                    int key = kf.Row * Cols + kf.Col;

                    m_selectedKfDictionary.Add(key, kf);
                }
            }

            public void TryResizeClip(IList<Keyframe> keyframes)
            {
                for (int i = 0; i < keyframes.Count; ++i)
                {
                    Keyframe kf = keyframes[i];
                    if (Cols <= kf.Col || Rows <= kf.Row)
                    {
                        Cols = Mathf.Max(Cols, kf.Col + 1);
                        Rows = Mathf.Max(Rows, kf.Row + 1);
                        UpdateDictionaries();
                    }
                }
            }

            public void AddKeyframes(params Keyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    Keyframe kf = keyframes[i];
                    int key = kf.Row * Cols + kf.Col;
                    m_kfDictionary.Add(key, kf);
                    m_keyframes.Add(kf);                    
                }
            }

            public void ClearKeyframes()
            {
                m_keyframes.Clear();
                m_kfDictionary.Clear();
            }

            public void ClearSelectedKeyframes()
            {
                m_selectedKeyframes.Clear();
                m_selectedKfDictionary.Clear();
            }

            public void RemoveKeyframes(bool all, params Keyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    Keyframe kf = keyframes[i];
                    int key = kf.Row * Cols + kf.Col;
                    
                    if(all)
                    {
                        if (m_kfDictionary.TryGetValue(key, out kf))
                        {
                            m_keyframes.Remove(kf);
                            m_kfDictionary.Remove(key);
                        }
                        else if (m_selectedKfDictionary.TryGetValue(key, out kf))
                        {
                            m_selectedKeyframes.Remove(kf);
                            m_selectedKfDictionary.Remove(key);
                        }
                    }
                    else
                    {
                        if (m_kfDictionary.TryGetValue(key, out kf))
                        {
                            m_keyframes.Remove(kf);
                            m_kfDictionary.Remove(key);
                        }
                    }  
                }
            }

            public void SelectKeyframes(params Keyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    Keyframe kf = keyframes[i];
                    int key = kf.Row * Cols + kf.Col;

                    if (m_kfDictionary.TryGetValue(key, out kf))
                    {
                        m_keyframes.Remove(kf);
                        m_kfDictionary.Remove(key);
                    }
                    
                    m_selectedKfDictionary.Add(key, kf);
                    m_selectedKeyframes.Add(kf);
                }
            }

            public void UnselectKeyframes(params Keyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    Keyframe kf = keyframes[i];
                    int key = kf.Row * Cols + kf.Col;

                    if (m_selectedKfDictionary.TryGetValue(key, out kf))
                    {
                        m_selectedKeyframes.Remove(kf);
                        m_selectedKfDictionary.Remove(key);
                    }

                    m_kfDictionary.Add(key, kf);
                    m_keyframes.Add(kf);
                }
            }
            

            public AnimationClip(int rows, int columns)
            {
                Rows = rows;
                Cols = columns;
            }

        }

        public class Keyframe
        {
            public int Row;
            public int Col;
            
            public Keyframe(int row, int col)
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

        //#warning replace array with more appropriate structure
        //        private Keyframe[,] m_keyframes = new Keyframe[0, 0];
        //        public Keyframe[,] Keyframes
        //        {
        //            get { return m_keyframes; }
        //            set { m_keyframes = value; }
        //        }


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
            int rows = m_clip.Rows;

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

            Vector3 keyframeScale = Vector3.one * rowHeight * 0.5f;

            //UpdateKeyframes(m_animationClip.Keyframes, m_material, index, cols, rows, rowHeight, aspect, offset, offsetColumns, visibleColumns, keyframeScale);
            //UpdateKeyframes(m_animationClip.SelectedKeyframes, m_selectionMaterial, index, cols, rows, rowHeight, aspect, offset, offsetColumns, visibleColumns, keyframeScale);
            UpdateKeyframes(m_clip.Keyframes, m_material, index, rowHeight, aspect, offset, visibleColumns, keyframeScale);
            UpdateKeyframes(m_clip.SelectedKeyframes, m_selectionMaterial, index, rowHeight, aspect, offset, visibleColumns, keyframeScale);
        }

        //private void UpdateKeyframes(List<Keyframe> keyframes, Material material, int index, int cols, int rows, float rowHeight, float aspect, Vector3 offset, float offsetColumns, float visibleColumns, Vector3 keyframeScale)
        private void UpdateKeyframes(IList<Keyframe> keyframes, Material material, int index, float rowHeight, float aspect, Vector3 offset, float visibleColumns, Vector3 keyframeScale)
        {
            //for (int i = 0; i < rows; ++i)
            //{
            //    for (int j = Mathf.FloorToInt(offsetColumns); j < cols && j < Mathf.Ceil(offsetColumns + visibleColumns + 1); ++j)
            //    {
            //        if (state == m_keyframes[i, j])
            //        {
            //            m_matrices[index] = Matrix4x4.TRS(offset + new Vector3(aspect * j / visibleColumns, -rowHeight * i, 1),
            //                Quaternion.Euler(0, 0, 45),
            //            keyframeScale);

            //            index++;
            //            if (index == k_batchSize)
            //            {
            //                index = 0;
            //                m_commandBuffer.DrawMeshInstanced(m_quad, 0, material, 0, m_matrices, k_batchSize);
            //            }
            //        }
            //    }
            //}

            for(int i = 0; i < keyframes.Count; ++i)
            {
                Keyframe keyframe = keyframes[i];
                m_matrices[index] = Matrix4x4.TRS(
                    offset + new Vector3(aspect * keyframe.Col / visibleColumns, -rowHeight * keyframe.Row, 1),
                    Quaternion.Euler(0, 0, 45),
                    keyframeScale);

                index++;
                if (index == k_batchSize)
                {
                    index = 0;
                    m_commandBuffer.DrawMeshInstanced(m_quad, 0, material, 0, m_matrices, k_batchSize);
                }
            }

            if (0 < index && index < k_batchSize)
            {
                m_commandBuffer.DrawMeshInstanced(m_quad, 0, material, 0, m_matrices, index);
            }
        }
    }
}
