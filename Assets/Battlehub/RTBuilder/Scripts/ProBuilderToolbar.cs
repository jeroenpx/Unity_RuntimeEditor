using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTBuilder
{
    public class ProBuilderToolbar : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_objectToggle = null;

        [SerializeField]
        private Toggle m_vetexToggle = null;

        [SerializeField]
        private Toggle m_edgeToggle = null;

        [SerializeField]
        private Toggle m_faceToggle = null;

        private IProBuilderTool m_tool;

        protected virtual void Awake()
        {
            if(m_objectToggle != null)
            {
                m_objectToggle.onValueChanged.AddListener(OnObject);
            }
            if(m_vetexToggle != null)
            {
                m_vetexToggle.onValueChanged.AddListener(OnVertex);
            }
            if(m_edgeToggle != null)
            {
                m_edgeToggle.onValueChanged.AddListener(OnEdge);
            }
            if(m_faceToggle != null)
            {
                m_faceToggle.onValueChanged.AddListener(OnFace);
            }
        }

        protected virtual void Start()
        {
            m_tool = IOC.Resolve<IProBuilderTool>();
            m_tool.ModeChanged += OnModeChanged;
        }

        protected virtual void OnDestroy()
        {
            if(m_tool != null)
            {
                m_tool.ModeChanged -= OnModeChanged;
            }

            if (m_objectToggle != null)
            {
                m_objectToggle.onValueChanged.RemoveListener(OnObject);
            }
            if (m_vetexToggle != null)
            {
                m_vetexToggle.onValueChanged.RemoveListener(OnVertex);
            }
            if (m_edgeToggle != null)
            {
                m_edgeToggle.onValueChanged.RemoveListener(OnEdge);
            }
            if (m_faceToggle != null)
            {
                m_faceToggle.onValueChanged.RemoveListener(OnFace);
            }
        }

        private void OnObject(bool value)
        {
            if(value)
            {
                m_tool.Mode = ProBuilderToolMode.Object;
            }
        }

        private void OnVertex(bool value)
        {
            if(value)
            {
                m_tool.Mode = ProBuilderToolMode.Vertex;
            }
        }

        private void OnEdge(bool value)
        {
            if(value)
            {
                m_tool.Mode = ProBuilderToolMode.Edge;
            }
        }

        private void OnFace(bool value)
        {
            if(value)
            {
                m_tool.Mode = ProBuilderToolMode.Face;
            }
        }

        private void OnModeChanged(ProBuilderToolMode oldMode)
        {
            switch (m_tool.Mode)
            {
                case ProBuilderToolMode.Object:
                    m_objectToggle.isOn = true;
                    break;
                case ProBuilderToolMode.Vertex:
                    m_vetexToggle.isOn = true;
                    break;
                case ProBuilderToolMode.Edge:
                    m_edgeToggle.isOn = true;
                    break;
                case ProBuilderToolMode.Face:
                    m_faceToggle.isOn = true;
                    break;
            }
        }
    }

}

