using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTEditor
{
  

    public class Dopesheet : MonoBehaviour
    {
        public enum Keyframe
        {
            Empty,
            Normal,
            Selected
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

#warning replace array with more appropriate structure
        private Keyframe[,] m_keyframes = new Keyframe[0, 0];
        public Keyframe[,] Keyframes
        {
            get { return m_keyframes; }
            set { m_keyframes = value; }
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
            int cols = m_keyframes.GetLength(1); //vLinesCount * m_parameters.VertLinesSecondary;
            int rows = m_keyframes.GetLength(0);  //hLinesCount - 1;

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

            UpdateKeyframes(Keyframe.Normal, m_material, index, cols, rows, rowHeight, aspect, offset, offsetColumns, visibleColumns, keyframeScale);

            UpdateKeyframes(Keyframe.Selected, m_selectionMaterial, index, cols, rows, rowHeight, aspect, offset, offsetColumns, visibleColumns, keyframeScale);

        }

        private void UpdateKeyframes(Keyframe state, Material material, int index, int cols, int rows, float rowHeight, float aspect, Vector3 offset, float offsetColumns, float visibleColumns, Vector3 keyframeScale)
        {
            for (int i = 0; i < rows; ++i)
            {
                for (int j = Mathf.FloorToInt(offsetColumns); j < cols && j < Mathf.Ceil(offsetColumns + visibleColumns + 1); ++j)
                {
                    if (state == m_keyframes[i, j])
                    {
                        m_matrices[index] = Matrix4x4.TRS(offset + new Vector3(aspect * j / visibleColumns, -rowHeight * i, 1),
                            Quaternion.Euler(0, 0, 45),
                        keyframeScale);

                        index++;
                        if (index == k_batchSize)
                        {
                            index = 0;
                            m_commandBuffer.DrawMeshInstanced(m_quad, 0, material, 0, m_matrices, k_batchSize);
                        }
                    }
                }
            }

            if (0 < index && index < k_batchSize)
            {
                m_commandBuffer.DrawMeshInstanced(m_quad, 0, material, 0, m_matrices, index);
            }
        }
    }
}
