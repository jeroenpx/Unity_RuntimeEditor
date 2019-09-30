using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class SceneGrid : RTEComponent
    {
        public RuntimeHandlesComponent Appearance;

        private GameObject m_grid0;
        private GameObject m_grid1;
        private Mesh m_grid0Mesh;
        private Mesh m_grid1Mesh;
        private Material m_grid0Material;
        private Material m_grid1Material;

        [SerializeField]
        private Vector3 m_gridOffset = new Vector3(0f, 0.01f, 0f);

        private float m_gridSize = 0.5f;
        public float SizeOfGrid
        {
            get { return m_gridSize; }
            set
            {
                if(m_gridSize != value)
                {
                    m_gridSize = value;
                    Rebuild();
                }
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            RuntimeHandlesComponent.InitializeIfRequired(ref Appearance);
            Rebuild();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            Cleanup();
        }

        private void Cleanup()
        {
            if (m_grid0Material != null)
            {
                Destroy(m_grid0Material);
            }
            if (m_grid1Material != null)
            {
                Destroy(m_grid1Material);
            }
            if (m_grid0Mesh != null)
            {
                Destroy(m_grid0Mesh);
            }
            if (m_grid1Mesh != null)
            {
                Destroy(m_grid1Mesh);
            }
            if (m_grid0 != null)
            {
                Destroy(m_grid0);
            }
            if (m_grid1 != null)
            {
                Destroy(m_grid1);
            }
        }

        private void Rebuild()
        {
            Cleanup();

            m_grid0Material = CreateGridMaterial(0.5f);
            m_grid1Material = CreateGridMaterial(0.5f);

            m_grid0Mesh = Appearance.CreateGridMesh(Appearance.Colors.GridColor, m_gridSize);
            m_grid1Mesh = Appearance.CreateGridMesh(Appearance.Colors.GridColor, m_gridSize);

            m_grid0 = CreateGridGameObject("Grid0", m_grid0Mesh, m_grid0Material);
            m_grid1 = CreateGridGameObject("Grid1", m_grid1Mesh, m_grid1Material);
        }


        protected virtual void Update()
        {
            float h = GetCameraOffset();
            h = Mathf.Abs(h);
            h = Mathf.Max(1, h);
            float scale = RuntimeHandlesComponent.CountOfDigits(h);
            float fadeDistance = h * 10;

            float alpha0 = GetAlpha(0, h, scale);
            float alpha1 = GetAlpha(1, h, scale);

            m_grid0.transform.localScale = Vector3.one * Mathf.Pow(10, scale - 1);
            m_grid1.transform.localScale = Vector3.one * Mathf.Pow(10, scale);

            SetGridPostion(m_grid0, Mathf.Pow(10, scale - 1));
            SetGridPostion(m_grid1, Mathf.Pow(10, scale));

            SetGridAlpha(m_grid0Material, alpha0, fadeDistance);
            SetGridAlpha(m_grid1Material, alpha1, fadeDistance);
        }

        private void SetGridPostion(GameObject grid, float spacing)
        {
            Vector3 position = Window.Camera.transform.position;
            position = transform.InverseTransformPoint(position);

            spacing *= m_gridSize;

            position.x = Mathf.Floor(position.x / spacing) * spacing;
            position.z = Mathf.Floor(position.z / spacing) * spacing;
            position.y = 0;

            position += m_gridOffset;

            grid.transform.localPosition = position;
        }

        private void SetGridAlpha(Material gridMaterial, float alpha, float fadeDistance)
        {
            Color color = gridMaterial.GetColor("_GridColor");
            color.a = alpha;
            gridMaterial.SetColor("_GridColor", color);
            gridMaterial.SetFloat("_FadeDistance", fadeDistance);
        }

        private Material CreateGridMaterial(float scale)
        {
            Shader shader = Shader.Find("Battlehub/RTHandles/Grid");
            Material material = new Material(shader);
            material.SetColor("_GridColor", Appearance.Colors.GridColor);
            return material;
        }

        private GameObject CreateGridGameObject(string name, Mesh mesh, Material material)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return go;
        }

        private float GetCameraOffset()
        {
            if (Window.Camera.orthographic)
            {
                return Window.Camera.orthographicSize;
            }

            Vector3 position = Window.Camera.transform.position;
            position = transform.InverseTransformPoint(position);
            return position.y;
        }

        private float GetAlpha(int grid, float h, float scale)
        {
            float nextSpacing = Mathf.Pow(10, scale);
            if (grid == 0)
            {
                float spacing = Mathf.Pow(10, scale - 1);
                return 1.0f - (h - spacing) / (nextSpacing - spacing);
            }

            float nextNextSpacing = Mathf.Pow(10, scale + 1);
            return (h * 10 - nextSpacing) / (nextNextSpacing - nextSpacing);            
        }
    }
}

