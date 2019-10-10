using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public struct TimelineGridParameters
    {
        public int VerticalLinesCount;
        public int HorizontalLinesCount;
        public Color LineColor;
    }

    public class TimelineGrid : MonoBehaviour
    {
        private CommandBuffer m_commandBuffer;
        private Camera m_camera;
        private Mesh m_vGridMesh0;
        private Mesh m_hGridMesh0;
        private Mesh m_vGridMesh1;
        private Mesh m_hGridMesh1;
        private Material m_vGridMaterial0;
        private Material m_vgridMaterial1;
        private Material m_hGridMaterial0;
        private Material m_hGridMaterial1;

        private const int kS = 5;
        
        private TimelineGridParameters m_parameters;

        private void OnDestroy()
        {
            if(m_camera != null)
            {
                m_camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
            }
        }

        public void Init(Camera camera)
        {
            if(m_camera != null)
            {
                m_camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
            }

            if (m_commandBuffer == null)
            {
                m_commandBuffer = new CommandBuffer();
            }

            m_camera = camera;
            m_camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
            m_vGridMaterial0 = CreateGridMaterial();
            m_vgridMaterial1 = CreateGridMaterial();
            m_hGridMaterial0 = CreateGridMaterial();
            m_hGridMaterial1 = CreateGridMaterial();

            m_parameters.HorizontalLinesCount = 5;
            m_parameters.VerticalLinesCount = 13;
            m_parameters.LineColor = new Color(1, 1, 1, 0.1f);
            SetGridParameters(m_parameters);
        }

        public void SetGridParameters(TimelineGridParameters parameters)
        {
            m_commandBuffer.Clear();

            m_parameters = parameters;

            if(m_vGridMesh0 != null)
            {
                Destroy(m_vGridMesh0);
            }

            if(m_hGridMesh0 != null)
            {
                Destroy(m_hGridMesh0);
            }

            if(m_vGridMesh1 != null)
            {
                Destroy(m_vGridMesh0);
            }

            if(m_hGridMesh1 != null)
            {
                Destroy(m_hGridMesh0);
            }

            m_vGridMesh0 = CreateGridMesh(m_parameters.VerticalLinesCount, true);
            m_hGridMesh0 = CreateGridMesh(m_parameters.HorizontalLinesCount, false);

            m_vGridMesh1 = CreateGridMesh(m_parameters.VerticalLinesCount * kS, true);
            m_hGridMesh1 = CreateGridMesh(m_parameters.HorizontalLinesCount * kS, false);
        }

        private float Lerp(float a, float b, float t)
        {
            return a * t + b * (1 - t);
        }

        public void UpdateGraphics(Vector2 viewportSize, Vector2 contentSize, Vector2 scrollOffset, Vector2 scrollSize, float verticalScale)
        {
            m_commandBuffer.Clear();

            Vector2 contentScale = new Vector2(
                1.0f / scrollSize.x,
                1.0f / scrollSize.y);

            float vSpace = contentSize.x / (m_parameters.VerticalLinesCount * kS);
            float hSpace = contentSize.y / (m_parameters.HorizontalLinesCount * kS);

            //RectTransform rt = (RectTransform)transform;
            //Vector3 pos = rt.anchoredPosition;
            //pos.x = Mathf.Floor(pos.x / vSpace) * vSpace;
            //pos.y = Mathf.Floor(pos.y / hSpace) * hSpace;
            //Vector3 offset = new Vector3(-0.5f - pos.x / rt.rect.width, 0.5f, 1.0f);

            
            Vector3 offset = new Vector3(-0.5f, 0.5f, 1.0f);
            offset.x -= (1 - scrollSize.x) * scrollOffset.x / scrollSize.x;
            offset.y += (1 - scrollSize.y) * (1 - scrollOffset.y) / scrollSize.y;

            Vector3 scale = Vector3.one;

            float aspect = viewportSize.x / viewportSize.y; 
            offset.x *= aspect;
            scale.x = aspect;

            scale.x *= contentScale.x;
            scale.y *= contentScale.y;

            const float fadeBeginPixels = 10;
            const float fadeOutPixels = 2;

            float vFade = Mathf.Clamp01((vSpace - fadeOutPixels) / fadeBeginPixels);
            float hFade = Mathf.Clamp01((hSpace - fadeOutPixels) / fadeBeginPixels);

            Color vLineColor = m_parameters.LineColor;
            Color hLineColor = m_parameters.LineColor;
            vLineColor.a = Lerp(vLineColor.a, vLineColor.a * 0.5f, vFade);
            hLineColor.a = Lerp(hLineColor.a, hLineColor.a * 0.5f, hFade);

            m_vGridMaterial0.color = vLineColor;
            m_hGridMaterial0.color = hLineColor;

            vLineColor = m_parameters.LineColor;
            hLineColor = m_parameters.LineColor;
            vLineColor.a = Lerp(vLineColor.a * 0.5f, 0, vFade);
            hLineColor.a = Lerp(hLineColor.a * 0.5f, 0, hFade);

            m_vgridMaterial1.color = vLineColor;
            m_hGridMaterial1.color = hLineColor;

            Matrix4x4 vMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            m_commandBuffer.DrawMesh(m_vGridMesh0, vMatrix, m_vGridMaterial0);
            m_commandBuffer.DrawMesh(m_vGridMesh1, vMatrix, m_vgridMaterial1);

           // scale.y = verticalScale;
            Matrix4x4 hMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            m_commandBuffer.DrawMesh(m_hGridMesh0, hMatrix, m_hGridMaterial0);
            m_commandBuffer.DrawMesh(m_hGridMesh1, hMatrix, m_hGridMaterial1);
        }

        private Material CreateGridMaterial()
        {
            Shader shader = Shader.Find("Battlehub/RTEditor/TimelineGrid");
            Material material = new Material(shader);
            return material;
        }

        private Mesh CreateGridMesh(int count, bool isVertical)
        {
            Mesh mesh = new Mesh();
            mesh.name = "TimelineGrid";

            float space = 1.0f / count;
            
            int index = 0;
            int[] indices = new int[count * 2];
            Vector3[] vertices = new Vector3[indices.Length];
            Color[] colors = new Color[indices.Length];

            if(isVertical)
            {
                for (int i = 0; i < count; ++i)
                {
                    vertices[index] = new Vector3(i * space, 0, 0);
                    vertices[index + 1] = new Vector3(i * space, -1, 0);

                    indices[index] = index;
                    indices[index + 1] = index + 1;

                    colors[index] = colors[index + 1] = Color.white;

                    index += 2;
                }
            }
            else
            {
                for (int i = 0; i < count; ++i)
                {
                    vertices[index] = new Vector3(0, -i * space, 0);
                    vertices[index + 1] = new Vector3(1, -i * space, 0);

                    indices[index] = index;
                    indices[index + 1] = index + 1;

                    colors[index] = colors[index + 1] = Color.white;

                    index += 2;
                }
            }


            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.colors = colors;

            return mesh;
        }

    }
}
