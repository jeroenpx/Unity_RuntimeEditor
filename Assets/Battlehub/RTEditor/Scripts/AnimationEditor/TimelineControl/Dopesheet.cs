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

            //if(Input.GetKey(KeyCode.T))
            {
                m_commandBuffer.Clear();

                //int m_rows = 10;
                //int m_cols = 10;

                //int m_batchSize = 512;
                //Matrix4x4[] m_matrices = new Matrix4x4[m_batchSize];

                //int index = 0;

                //for (int i = 0; i < m_rows; ++i)
                //{
                //    for (int j = 0; j < m_cols; ++j)
                //    {
                //        m_matrices[index] = Matrix4x4.TRS(
                //            new Vector3(i / 10.0f, j / 10.0f, 0),
                //            Quaternion.Euler(0, 0, 45),
                //            Vector3.one * 0.05f);

                //        index++;
                //        if (index == m_batchSize)
                //        {
                //            index = 0;
                //            m_commandBuffer.DrawMeshInstanced(m_quad, 0, m_material, 0, m_matrices, m_batchSize);
                //        }
                //    }
                //}

                //if (0 < index && index < m_batchSize)
                //{
                //    m_commandBuffer.DrawMeshInstanced(m_quad, 0, m_material, 0, m_matrices, index);
                //}
            }
        }

    }
}
