using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class TimelineGridParameters
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
        private Mesh m_vGridMesh1;
        private Mesh m_vGridMesh2;

        private Mesh m_hGridMesh0;
        private Mesh m_hGridMesh1;
        private Mesh m_hGridMesh2;

        private Material m_vGridMaterial0;
        private Material m_vGridMaterial1;
        private Material m_vGridMaterial2;
        
        private Material m_hGridMaterial0;
        private Material m_hGridMaterial1;
        private Material m_hGridMaterial2;
        
        public const int k_Lines = 5;
        public const int k_LinesSq = k_Lines * k_Lines;

        private const float k_FadeBeginPixels = 10;
        private const float k_FadeOutPixels = 2;

        private TimelineGridParameters m_parameters = null;

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
            m_vGridMaterial1 = CreateGridMaterial();
            m_vGridMaterial2 = CreateGridMaterial();
            m_vGridMaterial0 = CreateGridMaterial();
            m_hGridMaterial1 = CreateGridMaterial();
            m_hGridMaterial2 = CreateGridMaterial();
            m_hGridMaterial0 = CreateGridMaterial();
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

            if (m_vGridMesh0 != null)
            {
                Destroy(m_vGridMesh0);
            }

            if (m_vGridMesh1 != null)
            {
                Destroy(m_vGridMesh1);
            }

            if (m_vGridMesh2 != null)
            {
                Destroy(m_vGridMesh2);
            }

            if (m_hGridMesh0 != null)
            {
                Destroy(m_hGridMesh0);
            }

            if (m_hGridMesh1 != null)
            {
                Destroy(m_hGridMesh1);
            }

            if(m_hGridMesh2 != null)
            {
                Destroy(m_hGridMesh2);
            }

            int vLinesCount = m_parameters.VerticalLinesCount * k_Lines;
            int hLinesCount = m_parameters.HorizontalLinesCount * k_Lines;

            int repeatX = Mathf.Max(1, Mathf.CeilToInt((viewportSize.x / (vLinesCount * k_Lines)) / k_FadeOutPixels));
            int repeatY = Mathf.Max(1, Mathf.CeilToInt((viewportSize.y / (hLinesCount * k_Lines)) / k_FadeOutPixels));

            m_vGridMesh0 = CreateGridMesh(vLinesCount, true, repeatX, repeatY);
            m_hGridMesh0 = CreateGridMesh(hLinesCount, false, repeatY, repeatX);

            m_vGridMesh1 = CreateGridMesh(vLinesCount, true, repeatX, repeatY);
            m_hGridMesh1 = CreateGridMesh(hLinesCount, false, repeatY, repeatX);

            m_vGridMesh2 = CreateGridMesh(vLinesCount, true, repeatX, repeatY);
            m_hGridMesh2 = CreateGridMesh(hLinesCount, false, repeatY, repeatX);
        }

        public static float EaseOutQuad(float start, float end, float value)
        {
            end -= start;
            return -end * value * (value - 2) + start;
        }


        private void SetAlpha(Material material, float fadeOutOffset, float p)
        {
            Color color = m_parameters.LineColor;

            p = p % 3.0f / 3.0f;
            
            p = (p > 1 - fadeOutOffset) ? 1 : p / (1 - fadeOutOffset);
            p = (p <= 0.5) ? 0 : (p - 0.5f) * 2.0f;

            color.a *= EaseOutQuad(1, 0, p);
            material.color = color;
        }

        public void UpdateGraphics(Vector2 viewportSize, Vector2 contentSize, Vector2 normalizedOffset, Vector2 normalizedSize, Vector2 interval, float verticalScale)
        {
            if (m_parameters == null)
            {
                throw new System.InvalidOperationException("Call SetGridParameters method first");
            }

            m_commandBuffer.Clear();

            int vLinesCount = m_parameters.VerticalLinesCount;
            int hLinesCount = m_parameters.HorizontalLinesCount;
            Color lineColor = m_parameters.LineColor;

            //Vector2 scrollSize = new Vector2(Mathf.Pow(k_Lines, normalizedSize.x - 1), Mathf.Pow(k_Lines, normalizedSize.y - 1));
            //Vector2 scrollOffset = new Vector2(Mathf.Pow(k_Lines, normalizedOffset.x - 1), Mathf.Pow(k_Lines, normalizedOffset.y - 1));

            Vector2 contentScale = new Vector2(
                1.0f / normalizedSize.x,
                1.0f / normalizedSize.y);

            Vector3 offset = new Vector3(-0.5f, 0.5f, 1.0f);
            offset.x -= ((1 - normalizedSize.x) * normalizedOffset.x / normalizedSize.x) % (contentScale.x * k_LinesSq / Mathf.Max(1, vLinesCount));
            offset.y += ((1 - normalizedSize.y) * (1 - normalizedOffset.y) / normalizedSize.y) % (contentScale.y * k_LinesSq / Mathf.Max(1, hLinesCount));

            Vector3 scale = Vector3.one;

            float aspect = viewportSize.x / viewportSize.y;
            offset.x *= aspect;
            scale.x = aspect;

            float px = interval.x * normalizedSize.x;
            float py = interval.y * normalizedSize.y;

            float fadeOutOffset = Mathf.Min(0.4f, 1 - Mathf.Clamp01(viewportSize.x / 600.0f));

            SetAlpha(m_vGridMaterial0, fadeOutOffset, px - 1);
            SetAlpha(m_vGridMaterial1, fadeOutOffset, px);
            SetAlpha(m_vGridMaterial2, fadeOutOffset, px + 1);

            SetAlpha(m_hGridMaterial0, fadeOutOffset, py - 1);
            SetAlpha(m_hGridMaterial1, fadeOutOffset, py);
            SetAlpha(m_hGridMaterial2, fadeOutOffset, py + 1);

            
            scale.x = aspect * k_LinesSq / Mathf.Pow(k_Lines, (px - 1) % 3.0f);
            scale.y = aspect * k_LinesSq / Mathf.Pow(k_Lines, (py - 1) % 3.0f);

            Matrix4x4 vMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            if (m_vGridMesh0 != null)
            {
                m_commandBuffer.DrawMesh(m_vGridMesh0, vMatrix, m_vGridMaterial0);
            }

            Matrix4x4 hMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            if (m_hGridMesh0 != null)
            {
                m_commandBuffer.DrawMesh(m_hGridMesh0, hMatrix, m_hGridMaterial0);
            }

            scale.x = aspect * k_LinesSq / Mathf.Pow(k_Lines, px % 3.0f);
            scale.y = aspect * k_LinesSq / Mathf.Pow(k_Lines, py % 3.0f);
            vMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            if (m_vGridMesh1 != null)
            {
                m_commandBuffer.DrawMesh(m_vGridMesh1, vMatrix, m_vGridMaterial1);
            }

            hMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            if (m_hGridMesh1 != null)
            {
                m_commandBuffer.DrawMesh(m_hGridMesh1, hMatrix, m_hGridMaterial1);
            }

            scale.x = aspect * k_LinesSq / Mathf.Pow(k_Lines, (px + 1) % 3.0f);
            scale.y = aspect * k_LinesSq / Mathf.Pow(k_Lines, (py + 1) % 3.0f);
            vMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            if (m_vGridMesh2 != null)
            {
                m_commandBuffer.DrawMesh(m_vGridMesh2, vMatrix, m_vGridMaterial2);
            }

            hMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            if (m_hGridMesh2 != null)
            {
                m_commandBuffer.DrawMesh(m_hGridMesh2, hMatrix, m_hGridMaterial2);
            }
        }

        private Material CreateGridMaterial()
        {
            Shader shader = Shader.Find("Battlehub/RTEditor/TimelineGrid");
            Material material = new Material(shader);
            return material;
        }

        private Mesh CreateGridMesh(float count, bool isVertical, int repeat = 2, int lineLength = 2, int skipLine = int.MaxValue)
        {
            Mesh mesh = new Mesh();
            mesh.name = "TimelineGrid";

            float space = 1.0f / count;
            int totalCount = Mathf.CeilToInt(count * repeat);
            int index = 0;
            int[] indices = new int[(totalCount - totalCount / skipLine) * 2];
            Vector3[] vertices = new Vector3[indices.Length];
            Color[] colors = new Color[indices.Length];

            if(isVertical)
            {
                for (int i = 0; i < totalCount; ++i)
                {
                    if (i % skipLine == 0)
                    {
                        continue;
                    }

                    vertices[index] = new Vector3(i * space, 0, 0);
                    vertices[index + 1] = new Vector3(i * space, -lineLength, 0);

                    indices[index] = index;
                    indices[index + 1] = index + 1;

                    Color color = Color.white;
                    colors[index] = colors[index + 1] = color;

                    index += 2;
                }
            }
            else
            {
                for (int i = 0; i < totalCount; ++i)
                {
                    if (i % skipLine == 0)
                    {
                        continue;
                    }

                    vertices[index] = new Vector3(0, -i * space, 0);
                    vertices[index + 1] = new Vector3(lineLength, -i * space, 0);

                    indices[index] = index;
                    indices[index + 1] = index + 1;

                    Color color = Color.white;
                    colors[index] = colors[index + 1] = color;

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
