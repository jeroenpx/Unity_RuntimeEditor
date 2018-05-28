using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class ScaleHandleModel : BaseHandleModel
    {
        [SerializeField]
        private int m_xMatIndex = 0;
        [SerializeField]
        private int m_yMatIndex = 1;
        [SerializeField]
        private int m_zMatIndex = 2;
        [SerializeField]
        private int m_xArrowMatIndex = 3;
        [SerializeField]
        private int m_yArrowMatIndex = 4;
        [SerializeField]
        private int m_zArrowMatIndex = 5;
        [SerializeField]
        private int m_xyzMatIndex = 6;
        [SerializeField]
        private Transform m_armature;
        [SerializeField]
        private Transform m_model;

        private Transform m_b1x;
        private Transform m_b2x;
        private Transform m_b3x;
        private Transform m_b1y;
        private Transform m_b2y;
        private Transform m_b3y;
        private Transform m_b1z;
        private Transform m_b2z;
        private Transform m_b3z;
        private Transform m_b0;

        [SerializeField]
        private float m_radius = DefaultRadius;
        [SerializeField]
        private float m_length = DefaultLength;
        [SerializeField]
        private float m_arrowRadius = DefaultArrowRadius;

        private const float DefaultRadius = 0.05f;
        private const float DefaultLength = 1.0f;
        private const float DefaultArrowRadius = 0.1f;

        private Material[] m_materials;

        private Vector3 m_scale = Vector3.one;

        protected override void Awake()
        {
            base.Awake();
            m_b1x = m_armature.GetChild(0);
            m_b1y = m_armature.GetChild(1);
            m_b1z = m_armature.GetChild(2);
            m_b2x = m_armature.GetChild(3);
            m_b2y = m_armature.GetChild(4);
            m_b2z = m_armature.GetChild(5);
            m_b3x = m_armature.GetChild(6);
            m_b3y = m_armature.GetChild(7);
            m_b3z = m_armature.GetChild(8);
            m_b0 = m_armature.GetChild(9);

            Renderer renderer = m_model.GetComponent<Renderer>();
            m_materials = renderer.materials;
            renderer.sharedMaterials = m_materials;
        }

        protected override void Start()
        {
            base.Start();

            UpdateTransforms();
            SetColors();
        }

        public override void SetLock(LockObject lockObj)
        {
            base.SetLock(m_lockObj);
            SetColors();
        }

        public override void Select(RuntimeHandleAxis axis)
        {
            base.Select(axis);
            SetColors();
        }

        private void SetDefaultColors()
        {
            if (m_lockObj.ScaleX)
            {
                m_materials[m_xMatIndex].color = m_disabledColor;
                m_materials[m_xArrowMatIndex].color = m_disabledColor;
            }
            else
            {
                m_materials[m_xMatIndex].color = m_xColor;
                m_materials[m_xArrowMatIndex].color = m_xColor;
            }

            if (m_lockObj.ScaleY)
            {
                m_materials[m_yMatIndex].color = m_disabledColor;
                m_materials[m_yArrowMatIndex].color = m_disabledColor;
            }
            else
            {
                m_materials[m_yMatIndex].color = m_yColor;
                m_materials[m_yArrowMatIndex].color = m_yColor;
            }

            if (m_lockObj.ScaleZ)
            {
                m_materials[m_zMatIndex].color = m_disabledColor;
            }
            else
            {
                m_materials[m_zMatIndex].color = m_zColor;
                m_materials[m_zArrowMatIndex].color = m_zColor;
            }

            if(m_lockObj.IsPositionLocked)
            {
                m_materials[m_xyzMatIndex].color = m_disabledColor;
            }
            else
            {
                m_materials[m_xyzMatIndex].color = m_altColor;
            }
        }

        private void SetColors()
        {
            SetDefaultColors();
            switch (m_selectedAxis)
            {
                case RuntimeHandleAxis.X:
                    if (!m_lockObj.ScaleX)
                    {
                        m_materials[m_xArrowMatIndex].color = m_selectionColor;
                        m_materials[m_xMatIndex].color = m_selectionColor;
                    }
                    break;
                case RuntimeHandleAxis.Y:
                    if (!m_lockObj.ScaleY)
                    {
                        m_materials[m_yArrowMatIndex].color = m_selectionColor;
                        m_materials[m_yMatIndex].color = m_selectionColor;
                    }
                    break;
                case RuntimeHandleAxis.Z:
                    if (!m_lockObj.ScaleZ)
                    {
                        m_materials[m_zArrowMatIndex].color = m_selectionColor;
                        m_materials[m_zMatIndex].color = m_selectionColor;
                    }
                    break;
                case RuntimeHandleAxis.Free:
                    m_materials[m_xyzMatIndex].color = m_selectionColor;
                    break;
            }
        }

        public override void SetScale(Vector3 scale)
        {
            base.SetScale(scale);
            m_scale = scale;
            UpdateTransforms();
        }

        private void UpdateTransforms()
        {
            m_radius = Mathf.Max(0.01f, m_radius);

            Vector3 right = transform.rotation * Vector3.right * transform.localScale.x;
            Vector3 up = transform.rotation * Vector3.up * transform.localScale.y;
            Vector3 forward = transform.rotation * Vector3.forward * transform.localScale.z;
            Vector3 p = transform.position;
            float scale = m_radius / DefaultRadius;
            float arrowScale = m_arrowRadius / DefaultArrowRadius;


            m_b0.localScale = Vector3.one * arrowScale * 2;
            m_b3z.localScale = m_b3y.localScale = m_b3x.localScale = Vector3.one * arrowScale;

            m_b1x.position = p + right * m_arrowRadius;
            m_b1y.position = p + up * m_arrowRadius;
            m_b1z.position = p + forward * m_arrowRadius;

            m_b2x.position = p + right * (m_length * m_scale.x - m_arrowRadius); 
            m_b2y.position = p + up * (m_length * m_scale.y - m_arrowRadius);
            m_b2z.position = p + forward * (m_length * m_scale.z - m_arrowRadius);

            m_b2x.localScale = m_b1x.localScale = new Vector3(1, scale, scale);
            m_b2y.localScale = m_b1y.localScale = new Vector3(scale, scale, 1);
            m_b2z.localScale = m_b1z.localScale = new Vector3(scale, 1, scale);

            m_b3x.position = p + right * m_length * m_scale.x;
            m_b3y.position = p + up * m_length * m_scale.y;
            m_b3z.position = p + forward * m_length * m_scale.z;
        }

        private float m_prevRadius;
        private float m_prevLength;
        private float m_prevArrowRadius;
        protected override void Update()
        {
            base.Update();
            if (m_prevRadius != m_radius || m_prevLength != m_length || m_prevArrowRadius != m_arrowRadius)
            {
                m_prevRadius = m_radius;
                m_prevLength = m_length;
                m_prevArrowRadius = m_arrowRadius;
                UpdateTransforms();
            }

        }
    }
}
