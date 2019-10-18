using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTEditor
{
    public class Dopesheet : MonoBehaviour
    {
        [SerializeField]
        private Mesh m_quad = null;

        [SerializeField]
        private Material m_material = null;

        private CommandBuffer m_commandBuffer;
        private Camera m_camera;

        private TimelineGridParameters m_parameters = null;

        private const int k_batchSize = 512;

        private Matrix4x4[] m_matrices = new Matrix4x4[k_batchSize];

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
            int cols = vLinesCount * m_parameters.VertLinesSecondary;
            int rows = hLinesCount - 1;

            float rowHeight = m_parameters.FixedHeight / viewportSize.y;
            float aspect = viewportSize.x / viewportSize.y;

            int vLinesSq = m_parameters.VertLinesSecondary * m_parameters.VertLinesSecondary;
            int hLinesSq = m_parameters.HorLinesSecondary * m_parameters.HorLinesSecondary;

            Vector2 contentScale = new Vector2(
                1.0f / normalizedSize.x,
                1.0f / normalizedSize.y);

            Vector3 offset = new Vector3(-0.5f, 0.5f - rowHeight * 0.5f, 1.0f);
            offset.x -= ((1 - normalizedSize.x) * normalizedOffset.x / normalizedSize.x) % (contentScale.x * vLinesSq / Mathf.Max(1, vLinesCount));
            offset.x *= aspect;
            offset.y += ((1 - normalizedSize.y) * (1 - normalizedOffset.y) / normalizedSize.y);

            Vector3 keyframeScale = Vector3.one * rowHeight * 0.5f;

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    m_matrices[index] = Matrix4x4.TRS(
                        offset + new Vector3(aspect * j / cols, -rowHeight * i , 1),
                        Quaternion.Euler(0, 0, 45),
                        keyframeScale);

                    index++;
                    if (index == k_batchSize)
                    {
                        index = 0;
                        m_commandBuffer.DrawMeshInstanced(m_quad, 0, m_material, 0, m_matrices, k_batchSize);
                    }
                }
            }

            if (0 < index && index < k_batchSize)
            {
                m_commandBuffer.DrawMeshInstanced(m_quad, 0, m_material, 0, m_matrices, index);
            }
        }
    }
}
